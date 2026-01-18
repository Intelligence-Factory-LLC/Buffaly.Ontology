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
			List<Prototype> lstHiddens = new List<Prototype>();
			foreach (HCPTree.Node node in nodes1)
			{
				List<Prototype> lstLeaves = HCPTreeUtil.GetLeavesAsHidden(node);
				lstHiddens.AddRange(lstLeaves);
			}

			List<HCPTree.Node> nodes2 = HCPTrees.GetNonLeaves(root2);
			foreach (HCPTree.Node node in nodes2)
			{
				List<Prototype> lstLeaves = HCPTreeUtil.GetLeavesAsHidden(node);
				lstHiddens.AddRange(lstLeaves);
			}

			Map<int, List<Prototype>> mapECHPs = ExtractEntityCentricHiddenPrototypesHierarchies(lstHiddens);

			foreach (var pair in mapECHPs)
			{
				Logs.DebugLog.WriteEvent(Prototypes.GetPrototypeName(pair.Key), PrototypeLogging.ToChildString(new Collection(pair.Value)));
			}




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
