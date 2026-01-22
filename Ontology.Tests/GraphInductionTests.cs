using BasicUtilities;
using BasicUtilities.Collections;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ontology.GraphInduction;
using Ontology.GraphInduction.Model;
using Ontology.GraphInduction.Utils;
using Ontology.Simulation;
using ProtoScript.Interpretter;
using ProtoScript.Interpretter.Symbols;

namespace Ontology.Tests
{
	[TestClass]
	public sealed class GraphInductionTests
	{
		[TestInitialize]
		public void TestInit()
		{
			var builder = new ConfigurationBuilder();
			builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
			var config = builder.Build();

			Logs.LogSettings? logSettings = config.GetSection("LogSettings").Get<Logs.LogSettings>();
			if (null == logSettings)
				throw new Exception("Could not load configuration: LogSettings");

			Logs.Config(logSettings);


			BasicUtilities.Settings.SetAppSettings(config);

			Initializer.Initialize();

			//	Initializer.SetupDatabaseDisconnectedMode();
		}

		[TestMethod]
		public void Test_GraphInductionEngine_1()
		{
			string strFile = @"c:\dev\FairPath\FairPath.Data\Addresses.cs";
			string strCode = @"
  public partial class AddressesRow
    {
        public AddressesRow(AddressesRow oRow)
        {
                SqlParams sqlParams = new SqlParams();

                SqlParams sqlParams = new SqlParams();

        }

   
        public static int InsertAddress(
            int PatientID,
            string? Name,
            string? Line1,
            string? Line2,
            string? City,
            string? State,
            string? Zip,
            string? Country,
            string? Data)
        {
            int iAddressID = 0;

            try
            {
                string strStoredProc = ""InsertAddressSp"";

                SqlParams sqlParams = new SqlParams();

                sqlParams.Add(DataAccess.Params.ID(""@PatientID"", PatientID));

                sqlParams.Add(DataAccess.Params.String(""@Name"", Name));

                sqlParams.Add(DataAccess.Params.String(""@Line1"", Line1));

                sqlParams.Add(DataAccess.Params.String(""@Line2"", Line2));

                sqlParams.Add(DataAccess.Params.String(""@City"", City));

                sqlParams.Add(DataAccess.Params.String(""@State"", State));

                sqlParams.Add(DataAccess.Params.Zip(""@Zip"", Zip));

                sqlParams.Add(DataAccess.Params.String(""@Country"", Country));

                sqlParams.Add(DataAccess.Params.Text(""@Data"", Data));

                iAddressID = DataAccess.IntFromProc(strStoredProc, sqlParams, ""AddressID"");
            }

            catch (SqlException err)
            {
                if (err.Message.Contains(""Cannot insert duplicate key row in object""))
                    throw new RooTrax.Common.DB.InsertFailedException(""Cannot insert Address since it already exists ("" + StringUtil.Between(err.Message, ""("", "")"") + "")"", err);

                throw;
            }

            finally
            {

            }

            return iAddressID;
        }

        public static void UpdateAddress(
            int AddressID,
            int PatientID,
            string? Name,
            string? Line1,
            string? Line2,
            string? City,
            string? State,
            string? Zip,
            string? Country,
            string? Data)
        {
            try
            {
                string strStoredProc = ""UpdateAddressSp"";

                SqlParams sqlParams = new SqlParams();

                sqlParams.Add(DataAccess.Params.ID(""@AddressID"", AddressID));

                sqlParams.Add(DataAccess.Params.ID(""@PatientID"", PatientID));

                sqlParams.Add(DataAccess.Params.String(""@Name"", Name));

                sqlParams.Add(DataAccess.Params.String(""@Line1"", Line1));

                sqlParams.Add(DataAccess.Params.String(""@Line2"", Line2));

                sqlParams.Add(DataAccess.Params.String(""@City"", City));

                sqlParams.Add(DataAccess.Params.String(""@State"", State));

                sqlParams.Add(DataAccess.Params.Zip(""@Zip"", Zip));

                sqlParams.Add(DataAccess.Params.String(""@Country"", Country));

                sqlParams.Add(DataAccess.Params.Text(""@Data"", Data));

                DataAccess.ExecProc(strStoredProc, sqlParams);

                if (IsCachingEnabled)
                {
                    Cache.Invalidate(AddressID);
                }
            }

            catch (SqlException err)
            {
                if (err.Message.Contains(""Cannot insert duplicate key row in object""))
                    throw new RooTrax.Common.DB.InsertFailedException(""Cannot insert Address since it already exists ("" + StringUtil.Between(err.Message, ""("", "")"") + "")"", err);

                throw;
            }

            finally
            {

            }

        }
}
";


			CSharp.File file = CSharp.Parsers.Files.Parse(strFile);
			Prototype protoFile = NativeValuePrototypes.ToPrototype(file) ?? throw new InvalidOperationException("Failed to convert file to prototype.");


			CSharp.ClassDefinition clsAddresses = CSharp.Parsers.ClassDefinitions.Parse(strCode);

			Prototype protoAddresses = NativeValuePrototypes.ToPrototype(clsAddresses)!;
//			Prototype protoAddresses = protoFile;
			Logger.Log(protoAddresses);

			Prototype protoRoot = protoAddresses.Clone();

			//1. Mine candidates from the current graph view.
			List<Prototype> lstLeafPaths = LeafBasedTransforms.GetEntityPathsByLeaf2(protoRoot);

			Prototype protoLeaf1 = lstLeafPaths[2];
			Prototype protoLeaf2 = lstLeafPaths[6];

			Logger.Log(protoLeaf1);
			Logger.Log(protoLeaf2);

			Prototype protoParent1 = protoLeaf1.Parent;
			Prototype protoParent2 = protoLeaf2.Parent;

			Logger.Log(protoParent1);
			Logger.Log(protoParent2);

			PrototypeLogging.Log(protoParent1);
			PrototypeLogging.Log(protoParent2);

			bool bCategorized = TemporaryPrototypeCategorization.IsCategorized(protoParent2, protoParent1, true);
			bool bDestructive = true;

			//Generalize the graph, longest paths first, to remove any shorter common paths 
			foreach (Prototype protoPath in lstLeafPaths)
			{
				Collection lstInstances = PrototypeGraphs.Find(protoRoot, x => PrototypeGraphs.AreEqual(x, protoPath));

				foreach (Prototype protoInstance in lstInstances.Children)
				{
					if (!bDestructive)
					{
						protoInstance.IsMutable = true;
						protoInstance.InsertTypeOf(Compare.Entity.Prototype);
					}
					else
					{
						Revalue(protoInstance, Compare.Entity.Prototype);
					}
				}
			}

			PrototypeLogging.Log(protoRoot);

			List<Prototype> lstPaths2 = PrototypeGraphs.Parameterize(protoAddresses, protoRoot, true);

			List<Prototype?> lstValues = lstPaths2.Select(x => PrototypeGraphs.GetValue(protoAddresses, x)).ToList();

			Logger.Log(lstValues);

			Logger.Log(protoRoot);

			//Testing repeated mining
			Prototype protoCollection = new Collection();
			foreach (Prototype proto in lstValues)
			{
				if (!protoCollection.Children.Any(x => PrototypeGraphs.AreEquivalentCircular(x, proto)))
					protoCollection.Children.Add(proto);
			}
			

			Logger.Log(protoCollection);

			//2. Learn HCPs from repeated anchored structure.
			List<Prototype> lstLeafPaths2 = LeafBasedTransforms.GetEntityPathsByLeaf2(protoCollection);


			Prototype protoCollection2 = new Collection();
			foreach (Prototype proto in lstLeafPaths2)
			{
				if (!protoCollection2.Children.Any(x => PrototypeGraphs.AreEquivalentCircular(x, proto)))
					protoCollection2.Children.Add(proto);
			}

			Logger.Log(protoCollection2);

			Prototype protoCollectionOriginal = protoCollection.Clone();
			protoCollectionOriginal.SetParents();


			List<int> lstCounts = new List<int>();
			//N20260113-01 - Take the new entities an open holes 
			foreach (Prototype protoPath in protoCollection2.Children)
			{
				Collection lstInstances = PrototypeGraphs.Find(protoCollection, x => PrototypeGraphs.AreEqual(x, protoPath));
				lstCounts.Add(lstInstances.Children.Count);

				foreach (Prototype protoInstance in lstInstances.Children)
				{
					if (!bDestructive)
					{
						protoInstance.IsMutable = true;
						protoInstance.InsertTypeOf(Compare.Entity.Prototype);
					}
					else
					{
						Revalue(protoInstance, Compare.Entity.Prototype);
					}
				}
			}

			Logger.Log(protoCollection);
			var ordered = Enumerable
				.Range(0, lstCounts.Count)
				.Select(i => new
				{
					Count = lstCounts[i],
					Proto = protoCollection2.Children[i]
				})
				.OrderByDescending(x => x.Count);

			Prototype? protoTop1 = null;
			Prototype? protoTop2 = null;

			foreach (var item in ordered)
			{
				if (null == protoTop1)
					protoTop1 = item.Proto;


				else if (null == protoTop2)
					protoTop2 = item.Proto;

				Logs.DebugLog.WriteEvent(
					"Count: " + item.Count,
					FormatUtil.FormatPrototype(item.Proto).ToString()
				);
			}

			//Let's compare the top 2 recurring entities: sqlParams.Add vs System.String[string]
			Collection lstInstances1 = PrototypeGraphs.Find(protoCollectionOriginal, x => PrototypeGraphs.AreEqual(x, protoTop1));
			Collection lstInstances2 = PrototypeGraphs.Find(protoCollectionOriginal, x => PrototypeGraphs.AreEqual(x, protoTop2));

			foreach (Prototype protoInstance in lstInstances1.Children)
			{
				List<Prototype> lstPathToRoot = LeafBasedTransforms.GetPathToRoot(protoInstance);
				Prototype protoInstanceRoot = lstPathToRoot[lstPathToRoot.Count - 2]; //second to last

				Logger.Log(protoInstanceRoot);
			}


			foreach (Prototype protoInstance in lstInstances2.Children)
			{
				List<Prototype> lstPathToRoot2 = LeafBasedTransforms.GetPathToRoot(protoInstance);
				Prototype protoInstanceRoot2 = lstPathToRoot2[lstPathToRoot2.Count - 2]; //second to last

				Logger.Log(protoInstanceRoot2);
			}


			//Build an HCPTree for the first recurring entity
			HCPTree.Node root1 = new HCPTree.Node();

			foreach (Prototype protoInstance in lstInstances1.Children)
			{
				List<Prototype> lstPathToRoot = LeafBasedTransforms.GetPathToRoot(protoInstance);
				Prototype protoInstanceRoot = lstPathToRoot[lstPathToRoot.Count - 2]; //second to last

				HCPTrees.AddNode(ref root1, protoInstanceRoot);
			}

			Logger.Log(root1);

			//Build an HCPTree for the second recurring entity
			HCPTree.Node root2 = new HCPTree.Node();

			foreach (Prototype protoInstance in lstInstances2.Children)
			{
				List<Prototype> lstPathToRoot2 = LeafBasedTransforms.GetPathToRoot(protoInstance);
				Prototype protoInstanceRoot2 = lstPathToRoot2[lstPathToRoot2.Count - 2]; //second to last
				HCPTrees.AddNode(ref root2, protoInstanceRoot2);
			}

			Logger.Log(root2);


			root1 = HCPTreeUtil.ExtractNewHCPs(Dimensions.CSharp_Code_Hidden, root1);
			Logger.Log(root1);

			root2 = HCPTreeUtil.ExtractNewHCPs(Dimensions.CSharp_Code_Hidden, root2);
			Logger.Log(root2);


			//From each tree extract an label all of the entities. 
			List<HCPTree.Node> nodes1 = HCPTrees.GetNonLeaves(root1);
			List<Prototype> lstEntities = new List<Prototype>();
			foreach (HCPTree.Node node in nodes1)
			{
				List<Prototype> lstLeaves = HCPTreeUtil.GetLeavesAsHidden(node);
				lstEntities.AddRange(lstLeaves);
			}

			List<HCPTree.Node> nodes2 = HCPTrees.GetNonLeaves(root2);
			foreach (HCPTree.Node node in nodes2)
			{
				List<Prototype> lstLeaves = HCPTreeUtil.GetLeavesAsHidden(node);
				lstEntities.AddRange(lstLeaves);
			}

			//This just extracts entities, it doesn't do any equivalence to find latent subtypes. 
			Map<int, List<Prototype>> mapECHPs = ExtractEntityCentricHiddenPrototypesHierarchies(lstEntities);

			foreach (var pair in mapECHPs)
			{
				Logs.DebugLog.WriteEvent(Prototypes.GetPrototypeName(pair.Key), PrototypeLogging.ToChildString(new Collection(pair.Value)));
			}

			//N20260118-01- New mechanism to find latent subtypes. The old did exact or partial set overlaps. This does Jaccard similarity 
			//Finding latent subtypes 
			//Add associations between each entity and the slots
			PrototypeSet setSlots = new PrototypeSet();
			foreach (var pair in mapECHPs)
			{
				Prototype protoSlotPrototype = Prototypes.GetPrototype(pair.Key);
				setSlots.Add(protoSlotPrototype);
				foreach (Prototype protoEntity in pair.Value)
				{
					//Get the singleton
					Prototype protoEntitySingleton = Prototypes.GetPrototype(protoEntity.PrototypeID);
					protoSlotPrototype.BidirectionalAssociate(protoEntitySingleton);
				}
			}


			foreach (Prototype prototype in setSlots)
			{
				foreach (var tuple in prototype.Associations)
				{
					Logs.DebugLog.WriteEvent("Association", prototype.PrototypeName + " -> " + tuple.Key.PrototypeName + " (" + tuple.Value + ")");
				}
			}

			//Do a two step hop from a slot to find the largest overlapping sets
			foreach (Prototype slot in setSlots)
			{
				// Hop 1: slot -> entities
				List<Prototype> lstEntities1 = slot.Associations.Select(x => x.Key).ToList();

				// Intersection counts: otherSlotId -> count shared entities
				Map<int, int> mapIntersect = new Map<int, int>();

				foreach (Prototype entity in lstEntities1)
				{
					// Hop 2: entity -> slots (since edges are bidirectional)
					foreach (var assoc2 in entity.Associations)
					{
						Prototype other = assoc2.Key;

						// Only count other nodes that are also slots we are tracking
						if (!setSlots.Contains(other))
							continue;

						// Ignore self
						if (other.PrototypeID == slot.PrototypeID)
							continue;

						if (mapIntersect.ContainsKey(other.PrototypeID))
							mapIntersect[other.PrototypeID] = mapIntersect[other.PrototypeID] + 1;
						else
							mapIntersect[other.PrototypeID] = 1;
					}
				}

				// Optional: compute Jaccard for ranking (unweighted)
				int nA = slot.Associations.Count;

				// Build ranked results
				var ranked = mapIntersect
					.Select(pair =>
					{
						Prototype otherSlot = Prototypes.GetPrototype(pair.Key);
						int inter = pair.Value;

						int nB = otherSlot.Associations.Count;
						int union = nA + nB - inter;
						double j = union > 0 ? (double)inter / (double)union : 0.0;

						return new
						{
							OtherSlot = otherSlot,
							Intersection = inter,
							Jaccard = j,
							OtherCount = nB
						};
					})
					.OrderByDescending(x => x.Jaccard)
					.ThenByDescending(x => x.Intersection)
					.ToList();

				// Log top overlaps for this slot
				foreach (var res in ranked.Take(10))
				{
					Logs.DebugLog.WriteEvent(
						"Overlap",
						slot.PrototypeName + " ↔ " + res.OtherSlot.PrototypeName +
						$" |∩|={res.Intersection} |A|={nA} |B|={res.OtherCount} J={res.Jaccard:F3}"
					);
				}
			}


			// ---- Call it for the specific overlaps you logged ----

			Prototype slot_AddVar = Prototypes.GetPrototypeByPrototypeName("CSharp.Code.Hidden.5EB2F9E1745589129389BD4E5FFD4CE4.Field.1");
			Prototype slot_Param = Prototypes.GetPrototypeByPrototypeName("CSharp.Code.Hidden.78BA054917E3C80A2C50BEBAB5AAB875.Field.0");
			Prototype slot_StrVar = Prototypes.GetPrototypeByPrototypeName("CSharp.Code.Hidden.5A141C5500096E9FE1309E1E7482038F.Field.1");
			Prototype slot_AddLit = Prototypes.GetPrototypeByPrototypeName("CSharp.Code.Hidden.5EB2F9E1745589129389BD4E5FFD4CE4.Field.0");
			Prototype slot_StrLit = Prototypes.GetPrototypeByPrototypeName("CSharp.Code.Hidden.5A141C5500096E9FE1309E1E7482038F.Field.0");

			DumpOverlap(mapECHPs, slot_AddVar, slot_Param);
			DumpOverlap(mapECHPs, slot_AddVar, slot_StrVar);
			DumpOverlap(mapECHPs, slot_AddLit, slot_StrLit);
			DumpOverlap(mapECHPs, slot_Param, slot_StrVar);


			//Finding one to one relationships (bijections)
			//unsolved


			//N20260120-01 - Computing metrics
			//Calculate compression gain on each HCP 
			LogCompressionGainPerTree("sqlParams.Add tree", root1);
			LogCompressionGainPerTree("parameter declaration tree", root2);

			//Calculate next statement prediction 
			LogNextStatementPredictionByLevel("sqlParams.Add tree", protoAddresses, root1);
			LogNextStatementPredictionByLevel("parameter declaration tree", protoAddresses, root2);


			//Calculate prediction of each slot sequence
			LogSlotLevelNextEntityPredictionByHcpLevel("sqlParams.Add tree", protoAddresses, root1);
			LogSlotLevelNextEntityPredictionByHcpLevel("parameter declaration tree", protoAddresses, root2);

		}

