using Ontology;

namespace Buffaly.NLU.Tagger
{	
	public class ControllerNode
	{
		public class TrainingInfo
		{
			public ControllerNode Previous = null;
			public ControllerNode Selected = null;
			public int Iteration = 0;
			public Prototype SubTyped = null;
		}

		public enum ExplorationStrategy { Naive, Weighted, Max, WeightedAndAverage, ShortestFirst };
		public static ExplorationStrategy Strategy = ExplorationStrategy.WeightedAndAverage; // ExplorationStrategy.Naive;     //N20190814-01

		public double Value = 1;
		public double ExplorationValue = 1;
		public int Visits = 0;

		public TrainingInfo Info = new TrainingInfo(); 

		public double CurrentValue
		{
			get
			{
				return Value * ExplorationValue;
			}
		}

		public ControllerNode Root
		{
			get
			{
				if (this.Parent == null)
					return this;

				return this.Parent.Root;
			}
		}

		public List<ControllerNode> Possibilities = new List<ControllerNode>();

		public ControllerNode Parent = null;

		public ControllerNode AddPossibility(Prototype prototype, double dValue)
		{
			ControllerNode node = new ControllerNode();
			node.Parent = this;
			node.Value = dValue; 
			node.ExplorationValue = dValue > 0 ? 1 : 0;      //start at 1

			return AddPossibility(node);
		}

		public ControllerNode AddPossibility(ControllerNode node)
		{
			//TODO: Implement EqualTo for the BaseNode
			ControllerNode nodeExisting = this.Possibilities.FirstOrDefault(x => x == node);
			if (null == nodeExisting)
			{
				this.Possibilities.Add(node);
				nodeExisting = node;
				this.UpdateExplorationValues();
			}
			else if (nodeExisting.Value < node.Value)
			{
				nodeExisting.Value = node.Value;
				this.UpdateExplorationValues();
			}

			return nodeExisting;
		}

		public int Count()
		{
			if (!HasPossibilities())
				return 1;

			return this.Possibilities.Sum(x => x.Count());
		}

		public ControllerNode CloneShallow()
		{
			//N20221004-01
			ControllerNode node = this.MemberwiseClone() as ControllerNode;
			node.Value = this.Value;
			node.ExplorationValue = this.ExplorationValue;
			return node; 
		}

		public ControllerNode CloneSelected()
		{
			ControllerNode node = this.CloneShallow();
			node.Possibilities = new List<ControllerNode>();
			node.Info = new TrainingInfo();

			foreach (ControllerNode nodeChild in this.Possibilities)
			{
				node.Possibilities.Add(nodeChild.CloneShallow());
			}

			node.Info.Iteration = this.Info.Iteration;

			if (null != this.Info.Selected)
				node.Info.Selected = this.Info.Selected.CloneSelected();
			if (null != this.Info.SubTyped)
				node.Info.SubTyped = this.Info.SubTyped.Clone();

			node.Info.Previous = this.Info.Previous;

			return node; 
		}


		public bool HasPossibilities()
		{
			return this.Possibilities.Count > 0;
		}

		public bool HasMoreValuedPossibilities()
		{
			return this.Possibilities.Any(x => x.CurrentValue > 0);
		}

		public ControllerNode SelectNextPossibility()
		{
			//no random
			ControllerNode node = this.Possibilities.OrderByDescending(x => x.CurrentValue).FirstOrDefault();
			if (node == null)
				return null;

			if (node.CurrentValue == 0)
			{
				if (node.ExplorationValue > 0)
				{
					//If added with 0 value it needs to update the leaf and tree
					node.UpdateExplorationValues();
				}

				return null;
			}

			//#5  This should set the value to 0 if a leaf, or to the sum of all children otherwise
			if (null != node)
			{
				node.UpdateExplorationValues();
				node.Visits++;
			}

			if (null != this.Info)
			{
				this.Info.Selected = node; 
			}

			return node;
		}


		public void UpdateExplorationValues()
		{
			ControllerNode node = this;
			while (node != null)
			{
				if (node.Possibilities.Count == 0)
					node.ExplorationValue = 0;

				else
				{
					switch (Strategy)
					{
						//N20220121-03 - Use * x.Value so that we can turn off a node
						case ExplorationStrategy.Naive:
							node.ExplorationValue = node.Possibilities.Sum(x => x.ExplorationValue * x.Value);// / node.Possibilities.Count;
							break;

						//N20220501-04 - Prefer less possibilities
						case ExplorationStrategy.Weighted:
							node.ExplorationValue = node.Possibilities.Sum(x => x.ExplorationValue * x.Value) / Math.Pow(node.Possibilities.Count, 2);
							break;

						case ExplorationStrategy.WeightedAndAverage:
							node.ExplorationValue = node.Possibilities.Sum(x => x.ExplorationValue * x.Value) / node.Possibilities.Count;
							break;


						case ExplorationStrategy.Max:
							node.ExplorationValue = node.Possibilities.Max(x => x.ExplorationValue * x.Value);
							break;

						//Note: This one doesn't work 
						case ExplorationStrategy.ShortestFirst:
							node.ExplorationValue = node.Possibilities.Max(x => 1 * x.Value);
							break;
					}
				}


				node = node.Parent;
			}
		}

	}
}
