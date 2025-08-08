using BasicUtilities;
using Buffaly.AIProviderAPI;
using Buffaly.NLU;
using Buffaly.SemanticDB;
using Buffaly.SemanticDB.Data;
using Ontology.Simulation;
using System.Data;
using System.Text;
using Embeddings = Buffaly.SemanticDB.Embeddings;

namespace Ontology.Agents.DevelopmentAgent
{
	public class ProtoScriptAgent
	{
		static public async Task<Tuple<string, string>> GetSimpleGeneratedCall(string strDirective, JsonObject jsonValues)
		{
			string strPrompt = @"

# Instructions:

You are helping to generate a call to a method in response to a user's request. 

You will be provided with a user's request, a list of methods with examples, and a list of entities (if available). 

Your job is to select the best  method to call in response to the user's request and to find the parameters to 
the method. Make sure to fix any syntax mistakes in the call or the parameters. Correctly enclose strings. Use 
string escaping.

Respond in JSON with these fields: 

{
	'IsMethodAvailable' = true or false	//indicates if the method is available 
	'SelectedMethod' = 'Method Name' //the name of the method that was selected
	'Parameters' = ['parameter1', 'parameter 2', ...],
	'GeneratedCall' = 'MethodName(parameter1, parameter2, ...)' //the generated call to the method
}

## Syntax of the calls. 

* Use C# style string literal and escaping rules.
* Collections should be serialized as JSON Arrays (e.g. [1, 2, 3] or [""1"", ""2"", ""3""])

Sample code: 
Method1(""aaron"");
Method2([""Justin"", ""Brochetti""]);

Prototype result = Method3();
String strResult = Method4();

";

			string strUserInput = @"
# The user's request is 

{Directive}

# The most likely methods are:

{Methods}

# In the user request, these entities were identified:

{AugmentedEntities}

# Today is {Today}
";

			strUserInput = strUserInput.Replace("{Directive}", strDirective);

			strUserInput = strUserInput.Replace("{Today}", DateTime.Now.ToString("dddd, MMMM d, yyyy h:mmtt zzz"));

			foreach (string strKey in jsonValues.Keys)
			{
				strUserInput = strUserInput.Replace("{" + strKey + "}", jsonValues.GetStringOrDefault(strKey, string.Empty));
			}

			string strResult = await Completions.CompleteAsJSON(strUserInput, strPrompt);

			return new Tuple<string, string>(strResult, strUserInput);
		}



                static public async Task InsertAction(Prototype prototype, string strInfinitivePhrase)
                {

			FragmentsRow rowFragment = Fragments.GetFragmentOrNull(strInfinitivePhrase);
			if (null == rowFragment || null == rowFragment.GetPrototype() || !rowFragment.FragmentTagTags.Any(x => x.TagName == "ProtoScript Action"))
			{
				rowFragment = Fragments.GetOrInsertFragment(strInfinitivePhrase);
				rowFragment.SetPrototype(prototype);
				FragmentsRepository.UpdateFragmentData(rowFragment);

				FragmentTags.InsertOrUpdateFragmentTag(rowFragment.FragmentID, "Action");
				FragmentTags.InsertOrUpdateFragmentTag(rowFragment.FragmentID, "ProtoScript Action");

                                await Buffaly.SemanticDB.Embeddings.GetOrCalculateEmbeddingVectorByFragmentID(rowFragment.FragmentID);
                      
			}
		}

                //>Create a method InsertAsDirective that does the same as InsertAction except it inserts the fragment with the tag "Directive"
		static public async Task InsertAsDirective(Prototype prototype, string strInfinitivePhrase)
		{
			FragmentsRow rowFragment = Fragments.GetFragmentOrNull(strInfinitivePhrase);
			if (null == rowFragment || null == rowFragment.GetPrototype("LearnedDirective") ||  !rowFragment.FragmentTagTags.Any(x => x.TagName == "LearnedDirective"))
			{
				rowFragment = Fragments.GetOrInsertFragment(strInfinitivePhrase);
				rowFragment.SetPrototype("LearnedDirective", prototype);
				FragmentsRepository.UpdateFragmentData(rowFragment);

				FragmentTags.InsertOrUpdateFragmentTag(rowFragment.FragmentID, "Directive");

				List<double> lstVectors = await Buffaly.SemanticDB.Embeddings.GetOrCalculateEmbeddingVectorByFragmentID(rowFragment.FragmentID);
                        
			}
		}

