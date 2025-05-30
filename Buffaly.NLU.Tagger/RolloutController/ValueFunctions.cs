using Ontology;
using Buffaly.NLU.Tagger.Nodes;

namespace Buffaly.NLU.Tagger.RolloutController
{
	public class ValueFunctions
	{
		public static double GetFragmentValue(Prototype prototype)
		{
			return GetFragmentValue1f(prototype);
		}

		//N20190905-01
		public static double GetFragmentValue1d(Prototype prototype)
		{
			//Simple value function to prefer more tagged fragments first
			return 1.0 / prototype.Children.Count;
		}

		//N20190905-01
		public static double GetFragmentValue1c(Prototype prototype)
		{
			//Simple value function to prefer more tagged fragments first
			int iTagged = prototype.Children.Count(x => !MatchNode.IsLeafType(x));
			return ((double)(iTagged) + 1) / prototype.Children.Count;
		}

		//N20211009-01
		public static double GetFragmentValue1e(Prototype prototype)
		{
			//The idea here is to prefer short segments (more tagged), but within 
			//each shorter segment prefer those with less leaves
			
			//Most of the value is the length
			double dValue = 1.0 / prototype.Children.Count;
			double dValueNext = 1.0 / Math.Max(prototype.Children.Count - 1, 1);

			//Now add a small factor for the number tagged vs not tagged
			int iTagged = prototype.Children.Count(x => !MatchNode.IsLeafType(x));
			dValue += (dValueNext - dValue) * iTagged;  // / prototype.Children.Count; - this didn't work well

			return dValue;
		}

		//N20220501-04
		public static double GetFragmentValue1f(Prototype prototype)
		{
			//Give even greater weight to shorter fragments

			//Most of the value is the length
			double dValue = 1.0 / (Math.Pow(prototype.Children.Count, 2));
			double dValueNext = 1.0 / Math.Max(Math.Pow(prototype.Children.Count - 1, 2), 1);

			////Now add a small factor for the number tagged vs not tagged
			//int iTagged = prototype.Children.Count(x => !MatchNode.IsLeafType(x));
			//dValue += (dValueNext - dValue) * iTagged;  // / prototype.Children.Count; - this didn't work well

			return dValue;
		}
	}
}
