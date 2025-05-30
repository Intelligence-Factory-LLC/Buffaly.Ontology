using BasicUtilities;
using Buffaly.NLU;
using Ontology;
using Ontology.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtoScript.Tests.Helpers
{
	internal class ICDCodeLearner
	{

		public static async Task ICDBranchToObjects(ProtoScriptTagger tagger, Prototype protoRoot)
		{
			int iFalsePositives = 0;
			int iIncorrect = 0;
			bool bAllowFixeFalsePositives = false;

			foreach (Prototype protoChild in protoRoot.GetAllDescendants())
			{
				try
				{
					string strConcept = protoChild.PrototypeName;
					string strChildCode = protoChild.Properties.GetStringOrDefault("ICD10CM.Code.Field.CodeValue");
					if (StringUtil.IsEmpty(strChildCode))
						continue;

					var tuple1 = VerifySingleCode(tagger, strChildCode, protoChild);
					if (tuple1.Item1)
					{
						Logs.DebugLog.WriteEvent("Correct", strChildCode);


						if (tuple1.Item2.Count > 0)
							iFalsePositives++;

						if (tuple1.Item2.Count > 0 && bAllowFixeFalsePositives)
						{
							await FixFalsePositives(tagger, strChildCode);
						}
						else
						{
							continue; //already done
						}
					}

					await ClinicalEntities.MapICD(protoChild, tagger);

					if (!VerifySingleCode(tagger, strChildCode, protoChild).Item1)
					{
						Logs.DebugLog.WriteEvent("Incorrect", strChildCode);
						//At this point it failed to learn
						iIncorrect++;
					}
				}
				catch (Exception ex)
				{
					Logs.LogError(ex);
				}
			}
		}

		private static Tuple<bool, List<Prototype>> VerifySingleCode(ProtoScriptTagger tagger, string strCode, Prototype protoEntity)
		{
			List<string> lstPhrases = ICDCodeHelper.GetPhrases(protoEntity);
			List<Prototype> lstFalsePositive = new List<Prototype>();

			foreach (string strPhrase in lstPhrases)
			{
				Prototype protoTagged = ClinicalEntities.TagPhraseToPrototype(tagger, strPhrase);

				// false positives first
				List<Prototype> lstLocalFalsePositives = GetFalsePositives(strCode, protoEntity, protoTagged);

				// detect correct mapping
				bool bCorrect = false;
				Prototype? protoCorrect = null;

				foreach (Prototype protoParent in protoTagged.GetAllParents()
															 .Where(x => x.TypeOf("ClinicalOntology.ClinicalEntity")))
				{
					Prototype protoClinicalObject = TemporaryPrototypes.GetTemporaryPrototypeOrNull(protoParent.PrototypeName);
					if (protoClinicalObject == null)
						continue;

					string strCurrentCode = protoClinicalObject.Properties.GetStringOrDefault2("CodeValue");
					if (strCurrentCode != strCode)
						continue;

					bCorrect = true;
					protoCorrect = protoClinicalObject;
					break;                                          // found the expected code
				}

				// log outcomes
				if (!bCorrect)
				{
					ClinicalEntities.DebugLog.WriteEvent("Unverified Phrase", strPhrase);
				}
				else if (lstLocalFalsePositives.Count > 0)
				{
					StringBuilder sb = new StringBuilder();
					sb.AppendLine("Phrase: " + strPhrase);
					sb.AppendLine("Correct Entity: ");
					sb.AppendLine((protoCorrect?.PrototypeName ?? "(none)") + " - " + strCode);
					sb.AppendLine("False Positive:");
					foreach (Prototype protoFalse in lstLocalFalsePositives)
					{
						sb.AppendLine(protoFalse.PrototypeName + " - " + protoFalse.Properties.GetStringOrDefault2("CodeValue"));
					}
					lstFalsePositive.AddRange(lstLocalFalsePositives);
					ClinicalEntities.DebugLog.WriteEvent("Unverified Phrase", sb.ToString());
				}
			}

			return new Tuple<bool, List<Prototype>>(true, lstFalsePositive);
		}


		public static List<Prototype> GetFalsePositives(string strCode,
														 Prototype protoEntity,
														 Prototype protoTagged)
		{
			List<Prototype> lstFalsePositive = new List<Prototype>();

			foreach (Prototype protoParent in protoTagged.GetAllParents()
														 .Where(x => x.TypeOf("ClinicalOntology.ClinicalEntity")))
			{
				Prototype protoClinicalObject = TemporaryPrototypes.GetTemporaryPrototypeOrNull(protoParent.PrototypeName);
				if (protoClinicalObject == null)
					continue;

				string strOtherCode = protoClinicalObject.Properties.GetStringOrDefault2("CodeValue");

				// ignore: same code or missing code
				if (StringUtil.IsEmpty(strOtherCode) || strOtherCode == strCode)
					continue;

				// ignore: protoEntity already descends from the other code
				Prototype protoOtherCode = ICDCodeHelper.GetICDCodeByCode(strOtherCode);
				if (protoEntity.TypeOf(protoOtherCode))
					continue;

				lstFalsePositive.Add(protoClinicalObject);
			}

			return lstFalsePositive;
		}

		public static async Task<bool> FixFalsePositives(ProtoScriptTagger tagger, string strCode)
		{
			bool bDifferentiated = false;
			Prototype? protoEntity = ICDCodeHelper.GetClinicalEntityByCode(strCode);
			Prototype? protoCode = ICDCodeHelper.GetICDCodeByCode(strCode);
			List<string> lstPhrases = ICDCodeHelper.GetPhrases(protoCode);

			foreach (string strPhrase in lstPhrases)
			{
				Prototype protoTagged = ClinicalEntities.TagPhraseToPrototype(tagger, strPhrase);
				List<Prototype> lstFalsePositives = GetFalsePositives(strCode, protoEntity, protoTagged);

				foreach (Prototype protoFalsePositive in lstFalsePositives)
				{
					string strOtherCode = protoFalsePositive.Properties.GetStringOrDefault2("CodeValue");

					Prototype protoOtherCode = ICDCodeHelper.GetICDCodeByCode(strOtherCode);
					if (protoCode.TypeOf(protoOtherCode))
						continue; //parents sometimes categorize

					List<string> lstOtherPhrases = ICDCodeHelper.GetPhrases(protoOtherCode);

					foreach (string strPhrase2 in lstOtherPhrases)
					{
						Prototype protoTagged2 = ClinicalEntities.TagPhraseToPrototype(tagger, strPhrase2);

						bDifferentiated = await DifferentiateFeatures(protoTagged, protoTagged2, protoOtherCode, tagger);
					}

				}
			}

			return bDifferentiated;
		}

		private static async Task<bool> DifferentiateFeatures(Prototype protoTagged, Prototype protoTagged2, Prototype protoCode2, ProtoScriptTagger tagger)
		{
			//Move the features in protoTagged2 further way by making them more specific (if possible)
			bool bChanged = false;

			List<Prototype> lstFeatures = BagOfFeatures.GetFeatures(protoTagged);
			List<Prototype> lstPrimaryFeatures = BagOfFeatures.GetPrimaryFeatures(protoTagged);

			//False positive (should have extra features)
			List<Prototype> lstFeatures2 = BagOfFeatures.GetFeatures(protoTagged2);
			List<Prototype> lstPrimaryFeatures2 = BagOfFeatures.GetPrimaryFeatures(protoTagged2);

			List<Prototype> lstExtraFeatures = lstFeatures2.Where(x => !lstFeatures.Any(y => x.PrototypeID == y.PrototypeID)).ToList();
			List<Prototype> lstExtraPrimaryFeatures = BagOfFeatures.GetPrimaryFeatures(lstExtraFeatures);

			List<string> lstExtraLexemes = new List<string>();
			foreach (Prototype protoExtraFeature in lstExtraFeatures)
			{
				foreach (Prototype protoLexeme in TemporaryLexemes.GetLexemesByRelatedPrototype(protoExtraFeature, false))
				{
					TemporaryLexemesRow rowLexeme = TemporaryLexemes.GetTemporaryLexeme(protoLexeme);
					lstExtraLexemes.Add(rowLexeme.Lexeme);
				}
			}

			foreach (string strLexeme in lstExtraLexemes)
			{
				ClinicalEntities.UncacheWordForm(strLexeme);
				bChanged = true;
			}

			if (bChanged)
				await ClinicalEntities.MapICD(protoCode2, tagger);

			return bChanged;
		}
	}
}