		static private FragmentsDataTable? EntityFragments = null;

		const bool AllowInsertLexemes = false; 
		static public async Task InsertSemanticEntity(Prototype prototype, string strEntity, ProtoScriptTagger tagger)
		{
			if (null == EntityFragments)
			{
				EntityFragments = FragmentsRepository.Fragments_GetByTagID_Sp_PagingSp(Tags.GetTagByTagName("Entity").TagID,
					"", "FragmentID", false, 0, 20000);
			}

                        FragmentsRow? rowFragment = EntityFragments.FirstOrDefault(x => StringUtil.EqualNoCase(x.FragmentKey, strEntity));
			//Fragments.GetFragmentOrNull(strEntity);
			if (null == rowFragment)
			{
				rowFragment = Fragments.GetOrInsertFragment(strEntity);
				EntityFragments.Add(rowFragment);

				rowFragment.SetPrototype(prototype);
				FragmentsRepository.UpdateFragmentData(rowFragment);

				FragmentTags.InsertOrUpdateFragmentTag(rowFragment.FragmentID, "Entity");

                                await Buffaly.SemanticDB.Embeddings.GetOrCalculateEmbeddingVectorByFragmentID(rowFragment.FragmentID);
                        }
			else if (rowFragment.GetPrototype() == null || rowFragment.GetPrototype().PrototypeName != prototype.PrototypeName)
			{
				FragmentTags.InsertOrUpdateFragmentTag(rowFragment.FragmentID, "Entity");
				rowFragment.SetPrototype(prototype);
				FragmentsRepository.UpdateFragmentData(rowFragment);
			}

			if (AllowInsertLexemes)
				LexemeUtil.InsertLexemesForSemanticEntity(prototype, strEntity, tagger);
		}



		static public void InsertSemanticProgram(Prototype prototype, string strInfinitivePhrase)
		{

			FragmentsRow rowFragment = Fragments.GetFragmentOrNull(strInfinitivePhrase);
			if (null == rowFragment || null == rowFragment.GetPrototype() || !rowFragment.FragmentTagTags.Any(x => x.TagName == "ProtoScript Action"))
			{
				rowFragment = Fragments.GetOrInsertFragment(strInfinitivePhrase);
				rowFragment.SetPrototype(prototype);
				FragmentsRepository.UpdateFragmentData(rowFragment);

				FragmentTags.InsertOrUpdateFragmentTag(rowFragment.FragmentID, "Action");
				FragmentTags.InsertOrUpdateFragmentTag(rowFragment.FragmentID, "ProtoScript Action");

				var task = Task.Run(async () =>
				{
					List<double> lstVectors = await Embeddings.GetOrCalculateEmbeddingVectorByFragmentID(rowFragment.FragmentID);
				});
				task.Wait();
			}
		}

		public static async Task<Collection> GetCandidateActionsBySemanticSearch(ProtoScriptTagger tagger, string strSimplifiedSentence, int TotalMatches)
		{
			//Get the actions 
			//Vector embeddings change with characters like ? and carriage returns
			strSimplifiedSentence = new string(strSimplifiedSentence.Where(c => !char.IsPunctuation(c) && c != '\r' && c != '\n').ToArray());

			Buffaly.SemanticDB.Data.FragmentsDataTable dtFragments = await Buffaly.SemanticDB.Fragments.GetMostSimilar2ByTagID(strSimplifiedSentence, "ProtoScript Action", 0.50);

			Collection lstCandidates = new Collection();

                        int iCount = 0;

                        foreach (FragmentsRow rowFragmentAction in dtFragments)
                        {
                                Prototype protoMethod = rowFragmentAction.GetPrototype();
                                protoMethod = tagger.Interpretter.NewInstance(protoMethod);
                                lstCandidates.Add(protoMethod);

                                if (++iCount >= TotalMatches)
                                {
                                        break;
                                }
                        }

			return lstCandidates;
		}



