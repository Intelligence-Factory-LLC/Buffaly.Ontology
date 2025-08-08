using Buffaly.SemanticDB.Data;
using Buffaly.SemanticDB;
using System.Text;
using Ontology.Simulation;
using BasicUtilities;
using ProtoScript.Parsers;
using ProtoScript;
using Buffaly.NLU;
using Ontology.Agents.NodeBasedAgent;
using Ontology.Agents.DevelopmentAgent;
using Buffaly.AIProviderAPI;
using CSharp.Extensions.CodeIndex;

namespace Ontology.Agents.Actions
{
	public class ProtoScriptActions
	{
		//>create a method to get an existing program. Take the strDirective and return a Prototype 
		//>1) Look for most similar fragments with Tag "ProtoScript Action"
		//>2) If there is a similar fragment with threshold over 0.9 then get the prototype from the fragment and return it
		public static async Task<Prototype?> GetExistingProgram(string strDirective)
		{
			List<Prototype> lstSimilarFragments = await ProtoScriptActions.GetRelevantActions(strDirective);
			foreach (Prototype proto in lstSimilarFragments)
			{
				if (proto.Value > 0.9)
				{
					return proto;
				}
			}
			return null;
		}


		static public async Task<List<Prototype>> GetRelevantActions(string strDirective, int iLimit = 10)
		{
			FragmentsDataTable dtFragments = await Fragments.GetMostSimilar2ByTagID(strDirective, "ProtoScript Action", 0.10);
			StringBuilder sbUsefulMethods = new StringBuilder();
			List<Prototype> lstResults = new List<Prototype>();

			foreach (FragmentsRow rowFragment in dtFragments)
			{
				Prototype protoMethod = rowFragment.GetPrototype();

				if (null == protoMethod)
					continue;

				if (lstResults.Count > iLimit)
					break;

				lstResults.Add(protoMethod);
			}

			return lstResults;
		}

		static public string ActionToString(Prototype prototype, ProtoScriptTagger tagger)
		{
			Prototype protoInstance = tagger.Interpretter.NewInstance(prototype);

			string strName = protoInstance.Properties.GetStringOrDefault("ProtoScriptAction.Field.Name", "");
			string strDescription = protoInstance.Properties.GetStringOrDefault("ProtoScriptAction.Field.Description", "");
			string strSignature = protoInstance.Properties.GetStringOrDefault("ProtoScriptAction.Field.Signature", "");

			return $"** Function Name **: {strName}\r\n\t- **Signature**: {strSignature}\r\n\t- **Description**: {strDescription.Trim()}\r\n";
		}


