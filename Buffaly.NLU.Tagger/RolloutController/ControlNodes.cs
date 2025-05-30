namespace Buffaly.NLU.Tagger.RolloutController
{
	public class BaseNode : ControllerNode
	{
		public BaseNode()
		{
		}

		public BaseNode AddPossibility(BaseNode node)
		{
			node.Parent = this;
			this.Possibilities.Add(node);
			this.UpdateExplorationValues();
			return node;
		}

		public BaseNode AddPossibility(BaseNode node, double dValue)
		{
			node.Parent = this;
			node.Value = dValue;
			this.Possibilities.Add(node);
			this.UpdateExplorationValues();
			return node;
		}

		public virtual ControlFlag Evaluate()
		{
			return new Continue();
		}
	}

	public class FunctionNode : BaseNode
	{
		public virtual List<BaseNode> GeneratePossibilities()
		{
			return new List<BaseNode>();
		}

		public virtual ControlFlag EvaluateNext(BaseNode nodeChild)
		{
			return new Continue();
		}

		public virtual ControlFlag OnResultReceived(Result result)
		{
			return result; 
		}

		public virtual void OnNoMorePossibilities()
		{

		}

		public FunctionNode() : base()
		{
		}
	}
}
