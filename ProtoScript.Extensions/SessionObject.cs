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

		public string SessionKey;
		public ProtoScriptTagger Tagger;
		public JsonObject Settings = new JsonObject();

		public ConcurrentDictionary<string, ObjectCache> m_objectCache = null;
		public ConcurrentDictionary<string, RowCache> m_rowCache = null;

		public static SessionObject Create(string strSessionKey = null)
		{
			strSessionKey ??= Guid.NewGuid().ToString();

			SessionObject session = new SessionObject()
			{
				SessionKey = strSessionKey,
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
