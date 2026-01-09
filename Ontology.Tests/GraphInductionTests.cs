using BasicUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ontology.GraphInduction;
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

}
";


			CSharp.File file = CSharp.Parsers.Files.Parse(strFile);
			Prototype ? protoFile = NativeValuePrototypes.ToPrototype(file);


			//CSharp.ClassDefinition clsAddresses = file.Namespaces[0].Classes.First(x => x.ClassName.TypeName == "AddressesRow");
			CSharp.ClassDefinition clsAddresses = CSharp.Parsers.ClassDefinitions.Parse(strCode);

			Prototype ? protoAddresses = NativeValuePrototypes.ToPrototype(clsAddresses);
			Logger.Log(protoAddresses);



			Prototype protoRoot = protoAddresses;


			List<Prototype> lstLeafPaths = LeafBasedTransforms.GetEntityPathsByLeaf2(protoAddresses);

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


			//Generalize the graph, longest paths first, to remove any shorter common paths 
			foreach (Prototype protoPath in lstLeafPaths)
			{
				Collection lstInstances = PrototypeGraphs.Find(protoRoot, x => PrototypeGraphs.AreEqual(x, protoPath));

				foreach (Prototype protoInstance in lstInstances.Children)
				{
					//protoInstance.InsertTypeOf(Compare.Entity.Prototype, true);
					Revalue(protoInstance, Compare.Entity.Prototype);
				}
			}

			PrototypeLogging.Log(protoRoot);

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
