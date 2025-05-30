using BasicUtilities;
using System.Net;
using System.Text;

namespace ProtoScript.Extensions.Suggestions
{
	internal class LLMSuggestions
	{
		static private async Task<string> CompleteAsJSON(string strText, string strPrompt, string Model = null)
		{
			ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

			using (var httpClient = new HttpClient())
			{
				string token = BasicUtilities.Settings.GetString("OpenAI.Token");

				string chatModel = StringUtil.IsEmpty(Model) ? BasicUtilities.Settings.GetStringOrDefault("OpenAI.DefaultModel", "gpt-4o-mini") : Model;
				string chatApiUrl = "https://api.openai.com/v1/chat/completions";


				using (var request = new HttpRequestMessage(HttpMethod.Post, chatApiUrl))
				{
					request.Headers.Add("Authorization", $"Bearer {token}");

					var requestBody = new
					{
						model = chatModel,
						response_format = new { type = "json_object" },
						messages = new[] {
							new
							{
								role = "system",
								content = strPrompt
							},
							new
							{
								role = "user",
								content = strText
							}
						}
					};

					var jsonContent = new StringContent(JsonUtil.ToString(requestBody).ToString(), Encoding.UTF8, "application/json");
					request.Content = jsonContent;

					var response = await httpClient.SendAsync(request);
					var responseContent = await response.Content.ReadAsStringAsync();

					Logs.DebugLog.WriteEvent("OpenAI Response", responseContent);

					if (response.IsSuccessStatusCode)
					{
						JsonObject jsonResponse = new JsonObject(responseContent);
						return jsonResponse["choices"].ToJsonArray()[0].ToJsonObject()["message"].ToJsonObject()["content"].ToString();
					}
					else
					{
						return responseContent.ToString();
					}
				}
			}
		}

		static public List<string> Suggest(int iPos, CodeContext context, List<Statement> lstContext)
		{
			Logs.DebugLog.WriteMethodStart();

			string strPrompt =
@"You are a coding assistant that predicts the next line(s) of ProtoScript code. You will be provided 
with the codee in the file up to the cursor. Use it to predict the next line(s) of code. 

You may be given additional context, such as methods from other files, as separate snippets. Return multiple 
suggestions if possible, and return them in JSON format.

Example format: 

```json
{
  ""completions"": [
		""[SUGGESTED NEXT LINE(S) OF CODE OPTION 1]"",
		""[SUGGESTED NEXT LINE(S) OF CODE OPTION 2]"",
		""[SUGGESTED NEXT LINE(S) OF CODE OPTION 3]""
  ]
}
```
Info about ProtoScript. ProtoScript is a domain-specific language that has a syntax 
similar to C# or JavaScript. Instead of classes, we use prototype(s). Here are a few sample constructs in 
ProtoScript: 

prototype Token;				//this defines a prototype named Token, with no functions or properties
prototype SubToken: Token;		//this defines a prototype named SubToken that inherits from Token
prototype SubSubToken : SubToken;	//this defines a prototype named SubSubToken that inherits from SubToken

[AnnotationSyntax()]			//this is an annotation
prototype Sequence				//this defines a prototype named Sequence
{
	function GetChildren() : Collection			//this method takes no parameters and returns a Collection
	{
		return [];
	}

	Token TokenProperty = null;				//this defines a property named TokenProperty that is of type Token

	function Sequence() : void				//this is a constructor syntax
	{
		this.TokenProperty = SubToken;		//this sets the TokenProperty to an instance of SubToken
	}
}

Sequence sequence = new Sequence();			//this creates a new instance of Sequence
Collection collection = sequence.GetChildren();		//this calls the GetChildren method on the sequence instance

";

			ProtoScript.File file = context.Compiler.Files.FirstOrDefault(x => StringUtil.EqualNoCase(x.Info.FullName, context.ProtoScriptFile.FileName));
			if (null == file)
			{
				Logs.DebugLog.WriteEvent("Compiled File Not Found", context.ProtoScriptFile.FileName);
				return new List<string>();
			}

			string strCode = file.RawCode.Substring(0, Math.Min(iPos, file.RawCode.Length));

			string strMessage =
@"
## Current Code 

```protoscript
" + strCode + @"
```

";


			Logs.DebugLog.WriteEvent("LLMSuggestions.Message", strMessage);

			string strResult = null;

			var task = Task.Run(async () =>
			{
				try
				{
					strResult = await CompleteAsJSON(strMessage, strPrompt, "gpt-4o");
				}
				catch (Exception err)
				{
					Logs.LogError(err);
				}
			});

			task.Wait();


			List<string> lstSuggestions = new List<string>();
			JsonObject jsonResult = new JsonObject(strResult ?? "{}");
			foreach (var suggestion in jsonResult.GetJsonArrayOrDefault("completions"))
			{
				lstSuggestions.Add(suggestion.ToString());
			}

			Logs.DebugLog.WriteMethodStop();
			return lstSuggestions;
		}
	}
}
