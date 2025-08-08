using BasicUtilities;
using RooTrax.Cache;
using System.Collections.Concurrent;
using WebAppUtilities;
using Buffaly.NLU;

namespace Ontology.Agents
{


	public class RequestValues
	{
		public string RequestKey;
		public List<JsonObject> Responses;
		public List<string> Prompts;
	}


	public abstract class BaseAgent : JsonWs
	{
		protected string AgentName;
		protected string ProjectFile;

		protected ConcurrentDictionary<string, SessionObject> m_mapSessions;
		protected ConcurrentDictionary<string, RequestValues> m_mapRequests;

		protected BaseAgent()
		{
			m_mapSessions = new ConcurrentDictionary<string, SessionObject>();
			m_mapRequests = new ConcurrentDictionary<string, RequestValues>();
		}

		/// <summary>
		/// Called once (e.g., in a static constructor of the derived class)
		/// to set up the unique bits like AgentName, ProjectFile, etc.
		/// </summary>
		public abstract void Initialize();

		/// <summary>
		/// Each derived agent can configure the Tagger differently.
		/// </summary>
		protected abstract ProtoScriptTagger InitializeProjectTagger(SessionObject session);
		public abstract Task<JsonObject> ProcessDirectiveInternal(string sessionKey, string directive, JsonObject ? requestValues);


		public string GetProject()
		{
			if (StringUtil.IsEmpty(ProjectFile))
				throw new Exception("ProjectFile should be set before getting the project");

			return ProjectFile;
		}

		//public ProtoScriptTagger GetProjectTagger()
		//{
		//	SessionObject sessionObject = CreateSession();
		//	return sessionObject.Tagger;
		//}

		public SessionObject CreateSession(string ? strSessionKey)
		{
			if (strSessionKey == null)
			{
				strSessionKey = Guid.NewGuid().ToString();
			}			
			
			SessionObject session =new SessionObject
			{
				SessionKey = strSessionKey
			}; 

			// Enter the session before creating the tagger
			EnterSession(session);

			session.Tagger = InitializeProjectTagger(session);
			m_mapSessions[strSessionKey] = session;

			return session;
		}

		public SessionObject GetOrCreateSession(string strSessionKey)
		{
			if (StringUtil.IsEmpty(strSessionKey)
				|| !m_mapSessions.TryGetValue(strSessionKey, out SessionObject session))
			{
				return CreateSession(strSessionKey);
			}

			EnterSession(session);
			return session;
		}

		/// <summary>
		/// Sets the async-local caches and ensures each session 
		/// is "entered" for subsequent operations.
		/// </summary>
		protected void EnterSession(SessionObject session)
		{
			CacheManager.UseAsyncLocal = true;
			ObjectCacheManager.UseAsyncLocal = true;

			session.Enter();

			//TemporaryPrototypes uses a local pointer to the cache that has to be reloaded
			TemporaryPrototypes.ReloadCache();
		}

		/// <summary>
		/// Clears the local dictionaries and other static caches (if needed).
		/// </summary>
		public virtual void Reset()
		{
			m_mapSessions.Clear();
			m_mapRequests.Clear();

			// Also reset the global caches
			Initializer.ResetCache();
			CacheInitializer.ResetCache();
		}
	}

}