		public static async Task<JsonObject> GenerateSyntheticProgram(string strDirective,
	JsonObject jsonRequest,
	SessionObject session)
		{
			ProtoScriptTagger tagger = session.Tagger;

			string strInput = strDirective; // continuations use just the new value

			if (!session.HasSystemMessage())
			{
				FragmentsRow rowProgramGenerator = Fragments.GetFragmentByFragmentKey("Synthetic Semantic Program Generator");
				StringBuilder sbPrompt = new StringBuilder();
				sbPrompt.AppendLine(rowProgramGenerator.Fragment);

				//>+ get the ProtoScript Basics fragment and append it to the prompt
				FragmentsRow rowProtoScriptBasics = Fragments.GetFragmentByFragmentKey("ProtoScript Basics");
				sbPrompt.AppendLine(rowProtoScriptBasics.Fragment);

				string strPrompt = LLMs.AddSectionToPrompt(sbPrompt.ToString(), "Output Format",
						"Respond in JSON with a format of {Program: 'generated code here', Call : 'the call to the program with parameters'}");

				session.AddSystemMessage(strPrompt);
				Logs.DebugLog.WriteEvent("GenerateSyntheticProgram.Prompt", strPrompt);

				List<Prototype> lstProtoScriptActions = await ProtoScriptActions.GetRelevantActions(strDirective, 15);
				List<Prototype> lstCSharpMethods = await CSharpMethods.GetRelevantMethods(strDirective);

				StringBuilder strProtoScriptDescriptions = ActionsToString(lstProtoScriptActions, tagger);
				string strCSharpDescriptions = CSharpMethods.MethodsToString(lstCSharpMethods);

				List<Prototype> lstRelatedEntities = Entities.RecognizeEntitiesViaActivationSpreading(strDirective, tagger);
				StringBuilder sbEntities = new StringBuilder(Entities.EntitiesToString(lstRelatedEntities));

				strInput = @"
# Input Phrase

" + strDirective + @"

# ProtoScript Actions

The following are the ProtoScript actions most relevant to the directive. They represent
valid examples of ProtoScript Programs and can be used as an example, used directly,
or generalized.

" + strProtoScriptDescriptions + @"

# CSharp Methods

These methods are available to you to use in your synthetic program and are in CSharp:

"  + strCSharpDescriptions +  @"

# Related Entities

These entities are related to the directive and may be useful in your synthetic program:

" + sbEntities.ToString();
				foreach (var key in jsonRequest?.Keys ?? Enumerable.Empty<string>())
				{
					string strKey = key;
					string strValue = jsonRequest.GetStringOrNull(key);
					if (strKey != null)
					{
						strInput += string.Format("\n# {0}\n{1}", strKey, strValue);
					}
				}
			}

			session.AddUserMessage(strInput);

			Logs.DebugLog.WriteEvent("GenerateSyntheticProgram.Input", strInput);

			JsonObject jsonResult = await LLMs.ExecuteLLMPromptAndInputWithHistory(session.Messages, ModelSize.LargeModel);

			session.AddAgentMessage(jsonResult.ToString());

			return jsonResult;
		}

		//>+ merge the above two methods into a new method GenerateProgramAndVerify that 
		//>1) Gets the program 
		//>2) Calls both Parse and InterpretCode 
		//>3) Attempts to fix the program if either fails
		public static async Task<JsonObject> GenerateProgramAndVerify(SessionObject session, string strInfinitivePhrase, JsonObject requestValues)
		{
			JsonObject jsonProgram = new JsonObject();
			try
			{
				// Step 1: Generate the program
				jsonProgram = await ProtoScriptActions.GenerateSyntheticProgram(strInfinitivePhrase, requestValues, session);
				string strProgram = jsonProgram.GetStringOrNull("Program");

				strProgram = await VerifyAndFixProtoScript(strInfinitivePhrase, strProgram, session.Tagger);
				jsonProgram["Program"] = strProgram;
			}
			catch (Exception err)
			{
				jsonProgram["Error"] = err.Message;
			}

			return jsonProgram;
		}

		//>+ Extract the above code into a method FixProtoScript(strProgram, strErrorMessage) that returns strProgram
		public static async Task<string> VerifyAndFixProtoScript(string strDirective, string strProgram, ProtoScriptTagger tagger)
		{
			try
			{
				PrototypeDefinition prototype = PrototypeDefinitions.Parse(strProgram);
			}
			catch (Exception err)
			{
				string strErrorMessage = "Could not compile program: " + err.Message;
				if (null != err.InnerException)
					strErrorMessage += ", " + err.InnerException.Message;

				string strFixed = await FixProtoScript(strDirective, strProgram, strErrorMessage, tagger);
				JsonObject jsonResult = new JsonObject(strFixed);
				strProgram = jsonResult.GetStringOrNull("fixedProgram");
			}

			try
			{
				tagger.InterpretCode(strProgram);
			}
			catch (Exception err)
			{
				string strErrorMessage = "Could not execute program: ";

				if (tagger.Compiler.Diagnostics.Count > 0)
				{
					foreach (var diagnostic in tagger.Compiler.Diagnostics)
					{
						strErrorMessage += "\n" + diagnostic.Diagnostic.Message;
						StatementParsingInfo? info = diagnostic.Statement?.Info ?? diagnostic.Expression?.Info;
						if (null != info)
							strErrorMessage += " at `" + strProgram.Substring(info.StartingOffset, info.Length) + "`";
					}
				}
				else
				{
					strErrorMessage += err.Message;
					if (null != err.InnerException)
						strErrorMessage += ", " + err.InnerException.Message;
				}

				string strFixed = await FixProtoScript(strDirective, strProgram, strErrorMessage, tagger);
				JsonObject jsonResult = new JsonObject(strFixed);
				strProgram = jsonResult.GetStringOrNull("fixedProgram");

			}

			return strProgram;
		}

