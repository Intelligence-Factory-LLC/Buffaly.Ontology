//added
using BasicUtilities;
using System.Text;

namespace Ontology.GraphInduction.Model
{
	public class HCPUtil
	{
		static public HCP Create(List<Prototype> lstPrototypes, Prototype protoShadow, string strNamespace)
		{
			HCP? hcp = HCPUtil.CreateHcpFromShadow(strNamespace, protoShadow, lstPrototypes, true);
			if (hcp != null)
				HCPs.Add(hcp);
			return hcp;
		}

		private static HCP? CreateHcpFromShadow(string strNamespace, Prototype shadow, List<Prototype> lstExamples, bool bIncludeInstances = true)
		{
			if (lstExamples == null || lstExamples.Count == 0)
				return null;

			if (shadow.PrototypeID == Hidden.Base.PrototypeID)
			{
				for (int i = 0; i < lstExamples.Count; i++)
				{
					Prototype protoProperty = lstExamples[i];
					lstExamples[i] = HCPs.Unpack(protoProperty);
				}

				shadow = PrototypeGraphLists.ComparePrototypes(lstExamples);
			}

			string strShortForm = PrototypeGraphs.GetHash(shadow);
			Prototype protoHiddenBase = Prototypes.GetOrInsertPrototype(strNamespace + "." + strShortForm);

			List<Prototype> paths = PrototypeGraphs.Parameterize(lstExamples, shadow, true);

			// If we include instances, infer HiddenShadow by comparing the instance graphs.
			// If not, at least set HiddenShadow to the base prototype so Paths keys are stable.
			Prototype hiddenShadow = protoHiddenBase;

			if (bIncludeInstances)
			{
				List<Prototype> instances = new List<Prototype>();

				foreach (Prototype prototype in lstExamples)
				{
					List<Prototype?> lstEntities = PrototypeGraphs.GetEntitiesOrNull(prototype, paths);
					Prototype protoHiddenInstance = Hidden.CreateHiddenInstance(protoHiddenBase, lstEntities);
					instances.Add(protoHiddenInstance);
				}

				if (instances.Count > 0)
					hiddenShadow = PrototypeGraphLists.ComparePrototypes(instances);
			}

			HCP hcp = new HCP();
			hcp.Shadow = shadow;
			hcp.HiddenShadow = hiddenShadow;

			for (int i = 0; i < paths.Count; i++)
			{
				Prototype protoPath = paths[i];
				hcp.Paths.Add(Prototypes.GetOrInsertPrototype(hcp.HiddenShadow.PrototypeName + ".Field." + i).PrototypeID, protoPath);
			}

			// Note: does not populate Expectations (same as old CreateHCPFromExtractedInstanceInfo)
			return hcp;
		}

	
	}
}
