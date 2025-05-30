using System.Text;
using Ontology;
using Buffaly.NLU.Tagger.RolloutController;
using BasicUtilities.Collections;
using Ontology.Simulation;

namespace Buffaly.NLU.Tagger.Nodes
{
	public class LeafSequenceMatchNode : BaseTaggingNode 
	{
		public int SourceIndex = 0;
		public int ReferenceIndex = 0;
		public LeafSequenceMatchNode(Prototype protoSource, Prototype protoReference, int iSourceIndex, int iReferenceIndex) : base(protoSource, protoReference)
		{
			ReferenceIndex = iReferenceIndex;
			SourceIndex = iSourceIndex;

			StringBuilder sb = new StringBuilder();
			sb.Append(PrototypeLogging.ToChildString(new Collection(protoSource.Children.GetRange(SourceIndex - ReferenceIndex, protoReference.Children.Count))));
			sb.Append(" ");
			sb.Append(PrototypeLogging.ToChildString(protoReference));

			CacheKey = sb.ToString();

			bool result = false;

			if (AllowCaching && Cache.TryGetValue(CacheKey, out result))
			{
				if (!result)
					this.Value = 0;

				this.CachedResult = result;
			}
		}

		private static Map<string, bool> Cache = new Map<string, bool>();
		private string CacheKey = null;
		private bool? CachedResult = null;

		static public bool AllowCaching = true;

		static public void ResetCache()
		{
			Cache = new Map<string, bool>();
		}
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder("LeafSequenceMatchNode - ");
			for (int i = 0; i < Source.Children.Count; i++)
			{
				if (i > 0)
					sb.Append(",");

				if (i == this.SourceIndex)
					sb.Append("[");

				sb.Append(PrototypeLogging.ToChildString(Source.Children[i]));

				if (i == this.SourceIndex)
					sb.Append("]");
			}

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
		public override ControlFlag Evaluate()
		{
			bool bAlreadyMatched = false;
			if (this.CachedResult != null)
			{
				if (this.CachedResult == false)
					return new Continue();

				bAlreadyMatched = true;
			}

			int iSourceStart = SourceIndex - ReferenceIndex;

			if (!bAlreadyMatched)
			{

				for (int i = 0; i < Reference.Children.Count; i++)
				{
					Prototype protoSource = Source.Children[iSourceStart + i];

					Prototype protoReference = Reference.Children[i];
					if (!Prototypes.TypeOf(protoSource, protoReference))
					{
						Continue cont = new Continue();
						Cache[CacheKey] = false;
						return cont;
					}

				}
			}

			RangeResult rangeResult = new RangeResult();
			rangeResult.Result = Source.Clone();

			//Check for any QuantumPrototypes that need to be collapsed (after cloning)
			for (int i = 0; i < Reference.Children.Count; i++)
			{
				Prototype protoSource = rangeResult.Result.Children[iSourceStart + i];
				if (protoSource is QuantumPrototype qp && !qp.Collapsed)
				{
					Prototype protoReference = Reference.Children[i];
					qp.Collapse(protoReference);
				}
			}

			rangeResult.RangeStart = iSourceStart;
			rangeResult.RangeLength = Reference.Children.Count;

			Cache[CacheKey] = true;

			return rangeResult;
		}
	}
}
