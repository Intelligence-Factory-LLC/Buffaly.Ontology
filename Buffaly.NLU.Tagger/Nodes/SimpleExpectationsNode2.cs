//added
using Ontology;
using Buffaly.NLU.Tagger.RolloutController;

namespace Buffaly.NLU.Tagger.Nodes
{
	class SimpleExpectationsNode2 : BaseFunctionNode 
	{
		private Prototype Matched = null; 
		public SimpleExpectationsNode2(Prototype protoSource, Prototype protoReference) : base(protoSource, protoReference)
		{

		}

		public override List<BaseNode> GeneratePossibilities()
		{
			if (Source.Children.Count == 0 || Source.Children.Count != Reference.Children.Count)
				throw new Exception("SimpleExpectationsNode should be called only on same sized, non-empty sequences");

			Matched = Reference.ShallowClone();

			if (Prototypes.TypeOf(Reference, Sequence.Prototype))
				Matched = Sequences.GetSequence(Reference.PrototypeID);
			
			for (int i = 0; i < Reference.Children.Count; i++)
			{
				Matched.Children.Add(null);
			}

			List<BaseNode> lstBaseNode = new List<BaseNode>();

			for (int i = 0; i < Source.Children.Count; i++)
			{
				Prototype protoReference = Reference.Children[i];
				Prototype protoSource = Source.Children[i];
				if (Prototypes.TypeOf(protoSource, protoReference))
					Matched.Children[i] = protoSource;

				//N20210703-01 - For complex types use a match node and combine the results back together
				else
				{
					MatchNode node = new MatchNode(Collection.GetRange(Source, i, 1), protoReference);
					node.OnResultReceivedCallback = x => OnMatchResultReceived(x, node);
					lstBaseNode.Add(node);
				}
			}

			if (Matched.Children.All(x => x != null))
			{
				//Have to return a node so this will call and return Evaluate
				lstBaseNode.Add(new BaseNode());
			}

			return lstBaseNode;
		}

		public override ControlFlag EvaluateNext(BaseNode nodeChild)
		{
			if (this.Matched.Children.All(x => x != null))
			{
				//N20220121-03 Don't come back to this node
				this.Value = 0;
				this.UpdateExplorationValues();
				return new SingleResult(Matched);
			}

			return new Continue();
		}


		public ControlFlag OnMatchResultReceived(Result result, MatchNode matchNode)
		{
			if (result is SingleResult)
			{
				SingleResult singleResult = result as SingleResult;

				for (int i = 0; i < Reference.Children.Count; i++)
				{
					Prototype protoReference = Reference.Children[i];
					Prototype protoSource = Source.Children[i];

					if (Prototypes.AreShallowEqual(protoSource, matchNode.Source.Children.First()) && Prototypes.AreShallowEqual(protoReference, matchNode.Reference))
					{
						Matched.Children[i] = singleResult.Result;
					}
				}

				if (!Matched.Children.Any(x => x == null))
					return new SingleResult(Matched);

			}

			return null;
		}
	}
}