		internal static async Task<Prototype> SelectBestAction(DirectiveRequest request, Collection colActions)
		{
			StringBuilder sb = new StringBuilder();
			ProtoScriptTagger tagger = request.Tagger;

			foreach (Prototype protoValue in colActions.Children)
			{
				Prototype protoAction = tagger.Interpretter.NewInstance(protoValue);

				string strMethodName = new StringWrapper(protoAction.Properties["ProtoScriptAction.Field.Name"]).GetStringValue();
				string strSignature = new StringWrapper(protoAction.Properties["ProtoScriptAction.Field.Signature"]).GetStringValue();
				string strDescription = new StringWrapper(protoAction.Properties["ProtoScriptAction.Field.Description"]).GetStringValue();

				sb.AppendLine($"* **{strMethodName}**"); // Method name with bold formatting
				sb.AppendLine($"    * {strSignature}");  // Signature with indentation

				// Split description into lines and add indentation
				string[] descriptionLines = strDescription.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
				foreach (string line in descriptionLines)
				{
					sb.AppendLine($"        * {line}");
				}

				sb.AppendLine();
			}

			StringBuilder sbEntities = new StringBuilder();
			var mapEntities = PipelineBase.AugmentWithEntities2(request.SemanticEntities, tagger);

			foreach (Prototype protoEntity in request.SemanticEntities)
			{
				string strEntityName = new StringWrapper(protoEntity.Properties["LearnedEntity.Field.EntityName"]).GetStringValue();

				sb.AppendLine($"* {strEntityName}"); // Entity name with bullet point
				sb.AppendLine();
			}

			JsonObject jsonValues = new JsonObject();
			jsonValues["Methods"] = sb.ToString();
			jsonValues["AugmentedEntities"] = request.AugmentedEntities;


			var tuple = await SelectBestActionViaLLM(request.Directive, jsonValues);

			Logs.DebugLog.WriteEvent("Select Best Action Prompt", tuple.Item1);
			Logs.DebugLog.WriteEvent("Select Best Action Result", tuple.Item2.ToJSON());

			JsonObject jsonResult = tuple.Item2;
			if (jsonResult.GetBooleanOrFalse("IsMethodAvailable"))
			{
				string strSelectedMethod = jsonResult.GetStringOrNull("SelectedMethod");
                                Prototype? protoAction = colActions.FirstOrDefault(x => new StringWrapper(x.Properties["ProtoScriptAction.Field.Name"]).GetStringValue() == strSelectedMethod);
                                return protoAction;
			}

			return null;
		}

		static private async Task<Tuple<string, JsonObject>> SelectBestActionViaLLM(string strDirective, JsonObject jsonValues)
		{
			string strPrompt = @"

# Instructions:

You are selecting the best action (or none) to take in response to a user's request.

You will be provided with a user's request, a list of methods with examples, and a list of entities (if available). 

Your job is to select the best  method to call in response to the user's request -- or to indicate that none of the methods
are appropriate. There are other methods available that are not listed, so you should indicate if none of the methods are
appropriate.

If the user's request is simple an object with no actions, you should select the corresponding method to open the object (if available).

Respond in JSON with these fields: 

{
	'IsMethodAvailable' = true or false	//indicates if the method is available 
	'SelectedMethod' = 'Method Name' //the name of the method that was selected
}

";

			string strUserInput = @"
# The user's request is 

{Directive}

# The most likely methods are:

{Methods}

# In the user request, these entities were identified:

{AugmentedEntities}

# Today is {Today}
";

			strUserInput = strUserInput.Replace("{Directive}", strDirective);

			strUserInput = strUserInput.Replace("{Today}", DateTime.Now.ToString("dddd, MMMM d, yyyy h:mmtt zzz"));

			foreach (string strKey in jsonValues.Keys)
			{
				strUserInput = strUserInput.Replace("{" + strKey + "}", jsonValues.GetStringOrDefault(strKey, string.Empty));
			}

			string strResult = await Completions.CompleteAsJSON(strUserInput, strPrompt);

			return new Tuple<string, JsonObject>(strUserInput, new JsonObject(strResult));
		}
	}
}