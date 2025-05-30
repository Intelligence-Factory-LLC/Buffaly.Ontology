using Ontology;
using Buffaly.NLU.Tagger.RolloutController;

namespace Buffaly.NLU.Tagger.Nodes
{
	public class MultiTokenLexemePrototypeMatchNode : BaseTaggingNode
	{
		public MultiTokenLexemePrototypeMatchNode(Prototype protoSource, Prototype protoReference) : base(protoSource, protoReference)
		{
		}

		public MultiTokenLexemePrototypeMatchNode(Prototype protoSource, Prototype protoReference, bool bAllowPreceeding, bool bAllowTrailing)
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
					Prototype protoLexeme = prototype.Properties[Lexeme.PrototypeID];

					throw new NotImplementedException();

					//LexemePrototypesDataTable dtLexemePrototypes = LexemePrototypes.GetLexemePrototypesByPrototypeID(protoLexeme.PrototypeID);

					//foreach (LexemePrototypesRow rowLexemePrototype in dtLexemePrototypes)
					//{
					//	Prototype protoRelated = Prototypes.GetAsPrototype(rowLexemePrototype.RelatedPrototype.PrototypeID);
					//	if (Prototypes.TypeOf(protoRelated, Reference))
					//		lstResults.Add(protoRelated);
					//}
				}
			}

			if (lstResults.Count == 0)
				return new Continue();

			if (lstResults.Count == 1)
				return new SingleResult(lstResults.First());

			throw new NotImplementedException();
		}
	}
}
