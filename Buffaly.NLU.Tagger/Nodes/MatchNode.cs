using Ontology.BaseTypes;
using Ontology;
using System.Text;
using Buffaly.NLU.Tagger.RolloutController;

namespace Buffaly.NLU.Tagger.Nodes
{
	public class MatchNode : BaseFunctionNode
	{
		public static bool AllowHypothesizedMatch = false;
		public static bool AllowActions = true;
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder("MatchNode - ");
			sb.Append(PrototypeLogging.ToChildString(Source));
			sb.Append(" / ");
			sb.Append(PrototypeLogging.ToChildString(Reference));
			return sb.ToString();
		}
		public MatchNode(Prototype protoSource, Prototype protoReference) : base(protoSource, protoReference)
		{
		}

		public override List<BaseNode> GeneratePossibilities()
		{
			List<BaseNode> lstBaseNode = new List<BaseNode>();

			if (IsLeafType(this.Reference))
			{
				if (this.Source.Children.Count == 1)
					lstBaseNode.Add(new ExactMatchNode(this.Source.Children.First(), Reference));

				else if (this.Source.Children.All(x => Prototypes.TypeOf(x, Lexeme.Prototype)))
					lstBaseNode.Add(new MultiTokenLexemeMatchNode(Source, Reference));
				
			}

			else if (this.Source.Children.Count == 1)
			{
				Prototype protoChild = this.Source.Children[0];

				if (Prototypes.TypeOf(protoChild, Lexeme.Prototype))
					lstBaseNode.Add(new LexemePrototypeMatchNode(protoChild, Reference));

				lstBaseNode.Add(new ExactMatchNode(this.Source.Children.First(), Reference));
			}

			else
			{
				int iLexemes = this.Source.Children.Count(x => Prototypes.TypeOf(x, Lexeme.Prototype));

				if (iLexemes == this.Source.Children.Count)
					lstBaseNode.Add(new MultiTokenLexemePrototypeMatchNode(Source, Reference));				

				if (iLexemes > 0)
				{
					//Workbackwards
					//RolloutNode
				}
			}

			//lstBaseNode.Add(action node)

			return lstBaseNode;			
		}

		public static bool IsLeafType(Prototype prototype)
		{
			if (Prototypes.TypeOf(prototype, Lexeme.Prototype))
				return true;

			if (NativeValuePrototypes.IsBaseType(prototype.PrototypeID))
				return true;

			if (Prototypes.TypeOf(prototype, System_String.Prototype))
				return true;

			return false;
		}
	}
}
