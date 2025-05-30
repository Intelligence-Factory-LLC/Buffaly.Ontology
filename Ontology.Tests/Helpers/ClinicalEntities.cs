using BasicUtilities;
using BasicUtilities.Collections;
using Buffaly.NLU;
using Ontology;
using Ontology.Data;
using Ontology.Simulation;
using ProtoScript.Interpretter.RuntimeInfo;
using ProtoScript.Parsers;

namespace ProtoScript.Tests.Helpers
{
	internal class ClinicalEntities
	{
		private static DebugLog m_DebugLog = null;
		public static DebugLog DebugLog
		{
			get
			{
				if (null == m_DebugLog)
				{
					m_DebugLog = new DebugLog();
					m_DebugLog.Name = "ClinicalOntology";
					m_DebugLog.LogPath = @"Z:\Medical Ontologies\SnoMedWorking\logs"; //put these somewhere they won't be deleted
					m_DebugLog.DebugLevel = DebugLog.debug_level_t.INFORMATIONAL;
					m_DebugLog.MaxLinesPerFile = 0;
				}

				return m_DebugLog;
			}
		}


		public static async Task MapICD(Prototype protoRoot, ProtoScriptTagger tagger)
		{
			DebugLog.WriteEvent("Mapping ICD to Clinical Ontology", protoRoot.PrototypeName);
			string strCode = protoRoot.Properties.GetStringOrDefault2("CodeValue");

			List<string> lstStrings = ICDCodeHelper.GetPhrases(protoRoot);

			await LearnIntermediateLayer(tagger, lstStrings);

			//See if the correct clinical concept for this ICD exists yet 
			Prototype? protoCandidate = ICDCodeHelper.GetClinicalEntityByCode(strCode);

			if (null == protoCandidate)
			{
				string strPreferredName = lstStrings.First();
				string strPrototypeName = Lexemes.ToPrototypeName(strPreferredName);

				Prototype protoClinicalObject = TemporaryPrototypes.GetOrCreateTemporaryPrototype($"ClinicalOntology.{strPrototypeName}");
				protoClinicalObject.Properties.SetString("ClinicalOntology.ClinicalEntity.Field.CodeValue", strCode);
				protoClinicalObject.InsertTypeOf("ClinicalOntology.Condition");

				PrototypeDefinition defClinicalObject = PrototypeDefinitionHelpers.PrototypeToPrototypeDefinition(protoClinicalObject, false);

				//>log the defClinicalObject
				DebugLog.WriteEvent("Creating Clinical Object", SimpleGenerator.Generate(defClinicalObject));

				WriteNewPrototype(tagger, defClinicalObject, "ClinicalOntology.pts");
			}

			foreach (string strPhrase in lstStrings)
			{
				LinkBagOfWordsToClinicalObject(strCode, strPhrase, tagger);
			}

		}
		public static async Task MapSnoMed(Prototype protoRoot, ProtoScriptTagger tagger)
		{
			string strCode = StringWrapper.ToString(protoRoot.Properties.GetOrDefault2("MappedIcdCodes").Children.First());

			DebugLog.WriteEvent("Mapping SnoMed to Clinical Ontology", protoRoot.PrototypeName);

			Set<string> lstPhrases = SnoMedHelper.GetPhrases(protoRoot);

			await LearnIntermediateLayer(tagger, lstPhrases.ToList());

			//See if the correct clinical concept for this ICD exists yet 
			Prototype protoClinicalEntity = TemporaryPrototypes.GetTemporaryPrototype("ClinicalOntology.ClinicalEntity");
			Prototype protoCandidates = tagger.Interpretter.RunMethodAsPrototype(null, "GetClinicalObjectForConcept", new List<object>() { protoRoot.PrototypeName });
			Prototype? protoCandidate = protoCandidates.Children.FirstOrDefault();

			if (null == protoCandidate)
			{
				string strPreferredName = protoRoot.Properties.GetStringOrDefault("SnoMed.ClinicalConcept.Field.PreferredTerm");
				string strPrototypeName = Lexemes.ToPrototypeName(strPreferredName);

				Prototype protoClinicalObject = TemporaryPrototypes.GetOrCreateTemporaryPrototype($"ClinicalOntology.{strPrototypeName}");
				protoClinicalObject.InsertTypeOf("ClinicalOntology.Condition");

				protoClinicalObject.Properties["ClinicalOntology.ClinicalEntity.Field.Concepts"] = new Collection() { new StringWrapper(protoRoot.PrototypeName) };
				protoClinicalObject.Properties.SetString("ClinicalOntology.ClinicalEntity.Field.CodeValue", strCode);

				protoCandidate = protoClinicalObject;

				PrototypeDefinition defClinicalObject = PrototypeDefinitionHelpers.PrototypeToPrototypeDefinition(protoClinicalObject, false);

				//>log the defClinicalObject
				DebugLog.WriteEvent("Creating Clinical Object", "\n" + SimpleGenerator.Generate(defClinicalObject));

				WriteNewPrototype(tagger, defClinicalObject, "ClinicalOntology.pts");
			}

			//Need to check the NL mapping 
			foreach (string strPhrase in lstPhrases)
			{
				DebugLog.WriteEvent("Mapping Phrase", strPhrase);

				Prototype protoTagged = TagPhraseToPrototype(tagger, strPhrase);

				//N20250518-01 - If we are not mapping to the correct ClinicalEntity based on the SnoMed concept, first try to create a new sememe
				//phrase 
				if (!protoTagged.TypeOf("ClinicalOntology.ClinicalEntity") || !protoTagged.TypeOf(protoCandidate))
				{
					List<PrototypeDefinition> lstPrototypeDefinitions = LinkBagOfFeaturesToClinicalObject2(strPhrase, tagger, protoCandidate, protoTagged);
					//Try again 
					protoTagged = TagPhraseToPrototype(tagger, strPhrase);
				}

				//If tagging reveals a collection, then the Clinical Entity should be one of the items within it 
				if (protoTagged.Children.Count > 0 && protoTagged.TypeOf(Ontology.Collection.Prototype))
				{
					bool bIdentified = false;
					Prototype protoTaggedEntity = protoTagged.Children.FirstOrDefault(x => x.TypeOf(protoCandidate));
					if (null != protoTaggedEntity)
					{
						protoTagged = protoTaggedEntity;
						bIdentified = true;
					}

					//otherwise we may need to do a BagOfFeatures categorization on the collection 
					else
					{
						//This was moved into the TagPhraseToPrototype method
						protoTaggedEntity = BagOfFeatures.CategorizeAndUnderstand(protoTagged);
						if (protoTaggedEntity.TypeOf(protoCandidate))
						{
							protoTagged = protoTaggedEntity;
							bIdentified = true;
						}
					}

					if (!bIdentified)
					{
						//problem here
					}
				}

				//The existing phrase does not map to the expected Clinical Entity, we need to split it 
				//NewEntity typeof ExistingEntity 
				else if (!protoTagged.TypeOf(protoCandidate))
				{
					//This is the current Clinical Entity to which the phrase is mapped
					Prototype protoEntity = protoTagged.GetAllParents().First(x => x.PrototypeName.Contains("ClinicalOntology"));

					DebugLog.WriteEvent("Mapping Phrase", strPhrase + " already linked to " + protoEntity.PrototypeName + ", splitting...");

					//This is the current sememe that maps to the clinical entity 
					Prototype protoSememe = protoTagged.GetAncestorsBelow(protoEntity).FirstOrDefault() ?? throw new Exception(protoTagged.PrototypeName + " should have a sememe");


					//Change the Sememantic linking to the new child entity
					PrototypeDefinition protoDefSememe = PrototypeDefinitionHelpers.GetPrototypeDefinitionOrNull(GetProjectLocalFile(tagger, "IntermediateLayer.pts"), protoSememe.PrototypeName);
					protoDefSememe.Inherits.RemoveAll(x => x.TypeName == protoEntity.PrototypeName);    //Remove the old entity 
					protoDefSememe.Inherits.Add(new ProtoScript.Type() { TypeName = protoCandidate.PrototypeName }); //add the new entity 

					DebugLog.WriteEvent("Removing Link", protoSememe.PrototypeName + " -> " + protoEntity.PrototypeName);
					DebugLog.WriteEvent("Adding Link", protoSememe.PrototypeName + " -> " + protoCandidate.PrototypeName);
					//>log protoDefSememe
					DebugLog.WriteEvent("Creating Sememe", "\n" + SimpleGenerator.Generate(protoDefSememe));

					WriteNewPrototype(tagger, protoDefSememe, "IntermediateLayer.pts");

					//The new entity is now a typeof the old entity
					PrototypeDefinition protoDefCandidate = PrototypeDefinitionHelpers.GetPrototypeDefinitionOrNull(GetProjectLocalFile(tagger, "ClinicalOntology.pts"), protoCandidate.PrototypeName);
					if (!protoDefCandidate.Inherits.Any(x => x.TypeName == protoEntity.PrototypeName))
					{
						protoDefCandidate.Inherits.Add(new ProtoScript.Type() { TypeName = protoEntity.PrototypeName });
					}

					DebugLog.WriteEvent("Adding Link", protoCandidate.PrototypeName + " -> " + protoEntity.PrototypeName);

					WriteNewPrototype(tagger, protoDefCandidate, "ClinicalOntology.pts");
				}

			}
		}