		public sealed class SlotSeqStats
		{
			public Prototype Hcp = null!;
			public int SlotId;
			public string SlotName = "";
			public int Applies;   // number of slot transitions observed
			public int UniqueFrom;
			public int UniqueTo;
		}

		public static void LogSlotLevelNextEntityPredictionByHcpLevel(
			string label,
			Prototype protoRoot,
			HCPTree.Node hcpTreeRoot,
			int topFrom = 20,
			int topTo = 10)
		{
			Logs.DebugLog.WriteEvent("SlotSeq", $"=== {label} ===");

			List<HCPTree.Node> nodes = HCPTrees
				.GetNonLeaves(hcpTreeRoot)
				.Where(x => x.Categorization != null)
				.ToList();

			// Order by tree depth, then by unpacked size
			nodes = nodes
				.OrderBy(x => GetDepth(x))
				.ThenByDescending(x => PrototypeGraphs.Size(x.Unpacked))
				.ToList();

			foreach (HCPTree.Node node in nodes)
			{
				// Get slot ids for this HCP level from the hidden instances under this node.
				// This avoids any assumptions about how fields are represented in the prototype.
				List<Prototype> hiddenLeaves = HCPTreeUtil.GetLeavesAsHidden(node);

				HashSet<int> slotIds = new HashSet<int>();
				foreach (Prototype hidden in hiddenLeaves)
					foreach (var prop in hidden.Properties)
						slotIds.Add(prop.Key);

				if (slotIds.Count == 0)
					continue;

				string indent = new string(' ', GetDepth(node) * 2);

				Logs.DebugLog.WriteEvent(
					"SlotSeq",
					indent + node.Categorization.PrototypeName + " Slots=" + slotIds.Count);

				foreach (int slotId in slotIds.OrderBy(x => x))
				{
					Map<int, Map<int, int>> counts = CountSlotEntityTransitions(protoRoot, node, slotId);

					int applies = 0;
					HashSet<int> setFrom = new HashSet<int>();
					HashSet<int> setTo = new HashSet<int>();

					foreach (var from in counts)
					{
						setFrom.Add(from.Key);
						foreach (var to in from.Value)
						{
							setTo.Add(to.Key);
							applies += to.Value;
						}
					}

					Prototype slotProto = Prototypes.GetPrototype(slotId);

					Logs.DebugLog.WriteEvent(
						"SlotSeq",
						indent + "  " + slotProto.PrototypeName +
						" Applies=" + applies +
						" UniqueFrom=" + setFrom.Count +
						" UniqueTo=" + setTo.Count);

					LogTopFollowers(indent + "    ", counts, topFrom, topTo);
				}
			}
		}

