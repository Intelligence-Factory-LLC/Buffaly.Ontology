using Ontology.Simulation;
using Ontology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicUtilities;

namespace ProtoScript.Tests.Helpers
{
	internal class ICDCodeHelper
	{
		public static List<string> GetPhrases(Prototype child)
		{
			List<string> result = new List<string>();
			string strDescription = child.Properties.GetStringOrDefault("ICD10CM.Code.Field.Description");
			if (!StringUtil.IsEmpty(strDescription))
				result.Add(strDescription);

			Prototype protoCollection = child.Properties["ICD10CM.Code.Field.Includes"];
			foreach (Prototype protoInclude in protoCollection?.Children ?? Enumerable.Empty<Prototype>())
			{
				string strInclude = StringWrapper.ToString(protoInclude);
				if (!StringUtil.IsEmpty(strInclude))
					result.Add(strInclude);
			}

			return result;
		}

		public static Prototype ? GetClinicalEntityByCode(string strCode)
		{
			Prototype protoClinicalEntity = TemporaryPrototypes.GetTemporaryPrototype("ClinicalOntology.ClinicalEntity");
			List<Prototype> lstCandidates = protoClinicalEntity.GetAllDescendantsWhere(x =>
				x.Properties.GetStringOrDefault("ClinicalOntology.ClinicalEntity.Field.CodeValue") == strCode).ToList();
			Prototype? protoCandidate = lstCandidates.FirstOrDefault();

			return protoCandidate;
		}
		public static Prototype? GetICDCodeByCode(string strCode)
		{
			Prototype protoClinicalEntity = TemporaryPrototypes.GetTemporaryPrototype("ICD10CM.Code");
			List<Prototype> lstCandidates = protoClinicalEntity.GetAllDescendantsWhere(x =>
				x.Properties.GetStringOrDefault("ICD10CM.Code.Field.CodeValue") == strCode).ToList();
			Prototype? protoCandidate = lstCandidates.FirstOrDefault();
			return protoCandidate;
		}

		public static Prototype? GetICDCategoryByCode(string strCode)
		{
			Prototype protoClinicalEntity = TemporaryPrototypes.GetTemporaryPrototype("ICD10CM.Code");
			List<Prototype> lstCandidates = protoClinicalEntity.GetAllDescendantsWhere(x =>
				x.Properties.GetStringOrDefault("ICD10CM.Category.Field.CategoryName") == strCode).ToList();
			Prototype? protoCandidate = lstCandidates.FirstOrDefault();
			return protoCandidate;
		}
	}
}
