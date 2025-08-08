using BasicUtilities;
using Buffaly.AIProviderAPI;
using Buffaly.NLU;
using Buffaly.SemanticDB.Data;
using ProtoScript;
using ProtoScript.Parsers;

namespace Ontology.Agents.Language
{
	public class UnknownWords
	{
		static public async Task<List<PrototypeDefinition>> CreateUnknownWordAsPrototypes2(string strUnknownWord, string strDirective, 
			ProtoScriptTagger tagger, bool bAllowSpaces = false)
		{
			List<PrototypeDefinition> lstPrototypes = new List<PrototypeDefinition>();

			if (StringUtil.IsEmpty(strUnknownWord))
				throw new Exception("Unknown word cannot be empty");

			if (!bAllowSpaces && StringUtil.ContainsNonAlphanumericOrSpace(strUnknownWord))
				throw new Exception("Unknown word cannot contain non-alphanumeric characters");


			JsonObject jsonEntity = await GetUnknownWord2(strUnknownWord, strDirective);

			//{
			//  "Word": "<Original Word>",
			//  "Sentence": "<Example Usage>",
			//  "Type": "<Buffaly Prototype Category>",
			//  "WordNetPOS": "<noun/verb/adj/adv/etc. from WordNet>",
			//  "WordNetSynset": "<if available, e.g. 'run.v.01'>",
			//  "Singular": "<if Type=Object, else omit>",
			//  "Plural": "<if Type=Object, else omit>"
			//}

			//Types:
			//Action
			//Object
			//Qualifier
			//AdverbQualifier
			//Conjunction
			//Preposition
			//Interjection
			//State
			//Unknown
			//PersonalName
			//PlaceName
			//OrganizationName
			//PhoneNumber
			//EmailAddress
			//URL
			//IPAddress
			//Number
			//DateTime
			//SocialTag
			//LicenseKey
			//IDCode
			//Emoji

			string strSingular = jsonEntity.GetStringOrNull("Singular");
			string strPlural = jsonEntity.GetStringOrNull("Plural");
			string strType = jsonEntity.GetStringOrNull("Type");

			if (strType == "Unknown")
				return lstPrototypes;

			//check if this already exists 
			TemporaryLexeme ? rowLexeme = Lexemes.GetLexemeByLexeme(strUnknownWord);
			if (null != rowLexeme)
			{
				if (rowLexeme.LexemePrototypes.Any(x => Prototypes.TypeOf(x.Key, strType)))
				{
					//this word already exists
					return lstPrototypes;
				}
			}

			if (strType == "Object")
			{
				string strDefinition = await CreateObjectAsPrototype(jsonEntity);

				PrototypeDefinition protoDef = PrototypeDefinitions.Parse(strDefinition);
				//N20250419-03 - Moving forward let's always use Object as a suffix
				if (!protoDef.PrototypeName.TypeName.EndsWith("Object"))
				{
					protoDef.PrototypeName.TypeName += "Object";
				}

				//check the parents
				foreach (ProtoScript.Type type in protoDef.Inherits)
				{
					string strSafeName = type.TypeName;

					Prototype protoBase = TemporaryPrototypes.GetTemporaryPrototypeOrNull(strSafeName);
					if (null == protoBase)
					{
						lstPrototypes.AddRange(await CreateUnknownWordAsPrototypes2(type.TypeName, "the " + type.TypeName, tagger));
					}
					else if (Prototypes.TypeOf(protoBase, "Action"))
					{
						throw new Exception("Problem here");

						//we need to change it to Base+Object - usually manual process
					}

					type.TypeName = strSafeName;
				}

				lstPrototypes.Add(protoDef);
			}

			else if (new List<string> { "PlaceName", "PersonalName", "OrganizationName" }.Contains(strType))
			{
				string strDefinition = await CreateNameEntityAsPrototype(jsonEntity);

				PrototypeDefinition protoDef = PrototypeDefinitions.Parse(strDefinition);
				lstPrototypes.Add(protoDef);
			}

			else if (strType == "Action")
			{
				string strDefintions = await CreateActionAsPrototype(jsonEntity);
				
				ProtoScript.File file = Files.ParseFileContents(strDefintions);
				foreach (PrototypeDefinition protoDef in file.PrototypeDefinitions)
				{

					lstPrototypes.Add(protoDef);
				}

			}

			else if (strType == "AdverbQualifier")
			{
				string strDefintions = await CreateAdverbQualifierAsPrototype(jsonEntity);

				ProtoScript.File file = Files.ParseFileContents(strDefintions);
				foreach (PrototypeDefinition protoDef in file.PrototypeDefinitions)
				{
					if (!protoDef.PrototypeName.TypeName.EndsWith("AdverbQualifier"))
					{
						protoDef.PrototypeName.TypeName += "AdverbQualifier";
					}

					if (null == TemporaryPrototypes.GetTemporaryPrototypeOrNull(protoDef.Inherits.First().TypeName))
					{
						//We may just want to throw an error here
						lstPrototypes.AddRange(await CreateUnknownWordAsPrototypes2(protoDef.Inherits.First().TypeName, "the " + protoDef.Inherits.First().TypeName, tagger));
					}

					lstPrototypes.Add(protoDef);
				}
			}

			else if (strType == "Qualifier")
			{
				string strDefintions = await CreateAdjectiveQualifierAsPrototype(jsonEntity);

				ProtoScript.File file = Files.ParseFileContents(strDefintions);
				foreach (PrototypeDefinition protoDef in file.PrototypeDefinitions)
				{
					if (!protoDef.PrototypeName.TypeName.EndsWith("Qualifier"))
					{
						protoDef.PrototypeName.TypeName += "Qualifier";
					}

					if (null == TemporaryPrototypes.GetTemporaryPrototypeOrNull(protoDef.Inherits.First().TypeName))
					{
						//We may just want to throw an error here
						lstPrototypes.AddRange(await CreateUnknownWordAsPrototypes2(protoDef.Inherits.First().TypeName, "the " + protoDef.Inherits.First().TypeName, tagger));
					}


					lstPrototypes.Add(protoDef);
				}
			}

			else if (strType == "Abbreviation")
			{
				//	{
				//	"Word": "lol",
				//  "Sentence": "He replied with lol",
				//  "Type": "Abbreviation",
				//  "Meaning":"laugh out loud",
				//  "MeaningType": "Phrase"
				//}

				string strDefinition = await CreateAbbreviationAsPrototype(jsonEntity);
				PrototypeDefinition protoDef = PrototypeDefinitions.Parse(strDefinition);
				lstPrototypes.Add(protoDef);

				string strBaseType = protoDef.Inherits.First().TypeName;
				Prototype protoBase = TemporaryPrototypes.GetTemporaryPrototypeOrNull(strBaseType);
				if (null == protoBase)
					protoBase = TemporaryPrototypes.GetTemporaryPrototypeOrNull(strBaseType + "Object");
				if (null == protoBase)
					protoBase = Lexemes.GetLexemeByLexeme(strBaseType)?.LexemePrototypes.FirstOrDefault().Key;

				if (null == protoBase)
				{
					string strMeaningType = jsonEntity.GetStringOrNull("MeaningType");
					string strMeaning = jsonEntity.GetStringOrNull("Meaning");

					if (strMeaningType == "Object" || strMeaningType == "Action" || strMeaningType == "AdverbQualifier")
					{
						lstPrototypes.AddRange(await CreateUnknownWordAsPrototypes2(strMeaning, strDirective.Replace(strUnknownWord, strMeaning), tagger, true));
					}
					else if (strMeaningType == "Phrase" )
					{
						PrototypeDefinition protoBaseDef = PrototypeDefinitionHelpers.PrototypeToPrototypeDefinition(protoBase, false);
						lstPrototypes.Add(protoBaseDef);
					}


					else 
						throw new Exception($"Unknown MeaningType '{strMeaningType}' for '{strBaseType}'");

					protoDef.Inherits.First().TypeName = lstPrototypes.Last().PrototypeName.TypeName;
				}
			}

			if (lstPrototypes.Count == 0)
			{
				Logs.DebugLog.WriteEvent("Unknown Word", JsonUtil.ToFriendlyJSON(jsonEntity.ToJSON()));
			}


			return lstPrototypes;
		}