		// For one HCP node and one slot id, build a Markov table:
		// entityA -> entityB counts, where entities are the fillers of that slot
		// in consecutive categorized statements under the same parent.Children ordering.
		private static Map<int, Map<int, int>> CountSlotEntityTransitions(
			Prototype protoRoot,
			HCPTree.Node node,
			int slotPrototypeId)
		{
			Map<int, Map<int, int>> counts = new Map<int, Map<int, int>>();

			List<Prototype> parents = PrototypeGraphs.FindUniqueParents(protoRoot, child =>
				TemporaryPrototypeCategorization.IsCategorized(child, node.Unpacked));

			foreach (Prototype parent in parents)
			{
				Prototype? prevEnt = null;

				for (int i = 0; i < parent.Children.Count; i++)
				{
					Prototype stmt = parent.Children[i];

					if (!TemporaryPrototypeCategorization.IsCategorized(stmt, node.Unpacked))
						continue;

					Prototype hidden = HCPs.Convert(stmt, node.Categorization);

					if (!hidden.Properties.ContainsKey(slotPrototypeId))
						continue;

					Prototype? filler = hidden.Properties[slotPrototypeId];
					if (filler == null)
						continue;

					Prototype ent = Prototypes.GetPrototype(filler.PrototypeID);

					if (prevEnt != null)
						Increment(counts, prevEnt.PrototypeID, ent.PrototypeID);

					prevEnt = ent;
				}
			}

			return counts;
		}

