using BasicUtilities;
using BasicUtilities.Collections;
using Buffaly.NLU;
using Buffaly.SemanticDB;
using Buffaly.SemanticDB.Data;
using Ontology.Agents.RAG_SQL;
using Ontology.Agents.DevelopmentAgent;
using Ontology.Agents.Actions;
using Ontology.Simulation;
using ProtoScript.Interpretter.RuntimeInfo;
using System.Text;
using System.Threading.Tasks;

namespace Ontology.Agents
{
	public class ImplementDirectiveResult
	{
		public JsonObject Result = new JsonObject();
		public string Prompt = "";
	}

	public class DirectiveRequest
	{
		public string ApplicationName = "FeedingFrenzy";
		public string Directive = "";

		public SessionObject Session;

		public DirectiveRequest(string strDirective, SessionObject session)
		{
			this.Directive = strDirective;
			this.Tagger = session.Tagger;
			this.Session = session;
		}

		private FragmentsRow m_Fragment = null;
		public FragmentsRow Fragment
		{
			get
			{
				if (null == m_Fragment)
				{
					m_Fragment = Fragments.GetOrInsertFragment(Directive);
					FragmentTags.InsertOrUpdateFragmentTag(m_Fragment.FragmentID, "Directive");
				}

				return m_Fragment;
			}
		}

		public bool AllowLLMEntitySelection = true;

		public JsonObject RequestValues = new JsonObject();
		public Prototype Modality = null;
		public bool IsModalityFixed = false;

		public string GeneralizedSearchString = "";
		public string AugmentedEntities = "";

		public List<string> ExampleDirectives = new List<string>();

		public List<Prototype> Entities = new List<Prototype>();

		public Prototype KnownEntity = null;


		public List<Prototype> CandidateActions = new List<Prototype>();

		public ProtoScriptTagger Tagger = null;


		public Prototype Understanding = null;



		public List<Prototype> OntologyEntities = new List<Prototype>();
		public List<Prototype> SemanticEntities = new List<Prototype>();
		public List<Prototype> ActivatedEntities = new List<Prototype>();
		public List<Prototype> LLMSelectedEntities = new List<Prototype>();

		public List<Prototype> DirectiveEntities
		{
			get
			{
				List<Prototype> lstPrototypes = new List<Prototype>();
				HashSet<int> seenPrototypeIds = new HashSet<int>(); // Keep track of seen IDs

				if (null != KnownEntity && seenPrototypeIds.Add(KnownEntity.PrototypeID))
				{
					lstPrototypes.Add(KnownEntity);
				}

				// LLM Selected first
				foreach (var entity in LLMSelectedEntities)
				{
					if (seenPrototypeIds.Add(entity.PrototypeID))
					{
						lstPrototypes.Add(entity);
					}
				}

				// Then semantic
				foreach (var entity in SemanticEntities)
				{
					if (seenPrototypeIds.Add(entity.PrototypeID))
					{
						lstPrototypes.Add(entity);
					}
				}

				// Then ontology
				foreach (var entity in OntologyEntities)
				{
					if (seenPrototypeIds.Add(entity.PrototypeID))
					{
						lstPrototypes.Add(entity);
					}
				}

				// I don't think these are useful by themselves.
				if (lstPrototypes.Count == 0)
				{
					foreach (var entity in ActivatedEntities)
					{
						if (seenPrototypeIds.Add(entity.PrototypeID))
						{
							lstPrototypes.Add(entity);
						}
					}
				}

				if (IsModalityFixed && null != Modality)
				{
					lstPrototypes = lstPrototypes.Where(x => x.TypeOf(Modality)).ToList();
				}

				return lstPrototypes;
			}
		}

