using System.Text;
using Ontology;
using Buffaly.NLU.Tagger.RolloutController;
using BasicUtilities.Collections;
using BasicUtilities;


namespace Buffaly.NLU.Tagger.Nodes
{
	public class FollowExpectationsNode : BaseFunctionNode 
	{
		public delegate Prototype OnUnderstandDelegate(Prototype protoSequence);
		public static OnUnderstandDelegate OnUnderstand = null;

		public int SourceIndex = 0;
		public int ReferenceIndex = 0;
		public FollowExpectationsNode(Prototype protoSource, Prototype protoReference, int iSourceIndex, int iReferenceIndex) : base(protoSource, protoReference)
		{
			ReferenceIndex = iReferenceIndex;
			SourceIndex = iSourceIndex;

			StringBuilder sb = new StringBuilder();
			if (iReferenceIndex == 0)
			{
				sb.Append(PrototypeLogging.ToChildString(new Collection(protoSource.Children.GetRange(SourceIndex, protoSource.Children.Count - SourceIndex))));
			}
			else if (ReferenceIndex == protoReference.Children.Count - 1)
			{
				sb.Append(PrototypeLogging.ToChildString(new Collection(protoSource.Children.GetRange(0, SourceIndex + 1))));
			}
			else
			{
				//Not a determinate
				return;
			}

			sb.Append(" ");
			sb.Append(protoReference.PrototypeName + ">" + PrototypeLogging.ToChildString(protoReference));

			CacheKey = sb.ToString();

			ControlFlag result = null;

			if (Cache.TryGetValue(CacheKey, out result))
			{
				if (result is Continue)
					this.Value = 0;
			}
		}

		private static Map<string, ControlFlag> Cache = new Map<string, ControlFlag>();
		private string CacheKey = null;
		private ControlFlag CachedResult = null;

		static public void ResetCache()
		{
			Cache = new Map<string, ControlFlag>();
		}
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder("FollowExpectationsNode - ");
			
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
			sb.Append(Reference.PrototypeName).Append(">");
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
			List<BaseNode> lstBaseNode = new List<BaseNode>();

			if (Reference.TypeOf("BagOfFeatures"))
			{
				lstBaseNode.Add(new LeafBagOfFeaturesNode(Source, Reference, SourceIndex, ReferenceIndex));
			}

			//If the Reference is like a multi-token lexeme, this should be an easy match
			//The already matched node is exempt from the leaf check
			else if ((Reference.Children.All(x => MatchNode.IsLeafType(x) || x == Reference.Children[ReferenceIndex]))
				|| !RangeNode.AllowRanges)
			{
				lstBaseNode.Add(new LeafSequenceMatchNode(Source, Reference, SourceIndex, ReferenceIndex));
			}

			else
			{
				DivisionNode division = new DivisionNode(Source, Reference, SourceIndex, ReferenceIndex);

				division.Left.AllowPreceeding = AllowPreceeding;
				division.Left.AllowTrailing = false;
				division.Right.AllowPreceeding = false;
				division.Right.AllowTrailing = AllowTrailing;

				if (ReferenceIndex == 0)
				{
					division.Right.AllowPreceeding = false;
					division.Right.AllowTrailing = AllowTrailing;
				}

				if (ReferenceIndex == Reference.Children.Count - 1)
				{
					division.Left.AllowPreceeding = AllowPreceeding;
					division.Left.AllowTrailing = false;
				}


				division.Right.Source.Children = Source.Children.GetRange(SourceIndex + 1, Source.Children.Count - SourceIndex - 1);
				division.Right.Reference.Children = Reference.Children.GetRange(ReferenceIndex + 1, Reference.Children.Count - ReferenceIndex - 1);

				division.Left.Source.Children = Source.Children.GetRange(0, SourceIndex);
				division.Left.Reference.Children = Reference.Children.GetRange(0, ReferenceIndex);

				lstBaseNode.Add(division);
			}

			return lstBaseNode;
		}		

		public override ControlFlag OnResultReceived(Result result)
		{
			if (result is RangeResult)
			{
				RangeResult rangeResult = result as RangeResult;
			
				//Changed this from shallow clone to a Clone + Clear so it 
				//preserves any properties on the sequence
				Prototype protoSequence = Reference.Clone();
				if (Prototypes.TypeOf(Reference, Sequence.Prototype))
					protoSequence = Sequences.GetSequence(Reference.PrototypeID).Clone();

				protoSequence.Children.Clear();
				protoSequence.Children.AddRange(rangeResult.Result.Children.GetRange(rangeResult.RangeStart, rangeResult.RangeLength));

				if (null != OnUnderstand)
				{
					protoSequence = OnUnderstand(protoSequence);
				}

				if (null == protoSequence)
					return null;
				
				protoSequence.Value = 1;

				rangeResult.Result.Children.RemoveRange(rangeResult.RangeStart, rangeResult.RangeLength);

				//Understand can return a collection as a result
				if (Prototypes.TypeOf(protoSequence, Ontology.Collection.Prototype))
					rangeResult.Result.Children.InsertRange(rangeResult.RangeStart, protoSequence.Children);
				else
					rangeResult.Result.Children.Insert(rangeResult.RangeStart, protoSequence);

				return new SingleResult(rangeResult.Result);

			}

			else if (result is SingleResult)
				return result;

			return null;
		}

		public override void OnNoMorePossibilities()
		{
			if (!StringUtil.IsEmpty(CacheKey))
			{
				Cache[CacheKey] = new Continue();
			}
		}
	}
}
