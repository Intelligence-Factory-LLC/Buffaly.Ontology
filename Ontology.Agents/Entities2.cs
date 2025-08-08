using BasicUtilities;
using Buffaly.NLU;
using Buffaly.NLU.Tagger.Nodes;
using Buffaly.SemanticDB;
using Buffaly.SemanticDB.Data;
using Ontology.Agents.Actions;
using Ontology.Agents.Language;
using Ontology.BaseTypes;
using Ontology.Simulation;
using ProtoScript;
using System.Text;

namespace Ontology.Agents
{
	public partial class Entities
	{

		//>create a method that takes a string and uses the fragment "Entity Extraction Enhanced" to build a prompt
		//>and then sends the prompt to the LLM and returns the result, a list of extracted entities in JSON
		static public async Task<List<string>> ExtractEntitiesAsync(string strInput)
		{
			// Get the fragment for "Entity Extraction Enhanced"
			FragmentsRow rowEntityExtraction = Fragments.GetFragmentByFragmentKey("Entity Extraction Enhanced");

			// Build the prompt using the fragment and the input string
			string strPrompt = LLMs.AddSectionToPrompt(rowEntityExtraction.Fragment, "Task", "Extract entities from the provided input.");
			string strInputWithTask = $"# Input Text:\n{strInput}\n";

			// Execute the prompt and retrieve the results
			JsonObject jsonResult = await LLMs.ExecuteLLMPromptAndInput(strPrompt, strInputWithTask);

			// Extract the list of entities from the result JSON
			List<string> lstEntities = jsonResult.GetJsonArrayOrDefault("Entities").Select(entity => entity.ToString()).ToList();

			return lstEntities;
		}

//		public static JsonObject GetUnknownEntity(Prototype protoLexeme, string strSentence)
//		{
//			DataExtractorsRow rowDataExtractor = DataExtractors.GetDataExtractorByName("Unknown Entity Analyzer");
//			string strLexeme = protoLexeme.PrototypeName;

//			string strInput = @"
//**Original Word: **
//{Word}

//* *Example Sentence: **
//""{Sentence}""
//";
//			strInput = strInput.Replace("{Word}", strLexeme);
//			strInput = strInput.Replace("{Sentence}", strSentence);

//			return new JsonObject(DataExtractors.EvaluateDataExtractor(rowDataExtractor.DataExtractorID, strInput));
//		}


		public static Prototype ? GetRelatedEntity(string strPart)
		{
			TemporaryLexeme ? rowLexeme = Lexemes.GetLexemeByLexeme(strPart);
			if (null == rowLexeme || rowLexeme.LexemePrototypes.Count == 0)
				return null;

			return rowLexeme.LexemePrototypes.FirstOrDefault(x => !Prototypes.TypeOf(x.Key, Lexeme.Prototype)).Key;
		}