		private static void Increment(Map<int, Map<int, int>> counts, int fromId, int toId)
		{
			Map<int, int> inner;
			if (counts.ContainsKey(fromId))
				inner = counts[fromId];
			else
				inner = new Map<int, int>();

			if (inner.ContainsKey(toId))
				inner[toId] = inner[toId] + 1;
			else
				inner[toId] = 1;

			counts[fromId] = inner;
		}

		private static void LogTopFollowers(string indent, Map<int, Map<int, int>> counts, int topFrom, int topTo)
		{
			var rankedFrom = counts
				.Select(p =>
				{
					int total = 0;
					foreach (var q in p.Value)
						total += q.Value;

					return new { FromId = p.Key, Total = total };
				})
				.OrderByDescending(x => x.Total)
				.Take(topFrom)
				.ToList();

			foreach (var from in rankedFrom)
			{
				Prototype fromEnt = Prototypes.GetPrototype(from.FromId);

				Logs.DebugLog.WriteEvent("SlotSeq", indent + fromEnt.PrototypeName + " Total=" + from.Total);

				var rankedTo = counts[from.FromId]
					.OrderByDescending(x => x.Value)
					.Take(topTo)
					.ToList();

				foreach (var to in rankedTo)
				{
					Prototype toEnt = Prototypes.GetPrototype(to.Key);

					Logs.DebugLog.WriteEvent(
						"SlotSeq",
						indent + "  -> " + toEnt.PrototypeName + " Count=" + to.Value);
				}
			}
		}



