using System.Text;
using Ontology;
using Buffaly.NLU.Tagger.RolloutController;
using BasicUtilities.Collections;
using Ontology.Simulation;

namespace Buffaly.NLU.Tagger.Nodes
{
	public class LeafBagOfFeaturesNode : BaseTaggingNode 
	{
		public int SourceIndex = 0;
		public int ReferenceIndex = 0;
		public LeafBagOfFeaturesNode(Prototype protoSource, Prototype protoReference, int iSourceIndex, int iReferenceIndex) : base(protoSource, protoReference)
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
		private bool ? CachedResult = null;
		static public bool AllowCaching = true;

		static public void ResetCache()
		{
			Cache = new Map<string, bool>();
		}
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder("LeafBagOfFeaturesNode - ");
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
			int iLength = Reference.Children.Count;

			if (!bAlreadyMatched)
			{
				List<Prototype> lstRange = Source.Children.GetRange(iSourceStart, iLength);
				List<Prototype> lstFeatures = BagOfFeatures.GetFeatures(new Collection(lstRange));
				Set<int> setPrototypes = TemporarySequences.GetPossiblePrototypes(lstFeatures);

				for (int i = 0; i < Reference.Children.Count; i++)
				{
					Prototype protoReference = Reference.Children[i];
					if (!setPrototypes.Contains(protoReference.PrototypeID))
					{
						Cache[CacheKey] = false;
						return new Continue();
					}
				}
			}

			RangeResult rangeResult = new RangeResult();
			rangeResult.Result = Source.Clone();

			//Check for any QuantumPrototypes that need to be collapsed (after cloning)
			//in the Bag of Features these aren't necessarily in order 
			for (int i = 0; i < Reference.Children.Count; i++)
			{
				Prototype protoSource = rangeResult.Result.Children[iSourceStart + i];
				if (protoSource is QuantumPrototype qp && !qp.Collapsed)
				{
					List<Prototype> lstPossible = Reference.Children.Where(x => protoSource.TypeOf(x)).ToList();
					if (lstPossible.Count != 1) //don't collapse 
						continue;

					qp.Collapse(lstPossible.First());
				}
			}

			rangeResult.RangeStart = iSourceStart;
			rangeResult.RangeLength = Reference.Children.Count;

			Cache[CacheKey] = true;

			return rangeResult;
		}
	}
}
