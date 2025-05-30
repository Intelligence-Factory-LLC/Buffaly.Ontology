using BasicUtilities;
using Ontology;
using System.Text;

namespace Buffaly.NLU.Tagger.RolloutController
{
	public class Controller
	{
		public int MaxIterations = 10000;
		public int CurrentIterations = 0;
		public volatile bool IsStopped = false;

		protected Result ControlRoot(FunctionNode root)
		{
			Levels = 0;
			//WriteEvent("Source", PrototypeLogging.ToChildString(root.Source));
			//WriteEvent("Target", PrototypeLogging.ToChildString(root.Target));

			int i;
			for (i = 0; i < MaxIterations; i++)
			{
				CurrentIterations = i;
				Levels = 0;
				WriteStart("ControlOneStep", i.ToString());

				Result result = ControlOneStep(root);

				WriteStop("ControlOneStep", result == null ? "(null)" : result.ToString());

				if (null != result)
				{
					WriteEvent("ControlRoot", "Result Found in " + i.ToString() + " iterations");
					WritePathToStart(root, true);
					return result;
				}

				if (!root.HasMoreValuedPossibilities())
					break;

				if (IsStopped)
				{
					WriteEvent("ControlRoot", "Stop event detected.");
					break;
				}
			}

			WriteEvent("ControlRoot", $"Result not found after {i} iterations");

			return null;
		}

		private Result ControlOneStep(FunctionNode root)
		{
			if (IsStopped)
			{
				WriteEvent("ControlOneStep", "Stop event detected.");
				return null;
			}

			string strNodeType = root.GetType().Name;

			if (!root.HasPossibilities())
			{
				List<BaseNode> lstPossibilities = root.GeneratePossibilities();
				foreach (BaseNode childPossible in lstPossibilities)
				{
					//Cached paths can result in a known starting value of 0
					if (childPossible.Value > 0)
					{
						WriteEvent("Possibility", childPossible.ToString());
						root.AddPossibility(childPossible);
					}
				}
			}

			//N20210622-01 - Note that CurrentValue does not get moved up the tree, so a node 
			//can be visited that doesn't have any valued children. 
			BaseNode nodeChild = root.SelectNextPossibility() as BaseNode;
			if (null == nodeChild)
			{
				root.OnNoMorePossibilities();
				WriteEvent("ControlOneStep", "No more possibilities");
				return null;
			}

			else
			{
				nodeChild.Info.Iteration = CurrentIterations;
			}

			WriteStart(strNodeType + ".EvaluateNext", nodeChild.ToString());
			ControlFlag flag = root.EvaluateNext(nodeChild);
			WriteStop(strNodeType + ".EvaluateNext", null == flag ? "(null)" : flag.ToString());

			Result result = null;

			if (flag is Continue && nodeChild is FunctionNode)
			{
				WriteStart("ControlOneStep", nodeChild.ToString());
				result = ControlOneStep(nodeChild as FunctionNode) as Result;
				WriteStop("ControlOneStep", result == null ? "(null)" : result.ToString());
			}

			else if (flag is Result)
				result = flag as Result;

			if (null != result)
			{
				if (result is SingleResult)
					root.Info.SubTyped = (result as SingleResult).Result;
				else if (result is Nodes.RangeResult)
					root.Info.SubTyped = (result as Nodes.RangeResult).Result;
				else
				{

				}

				WriteStart(strNodeType + ".OnResultReceived", result.ToString());
				result = root.OnResultReceived(result) as Result;
				WriteStop(strNodeType + ".OnResultReceived", result == null ? "(null)" : result.ToString());
			}


			return result;
		}

		private static DebugLog m_DebugLog = null;
		static public DebugLog Log
		{
			get
			{
				if (null == m_DebugLog)
				{
					m_DebugLog = new DebugLog();
					m_DebugLog.LogPath = Logs.DebugLog.LogPath;
					m_DebugLog.MaxLinesPerFile = 0;
					m_DebugLog.Name = nameof(RolloutController);
				}

				return m_DebugLog;
			}
		}

		private int Levels = 0;
		public void WriteEvent(string strEventName, string strEventDescription)
		{
			strEventName = new string('\t', Levels) + strEventName;
			Log.WriteEvent(strEventName, strEventDescription);
		}

		public void WriteStart(string strEventName, string strEventDescription)
		{
			Levels++;
			WriteEvent(strEventName, strEventDescription);
		}

		public void WriteStop(string strEventName, string strEventDescription)
		{
			WriteEvent(strEventName, strEventDescription);
			Levels--;
		}


		public void WritePathToStart(ControllerNode nodeNew, bool bIncludeSelected)
		{
			WriteEvent("", "\r\n" + GetPathToStart(nodeNew).ToString());
		}

		public string GetPathToStart(ControllerNode nodeNew)
		{
			StringBuilder sbResult = new StringBuilder();

			sbResult.Append("Current " + CurrentIterations + "\t").AppendLine(nodeNew.ToString());
			sbResult.Append(WriteSelected(nodeNew));

			return sbResult.ToString();
		}
		private StringBuilder WritePrevious(ControllerNode node)
		{
			StringBuilder sbResult = new StringBuilder();

			List<Prototype> lstTrace = new List<Prototype>();
			ControllerNode nodeSelected = node;

			//This doesn't need to iterate because the call to WriteSelected 
			//performs the iteration via recursion
			if (nodeSelected.Info.Previous != null)
			{
				sbResult.Append(new string('\t', Levels)).Append("Previous " + nodeSelected.Info.Previous.Info.Iteration);
				sbResult.Append("\t");
				sbResult.AppendLine(nodeSelected.Info.Previous.ToString());

				nodeSelected = nodeSelected.Info.Previous;

				if (null != nodeSelected.Info.Selected)
				{
					sbResult.Append(WriteSelected(nodeSelected));
				}
			}

			return sbResult;
		}


		public StringBuilder WriteSelected(ControllerNode node)
		{
			StringBuilder sbResult = new StringBuilder();

			List<Prototype> lstTrace = new List<Prototype>();
			ControllerNode nodeSelected = node;
			
			Levels++;

			while (null != nodeSelected)
			{
				if (null != nodeSelected.Info.Selected)
				{
					StringBuilder sb = new StringBuilder();
					sb.Append(nodeSelected.Info.Selected.ToString());
					if (null != nodeSelected.Info.Selected.Info.SubTyped)
					{
						sb.Append(" -> ").Append(PrototypeLogging.ToChildString(nodeSelected.Info.Selected.Info.SubTyped));
					}

					sbResult.Append(new string('\t', Levels)).Append("Selected " + nodeSelected.Info.Selected.Info.Iteration);
					sbResult.Append("\t");
					sbResult.AppendLine(sb.ToString());
				}

				nodeSelected = nodeSelected.Info.Selected;
			}

			Levels--;

			nodeSelected = node;

			while (null != nodeSelected)
			{
				if (null != nodeSelected.Info.Previous)
				{
					sbResult.Append(WritePrevious(nodeSelected));
				}

				nodeSelected = nodeSelected.Info.Selected;
			}

			return sbResult;
		}

	}
}