		public static void LogNextStatementPredictionByLevel(
	string label,
	Prototype protoRoot,
	HCPTree.Node hcpTreeRoot)
		{
			Logs.DebugLog.WriteEvent("NextStatement", $"=== {label} ===");

			List<HCPTree.Node> nodes = HCPTrees
				.GetNonLeaves(hcpTreeRoot)
				.Where(x => x.Categorization != null)
				.ToList();

			// Order by depth so output is “by level” without a recursive walk.
			nodes = nodes
				.OrderBy(x => GetDepth(x))
				.ThenByDescending(x => PrototypeGraphs.Size(x.Unpacked))
				.ToList();

			foreach (HCPTree.Node node in nodes)
			{
				int depth = GetDepth(node);
				int bits = PrototypeGraphs.Size(node.Unpacked);

				int applies = 0;
				int correct = 0;
				int incorrect = 0;

				List<Prototype> parents = PrototypeGraphs.FindUniqueParents(protoRoot, x =>
					TemporaryPrototypeCategorization.IsCategorized(x, node.Unpacked));

				foreach (Prototype parent in parents)
				{
					for (int i = 0; i < parent.Children.Count - 1; i++)
					{
						Prototype cur = parent.Children[i];
						Prototype nxt = parent.Children[i + 1];

						if (!TemporaryPrototypeCategorization.IsCategorized(cur, node.Unpacked))
							continue;

						applies++;

						if (TemporaryPrototypeCategorization.IsCategorized(nxt, node.Unpacked))
							correct++;
						else
							incorrect++;
					}
				}

				double acc = applies > 0 ? (double)correct / (double)applies : 0.0;
				int score = correct * bits;

				string indent = new string(' ', depth * 2);

				Logs.DebugLog.WriteEvent(
					"NextStatement",
					indent +
					node.Categorization.PrototypeName +
					" Bits=" + bits +
					" Applies=" + applies +
					" Correct=" + correct +
					" Incorrect=" + incorrect +
					" Acc=" + acc.ToString("F3") +
					" Score=" + score);
			}
		}