		static public async Task<JsonArray> GetIncorrectlyTaggedWords(string strSentence, Prototype protoTagged)
		{
			FragmentsRow rowFragment = FragmentsRepository.GetFragmentByFragmentKey("Rolled Up Sentence Verifier");

			string strInput = "Sentence: " + strSentence + "\n";
			strInput += "Structure: \n```\n" + PrototypeLogging.ToFriendlyShadowString(protoTagged) + "\n```";
			string strResult = await Completions.Complete(strInput, rowFragment.Fragment, ModelSize.LargeModel);
			
			
			if (strResult.Contains("```json"))
				strResult = StringUtil.Between(strResult, "```json", "```");


			return new JsonArray(strResult);
		}

		//call "Word Form Generator" fragment
		public static async Task<JsonObject> GetWordForms(string strWord)
		{
			FragmentsRow rowFragment = FragmentsRepository.GetFragmentByFragmentKey("Word Form Generator");
			JsonObject jsonResult = new JsonObject(await Completions.CompleteAsJSON($"{strWord}", rowFragment.Fragment, ModelSize.LargeModel));

			return jsonResult;		}


		private static async Task<JsonObject> GetUnknownWord2(string strWord, string strExample)
		{
			FragmentsRow rowFragment = FragmentsRepository.GetFragmentByFragmentKey("WordNet POS Tagger - 2");

			JsonObject jsonResult = new JsonObject(await Completions.CompleteAsJSON($"Word: {strWord}\nExample: {strExample}", rowFragment.Fragment, ModelSize.LargeModel));

			return jsonResult;
		}

