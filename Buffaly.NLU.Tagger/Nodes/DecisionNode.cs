using Buffaly.NLU.Tagger.RolloutController;
using Ontology;

namespace Buffaly.NLU.Tagger.Nodes
{
	public class DecisionNode : BaseFunctionNode
	{
		public DecisionNode(Prototype protoSource, Prototype protoReference) : base(protoSource, protoReference)
		{
			 
		}

		public DecisionNode(Prototype protoSource, Prototype protoReference, bool bAllowPreceeding, bool bAllowTrailing)
		{
			Source = protoSource;
			Reference = protoReference;
			AllowPreceeding = bAllowPreceeding;
			AllowTrailing = bAllowTrailing;
		}

		public override List<BaseNode> GeneratePossibilities()
		{
			List<BaseNode> lstBaseNode = new List<BaseNode>();
			  
			
			if (this.Reference.Children.Count() == 1)
			{
				lstBaseNode.Add(new RangeNode(this));				
			}

			else if (Reference.Children.Count == Source.Children.Count)
			{
				lstBaseNode.Add(new SimpleExpectationsNode2(Source, Reference));
			}

			else
			{
				for (int i = 0; i < Reference.Children.Count; i++)
				{
					if (MatchNode.IsLeafType(Reference.Children[i]))
					{
						lstBaseNode.Add(new FindDivisionNode(Source, Reference, i));
					}
				}
			}

			//N20220126-01
			if (lstBaseNode.Count == 0)
			{
				//TODO: May be another node for a reverse sequence
				if (AllowPreceeding == false && AllowTrailing == true)
				{
					Prototype protoSource = new Collection(Source.Children.GetRange(0, Reference.Children.Count));
					SimpleExpectationsNode2 node = new SimpleExpectationsNode2(protoSource, Reference);
					node.OnResultReceivedCallback = x => OnRangedResultReceived(x, 0, Reference.Children.Count);
					lstBaseNode.Add(node);
				}

			}


			return lstBaseNode;
		}

		public override ControlFlag EvaluateNext(BaseNode nodeChild)
		{
			return base.EvaluateNext(nodeChild);
		}

		private ControlFlag OnRangedResultReceived(ControlFlag flag, int iRangeStart, int iRangeLength)
		{
			//N20210522-01 - Need to keep the original collection but identify the matching piece of the range
			if (flag is SingleResult)
			{
				SingleResult singleResult = flag as SingleResult;
				RangeResult result = new RangeResult();

				result.Result = new Collection();
				result.Result.Children.AddRange(Collection.GetRange(this.Source, 0, iRangeStart).Children);

				int iNewLength = iRangeLength;
				if (Prototypes.TypeOf(singleResult.Result, Ontology.Collection.Prototype))
				{
					result.Result.Children.AddRange(singleResult.Result.Children);
					iNewLength = singleResult.Result.Children.Count;
				}
				else
				{
					result.Result.Children.Add(singleResult.Result);
					iNewLength = 1;
				}

				result.Result.Children.AddRange(Collection.GetRange(this.Source, iRangeStart + iRangeLength, this.Source.Children.Count - iRangeStart - iRangeLength).Children);
				result.RangeStart = iRangeStart;
				result.RangeLength = iNewLength;

				return result;
			}

			return flag;
		}
	}
}
