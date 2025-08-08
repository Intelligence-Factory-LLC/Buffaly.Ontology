using BasicUtilities;
using Buffaly.AIProviderAPI;

namespace Ontology.Agents.DevelopmentAgent
{
	public class JavaScriptAgent
	{
		static public async Task<Tuple<string, string>> GetSimpleGeneratedCall(string strDirective, JsonObject jsonValues)
		{
			string strPrompt = @"

# Instructions:

You are helping to generate a call to a javascript method in response to a user's request. 

You will be provided with a user's request, a list of JavaScript methods with examples, and a list of entities (if available). 

Your job is to select the best JavaScript method to call in response to the user's request and to generate a call to that method.

Respond in JSON with these fields: 

{
	'IsMethodAvailable' = true or false	//indicates if the method is available 
	'IsPartiallyAvailable' = true or false //indicates if the method is available, but does not fully address the user's request
	'Explanation' = 'explanation' //if IsPartiallyAvailable is true, this is the explanation
	'SelectedMethod' = 'JavaScript Method Name' //the name of the method that was selected
	'GeneratedCall' = 'call' //the generated call to the javascript method	
}

The call should be valid JavaScript. The request may not actually be related to the most likely methods.
In this case you should return IsMethodAvaialble = false and an explanation. 

Do not guess. Do not make up javascript.

";

			string strUserInput = @"
# The user's request is 

{Directive}

# The most likely methods are:

{Methods}

# In the user request, these entities were identified:

{AugmentedEntities}


";

			strUserInput = strUserInput.Replace("{Directive}", strDirective);

			foreach (string strKey in jsonValues.Keys)
			{
				strUserInput = strUserInput.Replace("{" + strKey + "}", jsonValues.GetStringOrDefault(strKey, string.Empty));
			}

			string strResult = await Completions.CompleteAsJSON(strUserInput, strPrompt);

			return new Tuple<string, string>(strResult, strUserInput);
		}

	}
}