		private static int GetDepth(HCPTree.Node node)
		{
			int depth = 0;
			HCPTree.Node? cur = node;

			while (cur.Parent != null)
			{
				depth++;
				cur = cur.Parent;
			}

			return depth;
		}


		static void LogCompressionGainPerTree(string label, HCPTree.Node root)
		{
			Logs.DebugLog.WriteEvent("CompressionGain", $"=== {label} ===");

			void Walk(HCPTree.Node node, int depth)
			{
				if (node.Categorization != null && node.Children.Count > 0)
				{
					int gain = ComputeCompressionGain(node);
					string indent = new string(' ', depth * 2);

					Logs.DebugLog.WriteEvent(
						"CompressionGain",
						indent +
						node.Categorization.PrototypeName +
						" Gain=" + gain);
				}

				foreach (HCPTree.Node child in node.Children)
					Walk(child, depth + 1);
			}

			Walk(root, 0);
		}



		// Computes compression gain for each non-leaf HCP node.
		//
		// Gain is defined as the total size of concrete instances under the node
		// minus the cost of encoding them as one unpacked HCP skeleton plus the
		// sizes of the hidden instances (hole contents).
		//
		// Uses PrototypeGraphs.Size for all costs and avoids unit-cost holes.
		public static int ComputeCompressionGain(HCPTree.Node node)
		{
			// Computes compression gain for a single HCP tree node.

			Prototype hcp = node.Categorization;
			if (hcp == null)
				return 0;

			// Baseline: store each concrete instance directly
			int baseline = 0;
			List<Prototype> leavesConcrete = HCPTreeUtil.GetLeafInstances(node);
			foreach (Prototype inst in leavesConcrete)
				baseline += PrototypeGraphs.Size(inst);

			// Skeleton: unpacked shadow counted once
			int skeleton = PrototypeGraphs.Size(node.Unpacked);

			// Holes: sum size of hidden instances minus hidden root
			int holes = 0;
			List<Prototype> leavesHidden = HCPTreeUtil.GetLeavesAsHidden(node);
			foreach (Prototype hidden in leavesHidden)
				holes += (PrototypeGraphs.Size(hidden) - 1);

			return baseline - (skeleton + holes);
		}



