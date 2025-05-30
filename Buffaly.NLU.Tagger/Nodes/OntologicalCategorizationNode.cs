using Buffaly.NLU.Tagger.RolloutController;
using Ontology;
using System.Text;
using Ontology.Simulation;

namespace Buffaly.NLU.Tagger.Nodes
{
	public class OntologicalCategorizationNode : BaseFunctionNode
	{
		static public bool KeepLexemes = false;
		static public bool UseQuantumPrototypes = true;
		public OntologicalCategorizationNode(Prototype protoSource)
		{
			Source = protoSource;
			Value = ValueFunctions.GetFragmentValue(Source);
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder("OntologicalCategorizationNode - ");
			sb.Append(PrototypeLogging.ToChildString(Source));
			return sb.ToString();
		}

		public override List<BaseNode> GeneratePossibilities()
		{
			List<BaseNode> lstBaseNode = new List<BaseNode>();

			for (int i = 0; i < Source.Children.Count; i++)
			{
				Prototype prototype = Source.Children[i];

				//Only recognized strings
				if (Prototypes.TypeOf(prototype, Lexeme.Prototype))
				{
					OntologicalCategorizationSingleNode singleNode = new OntologicalCategorizationSingleNode(Source, i);
					lstBaseNode.Add(singleNode);
				}
			}

			return lstBaseNode;
		}

		public override ControlFlag OnResultReceived(Result result)
		{
			if (result is RangeResult)
			{
				RangeResult rangeResult = result as RangeResult;

				Prototype protoSequence = Source.Clone();
				protoSequence.Children[rangeResult.RangeStart] = rangeResult.Result.Clone();

				return new SingleResult(protoSequence);
			}

			return result;
		}
	}

	public class OntologicalCategorizationSingleNode : BaseFunctionNode
	{
		public Prototype SourceItem;
		public int Index = 0;
		public OntologicalCategorizationSingleNode(Prototype protoSource, int iIndex)
		{
			Source = protoSource;
			Index = iIndex;
			SourceItem = protoSource.Children[iIndex];

		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder("OntologicalCategorizationSingleNode - ");
			sb.Append(PrototypeLogging.ToChildString(Source));
			return sb.ToString();
		}

		public override List<BaseNode> GeneratePossibilities()
		{
			List<BaseNode> lstBaseNode = new List<BaseNode>();

			TemporaryLexeme ? lexeme = SourceItem as TemporaryLexeme;
			if (null == lexeme)
			{
				throw new Exception("OntologicalCategorizationSingleNode can only be used with TemporaryLexeme prototypes.");
			}
			
			List<Prototype> possiblePrototypes = new List<Prototype>();
			foreach (var row in lexeme.LexemePrototypes)
			{
				if (row.Value > 0)
				{
					var prototype = row.Key;
					if (!Prototypes.TypeOf(prototype, Lexeme.Prototype))
					{
						possiblePrototypes.Add((Prototype)prototype);
					}
				}
			}

			if (OntologicalCategorizationNode.UseQuantumPrototypes && possiblePrototypes.Count > 1)
			{
				QuantumPrototype quantumPrototype = new QuantumPrototype(possiblePrototypes);
				BaseTaggingNode baseTaggingNode = new BaseTaggingNode();
				baseTaggingNode.Source = SourceItem;
				baseTaggingNode.Reference = quantumPrototype;
				baseTaggingNode.Value = quantumPrototype.Value;
				lstBaseNode.Add(baseTaggingNode);
			}
			else
			{
				foreach (var proto in possiblePrototypes)
				{
					BaseTaggingNode baseTaggingNode = new BaseTaggingNode();
					baseTaggingNode.Source = SourceItem;
					baseTaggingNode.Reference = proto;
					baseTaggingNode.Value = proto.Value;
					lstBaseNode.Add(baseTaggingNode);
				}
			}

			return lstBaseNode;
		}


		public override ControlFlag EvaluateNext(BaseNode nodeChild)
		{
			BaseTaggingNode node = nodeChild as BaseTaggingNode;

			RangeResult rangeResult = new RangeResult();
			rangeResult.Result = node.Reference;
			rangeResult.RangeStart = Index;
			rangeResult.RangeLength = 1;

			//Keep the original lexemes
			if (OntologicalCategorizationNode.KeepLexemes)
				rangeResult.Result.Children.Add(SourceItem);

			return rangeResult;
		}

	}
}
