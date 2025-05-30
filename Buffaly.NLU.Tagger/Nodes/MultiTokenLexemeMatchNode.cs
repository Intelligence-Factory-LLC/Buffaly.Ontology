using Ontology;
using Buffaly.NLU.Tagger.RolloutController;

namespace Buffaly.NLU.Tagger.Nodes
{
	public class MultiTokenLexemeMatchNode : BaseTaggingNode
	{
		public MultiTokenLexemeMatchNode(Prototype protoSource, Prototype protoReference) : base(protoSource, protoReference)
		{
		}

		public MultiTokenLexemeMatchNode(Prototype protoSource, Prototype protoReference, bool bAllowPreceeding, bool bAllowTrailing)
			: base(protoSource, protoReference, bAllowPreceeding, bAllowTrailing)
		{

		}

		public override ControlFlag Evaluate()
		{
			List<Prototype> lstResults = new List<Prototype>();

			if (Source.Children.Count > 1)
			{
				//N20210509-01
				string strHash = PrototypeGraphs.GetHash(Source);
				Prototype rowPrototype = Prototypes.GetPrototypeByPrototypeName("Lexeme." + strHash);
				if (null != rowPrototype)
				{
					Prototype prototype = rowPrototype;

					//TODO: I don't see how this would work
					Prototype protoLexeme = prototype.Properties[Lexeme.PrototypeID];

					if (Prototypes.AreShallowEqual(protoLexeme, Reference))
						return new SingleResult(protoLexeme);
				}
			}

			return new Continue();
		}
	}
}