		// Dump the entity sets, intersection, and union for the overlaps you logged.
		// Assumes:
		// - mapECHPs: Map<int, List<Prototype>> from ExtractEntityCentricHiddenPrototypesHierarchies
		// - setSlots: PrototypeSet containing the slot prototypes (Prototypes.GetPrototype(pair.Key))
		// - PrototypeGraphs.AreEquivalentCircular is available (use AreEqual if you prefer)

		static List<Prototype> GetSet(Map<int, List<Prototype>> mapECHPs, Prototype slot)
		{
			if (!mapECHPs.ContainsKey(slot.PrototypeID))
				return new List<Prototype>();

			// Deduplicate by structural equivalence (important if instances exist)
			List<Prototype> src = mapECHPs[slot.PrototypeID];
			List<Prototype> res = new List<Prototype>();
			foreach (Prototype p in src)
			{
				if (!res.Any(x => PrototypeGraphs.AreEquivalentCircular(x, p)))
					res.Add(p);
			}
			return res;
		}

		static List<Prototype> IntersectSets(List<Prototype> a, List<Prototype> b)
		{
			List<Prototype> inter = new List<Prototype>();
			foreach (Prototype x in a)
			{
				if (b.Any(y => PrototypeGraphs.AreEquivalentCircular(y, x)))
					inter.Add(x);
			}
			return inter;
		}