		public static Prototype TagPhraseToPrototype(ProtoScriptTagger tagger, string strPhrase)
		{
			Prototype protoTagged = UnderstandUtil.Understand3(strPhrase, tagger);
			if (protoTagged.ShallowEqual(Ontology.Collection.Prototype))
			{
				if (protoTagged.Children.Count == 1)
					protoTagged = protoTagged.Children[0];
				else
					protoTagged = BagOfFeatures.CategorizeAndUnderstandMultiple(protoTagged);
			}

			return protoTagged;
		}

		private static string GetProjectLocalFile(ProtoScriptTagger tagger, string strLocalFile)
		{
			string strProjectDirectory = StringUtil.LeftOfLast(tagger.ProjectFile, "\\");
			string strEnglishFile = FileUtil.BuildPath(strProjectDirectory, strLocalFile);
			return strEnglishFile;
		}

		public static async Task<List<PrototypeDefinition>> CreateWordForms(ProtoScriptTagger tagger, string strWord)
		{
			string strJsonCachePath = FileUtil.BuildPath(@"Z:\Medical Ontologies\SnoMedWorking", "WordFormsCache.json");			List<PrototypeDefinition> lstPrototypes = new List<PrototypeDefinition>();

			JsonObject jsonCache = new JsonObject();
			if (System.IO.File.Exists(strJsonCachePath))			{
				jsonCache = new JsonObject(FileUtil.ReadFile(strJsonCachePath));
			}

			JsonObject jsonResult = jsonCache.GetJsonObjectOrDefault(strWord.ToLower());			if (jsonResult.Keys.Count != 0)
			{
				DebugLog.WriteEvent("Word Forms: " + strWord, "Already processed", DebugLog.debug_level_t.VERBOSE);
			}			else			{
				jsonResult = await UnknownWords.GetWordForms(strWord);

				if (jsonResult != null)
				{
					DebugLog.WriteEvent("Creating Word Forms: " + strWord, "\n" + JsonUtil.ToFriendlyJSON(jsonResult).ToString());

					List<string> lstForms = new List<string>();
					string strRootForm = null;
					Prototype? protoSememe = null;
					bool bCreateSememe = false;


					//Find the correct semantic root for the word first before doing anything else 
					foreach (JsonObject jsonForm in jsonResult.GetJsonArrayOrDefault("Forms"))
					{
						//The prompt seems to choose the root form as the original word
						bool bIsRootForm = jsonForm.GetBooleanOrFalse("IsRootForm");
						string strForm = jsonForm.GetStringOrNull("WordForm");

						//This form just uses RootFormSememe
						if (bIsRootForm)
							strRootForm = strForm;

						string strSememe = StringUtil.UppercaseFirstLetter(strForm) + "Sememe";
						protoSememe = TemporaryPrototypes.GetTemporaryPrototypeOrNull(strSememe);

						if (null != protoSememe)
							break; 

						//This original logic didn't work if we already had a sememe (but distant)
						//{
						//	LexemesRow? rowLexeme = Lexemes.GetLexemeByLexeme(strRootForm);
						//	if (null != rowLexeme)
						//	{
						//		//See if there is an existing semantic root for these forms
						//		Prototype? protoForm = rowLexeme.LexemePrototypes.FirstOrDefault(x => TemporaryPrototypes.GetTemporaryPrototype(x.RelatedPrototypeID).TypeOf("Sememe"))?.RelatedPrototype;
						//		foreach (Prototype protoTemp in protoForm?.GetAncestorsBelow("Sememe") ?? Enumerable.Empty<Prototype>())
						//		{
						//			//TODO: If there are multiple Sememes here we need logic to get the right one
						//			protoSememe = protoTemp;
						//		}
						//	}
						//}
					}

					if (null == protoSememe)
					{
						string strSememe = StringUtil.UppercaseFirstLetter(strRootForm) + "Sememe";
						bCreateSememe = (null == TemporaryPrototypes.GetTemporaryPrototypeOrNull(strSememe));
						protoSememe = TemporaryPrototypes.GetOrCreateTemporaryPrototype(strSememe);
						protoSememe.InsertTypeOf("Sememe");


						//>log the sememe
						DebugLog.WriteEvent("Creating Sememe", PrototypeLogging.ToFriendlyShadowString2(protoSememe));						
					}

					foreach (JsonObject jsonForm in jsonResult.GetJsonArrayOrDefault("Forms"))
					{
						string strForm = jsonForm.GetStringOrNull("WordForm");
						string strPartOfSpeech = jsonForm.GetStringOrNull("PartOfSpeech");
						string strDefinition = jsonForm.GetStringOrNull("Definition");
						List<string> lstMisspellings = jsonForm.GetJsonArrayOrDefault("Misspellings").ToStringList();
						List<string> lstAbbreviations = jsonForm.GetJsonArrayOrDefault("Abbreviations").ToStringList();
						string strExampleSentence = jsonForm.GetStringOrNull("ExampleSentence");
						bool bIsRootForm = jsonForm.GetBooleanOrFalse("IsRootForm");

						if (strForm.Contains(" ") || StringUtil.ContainsNonAlphanumericOrSpace(strForm))
						{
							continue;
						}

						lstForms.Add(strForm);
						if (bIsRootForm)
						{
							DebugLog.WriteEvent("Root Form", strForm);
							strRootForm = strForm;
						}

						LexemesRow? rowLexeme = Lexemes.GetLexemeByLexeme(strForm);
						
						if (null == rowLexeme)
						{
							//new word

							//>log the new word
							DebugLog.WriteEvent("New Word", $"Creating prototypes for '{strForm}'");
							lstPrototypes.AddRange(await UnknownWords.CreateUnknownWordAsPrototypes2(strForm, strExampleSentence, tagger));

							PrototypeDefinition ? prototypeDefinition = lstPrototypes.FirstOrDefault();
							if (null != prototypeDefinition)
							{
								foreach (string strAbbreviation in lstAbbreviations)
								{
									prototypeDefinition.Annotations.Add(AnnotationExpressions.Parse($"[global.Lexeme.Singular(\"{strAbbreviation}\")]"));
								}

								foreach (string strMisspelling in lstMisspellings)
								{
									prototypeDefinition.Annotations.Add(AnnotationExpressions.Parse($"[global.Lexeme.Singular(\"{strMisspelling}\")]"));
								}
							}

							foreach (PrototypeDefinition protoDef in lstPrototypes)
							{
								//Rare check to see if the new prototype is reserved for something else 
								object obj = tagger.Compiler.Symbols.GetSymbol(protoDef.PrototypeName.TypeName);
								if (obj != null && obj is not PrototypeTypeInfo)
								{
									protoDef.PrototypeName.TypeName += "2";
								}
							}
						}
					}

					if (lstPrototypes.Count > 0)
					{
						WriteNewPrototypes(tagger, lstPrototypes, "LearnedEnglish.pts");

						//>log each of the new prototype definitions
						foreach (PrototypeDefinition protoDef in lstPrototypes)
						{
							DebugLog.WriteEvent("Created Prototype", "\n" + SimpleGenerator.Generate(protoDef));
						}  
					}

					FastPrototypeSet lstPrototypeForms = new FastPrototypeSet();
					foreach (string strForm in lstForms)
					{
						LexemesRow rowLexeme = Lexemes.GetLexemeByLexeme(strForm);
						foreach (LexemePrototypesRow rowLexemePrototype in rowLexeme.LexemePrototypes)
						{
							Prototype protoForm = TemporaryPrototypes.GetTemporaryPrototype(rowLexemePrototype.RelatedPrototypeID);
							if (Prototypes.TypeOf(protoForm, Lexeme.Prototype))
								continue;
							
							if (protoForm.IsInstance())
								continue;

							if (Prototypes.TypeOf(protoForm, "Action")) // we don't need every verb form, just the base
							{
								//Get the first typeof Action (because quantum prototypes can have Object first)
								lstPrototypeForms.Add(protoForm.GetTypeOfs().Children.First(x => x.TypeOf("Action")));
								continue;
							}

							if (!Prototypes.TypeOf(protoForm, protoSememe))
							{
								lstPrototypeForms.Add(protoForm);
							}
						}
					}

					lstPrototypes.Clear();
					if (bCreateSememe)
					{
						lstPrototypes.Add(PrototypeDefinitionHelpers.PrototypeToPrototypeDefinition(protoSememe, false));
					}

					foreach (Prototype protoForm in lstPrototypeForms)
					{
						Prototype protoForm2 = protoForm.ShallowClone();
						protoForm2.InsertTypeOf(protoSememe);

						PrototypeDefinition protoDef = PrototypeDefinitionHelpers.PrototypeToPrototypeDefinition(protoForm2, false);
						//Just want the new typeof here 
						protoDef.Inherits.RemoveRange(1, protoDef.Inherits.Count - 1);

						DebugLog.WriteEvent("Linking", $"{protoForm.PrototypeName} -> {protoSememe.PrototypeName}");

						lstPrototypes.Add(protoDef);
					}

					WriteNewPrototypes(tagger, lstPrototypes, "IntermediateLayer.pts");


					//>log each prototype definition
					foreach (PrototypeDefinition protoDef in lstPrototypes)
					{
						DebugLog.WriteEvent("Created Prototype", "\n" + SimpleGenerator.Generate(protoDef));
					}  
				}

				jsonCache[strWord.ToLower()] = jsonResult;
				FileUtil.WriteFile(strJsonCachePath, JsonUtil.ToFriendlyJSON(jsonCache).ToString());
			}
			return lstPrototypes;
		}