		public static async Task<Tuple<Prototype, List<PrototypeDefinition>>> InsertEntity(string strEntity, string strParent, string strRelatedObject, ProtoScriptTagger tagger)
		{
			//Based on FragmentEntities2.CreateEntityInMemory but this version will create unknown words within the entity 
			//instead of just creating tokens
			strEntity = StringUtil.RemoveNonAlphanumericAndSpaces(strEntity);


			//First create any unknown words within the entity
			Prototype protoTokens = UnderstandUtil.Tokenize(strEntity);
			Prototype protoLexemes = UnderstandUtil.ConvertToLexemes(protoTokens);

			List<PrototypeDefinition> lstNewPrototypes = new List<PrototypeDefinition>();

			for (int i = 0; i < protoLexemes.Children.Count; i++)
			{
				Prototype protoToken = protoLexemes.Children[i];
				if (Prototypes.TypeOf(protoToken, System_String.Prototype))
				{
					string strToken = new StringWrapper(protoToken).GetStringValue();
					lstNewPrototypes.AddRange(await UnknownWords.CreateUnknownWordAsPrototypes2(strToken, strEntity, tagger));

					protoLexemes.Children[i] = TemporaryLexemes.GetLexemeByLexeme(strToken);
				}
			}

			List<Prototype> lstParts = new List<Prototype>();

			for (int i = 0; i < protoLexemes.Children.Count; i++)
			{
				bool bIsLast = i == protoLexemes.Children.Count - 1;

				Prototype protoToken = protoLexemes.Children[i];
				string strPart = new StringWrapper(protoTokens.Children[i]).GetStringValue();

				if (Prototypes.TypeOf(protoToken, Lexeme.Prototype))
				{
					Prototype protoRelated = LexemeUtil.GetRelatedEntityAsObjectOrTokenOrDefault(strPart);

					if (null == protoRelated)
					{
						//The code above here should always create the related entity
						throw new Exception("Unexpected");
					}

					if (protoRelated.IsInstance())
						protoRelated = protoRelated.GetBaseType();

					lstParts.Add(protoRelated);
				}
			}

			string strPrototypeName = string.Join("", lstParts.Select(x => x.PrototypeName.Contains(".") ? StringUtil.RightOfLast(x.PrototypeName, ".") : x.PrototypeName).ToArray());

			if (null == TemporaryPrototypes.GetTemporaryPrototypeOrNull(strPrototypeName))
			{
				Prototype prototype = TemporaryPrototypes.GetOrCreateTemporaryPrototype(strPrototypeName);
				TypeOfs.Insert(prototype, strParent);
				TypeOfs.Remove(prototype, "Token");

				if (lstParts.Count > 1)
				{
					tagger.Interpretter.RunMethodAsPrototype("Lexeme", "MultiToken", new List<object> { prototype, new Collection(lstParts) });
				}

				lstNewPrototypes.Add(PrototypeDefinitionHelpers.PrototypeToPrototypeDefinition(prototype, true));
			}

			Prototype protoEntity = TemporaryPrototypes.GetTemporaryPrototypeOrNull(strPrototypeName) ?? throw new Exception("Could not create prototype for entity: " + strPrototypeName);
			if (!StringUtil.IsEmpty(strRelatedObject))
				protoEntity.BidirectionalAssociate(strRelatedObject);

			return new Tuple<Prototype, List<PrototypeDefinition>>(protoEntity, lstNewPrototypes);
		}

		static public double EPSILON = 0.01;
		public static List<Prototype> RecognizeEntitiesViaActivationSpreading(string strSentence1, ProtoScriptTagger tagger)
		{
			//N20241216-02 - Very simple activation spreading routine
			Prototype protoTokens = UnderstandUtil.Tokenize(strSentence1);
			Prototype protoLexemes = UnderstandUtil.ConvertToLexemes(protoTokens);

			SpreadingActivationController bfs = new SpreadingActivationController();
			bfs.InsertPrototypes(protoLexemes.Children);

			//Start Lexemes at 1 since they are known
			protoLexemes.Children.ForEach(x => bfs.IncrementActivation(x, 1));

			FastPrototypeMap<double> mapActivations = bfs.ActivationValues;

			PrototypeSet setPrototypes = new PrototypeSet(protoLexemes.Children);

			while (bfs.HasMoreItems())
			{
				Prototype prototype = bfs.GetNextItem();
				List<Prototype> lstTemporaryAssociations = new List<Prototype>();

				double dCurrentValue = mapActivations[prototype];

				if (Prototypes.TypeOf(prototype, Lexeme.Prototype))
				{
					throw new NotImplementedException();
					//LexemePrototypesDataTable dtLexemePrototypes = LexemePrototypes.GetLexemePrototypesByPrototypeID(prototype.PrototypeID);
					//foreach (LexemePrototypesRow protoLexemePrototype in dtLexemePrototypes)
					//{
					//	TemporaryPrototype protoOccurrence = (TemporaryPrototype)Prototypes.GetAsPrototype(protoLexemePrototype.RelatedPrototypeID);
					//	lstTemporaryAssociations.Add(protoOccurrence);
					//	setPrototypes.Add(protoOccurrence);
					//}
				}

				GetExpectationsNode.UseDynamicPredictiveValues = true;
				List<Expectation> lstExpectations = GetExpectationsNode.GetExpectationsSingular(prototype, Direction.Both, Dimensions.NL.PrototypeID);
				foreach (Expectation expectation in lstExpectations)
				{
					if (Prototypes.TypeOf(expectation.Prototype, "MultiTokenPhrase"))
					{
						lstTemporaryAssociations.Add(expectation.Prototype);
					}
				}

				if (Prototypes.TypeOf(prototype, Sequence.Prototype))
				{
					//Add the target of the MultiTokenPhrase, so we can recursively continue associating
					if (Prototypes.TypeOf(prototype, "MultiTokenPhrase"))
					{
						Prototype protoTarget = MultiTokenPhrases.GetTarget(prototype);

						lstTemporaryAssociations.Add(protoTarget);
					}
				}


				Logs.DebugLog.WriteEvent("Activating", $"{prototype.PrototypeName} Value: {prototype.Value:F3} ({mapActivations[prototype]:F3})");

				foreach (var tuple in prototype.Associations)
				{
					bfs.IncrementActivation(tuple.Key, dCurrentValue * tuple.Value);
					Logs.DebugLog.WriteEvent("Association", $"{tuple.Key.PrototypeName} Value: {tuple.Key.Value:F3} ({mapActivations[tuple.Key]:F3})");

					if (mapActivations[tuple.Key] >= 0.1 && !bfs.m_setExpanded.Contains(tuple.Key.PrototypeID))
					{
						Logs.DebugLog.WriteEvent("Spreading", tuple.Key.PrototypeName);

						bfs.InsertPrototype(tuple.Key);
						setPrototypes.Add(tuple.Key);
					}
				}

				foreach (Prototype protoActivated in lstTemporaryAssociations)
				{
					bfs.IncrementActivation(protoActivated, dCurrentValue * 1);
					Logs.DebugLog.WriteEvent("Association", $"{protoActivated.PrototypeName} Value: {protoActivated.Value:F3} ({mapActivations[protoActivated]:F3})");

					if (mapActivations[protoActivated] >= 0.1 && !bfs.m_setExpanded.Contains(protoActivated.PrototypeID))
					{
						Logs.DebugLog.WriteEvent("Spreading", protoActivated.PrototypeName);

						bfs.InsertPrototype(protoActivated);
						setPrototypes.Add(protoActivated);
					}
				}
			}

			List<Prototype> lstResults = new List<Prototype>();

			foreach (var tuple in mapActivations.OrderByDescending(x => x.Value))
			{
				if (Prototypes.TypeOf(tuple.Key, "LearnedEntity"))
				{
					Logs.DebugLog.WriteEvent("Activation", tuple.Key.PrototypeName + " / " + tuple.Value + " / " + PrototypeLogging.ToChildString(tuple.Key));
					tuple.Key.Value = tuple.Value;
					lstResults.Add(tuple.Key);
				}
			}

			return lstResults;
		}

