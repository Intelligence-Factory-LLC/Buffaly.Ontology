using System.Text;
using Ontology;
using Buffaly.NLU.Tagger.RolloutController;

namespace Buffaly.NLU.Tagger.Nodes
{
	public class DivisionNode : BaseFunctionNode
	{
		public class DivisionResult : BaseNode
		{
			public DivisionResult(RangeResult rangeResult)
			{
				Result = rangeResult;
			}

			public RangeResult Result = null;
		}

		public BaseTaggingNode Left = new BaseTaggingNode(Ontology.Collection.Prototype.Clone(), Ontology.Collection.Prototype.Clone());
		public BaseTaggingNode Right = new BaseTaggingNode(Ontology.Collection.Prototype.Clone(), Ontology.Collection.Prototype.Clone());

		public int SourceIndex;
		public int ReferenceIndex;

		public DivisionNode()
		{

		}
		public DivisionNode(Prototype source, Prototype reference, int iSourceIndex, int iReferenceIndex) : base(source, reference)
		{
			this.SourceIndex = iSourceIndex;
			this.ReferenceIndex = iReferenceIndex;
			this.Reference = reference;
			this.Source = source;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder("DivisionNode - ");
			for (int i = 0; i < Source.Children.Count; i++)
			{
				if (i > 0)
					sb.Append(",");

				if (i == this.SourceIndex)
					sb.Append("[");

				sb.Append(PrototypeLogging.ToChildString(Source.Children[i]));

				if (i == this.SourceIndex)
					sb.Append("]");
			}

			sb.Append(" / ");

			for (int i = 0; i < Reference.Children.Count; i++)
			{
				if (i > 0)
					sb.Append(",");

				if (i == this.ReferenceIndex)
					sb.Append("[");

				sb.Append(PrototypeLogging.ToChildString(Reference.Children[i]));

				if (i == this.ReferenceIndex)
					sb.Append("]");
			}

			return sb.ToString();
		}

		public override List<BaseNode> GeneratePossibilities()
		{
			List<BaseNode> lstResults = new List<BaseNode>();

			if (Left.Source.Children.Count > 0 && Left.Reference.Children.Count > 0)
			{
				DecisionNode nodeLeft = new DecisionNode(Left.Source, Left.Reference);
				nodeLeft.AllowPreceeding = Left.AllowPreceeding;
				nodeLeft.OnResultReceivedCallback = this.OnLeftResultReceived;
				lstResults.Add(nodeLeft);
			}

			if (Right.Source.Children.Count > 0 && Right.Reference.Children.Count > 0)
			{
				DecisionNode nodeRight = new DecisionNode(Right.Source, Right.Reference);
				nodeRight.AllowTrailing = Right.AllowTrailing;
				nodeRight.OnResultReceivedCallback = this.OnRightResultReceived;
				lstResults.Add(nodeRight);
			}

			return lstResults;
		}

		public override ControlFlag EvaluateNext(BaseNode nodeChild)
		{
			if (nodeChild is DivisionResult)
				return (nodeChild as DivisionResult).Result;

			return new Continue();
		}

