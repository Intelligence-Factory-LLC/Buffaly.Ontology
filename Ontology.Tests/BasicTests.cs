namespace Ontology.Tests
{
	[TestClass]
	public sealed class BasicTests
	{
		[TestInitialize]
		public void TestInit()
		{
			// This method is called before each test method.
			Initializer.Initialize();

		//	Initializer.SetupDatabaseDisconnectedMode();
		}

		[TestMethod]
		public void Test_Instances()
		{
			Prototype prototype = TemporaryPrototypes.GetOrCreateTemporaryPrototype("TestPrototype");
			Prototype prototype1 = TemporaryPrototypes.GetOrCreateTemporaryPrototype("TestPrototype1", prototype);

			Assert.IsTrue(prototype1.TypeOf(prototype));
			Assert.IsTrue(prototype.GetAllDescendants().Contains(prototype1), "TestPrototype1 should be a descendant of TestPrototype");

			Prototype protoInstance = prototype1.CreateInstance();

			Assert.IsTrue(protoInstance.TypeOf(prototype1), "Instance should be of type TestPrototype1");
			Assert.IsTrue(protoInstance.TypeOf(prototype), "Instance should be of type TestPrototype");

			Assert.IsTrue(prototype1.GetDescendants().Contains(protoInstance), "Instance should be a descendant of TestPrototype1");

			Prototype prototype2 = TemporaryPrototypes.GetOrCreateTemporaryPrototype("TestPrototype2");
			prototype.InsertTypeOf(prototype2);

			Assert.IsTrue(prototype.TypeOf(prototype2), "TestPrototype should be of type TestPrototype2");
			Assert.IsTrue(prototype1.TypeOf(prototype2), "TestPrototype1 should be of type TestPrototype2");
			Assert.IsTrue(protoInstance.TypeOf(prototype2), "Instance should be of type TestPrototype1");


			//Test copies
			Prototype protoCopy = protoInstance.ShallowClone();
			Assert.IsTrue(protoCopy.TypeOf(prototype1), "Shallow clone should be of type TestPrototype1");
			Assert.IsTrue(protoCopy.TypeOf(prototype), "Shallow clone should be of type TestPrototype");

			try
			{
				protoCopy.InsertTypeOf(prototype2);
				Assert.Fail("Should not be able to insert type of TestPrototype2 into a shallow clone of TestPrototype1");
			}
			catch (Exception ex)
			{
			
			}

			Prototype protoChild = TemporaryPrototypes.GetOrCreateTemporaryPrototype("TestPrototypeChild");
			protoCopy.Children.Add(protoChild);

			Assert.IsTrue(protoCopy.Children.Contains(protoChild), "Shallow clone should contain TestPrototypeChild as a child");
			Assert.IsFalse(protoInstance.Children.Contains(protoChild), "Instance should not contain TestPrototypeChild as a child");

			Prototype prototype3 = TemporaryPrototypes.GetOrCreateTemporaryPrototype("TestPrototype3");
			prototype1.InsertTypeOf(prototype3);

			Assert.IsTrue(prototype1.TypeOf(prototype3), "TestPrototype1 should be of type TestPrototype3");
			Assert.IsTrue(protoInstance.TypeOf(prototype3), "Instance should be of type TestPrototype3");
			Assert.IsTrue(protoCopy.TypeOf(prototype3), "Shallow clone should be of type TestPrototype3");

		}

		[TestMethod]
		public void Test_TemporaryLexemes()
		{
			Prototype lexeme = TemporaryLexemes.GetOrInsertLexeme("TestLexeme");
			Assert.IsNotNull(lexeme, "TemporaryLexeme should not be null");
			Assert.IsTrue(lexeme is TemporaryLexeme);
		
			Prototype protoRelated = TemporaryPrototypes.GetOrCreateTemporaryPrototype("RelatedPrototype");
			TemporaryLexeme lexeme2 = (TemporaryLexeme) TemporaryLexemes.GetOrInsertLexeme("TestLexeme2", protoRelated);

			Assert.IsNotNull(lexeme2, "TemporaryLexeme2 should not be null");
			Assert.IsTrue(lexeme2.LexemePrototypes.ContainsKey(protoRelated), "TemporaryLexeme2 should contain RelatedPrototype as a related prototype");

			TemporaryLexeme lexemeClone = lexeme2.Clone() as TemporaryLexeme;
			Assert.IsNotNull(lexemeClone, "Cloned TemporaryLexeme should not be null");

		}
	}
}
