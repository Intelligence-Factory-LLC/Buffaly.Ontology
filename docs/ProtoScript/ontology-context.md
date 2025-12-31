# **ProtoScript Reference Manual**

## **Prototypes and ProtoScript in the Context of Ontologies**

ProtoScript operates over a typed, property-labeled Prototype graph—an executable ontology graph that prioritizes flexibility and developer intuition over rigid, formal schemas. The Buffaly system keeps all knowledge in this unified Prototype substrate, whether it originates from code, natural language, databases, or imported ontologies. ProtoScript is the declaration language for authoring and manipulating that graph, but Prototypes can also be created by importers and converters and may never exist as ProtoScript source text.

### **What is ProtoScript in the Context of Ontologies?**

ProtoScript is a graph-based ontology authoring language built around dynamic **Prototypes** rather than fixed classes. It combines the flexibility of prototype-based programming with the semantic clarity of typed property graphs, allowing entities to inherit from multiple parents, adapt at runtime, and generate new categorizations through instance comparisons. Instead of relying solely on formal logical axioms, ProtoScript emphasizes practical, auditable reasoning using **Least General Generalizations (LGG)**, subtyping operators, and transformation functions that can span multiple representations in the same graph.

For developers, ProtoScript feels like a programming language with the power of a graph database, offering a more intuitive alternative to traditional ontology systems like OWL or RDF Schema, which can be static, formal, and labor-intensive to modify.

### **How Prototypes Relate to Ontologies**

A traditional ontology defines **classes** (concepts), **properties** (attributes/relationships), and **axioms** (rules for reasoning). ProtoScript reframes this model in three key ways:

1. **Prototype-Based Instead of Class-Based**

   * Prototypes serve as both templates and instances, unlike OWL’s static classes.
   * They support dynamic, multiple inheritance, eliminating the need for deep, predefined hierarchies.
2. **Graph-Structured, Not Strictly Taxonomic**

   * Relationships are modeled as flexible graph edges, not limited to subclassing or formal property declarations.
   * Prototypes can include properties, functions, and rules, supporting diverse, heterogeneous structures natively.
3. **Reasoning Through Structural Generalization**

   * ProtoScript uses **LGG** to create ad-hoc generalizations (called *shadows*) from instance comparisons, enabling categorization based on structural similarity.
   * These shadows can be stored as *subtypes*, providing lightweight reasoning without complex deductive rules.

## Why ProtoScript is not OWL or RDF (and why we don’t treat them as interchangeable)

This project intentionally does **not** target OWL/RDF as its core representation. That’s not a claim that OWL/RDF are “bad.” It’s a claim that they optimize for a different problem.

ProtoScript is best understood as an **executable, typed, property-labeled graph runtime** with a compact declaration language (ProtoScript) and first-class support for dynamic learning and transformation.

OWL/RDF are best understood as **standardized semantic interchange formalisms** (and, for OWL, a family of description logics) designed for publishing, linking, and reasoning under a particular formal semantics.

Those are fundamentally different design centers.

### 1) Different representation goals

OWL/RDF goal:
- A shared, web-oriented representation where identifiers are global (IRIs), semantics are standardized, and many tools can consume the same artifact.

ProtoScript/Buffaly goal:
- A high-performance local representation where **everything is a graph object** you can:
  - Create,
  - Mutate,
  - Type,
  - Compare,
  - Generalize (LGG/shadows),
  - Subtype dynamically,
  - Transform across domains,
  - Execute computed relationships over,
  - And persist efficiently.

ProtoScript is trying to be the “native representation” of a living system, not a publishing format.

### 2) Different computational semantics (open-world logic vs executable graph rules)

OWL (in its mainstream profiles) is built around description logic semantics:
- Typically **open world** (absence of a fact is not evidence of negation),
- Typically **monotonic** (adding facts doesn’t invalidate previous entailments),
- Often relies on specialized reasoning procedures.

ProtoScript is an executable graph environment:
- It can encode closed-world behavior, procedural constraints, and operational semantics directly as functions/transforms.
- It can model computed relationships as graph traversals or evaluators.
- It is designed for **incremental updates and runtime evolution** of the graph.

This is a core reason ProtoScript is not “just another ontology syntax.”
It’s closer to “a programmable knowledge substrate.”

### 3) “But RDF can represent any graph, including ASTs”

Yes: you *can* encode an AST in RDF triples.

The real question is: what do you get *operationally*?

ProtoScript’s advantage is not “RDF is incapable of graphs.”
ProtoScript’s advantage is:
- The representation is **native and ergonomic** for building and operating a unified graph that includes ASTs, semantics, database records, and learned abstractions,
- The system has first-class operations that treat these objects as executable structures:
  - generalization (shadows),
  - parameterization (paths/entities),
  - dynamic subtyping (categorization),
  - transformation functions (graph-to-graph compilation),
  - and cross-domain reasoning.

In RDF/OWL ecosystems, those operations typically live outside the representation (in external code, rules engines, pipelines, or ad hoc conventions). In ProtoScript, they are the point.

### 4) The “integers vs strings” claim (what’s true, what isn’t)

It’s directionally true that ProtoScript systems commonly operate on **compact internal identifiers** for speed (e.g., prototypes and edges represented by integer IDs), while RDF/OWL’s *conceptual* model is expressed in IRIs and literals.