		public static async Task<string> FixProtoScript(string strDirective, string strProgram, string strErrorMessage, ProtoScriptTagger tagger)
		{
			// Load the fragment ProtoScript Basics 
			FragmentsRow rowProtoScript = Fragments.GetFragmentByFragmentKey("ProtoScript Basics");

			//>+ load the fragment "ProtoScript Error Fixes"
			FragmentsRow rowProtoScriptErrorFixes = Fragments.GetFragmentByFragmentKey("ProtoScript Error Fixes");


			string strPrompt = rowProtoScriptErrorFixes.Fragment;
			strPrompt = LLMs.AddSectionToPrompt(strPrompt, "ProtoScript Guidelines", rowProtoScript.Fragment);

			string strInput = @"
# Original Directive

" + strDirective + @"

# Original Program 

" + strProgram + @"

# Error Message

" + strErrorMessage;


			strProgram = await Completions.CompleteAsJSON(strInput, strPrompt, ModelSize.LargeModel);

			Logs.DebugLog.WriteEvent("Fixed Program", JsonUtil.ToFriendlyJSON(strProgram));

			return strProgram;
		}

		internal static StringBuilder ActionsToString(List<Prototype> lstMethods, ProtoScriptTagger tagger)
		{
			StringBuilder descriptionBuilder = new StringBuilder();
			foreach (Prototype method in lstMethods)
			{
				string strMethodString = ProtoScriptActions.ActionToString(method, tagger);
				descriptionBuilder.AppendLine("##Name: " + method.PrototypeName);
				descriptionBuilder.Append(strMethodString);

				foreach (var file in tagger.Compiler.Files)
				{
					foreach (var protoDef in file.PrototypeDefinitions)
					{
						if (protoDef.PrototypeName.TypeName == method.PrototypeName)
						{
							descriptionBuilder.AppendLine("##Code");
							string strMethodCode = file.RawCode.Substring(protoDef.Info.StartingOffset, protoDef.Info.Length);
							descriptionBuilder.AppendLine(strMethodCode);
							break;
						}
					}
				}
			}

			return descriptionBuilder;
		}


		public static async Task<string> ChatWithCode(string strDirective, JsonObject jsonRequest, SessionObject session)
		{
			//List<Prototype> lstMethods = await ProtoScriptActions.GetRelevantActions(strDirective);
			//lstMethods.AddRange(await CSharpMethods.GetRelevantMethods(strDirective));

			//return await ChatWithCodeFromActions(strDirective, lstMethods, jsonRequest, session);
			throw new NotImplementedException();

		}

