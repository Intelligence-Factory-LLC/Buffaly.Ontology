using Buffaly.NLU;
using ProtoScript;
using System.Text;

namespace Ontology.Agents.Actions
{
	public class MetaActionsInternal
	{
		static public async Task<string> EntityResolution(string strDirective, ProtoScriptTagger tagger)
		{
			FastPrototypeSet lstPrototypes = new FastPrototypeSet(Entities.RecognizeEntitiesViaActivationSpreading(strDirective, tagger).Where(x => x.Value >= 1));

			List<string> lstEntities = await Entities.ExtractEntitiesAsync(strDirective);

			foreach (System.String entity in lstEntities)
			{
				foreach (Prototype protoEntity in PipelineBase.GetOntologyEntities(PipelineBase.GetOntologyUnderstanding(entity, tagger, 10)))
				{
					if (lstPrototypes.TryGetValue(protoEntity, out Prototype? protoExisting))
					{
						protoExisting.Value += 1;
					}
					else
					{
						protoEntity.Value = 1;
						lstPrototypes.Add(protoEntity);
					}
				}


				List<Prototype> lstSemantic = await Entities.GetCandidateEntitiesBySemanticSearch(entity, 0.4);
				foreach (Prototype semantic in lstSemantic)
				{
					if (Prototypes.TypeOf(semantic, "LearnedEntity"))
					{
						if (lstPrototypes.TryGetValue(semantic, out Prototype? protoExisting))
						{
							protoExisting.Value += semantic.Value;
						}
						else
						{
							lstPrototypes.Add(semantic);
						}
					}
				}
			}

			List<Prototype> lstSorted = lstPrototypes.OrderByDescending(x => x.Value).ToList();


			//List<ResolvedEntity> lstResolvedEntities = new List<ResolvedEntity>();
			//foreach (Prototype prototype in lstSorted)
			//{
			//	PrototypeDefinition protoDef = PrototypeDefinitionHelpers.PrototypeToPrototypeDefinition(prototype, false);

			//	ResolvedEntity resolvedEntity = new ResolvedEntity()
			//	{
			//		Name = prototype.PrototypeName,
			//		Definition = ProtoScript.Parsers.SimpleGenerator.Generate(protoDef),
			//		Confidence = prototype.Value
			//	};
			//	lstResolvedEntities.Add(resolvedEntity);
			//}

			//return lstResolvedEntities;

			StringBuilder sb = new StringBuilder();

			foreach (Prototype prototype in lstSorted)
			{
				PrototypeDefinition protoDef = PrototypeDefinitionHelpers.PrototypeToPrototypeDefinition(prototype, false);
			
				//>append as formatted with Name, Confidence, and Definition
sb.AppendLine(@$"
** Name **:
{prototype.PrototypeName}, Confidence: ({prototype.Value:F2})
** Definition **: 
{ProtoScript.Parsers.SimpleGenerator.Generate(protoDef)}
---
");

			}

			return sb.ToString();
		}

		static public async Task<string> LookupActions(string Directive, ProtoScriptTagger tagger)
		{
			List<Prototype> lstProtoScriptActions = await ProtoScriptActions.GetRelevantActions(Directive, 15);

			StringBuilder strProtoScriptDescriptions = ProtoScriptActions.ActionsToString(lstProtoScriptActions, tagger);

			return strProtoScriptDescriptions.ToString();
		}
	}
}