		public static string GeneralizeEntityViaActivationSpreading(string strSentence, Prototype protoEntity, ProtoScriptTagger tagger)
		{
			Prototype protoTokens = UnderstandUtil.Tokenize(strSentence);
			Prototype protoLexemes = UnderstandUtil.ConvertToLexemes(protoTokens);

			List<Prototype> lstActivatedLexemes = RecognizeLexemesFromEntityViaActivationSpreading(strSentence, protoEntity, tagger);

			for (int i = 0; i < protoLexemes.Children.Count; i++)
			{
				Prototype protoLexeme = protoLexemes.Children[i];
				if (lstActivatedLexemes.Any(x => x.ShallowEqual(protoLexeme)))
				{
					string strToken = new StringWrapper(protoTokens.Children[i]).GetStringValue();
					if (!StringUtil.InString(strSentence, protoEntity.PrototypeName))  //only place it once for multi-token lexemes
						strSentence = strSentence.Replace(strToken, protoEntity.PrototypeName);
					else
						strSentence = StringUtil.ReplaceAll(strSentence, strToken, "");
				}
			}

			return strSentence;
		}

		public static List<Prototype> RecognizeLexemesFromEntityViaActivationSpreading(string strSentence, Prototype protoEntity, ProtoScriptTagger tagger)
		{
			Prototype protoTokens = UnderstandUtil.Tokenize(strSentence);
			Prototype protoLexemes = UnderstandUtil.ConvertToLexemes(protoTokens);

			SpreadingActivationController bfs = new SpreadingActivationController();
			bfs.InsertPrototype(protoEntity);
			bfs.IncrementActivation(protoEntity, 1);

			FastPrototypeMap<double> mapActivations = bfs.ActivationValues;

			PrototypeSet setPrototypes = new PrototypeSet(protoLexemes.Children);

			while (bfs.HasMoreItems())
			{
				Prototype prototype = bfs.GetNextItem();
				List<Prototype> lstTemporaryAssociations = new List<Prototype>();

				double dCurrentValue = mapActivations[prototype];


				//Add the target of the MultiTokenPhrase, so we can recursively continue associating
				{
					List<Prototype> lstPhrases = MultiTokenPhrases.GetMultiTokenPhrasesTargeting(prototype, true);
					foreach (Prototype protoPhrase in lstPhrases)
					{
						foreach (Prototype protoPart in protoPhrase.Children)
						{

							lstTemporaryAssociations.Add(protoPart);
						}
					}
				}


				{
					List<TemporaryLexeme> dtLexemePrototypes = TemporaryLexemes.GetLexemesByRelatedPrototype(prototype);
					foreach (TemporaryLexeme protoLexemePrototype in dtLexemePrototypes)
					{
						lstTemporaryAssociations.Add(protoLexemePrototype);
					}
				}


				Logs.DebugLog.WriteEvent("Activating", $"{prototype.PrototypeName} Value: {prototype.Value:F3} ({mapActivations[prototype]:F3})");

				foreach (var tuple in prototype.Associations)
				{
					bfs.IncrementActivation(tuple.Key, dCurrentValue * tuple.Value);
					Logs.DebugLog.WriteEvent("Association", $"{tuple.Key.PrototypeName} Value: {tuple.Key.Value:F3} ({mapActivations[tuple.Key]:F3})");

					if (mapActivations[tuple.Key] >= 0.1 && !bfs.m_setExpanded.Contains(tuple.Key.PrototypeID))
					{
						Logs.DebugLog.WriteEvent("Spreading", tuple.Key.PrototypeName);

						bfs.InsertPrototype(tuple.Key);
						setPrototypes.Add(tuple.Key);
					}
				}

				foreach (Prototype protoActivated in lstTemporaryAssociations)
				{
					bfs.IncrementActivation(protoActivated, dCurrentValue * 1);
					Logs.DebugLog.WriteEvent("Association", $"{protoActivated.PrototypeName} Value: {protoActivated.Value:F3} ({mapActivations[protoActivated]:F3})");

					if (mapActivations[protoActivated] >= 0.1 && !bfs.m_setExpanded.Contains(protoActivated.PrototypeID))
					{
						Logs.DebugLog.WriteEvent("Spreading", protoActivated.PrototypeName);

						bfs.InsertPrototype(protoActivated);
						setPrototypes.Add(protoActivated);
					}
				}
			}

			List<Prototype> lstResults = new List<Prototype>();

			foreach (var tuple in mapActivations.OrderByDescending(x => x.Value))
			{
				if (protoLexemes.Children.Any(x => x.ShallowEqual(tuple.Key)))
				{
					Logs.DebugLog.WriteEvent("Activation", tuple.Key.PrototypeName + " / " + tuple.Value + " / " + PrototypeLogging.ToChildString(tuple.Key));
					tuple.Key.Value = tuple.Value;
					lstResults.Add(tuple.Key);
				}
			}

			return lstResults;
		}

