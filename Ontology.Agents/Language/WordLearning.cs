using BasicUtilities;
using Buffaly.NLU;
using Ontology.BaseTypes;
using Ontology.Simulation;
using ProtoScript;
using ProtoScript.Parsers;
using System.Text;

namespace Ontology.Agents.Language
{
	public class WordLearning
	{
		public static async Task<List<PrototypeDefinition>> ProcessPhraseAsync(string strPhrase, ProtoScriptTagger tagger, bool bWriteToFile = true)

		{
			strPhrase = strPhrase.Replace(".", "");

			for (int iAttempt = 0; iAttempt < 3; iAttempt++)
			{
				try
				{
					List<PrototypeDefinition> lstPrototypeDefinitions = await GenerateUnknownWordPrototypes2(strPhrase, tagger, false, bWriteToFile);

					StringBuilder sb = new StringBuilder();
					foreach (PrototypeDefinition protoDef in lstPrototypeDefinitions)
					{
						sb.Append(SimpleGenerator.Generate(protoDef));
					}

					tagger.InterpretCode(sb.ToString());

					return lstPrototypeDefinitions;
				}

				catch (Exception err)
				{
					Logs.LogError(err);
				}
			}

			return new List<PrototypeDefinition>();
		}


		public static async Task<List<PrototypeDefinition>> GenerateUnknownWordPrototypes2(string strDirective,
			ProtoScriptTagger tagger, bool bCheckPOS = true, bool bWriteToFile = true)
		{
			//First create any unknown words within the entity
			Prototype protoTokens = UnderstandUtil.Tokenize(strDirective);
			Prototype protoLexemes = UnderstandUtil.ConvertToLexemes(protoTokens);

			List<PrototypeDefinition> lstNewPrototypes = new List<PrototypeDefinition>();

			for (int i = 0; i < protoLexemes.Children.Count; i++)
			{
				Prototype protoToken = protoLexemes.Children[i];
				if (Prototypes.TypeOf(protoToken, System_String.Prototype))
				{
					string strToken = new StringWrapper(protoToken).GetStringValue();

					if (StringUtil.IsNumber(strToken))
					{
						//Skip numbers
						continue;
					}

					lstNewPrototypes.AddRange(await UnknownWords.CreateUnknownWordAsPrototypes2(strToken, strDirective, tagger));

					protoLexemes.Children[i] = TemporaryLexemes.GetLexemeByLexeme(strToken);
				}
			}

			if (bCheckPOS && lstNewPrototypes.Count == 0)
			{
				//Check for incorrectly tagged words instead 
				Prototype protoTagged = UnderstandUtil.Understand3(strDirective, tagger);
				JsonArray jsonArray = await UnknownWords.GetIncorrectlyTaggedWords(strDirective, protoTagged);

				//[
				//  {
				//    "errorType": "Wrong Part of Speech",
				//    "word": "barks"
				//  }
				//]
				foreach (JsonValue jsonValue in jsonArray)
				{
					string strErrorType = jsonValue.ToJsonObject().GetStringOrNull("errorType");
					string strWord = jsonValue.ToJsonObject().GetStringOrNull("word");

					if (strErrorType == "Wrong Part of Speech")
					{
						lstNewPrototypes.AddRange(await UnknownWords.CreateUnknownWordAsPrototypes2(strWord, strDirective, tagger));
					}
					else
					{
						Logs.DebugLog.WriteEvent("Untagged", strDirective);
					}
				}

			}

			if (bWriteToFile)
				WriteNewPrototypes(tagger, lstNewPrototypes);


			return lstNewPrototypes;
		}

		public static void WriteNewPrototypes(ProtoScriptTagger tagger, List<PrototypeDefinition> lstNewPrototypes)
		{
			string strProjectDirectory = StringUtil.LeftOfLast(tagger.ProjectFile, "\\");
			string strEnglishFile = FileUtil.BuildPath(strProjectDirectory, "LearnedEnglish.pts");


			foreach (PrototypeDefinition protoDef in lstNewPrototypes)
			{
				PrototypeDefinitionHelpers.InsertOrUpdatePrototypeDefinition(protoDef, strEnglishFile);
			}
		}
	}
}
