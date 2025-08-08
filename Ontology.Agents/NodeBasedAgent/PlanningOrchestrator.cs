using Buffaly.SemanticDB.Data;
using Buffaly.SemanticDB;
using System.Text;
using BasicUtilities;
using ProtoScript.Interpretter.RuntimeInfo;
using ProtoScript;
using ProtoScript.Parsers;
using Buffaly.AIProviderAPI;

namespace Ontology.Agents.NodeBasedAgent
{
	public class PlanningOrchestrator
	{
		public string Trace = string.Empty;
		public int Iteration = 1;
		private int m_iSuccessiveFailures = 0;
		public void SetupSemanticProgram(SessionObject session, string strProgram)
		{
			FragmentsRow rowPlanningAgentControl = Fragments.GetFragmentByFragmentKey("Semantic Program Execution");
			FragmentsRow rowProgram = Fragments.GetFragmentByFragmentKey(strProgram);

			//>build the prompt
			StringBuilder sbPrompt = new StringBuilder();

			sbPrompt.AppendLine(rowPlanningAgentControl.Fragment);
			sbPrompt.AppendLine(rowProgram.Fragment);

			sbPrompt.AppendLine("<SystemInstructions>");
			sbPrompt.AppendLine(rowPlanningAgentControl.Fragment);
			sbPrompt.AppendLine("</SystemInstructions>");
			sbPrompt.AppendLine("<SemanticProgram>");
			sbPrompt.AppendLine(rowProgram.Fragment);
			sbPrompt.AppendLine("</SemanticProgram>");


			session.AddSystemMessage(sbPrompt.ToString());

			Trace += sbPrompt.ToString();
		}

		public void AddInitialDirective(SessionObject session, string strDirective)
		{
			string strInitialDirective = "<Directive>" + strDirective + "</Directive>";
			Trace += strInitialDirective;

		
			session.AddUserMessage(strInitialDirective);
		}


		public async Task<JsonObject> SingleAgentLoop(SessionObject session)
		{
			JsonObject jsonResult = new JsonObject();

			try
			{
				string strResult = await new OpenAICompletions()
				{
					LogRequest = true,
					CustomTimeout = TimeSpan.FromSeconds(180),
					UseJsonFormat = true
				}.CompleteWithHistoryAsync(session.Messages, OpenAIFeature.Feature.LargeModel);

				if (strResult.StartsWith("```json"))
					strResult = StringUtil.LeftOfLast(StringUtil.RightOfFirst(strResult, "```json"), "```");

				jsonResult = new JsonObject(strResult);

				string strJson = JsonUtil.ToFriendlyJSON(jsonResult).ToString();
				session.AddAgentMessage(strJson);

				if (jsonResult.GetStringOrNull("ExecutionStatus") != "InProgress")
				{
					return jsonResult;
				}

				JsonObject jsonResponse = new JsonObject();
				JsonArray jsonMetaActionResponses = new JsonArray();
				jsonResponse["Responses"] = jsonMetaActionResponses;
				jsonResponse["Iteration"] = Iteration;      //this is just to have a unique response
				jsonResponse["OrchestratorMessage"] = "";

				bool bSuccessfulCall = false;
				foreach (JsonValue jsonValue in jsonResult.GetJsonArrayOrDefault("Calls"))
				{
					string strCall = jsonValue.ToString();

					JsonObject jsonData = jsonResult.GetJsonObjectOrDefault("Data");

					strCall = StringUtil.ReplaceAll(strCall, "'", "\"");

					string strResponse = DispatchMetaAction(strCall, jsonData, session);

					string strFormatedResponse = $@"
** Call ** 
{strCall}

** Response **
{strResponse}";
					jsonMetaActionResponses.Add(strFormatedResponse);

					bSuccessfulCall = true;
				}

				if (!bSuccessfulCall)
				{
					jsonResponse["OrchestratorMessage"] = "No calls were made, be sure you are not stalled. Make progress on every iteration.";
				}

				session.AddUserMessage(JsonUtil.ToFriendlyJSON(jsonResponse).ToString());
				m_iSuccessiveFailures = 0;
			}

			catch (Exception err)
			{
				m_iSuccessiveFailures++;
				if (m_iSuccessiveFailures > 2)
				{
					throw err;
				}


				Trace += "\n## Error\n";
				Trace += err.Message;

				JsonObject jsonResponse = new JsonObject();
				jsonResponse["OrchestratorMessage"] = "Error: " + err.Message;
				session.AddUserMessage(JsonUtil.ToFriendlyJSON(jsonResponse).ToString());


				Logs.DebugLog.WriteEvent("Error", err.Message);

				jsonResult["ExecutionStatus"] = "Error";
			}
			finally
			{
				string strSessionFile = @$"c:\temp\{session.SessionKey}.json";
				string strSessionJSON2 = JsonUtil.ToFriendlyJSON(session.GetConversationHistoryAsJSON()).ToString();
				//>write to the session file (overwrite the file)
				FileUtil.WriteFile(strSessionFile, strSessionJSON2);
			}

			return jsonResult;
		}

		private string? DispatchMetaAction(string strCall, JsonObject jsonData, SessionObject session)
		{
			try
			{
				Expression expression = ProtoScript.Parsers.Expressions.Parse(strCall);
				MethodEvaluation methodEvaluation = expression.GetChildrenExpressions().First(x => x is MethodEvaluation) as MethodEvaluation;
				for (int i1 = 0; i1 < methodEvaluation.Parameters.Count; i1++)
				{
					Expression term = methodEvaluation.Parameters[i1];
					if (!(term.Terms[0] is Literal))
					{
						string strPath = SimpleGenerator.Generate(term);
						string[] strParts = StringUtil.Split(strPath, ".");
						JsonValue jsonValue = jsonData;
						for (int i = 0; i < strParts.Length; i++)
						{
							if (jsonValue.ToJsonObject() != null)
							{
								jsonValue = (jsonValue.ToJsonObject())[strParts[i]];
							}

							else if (jsonValue.ToJsonArray() != null)
							{
								jsonValue = (jsonValue.ToJsonArray())[int.Parse(strParts[i])];
							}
						}

						methodEvaluation.Parameters[i1] = new Literal(jsonValue.ToString());
					}
				}

				ProtoScript.Interpretter.Compiled.Expression expCompiled = session.Tagger.Compiler.Compile(expression);

				object obj = session.Tagger.Interpretter.Evaluate(expCompiled);
				Prototype protoValue = session.Tagger.Interpretter.GetAsPrototype(obj);

				if (null == protoValue)
				{
					if (obj is ValueRuntimeInfo)
					{
						ValueRuntimeInfo info = obj as ValueRuntimeInfo;
						obj = info.Value;
					}
				}

				return obj?.ToString();
			}
			catch (Exception err)
			{
				return $"Error parsing call: {err.Message}, in call: " + strCall;
			}
		}
	}

}
