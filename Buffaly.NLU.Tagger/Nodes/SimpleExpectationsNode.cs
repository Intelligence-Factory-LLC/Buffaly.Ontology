//added
using Ontology;
using Buffaly.NLU.Tagger.RolloutController;

namespace Buffaly.NLU.Tagger.Nodes
{
	class SimpleExpectationsNode : BaseTaggingNode 
	{
		public SimpleExpectationsNode(Prototype protoSource, Prototype protoReference) : base(protoSource, protoReference)
		{ 

		}

		public override ControlFlag Evaluate()
		{
			if (Source.Children.Count == 0 || Source.Children.Count != Reference.Children.Count)
				throw new Exception("SimpleExpectationsNode should be called only on same sized, non-empty sequences");

			Prototype protoSequence = Reference.ShallowClone();
			if (Prototypes.TypeOf(Reference, Sequence.Prototype))
				protoSequence = Sequences.GetSequence(Reference.PrototypeID);

			for (int i = 0; i < Source.Children.Count; i++)
			{
				Prototype protoReference = Reference.Children[i];
				Prototype protoSource = Source.Children[i];
				if (Prototypes.TypeOf(protoSource, protoReference))
					protoSequence.Children.Add(protoSource);

				//N20210518-01 - Merged with RollForwardNode
				else if (Prototypes.TypeOf(protoSource, Lexeme.Prototype) && !Prototypes.TypeOf(protoReference, Lexeme.Prototype))
				{
					List<Prototype> lstLexemePrototypes = Lexemes.GetAsMultiple(protoSource, protoReference);

					if (lstLexemePrototypes.Count == 0)
						return new Continue();

					else if (lstLexemePrototypes.Count == 1)
						protoSequence.Children.Add(lstLexemePrototypes.First());

					else if (lstLexemePrototypes.Count > 1)
						throw new NotImplementedException();
				}

				else
					return new Continue();					
			}

			return new SingleResult(protoSequence);
		}
	}
}