		static public void UncacheWordForm(string strWord)
		{
			string strJsonCachePath = FileUtil.BuildPath(@"Z:\Medical Ontologies\SnoMedWorking", "WordFormsCache.json");
			JsonObject jsonCache = new JsonObject();
			if (System.IO.File.Exists(strJsonCachePath))			{
				jsonCache = new JsonObject(FileUtil.ReadFile(strJsonCachePath));
			}

			JsonObject jsonResult = jsonCache.GetJsonObjectOrDefault(strWord.ToLower());			if (jsonResult.Keys.Count != 0)
			{
				jsonCache.Remove(strWord.ToLower());
			}

			//save the file
			FileUtil.WriteFile(strJsonCachePath, JsonUtil.ToFriendlyJSON(jsonCache).ToString());
		}

		public static async Task LearnIntermediateLayer(ProtoScriptTagger tagger, List<string> lstStrings)
		{

			foreach (string strDescription in lstStrings)
			{
				DebugLog.WriteEvent("Learning Intermediate Layer", strDescription);

				List<string> lstTokens = Lexemes.Split(strDescription);
				bool bProcessPhrase = false;
				foreach (string token in lstTokens)
				{
					if (null == Lexemes.GetLexemeByLexeme(token))
					{
						//This is a new word, we need to learn it
						DebugLog.WriteEvent("Learning New Word", token);
						bProcessPhrase = true;
						break;
					}
				}

				if (bProcessPhrase)
				{
					List<PrototypeDefinition> lstNewWords = await WordLearning.ProcessPhraseAsync(strDescription, tagger, true);

					//>Write each new word to the log
					foreach (PrototypeDefinition protoDef in lstNewWords)
					{
						DebugLog.WriteEvent("Learned New Word", protoDef.PrototypeName.TypeName);
						DebugLog.WriteEvent("Definition", "\n" + SimpleGenerator.Generate(protoDef));
					}
				}

				bool bLearnWordForms = false;
				List<Prototype> lstFeatures = GetFeatures(tagger, strDescription);
				foreach (Prototype feature in lstFeatures)
				{
					//If any feature is missing the base, just learn them all, don't try to isolate it
					if (!feature.TypeOf("Sememe"))  //we need learn everything 
					{
						DebugLog.WriteEvent("Learning Word Forms", "Sememe Missing: " + feature.PrototypeName);

						bLearnWordForms = true;
						break;
					}
				}

				if (bLearnWordForms)
				{
					foreach (string token in lstTokens)
					{
						await CreateWordForms(tagger, token);
					}
				}
			}
		}

