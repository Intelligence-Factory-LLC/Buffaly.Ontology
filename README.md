# Buffaly.Ontology
*A production-ready, neurosymbolic ontology engine for healthcare & beyond*


> **Buffaly.Ontology** is the open-source core we use to power our ICD-10-CM,  
> SNOMED CT and language-aware medical knowledge graphs.  
> Written in **ProtoScript** (a full programming language, not just a data spec),  
> it lets you *load, query and extend* huge ontologies **in real time**—no SQL, no RDF hassle.

---

## ✨ Key capabilities

| Feature | Why it matters |
|---------|----------------|
| **ProtoScript language** | Define concepts *and* executable functions in one concise file. Hot-compile at runtime. |
| **Unified graph** | Seamlessly mix ICD-10-CM, SNOMED CT, WordNet, VerbNet, custom vocabularies. |
| **Lazy-loading + caching** | Load only the SNOMED concepts you touch—handle 300 k+ items on a laptop. |
| **Neurosymbolic hooks** | Call out to an LLM to create new prototypes automatically, then store them as first-class objects. |
| **Explainability baked-in** | Every mapping, rule and inference is traceable—crucial for regulated domains such as healthcare. |

---

## 🚀 Quick start

```bash
dotnet add package Buffaly.Ontology
```

```csharp
using Buffaly.Ontology;
using Buffaly.Ontology.ICD10CM;

	// 1. Boot the engine
	OntologyWorld world = OntologyWorld.CreateDefault();

	// 2. Ask for a code
	ICD10Code i517 = world.Lookup<ICD10Code>("I51.7");
	Console.WriteLine(i517.Description);   // → "Cardiomegaly"

	// 3. Dynamically extend with ProtoScript
	string proto = """
	partial prototype Myocardial_HypertrophySememe : Sememe
	{
	    Children = [HeartSememe, HypertrophySememe];
	}
	""";
	world.CompileProtoScript(proto);
```

```csharp
using ProtoScript.Interpretter;
using ProtoScript.Interpretter.Interpretting;
using ProtoScript.Parsers;
using System.Collections.Generic;

string script = """
namespace SimpsonsOntology
{
	prototype Person {
		String Name = "";
		Collection ParentOf = new Collection();
		function IsParent() : Boolean {
			return ParentOf.Count > 0;
		}
		function HasChild(Person child) : Boolean {
			return ParentOf.Contains(child);
		}
	}

	prototype Bart : Person {
		Name = "Bart Simpson";
	}

	prototype Homer : Person {
		Name = "Homer Simpson";
		ParentOf = [Bart];
	}
}
""";

ProtoScript.File file = Files.ParseFileContents(script);
Compiler compiler = new Compiler();
compiler.Initialize();
ProtoScript.Interpretter.Compiled.File compiled = compiler.Compile(file);
NativeInterpretter interpreter = new NativeInterpretter(compiler);
interpreter.Evaluate(compiled);

bool isParent = (bool)interpreter.RunMethodAsObject("Homer", "IsParent", new List<object>());
Console.WriteLine($"Homer is a parent? {isParent}");

Dictionary<string, object> namedArgs = new Dictionary<string, object>
{
	{ "child", interpreter.GetLocalPrototype("Bart") }
};
bool isParentOfBart = (bool)interpreter.RunMethodAsObject("Homer", "HasChild", namedArgs);
Console.WriteLine($"Homer is Bart's parent? {isParentOfBart}");
```
See the unit tests under **Ontology.Tests** for examples on how to load
the ontology, query concepts and register custom behaviours.

---

## 📦 Repository layout

```
├─ Buffaly.NLU/
├─ Buffaly.NLU.Tagger/
├─ Buffaly.Ontology.Portal/
├─ Ontology/                # Core engine
├─ Ontology.Parsers/
├─ Ontology.Simulation/
├─ Ontology.Tests/
├─ ProtoScript/
├─ ProtoScript.Extensions/
├─ ProtoScript.Interpretter/
├─ ProtoScript.Parsers/
├─ Scripts/
	├─ examples/
	│  ├─ HelloWorld/
	│  └─ Simpsons/
└─ lib/
```

## 📚 Samples
- [HelloWorld](examples/HelloWorld/README.md)
- [Simpsons](examples/Simpsons/Simpsons.md)
---

## 🛠 Building and testing

This repository is a standard .NET solution. To build everything:

```bash
dotnet build Ontology/Ontology8.sln
```

Run the unit tests:

```bash
dotnet test Ontology.Tests/Ontology.Tests.csproj
```

---

## 🤝 Contributing

We welcome pull requests, issues and discussion!
All contributors must sign the **Contributor Licence Agreement (CLA)**—see `CLA.md`.

---

## 🛡 Licence

Buffaly.Ontology is released under the **GNU General Public License v3.0**.

See [`LICENSE`](LICENSE) for the full text.

---

## 🏥 Need help with medical ontologies?

We’ve spent years deploying explainable, neurosymbolic AI in clinical settings
(ICD-10, SNOMED, CPT, LOINC, you name it).
If you’d like guidance or custom development, drop us a line at **[justin@intelligencefactory.ai](mailto:justin@intelligencefactory.ai)**.

---

*© 2025 Intelligence Factory, LLC* – *Safe, controlled and understandable AI for mission-critical domains*
