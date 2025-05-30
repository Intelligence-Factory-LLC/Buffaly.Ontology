using Ontology;
using Buffaly.NLU.Tagger.RolloutController;


namespace Buffaly.NLU.Tagger.Nodes
{
	public class LexemePrototypeMatchNode : BaseTaggingNode
	{
		public LexemePrototypeMatchNode(Prototype protoSource, Prototype protoReference) : base(protoSource, protoReference)
		{
		}

		public LexemePrototypeMatchNode(Prototype protoSource, Prototype protoReference, bool bAllowPreceeding, bool bAllowTrailing)
			: base(protoSource, protoReference, bAllowPreceeding, bAllowTrailing)
		{

		}

		public override ControlFlag Evaluate()
		{
			List<Prototype> lstResults = new List<Prototype>();

			if (Prototypes.TypeOf(Source, Lexeme.Prototype))
			{
				lstResults = Lexemes.GetAsMultiple(Source, Reference);
			}

			if (lstResults.Count == 0)
				return new Continue();

			if (lstResults.Count == 1)
				return new SingleResult(lstResults.First());

			//N20211201-01 - Return a possibilities collection
			Prototype possibilities = Ontology.Simulation.Possibilities.Prototype.Clone();
			possibilities.Children.AddRange(lstResults);
			return new SingleResult(possibilities);
		}
	}
}