		public static List<Prototype> GetFeatures(ProtoScriptTagger tagger, string strDescription)
		{
			Prototype protoTagged = UnderstandUtil.Understand3(strDescription, tagger);

			return BagOfFeatures.GetFeatures(protoTagged);
		}


		public static void LinkBagOfWordsToClinicalObject(string strCode, string strPhrase, ProtoScriptTagger tagger)
		{
			Prototype? protoCandidate = ICDCodeHelper.GetClinicalEntityByCode(strCode);

			if (null == protoCandidate)
			{
				throw new Exception($"The ClinicalOntology Entity for {strCode} does not exist, cannot link");
			}

			Prototype prototype1 = UnderstandUtil.Understand3(strPhrase, tagger);
			LinkBagOfFeaturesToClinicalObject2(strPhrase, tagger, protoCandidate, prototype1);

		}

		private static List<PrototypeDefinition> LinkBagOfFeaturesToClinicalObject2(string strPhrase, ProtoScriptTagger tagger, Prototype protoCandidate, Prototype protoTagged)
		{
			DebugLog.WriteEvent("Linking BagOfFeatures to Clinical Object", strPhrase, DebugLog.debug_level_t.VERBOSE);

			List<PrototypeDefinition> lstPrototypeDefinitions = new List<PrototypeDefinition>();
			List<Prototype> lstFeatures = BagOfFeatures.GetFeatures(protoTagged);

			//every (important) token should have a sememe because we added it above, remove syntax elements
			lstFeatures = lstFeatures.Where(x => x.TypeOf("Sememe")).ToList();

			//Check if anything has changed
			//if (protoTagged.TypeOf(protoCandidate))
			//{
			//	DebugLog.WriteEvent("Already linked", protoTagged.PrototypeName);
			//	return lstPrototypeDefinitions; //done
			//}

			return InsertOrUpdateBagOfFeatures(strPhrase, tagger, protoCandidate, protoTagged);
		}
		
