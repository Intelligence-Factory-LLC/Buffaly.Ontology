using Ontology.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ontology.GraphInduction.Model
{
	public class HCPTree
	{
		public class Node
		{
			public Prototype Unpacked;

			public Prototype Context;
			public Prototype Categorization = null;

			//N20200812-01
			public Prototype Label = null;

			public List<Node> Children = new List<Node>();
			public HCPTree.Node Parent = null; 

			public void AddChild(Node node)
			{
				this.Children.Add(node);
				node.Parent = this;
			}

			public void SetChild(int i, Node node)
			{
				this.Children[i] = node;
				node.Parent = this;
			}

			public Node CloneSingle()
			{
				Node node = new Node();
				node.Unpacked = Unpacked?.Clone();
				node.Categorization  = Categorization?.Clone();
				node.Context = Context?.Clone();
				node.Label = Label?.Clone();

				return node;
			}

			public static HCPTree.Node FromPrototype(Prototype prototype)
			{
				HCPTree.Node node = Prototypes.FromPrototype(prototype) as HCPTree.Node;
				node = HCPTrees.SetParents(node);
				return node;
			}
		}
	}

	public class HCPTrees
	{
		static public bool AddNode(ref HCPTree.Node root, Prototype prototype)
		{
			if (null == root.Categorization)
			{
				//Empty tree, just set the categorization
				root.Categorization = prototype;
				return true;
			}

			HCPTree.Node node = new HCPTree.Node();
			node.Categorization = prototype;
			return AddNode(ref root, node);
		}

		static public bool AddNode(ref HCPTree.Node root, HCPTree.Node node)
		{
			//This version uses the local IsCategorized that deals with HCPs
			if (null == root)
			{
				root = node;
				return true;
			}

			Prototype prototype = node.Categorization;

			if (PrototypeGraphs.AreEqual(prototype, root.Categorization))
			{
				//N20200803-01 - Leaf already exists, can happen from a PCT 
				return true; 
			}

			if (!IsCategorized(root, prototype))
			{
				if (IsCategorized(node, root.Categorization))
				{
					bool bResult = AddNode(ref node, root);
					root = node;
				}
				else if (root.Parent == null)
				{
					//N20210223-01 - Try to add a new parent node 
					Prototype protoUnpacked1 = HCPs.Unpack(prototype);
					Prototype protoUnpacked2 = HCPs.Unpack(root.Categorization);

					Prototype ? protoNewShadow = PrototypeGraphs.ComparePrototypes(protoUnpacked1, protoUnpacked2);

					//N20260114-01 - If we have no common root, use Compare.Entity -- that works as a good common root
					HCPTree.Node nodeNew = new HCPTree.Node() { Categorization = protoNewShadow ?? Compare.Entity.Prototype };

					nodeNew.Children.Add(root);
					nodeNew.Children.Add(node);
					root.Parent = nodeNew;
					node.Parent = nodeNew;

					root = nodeNew;

					return true;
				}
				else
					return false;
			}

			if (node.Unpacked != null && root.Unpacked?.PrototypeID == node.Unpacked?.PrototypeID)
			{
				//N20191224-02
				if (root != node)
				{
					root.Categorization = node.Categorization;
					foreach (HCPTree.Node nodeChild in node.Children)
					{
						root.AddChild(nodeChild);
					}
				}
				return true;
			}

			//N20191224-03
			if (root.Unpacked == null && root.Children.Count > 0 && root.Children.All(x => IsCategorized(node, x.Categorization)))
			{
				//N20200819-02 - The root can have a Comparison operator, that gets replaced if we don't check
				//so use the more general here. We've already tested if the root is categorized by this example (above) and it is
				if (!IsCategorized(node, root.Categorization))
				{
					//leave the root it is more general. Another option is to insert this below the root and above all children
				}
				else
				{
					root.Unpacked = node.Unpacked;
					root.Categorization = node.Categorization;
				}
				return true;
			}

			bool bIsAdded = false;
			bool bIsAddedTopLevel = false;

			for (int i = 0; i < root.Children.Count; i++)
			{
				HCPTree.Node child = root.Children[i];
				bIsAdded = AddNode(ref child, node.CloneSingle()) || bIsAdded;

				//N20200120-01
				if (child != root.Children[i])
				{
					if (!bIsAddedTopLevel)
					{
						bIsAddedTopLevel = true;
						root.SetChild(i, child);
					}
					else
					{
						root.Children.RemoveAt(i);
						i--;
					}
				}
			}

			//N20260115-01 - We add refining shadows at this level if we cannot insert into any child subtree, this 
			//replaces the original algorithm that built a PCT tree first and then converted to HCP tree
			if (!bIsAdded)
			{
				// We failed to insert this node into any child subtree.
				// Now we attempt to create one or more refining intermediate shadows at THIS level
				// by comparing the new node against EACH existing child of root.

				Prototype protoNewUnpacked = HCPs.Unpack(node.Categorization);

				// We need a stable snapshot of the current children because we may rehome them.
				// This is not "accumulating state", it is just making iteration safe while mutating root.Children.
				List<HCPTree.Node> lstChildrenSnapshot = root.Children.ToList();

				// Track if we attached the new node under any refinement. If it attaches to none, it will become a leaf.
				bool bAttachedUnderAnyRefinement = false;

				foreach (HCPTree.Node child in lstChildrenSnapshot)
				{
					// Child might have already been rehomed by a previous refinement.
					// If it is no longer directly under root, skip it here.
					if (child.Parent != root)
						continue;

					Prototype protoChildUnpacked = HCPs.Unpack(child.Categorization);

					// Compare the new node with this child to see if they induce a refining shadow below root
					Prototype? protoShadow = PrototypeGraphs.ComparePrototypes(protoNewUnpacked, protoChildUnpacked);

					// If there is no common structure, treat the shadow as Compare.Entity.
					// That is not a useful refinement at this level.
					if (protoShadow == null)
						continue;

					// Determine if this protoShadow is a refinement relative to root:
					// 1) root categorizes shadow (shadow is below root)
					// 2) shadow does NOT categorize root (shadow is strictly more specific)
					// If shadow is equivalent to root (mutual categorization), it is NOT a refinement.
					if (!IsRefinementShadow(root.Categorization, protoShadow))
						continue;

					// Find an existing intermediate node under root that has the "same" categorization as protoShadow.
					// We do NOT use a dictionary. We do a linear scan.
					// Equality here is mutual categorization rather than AreEqual to tolerate Compare.Entity and unpacking.
					HCPTree.Node? mid = FindExistingRefinementChild(root, protoShadow);

					if (mid == null)
					{
						mid = new HCPTree.Node() { Categorization = protoShadow };
						mid.Parent = root;
						root.Children.Add(mid);
					}

					// Rehome this child if it belongs under the refinement.
					// Child belongs if the refinement categorizes the child.
					if (IsCategorized(mid, child.Categorization))
					{
						root.Children.Remove(child);
						child.Parent = mid;
						mid.Children.Add(child);
					}

					// Also attach the new node under this refinement if it belongs here.
					// Node can be added multiple times: we attach a clone under every refinement that categorizes it.
					if (IsCategorized(mid, node.Categorization))
					{
						HCPTree.Node nodeClone = node.CloneSingle();
						nodeClone.Parent = mid;
						mid.Children.Add(nodeClone);
						bAttachedUnderAnyRefinement = true;
					}
				}

				// If no refinement accepted the new node, add it as a leaf under root.
				// This matches your rule: if there are no refining shadows at this level (for this node), add as leaf.
				if (!bAttachedUnderAnyRefinement)
					root.AddChild(node);

				return true;
			}

			return true;

		}

		static private bool IsRefinementShadow(Prototype protoParent, Prototype protoShadow)
		{
			// shadow is a refinement if:
			// - parent categorizes shadow (shadow is inside parent)
			// - shadow does NOT categorize parent (shadow is strictly more specific)
			// Use Unpack to avoid HCP wrappers changing comparison structure.
			Prototype p = HCPs.Unpack(protoParent);
			Prototype s = HCPs.Unpack(protoShadow);

			bool parentCatShadow = TemporaryPrototypeCategorization.IsCategorized(s, p);
			bool shadowCatParent = TemporaryPrototypeCategorization.IsCategorized(p, s);

			return parentCatShadow && !shadowCatParent;
		}

		static private HCPTree.Node? FindExistingRefinementChild(HCPTree.Node root, Prototype protoShadow)
		{
			// Look only at direct children of root that are "refinement nodes" by mutual categorization.
			foreach (HCPTree.Node c in root.Children)
			{
				if (c == null)
					continue;

				// Only consider nodes that are direct children and that have a categorization.
				if (c.Parent != root || c.Categorization == null)
					continue;

				Prototype a = HCPs.Unpack(c.Categorization);
				Prototype b = HCPs.Unpack(protoShadow);

				bool aCatB = TemporaryPrototypeCategorization.IsCategorized(a, b);
				bool bCatA = TemporaryPrototypeCategorization.IsCategorized(b, a);

				if (aCatB && bCatA)
					return c;
			}

			return null;
		}


		static public void BreadthFirstWithControl(HCPTree.Node node, Func<HCPTree.Node, HCPTree.Node> func)
		{
			//This version allows the function to return the next object to operate on. Return a null to stop the search
			if (null != node)
			{
				node = func(node);

				if (null != node)
				{
					foreach (HCPTree.Node nodeChild in node.Children)
					{
						BreadthFirstWithControl(nodeChild, func);
					}
				}
			}
		}

		//static public HCPTree.Node Build(PrototypeCategorizationTree.Node root)
		//{
		//	HCPTree.Node tree = null;
		//	foreach (PrototypeCategorizationTree.Node node1 in PrototypeCategorizationTrees.GetAllNodes(root))
		//	{
		//		HCPTree.Node nodeTree = new HCPTree.Node();
		//		nodeTree.Categorization = node1.Categorization;
		//		AddNode(ref tree, nodeTree);
		//	}

		//	return tree;
		//}

		static public HCPTree.Node First(HCPTree.Node node, Predicate<HCPTree.Node> func)
		{
			if (func(node))
				return node;

			foreach (HCPTree.Node nodeChild in node.Children)
			{
				HCPTree.Node nodeResult = First(nodeChild, func);
				if (null != nodeResult)
					return nodeResult;
			}

			return null;
		}

		static public List<HCPTree.Node> FindByCategorization(HCPTree.Node root, Prototype prototype)
		{
			List<HCPTree.Node> lstResults = new List<HCPTree.Node>();

			if (null != root && IsCategorized(root, prototype))
			{
				bool bChild = false;
				foreach (HCPTree.Node node in root.Children)
				{
					List<HCPTree.Node> lstChildren = FindByCategorization(node, prototype);
					if (lstChildren.Count > 0)
					{
						bChild = true;
						lstResults.AddRange(lstChildren);
					}
				}

				if (!bChild)
					lstResults.Add(root);
			}

			return lstResults;
		}

		static public List<HCPTree.Node> FindByPartialCategorization(HCPTree.Node root, Prototype prototype)
		{
			List<HCPTree.Node> lstResults = new List<HCPTree.Node>();

			if (null != root && IsPartiallyCategorized(root, prototype))
			{
				bool bChild = false;
				foreach (HCPTree.Node node in root.Children)
				{
					List<HCPTree.Node> lstChildren = FindByPartialCategorization(node, prototype);
					if (lstChildren.Count > 0)
					{
						bChild = true;
						lstResults.AddRange(lstChildren);
					}
				}

				if (!bChild)
					lstResults.Add(root);
			}

			return lstResults;
		}

		public static List<HCPTree.Node> GetAllChildren(HCPTree.Node tree)
		{
			List<HCPTree.Node> lstAllChildren = new List<HCPTree.Node>();

			foreach (HCPTree.Node child in tree.Children)
			{
				lstAllChildren.Add(child);
				lstAllChildren.AddRange(GetAllChildren(child));
			}

			return lstAllChildren;
		}



		static public List<HCPTree.Node> GetLeaves(HCPTree.Node root)
		{
			List<HCPTree.Node> lstResults = new List<HCPTree.Node>();

			if (root.Children.Count > 0)
			{
				foreach (HCPTree.Node node in root.Children)
				{
					lstResults.AddRange(GetLeaves(node));
				}

			}
			else
			{
				lstResults.Add(root);
			}
			return lstResults;
		}

		//static public List<Prototype> GetLeavesAsHCP(HCPTree.Node root)
		//{
		//	List<Prototype> lstExamples = HCPTrees.GetLeaves(root).Select(x => x.Categorization).ToList();
		//	List<Prototype> lstInstances = new List<Prototype>();

		//	//Convert these examples to the current node's hidden
		//	foreach (Prototype protoExample in lstExamples)
		//	{
		//		if (!Prototypes.TypeOf(protoExample, root.Categorization))
		//			lstInstances.Add(HCPs.Convert(protoExample, root.Categorization));
		//		else
		//			lstInstances.Add(protoExample);
		//	}

		//	return lstInstances;
		//}


		static public bool IsCategorized(HCPTree.Node root, Prototype prototype)
		{
			Prototype protoShadow = root.Categorization;

			return IsCategorized(prototype, protoShadow);
		}

		static public bool IsCategorized(Prototype prototype, Prototype protoShadow)
		{
			if (protoShadow.PrototypeID == Hidden.Base.PrototypeID)
				return true;

			if (Prototypes.TypeOf(protoShadow, Hidden.Base.Prototype))
			{
				if (!Prototypes.TypeOf(prototype, Hidden.Base.Prototype))
				{
					HCP hcp = HCPs.Get(protoShadow.PrototypeID);

					//Check if the is the actual shadow or a leaf. For the shadow don't use Unpack here, we need the comparison operators
					if (PrototypeGraphs.AreEqual(hcp.HiddenShadow, protoShadow))
						protoShadow = HCPs.Get(protoShadow.PrototypeID).Shadow;
					else
						protoShadow = HCPs.Unpack(protoShadow);
				}

				//N2020065-03 - These can both be HCPs but different HCPs
				else if (!Prototypes.AreShallowEqual(protoShadow, prototype))
				{
					{
						HCP hcp = HCPs.Get(protoShadow.PrototypeID);

						if (null != hcp)
						{
							if (PrototypeGraphs.AreEqual(hcp.HiddenShadow, protoShadow))
								protoShadow = HCPs.Get(protoShadow.PrototypeID).Shadow;
							else
								protoShadow = HCPs.Unpack(protoShadow);
						}
					}

					{
						HCP hcp = HCPs.Get(prototype.PrototypeID);

						if (null != hcp)
						{
							if (PrototypeGraphs.AreEqual(hcp.HiddenShadow, prototype))
								prototype = HCPs.Get(prototype.PrototypeID).Shadow;
							else
								prototype = HCPs.Unpack(prototype);
						}
					}

				}

			}

			else if (Prototypes.TypeOf(prototype, Hidden.Base.Prototype))
			{
				prototype = HCPs.Unpack(prototype.Clone());
			}

			return TemporaryPrototypeCategorization.IsCategorized(prototype, protoShadow);
		}


		static public bool IsPartiallyCategorized(HCPTree.Node root, Prototype prototype)
		{
			Prototype protoShadow = root.Categorization;

			if (Prototypes.TypeOf(protoShadow, Hidden.Base.Prototype))
			{
				if (!Prototypes.TypeOf(prototype, Hidden.Base.Prototype))
				{
					HCP hcp = HCPs.Get(protoShadow.PrototypeID);

					//Check if the is the actual shadow or a leaf. For the shadow don't use Unpack here, we need the comparison operators
					if (PrototypeGraphs.AreEqual(hcp.HiddenShadow, protoShadow))
						protoShadow = HCPs.Get(protoShadow.PrototypeID).Shadow;
					else
						protoShadow = HCPs.Unpack(protoShadow);
				}

				//N2020065-03 - These can both be HCPs but different HCPs
				else if (!Prototypes.AreShallowEqual(protoShadow, prototype))
				{
					{
						HCP hcp = HCPs.Get(protoShadow.PrototypeID);

						if (PrototypeGraphs.AreEqual(hcp.HiddenShadow, protoShadow))
							protoShadow = HCPs.Get(protoShadow.PrototypeID).Shadow;
						else
							protoShadow = HCPs.Unpack(protoShadow);
					}

					{
						HCP hcp = HCPs.Get(prototype.PrototypeID);

						if (PrototypeGraphs.AreEqual(hcp.HiddenShadow, prototype))
							prototype = HCPs.Get(prototype.PrototypeID).Shadow;
						else
							prototype = HCPs.Unpack(prototype);
					}

				}

			}

			else if (Prototypes.TypeOf(prototype, Hidden.Base.Prototype))
			{
				prototype = HCPs.Unpack(prototype.Clone());
			}

			return IsPartiallyCategorized(prototype, protoShadow);
		}

		static public bool IsPartiallyCategorized(Prototype prototype, Prototype shadow)
		{
			if (shadow == null)
				return true;

			//N20210109-03 - Treat null as missing
			if (prototype == null)
				return true;

			if (!Prototypes.TypeOf(prototype, shadow))
				return false;

			foreach (var pair in prototype.Properties)
			{
				if (!IsPartiallyCategorized(pair.Value, shadow.Properties[pair.Key]))
					return false;
			}


			for (int i = 0; i < prototype.Children.Count && i < shadow.Children.Count; i++)
			{
				Prototype protoChild = prototype.Children[i];

				if (!IsPartiallyCategorized(protoChild, shadow.Children[i]))
					return false;
			}

			//N20200731-01 - Note: May need to look at StartsWith here for mismatched sizes
			if (prototype.Children.Count > shadow.Children.Count && shadow.Properties[Compare.Comparison.PrototypeID]?.PrototypeID == Compare.Exact.PrototypeID)
				return false;

			return true;
		}

		static public HCPTree.Node SetParents(HCPTree.Node tree)
		{
			foreach (HCPTree.Node node in tree.Children)
			{
				node.Parent = tree;
				SetParents(node);
			}

			return tree;
		}
	}
}
