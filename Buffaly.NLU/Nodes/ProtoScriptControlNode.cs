//added
using ProtoScript.Interpretter;
using Buffaly.NLU.Tagger;
using System.Text;

namespace Buffaly.NLU.Nodes
{
	public class ProtoScriptControlNode : BaseFunctionNode
	{
		protected NativeInterpretter Interpretter = null;
		public ProtoScriptControlNode(NativeInterpretter interpretter)
		{
			this.Interpretter = interpretter;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(this.GetType().Name + " - ");
			sb.Append(this.Source.PrototypeName);
			return sb.ToString();
		}

	}
}
