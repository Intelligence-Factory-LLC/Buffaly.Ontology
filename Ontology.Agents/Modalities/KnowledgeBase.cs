using BasicUtilities.Collections;
using BasicUtilities;
using Buffaly.SemanticDB.Data;
using Buffaly.SemanticDB;
using Ontology.Agents.Actions;
using System.Text;
using Buffaly.AIProviderAPI;

namespace Ontology.Agents.Modalities
{
	public class KnowledgeBase
	{

		public static async Task<JsonObject> AnswerQuestion(string strQuestion)
		{
			List<FragmentsRow> lstTopChunks = await SimpleResearchPipeline(strQuestion);

			if (!lstTopChunks.Any())
			{
				return new JsonObject(@"{
					IsAnswerable : false,
					InformationNeeded : 'No documents available'
				}");
			}

			JsonObject jsonResponse = await QueryCorpus(strQuestion, lstTopChunks);
			return jsonResponse;
		}

		public static async Task<JsonObject> AnswerQuestion2(string strQuestion)
		{
			//This is the original (more complex pipeline)

			//>+ get the most similar fragments with tag = 'KB Question'
			FragmentsDataTable dtMostSimilarFragments = await Fragments.GetMostSimilar2ByTagID(strQuestion, "KB Question", 0.7);
			List<FragmentsRow> lstCandidates = new List<FragmentsRow>();

			foreach (FragmentsRow fragment in dtMostSimilarFragments)
			{
				lstCandidates.AddRange(fragment.Fragments);
			}


			JsonObject? jsonResponse = null;


			if (lstCandidates.Count == 0)
			{
				jsonResponse = await BuildQuestionAndAnswers(strQuestion);
			}

			else
			{
				jsonResponse = BuildResponseFromAnswers(dtMostSimilarFragments);
			}

			return jsonResponse;
		}

		private static JsonObject BuildResponseFromAnswers(FragmentsDataTable dtMostSimilarFragments)
		{
			JsonObject jsonResponse;

			Map<int, string> mapFragmentID = new Map<int, string>();
			foreach (FragmentsRow rowFragment in dtMostSimilarFragments)
			{
				foreach (FragmentsRow fragment in rowFragment.Fragments)
				{
					if (!mapFragmentID.ContainsKey(fragment.FragmentID))
						mapFragmentID.Add(fragment.FragmentID, fragment.Fragment);
				}
			}

			if (mapFragmentID.Count == 1)
			{
				var pair = mapFragmentID.First();
				jsonResponse = new JsonObject();
				jsonResponse["IsAnswerable"] = true;
				jsonResponse["FragmentIDs"] = new JsonArray(new List<int>() { pair.Key });
				jsonResponse["Response"] = pair.Value;
			}
			else
			{
				throw new NotImplementedException();
			}

			return jsonResponse;
		}

		private static async Task<JsonObject> BuildQuestionAndAnswers(string strQuestion)
		{
			//Build the knowledge base 
			FragmentsRow rowQuestion = Fragments.GetOrInsertFragment(strQuestion);
			FragmentTags.InsertOrUpdateFragmentTag(rowQuestion.FragmentID, "KB Question");


			//>+ get the most similar fragments with tag = 'KB Answer'
			FragmentsDataTable dtMostSimilarAnswers = await Fragments.GetMostSimilar2ByTagID(strQuestion, "KB Answer", 0.1);

			JsonObject jsonResponse = await QueryCorpus(strQuestion, dtMostSimilarAnswers);

			if (!jsonResponse.GetBooleanOrFalse("IsAnswerable"))
			{
				jsonResponse = await ResearchAdditionalDocuments(strQuestion);
			}

			if (jsonResponse.GetBooleanOrFalse("IsAnswerable"))
			{
				InsertNewRespoinse(rowQuestion, jsonResponse);
			}

			return jsonResponse;
		}

