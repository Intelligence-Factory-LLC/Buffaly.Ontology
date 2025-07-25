using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoScript.Interpretter;
using ProtoScript.Interpretter.Symbols;

namespace Ontology.Tests
{
[TestClass]
public sealed class NullConditionalOperator_Tests
{
[TestInitialize]
public void Init()
{
Initializer.Initialize();
}

[TestMethod]
public void NullConditionalProperty_ReturnsNull()
{
string code = @"
prototype Person
{
String Name = \"Homer\";
}

function main() : Prototype
{
Person p = null;
return p?.Name;
}
";
ProtoScript.File file = ProtoScript.Parsers.Files.ParseFileContents(code);
Compiler compiler = new Compiler();
compiler.Initialize();
ProtoScript.Interpretter.Compiled.File compiled = compiler.Compile(file);
NativeInterpretter interp = new NativeInterpretter(compiler);
interp.Evaluate(compiled);
object? res = interp.RunMethodAsObject(null, "main", new List<object>());
Assert.IsNull(res);
}

[TestMethod]
public void NullConditionalMethod_ReturnsNull()
{
string code = @"
function main() : string
{
String s = null;
return s?.GetStringValue();
}
";
ProtoScript.File file = ProtoScript.Parsers.Files.ParseFileContents(code);
Compiler compiler = new Compiler();
compiler.Initialize();
ProtoScript.Interpretter.Compiled.File compiled = compiler.Compile(file);
NativeInterpretter interp = new NativeInterpretter(compiler);
interp.Evaluate(compiled);
object? res = interp.RunMethodAsObject(null, "main", new List<object>());
Assert.IsNull(res);
}
}
}
