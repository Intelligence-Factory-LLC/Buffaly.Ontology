using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoScript.Parsers;

namespace Ontology.Tests
{
	[TestClass]
	public class ReferenceStatementTests
	{
		[TestMethod]
		public void ParseReferenceWithImplicitName()
		{
			ProtoScript.ReferenceStatement statement = ReferenceStatements.Parse("reference CSharp.Extensions;;");
			Assert.AreEqual("CSharp.Extensions", statement.AssemblyName);
			Assert.AreEqual("CSharp.Extensions", statement.Reference);
		}
	}
}