		public static async Task<JsonObject> QueryCorpus(string strQuestion, IEnumerable<FragmentsRow> dtMostSimilarAnswers)
		{
			if (!dtMostSimilarAnswers.Any())
			{
				return new JsonObject(@"
				{
					IsAnswerable : false,
					InformationNeeded : 'No documents available'
				}");
			}

			StringBuilder sbDocuments = new StringBuilder();

			foreach (FragmentsRow answer in dtMostSimilarAnswers.Take(10))
			{
				sbDocuments.Append("<Fragment ID=\"" + answer.FragmentID + "\">");
				sbDocuments.AppendLine(answer.Fragment);
				sbDocuments.AppendLine("</Fragment>");
			}

			string strInstructions = @"
You are helping to answer a user's question based on a knowledge base of information. You will be provided with one or more 
documents (labelled as Fragment) from the knowledge base. You will also be provided with the question from the user. You may 
utilize information from the entire conversation to clarify the user's request.
Your job is to use the document(s) to potentially address the user's message. 

# Response Guidelines

* Do not make up information. All information must come directly from the document 
* Respond with markdown. 
* Preserve markdown links from the original document and return them to allow your response to be tied back to the Document
* If possible return a link for each section that you utilize so we can tie multiple pieces of the response back to the document
* If the user's message is not about the document you will respond with a flag IsUnrelatedMessage 
* If the user's message cannot be answered from the document you will response with IsAnswerable = false 
* If you need more information from the user you will respond with a field InformationNeeded = message

";



			string strJson = @"
{
   Response: a string response 
   IsAnswerable : boolean value indicating the message can or cannot be answered from the document 
   InformationNeeded : any additional information needed from the user to answer the message
	FragmentIDs : [an array of the fragment id's used to answer]
}
";
			string strPrompt = LLMs.CreateLLMPrompt(strInstructions, null, strJson);

			string strInput = @"
# Question 

" + strQuestion + @"

# Documents

" + sbDocuments.ToString();

			string strResult = await Completions.CompleteAsJSON(strInput, strPrompt, ModelSize.LargeModel);

			return  new JsonObject(strResult);
		}

		private static async Task<JsonObject> ResearchAdditionalDocuments(string strQuestion)
		{
			FragmentsDataTable dtMostSimilarAnswers = await Fragments.GetMostSimilar2ByTagID(strQuestion, "Google Docs", 0.10);

			if (dtMostSimilarAnswers.Count == 0)
			{
				return new JsonObject(@"
				{
					IsAnswerable : false,
					InformationNeeded : 'No documents available'
				}");
			}

			JsonObject jsonResponse = await QueryCorpus(strQuestion, dtMostSimilarAnswers);

			return jsonResponse;
		}

		private static void InsertNewRespoinse(FragmentsRow rowQuestion, JsonObject jsonResponse)
		{
			FragmentsRow rowAnswer = Fragments.GetOrInsertFragment(jsonResponse.GetStringOrNull("Response"));
			rowAnswer.ParentFragmentID = rowQuestion.FragmentID;
			rowAnswer.DataObject["Sources"] = jsonResponse["FragmentIDs"];

			FragmentTags.InsertOrUpdateFragmentTag(rowAnswer.FragmentID, "KB Answer");

			FragmentsRepository.UpdateFragment(rowAnswer);
		}


		public static async Task<List<FragmentsRow>> SimpleResearchPipeline(string strQuestion)
		{
			//>+ get the most similar fragments by tag = Fact 
			//>get the most similar fragments by tag = Chunk
			//>merge the two lists and sort by Similarity Descending
			FragmentsDataTable dtFact = await Fragments.GetMostSimilar2ByTagID(strQuestion, "Fact", 0.2);
			FragmentsDataTable dtChunk = await Fragments.GetMostSimilar2ByTagID(strQuestion, "Chunk", 0.2);

			//>+ iterate over the dtFact and get each unique ParentFragmentID.
			//>count the number of fragments for each chunk and store in a dictionary
			//>iterate over the dtChunk and and see if any are not in the dictionary

			Map<int, int> dctFragmentCounts = new Map<int, int>();
			// Iterate over dtFact and count fragments for each unique ParentFragmentID
			foreach (FragmentsRow row in dtFact)
			{
				if (null == row.ParentFragmentID)
					continue;

				int iParentID = row.ParentFragmentID.Value;
				if (dctFragmentCounts.ContainsKey(iParentID))
				{
					dctFragmentCounts[iParentID]++;
				}
				else
				{
					dctFragmentCounts.Add(iParentID, 1);
				}
			}

			//>iterate over the dtChunks and any where the FragmentID is not in the dictionary, add it to the list with value 0
			foreach (FragmentsRow row in dtChunk)
			{
				if (!dctFragmentCounts.ContainsKey(row.FragmentID))
				{
					dctFragmentCounts.Add(row.FragmentID, 0);
				}
			}

			//>order the dictionary by count descending, and write out the FragmentID, Count and Similarity
			var sortedFragmentCounts = dctFragmentCounts.OrderByDescending(pair => pair.Value);
			List<FragmentsRow> lstTopChunks = new List<FragmentsRow>();

			foreach (var pair in sortedFragmentCounts)
			{
				int iFragmentID = pair.Key;
				int iCount = pair.Value;

				//>+ get the fragment from dtChunk by FragmentID
				FragmentsRow? rowFoundFragment = dtChunk.FirstOrDefault(x => x.FragmentID == iFragmentID);
				double dSimilarity = rowFoundFragment?.DataObject.GetDoubleOrDefault("Similarity", 0.0) ?? 0.0;

				if (rowFoundFragment != null)
				{
					Logs.DebugLog.WriteEvent("FragmentInfo", "FragmentID: " + iFragmentID.ToString() + ", Count: " +
						iCount.ToString() + ", Similarity: " + dSimilarity);
				}
				else
				{
					Logs.DebugLog.WriteEvent("FragmentInfo", "FragmentID: " + iFragmentID.ToString() + " not found in dtChunk, Count: " + iCount.ToString());
				}

				if (iCount >= 0 || dSimilarity >= 0.4)
				{
					lstTopChunks.Add(rowFoundFragment);
				}
			}

			return lstTopChunks;
		}
	}
}
