using BasicUtilities;
using Buffaly.NLU;

namespace Ontology.Agents
{
	public class WebPagePipeline
	{
		public static async Task UnderstandDirective(DirectiveRequest request)
		{
			request.Modality = TemporaryPrototypes.GetTemporaryPrototypeOrNull("WebPage");

			await PipelineBase.UnderstandDirective(request);
		}

		public static async Task<ImplementDirectiveResult> ImplementDirective(DirectiveRequest request)
		{
			ProtoScriptTagger tagger = request.Tagger;
			ImplementDirectiveResult result = null;
                        Prototype? protoFirstAction = request.CandidateActions.FirstOrDefault();

			if (request.KnownEntity != null) // this may be an entity request
			{
				result = new ImplementDirectiveResult();

				Prototype protoResult = ToOpenAWebPage(request);
				result.Result["ProtoScriptResult"] = protoResult.ToFriendlyJsonObject();
				result.Result["ResultType"] = "ProtoScript Result";

				return result;
			}

			if (request.CandidateActions.Count > 0)
			{
				result = await PipelineBase.ImplementDirective(request);
			}

			if (null == result)
			{
				JsonObject jsonResponse = ToChatWithAWebPage(request);
				if (jsonResponse.GetBooleanOrFalse("IsUnrelatedMessage"))
					return null;

				jsonResponse["ResultType"] = "JSON";
				result = new ImplementDirectiveResult() { Result = jsonResponse };
			}

			return result;
		}

		private static JsonObject ToChatWithAWebPage(DirectiveRequest request)
		{
			Prototype protoAction = TemporaryPrototypes.GetTemporaryPrototypeOrNull("ToChatWithAWebPage");
                        Prototype? protoEntity = request.Entities.FirstOrDefault(x => Prototypes.TypeOf(x, "WebPage"));
			object oResult = request.Tagger.Interpretter.RunMethodAsObject(protoAction, "Execute", new List<object>() { protoEntity, request.Directive });

			return oResult as JsonObject;
		}
		private static Prototype ToOpenAWebPage(DirectiveRequest request)
		{
			Prototype protoAction = TemporaryPrototypes.GetTemporaryPrototypeOrNull("ToOpenAWebPage");
                        Prototype? protoEntity = request.Entities.FirstOrDefault(x => Prototypes.TypeOf(x, "WebPage"));
			return request.Tagger.Interpretter.RunMethodAsPrototype(protoAction, "Execute", new List<object>() { protoEntity });
		}
	}
}
