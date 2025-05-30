using Buffaly.NLU.Tagger.RolloutController;
using Ontology;
using System.Text;

namespace Buffaly.NLU.Tagger.Nodes
{
	public class FindDivisionNode : BaseFunctionNode
	{
		public int ReferenceIndex = 0; 

		public FindDivisionNode(Prototype source, Prototype reference, int index) : base(source, reference)
		{ 
			this.ReferenceIndex = index;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder("FindDivisionNode - ");
			sb.Append(PrototypeLogging.ToChildString(Source));



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

			if (Reference.Children.Count < 2)
				throw new Exception("Unexpected: Can't divide a reference with less than 2 items");

			Prototype protoReference = this.Reference.Children[ReferenceIndex];

			for (int iSourceIndex = 0; iSourceIndex < this.Source.Children.Count; iSourceIndex++)
			{
				Prototype protoSource = this.Source.Children[iSourceIndex];
				
				if (Prototypes.AreShallowEqual(protoSource, protoReference))
				{
					DivisionNode division = new DivisionNode(Source, Reference, iSourceIndex, ReferenceIndex);

					division.Left.Source.Children = Source.Children.GetRange(0, iSourceIndex);
					division.Right.Source.Children = Source.Children.GetRange(iSourceIndex + 1, this.Source.Children.Count - iSourceIndex - 1);

					division.Left.Reference.Children = Reference.Children.GetRange(0, ReferenceIndex);
					division.Right.Reference.Children = Reference.Children.GetRange(ReferenceIndex + 1, Reference.Children.Count - ReferenceIndex - 1);

					if (division.Left.Source.Children.Count == 0 && division.Left.Reference.Children.Count > 0)
						continue;

					if (division.Right.Source.Children.Count == 0 && division.Right.Reference.Children.Count > 0)
						continue;

					if (division.Left.Source.Children.Count > 0 && division.Left.Reference.Children.Count == 0)
						continue;

					if (division.Right.Source.Children.Count > 0 && division.Right.Reference.Children.Count == 0)
						continue;

					//todo: If the source is less than the reference on either side it isn't valid

					lstResults.Add(division);
				}
			}

			return lstResults;
		}
	}
}
