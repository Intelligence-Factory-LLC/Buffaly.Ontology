using Ontology.GraphInduction.Model;

namespace Ontology.GraphInduction.Utils
{
	public class Logger
	{


		public static void Log(HCP hcp)
		{
			Logs.DebugLog.WriteEvent("HCP", null == hcp ? "(null)" : "\r\n" + FormatUtil.ToFriendlyString(hcp, new FormatUtil.HCPFormatting() ).ToString());
		}


		//public static void Log(HCPTree.Node node)
		//{
		//	Logs.DebugLog.WriteEvent("HCPTree", "\r\n" + FormatUtil.ToFriendlyString(node, 0).ToString());
		//}


		static public void Log(Prototype ? prototype)
		{
			Logs.DebugLog.WriteEvent("Prototype", null == prototype ? "(null)" : "\r\n" + FormatUtil.FormatPrototype(prototype).ToString());
		}

		static public void Log(IEnumerable<Prototype> lstPrototypes)
		{
			int i = 0; 
			foreach (Prototype child in lstPrototypes)
			{
				Logs.DebugLog.WriteEvent("Prototype " + i++ + " (" + child.Value + ")", "\r\n" + FormatUtil.FormatPrototype(child).ToString());
			}
		}

		static public void Log(Collection collection)
		{
			Log((IEnumerable<Prototype>)collection);
		}

		static public void Log(CSharp.Statement statement)
        {
            Logs.DebugLog.WriteEvent("Statement", "\r\n" + CSharp.Parsers.SimpleGenerator.Generate(statement));
        }

		static public void Log(PrototypeComparison.Result result)
		{
			Logs.DebugLog.WriteEvent("Difference", result.DifferenceType.ToString());

			if (null != result.IsolatedOriginal)
			{
				Logger.Log(result.OriginalParent);
			}

			Logs.DebugLog.WriteEvent("-->", "");

			if (null != result.IsolatedNew)
			{
				Logger.Log(result.NewParent);
			}

			Logs.DebugLog.WriteEvent("Isolated", result.DifferenceType.ToString());

			if (null != result.IsolatedOriginal)
			{
				Logger.Log(result.IsolatedOriginal);
			}

			if (null != result.IsolatedNew)
			{
				Logger.Log(result.IsolatedNew);
			}
		}
	}
}