		private static async Task<string> CreatePrototypeFromFragment(JsonObject jsonEntity, string fragmentKey)
		{
			FragmentsRow rowFragment = FragmentsRepository.GetFragmentByFragmentKey(fragmentKey);
			Logs.DebugLog.WriteEvent("Calling LLM Prompt", fragmentKey);
			string strResult = await Completions.Complete(jsonEntity.ToJSON(), rowFragment.Fragment, ModelSize.LargeModel);
			if (strResult.Contains("```"))
			{
				strResult = StringUtil.Between(strResult, "```", "```");
			}
			return strResult;
		}

		private static async Task<string> CreateAbbreviationAsPrototype(JsonObject jsonEntity)
		{
			return await CreatePrototypeFromFragment(jsonEntity, "Abbreviation Generator");
		}

		private static async Task<string> CreateObjectAsPrototype(JsonObject jsonEntity)
		{
			return await CreatePrototypeFromFragment(jsonEntity, "WordNet Noun Generator - 2");
		}

		private static async Task<string> CreateActionAsPrototype(JsonObject jsonEntity)
		{
			return await CreatePrototypeFromFragment(jsonEntity, "WordNet Verb Generator");
		}

		private static async Task<string> CreateNameEntityAsPrototype(JsonObject jsonEntity)
		{
			return await CreatePrototypeFromFragment(jsonEntity, "WordNet Named Entity Generator");
		}

		private static async Task<string> CreateAdverbQualifierAsPrototype(JsonObject jsonEntity)
		{
			return await CreatePrototypeFromFragment(jsonEntity, "WordNet Adverb Generator");
		}

		private static async Task<string> CreateAdjectiveQualifierAsPrototype(JsonObject jsonEntity)
		{
			return await CreatePrototypeFromFragment(jsonEntity, "WordNet Adjective Generator");
		}

