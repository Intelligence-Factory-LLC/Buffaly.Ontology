using BasicUtilities;
using BasicUtilities.Collections;
using Buffaly.NLU;
using Ontology.Agents.Actions;

namespace Ontology.Agents
{
	public class BuffalyAgent : BaseAgent
	{
		static BuffalyAgent()
		{
			Instance = new BuffalyAgent();
			Instance.Initialize();
		}

		public static BuffalyAgent Instance { get; }

		public override void Initialize()
		{
			AgentName = "Buffaly.Agent";
			ProjectFile = BasicUtilities.Settings.GetStringOrDefault(
				"Buffaly.Agent.ProjectFile",
				@"C:\dev\ai\Ontology\ProtoScript.Tests\NL_Project_1.0.1\Project.pts"
			);
		}

		protected override ProtoScriptTagger InitializeProjectTagger(SessionObject session)
		{
			// Potentially different logic from FeedingFrenzy if you want:
			UnderstandUtil.TaggingSettings settings = new UnderstandUtil.TaggingSettings()
			{
				EnableDatabase = false, 
				MaxIterations = 1000,
				GlobalObjects = new Map<string, object>() { },
				Project = GetProject()
			};

			settings.GlobalObjects["_sessionObject"] = session;

			return UnderstandUtil.GetAndInitializeProtoScriptTagger(settings);
		}

		public override async Task<JsonObject> ProcessDirectiveInternal(string sessionKey, string directive, JsonObject? requestValues)
		{
			string strRequestKey = Guid.NewGuid().ToString();
			Logs.DebugLog.WriteEvent("BuffalyAgent.ProcessDirective", strRequestKey + ":" + directive);

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

	}
}