		private List<RangeResult> Rights = new List<RangeResult>();
		public ControlFlag OnRightResultReceived(Result result)
		{
			RangeResult divisionResult = new RangeResult();

			if (result is RangeResult)
				divisionResult = (result as RangeResult);

			else if (result is SingleResult)
			{
				divisionResult.Result = (result as SingleResult).Result;
				divisionResult.RangeStart = 0;

				//Can be a single item return instead of a collection
				if (Prototypes.AreShallowEqual(divisionResult.Result, Ontology.Collection.Prototype))
					divisionResult.RangeLength = Math.Max(divisionResult.Result.Children.Count, 1);
				else
					divisionResult.RangeLength = 1;
			}

			Rights.Add(divisionResult);

			RangeResult resultImmediate = null;

			if (Left.Reference.Children.Count > 0)
			{
				foreach (RangeResult left in Lefts)
				{
					RangeResult resultCombined = new RangeResult();

					//N20210703-02 - The result has to be built from the returned result because it may 
					//change via lower nodes. 
					resultCombined.Result = new Collection();

					Add(resultCombined.Result, left.Result);
					Add(resultCombined.Result, this.Source.Children[SourceIndex]);
					Add(resultCombined.Result, divisionResult.Result);
					
					resultCombined.RangeStart = left.RangeStart;
					resultCombined.RangeLength = left.RangeLength + 1 + divisionResult.RangeLength;

					if (null == resultImmediate)
						resultImmediate = resultCombined;
					else
						this.AddPossibility(new DivisionResult(resultCombined));
				}
			}
			else
			{
				RangeResult resultCombined = new RangeResult();
				
				resultCombined.Result = new Collection();
				if (SourceIndex > 0)
					Add(resultCombined.Result, Collection.GetRange(this.Source, 0, SourceIndex));

				Add(resultCombined.Result, this.Source.Children[SourceIndex]);
				Add(resultCombined.Result, divisionResult.Result);

				resultCombined.RangeStart = SourceIndex + divisionResult.RangeStart;
				resultCombined.RangeLength = 1 + divisionResult.RangeLength;

				resultImmediate = resultCombined;
			}			

			return resultImmediate;
		}

		private Prototype Add(Prototype prototype, Prototype protoNew)
		{
			//N20220113-02
			if (Prototypes.TypeOf(protoNew, Ontology.Collection.Prototype))
				prototype.Children.AddRange(protoNew.Children);
			else
				prototype.Children.Add(protoNew); 

			return prototype;
		}

		private List<RangeResult> Lefts = new List<RangeResult>();
		public ControlFlag OnLeftResultReceived(Result result)
		{
			RangeResult divisionResult = new RangeResult();

			if (result is RangeResult)
				divisionResult = (result as RangeResult);

			else if (result is SingleResult)
			{
				divisionResult.Result = (result as SingleResult).Result;
				divisionResult.RangeStart = 0;

				//Can be a single item return instead of a collection
				if (Prototypes.AreShallowEqual(divisionResult.Result, Ontology.Collection.Prototype))
					divisionResult.RangeLength = Math.Max(divisionResult.Result.Children.Count, 1);
				else
					divisionResult.RangeLength = 1;
			}

			Lefts.Add(divisionResult);

			RangeResult resultImmediate = null;

			if (Right.Reference.Children.Count > 0)
			{
				foreach (RangeResult right in Rights)
				{
					RangeResult resultCombined = new RangeResult();

					resultCombined.Result = new Collection();
					Add(resultCombined.Result, divisionResult.Result);
					Add(resultCombined.Result, this.Source.Children[SourceIndex]);
					Add(resultCombined.Result, right.Result);
					
					resultCombined.RangeStart = divisionResult.RangeStart;
					resultCombined.RangeLength = right.RangeLength + 1 + divisionResult.RangeLength;

					if (null == resultImmediate)
						resultImmediate = resultCombined;
					else 
						this.AddPossibility(new DivisionResult(resultCombined));
				}
			}
			else
			{
				RangeResult resultCombined = new RangeResult();
				resultCombined.Result = new Collection();

				Add(resultCombined.Result, divisionResult.Result);
				Add(resultCombined.Result, this.Source.Children[SourceIndex]);

				if (SourceIndex < this.Source.Children.Count - 1)
					Add(resultCombined.Result, Collection.GetRange(this.Source, SourceIndex + 1, this.Source.Children.Count - SourceIndex - 1));
				
				
				resultCombined.RangeStart = divisionResult.RangeStart;
				resultCombined.RangeLength = 1 + divisionResult.RangeLength;

				resultImmediate = resultCombined;
			}

			return resultImmediate;
		}
	}
}
