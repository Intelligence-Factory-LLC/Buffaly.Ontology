//added
using Ontology;
using System.Text;
using Buffaly.NLU.Tagger.RolloutController;

namespace Buffaly.NLU.Tagger
{
    public class BaseTaggingNode : BaseNode
    {
		public Prototype Source;
		public Prototype Reference;
		public bool AllowPreceeding = false;
		public bool AllowTrailing = false;

		public BaseTaggingNode()
		{

		}

		public BaseTaggingNode(Prototype protoSource, Prototype protoReference)
		{
			Source = protoSource;
			Reference = protoReference;
		}

		public BaseTaggingNode(Prototype protoSource, Prototype protoReference, bool bAllowPreceeding, bool bAllowTrailing)
		{
			Source = protoSource;
			Reference = protoReference;
			AllowPreceeding = bAllowPreceeding;
			AllowTrailing = bAllowTrailing;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder("BaseTaggingNode - ");
			sb.Append(PrototypeLogging.ToChildString(Source));
			sb.Append(" / ");
			sb.Append(PrototypeLogging.ToChildString(Reference));
			sb.Append(" (").Append(this.Value).Append(")");
			return sb.ToString();
		}
	}

	public class BaseFunctionNode : FunctionNode
	{
		public Prototype Source;
		public Prototype Reference;
		public bool AllowPreceeding = false;
		public bool AllowTrailing = false;

		public delegate ControlFlag OnResultReceivedDelegate(Result result);
		public OnResultReceivedDelegate OnResultReceivedCallback = null;

		public override ControlFlag EvaluateNext(BaseNode nodeChild)
		{
			return nodeChild.Evaluate();
		}

		public BaseFunctionNode()
		{

		}

		public BaseFunctionNode(BaseFunctionNode node)
		{
			Source = node.Source;
			Reference = node.Reference;
			AllowPreceeding = node.AllowPreceeding;
			AllowTrailing = node.AllowTrailing;
		}
		public BaseFunctionNode(Prototype protoSource, Prototype protoReference)
		{
			Source = protoSource;
			Reference = protoReference;
		}

		public BaseFunctionNode(Prototype protoSource, Prototype protoReference, bool bAllowPreceeding, bool bAllowTrailing)
		{
			Source = protoSource;
			Reference = protoReference;
			AllowPreceeding = bAllowPreceeding;
			AllowTrailing = bAllowTrailing;
		}

		public override ControlFlag OnResultReceived(Result result)
		{
			if (null != this.OnResultReceivedCallback)
				return OnResultReceivedCallback(result);

			return result;
		}
	}

	public class SingleResult : Result
	{
		public Prototype Result;

		public SingleResult(Prototype result)
		{
			Result = result;
		}

		public override string ToString()
		{
			if (Prototypes.TypeOf(Result, Collection.Prototype))
				return "SingleResult[" + PrototypeLogging.ToChildString(Result) + "]";
			return "SingleResult[" + Result.ToString() + "]";
		}

		public override ControlFlag Clone()
		{
			SingleResult result = new SingleResult(this.Result.Clone());
			return result;
		}
	}


}
