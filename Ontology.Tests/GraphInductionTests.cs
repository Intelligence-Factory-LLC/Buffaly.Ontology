using BasicUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ontology.GraphInduction;
using Ontology.GraphInduction.Utils;
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
  public  class AddressesRow 
    {
        public string? Name { get; set; }
}
"; 
//			string strCode = @" 
//  public partial class AddressesRow : RooTrax.Common.DB.BasicRow
//    {

//        public int AddressID { get; set; }

//        public string? Name { get; set; }

//        public string? Line1 { get; set; }

//        public string? Line2 { get; set; }

//        public string? City { get; set; }

//        public string? State { get; set; }

//        public string? Zip { get; set; }

//        public string? Country { get; set; }

//        public DateTime DateCreated { get; set; }

//        public DateTime LastUpdated { get; set; }

//}
//";

			//CSharp.File file = CSharp.Parsers.Files.Parse(strCode);
			//Prototype protoFile = NativeValuePrototypes.ToPrototype(file);

			//PrototypeLogging.Log(protoFile);


			//CSharp.ClassDefinition clsAddresses = file.Namespaces[0].Classes.First(x => x.ClassName.TypeName == "AddressesRow");
			CSharp.ClassDefinition clsAddresses = CSharp.Parsers.ClassDefinitions.Parse(strCode);

			Prototype protoAddresses = NativeValuePrototypes.ToPrototype(clsAddresses);
			CSharp.ClassDefinition clsAddresses2 = (CSharp.ClassDefinition) NativeValuePrototypes.FromPrototype(protoAddresses);

			PrototypeLogging.Log(protoAddresses);
			Logger.Log(protoAddresses);

			List<Prototype> lstLeafPaths = LeafBasedTransforms.GetEntityPathsByLeaf2(protoAddresses);
		}



	}

}