		public JsonObject ToJSON()
		{
			JsonObject jsonObject = new JsonObject();

			jsonObject["Directive"] = Directive;
			jsonObject["GeneralizedSearchString"] = GeneralizedSearchString;
			jsonObject["ExampleDirectives"] = new JsonArray(ExampleDirectives);
			jsonObject["Modality"] = Modality?.PrototypeName ?? null;
			jsonObject["Entities"] = new JsonArray(Entities.Select(x => x.ToFriendlyJsonObject()));
			jsonObject["DirectiveEntities"] = new JsonArray(DirectiveEntities.Select(x => x.PrototypeName).ToList());
			jsonObject["CandidateActions"] = new JsonArray(CandidateActions.Select(x => x.PrototypeName + " (" + x.Value + ")").ToList());
			jsonObject["OntologyEntities"] = new JsonArray(OntologyEntities.Select(x => x.PrototypeName).ToList());
			jsonObject["SemanticEntities"] = new JsonArray(SemanticEntities.Select(x => x.PrototypeName).ToList());
			jsonObject["ActivatedEntities"] = new JsonArray(ActivatedEntities.Select(x => x.PrototypeName).ToList());
			jsonObject["KnownEntity"] = KnownEntity?.PrototypeName ?? null;
			jsonObject["Understanding"] = PrototypeLogging.ToFriendlyShadowString(Understanding).ToString();
			jsonObject["AugmentedEntities"] = AugmentedEntities;
			jsonObject["LLMSelectedEntities"] = new JsonArray(LLMSelectedEntities.Select(x => x.PrototypeName).ToList());

			return jsonObject;
		}

	}

	public static class FragmentsRowExtensions
	{
		// Gets the Prototype from the FragmentsRow
		public static Prototype GetPrototype(this FragmentsRow rowFragment)
		{
			string strPrototypeName = rowFragment.DataObject.GetStringOrNull("PrototypeName");

			if (StringUtil.IsEmpty(strPrototypeName))
				return null;

			Prototype protoKnownEntity = TemporaryPrototypes.GetTemporaryPrototypeOrNull(strPrototypeName);
			if (null != protoKnownEntity)
			{
				//If we do anything with prototype, including setting the value, we must do it on a clone 
				//or it will overwrite the prototype in the cache
				protoKnownEntity = protoKnownEntity.Clone();
				protoKnownEntity.Value = rowFragment.DataObject.GetDoubleOrDefault("Similarity", 1);
			}

			return protoKnownEntity;
		}

		// Sets the Prototype on the FragmentsRow
		public static void SetPrototype(this FragmentsRow row, Prototype prototype)
		{
			row.DataObject["PrototypeName"] = prototype.PrototypeName;
		}

		public static Prototype? GetPrototype(this FragmentsRow rowFragment, string strPrototype)
		{
			string strPrototypeName = rowFragment.DataObject.GetStringOrNull($"{strPrototype}.PrototypeName");

			if (StringUtil.IsEmpty(strPrototypeName))
				return null;

			Prototype protoKnownEntity = TemporaryPrototypes.GetTemporaryPrototypeOrNull(strPrototypeName);
			if (null != protoKnownEntity)
			{
				//If we do anything with prototype, including setting the value, we must do it on a clone 
				//or it will overwrite the prototype in the cache
				protoKnownEntity = protoKnownEntity.Clone();
				protoKnownEntity.Value = rowFragment.DataObject.GetDoubleOrDefault("Similarity", 1);
			}

			return protoKnownEntity;
		}

		// Sets the Prototype on the FragmentsRow
		public static void SetPrototype(this FragmentsRow row, string strPrototype, Prototype prototype)
		{
			row.DataObject[$"{strPrototype}.PrototypeName"] = prototype.PrototypeName;
		}
	}

	public class PipelineBase
	{
		public static async Task UnderstandDirective(DirectiveRequest request)
		{
			const int TotalMatches = 15;

			//Adding augmented entities to the search string to increase the chances of a match
			request.GeneralizedSearchString = BuildGeneralizedSearchString(request.Directive, request, request.Tagger);

			request.CandidateActions = await GetCandidateActionsBySemanticSearch(request, TotalMatches);

			request.ExampleDirectives = await GetExampleDirectivesBySemanticSearch(request.Directive);
		}