		////Older format
		//static public Tuple<List<Prototype>, Prototype> CreateUnknownWordsAsTokens(string strPhrase)
		//{
		//	Prototype protoTokens = UnderstandUtil.Tokenize(strPhrase);
		//	Prototype protoLexemes = UnderstandUtil.ConvertToLexemes(protoTokens);

		//	List<Prototype> lstNewPrototypes = new List<Prototype>();

		//	for (int i = 0; i < protoLexemes.Children.Count; i++)
		//	{
		//		Prototype protoToken = protoLexemes.Children[i];
		//		if (Prototypes.TypeOf(protoToken, System_String.Prototype))
		//		{
		//			string strToken = new StringWrapper(protoToken).GetStringValue();
		//			var tuple = CreateUnknownWordAsToken(strToken);
		//			lstNewPrototypes.Add(tuple.Item1);
		//			protoLexemes.Children[i] = tuple.Item2;
		//		}
		//	}

		//	return new Tuple<List<Prototype>, Prototype>(lstNewPrototypes, protoLexemes);
		//}

		//static public List<Prototype> CreateUnknownWordAsPrototypes(string strUnknownWord, string strDirective, ProtoScriptTagger tagger)
		//{
		//	List<Prototype> lstPrototypes = new List<Prototype>();

		//	if (StringUtil.IsEmpty(strUnknownWord))
		//		throw new Exception("Unknown word cannot be empty");

		//	if (StringUtil.ContainsNonAlphanumericOrSpace(strUnknownWord))
		//		throw new Exception("Unknown word cannot contain non-alphanumeric characters");

		//	//This method does not work well with multi-token lexemes
		//	FragmentsRow rowFragment = Fragments.GetOrInsertFragment(strUnknownWord);
		//	FragmentTags.InsertOrUpdateFragmentTag(rowFragment.FragmentID, "Word");

		//	JsonObject jsonExtracted = GetUnknownWord(strUnknownWord, strDirective);

		//	rowFragment = Fragments.GetFragment(rowFragment.FragmentID);
		//	JsonObject jsonEntity = rowFragment.DataObject.GetJsonObjectOrDefault("ExtractedData");

		//	foreach (string strKey in jsonExtracted.Keys)
		//	{
		//		jsonEntity[strKey] = jsonExtracted[strKey];
		//	}

		//	rowFragment.DataObject["ExtractedData"] = jsonEntity;
		//	FragmentsRepository.UpdateFragmentData(rowFragment);

		//	//{
		//	//	"Categories": [

		//	//		"Other"
		//	//    ],
		//	//    "Plural": "representatives",
		//	//    "Sentence": "The representatives from each department met to discuss the project.",
		//	//    "Singular": "representative",
		//	//    "SuggestedCategories": [],
		//	//    "Timestamp": 638676106546748000,
		//	//    "Type": "Object",
		//	//    "Word": "representatives"
		//	//}

		//	string strSingular = jsonEntity.GetStringOrNull("Singular");
		//	string strPlural = jsonEntity.GetStringOrNull("Plural");
		//	string strType = jsonEntity.GetStringOrNull("Type");
		//	JsonArray jsonCategories = jsonEntity.GetJsonArrayOrDefault("Categories");

		//	if (strType == "Object")
		//	{
		//		lstPrototypes.Add(CreateSingularPluralObjectAsPrototype(strSingular, strPlural, jsonCategories, tagger));
		//	}


		//	if (strType == "Action")
		//	{

		//		DataExtractorsRow rowActionExtractor = DataExtractors.GetDataExtractorByName("Action Word Extractor");
		//		DataExtractors.ExtractJSONIfUpdated(rowFragment.FragmentID, new DataExtractor(rowActionExtractor), new JsonObject());
		//		rowFragment = Fragments.GetFragment(rowFragment.FragmentID);
		//		JsonObject jsonAction = rowFragment.DataObject.GetJsonObjectOrDefault("ExtractedData");

