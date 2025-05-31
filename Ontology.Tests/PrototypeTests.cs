using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoScript.Interpretter;
using ProtoScript.Interpretter.Interpretting;
using ProtoScript.Interpretter.Symbols;

namespace Ontology.Tests
{
	[TestClass]
	public sealed class PrototypeTests
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
			TemporaryLexeme lexeme2 = (TemporaryLexeme)TemporaryLexemes.GetOrInsertLexeme("TestLexeme2", protoRelated);

			Assert.IsNotNull(lexeme2, "TemporaryLexeme2 should not be null");
			Assert.IsTrue(lexeme2.LexemePrototypes.ContainsKey(protoRelated), "TemporaryLexeme2 should contain RelatedPrototype as a related prototype");

			TemporaryLexeme lexemeClone = lexeme2.Clone() as TemporaryLexeme;
			Assert.IsNotNull(lexemeClone, "Cloned TemporaryLexeme should not be null");

		}

		[TestMethod]
		public void Test_Collections()
		{
			string strCode = @"
prototype Object;
prototype Kangaroo;


function TestFunction_1() : Collection
{
	Collection collection = new Collection();
	collection.Add(new Object());
	return collection;
}

function TestFunction_2() : Integer
{
	Collection collection = new Collection();
	collection.Add(new Object());
	return collection.Count;
}

function TestFunction_3() : Boolean
{
	Collection collection = new Collection();
	collection.Add(new Object());
	return collection.Count > 0;
}


prototype CollectionContainer
{
	Collection collection = new Collection();

	function TestFunction_4() : Collection
	{
		return collection;
	}
}

";

			ProtoScript.File file = ProtoScript.Parsers.Files.ParseFileContents(strCode);

			Compiler compiler = new Compiler();
			compiler.Initialize();
			ProtoScript.Interpretter.Compiled.File fileCompiled = compiler.Compile(file);
			SymbolTable symbols = compiler.Symbols;

			NativeInterpretter interpretter = new NativeInterpretter(compiler);
			interpretter.Evaluate(fileCompiled);

			object? oRes1 = interpretter.RunMethodAsObject(null, "TestFunction_1", new List<object>());
			Assert.IsTrue(oRes1 is Collection, "TestFunction_1 should return a Collection object");
			Collection collection1 = (Collection)oRes1;
			Assert.IsTrue(collection1.Count == 1, "Collection from TestFunction_1 should contain 1 Object");
			Assert.IsTrue(collection1.Children[0].TypeOf("Object"), "Collection from TestFunction_1 should contain an Object instance");


			object? oRes2 = interpretter.RunMethodAsObject(null, "TestFunction_2", new List<object>());
			Assert.IsTrue(oRes2 is int, "TestFunction_2 should return an Integer");
			int count = (int)oRes2;
			Assert.IsTrue(count == 1, "TestFunction_2 should return a count of 1 for the Collection created in TestFunction_1");

			object? oRes3 = interpretter.RunMethodAsObject(null, "TestFunction_3", new List<object>());
			Assert.IsTrue(oRes3 is bool, "TestFunction_3 should return a Boolean");
			bool isNotEmpty = (bool)oRes3;
			Assert.IsTrue(isNotEmpty, "TestFunction_3 should return true since the Collection created in TestFunction_1 is not empty");


			//Test CollectionContainer
			object? oRes4 = interpretter.RunMethodAsObject("CollectionContainer", "TestFunction_4", new List<object>());
			Assert.IsTrue(oRes4 is Collection, "TestFunction_4 should return a Collection object from the CollectionContainer prototype");

		}

		[TestMethod]
		public void Test_ReturnReference()
		{
			//This test checks if a function can return a reference to an object in the prototype scope. Instead
			//of returning the property itself

			string strCode = @"
prototype Object;
prototype Kangaroo;

prototype CollectionContainer
{
	Object obj = new Object();

	function TestFunction_1() : Prototype
	{
		return obj;
	}

	function TestFunction_2() : Prototype
	{
		return this.obj;
	}

	function TestFunction_3() : Prototype
	{
		Prototype obj2 = obj;
		return obj2;
	}
}

";

			ProtoScript.File file = ProtoScript.Parsers.Files.ParseFileContents(strCode);

			Compiler compiler = new Compiler();
			compiler.Initialize();
			ProtoScript.Interpretter.Compiled.File fileCompiled = compiler.Compile(file);
			SymbolTable symbols = compiler.Symbols;

			NativeInterpretter interpretter = new NativeInterpretter(compiler);
			interpretter.Evaluate(fileCompiled);

			Prototype ? oRes1 = interpretter.RunMethodAsObject("CollectionContainer", "TestFunction_1", new List<object>()) as Prototype;
			Assert.IsNotNull(oRes1, "TestFunction_1 should return a Prototype object");
			Assert.IsTrue(oRes1.TypeOf("Object"), "TestFunction_1 should return a Object");

			Prototype ? oRes2 = interpretter.RunMethodAsObject("CollectionContainer", "TestFunction_2", new List<object>()) as Prototype;
			Assert.IsNotNull(oRes2, "TestFunction_2 should return a Prototype object");
			Assert.IsTrue(oRes2.TypeOf("Object"), "TestFunction_2 should return a Object");

			Prototype ? oRes3 = interpretter.RunMethodAsObject("CollectionContainer", "TestFunction_3", new List<object>()) as Prototype;
			Assert.IsNotNull(oRes3, "TestFunction_3 should return a Prototype object");
			Assert.IsTrue(oRes3.TypeOf("Object"), "TestFunction_3 should return a Object");




		}

	}

}
