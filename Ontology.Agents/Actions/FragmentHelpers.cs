using Buffaly.SemanticDB;
using Buffaly.SemanticDB.Data;

namespace Ontology.Agents.Actions
{
	public class FragmentHelpers
	{
		static public async Task<int?> GetClosestFragmentByTagID(string Search, string TagName)
		{
			return (await Fragments.GetMostSimilar2ByTagID(Search, TagName, 0.5)).FirstOrDefault()?.FragmentID;
		}


		static public async Task<List<Prototype>> GetClosestPrototypesByTagName(string Search, string TagName, double dThreshold = 0.5)
		{
			List<Prototype> lstPrototypes = new List<Prototype>();

			FragmentsDataTable dtFragments = await Fragments.GetMostSimilar2ByTagID(Search, TagName, dThreshold);
			foreach (FragmentsRow rowFragment in dtFragments)
			{
				Prototype? prototype = rowFragment.GetPrototype();
				if (null == prototype)
					continue;

				prototype.Value = rowFragment.DataObject.GetDoubleOrDefault("Similarity", 1);
				lstPrototypes.Add(prototype);
			}

			return lstPrototypes;
		}



		static public async Task<int?> GetClosestGoogleDocByTitle(string Search)
		{
			FragmentsRow ? rowTitle = (await Fragments.GetMostSimilar2ByTagID(Search, "Google Docs Title", 0.5)).FirstOrDefault();
			return rowTitle?.ParentFragmentID;
		}

		static public int? GetGoogleDocumentFragmentByDocumentID(string DocumentID)
		{
			ArtifactsRow ? rowArtifact = ArtifactsRepository.GetArtifactsByName(DocumentID).OrderByDescending(x => x.ArtifactID).FirstOrDefault();
			return rowArtifact?.FragmentID;
		}

		static public int? GetLeadFragmentByLeadID(int LeadID)
		{
			FragmentsRow? rowFragment = FragmentsRepository.GetFragmentByFragmentKey("Lead Info " + LeadID);
			return rowFragment?.FragmentID;
		}

		static public int? GetSalesRepresentativeFragmentByDay(int SalesRepresentativeID, string Day)
		{
			DateTime dtDate = DateTime.Parse(Day);
			string strFragmentKey = $"Sales Representative Daily Summary {SalesRepresentativeID} for {dtDate}";
			FragmentsRow? rowFragment = FragmentsRepository.GetFragmentByFragmentKey(strFragmentKey);
			return rowFragment?.FragmentID;
		}

	
	}
}
