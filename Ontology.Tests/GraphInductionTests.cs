using BasicUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
			// This method is called before each test method.
			Initializer.Initialize();

			//	Initializer.SetupDatabaseDisconnectedMode();
		}

		[TestMethod]
		public void Test_GraphInductionEngine_1()
		{
			string strCode = @"c:\dev\FairPath\FairPath.Data\Addresses.cs";
			CSharp.File file = CSharp.Parsers.Files.Parse(strCode);
			Prototype protoFile = NativeValuePrototypes.ToPrototype(file);
		}

	}

}
