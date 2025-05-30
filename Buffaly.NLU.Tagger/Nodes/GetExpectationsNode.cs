//added
using Buffaly.NLU.Tagger.RolloutController;
using Ontology;
using System.Text;
using BasicUtilities.Collections;
using Ontology.Simulation;

namespace Buffaly.NLU.Tagger.Nodes
{
	public class Expectation
	{
		public Prototype Prototype;
		public int Index;
		public double Value;
		public string PrototypeName;

		public override string ToString()
		{
			return "Tagging.Expectation[" + Prototype.ToString() + "[" + string.Join(",", Prototype.Children.Select(x => x.ToString())) + "]]";
		}
	}

	public enum Direction
	{
		Forward,
		Backward,
		Both
	}

	public class GetExpectationsNode : BaseFunctionNode
	{
		public int Index = 0;
		public static bool FilterPossiblePrototypes = true;
		public GetExpectationsNode(Prototype protoSource, int iIndex)
		{
			Index = iIndex;
			Source = protoSource;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder("GetExpectationsNode - ");
			for (int i = 0; i < Source.Children.Count; i++)
			{
				if (i > 0)
					sb.Append(",");

				if (i == this.Index)
					sb.Append("[");

				sb.Append(PrototypeLogging.ToChildString(Source.Children[i]));

				if (i == this.Index)
					sb.Append("]");
			}

			return sb.ToString();
		}

		public override List<BaseNode> GeneratePossibilities()
		{
			List<BaseNode> lstBaseNode = new List<BaseNode>();

			Direction direction = Direction.Both;
			if (Index == 0)
				direction = Direction.Forward;
			else if (Index == Source.Children.Count - 1)
				direction = Direction.Backward;

			Prototype protoSelected = Source.Children[Index];

			List<Expectation> lstExpectations = GetExpectations(protoSelected, direction, Dimensions.NL.PrototypeID);
	
			//N20190905-01
			int iMaxAfter = Source.Children.Count - Index;

			Set<int> setPrototypes = new Set<int>();

			//N20220201-01 - Only use sequences where it is possible to match all of the items in it
			bool bFilterPossiblePrototypes = FilterPossiblePrototypes;

			if (bFilterPossiblePrototypes)
				setPrototypes = TemporarySequences.GetPossiblePrototypes(this.Source.Children);

			foreach (Expectation expectation in lstExpectations)
			{
				int iAfter = expectation.Prototype.Children.Count - expectation.Index;

				if (expectation.Index <= Index && iAfter <= iMaxAfter)
				{
					if (bFilterPossiblePrototypes)
					{
						if (expectation.Prototype.Children.Any(x => !setPrototypes.Contains(x.PrototypeID)))
							continue;

						//Fast pass parser checking an exact length sequence
						if (RangeNode.AllowRanges == false)
						{
							int iStart = Index - expectation.Index;
							int iLength = expectation.Prototype.Children.Count;

							Prototype protoSub = new Collection(Source.Children.GetRange(iStart, iLength));
							Set<int> setSubPrototypes =TemporarySequences.GetPossiblePrototypes(protoSub.Children);

							if (expectation.Prototype.Children.Any(x => !setSubPrototypes.Contains(x.PrototypeID)))
								continue;
						}
					}

					FollowExpectationsNode node = new FollowExpectationsNode(Source, expectation.Prototype, Index, expectation.Index);

					if (0 != node.Value)
					{
						node.Value = expectation.Value;
						node.AllowPreceeding = (expectation.Index < Index);
						node.AllowTrailing = (iAfter < iMaxAfter);
						lstBaseNode.Add(node);
					}
				}
			}

			if (AlreadyLinkedSequenceMatchNode.IsEnabled && Index > 0)
			{
				Prototype protoRoot = Source.Children[Index - 1];
				Prototype protoLinked = AlreadyLinkedSequenceMatchNode.GetLastLinked(protoRoot);
				if (protoRoot != protoLinked)
				{					
					foreach (Expectation expectation in lstExpectations)
					{
						int iAfter = expectation.Prototype.Children.Count - expectation.Index;

						if (expectation.Index == 1 && iAfter <= iMaxAfter)
						{
							if (FilterPossiblePrototypes)
							{
								Collection colProjectedSource = new Collection();
								colProjectedSource.Add(protoLinked);
								colProjectedSource.Children.AddRange(Source.Children.GetRange(this.Index, expectation.Prototype.Children.Count - 1));

								Set<int> setSubPrototypes = TemporarySequences.GetPossiblePrototypes(colProjectedSource);

								if (expectation.Prototype.Children.Any(x => !setSubPrototypes.Contains(x.PrototypeID)))
									continue;
							}

							AlreadyLinkedSequenceMatchNode node = new AlreadyLinkedSequenceMatchNode(Source, expectation.Prototype, Index, expectation.Index);
							lstBaseNode.Add(node);
						}
					}

				}

			}
			
			return lstBaseNode;
		}