		public static async Task UnderstandEntities(DirectiveRequest request)
		{
			bool bIsKnownEntity = request.Fragment.FragmentTagTags.Any(x => x.TagName == "Entity");
			if (bIsKnownEntity)
			{
				Prototype protoKnownEntity = request.Fragment.GetPrototype();

				if (null != protoKnownEntity)
				{
					//Allow this to fall through and add associations as well 
					List<Prototype> lstEntities = new List<Prototype>();

					lstEntities.Add(protoKnownEntity);
					lstEntities.AddRange(protoKnownEntity.Associations.Select(x => x.Key));
					request.Entities = SelectEntities(lstEntities);
					request.KnownEntity = request.Entities.FirstOrDefault();
				}
			}

			//I can attempt to tag it
			request.Understanding = GetOntologyUnderstanding(request.Directive, request.Tagger);
			request.OntologyEntities = GetOntologyEntities(request.Understanding).ToList();

			//I can use associations to find one or more entities
			request.ActivatedEntities = Entities.RecognizeEntitiesViaActivationSpreading(request.Directive, request.Tagger).Where(x => x.Value >= 1).ToList();

			//Look for an unambiguous partial like "Justin" in "Justin Brochetti"
			if (null == request.KnownEntity && Prototypes.TypeOf(request.Understanding, "BaseObject"))
			{
				request.Entities = SelectEntities(request.ActivatedEntities);
				request.KnownEntity = request.Entities.FirstOrDefault();
			}

			//I can use semantic search to find entities 
			request.SemanticEntities = await Entities.GetCandidateEntitiesBySemanticSearch(request.Directive, 0.4);

			StringBuilder sbEntities = new StringBuilder();
			var mapEntities = PipelineBase.AugmentWithEntities2(request.SemanticEntities, request.Tagger);
			HashSet<string> setLines = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (Prototype protoEntity in request.SemanticEntities)
			{
				string strEntityName = new StringWrapper(protoEntity.Properties["LearnedEntity.Field.EntityName"]).GetStringValue();

				if (mapEntities.ContainsKey(protoEntity.PrototypeName))
				{
					foreach (string strBaseType in mapEntities[protoEntity.PrototypeName])
					{
						string strLine = $"'{strEntityName}' is a {strBaseType}";
						if (setLines.Add(strLine)) // Add returns true if the item was successfully added
						{
							sbEntities.AppendLine(strLine);
						}
					}

				}
			}

			request.AugmentedEntities = sbEntities.ToString();

			if (request.AllowLLMEntitySelection && request.KnownEntity == null && request.SemanticEntities.Count > 1)
			{
				request.LLMSelectedEntities = await GetLLMSelectedEntities(request);
			}

			//Entities is the ultimate decider 
			if (request.Entities.Count == 0)
			{
				request.Entities = SelectEntities(request.DirectiveEntities);
			}

			if (!request.IsModalityFixed)
				request.Modality = request.Entities.FirstOrDefault();
		}