		static public async Task<List<Prototype>> GetCandidateEntitiesBySemanticSearch(string strSearch, double dThreshold = 0.4)
		{
			FragmentsDataTable dtFragments = await Fragments.GetMostSimilar2ByTagID(strSearch, "Entity", dThreshold);
			List<Prototype> lstResults = new List<Prototype>();

			foreach (FragmentsRow rowFragment in dtFragments)
			{
				Prototype protoEntity = rowFragment.GetPrototype();
				if (null == protoEntity)
					continue;

				protoEntity.Value = rowFragment.DataObject.GetDoubleOrDefault("Similarity", 0);
				protoEntity.Properties["LearnedEntity.Field.EntityName"] = new StringWrapper(rowFragment.Fragment);

				if (!lstResults.Any(x => x.ShallowEqual(protoEntity)))
					lstResults.Add(protoEntity);
			}

			return lstResults;
		}

		public static string EntityToString(Prototype prototype)
		{
			PrototypeDefinition protoDef = PrototypeDefinitionHelpers.PrototypeToPrototypeDefinition(prototype, false);
			return ProtoScript.Parsers.SimpleGenerator.Generate(protoDef);
		}

		public static string EntitiesToString(List<Prototype> lstResults)
		{
			StringBuilder sb = new StringBuilder();
			foreach (Prototype prototype in lstResults)
			{
				sb.AppendLine(EntityToString(prototype));
			}

			foreach (Prototype proto in lstResults)
			{
				int ? protoParent = proto.GetTypeOfs().FirstOrDefault();
				if (null != protoParent)
				{
					//Get the original so it has the properties
					sb.AppendLine(Entities.EntityToString(TemporaryPrototypes.GetTemporaryPrototype(protoParent.Value)));
				}
			}
			return sb.ToString();
		}

	}
}
