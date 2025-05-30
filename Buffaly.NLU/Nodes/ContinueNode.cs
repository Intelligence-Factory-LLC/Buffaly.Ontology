//added
using Ontology;
using ProtoScript.Interpretter;
using ProtoScript.Interpretter.RuntimeInfo;
using Buffaly.NLU.Tagger;
using Buffaly.NLU.Tagger.RolloutController;

namespace Buffaly.NLU.Nodes
{
	public class ContinueNode : ProtoScriptControlNode
	{
		private ControllerInfo ControllerInfo = null;
		public ContinueNode(Prototype source, ControllerInfo info, NativeInterpretter interpretter) : base(interpretter)
		{
			ControllerInfo = info;
			Source = source;
		}

		public override List<BaseNode> GeneratePossibilities()
		{
			List<BaseNode> lstBaseNodes = new List<BaseNode>();

			foreach (FunctionRuntimeInfo f in this.ControllerInfo.ContinueFunctions)
			{
				Prototype protoParameter = (f.Parameters[0].Type as PrototypeTypeInfo).Prototype;
				if (Prototypes.TypeOf(this.Source, protoParameter))
				{
					Prototype result = this.Interpretter.RunMethodAsPrototype(f, null, new List<object> { this.Source });
					foreach (Prototype child in result.Children)
					{
						ContinueNode childContinue = new ContinueNode(child, this.ControllerInfo, this.Interpretter);
						lstBaseNodes.Add(childContinue);
					}
				}
			}

			return lstBaseNodes;
		}

		public override ControlFlag EvaluateNext(BaseNode node)
		{
			ContinueNode continueNode = node as ContinueNode;

			foreach (FunctionRuntimeInfo f in this.ControllerInfo.ExitConditions)
			{
				Prototype protoParameter = (f.Parameters[0].Type as PrototypeTypeInfo).Prototype;
				if (Prototypes.TypeOf(continueNode.Source, protoParameter))
				{
					Prototype result = this.Interpretter.RunMethodAsPrototype(f, null, new List<object> { continueNode.Source });
					if (null != result)
					{
						//N20220726-02 - Allow the controller to return a normal prototype then wrap it here
						//instead of dealing the C# object SingleResult 
						return new SingleResult(result);
					}
				}
			}

			return new Continue();
		}

	}
}