		static List<Prototype> UnionSets(List<Prototype> a, List<Prototype> b)
		{
			List<Prototype> uni = new List<Prototype>();
			foreach (Prototype x in a)
			{
				if (!uni.Any(y => PrototypeGraphs.AreEquivalentCircular(y, x)))
					uni.Add(x);
			}
			foreach (Prototype x in b)
			{
				if (!uni.Any(y => PrototypeGraphs.AreEquivalentCircular(y, x)))
					uni.Add(x);
			}
			return uni;
		}

		static void DumpOverlap(
			Map<int, List<Prototype>> mapECHPs,
			Prototype slotA,
			Prototype slotB)
		{
			List<Prototype> setA = GetSet(mapECHPs, slotA);
			List<Prototype> setB = GetSet(mapECHPs, slotB);

			List<Prototype> inter = IntersectSets(setA, setB);
			List<Prototype> uni = UnionSets(setA, setB);

			double j = uni.Count > 0 ? (double)inter.Count / (double)uni.Count : 0.0;

			Logs.DebugLog.WriteEvent("SetA", slotA.PrototypeName + " |A|=" + setA.Count);
			Logs.DebugLog.WriteEvent("SetA", PrototypeLogging.ToChildString(new Collection(setA)));

			Logs.DebugLog.WriteEvent("SetB", slotB.PrototypeName + " |B|=" + setB.Count);
			Logs.DebugLog.WriteEvent("SetB", PrototypeLogging.ToChildString(new Collection(setB)));

			Logs.DebugLog.WriteEvent("Intersect", "|∩|=" + inter.Count);
			Logs.DebugLog.WriteEvent("Intersect", PrototypeLogging.ToChildString(new Collection(inter)));

			Logs.DebugLog.WriteEvent("Union", "|∪|=" + uni.Count + " J=" + j.ToString("F3"));
			Logs.DebugLog.WriteEvent("Union", PrototypeLogging.ToChildString(new Collection(uni)));
		}

	


		static public Map<int, List<Prototype>> ExtractEntityCentricHiddenPrototypesHierarchies(List<Prototype> lstInstances)
		{
			//This version uses an AreEqual, not a AreShallowEqual
			Map<int, List<Prototype>> mapEHCPs = new Map<int, List<Prototype>>();

			foreach (Prototype protoInstance in lstInstances)
			{
				foreach (var pairProp in protoInstance.Properties)
				{
					if (null == pairProp.Value)
						continue;

					List<Prototype> protoECHPs = new List<Prototype>();
					if (mapEHCPs.ContainsKey(pairProp.Key))
						protoECHPs = mapEHCPs[pairProp.Key];

					if (!protoECHPs.Any(x => PrototypeGraphs.AreEqual(x, pairProp.Value)))
						protoECHPs.Add(pairProp.Value);

					mapEHCPs[pairProp.Key] = protoECHPs;
				}
			}


			return mapEHCPs;
		}


		public static void Revalue(Prototype prototype, Prototype protoNew)
		{
			Prototype protoParent = prototype.Parent;
			foreach (var pair in protoParent.Properties)
			{
				if (pair.Value == prototype)
					protoParent.Properties[pair.Key] = protoNew;
			}

			for (int i = 0; i < protoParent.Children.Count; i++)
			{
				if (protoParent.Children[i] == prototype)
					protoParent.Children[i] = protoNew;
			}
		}


	}

}
