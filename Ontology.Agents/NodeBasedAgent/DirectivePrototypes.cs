using BasicUtilities;
using ProtoScript.Parsers;
using ProtoScript;
using Ontology.Simulation;
using Buffaly.SemanticDB.Data;
using Buffaly.SemanticDB;
using Buffaly.NLU;
using Buffaly.AIProviderAPI;

namespace Ontology.Agents.NodeBasedAgent
{
	public class DirectivePrototypes
	{

		public static async Task<Prototype> GetDirectivePrototype(string strDirective, ProtoScriptTagger tagger)
		{
			//>first try to get a similar fragment with Tag "Directive" that has a value over 0.9
                        FragmentsDataTable dtSimilarFragments = await Fragments.GetMostSimilar2ByTagID(strDirective, "Directive", 0.9);
			if (dtSimilarFragments.Count > 0)
			{
				FragmentsRow rowFragment = dtSimilarFragments[0];
				Prototype protoEnglishPredicate = rowFragment.GetPrototype("LearnedDirective");
				if (null != protoEnglishPredicate)
					return protoEnglishPredicate;
			}

			Prototype protoEnglish = await GetNewDirectivePrototype(strDirective, tagger);

			return protoEnglish;
		}

		private static async Task<Prototype> GetNewDirectivePrototype(string strDirective, ProtoScriptTagger tagger)
		{
			string strFragment = Fragments.GetFragmentByFragmentKey("Semantic Program English Predicate").Fragment;
			string strFragmentSimpleTextGuidelines = Fragments.GetFragmentByFragmentKey("Simple Text Guidelines").Fragment;
			string strInput = @"
# Input Phrase 

Use the following directive to generate your response: 

\t'" + strDirective + @"'

Your response:

";
			string strPredicate = await Completions.CompleteAsJSON(strInput, strFragment + strFragmentSimpleTextGuidelines, ModelSize.LargeModel);

			Prototype protoEnglish = DirectivePrototypes.EnglishDirectiveToPrototype(strDirective, new JsonObject(strPredicate), tagger);

			DirectivePrototypes.WriteDirectivePrototypeToFile(strDirective, tagger, protoEnglish);
			return protoEnglish;
		}

		public static Prototype EnglishDirectiveToPrototype(string strDirective, JsonObject jsonPredicate, ProtoScriptTagger tagger)
		{
			string strPrototypeName = "Directive_" + StringHelper.SentenceToPrototypeName(strDirective);

			Prototype prototype = TemporaryPrototypes.GetOrCreateTemporaryPrototype(strPrototypeName);
			TypeOfs.Insert(prototype, "LearnedDirective");

			Collection colPredicates = new Collection();

			foreach (JsonValue jsonPredicateValue in jsonPredicate.GetJsonArrayOrDefault("predicates"))
			{
				string strPredicate = jsonPredicateValue.ToString();
				colPredicates.Add(new StringWrapper(strPredicate));
			}

			prototype.Properties["LearnedDirective.Field.Predicates"] = colPredicates;

			Collection colGoals = new Collection();
			foreach (JsonValue jsonGoalValue in jsonPredicate.GetJsonArrayOrDefault("goals"))
			{
				string strGoal = jsonGoalValue.ToString();
				colGoals.Add(new StringWrapper(strGoal));
			}

			prototype.Properties["LearnedDirective.Field.GoalStates"] = colGoals;

			Collection colFeasibilityChecks = new Collection();
			foreach (JsonValue jsonFeasibilityCheckValue in jsonPredicate.GetJsonArrayOrDefault("feasibilityChecks"))
			{
				string strFeasibilityCheck = jsonFeasibilityCheckValue.ToString();
				colFeasibilityChecks.Add(new StringWrapper(strFeasibilityCheck));
			}

			prototype.Properties["LearnedDirective.Field.Prerequisites"] = colFeasibilityChecks;

			return prototype;
		}

		public static void WriteDirectivePrototypeToFile(string strDirective, ProtoScriptTagger tagger, Prototype prototype)
		{
			PrototypeDefinition prototypeDefinition = PrototypeDefinitionHelpers.PrototypeToPrototypeDefinition(prototype, false);
			//	string strDirectiveLiteral = StringHelper.EscapeStringForCSharpLiteral(strDirective);
			prototypeDefinition.Annotations.Add(AnnotationExpressions.Parse($"[SemanticProgram.Directive(@\"{strDirective}\")]"));

			string strPrototypeDefinition = SimpleGenerator.Generate(prototypeDefinition);

			//>insert the prototype definition into the project
			string strFile = FileUtil.BuildPath(StringUtil.LeftOfLast(tagger.ProjectFile, "\\"), "Directives.pts");
			PrototypeDefinitionHelpers.InsertOrUpdatePrototypeDefinition(prototypeDefinition, strFile);
		}


	}
}
