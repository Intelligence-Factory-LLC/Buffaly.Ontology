using BasicUtilities.Collections;
using Ontology;
using Ontology.BaseTypes;
using System.Text;
using Buffaly.NLU.Tagger.RolloutController;
using Ontology.Simulation;

namespace Buffaly.NLU.Tagger.Nodes
{
	public class ExactMatchNode : BaseTaggingNode
	{
		public ExactMatchNode(Prototype protoSource, Prototype protoReference)
		{
			Source = protoSource;
			Reference = protoReference;

			if (EnableCaching)
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(PrototypeLogging.ToChildString(protoSource));
				sb.Append(" ");
				sb.Append(PrototypeLogging.ToChildString(protoReference));

				CacheKey = sb.ToString();

				ControlFlag result = null;

				if (Cache.TryGetValue(CacheKey, out result))
				{
					if (result is Continue)
						this.Value = 0;

					else if (result is Result)
					{
						//N20220119-02 - The cached result may not have all the properties
						if (Prototypes.AreShallowEqual((result as SingleResult).Result, protoSource))
							this.CachedResult = new SingleResult(protoSource);
						else
							this.CachedResult = result;
					}
				}
			}

		}

		public static bool EnableCaching = false; //N20220202-01 there is a bug 
		private static Map<string, ControlFlag> Cache = new Map<string, ControlFlag>();
		private string CacheKey = null;
		private ControlFlag CachedResult = null;

		static public void ResetCache()
		{
			Cache = new Map<string, ControlFlag>();
		}
		public override ControlFlag Evaluate()
		{
			if (this.CachedResult != null)
				return this.CachedResult.Clone();

			ControlFlag result = new Continue();

			if (Prototypes.AreShallowEqual(this.Source, this.Reference))
				result = new SingleResult(this.Source);

			else if (Prototypes.TypeOf(this.Source, this.Reference))
			{
				if (this.Source is QuantumPrototype qp && !qp.Collapsed)
				{
					qp.Collapse(this.Reference);
				}

				result = new SingleResult(this.Source);
			}

			else if (Prototypes.TypeOf(this.Reference, System_String.Prototype) && Prototypes.TypeOf(this.Source, Lexeme.Prototype))
			{
				Prototype protoString = Lexeme.GetStringValue(this.Source);
				if (Prototypes.TypeOf(protoString, this.Reference))
					result = new SingleResult(protoString);

			}

			if (EnableCaching)
				Cache[CacheKey] = result.Clone();

			return result;
		}
	}
}