		private static List<PrototypeDefinition> InsertOrUpdateBagOfFeatures(string strPhrase, ProtoScriptTagger tagger, Prototype protoCandidate, Prototype protoTagged)
		{
			List<PrototypeDefinition> lstPrototypeDefinitions = new List<PrototypeDefinition>();

			//if it is fully tagged but not the candidate, this may be a an existing sequence. 
			//add the new linkage
			if (protoTagged.Children.Count == 0 && protoTagged.TypeOf("ClinicalOntology.ClinicalEntity"))
			{
				//This scenario can be ok, if we have additional features to use. In which case 
				//we just continue.
				DebugLog.WriteEvent("*** Warning ***", protoTagged.PrototypeName + " already linked to something else");				
			}

			List<Prototype> lstPrimaryFeatures = BagOfFeatures.GetPrimaryFeatures(protoTagged);

			if (lstPrimaryFeatures.Count == 1) // Not a sequence
			{
				Prototype protoSememe = lstPrimaryFeatures[0];
				if (!protoSememe.TypeOf(protoCandidate))
				{
					protoSememe.InsertTypeOf(protoCandidate);
					PrototypeDefinition protoDef = PrototypeDefinitionHelpers.PrototypeToPrototypeDefinition(protoSememe, false);

					//>log protoDef
					DebugLog.WriteEvent($"Linking {protoSememe.PrototypeName} -> {protoCandidate.PrototypeName}", "\n" + SimpleGenerator.Generate(protoDef));

					WriteNewPrototype(tagger, protoDef, "IntermediateLayer.pts");

					lstPrototypeDefinitions.Add(protoDef);
				}
			}
			else 
			{
				string strPrototypeName = Lexemes.ToPrototypeName(strPhrase);
				Prototype ? protoBagOfFeatures = TemporaryPrototypes.GetTemporaryPrototypeOrNull(strPrototypeName + "Sememe");

				if (null != protoBagOfFeatures)
				{
					//E20250525-01 - The ontology may have changed, rebuild the sequence if necessary
					bool bMissingFeatures = false;
					foreach (Prototype protoFeature in protoBagOfFeatures.Children)
					{
						if (!lstPrimaryFeatures.Any(x => x.TypeOf(protoFeature)))
						{
							bMissingFeatures = true;
						}
					}

					//Check for new features in the training data 
					foreach (Prototype protoFeature in lstPrimaryFeatures)
					{
						if (!protoBagOfFeatures.Children.Any(x => x.ShallowEqual(protoFeature)))
						{
							bMissingFeatures = true;
						}
					}

					if (bMissingFeatures)
					{
						protoBagOfFeatures = null;
					}
				}

				if (null == protoBagOfFeatures) //Don't add it again 
				{
					protoBagOfFeatures = TemporaryPrototypes.GetOrCreateTemporaryPrototype(strPrototypeName + "Sememe");
					protoBagOfFeatures.InsertTypeOf("Sememe");
					protoBagOfFeatures.InsertTypeOf("BagOfFeatures");
					protoBagOfFeatures.InsertTypeOf(protoCandidate);
					protoBagOfFeatures.RemoveTypeOf(Ontology.Sequence.Prototype); //Causes a compilation error if serialized, fix at some point
					protoBagOfFeatures.Children = new Ontology.Collection(lstPrimaryFeatures);

					PrototypeDefinition protoDef = PrototypeDefinitionHelpers.PrototypeToPrototypeDefinition(protoBagOfFeatures, false);
					protoDef.Annotations.Add(AnnotationExpressions.Parse("[BagOfFeatures.Setup()]"));

					DebugLog.WriteEvent($"Linking {protoBagOfFeatures.PrototypeName} -> {protoCandidate.PrototypeName}", "\n" + SimpleGenerator.Generate(protoDef));

					WriteNewPrototype(tagger, protoDef, "IntermediateLayer.pts");

					lstPrototypeDefinitions.Add(protoDef);
				}
				else
				{
					//We don't want to support this scenario right now
					if (protoBagOfFeatures.Children.Any(x => x.TypeOf("BagOfFeatures")))
						throw new Exception("Warning, BagOfFeatures already exists: " + protoBagOfFeatures.PrototypeName);

					DebugLog.WriteEvent("Already linked", protoBagOfFeatures.PrototypeName);
				}
			}

			return lstPrototypeDefinitions;
		}

		public static void WriteNewPrototypes(ProtoScriptTagger tagger, List<PrototypeDefinition> lstNewPrototypes, string strLocalFile)
		{
			string strProjectDirectory = StringUtil.LeftOfLast(tagger.ProjectFile, "\\");
			string strEnglishFile = FileUtil.BuildPath(strProjectDirectory, strLocalFile);

			//Running the code before writing it ensures it works
			foreach (PrototypeDefinition protoDef in lstNewPrototypes)
			{
				tagger.Interpretter.InterpretStatement(protoDef);
			}

			foreach (PrototypeDefinition protoDef in lstNewPrototypes)
			{
				PrototypeDefinitionHelpers.InsertOrUpdatePrototypeDefinition(protoDef, strEnglishFile);
			}


		}

		public static void WriteNewPrototype(ProtoScriptTagger tagger, PrototypeDefinition protoDef, string strLocalFile)
		{
			WriteNewPrototypes(tagger, new List<PrototypeDefinition> { protoDef }, strLocalFile);
		}
	}
}