		public static List<Prototype> SelectEntities(List<Prototype> lstEntities)
		{
			return lstEntities.Where(x => x.TypeOf("LearnedEntity") || x.TypeOf("Table")).ToList();
		}
		private static async Task<List<Prototype>> GetLLMSelectedEntities(DirectiveRequest request)
		{

			//We stick to SemanticEntities because they have a descriptive string (because they come from a Tag)
			StringBuilder sbEntities = new StringBuilder();
			var mapEntities = PipelineBase.AugmentWithEntities2(request.SemanticEntities, request.Tagger);
			HashSet<string> setLines = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (Prototype protoEntity in request.SemanticEntities)
			{
				string strEntityName = new StringWrapper(protoEntity.Properties["LearnedEntity.Field.EntityName"]).GetStringValue();

				if (mapEntities.ContainsKey(protoEntity.PrototypeName))
				{
					foreach (string strBaseType in mapEntities[protoEntity.PrototypeName])
					{
						string strLine = $"* '{protoEntity.PrototypeName}' : '{strEntityName}' is a {strBaseType}";
						if (setLines.Add(strLine)) // Add returns true if the item was successfully added
						{
							sbEntities.AppendLine(strLine);
						}
					}
				}
			}

			string strEntities = sbEntities.ToString();

			string strPrompt = LLMs.CreateLLMPrompt(@"
You are an AI assistant helping a user find relevant information. 

You will receive a user's request and a list of possible objects related to that request. 

Your task is to analyze the request and choose the object(s) that are most relevant to what the user is asking for.

The objects will be presented in the following format:

* 'PrototypeName' : 'object name' is a type

If none of the objects seem relevant, return an empty array: `[]`.

Return your answer as a JSON array containing the 'PrototypeName' of the selected objects, like this:

{""SelectedObjects"": [""PrototypeName1"", ""PrototypeName2""]} 

For example:

{""SelectedObjects"": [""MyDocument"", ""MyContact""]}
", new Collection(), "{SelectedObjects:[object]}");

			string strInput = $@"
# User's Request:
    {request.Directive}

# Possible Objects:
{strEntities}";

			JsonObject jsonResult = await LLMs.ExecuteLLMPromptAndInput(strPrompt, strInput);
			List<Prototype> lstLLMEntities = new List<Prototype>();

			foreach (string strSelectedEntity in jsonResult.GetJsonArrayOrDefault("SelectedObjects").ToStringList())
			{
                                Prototype? protoEntity = request.SemanticEntities.FirstOrDefault(x => x.PrototypeName == strSelectedEntity);
				if (Prototypes.TypeOf(protoEntity, "LearnedEntity"))
					lstLLMEntities.Add(protoEntity);
				else
				{
					foreach (var pair in protoEntity.Associations)
					{
						if (Prototypes.TypeOf(pair.Key, "LearnedEntity"))
							lstLLMEntities.Add(pair.Key);
					}
				}
			}

			return lstLLMEntities;

		}

		private static string BuildGeneralizedSearchString(string strSimplifiedSentence, DirectiveRequest request, ProtoScriptTagger tagger)
		{
			//N20241217-01 - This format gives the best results:
			//Summarize (sales representative)  Justin Brochetti's activities from (day)  yesterday 
                        Prototype? protoEntityInstance = request.Entities.FirstOrDefault(x => Prototypes.TypeOf(x, request.Modality));
			if (null == protoEntityInstance)
			{
				return strSimplifiedSentence;
			}

			string strGeneralizedSearchString = Entities.GeneralizeEntityViaActivationSpreading(request.Directive, protoEntityInstance, request.Tagger);

			Map<string, List<string>> mapEntities = AugmentWithEntities2(request.Entities, tagger);

			foreach (var pair in mapEntities)
			{
				if (StringUtil.InString(strGeneralizedSearchString, pair.Key))
				{
					//insert the pair.Value before the location of pair.Key in strGeneralizedSearchString
					int iIndex = strGeneralizedSearchString.IndexOf(pair.Key, StringComparison.InvariantCultureIgnoreCase);
					strGeneralizedSearchString = strGeneralizedSearchString.Remove(iIndex, pair.Key.Length);
					strGeneralizedSearchString = strGeneralizedSearchString.Insert(iIndex, string.Join(" ", pair.Value));
				}
			}

			return strGeneralizedSearchString;
		}


		private static async Task<List<Prototype>> GetCandidateActionsBySemanticSearch(DirectiveRequest request,
			int TotalMatches
			)
		{
			ProtoScriptTagger tagger = request.Tagger;
			string strSimplifiedSentence = request.GeneralizedSearchString;
			Prototype protoTargetModality = request.Modality;


			//Get the actions 
			//Vector embeddings change with characters like ? and carriage returns
			strSimplifiedSentence = new string(strSimplifiedSentence.Where(c => !char.IsPunctuation(c) && c != '\r' && c != '\n').ToArray());

			Buffaly.SemanticDB.Data.FragmentsDataTable dtFragments = await Buffaly.SemanticDB.Fragments.GetMostSimilar2ByTagID(strSimplifiedSentence, "Action", 0.60);
			FastPrototypeSet lstCandidates = new FastPrototypeSet();

			int iCount = 0;
			int iFirstN = 0;

			foreach (FragmentsRow rowFragmentAction in dtFragments)
			{
				if (Prototypes.TypeOf(protoTargetModality, "LearnedEntity"))
				{
					Prototype protoMethod = rowFragmentAction.GetPrototype();
					if (null == protoMethod)
						continue;

					protoMethod = tagger.Interpretter.NewInstance(protoMethod);
					protoMethod.Value = rowFragmentAction.DataObject.GetDoubleOrDefault("Similarity", 0.0);

					Prototype protoModality = protoMethod.Properties["LearnedAction.Field.Modality"];
					if (!protoTargetModality.ShallowEquivalent(protoModality))
						continue;

					//Must have a signature of Execute with 1 parameter
					FunctionRuntimeInfo info = tagger.Interpretter.FindOverriddenMethod(protoMethod, "Execute");
					if (null == info || info.Parameters.Count != 1)
						continue;

					if (!lstCandidates.Any(x => x.ShallowEquivalent(protoMethod)))
						lstCandidates.Add(protoMethod);
				}

				else if (Prototypes.TypeOf(protoTargetModality, "Table"))
				{
                                        ArtifactsRow? rowArtifact = rowFragmentAction.Artifacts.FirstOrDefault();
                                        if (null == rowArtifact)
                                                continue;

					if (rowFragmentAction.FragmentTagTags.Any(x => x.TagName == "Stored Procedure"))
					{
						string strStoredProcedure = rowArtifact.Name;
						Prototype protoStoredProcedure = tagger.Interpretter.NewInstance("SQL." + request.ApplicationName + "." + strStoredProcedure);
						Prototype protoTargets = protoStoredProcedure.Properties["LearnedAction.Field.Targets"];


						bool bUnexpected = false;
						//Simple now, check if the Target is unexpected
						foreach (Prototype protoParget in protoTargets.Children)
						{
							if (!request.Entities.Any(x => x.TypeOf(protoParget)))
							{
								bUnexpected = true;
								continue;
							}
						}

						const int TakeTopNSemanticMatches = 5;
						//Take the top N semantic matches no matter what
						if (++iFirstN >= TakeTopNSemanticMatches && bUnexpected)
						{
							continue;
						}

						lstCandidates.Add(protoStoredProcedure.ShallowClone());
					}

				}


				if (++iCount >= TotalMatches)
				{
					break;
				}
			}

			return lstCandidates.OrderByDescending(x => x.Value).ToList();
		}

		private static async Task<List<string>> GetExampleDirectivesBySemanticSearch(string strSimplifiedSentence)
		{
			//Get the actions 
			//Vector embeddings change with characters like ? and carriage returns
			strSimplifiedSentence = new string(strSimplifiedSentence.Where(c => !char.IsPunctuation(c) && c != '\r' && c != '\n').ToArray());

			//We use a higher threshold here 
			Buffaly.SemanticDB.Data.FragmentsDataTable dtFragments = await Buffaly.SemanticDB.Fragments.GetMostSimilar2ByTagID(strSimplifiedSentence, "Directive - Example Fulfillments", 0.70);


			List<string> lstCandidates = new List<string>();

			foreach (FragmentsRow rowFragmentAction in dtFragments)
			{
				if (rowFragmentAction.Fragments.Count == 0)
					continue;

				StringBuilder sb = new StringBuilder();

				sb.Append("Original Instruction: ");
				sb.AppendLine(rowFragmentAction.Fragment);
				sb.AppendLine("Examples:");

				foreach (FragmentsRow rowFragment in rowFragmentAction.Fragments)
				{
					sb.AppendLine(rowFragment.Fragment);
				}

				lstCandidates.Add(sb.ToString());

				if (lstCandidates.Count > 2)
					break;
			}

			return lstCandidates;
		}


		public static async Task<ImplementDirectiveResult> ImplementDirective(DirectiveRequest request)
		{
                        Prototype? protoFirstAction = request.CandidateActions.FirstOrDefault();
			if (null == protoFirstAction)
				throw new Exception("No action found"); // eventually this should become part of a meta-routine

			if (Prototypes.TypeOf(protoFirstAction, "StoredProcedure"))
			{
				//Collection colSpActions = new Collection(request.CandidateActions.Where(x => Prototypes.TypeOf(x, "StoredProcedure")));
				//return await ImplementDirectiveViaStoredProcedure(rowFragment, tagger, jsonExtractedData, colSpActions);
			}

			if (Prototypes.TypeOf(protoFirstAction, "JavaScriptMethod"))
			{
				//Collection colJsActions = new Collection(request.CandidateActions.Where(x => Prototypes.TypeOf(x, "JavaScriptMethod")));
				//return await ImplementDirectiveViaJavaScript(rowFragment, tagger, jsonExtractedData, colJsActions);
			}

			if (Prototypes.TypeOf(protoFirstAction, "ProtoScriptAction"))
			{
				Collection colPtsActions = new Collection(request.CandidateActions.Where(x => Prototypes.TypeOf(x, "ProtoScriptAction")));
				return await ImplementDirectiveViaProtoScript(request, colPtsActions);
			}


			throw new Exception("No valid action was identified to fulfill this directive");
		}


		public static async Task<ImplementDirectiveResult> BindProtoScriptToDirective(string strInput,
			ProtoScriptTagger tagger, JsonObject jsonExtractedData, Collection colActions)
		{
			StringBuilder sb = new StringBuilder();


			foreach (Prototype protoValue in colActions.Children)
			{
				Prototype protoAction = tagger.Interpretter.NewInstance(protoValue);

				string strMethodName = new StringWrapper(protoAction.Properties["ProtoScriptAction.Field.Name"]).GetStringValue();
				string strSignature = new StringWrapper(protoAction.Properties["ProtoScriptAction.Field.Signature"]).GetStringValue();
				string strDescription = new StringWrapper(protoAction.Properties["ProtoScriptAction.Field.Description"]).GetStringValue();

				sb.AppendLine(strMethodName);
				sb.AppendLine(strSignature);
				sb.AppendLine(strDescription);

				sb.AppendLine();
			}


			JsonObject jsonValues = new JsonObject();
			jsonValues["Methods"] = sb.ToString();
			jsonValues["AugmentedEntities"] = jsonExtractedData.GetStringOrNull("AugmentedEntities");


			var tuple = await ProtoScriptAgent.GetSimpleGeneratedCall(strInput, jsonValues);

			Logs.DebugLog.WriteEvent("Select Action Prompt", tuple.Item2);
			Logs.DebugLog.WriteEvent("Select Action Result", tuple.Item1);

			JsonObject jsonResult = new JsonObject(tuple.Item1);
			ImplementDirectiveResult result = new ImplementDirectiveResult();

			result.Result = jsonResult;
			result.Prompt = tuple.Item2;
			result.Result["ResultType"] = "ProtoScript";

			return result;
		}

		public static async Task<ImplementDirectiveResult> ImplementDirectiveViaProtoScript(DirectiveRequest request, Collection colActions)
		{
			ProtoScriptTagger tagger = request.Tagger;

                        Prototype? protoAction = null;

			//If there is not a clear best action, use an LLM selection
			if (colActions.Count > 1 && colActions.Children.First().Value < 0.75)
				protoAction = await ProtoScriptAgent.SelectBestAction(request, colActions);
			else
                                protoAction = colActions.FirstOrDefault();

			if (null == protoAction)
				return null;


			Prototype protoSources = protoAction.Properties["LearnedAction.Field.Sources"];
			List<object> lstParameters = new List<object>();

			if (null != protoSources)
			{
				foreach (Prototype protoSource in protoSources.Children)
				{
                                        Prototype? protoParameter = request.DirectiveEntities.FirstOrDefault(x => x.TypeOf(protoSource));
					if (null == protoParameter)
						protoParameter = request.Entities.FirstOrDefault(x => x.TypeOf(protoSource));

					if (null != protoParameter)
						lstParameters.Add(protoParameter);
				}
			}

			object oResult = tagger.Interpretter.RunMethodAsObject(protoAction, "Execute", lstParameters);

			ImplementDirectiveResult result = new ImplementDirectiveResult();
			if (oResult is JsonObject)
			{
				result.Result = (JsonObject)oResult;
				result.Result["ResultType"] = "JSON";
			}

			else if (tagger.Interpretter.GetOrConvertToPrototype(oResult) is Prototype protoResult)
			{
				result.Result["ProtoScriptResult"] = protoResult.ToFriendlyJsonObject();
				result.Result["ResultType"] = "ProtoScript Result";
			}

			else
			{
				throw new Exception("Don't know how to handle result + " + (null == oResult ? "(null)" : oResult.ToString()));
			}


			return result;

		}




		public static Prototype GetOntologyUnderstanding(string strSentence, ProtoScriptTagger tagger, int iMaxIterations = 10)
		{
			int iIterations = tagger.MaxIterations;
			bool bTagIteratively = tagger.TagIteratively;
			tagger.MaxIterations = iMaxIterations;
			tagger.TagIteratively = true;
			Prototype protoResult = UnderstandUtil.Understand3(strSentence, tagger);

			tagger.MaxIterations = iIterations;
			tagger.TagIteratively = bTagIteratively;

			//Iterative tagging can return nested collections
			while (Prototypes.TypeOf(protoResult, Ontology.Collection.Prototype) && protoResult.Children.Count == 1)
				protoResult = protoResult.Children[0];

			return protoResult;
		}

		public static FastPrototypeSet GetOntologyEntities(Prototype protoUnderstanding)
		{
			FastPrototypeSet lstResults = new FastPrototypeSet();

			Set<int> setHashes = new Set<int>();

			PrototypeGraphs.BreadthFirstWithControlOnNormal(protoUnderstanding, x =>
			{
				if (setHashes.Contains(x.GetHashCode()))
					return null;

				setHashes.Add(x.GetHashCode());

				if (Prototypes.TypeOf(x, "LearnedEntity") ||
					Prototypes.TypeOf(x, "Table"))
				{
					lstResults.Add(x.Clone());      //preserve properties
					return null;                    //These types are the target, don't continue
				}
				if (Prototypes.TypeOf(x, "Object"))
				{
					lstResults.Add(x.Clone());      //preserve properties
													//return null; Continue down the tree to find more entities
				}

				return x;
			});

			return lstResults;
		}



		public static Map<string, List<string>> AugmentWithEntities2(IEnumerable<Prototype> lstEntities, ProtoScriptTagger tagger)
		{
			//This version returns a list of each entity and it's generalized types
			Map<string, List<string>> mapResults = new Map<string, List<string>>();

			foreach (Prototype prototype in lstEntities)
			{
				Prototype prototype1 = prototype;
				string strEntity = prototype1.PrototypeName;
				//string strEntity = TemporaryLexemes.Unroll(prototype1);
				//if (StringUtil.IsEmpty(strEntity))
				//{
				//	//This entity is just the key we use to search the original string 
				//	strEntity = prototype1.PrototypeName;
				//}


				mapResults[strEntity] = DescribeEntity(tagger, prototype1, strEntity);

				if (mapResults[strEntity].Count == 0)
				{
					foreach (Prototype protoAssociation in prototype.Associations.Select(x => x.Key))
					{
						mapResults[strEntity].AddRange(DescribeEntity(tagger, protoAssociation, strEntity));
					}
				}

			}

			return mapResults;
		}

		public static List<string> DescribeEntity(ProtoScriptTagger tagger, Prototype prototype1, string strEntity)
		{
			List<string> lstDescriptions = new List<string>();

			if (Prototypes.TypeOf(prototype1, "Token"))
			{
				List<Prototype> lstTargets = MultiTokenPhrases.GetTargetsOfContainingPhrases(prototype1);

				foreach (Prototype target in lstTargets)
				{
					string strTarget = Unroll(target);
					lstDescriptions.Add(strTarget);
				}
			}

			else
			{
				foreach (Prototype protoParent in prototype1.Ancestors)
				{
					string strParentEntity = Unroll(protoParent);
					if (!StringUtil.IsEmpty(strParentEntity) && !StringUtil.EqualNoCase(strParentEntity, strEntity))
					{
						lstDescriptions.Add(strParentEntity);
					}
				}

				if (Prototypes.TypeOf(prototype1, "Table"))
				{
					Prototype protoTableInstance = tagger.Interpretter.NewInstance(prototype1);

					if (null != protoTableInstance.Properties["Table.Field.TableName"])
					{
						Prototype protoObjectName = protoTableInstance.Properties["Table.Field.TableName"];
						lstDescriptions.Add(new StringWrapper(protoObjectName).GetStringValue());
					}
				}
			}

			return lstDescriptions;
		}

		static public string Unroll(Prototype prototype)
		{
			if (Prototypes.TypeOf(prototype, Lexeme.Prototype))
			{
				TemporaryLexeme rowLexeme = (TemporaryLexeme)prototype;
				return rowLexeme.Lexeme;
			}
			else if (Prototypes.TypeOf(prototype, Ontology.Collection.Prototype) || Prototypes.TypeOf(prototype, "MultiTokenPhrase"))
			{
				StringBuilder sb = new StringBuilder();
				foreach (Prototype child in prototype.Children)
				{
					sb.Append(Unroll(child)).Append(" ");
				}

				return sb.ToString();
			}


			else
			{
				Collection collection = new Collection(TemporaryLexemes.GetLexemesByRelatedPrototype(prototype, false));
				if (collection.Count > 0)
				{
					return Unroll(collection.Children.Last());
				}
				else
				{
					Prototype? protoPhrase = MultiTokenPhrases.GetMultiTokenPhrasesTargeting(prototype, false).FirstOrDefault();
					if (null != protoPhrase)
						return Unroll(protoPhrase);

					//Do not return PrototypeName, if there isn't a good lexeme, this isn't a unrollable prototype
					return string.Empty;
				}
			}
		}
	}
}