But it’s not strictly true that RDF stores “must be strings.”
Many triple stores and RDF databases also dictionary-encode IRIs/literals into integer IDs internally.

So the real differentiator is not “strings vs ints” as a universal truth.
The differentiator is that ProtoScript is designed end-to-end as:
- A compact runtime object model,
- A mutation-friendly graph,
- With executable semantics and learning operations,
- Without requiring global web identifiers or DL entailment as the core engine.

### 5) What ProtoScript can do that OWL/RDF systems typically don’t target

ProtoScript is designed to treat a single graph as a unified substrate for:
- Ontologies (SNOMED/WordNet/VerbNet/etc.),
- Program structures (AST-like code graphs),
- Natural language semantics,
- Data records (rows/events),
- Learned types and relationships from unstructured data,
- Transformation pipelines that map between these representations.

That “single substrate across representations” is not what OWL/RDF were built to optimize.

### 6) When you would choose ProtoScript vs OWL/RDF

Choose ProtoScript when you need:
- A living, mutable knowledge system (not just a published ontology artifact),
- Cross-domain graph representation (code + language + data + ontologies in one substrate),
- Computed relationships and transformations as first-class citizens,
- Structural learning and generalization without gradient descent,
- Operational control over how knowledge is added, verified, and revised.

Choose OWL/RDF when you need:
- Interoperability with Semantic Web tools and standards,
- A shared publishing format for exchanging ontologies across organizations,
- DL-centric entailment semantics as the central feature.

This project prioritizes the first set of needs, which is why OWL/RDF are not used as the primary representation.

## Are ProtoScript graphs “ontologies”?

It is accurate to call many ProtoScript graphs “ontologies” *if* we state what we mean by the word.

In Buffaly, an ontology is:
- A **typed, property-labeled graph** of concepts and instances,
- With explicit type structure and relationship structure,
- That supports:
  - querying,
  - constraint-like checks,
  - generalization,
  - categorization (dynamic subtyping),
  - and transformation.

This is compatible with the practical meaning of “ontology” in many engineering systems: a structured conceptual model that enables precise retrieval and reasoning.

However, ProtoScript ontologies are not OWL ontologies, because:
- ProtoScript is not defined by description logic entailment semantics,
- ProtoScript allows executable semantics (computed relationships and transformations) as first-class features,
- ProtoScript is designed for incremental growth and operational control rather than web publication.

If you want a phrase that avoids semantic-web baggage while staying precise:
**“executable ontology graph”** or **“typed property-graph ontology”** describes what ProtoScript is doing.

### Why the “single graph across representations” matters

A recurring theme in ProtoScript is that we do not maintain separate representations for:
- code graphs,
- semantic graphs,
- database graphs,
- and ontology graphs.

They are all represented as Prototypes in one runtime substrate.

That is what makes cross-domain reasoning and transformation possible:
- A natural language statement can map to a semantic form,
- that semantic form can map to a code prototype,
- that code prototype can map to a database query prototype,
- and all of it remains explainable because every step is explicit graph structure.

ProtoScript is optimized for that unification.

### **Use Cases Where ProtoScript Excels**

* **Semantic Modeling with Changing Requirements**: Adapt knowledge structures as new concepts emerge, without refactoring fixed hierarchies or running consistency checks.
* **Explainable AI and Reasoning**: Trace *why* an instance matches a category via explicit graph paths, unlike opaque AI models.
* **Auditable Systems**: Track prototype matches, function calls, and generalizations, ideal for domains like healthcare or finance.
* **Integrating External Ontologies**: Import OWL or RDF schemas and evolve them with richer, dynamic behavior while preserving semantic integrity.
* **Cross-domain transformations**: Treat code artifacts, NL semantics, and data records as co-equal Prototypes in one graph so transformations can hop between them without impedance mismatches.

### **Example: Ontology-Like Modeling**

Consider modeling a geographic ontology:

```protoscript
prototype Location {
    System.String Name = "";
}
prototype City : Location {
    State State = new State();
}
prototype State : Location {
    Collection Cities = new Collection();
}
prototype NewYork_City : City {
    Name = "New York City";
}
prototype NewYork_State : State {
    Name = "New York";
}
NewYork_City.State = NewYork_State;
NewYork_State.Cities = [NewYork_City];
```

This creates a graph where `NewYork_City` and `NewYork_State` form a bidirectional relationship, akin to an ontology’s object properties, but with the flexibility to add new properties or subtypes at runtime. The same substrate can also hold AST-like Prototypes for code that manipulates these entities or semantic parses of natural language queries about them, enabling transformations and checks across representations without leaving the Prototype graph.

### **Summary**

ProtoScript redefines ontology building with a developer-friendly, graph-based paradigm. Prototypes enable evolving knowledge structures, while built-in reasoning tools like LGG and subtyping provide transparency without heavyweight inference systems. Because code graphs, semantic parses, database records, and imported ontologies all live in the same executable ontology graph, ProtoScript keeps cross-domain reasoning explicit and auditable for adaptive systems.

---

**Previous:** [**ProtoScript Reference Manual - Introduction**](introduction.md) | **Next:** [**What Are Prototypes?**](what-are-prototypes.md)
