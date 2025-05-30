using Ontology;

namespace Buffaly.NLU
{
	public class CacheInitializer
	{
		public static void ResetCache()
		{
			//TemporaryLexemes.m_mapRelatedPrototypes.Clear();
			TemporaryPrototypes.ResetCache();
			TemporaryActions.Clear();
			TemporarySubTypes.Clear();

			Buffaly.NLU.Nodes.ProtoScriptControllers.Clear();
		}
	}
}
