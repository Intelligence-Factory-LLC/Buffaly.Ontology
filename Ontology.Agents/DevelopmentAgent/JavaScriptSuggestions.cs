using BasicUtilities;
using Buffaly.AIProviderAPI;
using Buffaly.NLU;
using Buffaly.SemanticDB;

namespace Ontology.Agents.DevelopmentAgent
{
    public static class JavaScriptSuggestions
    {
        public static async Task<List<string>> ProcessDirectiveJavaScript(
			string strSesionKey,
			string strDirective,
            int iPos,
            string strAllCode)
        {
            Logs.DebugLog.WriteMethodStart();

			SessionObject session = DevAgent.Instance.GetOrCreateSession(strSesionKey);

			string strPrompt =
@"You are a helpful coding assistant that writes JavaScript code in response to a user's directive. The 
user has types the directive as

//>directive here

and you are filling in the next lines of code as indicated by <<CURSOR>>.

Return a *single* best guess for the next line(s) of code in JSON format. The JSON format is 
{
  ""completions"": ""[SUGGESTED NEXT LINE(S) OF CODE]""
}

If there is no suggestion return  an empty string within the JSON. 
";
            strPrompt +=
@"
## Additional guidelines: ##

1. Do *not* provide multiple suggestions; only one item in the `completions` array.
2. The user’s directive and code context are below.
3. Use any relevant methods/files from the context if they help produce a better guess.
4. Return only valid JSON. No extra text outside the JSON.
5. If the suggestion is multi-line, format it with multiple lines using newline characters and tabs. Match the surrounding code.

";
            strPrompt += Fragments.GetFragmentByFragmentKey("JavaScript Prompt")?.Fragment;

            string strCode = strAllCode.Substring(0, iPos);
            strCode += "\r\n<<<CURSOR>>>";

            string strMessage =
@"## User Directive ##

" + strDirective + @"

## Current Code ##

This is the code of the current file with the cursor marker. Use this as context for your response.

```javascript
" + strCode + @"
```

## Relevant Methods ##

These methods may be helpful for fulfilling the directive:
";

            Logs.DebugLog.WriteEvent("LLMSuggestions.ProcessDirective", "Getting tagger");
            ProtoScriptTagger tagger = session.Tagger;
            Logs.DebugLog.WriteEvent("LLMSuggestions.ProcessDirective", "Tagger initialized");


            strMessage += @"
## Your Response ##

Remember to respond in JSON: ";

            Logs.DebugLog.WriteEvent("LLMSuggestions.Message v4", strMessage);

            string strResult = null;
            try
            {
                var model = strDirective.Contains("-") ? ModelSize.SmallModel : ModelSize.LargeModel;
                strResult = await Completions.CompleteAsJSON(strMessage, strPrompt, model);
                Logs.DebugLog.WriteEvent("LLMSuggestions.Result", JsonUtil.ToFriendlyJSON(strResult));
            }
            catch (Exception err)
            {
                Logs.LogError(err);
            }

            List<string> lstSuggestions = new List<string>();
            JsonObject jsonResult = new JsonObject(strResult ?? "{}");
            lstSuggestions.Add(jsonResult.GetStringOrNull("completions"));

            Logs.DebugLog.WriteMethodStop();
            return lstSuggestions;
        }
    }
}
