//added
using Ontology;
using ProtoScript.Interpretter;
using ProtoScript.Interpretter.RuntimeInfo;
using Buffaly.NLU.Tagger;
using Buffaly.NLU.Tagger.RolloutController;

namespace Buffaly.NLU.Nodes
{
	public class StartNode : ProtoScriptControlNode
	{
		private ControllerInfo ControllerInfo = null;
		public StartNode(Prototype source, ControllerInfo info, NativeInterpretter interpretter) : base(interpretter)
		{
			ControllerInfo = info;
			Source = source;
		}

		public override List<BaseNode> GeneratePossibilities()
		{
			List<BaseNode> lstBaseNodes = new List<BaseNode>();

			Prototype collection = this.Interpretter.RunMethodAsPrototype(this.ControllerInfo.StartFunction, null, new List<object> { this.Source });
			foreach (Prototype entity in collection.Children)
			{
				ContinueNode node = new ContinueNode(entity, this.ControllerInfo, this.Interpretter);
				lstBaseNodes.Add(node);
			}

			return lstBaseNodes;
		}

		public override ControlFlag EvaluateNext(BaseNode node)
		{
			ContinueNode startNode = node as ContinueNode;

			foreach (FunctionRuntimeInfo f in this.ControllerInfo.ExitConditions)
			{
				Prototype protoParameter = (f.Parameters[0].Type as PrototypeTypeInfo).Prototype;
				if (Prototypes.TypeOf(startNode.Source, protoParameter))
				{
					Prototype result = this.Interpretter.RunMethodAsPrototype(f, null, new List<object> { startNode.Source });
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
