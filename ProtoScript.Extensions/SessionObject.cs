using BasicUtilities;
using Buffaly.NLU;
using RooTrax.Cache;
using System.Collections.Concurrent;

namespace ProtoScript.Extensions
{
	public class SessionObject
	{
		private CodeContext m_Context = new CodeContext();
		public CodeContext Context
		{
			get
			{
				return m_Context;
			}

			set
			{
				m_Context = value;
			}

		}



		//		public SessionsRow SessionsRow;
		public string SessionKey;
		public ProtoScriptTagger Tagger;
		public JsonObject Settings = new JsonObject();

		public ConcurrentDictionary<string, ObjectCache> m_objectCache = null;
		public ConcurrentDictionary<string, RowCache> m_rowCache = null;

		public static SessionObject Create(string strSessionKey = null)
		{
			strSessionKey ??= Guid.NewGuid().ToString();
//			SessionsRow rowNewSession = new SessionsRow();
//			rowNewSession.DataObject["ExternalKey"] = strSessionKey;

			SessionObject session = new SessionObject()
			{
				SessionKey = strSessionKey,
//				SessionsRow = rowNewSession
			};

			return session;
		}

		public void Enter()
		{
			if (this.m_rowCache == null)
			{
				this.m_rowCache = CacheManager.SetAsyncLocalCache();
				this.m_objectCache = ObjectCacheManager.SetAsyncLocalCache();
			}
			else
			{
				CacheManager.SetAsyncLocalCache(this.m_rowCache);
				ObjectCacheManager.SetAsyncLocalCache(this.m_objectCache);
			}
		}

		//public List<OpenAIAPI.OpenAICompletions.Message> ConvertMessagesToList()
		//{
		//	List<OpenAIAPI.OpenAICompletions.Message> lstMessages = new List<OpenAIAPI.OpenAICompletions.Message>();
		//	foreach (Data.MessagesRow row in this.SessionsRow.Messages)
		//	{
		//		OpenAIAPI.OpenAICompletions.Message message = new OpenAIAPI.OpenAICompletions.Message()
		//		{
		//			Content = row.MessageText,
		//			Role = row.IsIncoming ? OpenAIAPI.OpenAICompletions.MessageRole.User : (row.IsSystemMessage ? OpenAIAPI.Completions.MessageRole.System : OpenAIAPI.Completions.MessageRole.Assistant)
		//		};
		//		lstMessages.Add(message);
		//	}
		//	return lstMessages;
		//}


		//The idea behind these methods is to allow for an object Session to be saved 
		//But it is not currently being used. 
		public void SetObject<T>(string strKey, T oObj) where T : class
		{
			ObjectCache session = ObjectCacheManager.Instance.GetOrCreateCache("Session");
			session.Insert<T>(oObj, strKey);
		}

		public T GetObject<T>(string strKey) where T : class
		{
			ObjectCache session = ObjectCacheManager.Instance.GetOrCreateCache("Session");
			return session.Get<T>(strKey);
		}
	}

}
