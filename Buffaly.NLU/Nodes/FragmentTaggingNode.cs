//added
using Buffaly.NLU.Tagger.RolloutController;
using Ontology;
using Buffaly.NLU.Tagger;
using System.Text;
using Buffaly.NLU.Tagger.Nodes;

namespace Buffaly.NLU.Nodes
{
	public class FragmentTaggingNode : BaseFunctionNode
	{

		public delegate bool ExitConditionDelegate(Prototype protoSequence);
		public ExitConditionDelegate ExitCondition = null;

		public FragmentTaggingNode(Prototype protoSource)
		{
			Source = new Collection();

			foreach (Prototype child in protoSource.Children)
			{
				this.Source.Children.Add(child);
			}

		}

		public FragmentTaggingNode()
		{

		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder("FragmentTaggingNode - ");
			sb.Append(PrototypeLogging.ToChildString(Source));
			return sb.ToString();
		}

		public override List<BaseNode> GeneratePossibilities()
		{
			List<BaseNode> lstBaseNode = new List<BaseNode>();

			if (Source.Children.All(x => x.Value > 0))
			{
				lstBaseNode.Add(new RolloutNode(Source));
			}

			return lstBaseNode;
		}

		public override ControlFlag OnResultReceived(Result result)
		{
			//Exit conditions 
			if (result is SingleResult)
			{
				SingleResult singleResult = result as SingleResult;
				if (singleResult.Result.PrototypeID == Ontology.Collection.PrototypeID)
				{
					Prototype protoResult = singleResult.Result;

					if (null != ExitCondition)
					{
						if (this.ExitCondition(protoResult))
							return singleResult;
					}
					else
					{
						if (protoResult.Children.All(x =>
							Prototypes.TypeOf(x, TemporaryPrototypes.GetTemporaryPrototypeOrNull("Action")) &&

							//N20220405-03
							!Prototypes.TypeOf(x, TemporaryPrototypes.GetTemporaryPrototypeOrNull("DependentPhrase"))
						))
							return singleResult;
					}
				}
				else
					throw new Exception("Unexpected: Tagging should always return a Collection");

				//Check that we don't use the same possibility again. The check can't be done inside
				//the AddPossibility method because it is in BaseNode. I could add an == operator later 
				//to make this easier
				if (!this.Possibilities.Any(x => PrototypeGraphs.AreEquivalentCircular((x as BaseFunctionNode).Source, singleResult.Result)))
				{
					if (singleResult.Result.Children.All(x => x.Value > 0))
					{
						this.AddPossibility(new RolloutNode(singleResult.Result));
					}
				}
				else

				{

				}

				return new Continue();

			}

			return result;
		}

	}
}
