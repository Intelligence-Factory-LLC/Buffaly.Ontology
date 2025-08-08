using Buffaly.SemanticDB.Data;
using Buffaly.SemanticDB;
using System.Text;
using BasicUtilities;

namespace Ontology.Agents.Actions
{
	public class WebPages
	{
		static public ArtifactsRow InsertFragmentAndArtifact(string strUrl, bool bForceRefresh)
		{
                        ArtifactsRow? rowArtifact = ArtifactsRepository.GetArtifactsByName(strUrl).FirstOrDefault();
			if (null == rowArtifact)
			{
				string strHtmlContent = GetWebPageAsText(strUrl);
				string strMarkdown = ConvertHtmlToMarkdown(strHtmlContent);
				int iFragmentID = Fragments.InsertOrUpdateFragment(strUrl, strMarkdown);

				// Create an artifact entry
				int iArtifactTypeID = ArtifactTypes.InsertArtifactType("WebPage", strMarkdown);
				int iArtifactID = Artifacts.InsertArtifact(iArtifactTypeID, strUrl, strUrl,
					"A webpage captured and converted from " + strUrl, iFragmentID, null);

				rowArtifact = ArtifactsRepository.Get(iArtifactID);
			}
			else if (bForceRefresh)
			{
				string strHtmlContent = GetWebPageAsText(strUrl);
				string strMarkdown = ConvertHtmlToMarkdown(strHtmlContent);

				// Update the fragment
				rowArtifact.Fragment.Fragment = strMarkdown;

				FragmentsRepository.UpdateFragment(rowArtifact.Fragment);
			}

			return rowArtifact;
		}

		//>create a method to get a webpage as text
		static public string GetWebPageAsText(string strUrl)
		{
			StringBuilder sbContent = HttpUtil.GetWebPage(strUrl);
			return sbContent.ToString();
		}



		//>+ create a method to convert html to markdown using the library ReverseMarkdown
		static public string ConvertHtmlToMarkdown(string strHtml)
		{
			var converter = new ReverseMarkdown.Converter();
			string strMarkdown = converter.Convert(strHtml);
			return strMarkdown;
		}

		//>+ the above code leaves quite bit of HTML in the markdown, create a method to clean it up 
		static public string CleanMarkdown(string strMarkdown)
		{
			// Remove any remaining HTML tags from the markdown
			StringBuilder sbCleanedMarkdown = new StringBuilder();
			bool bInsideTag = false;

			foreach (char ch in strMarkdown)
			{
				if (ch == '<')
				{
					bInsideTag = true;
				}
				else if (ch == '>')
				{
					bInsideTag = false;
					continue;
				}

				if (!bInsideTag)
				{
					sbCleanedMarkdown.Append(ch);
				}
			}

			return StringUtil.RemoveNonAlphanumericAndSpaces(sbCleanedMarkdown.ToString());
		}

		static public async Task<string> AskQuestionAboutWebPage(string strWebSite,
				string strQuestion,
				Buffaly.AIProviderAPI.ModelSize model)
		{
			


			ArtifactsRow rowArtifact = InsertFragmentAndArtifact(strWebSite, false);

			string strPrompt =
@"
You are helping to find information about a particular webpage that is provided. Your job is to answer the question: 

'" + strQuestion + @"'

use only information from the webpage. If the information cannot be found on the webpage, then let the user know the
information was not listed but do not make up a response. If there are additional links from the webpage that may be
useful in answering the question, you should return those as suggestions, but make sure to use full links, not 
just relative paths. The original page's url is:
'" +strWebSite + @"' 
All information from the webpage should be supported by an exact quote from the webpage. Use markdown for your response.

You will receive the webpage. The main content may be in markdown format. Some navigation elements may be in HTML.

Keep your response relevant and on topic to the question.
";

			string strPromptResponse = await Fragments.GetCompletion2(rowArtifact.FragmentID.Value, strPrompt, model);

			return strPromptResponse;
		}

		//>+ extract the previous code as a method to extract JsonArray from a specified table from and HTML input: ExtractTableFromHTML(string strHtml, string strTableID)
		public static JsonArray ExtractTableFromHTML(string strHtml, string strTableID)
		{
			var htmlDoc = new HtmlAgilityPack.HtmlDocument();
			htmlDoc.LoadHtml(strHtml);
			var table = htmlDoc.DocumentNode.SelectSingleNode($"//table[@id='{strTableID}']");
			List<string> lstColumns = new List<string>();
			List<List<string>> lstRows = new List<List<string>>();

			if (table != null)
			{
				var rows = table.SelectNodes("tr");
				foreach (var row in rows)
				{
					List<string> lstRow = new List<string>();
					var headerColumns = row.SelectNodes("th");
					if (headerColumns != null)
					{
						foreach (var headerColumn in headerColumns)
						{
							string columnText = headerColumn.InnerText.Trim();
							lstColumns.Add(columnText);
						}
					}
					var columns = row.SelectNodes("td");

					if (null != columns)
					{
						foreach (var column in columns)
						{
							string cellText = column.InnerText.Trim();
							lstRow.Add(cellText);
						}
						lstRows.Add(lstRow);
					}
				}
			}

			JsonArray jsonArray = new JsonArray();
			for (int i = 0; i < lstRows.Count; i++)
			{
				JsonObject jsonObject = new JsonObject();
				for (int j = 0; j < lstColumns.Count; j++)
				{
					jsonObject.Add(lstColumns[j], lstRows[i][j]);
				}
				jsonArray.Add(jsonObject);
			}

			return jsonArray;
		}

		//>+ create a method that takes the URL and the table ID and returns a JsonArray of the table contents
		//>it should download the url to a fragment if the fragment doesn't already exist
		//>add a flag to allow us to force a refresh

		public static JsonArray ExtractTableFromUrlAsJsonArray(string strUrl, string strTableID, bool bForceRefresh)
		{
			// Check if fragment exists for the URL
			FragmentsRow fragment = Fragments.GetFragmentByFragmentKey(strUrl);

			if (fragment == null || bForceRefresh)
			{
				// Download the HTML content if fragment doesn't exist or refresh is forced
				string strHtmlContent = WebPages.GetWebPageAsText(strUrl);
				int iFragmentID = Fragments.InsertOrUpdateFragment(strUrl, strHtmlContent);
				fragment = Fragments.GetFragment(iFragmentID);
			}

			// Extract table content to JsonArray
			JsonArray jsonArray = WebPages.ExtractTableFromHTML(fragment.Fragment, strTableID);

			return jsonArray;
		}



	}
}
