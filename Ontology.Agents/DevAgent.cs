using BasicUtilities;
using BasicUtilities.Collections;
using Buffaly.NLU;
using Ontology.Agents.Modalities;
using Ontology.Agents.Actions;
using System.Text;

namespace Ontology.Agents
{
	public class DevAgent : BaseAgent
	{
		static DevAgent()
		{
			Instance = new DevAgent();
			Instance.Initialize();
		}

		public static DevAgent Instance { get; }

		public override void Initialize()
		{
			AgentName = "Dev.Agent";
			ProjectFile = BasicUtilities.Settings.GetStringOrDefault(
				"Dev.Agent.ProjectFile",
				@"C:\dev\ai\Ontology\ProtoScript.Tests\DevAgent\Project.pts"
			);
			SQLPipeline.ApplicationName = AgentName;
		}

		protected override ProtoScriptTagger InitializeProjectTagger(SessionObject session)
		{
			// Potentially different logic from FeedingFrenzy if you want:
			UnderstandUtil.TaggingSettings settings = new UnderstandUtil.TaggingSettings()
			{
				EnableDatabase = false, 
				MaxIterations = 100,
				GlobalObjects = new Map<string, object>() { },
				Project = GetProject()
			};

			settings.GlobalObjects["_sessionObject"] = session;

			return UnderstandUtil.GetAndInitializeProtoScriptTagger(settings);
		}

		public override async Task<JsonObject> ProcessDirectiveInternal(string sessionKey, string directive, JsonObject? requestValues)
		{
			string strRequestKey = Guid.NewGuid().ToString();
			Logs.DebugLog.WriteEvent("DevAgent.ProcessDirective", strRequestKey + ":" + directive);

			SessionObject session = GetOrCreateSession(sessionKey);
			requestValues ??= new JsonObject();
			requestValues["Conversation"] = session.GetConversationHistoryAsTextBlock();
			JsonObject jsonResult = new JsonObject(); 
			string strResult = await ProtoScriptActions.ChatWithCode(directive, requestValues, session);

			session.AddUserMessage(directive);
			session.AddAgentMessage(strResult);

			jsonResult["Result"] = strResult;
			return jsonResult;
		}

		public static async Task<JsonObject> ProcessDirective(string SessionKey, string Directive, JsonObject RequestValues)
		{
			return await Instance.ProcessDirectiveInternal(SessionKey, Directive, RequestValues);
		}


		public static string GetNewSessionKey()
		{
			return Guid.NewGuid().ToString();
		}



		public static async Task<JsonObject> GenerateProgram(string strSessionKey, string strDirective, JsonObject requestValues)
		{
			SessionObject session = Instance.GetOrCreateSession(strSessionKey);

			JsonObject jsonResult = await ProtoScriptActions.GenerateProgramAndVerify(session, strDirective, requestValues);

			return jsonResult;
		}

		//> create a method to call PlanProgram
		public static async Task<JsonObject> PlanProgram(string strSessionKey, string strDirective, JsonObject requestValues)
		{
			SessionObject session = Instance.GetOrCreateSession(strSessionKey);

			JsonObject jsonResult = await ProtoScriptActions.PlanProgram2(strDirective, requestValues, session);

			return jsonResult;
		}


		//> create a method to call KnowledgeBase.AnswerQuestion
		public static async Task<JsonObject> AnswerQuestion(string strQuestion)
		{
			return await KnowledgeBase.AnswerQuestion(strQuestion);
		}
	}
}
