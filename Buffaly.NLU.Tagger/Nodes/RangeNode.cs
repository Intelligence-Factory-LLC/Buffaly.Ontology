using Buffaly.NLU.Tagger.RolloutController;
using Ontology;
using System.Text;
using BasicUtilities.Collections;
using BasicUtilities;

namespace Buffaly.NLU.Tagger.Nodes
{
	public class RangeResult : Result
	{
		public Prototype Result;
		public int RangeStart;
		public int RangeLength;

		public override ControlFlag Clone()
		{
			RangeResult rangeResult = new RangeResult();
			rangeResult.Result = this.Result.Clone();
			rangeResult.RangeStart = this.RangeStart;
			rangeResult.RangeLength = this.RangeLength;
			return rangeResult;
		}
	}

	public class RangeNode : BaseFunctionNode
	{
		//N20220126-02
		public static bool AllowRanges = false;

		//These constructors should only be used for testing
		public RangeNode(Prototype protoSource, Prototype protoReference) : base(protoSource, protoReference)
		{

		}

		public RangeNode(Prototype protoSource, Prototype protoReference, bool bAllowPreceeding, bool bAllowTrailing)
		{
			Source = protoSource;
			Reference = protoReference;
			AllowPreceeding = bAllowPreceeding;
			AllowTrailing = bAllowTrailing;
		}


		public RangeNode(BaseFunctionNode node) : base(node)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(PrototypeLogging.ToChildString(Source));
			sb.Append(" ");
			sb.Append(PrototypeLogging.ToChildString(Reference));
			sb.Append(" ");
			sb.Append(AllowPreceeding).Append(AllowTrailing);

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
			StringBuilder sb = new StringBuilder("RangeNode - ");

			sb.Append(PrototypeLogging.ToChildString(Source));


			sb.Append(" / ");

			if (AllowPreceeding)
				sb.Append("{");

			sb.Append(PrototypeLogging.ToChildString(Reference));

			if (AllowTrailing)
				sb.Append("}");

			return sb.ToString();
		}

		public override List<BaseNode> GeneratePossibilities()
		{
			List<BaseNode> lstBaseNode = new List<BaseNode>();

			if (Reference.Children.Count != 1)
				throw new Exception("Unexpected: reference should contain 1 child");

			Prototype protoReference = Reference.Children.First();

			if (!AllowPreceeding && !AllowTrailing)
				lstBaseNode.Add(new MatchNode(Source, protoReference));

			else if (!AllowPreceeding && AllowTrailing)
			{

				for (int i = 0; i < Source.Children.Count; i++)
				{
					int iRangeStart = 0;
					int iRangeLength = Source.Children.Count - i;

					if (!AllowRanges)
					{
						if (iRangeLength != 1 && iRangeLength != Source.Children.Count)
							continue;
					}

					MatchNode node = new MatchNode(Collection.GetRange(Source, iRangeStart, iRangeLength), protoReference);
					node.OnResultReceivedCallback = x => OnRangedResultReceived(x, iRangeStart, iRangeLength);
					lstBaseNode.Add(node);
				}

			}

			else if (AllowPreceeding && !AllowTrailing)
			{
				for (int i = 0; i < Source.Children.Count; i++)
				{
					int iRangeStart = i;
					int iRangeLength = Source.Children.Count - i;

					if (!AllowRanges)
					{
						if (iRangeLength != 1 && iRangeLength != Source.Children.Count)
							continue;
					}

					MatchNode node = new MatchNode(Collection.GetRange(Source, iRangeStart, iRangeLength), protoReference);
					node.OnResultReceivedCallback = x => OnRangedResultReceived(x, iRangeStart, iRangeLength);

					lstBaseNode.Add(node);
				}
			}

			else
				throw new Exception("Unexpected: too many possibilities");

			return lstBaseNode;
		}

		private ControlFlag OnRangedResultReceived(ControlFlag flag, int iRangeStart, int iRangeLength)
		{
			//N20210522-01 - Need to keep the original collection but identify the matching piece of the range
			if (flag is SingleResult)
			{
				SingleResult singleResult = flag as SingleResult;
				RangeResult result = new RangeResult();

				result.Result = new Collection();
				result.Result.Children.AddRange(Collection.GetRange(this.Source, 0, iRangeStart).Children);

				int iNewLength = iRangeLength;
				if (Prototypes.TypeOf(singleResult.Result, Ontology.Collection.Prototype))
				{
					result.Result.Children.AddRange(singleResult.Result.Children);
					iNewLength = singleResult.Result.Children.Count;
				}
				else
				{
					result.Result.Children.Add(singleResult.Result);
					iNewLength = 1;
				}

				result.Result.Children.AddRange(Collection.GetRange(this.Source, iRangeStart + iRangeLength, this.Source.Children.Count - iRangeStart - iRangeLength).Children);
				result.RangeStart = iRangeStart;
				result.RangeLength = iNewLength;

				return result;
			}

			return flag;
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