		public async static Task<string> PlanProgram(string strDirective, JsonObject jsonRequest, SessionObject session)
		{
			//			ProtoScriptTagger tagger = session.Tagger;

			//			if (!session.HasSystemMessage())
			//			{
			//				FragmentsRow rowPlanProgram = Fragments.GetFragmentByFragmentKey("Plan Program");

			//				StringBuilder sbPrompt = new StringBuilder();
			//				sbPrompt.AppendLine(rowPlanProgram.Fragment);

			//				//>+ get the ProtoScript Basics fragment and append it to the prompt
			//				FragmentsRow rowProtoScriptBasics = Fragments.GetFragmentByFragmentKey("ProtoScript Basics");
			//				sbPrompt.AppendLine(rowProtoScriptBasics.Fragment);

			//				string strPrompt = sbPrompt.ToString();

			//				session.AddSystemMessage(strPrompt);

			//				Logs.DebugLog.WriteEvent("PlanProgram.Prompt", strPrompt);
			//			}


			//                        List<Prototype> lstProtoScriptActions = await ProtoScriptActions.GetRelevantActions(strDirective);
			//                        List<Prototype> lstCSharpMethods = await CSharpMethods.GetRelevantMethods(strDirective);

			//			StringBuilder strProtoScriptDescriptions = ActionsToString(lstProtoScriptActions, tagger);
			//			string strCSharpDescriptions = CSharpMethods.MethodsToString(lstCSharpMethods);

			//			List<Prototype> lstRelatedEntities = Entities.RecognizeEntitiesViaActivationSpreading(strDirective, tagger);
			//			StringBuilder sbEntities = new StringBuilder(Entities.EntitiesToString(lstRelatedEntities));



			//			string strInput = @"
			//# Input Phrase

			//" + strDirective + @"

			//# ProtoScript Actions

			//These actions are available to you to use in your program and are in ProtoScript:

			//" + strProtoScriptDescriptions + @"

			//# CSharp Methods

			//These methods are available to you to use in your program and are in CSharp:

			//" + strCSharpDescriptions + @"

			//# Related Entities

			//These entities are related to the directive and may be useful in your program:

			//" + sbEntities.ToString();

			//			//> add each key / value from the request to the input
			//			foreach (var key in jsonRequest?.Keys ?? Enumerable.Empty<string>())
			//			{
			//				string strKey = key;
			//				string strValue = jsonRequest.GetStringOrNull(key);
			//				if (strKey != null)
			//				{
			//					strInput += string.Format("\n# {0}\n{1}", strKey, strValue);
			//				}
			//			}

			//			session.AddUserMessage(strInput);

			//			Logs.DebugLog.WriteEvent("PlanProgram.Input", strInput);


			//			JsonObject jsonResult = await LLMs.ExecuteLLMPromptAndInputWithHistory(session.Messages, ModelSize.LargeModel);

			//			string strResult = jsonResult.ToJSON();

			//			session.AddAgentMessage(strResult);

			//			return strResult;
			throw new NotImplementedException();
		}

		static public async Task<JsonObject> PlanProgram2(string strDirective, JsonObject jsonRequest, SessionObject session)
		{
			JsonObject jsonResult = new JsonObject();

			ProtoScriptTagger tagger = session.Tagger;

			Prototype protoDirective = await DirectivePrototypes.GetDirectivePrototype(strDirective, tagger);
			Prototype protoPredicates = protoDirective.Properties["LearnedDirective.Field.Predicates"];

			jsonResult["DirectivePrototype"] = protoDirective.ToFriendlyJsonObject();
			JsonArray jsonPredictateResults = new JsonArray();
			jsonResult["PredictateResults"] = jsonPredictateResults;

			bool bAllVerified = await Predicates.VerifyAllPredicates(protoPredicates, session, jsonPredictateResults);

			if (bAllVerified)
				return jsonResult;


			Prototype? protoProgram = await ProtoScriptActions.GetExistingProgram(strDirective);
			string? strNewProgram = null;

			if (null != protoProgram)
			{
				jsonResult["ExistingProgram"] = protoProgram.PrototypeName;
			}

			else
			{
				JsonObject jsonProgram = await ProtoScriptActions.GenerateProgramAndVerify(session, strDirective, jsonRequest);

				string strProgram = jsonProgram.GetStringOrNull("Program");
				string strCall = jsonProgram.GetStringOrNull("Call");

				jsonResult["Program"] = strProgram;
				jsonResult["Call"] = strCall;
			}

			return jsonResult;
		}


	}

}