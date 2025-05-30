using Buffaly.NLU.Tagger.RolloutController;
using Ontology;
using System.Text;

namespace Buffaly.NLU.Tagger.Nodes
{
	public class TaggingNode : BaseFunctionNode
	{
		public delegate bool ExitConditionDelegate(Prototype protoSequence);
		public ExitConditionDelegate ExitCondition = null;

		public TaggingNode(Prototype protoSource)
		{
			Source = new Collection();

			foreach (Prototype child in protoSource.Children)
			{
				Prototype prototype = child;

				string strLexeme = (child as NativeValuePrototype)?.NativeValue as string;
				if (null != strLexeme)
				{
					prototype = TemporaryLexemes.GetLexemeByLexeme(strLexeme);
				}

				this.Source.Children.Add(prototype);
			}

		}

		public TaggingNode()
		{

		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder("TaggingNode - ");
			sb.Append(PrototypeLogging.ToChildString(Source));
			return sb.ToString();
		}

		public override List<BaseNode> GeneratePossibilities()
		{
			List<BaseNode> lstBaseNode = new List<BaseNode>();

			if (Source.Children.Any(x => Prototypes.TypeOf(x, Lexeme.Prototype)))
			{
				lstBaseNode.Add(new OntologicalCategorizationNode(Source));
			}

			if (Source.Children.All(x => x.Value > 0))
			{
				lstBaseNode.Add(new RolloutNode(Source));
			}

			return lstBaseNode;
		}

		public override ControlFlag EvaluateNext(BaseNode nodeChild)
		{
			//Logs.DebugLog.WriteEvent("TaggingNode", nodeChild.CurrentValue + " " + nodeChild.ToString() , BasicUtilities.DebugLog.debug_level_t.MINIMAL);
			return base.EvaluateNext(nodeChild);
		}

		public override ControlFlag OnResultReceived(Result result)
		{
			//Exit conditions 
			if (result is SingleResult)
			{
				SingleResult singleResult = result as SingleResult;
				if (singleResult.Result.PrototypeID == Ontology.Collection.PrototypeID)
				{
					if (singleResult.Result.Children.Count == 0
						|| Prototypes.TypeOf(singleResult.Result.Children.First(), Lexeme.Prototype))
					{
						//continue
					}

					//Simple exit condition
					else if (singleResult.Result.Children.Count == 1)
					{
						Prototype protoResult = singleResult.Result.Children.First();
						if (null != this.ExitCondition)
						{
							if (this.ExitCondition(protoResult))
								return new SingleResult(protoResult);
						}

						else
							return new SingleResult(protoResult);
					}

					//allow checking for combined actions like "we like to eat and smoke" but only 
					//if we are using an ExitCondition
					else if (null != this.ExitCondition && !singleResult.Result.Children.Any(x => Prototypes.TypeOf(x, Lexeme.Prototype)))
					{
						Prototype protoResult = singleResult.Result;
						if (this.ExitCondition(protoResult))
							return new SingleResult(protoResult);
					}
						
				}
				else
					throw new Exception("Unexpected: Tagging should always return a Collection");

				//Check that we don't use the same possibility again. The check can't be done inside
				//the AddPossibility method because it is in BaseNode. I could add an == operator later 
				//to make this easier
				if (!this.Possibilities.Any(x => PrototypeGraphs.AreEquivalentCircular((x as BaseFunctionNode).Source, singleResult.Result)))
				{
					//See Tagger.GetFragmentValue for current value
					if (singleResult.Result.Children.Any(x => Prototypes.TypeOf(x, Lexeme.Prototype)))
					{
						OntologicalCategorizationNode ontologicalCategorizationNode = new OntologicalCategorizationNode(singleResult.Result);
						ontologicalCategorizationNode.Info.Previous = this.Info.Selected.CloneSelected();
						this.AddPossibility(ontologicalCategorizationNode);
					}

					if (singleResult.Result.Children.All(x => x.Value > 0))
					{
						RolloutNode rolloutNode = new RolloutNode(singleResult.Result);
						rolloutNode.Info.Previous = this.Info.Selected.CloneSelected();
						this.AddPossibility(rolloutNode);
					}

					Logs.DebugLog.WriteEvent("Result", PrototypeLogging.ToChildString(singleResult.Result), BasicUtilities.DebugLog.debug_level_t.MINIMAL);
				}
				else

				{
					//Logs.DebugLog.WriteEvent("Duplicate", PrototypeLogging.ToChildString(singleResult.Result), BasicUtilities.DebugLog.debug_level_t.MINIMAL);
				}

				return new Continue();

			}

			return result;
		}


	}
}