		static public double Threshold = 0.01;
		static public bool UpdatePredictiveValue = false;

		//N20241104-02 - Calculate the predictive values of the sequences as we pull them 
		static public bool UseDynamicPredictiveValues = false; 

		static public List<Expectation> GetExpectations(Prototype prototype, Direction direction, int iDimensionPrototypeID)
		{
			//From Tagger.Expectations.GetExpectations
			List<Expectation> lstExpectations = new List<Expectation>();

			if (prototype is QuantumPrototype qp && !qp.Collapsed)
			{
				foreach (Prototype proto in qp.PossiblePrototypes)
				{
					List<Expectation> lstExpectationsSub = GetExpectationsSingular(proto, direction, iDimensionPrototypeID);
					lstExpectations.AddRange(lstExpectationsSub);
				}
			}

			else
			{
				lstExpectations = GetExpectationsSingular(prototype, direction, iDimensionPrototypeID);
			}

			//N20190808-02 - Minor speedup. Note: this may be a problem if we ever add a sequence with "Lexeme" in it
			if (!Prototypes.TypeOf(prototype, Lexeme.Prototype))
			{

				//If we allow matches on the parents. 
				foreach (int parent in prototype.GetAllParents())
				{
					Prototype rowParent = Prototypes.GetPrototype(parent);
					//if (rowParent.PredictiveValue >= Threshold)             //shortcut so it doesn't go down low value paths (1)
					{
						List<Expectation> lstParentExpectations = GetExpectationsSingular(rowParent, direction, iDimensionPrototypeID);

						//This assumes that we multiply the value of the prototype parent by the value of the sequence
						//I don't know if that 
						for (int i = 0; i < lstParentExpectations.Count; i++)
						{
							Expectation parentExpectation = lstParentExpectations[i];

							//Do not degrade the value, see N20190111-01
							//parentExpectation.Value = rowPrototype.PredictiveValue * parentExpectation.Value;

							//We cannot have a threshold and update or the values will drop to 0 eventually
							if (UpdatePredictiveValue || parentExpectation.Value >= Threshold)
								lstExpectations.Add(parentExpectation);
						}
					}
				}
			}

			//N20190823-02 - Longest first if the same value
			return lstExpectations.OrderByDescending(x => x.Value).ThenByDescending(x => x.Prototype.Children.Count).ToList();
		}

		static public List<Expectation> GetExpectationsSingular(Prototype prototype, Direction direction, int iDimensionPrototypeID)
		{
			List<Expectation> lstExpectations = new List<Expectation>();

			List<Prototype> lstSequencePatterns = new List<Prototype>();

			Prototype rowPrototype = Prototypes.GetPrototype(prototype.PrototypeID);

			foreach (var rowValue in rowPrototype.PartOfValues)
			{
				Prototype protoSequencePattern = rowValue.Key;

				if (protoSequencePattern.Children.Any(x => x.Value == 0))
					continue;

				protoSequencePattern = protoSequencePattern.Clone();
				protoSequencePattern.Value = rowValue.Value;

				lstSequencePatterns.Add(protoSequencePattern);

				if (direction == Direction.Both)
				{
					//This version allows for multiple instances of the same prototype within a sequence pattern
					for (int i = 0; i < protoSequencePattern.Children.Count; i++)
					{
						Prototype child = protoSequencePattern.Children[i];

						if (child.PrototypeID == prototype.PrototypeID)
						{
							Expectation expectation = new Expectation();
							expectation.Prototype = protoSequencePattern;
							expectation.Index = i;
							expectation.Value = rowValue.Value;
							expectation.PrototypeName = prototype.PrototypeName;

							lstExpectations.Add(expectation);
						}
					}
				}
				else if (direction == Direction.Forward)
				{
					Prototype child = protoSequencePattern.Children[0];

					if (child.PrototypeID == prototype.PrototypeID)
					{
						Expectation expectation = new Expectation();
						expectation.Prototype = protoSequencePattern;
						expectation.Index = 0;
						expectation.Value = rowValue.Value;
						expectation.PrototypeName = prototype.PrototypeName;

						lstExpectations.Add(expectation);
					}
				}

				else if (direction == Direction.Backward)
				{
					Prototype child = protoSequencePattern.Children.Last();

					if (child.PrototypeID == prototype.PrototypeID)
					{
						Expectation expectation = new Expectation();
						expectation.Prototype = protoSequencePattern;
						expectation.Index = protoSequencePattern.Children.Count - 1;
						expectation.Value = rowValue.Value;
						expectation.PrototypeName = prototype.PrototypeName;

						lstExpectations.Add(expectation);
					}
				}
			}

			return lstExpectations;
		}


	}
}