		//		//{
		//		//		"Word": "run",
		//		//		"Sentence": "She decided to run every morning.",
		//		//		"Type": "Action",
		//		//		"BaseForm": "run",
		//		//		"PastTense": "ran",
		//		//		"ThirdPersonSingular": "runs",
		//		//		"PastParticiple": "run",
		//		//		"Gerund": "running"
		//		//}

		//		string strBaseForm = jsonAction.GetStringOrNull("BaseForm");
		//		string strPastTense = jsonAction.GetStringOrNull("PastTense");
		//		string strPastParticiple = jsonAction.GetStringOrNull("PastParticiple");
		//		string strThirdPersonSingular = jsonAction.GetStringOrNull("ThirdPersonSingular");
		//		string strGerund = jsonAction.GetStringOrNull("Gerund");

		//		ProtoScript.Tagging.GeneratorUtil.VerbForms forms = new ProtoScript.Tagging.GeneratorUtil.VerbForms();
		//		forms.NakedInfinitive = StringUtil.UppercaseFirstLetter(strBaseForm);
		//		forms.EdVerb = StringUtil.UppercaseFirstLetter(strPastTense);
		//		forms.SVerb = StringUtil.UppercaseFirstLetter(strThirdPersonSingular);
		//		forms.IngVerb = StringUtil.UppercaseFirstLetter(strGerund);
		//		forms.Infinitive = "To" + StringUtil.UppercaseFirstLetter(strBaseForm);

		//		//Note: This version doesn't set the Infinitive correctly
		//		Prototype protoBase = TemporaryPrototypes.GetOrCreateTemporaryPrototype(forms.NakedInfinitive + "Base");
		//		TypeOfs.Insert(protoBase, "AnimateAction");     //TODO: This needs to be intelligently set
		//		lstPrototypes.Add(protoBase);

		//		Prototype protoInfinitive = TemporaryPrototypes.GetOrCreateTemporaryPrototype(forms.Infinitive);
		//		TypeOfs.Insert(protoInfinitive, forms.NakedInfinitive + "Base");
		//		TypeOfs.Insert(protoInfinitive, "Infinitive");
		//		lstPrototypes.Add(protoInfinitive);

		//		Prototype protoNakedInfinitive = TemporaryPrototypes.GetOrCreateTemporaryPrototype(forms.NakedInfinitive);
		//		TypeOfs.Insert(protoNakedInfinitive, forms.NakedInfinitive + "Base");
		//		TypeOfs.Insert(protoNakedInfinitive, "NakedInfinitive");
		//		TemporaryLexemes.GetOrInsertLexeme(forms.NakedInfinitive.ToLower(), protoNakedInfinitive);
		//		protoNakedInfinitive.Properties["NakedInfinitive.Field.Infinitive"] = protoInfinitive;
		//		lstPrototypes.Add(protoNakedInfinitive);

		//		Prototype protoEdVerb = TemporaryPrototypes.GetOrCreateTemporaryPrototype(forms.EdVerb);
		//		TypeOfs.Insert(protoEdVerb, forms.NakedInfinitive + "Base");
		//		TypeOfs.Insert(protoEdVerb, "EdVerb");
		//		TemporaryLexemes.GetOrInsertLexeme(forms.EdVerb.ToLower(), protoEdVerb);
		//		lstPrototypes.Add(protoEdVerb);

		//		Prototype protoSVerb = TemporaryPrototypes.GetOrCreateTemporaryPrototype(forms.SVerb);
		//		TypeOfs.Insert(protoSVerb, forms.NakedInfinitive + "Base");
		//		TypeOfs.Insert(protoSVerb, "SVerb");
		//		TemporaryLexemes.GetOrInsertLexeme(forms.SVerb.ToLower(), protoSVerb);
		//		lstPrototypes.Add(protoSVerb);

