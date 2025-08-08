using BasicUtilities;
using Buffaly.AIProviderAPI;
using Ontology.Simulation;
using System.Text;

namespace Ontology.Agents.Actions
{
	public class LLMs
	{
		static public string CreateLLMPrompt(string instructions, Prototype examples, string outputFormat)
		{
			StringBuilder sbExamples = new StringBuilder();

            if (null != examples)
            {
                foreach (Prototype example in examples.Children)
                {
                    if (Prototypes.TypeOf(example, "Example"))
                    {
                        sbExamples.AppendLine("  Example: " + new StringWrapper(example.Properties["Example.Field.Source"]).GetStringValue());
                        sbExamples.AppendLine("  Output: " + new StringWrapper(example.Properties["Example.Field.Target"]).GetStringValue());
                        sbExamples.AppendLine();
                    }
                    else
                    {
                        sbExamples.AppendLine("  " + new StringWrapper(example).GetStringValue());
                    }
                }
            }

			string strPrompt = @"

# Instructions:

	" + instructions;

			if (examples?.Children.Count > 0)
			{
				strPrompt += @"

# Examples:

" + sbExamples.ToString();
			}

			strPrompt += @"

# Output Format:

Respond in JSON with these fields: 

	" + outputFormat;

			strPrompt += "\nToday is " + DateTime.Now.ToString("dddd, MMMM d, yyyy h:mmtt zzz");

			return strPrompt;
		}

		static public string AddSectionToPrompt(string prompt, string sectionName, string sectionContent)
		{
			return prompt + @"
## *" + sectionName + @"*
" + sectionContent;
		}

		static public async Task<JsonObject> ExecuteLLMPromptAndInput(string strPrompt, string strInput)
		{
			string strResult = await Completions.CompleteAsJSON(strInput, strPrompt, ModelSize.LargeModel);
			return new JsonObject(strResult);
		}

		
		static async public Task<JsonObject> ExecuteLLMPromptAndInputWithHistory(List<Message> lstHistory, ModelSize modelSize)
		{
			string strResult = await Completions.CompleteAsJSONWithHistory(lstHistory, modelSize);
			return new JsonObject(strResult);
		}

		static public List<Message> GetPromptAndInput(string strPrompt, string strInput)
		{
			List<Message> lstMessages = new List<Message>();
			lstMessages.Add(new Message() { Role = MessageRole.Assistant, Content = strPrompt });
			lstMessages.Add(new Message() { Role = MessageRole.User, Content = strInput });
			return lstMessages;
		}
		


		static public JsonObject ExecuteLLMPromptAndInputSmall(string strPrompt, string strInput)
        {
            string strResult = Completions.CompleteAsJSON(strInput, strPrompt, ModelSize.SmallModel).Result;
            return new JsonObject(strResult);
        }

		static public string AddJsonRequestToMessage(string strMessage, JsonObject jsonRequest)
		{
			foreach (string strKey in jsonRequest.Keys)
			{
				strMessage += "\n## " + strKey + " ##\n";
				strMessage += jsonRequest.GetStringOrNull(strKey) + "\n";
			}

			return strMessage;
		}
	}
}
