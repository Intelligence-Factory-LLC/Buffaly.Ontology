````markdown
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
````

```csharp
using Buffaly.Ontology;
using Buffaly.Ontology.ICD10CM;

// 1. Boot the engine
var world = OntologyWorld.CreateDefault();

// 2. Ask for a code
var i517 = world.Lookup<ICD10Code>("I51.7");
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

See **docs/GettingStarted.md** for a 10-minute tutorial covering:

* Loading the full ICD-10 hierarchy
* Lazy-loading SNOMED CT concepts on demand
* Registering a C# transfer function between lexical and clinical layers

---

## 📦 Repository layout

```
├─ src/
│   ├─ Buffaly.Ontology.Core/     // ProtoScript compiler & runtime
│   ├─ Buffaly.Ontology.ICD10CM/  // 15k ICD-10-CM codes as ProtoScript
│   └─ Buffaly.Ontology.Lexical/  // WordNet + VerbNet seeds
├─ examples/
│   ├─ 01-query-icd10/            // LINQ-style querying demo
│   └─ 02-hot-compile/            // add prototypes at runtime
└─ docs/
    ├─ Architecture.md
    └─ GettingStarted.md
```

---

## 🤝 Contributing

We welcome pull requests, issues and discussion!
All contributors must sign the **Contributor Licence Agreement (CLA)**—see `CLA.md`.

---

## 🛡 Licence (dual)

* **Community Edition** – released under the **Apache License 2.0**.
  You may use it freely in research, startups and open applications.

* **Commercial Edition** – if you need to embed Buffaly.Ontology in a closed-source
  or SaaS product, or require indemnity & SLA, we offer a commercial licence.
  Email **[justin@intelligencefactory.ai](mailto:justin@intelligencefactory.ai)** for pricing and terms.

The full text of the Apache 2.0 licence is in [`LICENSE`](LICENSE).

---

## 🏥 Need help with medical ontologies?

We’ve spent years deploying explainable, neurosymbolic AI in clinical settings
(ICD-10, SNOMED, CPT, LOINC, you name it).
If you’d like guidance or custom development, drop us a line at **[justin@intelligencefactory.ai](mailto:justin@intelligencefactory.ai)**.

---

*© 2025 Intelligence Factory, LLC* – *Safe, controlled and understandable AI for mission-critical domains*

```
```