		//		Prototype protoIngVerb = TemporaryPrototypes.GetOrCreateTemporaryPrototype(forms.IngVerb);
		//		TypeOfs.Insert(protoIngVerb, forms.NakedInfinitive + "Base");
		//		TypeOfs.Insert(protoIngVerb, "IngVerb");
		//		TemporaryLexemes.GetOrInsertLexeme(forms.IngVerb.ToLower(), protoIngVerb);
		//		lstPrototypes.Add(protoIngVerb);
		//	}

		//	if (strType == "Unknown")
		//	{
		//		//"{\n  \"Word\": \"DebtX\",\n  \"Sentence\": \"DebtX\",\n  \"Type\": \"Unknown\"\n}",
		//		if (StringUtil.IsEmpty(strSingular))
		//		{
		//			strSingular = strUnknownWord;
		//		}

		//		lstPrototypes.Add(CreateUnknownWordAsToken(strSingular).Item1);
		//	}

		//	return lstPrototypes;


		//}

		//private static Tuple<Prototype, Prototype>  CreateUnknownWordAsToken(string strSingular)
		//{
		//	return LexemeUtil.CreateUnknownWordAsToken(strSingular);
		//}

		//public static JsonObject GetUnknownWord(string strWord, string strExample)
		//{
		//	DataExtractorsRow rowWordExtractor = DataExtractors.GetDataExtractorByName("Unknown Entity Analyzer");
		//	string strJson = DataExtractors.EvaluateDataExtractor(rowWordExtractor.DataExtractorID, $"Original Word: {strWord}\nExample Sentence: {strExample}");
		//	return new JsonObject(strJson);
		//}

		//static public Prototype CreateSingularPluralObjectAsPrototype(string strSingular, string strPlural,
		//		JsonArray jsonCategories,
		//		ProtoScriptTagger tagger)
		//{
		//	//This method will create or modify the singular and plural lexemes to tag as an object, in memory
		//	LexemesRow rowLexemeSingular = Lexemes.GetLexemeByLexeme(strSingular);
		//	LexemesRow rowLexemePlural = Lexemes.GetLexemeByLexeme(strPlural);

		//	Prototype protoRelatedSingular = LexemeUtil.GetRelatedEntityAsObjectOrToken(strSingular);
		//	if (null == protoRelatedSingular)
		//	{
		//		protoRelatedSingular = TemporaryPrototypes.GetOrCreateTemporaryPrototype(StringUtil.UppercaseFirstLetter(strSingular));
		//		TypeOfs.Insert(protoRelatedSingular, "BaseObject");
		//		TypeOfs.Insert(protoRelatedSingular, "Token");

		//		TemporaryLexemes.GetOrInsertLexeme(strSingular, protoRelatedSingular);
		//	}
		//	else if (!Prototypes.TypeOf(protoRelatedSingular, "BaseObject"))
		//	{
		//		protoRelatedSingular = TemporaryPrototypes.GetTemporaryPrototypeOrNull(protoRelatedSingular.PrototypeID);
		//		TypeOfs.Insert(protoRelatedSingular, "BaseObject");
		//	}

		//	if (null == rowLexemePlural || rowLexemePlural.LexemeID != rowLexemeSingular?.LexemeID)
		//	{
		//		tagger.Interpretter.RunMethodAsPrototype("Lexeme", "Plural", new List<object> { protoRelatedSingular, strPlural });
		//	}

		//	foreach (JsonValue jsonValue in jsonCategories)
		//	{
		//		string strCategory = jsonValue.ToString();
		//		if (!StringUtil.EqualNoCase(strCategory, "Other"))
		//		{
		//			TypeOfs.Insert(protoRelatedSingular, strCategory);
		//		}
		//	}

		//	if (null != TemporaryPrototypes.GetTemporaryPrototypeOrNull("UnknownLexeme"))
		//		TypeOfs.Remove(protoRelatedSingular, "UnknownLexeme");

		//	return protoRelatedSingular;
		//}


	}
}
