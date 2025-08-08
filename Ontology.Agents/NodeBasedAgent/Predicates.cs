using BasicUtilities;
using Ontology.Agents.Actions;
using Ontology.Simulation;
using ProtoScript.Interpretter;

namespace Ontology.Agents.NodeBasedAgent
{
	public class Predicates
	{
		public static async Task<bool> VerifyAllPredicates(Prototype protoPredicates, SessionObject session, JsonArray jsonResults)
		{
			bool bAllTrue = protoPredicates.Children.Count > 0;

			foreach (Prototype protoPredicate in protoPredicates.Children)
			{
				JsonObject jsonPredicate = new JsonObject();
				jsonResults.Add(jsonPredicate);				
				
				string strPredicate = StringWrapper.ToString(protoPredicate);
				bool bIsTrue = await IsPredicateTrue(strPredicate, session, jsonPredicate);
				Logs.DebugLog.WriteEvent("Predicate", $"Predicate: {strPredicate}, Result: {bIsTrue}");

				bAllTrue = bIsTrue && bAllTrue;

				if (!bAllTrue)
					break;
			}

			return bAllTrue;
		}

		public static async Task<bool> IsPredicateTrue(string strPredicate, SessionObject session, JsonObject jsonPredicate)
		{
			jsonPredicate["Predicate"] = strPredicate;
			bool bResult;

			//>Get an existing semantic program first
			Prototype program = await ProtoScriptActions.GetExistingProgram(strPredicate);
			if (program != null)
			{
				jsonPredicate["ExistingProgram"] = program.PrototypeName;

				try
				{
					//> execute the program prototype and return the result				
					object oRes2 = session.Tagger.Interpretter.RunMethodAsObject(program, "Execute", new List<object>());
					bResult = (bool)SimpleInterpretter.GetAs(oRes2, typeof(bool));
				}
				catch (Exception ex)
				{
					Logs.DebugLog.WriteEvent("Predicate", $"Error: {ex.Message}");
					bResult = false;
					jsonPredicate["Error"] = ex.Message;
				}

				jsonPredicate["IsTrue"] = bResult;

				return bResult;
			}

			JsonObject jsonResult = await ProtoScriptActions.GenerateProgramAndVerify(session, strPredicate, null);

			string strProgram = jsonResult.GetStringOrNull("Program");
			string strCall = jsonResult.GetStringOrNull("Call");

			jsonPredicate["Program"] = strProgram;
			jsonPredicate["Call"] = strCall;

			try
			{
				object oRes = session.Tagger.InterpretImmediate(strCall);
				bResult = (bool)SimpleInterpretter.GetAs(oRes, typeof(bool));
			}
			catch (Exception ex)
			{
				Logs.DebugLog.WriteEvent("Predicate", $"Error: {ex.Message}");
				bResult = false;
				jsonPredicate["Error"] = ex.Message;
			}

			return bResult;
		}
	}
}
