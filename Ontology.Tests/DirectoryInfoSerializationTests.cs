using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ontology.Tests
{
	[TestClass]
	public class DirectoryInfoSerializationTests
	{
		[TestInitialize]
		public void TestInitialize()
		{
			Initializer.Initialize();
		}

	[TestMethod]
	public void Serialize_DirectoryInfo_DoesNotRecurse()
	{
		System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(".");
		NativeValuePrototype ? prototype = NativeValuePrototypes.ToPrototype(di);

		Assert.IsNull(prototype);
	}
}
}
