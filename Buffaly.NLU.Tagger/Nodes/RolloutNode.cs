using Buffaly.NLU.Tagger.RolloutController;
using Ontology;
using System.Text;

namespace Buffaly.NLU.Tagger.Nodes
{
	public class RolloutNode : BaseFunctionNode
	{
		public RolloutNode(Prototype protoSource)
		{
			Source = protoSource;
			Value = ValueFunctions.GetFragmentValue(protoSource);
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder("RolloutNode - ");
			sb.Append(PrototypeLogging.ToChildString(Source));
			return sb.ToString();
		}

		public override List<BaseNode> GeneratePossibilities()
		{
			List<BaseNode> lstBaseNode = new List<BaseNode>();

			//N20220201-01 - Don't rollout if there is no value
			if (Source.Children.All(x => x.Value > 0))
			{
				for (int i = 0; i < Source.Children.Count; i++)
				{
					Prototype protoChild = Source.Children[i];
					if (protoChild.Value > 0)
					{
						lstBaseNode.Add(new GetExpectationsNode(Source, i));
						lstBaseNode.Last().Value = protoChild.Value;
					}
				}
			}

			return lstBaseNode;
		}

		public override ControlFlag OnResultReceived(Result result)
		{
			if (result is RangeResult)
			{
				RangeResult rangeResult = result as RangeResult;

				Prototype protoSequence = Reference.ShallowClone();
				if (Prototypes.TypeOf(Reference, Sequence.Prototype))
					protoSequence = Sequences.GetSequence(Reference.PrototypeID);

				protoSequence.Children.AddRange(rangeResult.Result.Children.GetRange(rangeResult.RangeStart, rangeResult.RangeLength));

				rangeResult.Result.Children.RemoveRange(rangeResult.RangeStart, rangeResult.RangeLength);
				rangeResult.Result.Children.Insert(rangeResult.RangeStart, protoSequence);

				return new SingleResult(rangeResult.Result);

			}

			return result;
		}

	}
}
