using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ontology.GraphInduction.Model
{
	public class HCPTreeUtil
	{
		public static List<Prototype> GetLeafInstances(HCPTree.Node tree)
		{
			List<Prototype> lstInstances = HCPTrees.GetLeaves(tree).Select(x => x.Categorization).ToList();
			return lstInstances;
		}

		public static List<Prototype> GetLeavesAsHidden(HCPTree.Node tree)
		{
			//Convert each leaf to the Context on the tree, which should be a hidden
			List<Prototype> lstInstances = HCPTrees.GetLeaves(tree).Select(x => x.Categorization).ToList();
			List<Prototype> lstHiddens = new List<Prototype>();

			foreach (Prototype prototype in lstInstances)
			{
				Prototype protoHidden = HCPs.Convert(prototype, tree.Categorization);
				lstHiddens.Add(protoHidden);
			}

			return lstHiddens;
		}


		public static HCPTree.Node ExtractNewHCPs(string strNamespace, HCPTree.Node tree, bool bMaterialize = true)
		{
			Prototype protoShadow = tree.Categorization;

			if (!Prototypes.TypeOf(protoShadow, Hidden.Base.Prototype) && tree.Children.Count > 0)
			{
				List<Prototype> lstExamples = HCPTrees.GetLeaves(tree).Select(x => x.Categorization).ToList();
				HCP hcp = HCPUtil.Create(lstExamples, protoShadow, strNamespace);


				if (HCPs.HCPCollection.ContainsKey(hcp.HiddenShadow.PrototypeID))
					hcp = HCPs.Get(hcp.HiddenShadow.PrototypeID);

				if (!PrototypeGraphs.AreEqual(protoShadow, hcp.HiddenShadow))
				{
					tree.Categorization = hcp.HiddenShadow;
					tree.Unpacked = hcp.Shadow;
				}
				else
				{
					tree.Unpacked = hcp.Shadow;
				}

				HCPs.Add(hcp, bMaterialize);
			}

			foreach (HCPTree.Node node in tree.Children)
			{
				ExtractNewHCPs(strNamespace, node, bMaterialize);
			}

			return tree;
		}
	}
}
