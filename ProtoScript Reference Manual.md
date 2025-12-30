# **ProtoScript Reference Manual \- Introduction**

ProtoScript is a graph-based programming language developed by Matt Furnari for the Buffaly system. It is inspired by prototype-based programming, knowledge graphs, and logic programming. Welcome to **ProtoScript**, a powerful way to model and manipulate complex relationships in a flexible, dynamic way. If you’re a developer familiar with languages like C\# or JavaScript, ProtoScript offers a fresh yet approachable paradigm that blends the structure of object-oriented programming with the fluidity of graph-based knowledge representation. This reference manual is your guide to mastering ProtoScript, providing clear explanations, incremental learning, and abundant examples to help you build sophisticated applications, from natural language processing to semantic transformations.

## **What is ProtoScript?**

ProtoScript is a declarative, prototype-oriented language designed to work within the **Buffaly system**, a framework for representing knowledge as graphs. Unlike traditional programming languages that rely on rigid class hierarchies or procedural logic, ProtoScript uses **Prototypes**—versatile entities that act as both templates and instances—to model data and relationships as nodes and edges in a directed graph. This graph-based approach enables ProtoScript to handle diverse domains, such as:

* **Code Structures**: Representing C\# variable declarations (e.g., `int i = 0`) or SQL queries.  
* **Natural Language**: Parsing sentences like "I need to buy some covid-19 test kits" into semantic graphs.  
* **Abstract Concepts**: Modeling causal or conditional relationships, like "New York City is in New York State."

Think of ProtoScript as a blend of C\#’s structured syntax and a database’s relational power, but with the flexibility of a graph database like Neo4j. It’s designed to simplify the creation, manipulation, and serialization of complex graph structures, making it ideal for tasks requiring dynamic categorization, transformation, and reasoning.

### ProtoScript has three layers

ProtoScript is organized into three complementary layers that clarify how programs are represented, executed, and adapted over time. The **Representation layer** treats programs and data as typed, property-labeled graphs built from Prototypes, with primitives stored as nodes or values within that structure. The **Reasoning layer** relies on deterministic operators—such as `typeof`, the categorization arrow (`->`), and graph traversal functions—to provide auditable execution over those graphs. The **Learning layer** connects comparison and Least General Generalization to the creation of Shadows, parameterized Paths, and Hidden Context Prototypes (HCPs). It uses clustering feedback to refine these artifacts so the system can predict or complete structures based on prior observations. HCPs act as deltas from Shadows, enabling categorization, transformation, and reasoning updates without mutating the original graph directly. Together, these layers let ProtoScript represent knowledge, reason over it, and iteratively improve its models.

### **Analogy to Familiar Concepts**

If you know C\#, imagine ProtoScript as a language where:

* **Classes** are replaced by Prototypes, which can inherit from multiple parents (like interfaces but more flexible) and change at runtime.  
* **Objects** are graph nodes that can represent anything—a variable, a query, or a concept—linked by edges that define relationships.  
* **Methods** are functions that traverse or modify the graph, computing results based on node connections.

For JavaScript developers, ProtoScript’s prototype-based nature might feel like JavaScript’s prototypal inheritance, but with a stronger focus on graph operations and symbolic computation rather than object cloning.

## **The Prototype System**

At the heart of ProtoScript lies the **Prototype system**, where every entity is a Prototype—a node in a graph that encapsulates properties, behaviors, and relationships. Prototypes are more than just data structures; they’re dynamic entities that can:

* **Inherit from Multiple Parents**: A Prototype like `Buffalo_City` can inherit from both `City` and `Location`, combining their properties without the limitations of single inheritance in C\#.  
* **Store Data**: Properties like `City.State = NewYork_State` represent stored (extensional) facts, similar to fields in a class.  
* **Compute Relationships**: Functions define computed (intensional) relationships, like determining if a city is in a specific state, akin to methods but operating on graph traversals.

Prototypes form a **directed graph** (often a directed acyclic graph, or DAG, for inheritance, with cycles allowed in property relationships), where edges represent inheritance, properties, or computed links. This structure allows ProtoScript to model complex, real-world relationships—like bidirectional links between a state and its cities—more naturally than traditional class-based systems.

### **Example: A Simple Prototype**

Here’s a glimpse of ProtoScript modeling a city, relatable to developers familiar with object-oriented programming:

```javascript
prototype City {
    System.String Name = "";
    State State = new State();
}
prototype NewYork_City : City {
    Name = "New York City";
}
prototype State {
    Collection Cities = new Collection();
}
prototype NewYork_State : State;

// Link them
NewYork_City.State = NewYork_State;
NewYork_State.Cities = [NewYork_City];
```

**What’s Happening?**

* `City` and `State` are Prototypes, like classes but more flexible.  
* `NewYork_City` inherits from `City`, setting its `Name` property.  
* The graph links `NewYork_City` to `NewYork_State` and vice versa, forming a cycle (a bidirectional relationship).  
* This resembles a C\# class with fields but allows runtime modifications and multiple inheritance.

## **Design Goals of ProtoScript**

ProtoScript is built with several key objectives, making it a unique tool for developers:

1. **Simplified Graph Creation**: Streamline the process of building complex graph structures, reducing boilerplate compared to C\#’s verbose class definitions.  
2. **Multiple Inheritance**: Allow Prototypes to inherit from multiple parents, reflecting real-world complexity where entities belong to multiple categories (e.g., a `Buffalo` as both `City` and `Location`).  
3. **Support for Stored and Computed Relationships**: Enable both extensional facts (e.g., `City.State`) and intensional rules (e.g., a function determining valid states), akin to combining database tables with logic.  
4. **Native Graph Operations**: Provide built-in tools for traversing and manipulating graphs, similar to querying a database but integrated into the language.  
5. **Serialization-First Approach**: Ensure Prototypes can be easily stored or shared across systems, like JSON serialization in modern APIs.  
6. **Dynamic Runtime Modifications**: Allow Prototypes to adapt at runtime, unlike C\#’s static type system, enabling flexible categorization and transformation.

## **Why Use ProtoScript?**

ProtoScript shines in scenarios requiring dynamic, interconnected data modeling, such as:

* **Natural Language Processing**: Parsing sentences into semantic graphs for AI applications.  
* **Code Analysis and Transformation**: Refactoring code (e.g., `string s1 = ""` to `string s1 = string.Empty`) or generating code from NL descriptions.  
* **Knowledge Representation**: Building ontologies for domains like geography or fiction (e.g., modeling Simpsons characters).

For developers, ProtoScript offers:

* **Familiarity**: C\#-like syntax lowers the learning curve.  
* **Power**: Graph-based flexibility surpasses traditional object-oriented limitations.  
* **Expressiveness**: Dynamic features like subtyping and transformation functions enable sophisticated reasoning.

### **Example: Modeling a Real-World Scenario**

To illustrate, consider modeling characters from *The Simpsons*:

```javascript
prototype Person {
    System.String Gender = "";
    Location Location = new Location();
    Collection ParentOf = new Collection();
}
prototype Homer : Person {
    Gender = "Male";
    Location = SimpsonsHouse;
    ParentOf = [Bart, Lisa, Maggie];
}
prototype SimpsonsHouse : Location {
    System.String Address = "742 Evergreen Terrace";
}
```

**What’s Happening?**

* `Person` defines a Prototype with properties, like a C\# class.  
* `Homer` inherits from `Person`, setting specific values.  
* `SimpsonsHouse` is a `Location` node, linked to `Homer` via the `Location` property.  
* This creates a graph where `Homer` connects to `SimpsonsHouse` and his children, mirroring a relational database but with dynamic, graph-based querying.

## **How This Manual is Organized**

This manual is structured around the three layers introduced above, so you always know whether a concept is about representation, reasoning, or learning:

* **Representation layer**: Learn ProtoScript’s syntax, Prototypes, NativeValuePrototypes, and core constructs, with analogies to C\# and JavaScript.
* **Reasoning layer**: Explore `typeof`, the categorization arrow (`->`), graph traversal, and the taxonomy of relationships that drive deterministic execution.
* **Learning layer**: Dive into shadows, paths, Hidden Context Prototypes, clustering feedback, and transformation functions for dynamic categorization and mapping.
* **Examples and integration**: Apply ProtoScript to practical scenarios and understand its seamless integration with C\# across all three layers.

Each section is packed with examples, drawing on familiar programming concepts to make ProtoScript accessible. Whether you’re building AI-driven applications or exploring symbolic computation, this manual equips you to harness ProtoScript’s full potential.

## **Getting Started**

To begin, you’ll need a basic understanding of:

* **Object-Oriented Programming**: Familiarity with classes, objects, and inheritance (e.g., in C\# or Java).  
* **Graph Concepts**: A high-level grasp of nodes and edges, as in graph databases or data structures.  
* **C\# Syntax**: ProtoScript’s syntax is C\#-inspired, so knowledge of C\# will accelerate your learning.

No prior experience with graph-based languages is required—ProtoScript’s intuitive design and this manual’s examples will guide you step-by-step. Ready to explore the graph-based world of ProtoScript? Let’s dive into the core concepts in the next section\!

# **ProtoScript Reference Manual**

## **Prototypes and ProtoScript in the Context of Ontologies**

ProtoScript offers a dynamic, graph-based approach to building ontologies—or ontology-like systems—that prioritizes flexibility and developer intuition over the rigid, formal structures of traditional ontology frameworks. Unlike systems like OWL or RDF, which rely on static class definitions and complex logical axioms, ProtoScript uses **Prototypes** to represent concepts, enabling both structure and semantics to evolve at runtime. This section introduces ProtoScript’s unique position in knowledge representation, highlighting its advantages for developers building adaptive, explainable systems.

### **What is ProtoScript in the Context of Ontologies?**

ProtoScript is a graph-based ontology representation framework built around dynamic **Prototypes** rather than fixed classes. It combines the flexibility of prototype-based programming with the semantic clarity of ontologies, allowing entities to inherit from multiple parents, adapt at runtime, and generate new categorizations through instance comparisons. Instead of relying solely on formal logical axioms, ProtoScript emphasizes practical, lightweight reasoning using **Least General Generalizations (LGG)** and subtyping operators. This makes it easier to adapt, scale, and maintain complex knowledge bases, especially in domains with evolving or uncertain conceptualizations.

For developers, ProtoScript feels like a programming language with the power of a graph database, offering a more intuitive alternative to traditional ontology systems like OWL or RDF Schema, which can be static, formal, and labor-intensive to modify.

### **How Prototypes Relate to Ontologies**

A traditional ontology defines **classes** (concepts), **properties** (attributes/relationships), and **axioms** (rules for reasoning). In systems like OWL, these are static, and reasoning often requires external inference engines. ProtoScript reimagines this model in three key ways:

1. **Prototype-Based Instead of Class-Based**

   * Prototypes serve as both templates and instances, unlike OWL’s static classes.  
   * They support dynamic, multiple inheritance, eliminating the need for deep, predefined hierarchies.  
2. **Graph-Structured, Not Strictly Taxonomic**

   * Relationships are modeled as flexible graph edges, not limited to subclassing or formal property declarations.  
   * Prototypes can include properties, functions, and rules, supporting diverse, heterogeneous structures natively.  
3. **Reasoning Through Structural Generalization**

   * ProtoScript uses **LGG** to create ad-hoc generalizations (called *shadows*) from instance comparisons, enabling categorization based on structural similarity.  
   * These shadows can be stored as *subtypes*, providing lightweight reasoning without complex deductive rules.

### **Key Advantages Over Traditional Ontologies**

| Capability | Traditional Ontology (OWL/RDF) | ProtoScript |
| ----- | ----- | ----- |
| **Concept Definition** | Static classes | Dynamic prototypes |
| **Inheritance** | Rigid, single-class hierarchies | Flexible, multiple inheritance |
| **Runtime Adaptation** | Difficult | Native support for modifying prototypes |
| **Reasoning Model** | Axioms \+ inference engine | Structural generalization \+ categorization |
| **Transparency** | High (formal semantics) | High (explicit graph structure) |
| **Flexibility** | Low | High for rapid prototyping |
| **Best Used For** | Interoperable semantic layers | Evolving, explainable knowledge graphs |

### **Use Cases Where ProtoScript Excels**

* **Semantic Modeling with Changing Requirements**: Adapt knowledge structures as new concepts emerge, without refactoring fixed hierarchies or running consistency checks.  
* **Explainable AI and Reasoning**: Trace *why* an instance matches a category via explicit graph paths, unlike opaque AI models.  
* **Auditable Systems**: Track prototype matches, function calls, and generalizations, ideal for domains like healthcare or finance.  
* **Integrating External Ontologies**: Import OWL or RDF schemas and evolve them with richer, dynamic behavior while preserving semantic integrity.

### **Example: Ontology-Like Modeling**

Consider modeling a geographic ontology:

```javascript
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

This creates a graph where `NewYork_City` and `NewYork_State` form a bidirectional relationship, akin to an ontology’s object properties, but with the flexibility to add new properties or subtypes at runtime.

### **Summary**

ProtoScript redefines ontology building with a developer-friendly, graph-based paradigm. Prototypes enable evolving knowledge structures, while built-in reasoning tools like LGG and subtyping provide transparency without heavyweight inference systems. For domains prioritizing adaptability, explainability, and real-world complexity—such as AI, regulatory systems, or messy data—ProtoScript offers a modern alternative to traditional ontologies.


The following sections begin the Representation layer, focusing on prototypes and core syntax.


# **What Are Prototypes?**

Prototypes are the cornerstone of ProtoScript, serving as the fundamental units for modeling data, behavior, and relationships within the Buffaly system’s graph-based framework. If you’re familiar with C\# or JavaScript, you can think of Prototypes as a hybrid of classes and objects, but with a twist: they are dynamic, graph-based entities that can act as both templates and instances, inherit from multiple parents, and adapt at runtime. This section introduces Prototypes, their key characteristics, and how they enable flexible knowledge representation, drawing analogies to familiar programming concepts and providing examples to illustrate their power.

## **Defining Prototypes**

A **Prototype** in ProtoScript is a node in a directed graph that encapsulates:

* **Properties**: Stored data, like fields in a C\# class, representing attributes or relationships (e.g., a city’s name or state).  
* **Behaviors**: Functions that compute results or modify the graph, similar to methods.  
* **Relationships**: Edges to other Prototypes, defining inheritance, properties, or computed links.

Unlike C\# classes, which are static templates for creating objects, Prototypes blur the line between template and instance. A Prototype can be used directly (like a singleton) or instantiated to create new nodes, each inheriting its structure and behavior. This flexibility makes Prototypes ideal for modeling diverse domains, from code structures to natural language semantics.

### **Analogy to Familiar Concepts**

For **C\# developers**:

* A Prototype is like a class that can also act as an object. Imagine defining a `City` class with fields and methods, but you can use `City` itself as an entity or create instances like `NewYork_City` that inherit and extend it.  
* Unlike C\#’s single inheritance, Prototypes support multiple parents, similar to interfaces but with richer property and behavior inheritance.

For **JavaScript developers**:

* Prototypes resemble JavaScript’s prototypal inheritance, where objects inherit directly from other objects. However, ProtoScript Prototypes are organized in a graph, not a linear chain, and include structured properties and functions for symbolic computation.

For **database developers**:

* Think of a Prototype as a node in a graph database (e.g., Neo4j), with properties as attributes and edges as relationships. ProtoScript adds programming logic, making these nodes programmable and dynamic.

## **Key Characteristics of Prototypes**

Prototypes are designed to be versatile and expressive, with the following defining features:

1. **Dual Role as Template and Instance**

   * A Prototype can define a reusable structure (like a C\# class) and be used directly as an entity (like an object).  
   * **Example**: The `City` Prototype defines properties for all cities, but `City` itself can represent a generic city in queries, or you can instantiate `NewYork_City` for specific use.  
2. **Multiple Inheritance**

   * Prototypes can inherit from multiple parent Prototypes, combining their properties and behaviors without the restrictions of C\#’s single inheritance.  
   * **Example**: `Buffalo_City` can inherit from both `City` and `Location`, gaining properties like `State` and `Coordinates`.  
3. **Stored (Extensional) Relationships**

   * Properties store data as edges to other nodes or values, representing fixed facts, similar to database records.  
   * **Example**: `NewYork_City.State = NewYork_State` creates a direct link in the graph.  
4. **Computed (Intensional) Relationships**

   * Functions define dynamic relationships or behaviors, computed at runtime by traversing or modifying the graph, akin to methods but graph-centric.  
   * **Example**: A function might determine if a city is in a specific state by checking its `State` property.  
5. **Dynamic Runtime Modifications**

   * Prototypes can be modified at runtime—adding properties, changing inheritance, or updating relationships—unlike C\#’s static type system.  
   * **Example**: You can dynamically add a `Population` property to `City` during execution.  
6. **Graph-Based Structure**

   * Prototypes form a directed graph (often a directed acyclic graph, or DAG, for inheritance, with cycles allowed in property relationships), where nodes are Prototypes and edges represent inheritance or properties.  
   * **Example**: The graph links `NewYork_City` to `NewYork_State` and back, forming a cycle via `State.Cities`.

### **Example: Modeling a City**

Here’s a simple ProtoScript example to illustrate Prototypes, relatable to object-oriented programming:

```javascript
prototype City {
    System.String Name = "";
    State State = new State();
}
prototype NewYork_City : City {
    Name = "New York City";
}
prototype State {
    Collection Cities = new Collection();
}
prototype NewYork_State : State {
    Cities = [NewYork_City];
}
NewYork_City.State = NewYork_State;
```

**What’s Happening?**

* `City` is a Prototype defining a template with `Name` and `State` properties, like a C\# class.  
* `NewYork_City` inherits from `City`, setting its `Name` to "New York City."  
* `State` defines a `Cities` collection, and `NewYork_State` links to `NewYork_City`.  
* The assignment `NewYork_City.State = NewYork_State` creates a bidirectional relationship, forming a cycle in the graph.  
* **C\# Equivalent**: Imagine a `City` class with a `State` field and a `State` class with a `List<City>` field, but ProtoScript allows runtime modifications and multiple inheritance.

This example shows how Prototypes model real-world entities as graph nodes, with edges representing relationships, offering more flexibility than traditional classes.

## **Prototypes in the Context of Ontologies**

In ontology terms, Prototypes are akin to **classes** or **individuals**, but with dynamic capabilities:

* **Classes**: Like OWL classes, Prototypes define concepts (e.g., `City`), but they can evolve at runtime without redefining the ontology.  
* **Individuals**: Prototypes like `NewYork_City` act as instances, linked to other nodes via properties, similar to RDF triples.  
* **Reasoning**: ProtoScript uses structural generalization (e.g., comparing instances to find common patterns) rather than formal axioms, making it more adaptable for evolving knowledge bases.

For example, the `City`/`State` graph above resembles an ontology with object properties (`City.State`, `State.Cities`), but ProtoScript’s runtime flexibility allows adding new properties or relationships without schema changes, unlike OWL’s static structure.

### **Example: Modeling a Fictional Domain**

To further illustrate, consider modeling characters from *The Simpsons*:

```javascript
prototype Person {
    System.String Gender = "";
    Location Location = new Location();
    Collection ParentOf = new Collection();
}
prototype Homer : Person {
    Gender = "Male";
    Location = SimpsonsHouse;
    ParentOf = [Bart, Lisa, Maggie];
}
prototype Marge : Person {
    Gender = "Female";
    Location = SimpsonsHouse;
    ParentOf = [Bart, Lisa, Maggie];
}
prototype SimpsonsHouse : Location {
    System.String Address = "742 Evergreen Terrace";
}
```

**What’s Happening?**

* `Person` is a Prototype with properties for `Gender`, `Location`, and `ParentOf`, like a C\# class with fields.  
* `Homer` and `Marge` inherit from `Person`, setting specific values and linking to `SimpsonsHouse`.  
* `SimpsonsHouse` is a `Location` node, connected to `Homer` and `Marge` via their `Location` properties.  
* **Graph View**: The graph links `Homer` and `Marge` to `SimpsonsHouse` and their children (`Bart`, `Lisa`, `Maggie`), forming a network of relationships.  
* **Database Equivalent**: This resembles a relational database with tables for `Person` and `Location`, but ProtoScript’s graph allows dynamic queries and modifications.

This example demonstrates how Prototypes can model complex, interconnected entities, making them suitable for knowledge representation tasks.

## **How Prototypes Work Internally**

Internally, Prototypes operate within a graph-based runtime:

* **Nodes**: Each Prototype is a node with a unique identifier, storing properties and functions.  
* **Edges**: Relationships are edges, including:  
  * **Inheritance Edges**: `isa` links to parent Prototypes (e.g., `NewYork_City isa City`).  
  * **Property Edges**: Links to other nodes or values (e.g., `NewYork_City.State → NewYork_State`).  
  * **Computed Edges**: Functions create dynamic links at runtime.  
* **Graph Structure**: The runtime manages a directed graph, typically a DAG for inheritance, but allows cycles in property relationships (e.g., `City ↔ State`).  
* **Instantiation**: Creating a new Prototype (e.g., `new City()`) clones the node, copying properties and establishing new edges as needed.

This graph-centric approach enables ProtoScript to handle dynamic, real-world relationships more naturally than traditional object-oriented systems, where hierarchies are fixed and cycles are restricted to object references.

### **Example: Dynamic Modification**

Prototypes can adapt at runtime, a feature not easily achievable in C\#:

```javascript
prototype Buffalo {
    System.String Name = "Buffalo";
}
// Dynamically add a type
Typeofs.Insert(Buffalo, Animal);
// Add a property
prototype Color;
prototype Red : Color;
Buffalo.Properties[Color] = Red;
```

**What’s Happening?**

* `Buffalo` starts as a simple Prototype with a `Name`.  
* `Typeofs.Insert` adds `Animal` as a parent, making `Buffalo typeof Animal` true.  
* A `Color` property is dynamically added, linking `Buffalo` to `Red`.  
* **C\# Equivalent**: This would require reflection or dynamic types, which are less straightforward and less integrated than ProtoScript’s native support.  
* **Graph View**: The runtime updates the graph, adding an `isa` edge to `Animal` and a property edge to `Red`.

This example highlights Prototypes’ flexibility, allowing developers to evolve their models as requirements change.

## **Why Prototypes Matter**

Prototypes empower developers to:

* **Model Complex Relationships**: Capture real-world complexity, like bidirectional state-city links, with ease.  
* **Adapt Dynamically**: Modify structures at runtime, ideal for evolving domains like AI or data integration.  
* **Unify Diverse Domains**: Represent code, language, or concepts in a single graph-based framework.  
* **Enable Reasoning**: Support structural generalization and categorization, as explored in later sections.

For developers, Prototypes offer a familiar yet powerful abstraction, combining the structure of classes with the flexibility of graphs, making ProtoScript a versatile tool for modern applications.

### **Example: Code Structure**

Prototypes can model programming constructs, such as a C\# variable declaration:

```javascript
prototype CSharp_VariableDeclaration {
    CSharp_Type Type = new CSharp_Type();
    System.String VariableName = "";
    CSharp_Expression Initializer = new CSharp_Expression();
}
prototype CSharp_Type {
    System.String TypeName = "";
}
prototype Int_Declaration : CSharp_VariableDeclaration {
    Type.TypeName = "int";
    VariableName = "i";
    Initializer = IntegerLiteral_0;
}
prototype IntegerLiteral_0 : CSharp_Expression {
    System.String Value = "0";
}
```

**What’s Happening?**

* `CSharp_VariableDeclaration` defines a template for variable declarations, like a C\# class.  
* `Int_Declaration` represents `int i = 0`, inheriting and setting specific values.  
* `IntegerLiteral_0` models the initializer `0` as a node.  
* **Graph View**: Nodes link `Int_Declaration` to `CSharp_Type` (`int`) and `IntegerLiteral_0` (`0`), forming a hierarchical structure.  
* **Use Case**: This could be used for code analysis or transformation, like refactoring `int i = 0` to another form.

This example shows how Prototypes unify code representation with graph-based modeling, a theme expanded in later sections.

## **Moving Forward**

Prototypes are the foundation of ProtoScript, providing a flexible, graph-based way to model entities and relationships. In the next section, we’ll explore ProtoScript’s **syntax and features**, diving into how you define, manipulate, and extend Prototypes using C\#-inspired constructs. With Prototypes under your belt, you’re ready to start building dynamic, interconnected systems\!

# **ProtoScript Syntax and Features**

ProtoScript’s syntax and features form the backbone of its graph-based programming paradigm, enabling developers to define, manipulate, and query Prototypes within the Buffaly system. If you’re familiar with C\#, ProtoScript’s syntax will feel intuitive, with a structure reminiscent of classes, methods, and attributes, but tailored for a dynamic, graph-oriented model. Unlike C\#’s static, class-based approach, ProtoScript is prototype-based, allowing runtime flexibility, multiple inheritance, and graph traversal operations. This section introduces ProtoScript’s core syntax and features, providing examples to illustrate their use and drawing analogies to familiar programming concepts. We’ll cover how these elements work together to model complex relationships, with a focus on clarity for developers new to ProtoScript.

## **Syntax Overview**

ProtoScript uses a C\#-like syntax, with semicolons to terminate statements, curly braces `{}` to define blocks, and comments using `//` or `/* */`. Its constructs are designed to create and manipulate Prototypes—graph nodes that represent entities and their relationships. Below are the key syntactic elements, each explained with comparisons to C\# or JavaScript to ease the transition.

### **Basic Structure**

Here’s a simple ProtoScript example to set the stage:

```javascript
prototype City {
    System.String Name = "";
    State State = new State();
}
prototype State {
    Collection Cities = new Collection();
}
```

**What’s Happening?**

* `prototype City` defines a Prototype, like a C\# class, with properties `Name` and `State`.  
* `prototype State` defines another Prototype with a `Cities` collection.  
* The syntax mirrors C\#’s class and field declarations but operates on graph nodes.

### **Comments**

ProtoScript supports C\#-style comments:

* **Single-line**: `// This is a comment`  
* **Multi-line**: `/* This is a multi-line comment */`

### **Identifiers**

Identifiers (e.g., Prototype names, properties, functions) follow C\# conventions:

* Alphanumeric with underscores, starting with a letter or underscore (e.g., `City`, `NewYork_City`, `_internalState`).  
* Case-sensitive, like C\#.

## **Core Features and Syntax**

ProtoScript’s features are tailored for graph-based programming, enabling developers to define Prototypes, their properties, behaviors, and relationships. Below, we detail each feature, its syntax, and its role in the graph model, with examples to illustrate practical use.

### **1\. Prototype Declaration**

**Purpose**: Defines a Prototype, the fundamental unit in ProtoScript, acting as both a template and an instance.

**Syntax**:

```javascript
prototype Name : Parent1, Parent2 {
    // Properties, functions, and other members
}
```

**Details**:

* `Name` is the Prototype’s identifier (e.g., `City`).  
* `: Parent1, Parent2` specifies optional parent Prototypes for multiple inheritance, like C\# interfaces but inheriting properties and behaviors.  
* The body `{}` contains properties, functions, and other members.  
* Unlike C\# classes, Prototypes can be used directly or instantiated with `new`.

**C\# Analogy**: Similar to a `class` declaration, but supports multiple inheritance and runtime modification, unlike C\#’s single inheritance (plus interfaces).

**Example**:

```javascript
prototype Location {
    System.String Name = "";
}
prototype City : Location {
    State State = new State();
}
prototype Buffalo_City : City {
    Name = "Buffalo";
}
```

**What’s Happening?**

* `Location` is a base Prototype with a `Name` property.  
* `City` inherits from `Location`, adding a `State` property.  
* `Buffalo_City` inherits from `City`, setting `Name` to "Buffalo".  
* **Graph View**: `Buffalo_City` is a node with an `isa` edge to `City`, which links to `Location`.

### **2\. Properties**

**Purpose**: Define stored (extensional) data within a Prototype, representing attributes or relationships as edges to other nodes or values.

**Syntax**:

Type Name \= DefaultValue;

**Details**:

* `Type` is a Prototype or native type (e.g., `System.String`, `State`).  
* `Name` is the property identifier.  
* `DefaultValue` is optional, often a `new` instance or literal (e.g., `""`, `new State()`).  
* Properties can be fully qualified (e.g., `City::State`) to resolve conflicts in multiple inheritance, unlike C\#’s implicit resolution.  
* Properties are graph edges, linking the Prototype to another node or value.

**C\# Analogy**: Like fields in a class, but properties are inherently part of the graph, allowing dynamic addition and traversal.

**Example**:

```javascript
prototype Person {
    System.String Gender = "";
    Location Location = new Location();
}
prototype Homer : Person {
    Gender = "Male";
    Location = SimpsonsHouse;
}
prototype SimpsonsHouse : Location {
    System.String Address = "742 Evergreen Terrace";
}
```

**What’s Happening?**

* `Person` defines `Gender` (a native value) and `Location` (a Prototype instance).  
* `Homer` sets `Gender` to "Male" and links `Location` to `SimpsonsHouse`.  
* `SimpsonsHouse` has an `Address` property.  
* **Graph View**: `Homer` has edges to `System.String[Male]` and `SimpsonsHouse`.

### **3\. Functions**

**Purpose**: Define computed (intensional) behaviors, operating on the graph to compute results or modify relationships.

**Syntax**:

function Name(Parameters) : ReturnType {  
    // Statements  
}

**Details**:

* `Name` is the function identifier.  
* `Parameters` are typed, like C\# method parameters.  
* `ReturnType` is a Prototype or native type.  
* The body uses C\#-like control flow (`if`, `foreach`) and graph operations (e.g., property access, traversal).  
* Functions can modify the graph (e.g., setting properties) or compute values by traversing edges.

**C\# Analogy**: Like methods, but designed for graph manipulation, often traversing or updating node relationships.

**Example**:

prototype City {  
    System.String Name \= "";  
    State State \= new State();  
    function GetStateName() : System.String {  
        return State.Name;  
    }  
}  
prototype State {  
    System.String Name \= "";  
}  
prototype NewYork\_City : City {  
    Name \= "New York City";  
}  
prototype NewYork\_State : State {  
    Name \= "New York";  
}  
NewYork\_City.State \= NewYork\_State;

**What’s Happening?**

* `City` defines a `GetStateName` function that traverses the `State` property to return its `Name`.  
* `NewYork_City` links to `NewYork_State`.  
* Calling `NewYork_City.GetStateName()` returns "New York".  
* **Graph View**: The function follows the `State` edge to access `NewYork_State.Name`.

### **4\. Annotations**

**Purpose**: Attach metadata to Prototypes or functions, guiding runtime behavior, such as natural language processing or categorization.

**Syntax**:

\[AnnotationName(Parameters)\]

**Details**:

* Annotations are applied to Prototypes or functions, similar to C\# attributes.  
* Common annotations:  
  * `[Lexeme.SingularPlural("word", "plural")]` maps Prototypes to natural language tokens.  
  * `[SubType]` marks a Prototype for dynamic categorization (detailed in later sections).  
  * `[TransferFunction(Dimension)]` defines transformation functions (covered later).  
* Annotations are processed by the ProtoScript runtime, influencing interpretation or execution.

**C\# Analogy**: Like `[Attribute]` in C\#, but with a focus on graph behavior and runtime processing for tasks like NLP.

**Example**:

\[Lexeme.SingularPlural("city", "cities")\]  
prototype City {  
    System.String Name \= "";  
}

**What’s Happening?**

* The `[Lexeme.SingularPlural]` annotation links `City` to the words "city" and "cities" for natural language processing.  
* The runtime uses this to map text like "cities" to the `City` Prototype.  
* **Graph View**: The annotation adds metadata to the `City` node, used during parsing.

### **5\. Categorization Operator (`->`)**

**Purpose**: Tests if a Prototype satisfies a categorization condition, querying its graph structure.

**Syntax**:

prototype \-\> Type { Condition }

**Details**:

* `prototype` is the target Prototype.  
* `Type` specifies the context Prototype for the condition.  
* `Condition` is a boolean expression, often involving property checks or `typeof`.  
* Returns `true` if the condition holds, enabling dynamic categorization.

**C\# Analogy**: Like a LINQ `Where` clause, but operates on graph nodes and relationships rather than collections.

**Example**:

prototype City {  
    System.String Name \= "";  
    State State \= new State();  
}  
prototype State {  
    System.String Name \= "";  
}  
prototype NewYork\_City : City {  
    Name \= "New York City";  
}  
prototype NewYork\_State : State {  
    Name \= "New York";  
}  
NewYork\_City.State \= NewYork\_State;

function IsInNewYork(City city) : bool {  
    return city \-\> City { this.State.Name \== "New York" };  
}

**What’s Happening?**

* `IsInNewYork` checks if a `City`’s `State.Name` is "New York".  
* `NewYork_City -> City { this.State.Name == "New York" }` returns `true`.  
* **Graph View**: The operator traverses the `State` edge to check `Name`.

### **6\. Typeof Operator**

**Purpose**: Checks if a Prototype inherits from another, verifying its position in the inheritance graph.

**Syntax**:

prototype typeof Type

**Details**:

* Returns `true` if `prototype` has a direct or transitive `isa` relationship to `Type`.  
* Used in conditions, functions, or categorizations.

**C\# Analogy**: Like the `is` operator in C\#, but operates on the graph’s inheritance DAG.

**Example**:

prototype Location {  
    System.String Name \= "";  
}  
prototype City : Location {  
    System.String Name \= "";  
}  
prototype Buffalo\_City : City {  
    Name \= "Buffalo";  
}

function IsCity(Prototype proto) : bool {  
    return proto typeof City;  
}

**What’s Happening?**

* `IsCity(Buffalo_City)` returns `true` because `Buffalo_City isa City`.  
* **Graph View**: The operator checks for an `isa` edge from `Buffalo_City` to `City`.

### **7\. Collections**

**Purpose**: Manage lists or sets of Prototypes, representing one-to-many relationships.

**Syntax**:

Collection Name \= new Collection();

**Details**:

* `Collection` is a built-in Prototype, like `List<T>` in C\#.  
* Methods include `Add`, `Remove`, and `Count`, similar to C\# collections.  
* Collections are graph edges to multiple nodes.

**C\# Analogy**: Like `List<T>` or `IEnumerable<T>`, but integrated into the graph structure.

**Example**:

prototype State {  
    Collection Cities \= new Collection();  
}  
prototype City {  
    System.String Name \= "";  
}  
prototype NewYork\_State : State;  
prototype NewYork\_City : City {  
    Name \= "New York City";  
}  
NewYork\_State.Cities.Add(NewYork\_City);

**What’s Happening?**

* `State.Cities` is a collection linking to `City` nodes.  
* `NewYork_State.Cities.Add(NewYork_City)` creates an edge to `NewYork_City`.  
* **Graph View**: `NewYork_State` has a `Cities` edge to `NewYork_City`.

### **8\. Control Flow**

**Purpose**: Provide standard programming constructs for logic and iteration.

**Syntax**:

if (condition) {  
    // Statements  
} else {  
    // Statements  
}

foreach (Type variable in collection) {  
    // Statements  
}

**Details**:

* `if` and `foreach` mirror C\#’s syntax and behavior.  
* Conditions often involve graph queries (e.g., `typeof`, `->`).  
* Used within functions to control graph operations.

**C\# Analogy**: Nearly identical to C\#’s `if` and `foreach`, but applied to graph nodes.

**Example**:

prototype State {  
    Collection Cities \= new Collection();  
    function CountCities() : System.Int32 {  
        System.Int32 count \= 0;  
        foreach (City city in Cities) {  
            count \= count \+ 1;  
        }  
        return count;  
    }  
}

**What’s Happening?**

* `CountCities` iterates over `Cities`, counting nodes.  
* **Graph View**: The `foreach` traverses `Cities` edges to increment `count`.

## **Integration with C\#**

ProtoScript integrates seamlessly with C\#:

* **Native Types**: `System.String`, `System.Int32`, etc., mirror C\# primitives, wrapped as NativeValuePrototypes.  
* **Runtime Calls**: Functions can invoke C\# methods (e.g., `String.Format`).  
* **Type Conversions**: The runtime handles mappings between ProtoScript and C\# types.

**Example**:

prototype City {  
    System.String Name \= "";  
    function FormatName() : System.String {  
        return String.Format("City: {0}", Name);  
    }  
}

**What’s Happening?**

* `FormatName` uses C\#’s `String.Format` to format the `Name` property.  
* **Graph View**: The function accesses the `Name` node and returns a new `System.String`.

## **Internal Mechanics**

ProtoScript’s syntax operates on a graph-based runtime:

* **Nodes**: Prototypes, properties, and functions are nodes with unique IDs.  
* **Edges**: Inheritance (`isa`), properties, and computed relationships link nodes.  
* **Runtime**: Manages instantiation, traversal, and execution, ensuring graph integrity.  
* **Operators**: `->` and `typeof` trigger graph queries, traversing edges to evaluate conditions.  
* **Annotations**: Guide runtime behavior, processed by the interpreter for tasks like NLP.

## **Why These Features Matter**

ProtoScript’s syntax and features provide:

* **Familiarity**: C\#-like syntax reduces the learning curve for developers.  
* **Expressiveness**: Properties, functions, and operators enable rich graph modeling.  
* **Flexibility**: Runtime modifications and graph operations support dynamic systems.  
* **Integration**: Seamless C\# interoperability leverages existing tools and libraries.

### **Example: Modeling a Code Structure**

To show how these features combine, consider modeling a C\# variable declaration:

prototype CSharp\_VariableDeclaration {  
    CSharp\_Type Type \= new CSharp\_Type();  
    System.String VariableName \= "";  
    CSharp\_Expression Initializer \= new CSharp\_Expression();  
    function IsInitialized() : bool {  
        return Initializer \-\> CSharp\_Expression { this.Value \!= "" };  
    }  
}  
prototype CSharp\_Type {  
    System.String TypeName \= "";  
}  
prototype CSharp\_Expression {  
    System.String Value \= "";  
}  
prototype Int\_Declaration : CSharp\_VariableDeclaration {  
    Type.TypeName \= "int";  
    VariableName \= "i";  
    Initializer \= IntegerLiteral\_0;  
}  
prototype IntegerLiteral\_0 : CSharp\_Expression {  
    Value \= "0";  
}

**What’s Happening?**

* `CSharp_VariableDeclaration` defines a Prototype with properties and a function.  
* `IsInitialized` uses the `->` operator to check the `Initializer`’s `Value`.  
* `Int_Declaration` models `int i = 0`, linking to `IntegerLiteral_0`.  
* **Graph View**: Nodes connect `Int_Declaration` to `CSharp_Type` (`int`) and `IntegerLiteral_0` (`0`).  
* **Use Case**: This could support code analysis, checking if variables are initialized.

## **Moving Forward**

ProtoScript’s syntax and features provide a robust foundation for graph-based programming, enabling you to define and manipulate Prototypes with ease. In the next section, we’ll explore **NativeValuePrototypes**, which encapsulate primitive values as graph nodes, ensuring uniformity and enabling seamless integration with the Prototype system. With these tools, you’re ready to start building dynamic, interconnected models\!

# **NativeValuePrototypes**

In ProtoScript, **NativeValuePrototypes** are specialized Prototypes that encapsulate primitive values—such as strings, booleans, integers, and doubles—as nodes within the graph-based Buffaly system. For developers familiar with C\# or JavaScript, NativeValuePrototypes are akin to primitive values (e.g., `int`, `string`) elevated to full-fledged graph entities, enabling them to participate in relationships, inheritance, and runtime operations like any other Prototype. This section explores the purpose, syntax, and mechanics of NativeValuePrototypes, with examples illustrating their role and analogies to familiar programming concepts.

## **What Are NativeValuePrototypes?**

A **NativeValuePrototype** is a Prototype that wraps a primitive value, such as `"hello"`, `true`, or `42`, as a node in the graph, complete with a type identifier and metadata. Unlike raw primitives in C\# (e.g., `int x = 5`), which lack structure, NativeValuePrototypes integrate seamlessly into ProtoScript’s graph model, allowing uniform querying, serialization, and computation. ProtoScript’s runtime translates literal values (e.g., string literals, boolean literals) into NativeValuePrototypes, making them easy to use while preserving their graph-based nature.

### **Key Characteristics**

1. **Encapsulation of Primitive Values**

   * NativeValuePrototypes hold a single primitive value and its type (e.g., `string`, `System.String`).  
   * **Example**: `"Buffalo"` or `System.String["Buffalo"]` represents the string "Buffalo" as a graph node.  
2. **Graph Integration**

   * They are nodes with edges to other Prototypes, functioning as properties or linking to complex structures.  
   * **Example**: A `City` Prototype’s `Name` property might be `"Buffalo"`, linking to a string node.  
3. **Uniformity**

   * Treating primitives as Prototypes ensures all entities are manipulable identically, simplifying graph operations.  
   * **Example**: Querying `string` nodes is as straightforward as querying `City` nodes.  
4. **Support for Stored Relationships**

   * They store extensional facts, preserving data for serialization or analysis.  
   * **Example**: `false` as a property indicates a non-nullable variable.  
5. **Foundation for Computed Relationships**

   * They serve as inputs to functions for dynamic operations (e.g., string manipulation).  
   * **Example**: A function could uppercase `"hello"`.

### **Analogy to Familiar Concepts**

For **C\# developers**:

* NativeValuePrototypes are like boxed primitives (e.g., `object x = 5`), but as graph nodes. Using `string` with `"hello"` is like `string s = "hello"`, while `System.String["hello"]` is a structured node with metadata, akin to a lightweight class.  
* Unlike C\#’s `struct` types, NativeValuePrototypes are graph-integrated, not standalone.

For **JavaScript developers**:

* They resemble primitive wrapper objects (e.g., `new String("hello")`), but persist as graph nodes, not transient wrappers, designed for traversal and relationships.

For **database developers**:

* Think of NativeValuePrototypes as graph database nodes with a single value property, linked to others, like scalar values in a relational database but with graph capabilities.

## **Syntax and Usage**

NativeValuePrototypes are used as property values or function inputs/outputs, with ProtoScript allowing direct literal initializers (`"hello"`, `true`, `42`) or explicit type notation (`System.String["hello"]`). The runtime translates literals into NativeValuePrototypes, ensuring graph consistency.

**Syntax Options**:

**Primitive Types with Literals**:

 string Name \= "value";  
bool Flag \= true;  
int Number \= 42;

1.   
   * Uses lowercase types (`string`, `bool`, `int`) for raw primitive values.  
   * Literals are translated by the runtime into NativeValuePrototypes (e.g., `"value"` becomes a `string` node).

**Prototype Types with Literals**:

 System.String Name \= "value";  
System.Boolean Flag \= true;  
System.Int32 Number \= 42;

2.   
   * Uses uppercase .NET types (`System.String`, `System.Boolean`, `System.Int32`) to explicitly denote NativeValuePrototypes.  
   * Literals are wrapped as nodes with type metadata.

**Details**:

* Both forms create graph nodes, but lowercase types emphasize simplicity, while uppercase types highlight the Prototype nature.  
* The notation `Type["value"]` (e.g., `System.String["hello"]`) explicitly indicates a NativeValuePrototype instance, though direct literals (e.g., `"hello"`) are preferred for brevity.  
* The runtime ensures literals are treated as NativeValuePrototypes, preserving type and value for graph operations.

**C\# Analogy**: Assigning `string s = "hello"` in C\# is like `string Name = "hello"` in ProtoScript, but the latter creates a graph node. Using `System.String["hello"]` adds explicit Prototype metadata, like a structured object.

**Example**:

prototype City {  
    string Name \= "";  
    System.Boolean IsCapital \= false;  
}  
prototype Buffalo\_City : City {  
    Name \= "Buffalo";  
    IsCapital \= System.Boolean\[False\];  
}

**What’s Happening?**

* `City` defines `Name` as a `string` with a literal `""` and `IsCapital` as a `System.Boolean` with `false`.  
* `Buffalo_City` sets `Name` to `"Buffalo"` (runtime translates to a `string` node) and `IsCapital` to `System.Boolean[False]` (explicit Prototype).  
* **Graph View**: `Buffalo_City` links to nodes for `"Buffalo"` and `false`.

## **Common Native Types**

ProtoScript supports native types aligned with C\# primitives, available in two forms:

* **Primitive Types** (lowercase):  
  * `string`: Text values (e.g., `"hello"`).  
  * `bool`: True/false values (e.g., `true`).  
  * `int`: 32-bit integers (e.g., `42`).  
  * `double`: Floating-point numbers (e.g., `3.14`).  
* **Prototype Types** (uppercase):  
  * `System.String`: Text nodes (e.g., `System.String["hello"]`).  
  * `System.Boolean`: Boolean nodes (e.g., `System.Boolean[True]`).  
  * `System.Int32`: Integer nodes (e.g., `System.Int32[42]`).  
  * `System.Double`: Floating-point nodes (e.g., `System.Double[3.14]`).

Both forms are NativeValuePrototypes in the graph, with uppercase types emphasizing their node structure.

## **Why NativeValuePrototypes Matter**

NativeValuePrototypes are crucial for ProtoScript’s graph-based model:

1. **Unified Representation**

   * Primitives as nodes ensure all data is treated uniformly, simplifying queries.  
   * **Example**: Querying `string` nodes finds both variable names and city names.  
2. **Stored Relationships**

   * They preserve exact values for serialization or analysis.  
   * **Example**: `"i"` as a variable name ensures fidelity in code structures.  
3. **Computed Potential**

   * They enable dynamic operations via functions.  
   * **Example**: Uppercasing `"hello"` in a function.  
4. **Serialization and Interoperability**

   * Type metadata ensures accurate serialization (e.g., to JSON).  
   * **Example**: `System.Int32[0]` serializes with its type.  
5. **Flexibility**

   * They support diverse domains without special cases.  
   * **Example**: `true` flags a condition in NLP or code analysis.

### **Example: Modeling a C\# Variable Declaration**

Representing `int i = 0`:

prototype CSharp\_VariableDeclaration {  
    CSharp\_Type Type \= new CSharp\_Type();  
    string VariableName \= "";  
    CSharp\_Expression Initializer \= new CSharp\_Expression();  
}  
prototype CSharp\_Type {  
    string TypeName \= "";  
    bool IsNullable \= false;  
}  
prototype CSharp\_Expression {  
    string Value \= "";  
}  
prototype Int\_Declaration : CSharp\_VariableDeclaration {  
    Type.TypeName \= "int";  
    VariableName \= "i";  
    Initializer \= IntegerLiteral\_0;  
}  
prototype IntegerLiteral\_0 : CSharp\_Expression {  
    Value \= "0";  
}

**What’s Happening?**

* `CSharp_VariableDeclaration` uses `string` for `VariableName` and `CSharp_Type` for `Type`.  
* `Int_Declaration` sets `TypeName` to `"int"`, `VariableName` to `"i"`, and `Initializer.Value` to `"0"`, all as NativeValuePrototypes.  
* **Graph View**: `Int_Declaration` links to nodes for `"int"`, `"i"`, and `"0"`.  
* **Use Case**: Supports code analysis or transformation.

### **Example: Natural Language Semantics**

Modeling "I need to buy some covid-19 test kits":

prototype Need {  
    BaseObject Subject \= new BaseObject();  
    Action Object \= new Action();  
}  
prototype Action {  
    string Infinitive \= "";  
}  
prototype COVID\_TestKit {  
    string Quantity \= "";  
}  
prototype Need\_BuyTestKits : Need {  
    Subject \= Person\_I;  
    Object \= BuyAction;  
}  
prototype Person\_I : BaseObject {  
    System.String Pronoun \= "I";  
}  
prototype BuyAction : Action {  
    Infinitive \= "ToBuy";  
    BaseObject Object \= TestKit;  
}  
prototype TestKit : COVID\_TestKit {  
    Quantity \= "Some";  
}

**What’s Happening?**

* `Need_BuyTestKits` uses `string` for `Infinitive` (`"ToBuy"`) and `Quantity` (`"Some"`), and `System.String` for `Pronoun` (`"I"`).  
* Literals are translated to NativeValuePrototypes by the runtime.  
* **Graph View**: Links to nodes for `"I"`, `"ToBuy"`, and `"Some"`.  
* **Use Case**: Enables semantic parsing for AI.

## **Internal Mechanics**

NativeValuePrototypes operate within ProtoScript’s graph-based runtime:

* **Nodes**: Each is a node with a type (e.g., `string`, `System.String`) and value (e.g., `"hello"`), identified by a unique ID.  
* **Edges**: Link to other Prototypes via properties (e.g., `City.Name → "Buffalo"`).  
* **Runtime**: Translates literals to NativeValuePrototypes, manages instantiation, and ensures serialization fidelity.  
* **Traversal**: Operators like `->` or functions treat them like any Prototype.

## **Integration with C\#**

NativeValuePrototypes align with C\# primitives:

* **Type Mapping**: `string`/`System.String` maps to `string`, `bool`/`System.Boolean` to `bool`, etc.  
* **Usage**: Literals or explicit nodes can be passed to C\# methods (e.g., `String.Format("hello")`).  
* **Conversion**: The runtime handles seamless translation.

**Example**:

prototype City {  
    string Name \= "";  
    function FormatName() : string {  
        return String.Format("City: {0}", Name);  
    }  
}  
prototype Buffalo\_City : City {  
    Name \= "Buffalo";  
}

**What’s Happening?**

* `Name` uses `string` with `"Buffalo"`, translated to a NativeValuePrototype.  
* `FormatName` uses C\#’s `String.Format`.  
* **Graph View**: Accesses the `"Buffalo"` node.

## **Why NativeValuePrototypes Are Essential**

NativeValuePrototypes ensure:

* **Consistency**: Uniform treatment of all data as nodes.  
* **Fidelity**: Accurate storage of values.  
* **Flexibility**: Support for diverse domains.  
* **Interoperability**: Seamless C\# integration.

## **Moving Forward**

NativeValuePrototypes anchor ProtoScript’s graph model, making even simple values powerful graph entities. In the next section, we’ll explore **Examples of Prototype Creation**, showing how Prototypes and NativeValuePrototypes model real-world scenarios, from code to natural language semantics. You’re now equipped to build rich, interconnected systems\!

# **Examples of Prototype Creation**

ProtoScript’s Prototypes are exceptionally versatile, capable of modeling any data type—from C\# code and SQL queries to database objects and natural language semantics—with the same ease and flexibility. This ability to unify diverse domains within a single graph-based framework sets ProtoScript apart from traditional ontologies, which often rely on rigid, domain-specific schemas and complex logical axioms. By representing everything as Prototypes, ProtoScript enables developers to discover relationships and transformations across domains, such as mapping a natural language request to a SQL query or transforming C\# code into a semantic model. This section showcases four examples of Prototype creation, illustrating how ProtoScript handles C\# variable declarations, SQL queries, database objects, and natural language, and highlights the power of cross-domain integration.

## **Why Prototypes Go Beyond Traditional Ontologies**

Traditional ontologies, such as those built with OWL or RDF, are designed for structured knowledge representation, typically within a single domain (e.g., medical terminology or geographic data). They use static classes, predefined properties, and formal axioms, which can be inflexible and labor-intensive to adapt across diverse data types. ProtoScript’s Prototypes overcome these limitations in several key ways:

1. **Universal Data Representation**

   * Prototypes can model any data type—C\# code, SQL, database records, or natural language—as graph nodes with properties and relationships, without requiring domain-specific schemas.  
   * **Example**: A single ProtoScript model can represent a C\# variable (`int i = 0`) and a natural language phrase ("buy test kits") as interconnected Prototypes.  
2. **Ease of Use Across Domains**

   * ProtoScript’s C\#-like syntax and dynamic nature make it as straightforward to define a SQL query structure as a semantic parse, reducing the learning curve compared to ontology tools like Protégé.  
   * **Example**: Developers can use the same `prototype` construct for both a database table and a linguistic concept.  
3. **Dynamic Flexibility**

   * Unlike static ontologies, Prototypes support runtime modifications, allowing models to evolve as new data types or relationships emerge.  
   * **Example**: A Prototype for a database object can be extended to include natural language annotations without redefining the ontology.  
4. **Cross-Domain Relationships and Transformations**

   * By unifying data types in a graph, ProtoScript enables the discovery of relationships (e.g., a C\# variable’s type matching a database column’s type) and transformations (e.g., converting a natural language query to SQL).  
   * **Example**: A natural language request can be transformed into a SQL query by mapping Prototypes across domains.  
5. **Lightweight Reasoning**

   * ProtoScript uses structural generalization (e.g., comparing instances to find patterns) instead of heavy axiomatic reasoning, making it more adaptable for cross-domain applications.  
   * **Example**: Generalizing a C\# variable and a SQL query to a shared "data declaration" concept.

### **Analogy to Familiar Concepts**

For **C\# developers**:

* Prototypes are like classes that can model any entity—code, queries, or text—with the flexibility to link them in a graph, unlike C\#’s domain-specific types (e.g., `SqlCommand` for SQL, `string` for text).  
* Think of ProtoScript as a universal ORM (Object-Relational Mapping) that maps not just databases but also code and language to a graph.

For **JavaScript developers**:

* Prototypes resemble JavaScript objects but organized in a graph, allowing you to model SQL or natural language as easily as JSON, with dynamic links between them.

For **database developers**:

* Prototypes are like graph database nodes that can represent tables, queries, or even sentences, with edges enabling transformations (e.g., query to result set).

## **Examples of Prototype Creation**

Below are four examples demonstrating ProtoScript’s ability to model C\# code, SQL queries, database objects, and natural language semantics, showcasing their uniform representation and cross-domain potential.

### **Example 1: C\# Variable Declaration**

**Scenario**: Model the C\# declaration `int i = 0`.

prototype CSharp\_VariableDeclaration {  
    CSharp\_Type Type \= new CSharp\_Type();  
    string VariableName \= "";  
    CSharp\_Expression Initializer \= new CSharp\_Expression();  
}  
prototype CSharp\_Type {  
    string TypeName \= "";  
    bool IsNullable \= false;  
}  
prototype CSharp\_Expression {  
    string Value \= "";  
}  
prototype Int\_Declaration : CSharp\_VariableDeclaration {  
    Type.TypeName \= "int";  
    VariableName \= "i";  
    Initializer \= IntegerLiteral\_0;  
}  
prototype IntegerLiteral\_0 : CSharp\_Expression {  
    Value \= "0";  
}

**What’s Happening?**

* `CSharp_VariableDeclaration` defines a template for variable declarations, with properties for type, name, and initializer.  
* `Int_Declaration` models `int i = 0`, using string literals (`"int"`, `"i"`, `"0"`) that the runtime translates to NativeValuePrototypes.  
* **Graph View**: `Int_Declaration` links to nodes for `"int"`, `"i"`, and `"0"`, forming a hierarchical structure.  
* **Beyond Ontologies**: Unlike OWL, which would require a specific ontology for code constructs, ProtoScript uses a generic `prototype` construct, easily adaptable to other languages (e.g., Java).

**Cross-Domain Potential**:

* **Relationship**: The `TypeName` (`"int"`) could link to a database column’s type, enabling type consistency checks.  
* **Transformation**: This Prototype could be transformed into a variable declaration in another language (e.g., `let i: number = 0` in TypeScript).

### **Example 2: SQL Query**

**Scenario**: Model the SQL query `SELECT TOP 10 * FROM Prototypes ORDER BY 1 DESC`.

prototype SQL\_Select {  
    Collection Columns \= new Collection();  
    SQL\_Table Table \= new SQL\_Table();  
    string Limit \= "";  
    Collection OrderBys \= new Collection();  
}  
prototype SQL\_Table {  
    string TableName \= "";  
}  
prototype SQL\_Expression {  
    string Value \= "";  
}  
prototype SQL\_OrderByClause {  
    SQL\_Expression Expression \= new SQL\_Expression();  
    int SortDirection \= 0; // 1 for DESC, 0 for ASC  
}  
prototype Select\_Prototypes : SQL\_Select {  
    Columns \= \[Wildcard\_Expression\];  
    Table.TableName \= "Prototypes";  
    Limit \= "10";  
    OrderBys \= \[OrderBy\_FirstColumn\];  
}  
prototype Wildcard\_Expression : SQL\_Expression {  
    Value \= "\*";  
}  
prototype OrderBy\_FirstColumn : SQL\_OrderByClause {  
    Expression \= NumberLiteral\_1;  
    SortDirection \= 1;  
}  
prototype NumberLiteral\_1 : SQL\_Expression {  
    Value \= "1";  
}

**What’s Happening?**

* `SQL_Select` defines a template for SQL SELECT queries, with properties for columns, table, limit, and order-by clauses.  
* `Select_Prototypes` models the query, using literals (`"Prototypes"`, `"10"`, `"*"`).  
* **Graph View**: `Select_Prototypes` links to nodes for `"Prototypes"`, `"10"`, and `"*"` (wildcard), with `OrderBys` linking to `"1"` and `SortDirection`.  
* **Beyond Ontologies**: Traditional ontologies struggle with procedural constructs like queries; ProtoScript models them as easily as static data, using the same graph structure.

**Cross-Domain Potential**:

* **Relationship**: The `TableName` (`"Prototypes"`) could link to a database object’s schema, ensuring consistency.  
* **Transformation**: The query could be transformed into a natural language description (e.g., "Get the top 10 prototypes, sorted descending by the first column").

### **Example 3: Database Object**

**Scenario**: Model a database table `Employees` with columns `ID` (integer) and `Name` (varchar).

prototype Database\_Table {  
    string TableName \= "";  
    Collection Columns \= new Collection();  
}  
prototype Database\_Column {  
    string ColumnName \= "";  
    string DataType \= "";  
}  
prototype Employees\_Table : Database\_Table {  
    TableName \= "Employees";  
    Columns \= \[ID\_Column, Name\_Column\];  
}  
prototype ID\_Column : Database\_Column {  
    ColumnName \= "ID";  
    DataType \= "int";  
}  
prototype Name\_Column : Database\_Column {  
    ColumnName \= "Name";  
    DataType \= "varchar";  
}

**What’s Happening?**

* `Database_Table` defines a template for tables, with a name and column collection.  
* `Employees_Table` models the `Employees` table, linking to `ID_Column` and `Name_Column`.  
* **Graph View**: `Employees_Table` links to `"Employees"` and a collection of column nodes (`"ID"`, `"int"`, `"Name"`, `"varchar"`).  
* **Beyond Ontologies**: ProtoScript’s generic Prototypes model database schemas as easily as conceptual data, unlike OWL’s domain-specific ontologies.

**Cross-Domain Potential**:

* **Relationship**: The `DataType` (`"int"`) matches the C\# example’s `TypeName`, enabling type alignment across code and database.  
* **Transformation**: The table could be transformed into a C\# class or SQL CREATE statement.

### **Example 4: Natural Language Semantics**

**Scenario**: Model the sentence "I need to buy some covid-19 test kits".

prototype Need {  
    BaseObject Subject \= new BaseObject();  
    Action Object \= new Action();  
}  
prototype Action {  
    string Infinitive \= "";  
    BaseObject Object \= new BaseObject();  
}  
prototype COVID\_TestKit {  
    string Quantity \= "";  
}  
prototype Need\_BuyTestKits : Need {  
    Subject \= Person\_I;  
    Object \= BuyAction;  
}  
prototype Person\_I : BaseObject {  
    string Pronoun \= "I";  
}  
prototype BuyAction : Action {  
    Infinitive \= "ToBuy";  
    Object \= TestKit;  
}  
prototype TestKit : COVID\_TestKit {  
    Quantity \= "Some";  
}

**What’s Happening?**

* `Need_BuyTestKits` models the sentence, with literals `"I"`, `"ToBuy"`, and `"Some"` as NativeValuePrototypes.  
* **Graph View**: `Need_BuyTestKits` links to nodes for `"I"`, `"ToBuy"`, and `"Some"`, forming a semantic graph.  
* **Beyond Ontologies**: ProtoScript handles linguistic structures as naturally as code or data, unlike OWL’s focus on static concepts.

**Cross-Domain Potential**:

* **Relationship**: The `Object` (`TestKit`) could link to a database record for test kits, connecting language to data.  
* **Transformation**: The sentence could be transformed into a SQL query (e.g., `SELECT * FROM TestKits WHERE Quantity = 'Some'`).

## **Discovering Relationships and Transformations**

ProtoScript’s unified graph model enables powerful cross-domain interactions:

* **Relationships**: Prototypes share common properties (e.g., `"int"` in C\# and database examples), allowing discovery of type consistency or semantic links (e.g., `TestKit` in NLP and database).  
* **Transformations**: By mapping Prototypes, ProtoScript can transform a natural language request into a SQL query, a C\# variable into a database column, or a SQL query into a code snippet.  
  * **Example**: The NLP example’s `Need_BuyTestKits` could generate a SQL query by mapping `TestKit` to `Employees_Table`’s schema, or a C\# method to fetch test kits.

This flexibility surpasses traditional ontologies, which require separate models and mappings for each domain, often needing external tools for transformation.

### **Example: Cross-Domain Transformation**

**Scenario**: Transform the NLP sentence into a SQL query.

prototype QueryGenerator {  
    Need Need \= new Need();  
    function ToSQL() : SQL\_Select {  
        SQL\_Select query \= new SQL\_Select();  
        if (Need.Object.Object typeof COVID\_TestKit) {  
            query.Table.TableName \= "TestKits";  
            query.Columns \= \[Wildcard\_Expression\];  
        }  
        return query;  
    }  
}  
prototype Wildcard\_Expression : SQL\_Expression {  
    Value \= "\*";  
}

**What’s Happening?**

* `QueryGenerator` takes a `Need` Prototype (from the NLP example) and generates a `SQL_Select` Prototype.  
* The function checks if the `Need` involves a `COVID_TestKit`, setting the query’s table to `"TestKits"`.  
* **Graph View**: Links `Need_BuyTestKits` to a new `SQL_Select` node with `"TestKits"` and `"*"`.  
* **Result**: Produces `SELECT * FROM TestKits`.  
* **Beyond Ontologies**: ProtoScript’s graph unifies NLP and SQL, enabling seamless transformation without external mappings.

## **Internal Mechanics**

ProtoScript’s runtime manages Prototype creation:

* **Nodes**: Each Prototype (e.g., `Int_Declaration`, `Select_Prototypes`) is a graph node with a unique ID.  
* **Edges**: Properties create edges to other nodes or NativeValuePrototypes (e.g., `"int"`, `"Some"`).  
* **Instantiation**: `new` clones templates, and literals are translated to NativeValuePrototypes.  
* **Traversal**: Functions and operators traverse the graph to discover relationships or perform transformations.

## **Why This Matters**

ProtoScript’s ability to model any data type as Prototypes offers:

* **Universal Flexibility**: Handle C\#, SQL, databases, and NLP with one framework, unlike ontologies’ domain-specific models.  
* **Cross-Domain Insights**: Discover relationships (e.g., type alignment) and transformations (e.g., NLP to SQL) naturally.  
* **Developer Ease**: C\#-like syntax makes modeling intuitive across domains.  
* **Dynamic Evolution**: Adapt models without redefining schemas, surpassing static ontologies.

## **Moving Forward**

These examples demonstrate ProtoScript’s power to unify diverse domains in a graph, enabling cross-domain relationships and transformations that go beyond traditional ontologies. In the next section, we’ll explore the **Simpsons Example for Prototype Modeling**, applying these concepts to a fictional dataset to further illustrate real-world applications. You’re now ready to model and connect complex systems with ProtoScript\!

# **Simpsons Example for Prototype Modeling**

ProtoScript’s Prototypes excel at modeling real-world entities and relationships within a dynamic, graph-based ontology, offering a flexible alternative to traditional ontologies like OWL or RDF. This section uses the fictional dataset from *The Simpsons* to demonstrate how ProtoScript creates an ontology-like structure to represent characters, locations, and their interconnections. By modeling entities such as Homer, Marge, and the Simpsons’ house as Prototypes, we illustrate how ProtoScript’s unified graph framework captures complex relationships (e.g., family ties, locations) with ease, supports runtime adaptability, and enables reasoning through structural patterns. This example highlights ProtoScript’s ability to go beyond traditional ontologies, which often rely on static schemas and formal axioms, by providing a developer-friendly, dynamic approach to knowledge representation.

## **Why This Example Matters**

The *Simpsons* dataset is relatable and rich with relationships, making it an ideal case study for demonstrating ProtoScript’s ontology capabilities:

* **Real-World Modeling**: Characters (e.g., Homer, Marge) and locations (e.g., Springfield) form a network of relationships, mirroring real-world ontologies like geographic or social systems.  
* **Dynamic Ontology**: ProtoScript’s Prototypes allow runtime modifications (e.g., adding new relationships), unlike OWL’s rigid class hierarchies.  
* **Cross-Domain Potential**: The model can integrate with other domains (e.g., linking characters to a database or natural language queries), showcasing ProtoScript’s versatility.  
* **Reasoning and Relationships**: The graph enables discovery of relationships (e.g., family structures) and transformations (e.g., generating queries about characters).

### **Beyond Traditional Ontologies**

Traditional ontologies define static classes (e.g., `Person`, `Location`) and properties with formal axioms, requiring significant upfront design and external inference engines for reasoning. ProtoScript’s Prototypes offer several advantages:

1. **Dynamic Structure**: Prototypes can evolve at runtime, adding properties or relationships without redefining the ontology, unlike OWL’s static schemas.  
2. **Unified Representation**: The same `prototype` construct models characters, locations, or even abstract concepts, simplifying development compared to domain-specific ontology tools.  
3. **Lightweight Reasoning**: Structural generalization (e.g., finding common traits among characters) replaces heavy axiomatic logic, making reasoning more intuitive.  
4. **Developer-Friendly**: C\#-like syntax lowers the barrier for developers, unlike ontology editors like Protégé, which require specialized knowledge.

### **Analogy to Familiar Concepts**

For **C\# developers**:

* Prototypes are like classes that can represent people, places, or relationships in a graph, similar to an ORM mapping objects to a database but with dynamic, graph-based flexibility.  
* Think of this as building a social network model where objects (characters) link to others (locations, family) in a graph database.

For **JavaScript developers**:

* Prototypes resemble JavaScript objects in a graph, where each object (e.g., Homer) can inherit from multiple prototypes and link to others, like a JSON-based knowledge graph.

For **database developers**:

* The ontology is like a graph database where nodes (Prototypes) represent entities and edges (properties) define relationships, but with programmable behaviors via functions.

## **Modeling The Simpsons Ontology**

This example models key characters (Homer, Marge, Bart) and locations (Simpsons’ house, Springfield) from *The Simpsons*, demonstrating how ProtoScript creates a graph-based ontology. We’ll define Prototypes, establish relationships, and show how the model supports querying and reasoning.

### **Prototype Definitions**

Below is the ProtoScript code to model the *Simpsons* ontology, corrected for syntax accuracy based on your clarifications (e.g., using lowercase `string` for literals, direct literal initializers).

prototype Entity {  
    string Name \= "";  
}  
prototype Location : Entity {  
    string Address \= "";  
}  
prototype Person : Entity {  
    string Gender \= "";  
    Location Location \= new Location();  
    Collection ParentOf \= new Collection();  
    Person Spouse \= new Person();  
    int Age \= 0;  
}  
prototype SimpsonsHouse : Location {  
    Name \= "Simpsons House";  
    Address \= "742 Evergreen Terrace";  
}  
prototype Springfield : Location {  
    Name \= "Springfield";  
    Address \= "Unknown";  
}  
prototype Homer : Person {  
    Name \= "Homer Simpson";  
    Gender \= "Male";  
    Location \= SimpsonsHouse;  
    ParentOf \= \[Bart, Lisa, Maggie\];  
    Spouse \= Marge;  
    Age \= 39;  
}  
prototype Marge : Person {  
    Name \= "Marge Simpson";  
    Gender \= "Female";  
    Location \= SimpsonsHouse;  
    ParentOf \= \[Bart, Lisa, Maggie\];  
    Spouse \= Homer;  
    Age \= 36;  
}  
prototype Bart : Person {  
    Name \= "Bart Simpson";  
    Gender \= "Male";  
    Location \= SimpsonsHouse;  
    Age \= 10;  
}  
prototype Lisa : Person {  
    Name \= "Lisa Simpson";  
    Gender \= "Female";  
    Location \= SimpsonsHouse;  
    Age \= 8;  
}  
prototype Maggie : Person {  
    Name \= "Maggie Simpson";  
    Gender \= "Female";  
    Location \= SimpsonsHouse;  
    Age \= 1;  
}

**What’s Happening?**

* `Entity` is a base Prototype with a `Name` property, acting as a root for all entities.  
* `Location` inherits from `Entity`, adding an `Address` property.  
* `Person` inherits from `Entity`, defining properties for `Gender`, `Location`, `ParentOf`, `Spouse`, and `Age`, using lowercase types (`string`, `int`) with direct literals.  
* Specific instances (`Homer`, `Marge`, `Bart`, `SimpsonsHouse`) set literal values (e.g., `"Male"`, `39`), which the runtime translates to NativeValuePrototypes.  
* Relationships are established via properties (e.g., `Homer.Spouse = Marge`, `Homer.ParentOf = [Bart, Lisa, Maggie]`).  
* **Graph View**: Nodes for `Homer`, `Marge`, and `SimpsonsHouse` are linked via edges (`Spouse`, `Location`, `ParentOf`), forming a network of relationships.

**Syntax Corrections**:

* Used lowercase `string`, `bool`, `int` for primitive types, as per your clarification, with direct literals (e.g., `"Homer Simpson"`, `39`).  
* Avoided incorrect notations like `System.String[Value]` or uppercase types unless explicitly needed, aligning with examples like `string Name = "Buffalo"`.  
* Ensured `Collection` is used correctly for lists (e.g., `ParentOf`), matching document conventions.

### **Ontology Structure**

The *Simpsons* model forms a graph-based ontology:

* **Classes**: `Entity`, `Location`, `Person` act as conceptual classes, like OWL classes, but are dynamic Prototypes.  
* **Individuals**: `Homer`, `Marge`, `SimpsonsHouse` are instances, akin to RDF individuals, linked via properties.  
* **Properties**: `Spouse`, `ParentOf`, `Location` are edges, similar to OWL object properties, but support runtime additions and cycles (e.g., `Homer.Spouse ↔ Marge.Spouse`).  
* **Reasoning**: The graph enables structural queries (e.g., finding all parents) without formal axioms.

**Beyond Traditional Ontologies**:

* **Dynamic Evolution**: Unlike OWL, which requires schema updates to add a property like `Occupation`, ProtoScript can dynamically add it to `Person` at runtime.  
* **Unified Modeling**: The same `prototype` construct models characters and locations, unlike ontology tools needing separate class definitions.  
* **Ease of Use**: C\#-like syntax simplifies ontology creation compared to Protégé’s complex interface.  
* **Cross-Domain Integration**: The model can link to external data (e.g., a database of Springfield residents), enabling transformations.

### **Querying the Ontology**

ProtoScript’s graph model supports querying relationships using functions and operators:

prototype Person {  
    string Gender \= "";  
    Location Location \= new Location();  
    Collection ParentOf \= new Collection();  
    Person Spouse \= new Person();  
    int Age \= 0;  
    function IsParent() : bool {  
        return ParentOf.Count \> 0;  
    }  
    function LivesInSpringfield() : bool {  
        return Location \-\> Location { this.Name \== "Springfield" };  
    }  
}

**What’s Happening?**

* `IsParent` checks if a `Person` has children by counting `ParentOf` entries.  
* `LivesInSpringfield` uses the `->` operator to verify if the `Location`’s `Name` is `"Springfield"`.  
* **Usage**: `Homer.IsParent()` returns `true`, `Homer.LivesInSpringfield()` returns `true` (if `SimpsonsHouse` links to `Springfield`).  
* **Graph View**: `IsParent` traverses `ParentOf` edges, `LivesInSpringfield` follows `Location` to check `Name`.  
* **Beyond Ontologies**: These queries use structural traversal, not axiomatic reasoning, making them more intuitive and adaptable than OWL queries.

### **Discovering Relationships**

The graph enables discovery of relationships:

* **Family Structure**: `Homer.ParentOf` and `Marge.ParentOf` both link to `Bart`, `Lisa`, `Maggie`, revealing shared parenthood.  
* **Location-Based Grouping**: Querying `Person` nodes with `Location = SimpsonsHouse` identifies residents (Homer, Marge, Bart, Lisa, Maggie).  
* **Spousal Symmetry**: `Homer.Spouse = Marge` and `Marge.Spouse = Homer` form a bidirectional relationship, modeled naturally as a cycle.

**Example Function**:

prototype Person {  
    Collection ParentOf \= new Collection();  
    function GetChildrenNames() : Collection {  
        Collection names \= new Collection();  
        foreach (Person child in ParentOf) {  
            names.Add(child.Name);  
        }  
        return names;  
    }  
}

**What’s Happening?**

* `GetChildrenNames` collects `Name` properties from `ParentOf` nodes.  
* `Homer.GetChildrenNames()` returns a collection with `"Bart Simpson"`, `"Lisa Simpson"`, `"Maggie Simpson"`.  
* **Beyond Ontologies**: This dynamic query avoids OWL’s need for predefined SPARQL queries, using ProtoScript’s native graph traversal.

### **Cross-Domain Transformations**

The *Simpsons* ontology can integrate with other domains:

* **Database Integration**: Link `Person` to a database table `Residents` with columns `Name`, `Gender`, `Age`, mapping `Homer.Name` to a record.

**Natural Language**: Transform a query like "Who lives in Springfield?" into a ProtoScript function:  
prototype Query {  
    string Question \= "";  
    function ToPersonList() : Collection {  
        Collection people \= new Collection();  
        if (Question \== "Who lives in Springfield?") {  
            foreach (Person p in AllPersons) {  
                if (p.LivesInSpringfield()) {  
                    people.Add(p);  
                }  
            }  
        }  
        return people;  
    }  
}

*   
  * **What’s Happening?**: The function maps a natural language question to a list of `Person` nodes, leveraging the ontology.  
  * **Beyond Ontologies**: This transformation unifies NLP and graph querying, unlike OWL’s separate processing pipelines.

## **Internal Mechanics**

The *Simpsons* ontology operates within ProtoScript’s graph-based runtime:

* **Nodes**: Prototypes (`Homer`, `SimpsonsHouse`) are nodes with unique IDs.  
* **Edges**: Properties (`Spouse`, `ParentOf`, `Location`) create edges, including cycles (e.g., `Homer ↔ Marge`).  
* **Runtime**: Translates literals (e.g., `"Male"`, `39`) to NativeValuePrototypes, manages instantiation, and supports traversal.  
* **Querying**: Functions and operators (`->`, `typeof`) traverse the graph to evaluate relationships.

## **Why This Example Matters**

The *Simpsons* ontology showcases ProtoScript’s strengths:

* **Dynamic Modeling**: Prototypes capture complex relationships (family, location) with runtime flexibility, surpassing OWL’s static schemas.  
* **Unified Framework**: The same constructs model characters and locations, simplifying development compared to ontology-specific tools.  
* **Relationship Discovery**: The graph reveals family structures and location-based groupings naturally.  
* **Cross-Domain Potential**: The model supports integration with databases or NLP, enabling transformations like query generation.  
* **Developer-Friendly**: C\#-like syntax makes ontology creation accessible to developers, not just ontology experts.

## **Moving Forward**

This *Simpsons* example demonstrates how ProtoScript’s Prototypes create a dynamic, graph-based ontology that models real-world entities with ease and flexibility. In the next section, we’ll explore **Relationships in ProtoScript**, diving into the taxonomy of relationships—from simple associations to complex computed links—that power the ontology’s connectivity. You’re now equipped to model rich, interconnected systems with ProtoScript\!

# **Relationships in ProtoScript**

This section opens the Reasoning layer, focusing on the operators and traversal patterns that act on the representation graphs introduced earlier.

Relationships in ProtoScript define how Prototypes connect within the graph-based Buffaly system, forming the backbone of its dynamic, ontology-like structure. Unlike traditional ontologies, which rely on rigid class hierarchies and formal axioms, ProtoScript’s relationships are flexible, supporting a spectrum of connections—from simple, unlabeled links to complex, computed dependencies. This section introduces the taxonomy of relationships, detailing seven types: associative relationships, associations, cyclical relationships, type relationships (`typeof`), labeled properties, bidirectional relationships, and computed relationships. Each type builds on the previous, enabling developers to model real-world complexities with ease. Through examples rooted in familiar domains, we’ll explore how these relationships work, their syntax, and their advantages over traditional ontology frameworks, drawing analogies to C\# and database concepts.

## **Why Relationships Matter**

Relationships are the edges in ProtoScript’s graph, linking Prototype nodes to represent knowledge, behavior, and semantics. They enable:

* **Complex Modeling**: Capture intricate real-world connections, like family ties or database schemas.  
* **Dynamic Adaptability**: Add or modify relationships at runtime, unlike static ontologies.  
* **Cross-Domain Integration**: Connect code, data, and language within a unified graph.  
* **Reasoning**: Discover patterns and transform data by traversing relationships.

**Beyond Traditional Ontologies**:

* **Flexibility**: ProtoScript’s relationships evolve without redefining schemas, unlike OWL’s fixed properties.  
* **Unified Framework**: A single graph model handles diverse relationship types, simplifying development compared to ontology-specific tools.  
* **Lightweight Reasoning**: Structural traversal replaces heavy axiomatic logic, making reasoning intuitive.  
* **Developer-Friendly**: C\#-like syntax lowers the barrier for modeling relationships.

### **Analogy to Familiar Concepts**

For **C\# developers**:

* Relationships are like class fields, properties, or navigation properties in an ORM, but organized in a graph with dynamic, multi-faceted connections.  
* Think of ProtoScript as a graph-based ORM that links objects (Prototypes) in a flexible network, not just a database schema.

For **JavaScript developers**:

* Relationships resemble object properties in a JSON graph, where each Prototype links to others, supporting dynamic updates and complex traversals.

For **database developers**:

* Relationships are edges in a graph database, connecting nodes (Prototypes) to model entities and their interactions, with programmable logic.

## **Taxonomy of Relationships**

ProtoScript’s relationships form a taxonomy, progressing from simple to complex. Below, we detail each type, its syntax, mechanics, and examples, using a consistent *Simpsons* dataset for clarity.

### **1\. Associative Relationships**

**Purpose**: Represent the simplest, unlabeled, unweighted connections between Prototypes, forming loose links without semantics.

**Syntax**: Implicitly defined by referencing Prototypes in collections or properties.

**Details**:

* No explicit weight or label, just a basic edge in the graph.  
* Used for preliminary connections before adding meaning.  
* **C\# Analogy**: Like a `List<object>` holding references without specific roles.

**Example**:

prototype Entity {  
    string Name \= "";  
}  
prototype Group {  
    Collection Members \= new Collection();  
}  
prototype Springfield\_Group : Group {  
    Members \= \[Homer, Marge\];  
}  
prototype Homer : Entity {  
    Name \= "Homer Simpson";  
}  
prototype Marge : Entity {  
    Name \= "Marge Simpson";  
}

**What’s Happening?**

* `Springfield_Group.Members` links to `Homer` and `Marge` without specifying why.  
* **Graph View**: `Springfield_Group` has edges to `Homer` and `Marge` nodes.  
* **Beyond Ontologies**: Unlike OWL’s need for defined properties, associative relationships allow quick, informal links.

### **2\. Associations**

**Purpose**: Add weight and bidirectionality to connections, forming explicit, stored relationships with minimal semantics.

**Syntax**:

prototype1.BidirectionalAssociate(prototype2);

**Details**:

* Creates mutual edges with a numeric weight (default 1), incremented by repeated associations.  
* Stored as extensional facts in the graph.  
* **C\# Analogy**: Like a weighted adjacency list in a graph data structure.

**Example**:

prototype Food {  
    string Name \= "";  
}  
prototype Turkey\_Food : Food {  
    Name \= "Turkey";  
}  
prototype Gravy\_Food : Food {  
    Name \= "Gravy";  
}  
Turkey\_Food.BidirectionalAssociate(Gravy\_Food);

**What’s Happening?**

* `Turkey_Food` and `Gravy_Food` are linked bidirectionally, with a weight of 1\.  
* **Graph View**: Edges `Turkey_Food ↔ Gravy_Food` with weight metadata.  
* **Beyond Ontologies**: Associations are simpler than OWL’s object properties, enabling rapid relationship setup.

### **3\. Cyclical Relationships**

**Purpose**: Enable bidirectional, cyclic property references, modeling mutual dependencies (e.g., a state and its cities).

**Syntax**:

prototype Type1 {  
    Type2 Property \= new Type2();  
}  
prototype Type2 {  
    Collection Type1s \= new Collection();  
}

**Details**:

* Properties create loops (e.g., `City.State → State`, `State.Cities → City`).  
* Managed by the runtime to prevent infinite traversal.  
* **C\# Analogy**: Like circular object references (e.g., `class City { State State; }`, `class State { List<City> Cities; }`).

**Example**:

prototype State {  
    string Name \= "";  
    Collection Cities \= new Collection();  
}  
prototype City {  
    string Name \= "";  
    State State \= new State();  
}  
prototype NewYork\_State : State {  
    Name \= "New York";  
}  
prototype NewYork\_City : City {  
    Name \= "New York City";  
}  
NewYork\_City.State \= NewYork\_State;  
NewYork\_State.Cities.Add(NewYork\_City);

**What’s Happening?**

* `NewYork_City.State` links to `NewYork_State`, and `NewYork_State.Cities` links back, forming a cycle.  
* **Graph View**: `NewYork_City ↔ NewYork_State` via `State` and `Cities` edges.  
* **Beyond Ontologies**: Cycles are natural in ProtoScript, unlike OWL’s preference for acyclic hierarchies.

### **4\. Type Relationships (`typeof`)**

**Purpose**: Define directional inheritance relationships, checked by the `typeof` operator, to establish a Prototype’s place in the graph.

**Syntax**:

prototype Child : Parent;  
if (prototype typeof Parent) { ... }

**Details**:

* Inheritance creates `isa` edges, forming a DAG for type relationships.  
* `typeof` checks direct or transitive inheritance.  
* **C\# Analogy**: Like `is` or inheritance in C\# (e.g., `class Child : Parent`).

**Example**:

prototype Person {  
    string Name \= "";  
}  
prototype Homer : Person {  
    Name \= "Homer Simpson";  
}  
function IsPerson(Prototype proto) : bool {  
    return proto typeof Person;  
}

**What’s Happening?**

* `Homer isa Person`, and `IsPerson(Homer)` returns `true`.  
* **Graph View**: `Homer` has an `isa` edge to `Person`.  
* **Beyond Ontologies**: `typeof` enables dynamic type checks, simpler than OWL’s class assertions.

### **5\. Labeled Properties**

**Purpose**: Represent specific, named attributes linking Prototypes, storing extensional relationships.

**Syntax**:

Type Name \= DefaultValue;

**Details**:

* Properties are named edges to other Prototypes or values, like database foreign keys.  
* **C\# Analogy**: Like class fields or navigation properties in Entity Framework.

**Example**:

prototype Person {  
    string Name \= "";  
    Location Location \= new Location();  
}  
prototype Location {  
    string Name \= "";  
}  
prototype Homer : Person {  
    Name \= "Homer Simpson";  
    Location \= SimpsonsHouse;  
}  
prototype SimpsonsHouse : Location {  
    Name \= "Simpsons House";  
}

**What’s Happening?**

* `Homer.Location` links to `SimpsonsHouse`.  
* **Graph View**: `Homer → SimpsonsHouse` via the `Location` edge.  
* **Beyond Ontologies**: Labeled properties are intuitive, unlike OWL’s complex property declarations.

### **6\. Bidirectional Relationships**

**Purpose**: Ensure mutual, synchronized links between Prototypes, maintaining consistency.

**Syntax**: Defined via paired properties or runtime synchronization.

**Details**:

* Properties are set to reflect mutual relationships (e.g., `Spouse` links).  
* **C\# Analogy**: Like two-way navigation properties in an ORM, with manual consistency.

**Example**:

prototype Person {  
    string Name \= "";  
    Person Spouse \= new Person();  
}  
prototype Homer : Person {  
    Name \= "Homer Simpson";  
    Spouse \= Marge;  
}  
prototype Marge : Person {  
    Name \= "Marge Simpson";  
    Spouse \= Homer;  
}

**What’s Happening?**

* `Homer.Spouse = Marge` and `Marge.Spouse = Homer` create mutual links.  
* **Graph View**: `Homer ↔ Marge` via `Spouse` edges.  
* **Beyond Ontologies**: Bidirectional relationships are explicit and dynamic, unlike OWL’s need for inverse properties.

### **7\. Computed Relationships**

**Purpose**: Define dynamic, intensional relationships via functions, computed at runtime.

**Syntax**:

function Name(Parameters) : ReturnType {  
    // Compute relationship  
}

**Details**:

* Functions traverse the graph to compute relationships, like dynamic queries.  
* **C\# Analogy**: Like computed properties or LINQ queries.

**Example**:

prototype Person {  
    string Name \= "";  
    Collection ParentOf \= new Collection();  
    function IsParent() : bool {  
        return ParentOf.Count \> 0;  
    }  
}  
prototype Homer : Person {  
    Name \= "Homer Simpson";  
    ParentOf \= \[Bart, Lisa, Maggie\];  
}  
prototype Bart : Person {  
    Name \= "Bart Simpson";  
}

**What’s Happening?**

* `IsParent` computes if a `Person` has children.  
* `Homer.IsParent()` returns `true`.  
* **Graph View**: Traverses `ParentOf` edges to count nodes.  
* **Beyond Ontologies**: Computed relationships enable flexible reasoning without OWL’s axioms.

## **Cross-Domain Relationships**

ProtoScript’s unified graph allows relationships to span domains:

* **Example**: Link a `Person`’s `Location` to a database table’s `Address` column, or map a computed relationship (`IsParent`) to a natural language query ("Who are the parents?").  
* **Transformation**: Convert a bidirectional `Spouse` relationship to a SQL JOIN query.

**Example**:

prototype Query {  
    string Question \= "";  
    function ToPersonList() : Collection {  
        Collection parents \= new Collection();  
        if (Question \== "Who are the parents?") {  
            foreach (Person p in AllPersons) {  
                if (p.IsParent()) {  
                    parents.Add(p);  
                }  
            }  
        }  
        return parents;  
    }  
}

**What’s Happening?**

* Maps a natural language question to a list of `Person` nodes using `IsParent`.  
* **Beyond Ontologies**: Unifies NLP and graph querying, unlike OWL’s separate pipelines.

## **Internal Mechanics**

Relationships are managed by ProtoScript’s runtime:

* **Nodes**: Prototypes are nodes with unique IDs.  
* **Edges**: Relationships are edges (inheritance, properties, computed links).  
* **Runtime**: Handles traversal, cycle management, and synchronization.  
* **Storage**: Extensional relationships (e.g., properties) are stored; intensional ones (e.g., functions) are computed.

## **Why Relationships Are Essential**

ProtoScript’s relationship taxonomy provides:

* **Versatility**: From simple links to complex computations, covering diverse use cases.  
* **Dynamic Modeling**: Runtime adaptability surpasses static ontologies.  
* **Cross-Domain Power**: Enables relationships and transformations across code, data, and language.  
* **Intuitive Reasoning**: Structural traversal simplifies ontology queries.

## **Moving Forward**

This taxonomy of relationships showcases ProtoScript’s ability to model complex, dynamic connections in a graph-based ontology. In the next section, we’ll explore **Shadows and Least General Generalization (LGG)**, diving into how ProtoScript creates ad-hoc subtypes to generalize and categorize Prototypes, further enhancing its reasoning capabilities. You’re now ready to build interconnected, flexible systems with ProtoScript\!

# **Shadows and Least General Generalization (LGG)**

This section begins the Learning layer, following Shadows through Paths, Hidden Context Prototypes, clustering feedback, and related transforms that adapt the system over time.

Shadows are a cornerstone of ProtoScript’s graph-based ontology, serving as the primary mechanism for learning and categorization. Through **Least General Generalization (LGG)**, Shadows create ad-hoc subtypes that generalize common structures across Prototypes, enabling unsupervised learning without the need for gradient descent. Shadows are ProtoScript’s primary structural generalization mechanism: `Compare`/LGG constructs explicit Prototypes from bounded graph comparisons, and feedback-driven pruning keeps those generalizations focused. This section carefully unpacks Shadows and LGG, explaining their purpose, mechanics, and significance with clear analogies, step-by-step examples, and practical applications. We’ll focus on how Shadows generalize Prototypes, categorize instances, and power ProtoScript’s dynamic reasoning, ensuring clarity for developers familiar with C\# or JavaScript.

## **Why Shadows Are Foundational**

Shadows are the heart of ProtoScript’s learning capability, allowing the system to:

* **Generalize Patterns**: Identify shared structures across Prototypes (e.g., finding that two variable declarations share a common type).  
* **Create Subtypes Dynamically**: Form ad-hoc categories (e.g., “initialized integer variables”) without predefined schemas.  
* **Enable Unsupervised Learning**: Discover patterns in data without labeled training sets, unlike supervised machine learning.  
* **Scale Efficiently**: Operate on graph structures with bounded comparison sets and pruning policies rather than open-ended numeric optimization.

**Learning Role in ProtoScript**:

* **Structural Learning Path**: Shadows, via LGG, provide the runtime pathway for learning new categories and relationships by materializing shared structure as explicit Prototypes.  
* **Graph-Focused Alternative**: This learning process is symbolic: it derives graph artifacts (Shadows, Paths) from structural comparisons instead of adjusting numeric weights, making it suited to sparse or evolving ontology data.

### **Beyond Traditional Ontologies**

Traditional ontologies (e.g., OWL, RDF) rely on static class definitions and formal axioms, with learning limited to predefined rules or external inference engines. ProtoScript’s Shadows offer:

1. **Dynamic Categorization**: Shadows create subtypes on-the-fly by comparing instances, unlike OWL’s fixed hierarchies.  
2. **Unsupervised Learning**: No need for labeled data, unlike supervised ontology learning tools.  
3. **Structural Reasoning**: LGG generalizes based on graph structure, not statistical patterns, ensuring interpretability.  
4. **Scalability**: Uses pairwise comparisons selected through indexing or shortlists, with pruning to discard low-utility Shadows, avoiding unbounded pair counts.

### **Analogy to Familiar Concepts**

For **C\# developers**:

* Shadows are like finding the common interface or base class between two objects by comparing their properties, but done dynamically at runtime without predefined types.  
* Think of LGG as a LINQ query that extracts the shared structure of two objects, creating a new “type” on the fly.

For **JavaScript developers**:

* Shadows resemble finding the common properties of two JSON objects to form a prototype, but organized in a graph for reasoning.  
* LGG is like merging objects to keep only shared keys and values, automatically generating a reusable template.

For **database developers**:

* Shadows are like discovering a common schema for two database records by comparing their fields, creating a new table definition.  
* LGG is akin to a graph query that finds the intersection of two node structures.

## **What Are Shadows?**

A **Shadow** is a Prototype generated by applying **Least General Generalization (LGG)** to two or more Prototypes, capturing their most specific common structure. It acts as an ad-hoc subtype, representing the shared properties and relationships of the input Prototypes while omitting their differences. Shadows enable ProtoScript to:

* **Categorize**: Group Prototypes under a common subtype (e.g., “initialized variables”).  
* **Learn**: Discover patterns without supervision by generalizing instances.  
* **Reason**: Query and transform data based on generalized structures.

**LGG Defined**: LGG finds the least general (most specific) Prototype that subsumes two or more input Prototypes, retaining only their common properties, types, and relationships. It’s the opposite of finding the most specific common instance; instead, it creates the most specific shared abstraction.

### **Key Characteristics**

1. **Structural Generalization**

   * LGG compares Prototype graphs, keeping shared properties and generalizing differences (e.g., different variable names become a generic `string`).  
   * **Example**: `int i = 0` and `int j = -1` generalize to `int _ = _`.  
2. **Ad-Hoc Subtypes**

   * Shadows form temporary or persistent subtypes, categorizing Prototypes dynamically.  
   * **Example**: A Shadow for “parents” groups `Homer` and `Marge`.  
3. **Unsupervised Learning**

   * Shadows learn by comparing instances, requiring no labeled data.  
   * **Example**: Generalizing two SQL queries to a common query structure.  
4. **Graph-Based**

   * Operates on the graph’s nodes and edges, ensuring scalability and interpretability.  
   * **Example**: Traversing properties to find commonalities.

## **How Shadows Work**

Shadows are created using the **compare operator**, which applies LGG to two Prototypes. The process involves:

1. **Comparison**: Analyze properties, types, and relationships of the input Prototypes.  
2. **Retention**: Keep identical properties and values (e.g., same type).  
3. **Generalization**: Replace differing values with their common type or a wildcard (e.g., `_` for names).  
4. **Output**: Produce a new Prototype (the Shadow) representing the shared structure.

**Syntax** (Conceptual, executed by runtime):

Compare(prototype1, prototype2) // Returns a Shadow Prototype

**C\# Analogy**: Like a method that compares two objects’ fields and returns a new object with only their common properties, but operating on graph nodes.

### **LGG Rules**

1. **Exact Match**: Identical properties or values are retained (e.g., `TypeName = "int"` in both Prototypes).  
2. **Type Generalization**: Differing values of the same type generalize to the type (e.g., `"i"` and `"j"` become `string`).  
3. **Structural Generalization**: Differing substructures generalize to their common parent (e.g., different expressions become `Expression`).  
4. **Omission**: Properties present in one but not the other are excluded unless structurally required.  
5. **Annotations**: Comparison metadata (e.g., `Compare.Exact`, `Compare.StartsWith`) may annotate the Shadow to indicate match precision.

### **Example 1: Generalizing C\# Variable Declarations**

**Scenario**: Create a Shadow for `int i = 0` and `int j = -1`.

**Input Prototypes**:

prototype CSharp\_VariableDeclaration {  
    CSharp\_Type Type \= new CSharp\_Type();  
    string VariableName \= "";  
    CSharp\_Expression Initializer \= new CSharp\_Expression();  
}  
prototype CSharp\_Type {  
    string TypeName \= "";  
    bool IsNullable \= false;  
}  
prototype CSharp\_Expression {  
    string Value \= "";  
}  
prototype Int\_Declaration\_I : CSharp\_VariableDeclaration {  
    Type.TypeName \= "int";  
    VariableName \= "i";  
    Initializer \= IntegerLiteral\_0;  
}  
prototype IntegerLiteral\_0 : CSharp\_Expression {  
    Value \= "0";  
}  
prototype Int\_Declaration\_J : CSharp\_VariableDeclaration {  
    Type.TypeName \= "int";  
    VariableName \= "j";  
    Initializer \= UnaryExpression\_Minus1;  
}  
prototype UnaryExpression\_Minus1 : CSharp\_Expression {  
    string Operator \= "-";  
    string Value \= "1";  
}

**Shadow Creation**:

* **Comparison**:  
  * `Type.TypeName`: Both `"int"` (exact match, retained).  
  * `IsNullable`: Both `false` (exact match, retained).  
  * `VariableName`: `"i"` vs. `"j"` (differ, generalize to `string`).  
  * `Initializer`: `IntegerLiteral_0` vs. `UnaryExpression_Minus1` (differ, generalize to `CSharp_Expression`).  
* **Resulting Shadow**:

prototype InitializedIntVariable : CSharp\_VariableDeclaration {  
    Type.TypeName \= "int";  
    Type.IsNullable \= false;  
    VariableName \= "";  
    Initializer \= new CSharp\_Expression();  
}

**C\# Visualization**: `int _ = _;`

**What’s Happening?**

* The Shadow captures the common structure: both are non-nullable `int` declarations with an initializer.  
* `VariableName` generalizes to an empty `string` (a wildcard), and `Initializer` to `CSharp_Expression`.  
* **Graph View**: The Shadow is a node with edges to `"int"`, `false`, and a generic `CSharp_Expression` node.  
* **Learning Outcome**: The Shadow defines a new subtype, “initialized integer variables,” categorizing both inputs.

**Beyond Ontologies**: Unlike OWL, which requires predefined classes, Shadows dynamically learn this subtype from instance comparisons, enabling unsupervised categorization.

### **Example 2: Generalizing Simpsons Characters**

**Scenario**: Create a Shadow for `Homer` and `Marge` to identify shared traits.

**Input Prototypes** (from Simpsons example):

prototype Person {  
    string Name \= "";  
    string Gender \= "";  
    Location Location \= new Location();  
    Collection ParentOf \= new Collection();  
    Person Spouse \= new Person();  
    int Age \= 0;  
}  
prototype Location {  
    string Name \= "";  
}  
prototype SimpsonsHouse : Location {  
    Name \= "Simpsons House";  
}  
prototype Homer : Person {  
    Name \= "Homer Simpson";  
    Gender \= "Male";  
    Location \= SimpsonsHouse;  
    ParentOf \= \[Bart, Lisa, Maggie\];  
    Spouse \= Marge;  
    Age \= 39;  
}  
prototype Marge : Person {  
    Name \= "Marge Simpson";  
    Gender \= "Female";  
    Location \= SimpsonsHouse;  
    ParentOf \= \[Bart, Lisa, Maggie\];  
    Spouse \= Homer;  
    Age \= 36;  
}

**Shadow Creation**:

* **Comparison**:  
  * `Name`: `"Homer Simpson"` vs. `"Marge Simpson"` (differ, generalize to `string`).  
  * `Gender`: `"Male"` vs. `"Female"` (differ, generalize to `string`).  
  * `Location`: Both `SimpsonsHouse` (exact match, retained).  
  * `ParentOf`: Both `[Bart, Lisa, Maggie]` (exact match, retained).  
  * `Spouse`: `Marge` vs. `Homer` (differ, generalize to `Person`).  
  * `Age`: `39` vs. `36` (differ, generalize to `int`).  
* **Resulting Shadow**:

prototype SimpsonsHouseParent : Person {  
    Name \= "";  
    Gender \= "";  
    Location \= SimpsonsHouse;  
    ParentOf \= \[Bart, Lisa, Maggie\];  
    Spouse \= new Person();  
    Age \= 0;  
}

**What’s Happening?**

* The Shadow defines a subtype for “parents living in the Simpsons’ house with children Bart, Lisa, and Maggie.”  
* Differing properties (`Name`, `Gender`, `Age`) generalize to their types; `Spouse` to a generic `Person`.  
* **Graph View**: The Shadow links to `SimpsonsHouse` and `[Bart, Lisa, Maggie]` nodes, with placeholder edges for `Name`, `Gender`, `Spouse`, and `Age`.  
* **Learning Outcome**: Categorizes `Homer` and `Marge` as instances of this subtype, learned without supervision.

**Beyond Ontologies**: Shadows enable ProtoScript to learn family structures dynamically, unlike OWL’s need for predefined family classes.

### **Example 3: Categorizing with Shadows**

**Scenario**: Use the Shadow to categorize a new Prototype, `Ned`.

**New Prototype**:

prototype Ned : Person {  
    Name \= "Ned Flanders";  
    Gender \= "Male";  
    Location \= FlandersHouse;  
    ParentOf \= \[Rod, Todd\];  
    Spouse \= Maude;  
    Age \= 40;  
}  
prototype FlandersHouse : Location {  
    Name \= "Flanders House";  
}

**Categorization**:

function IsSimpsonsHouseParent(Person person) : bool {  
    return person \-\> SimpsonsHouseParent {  
        this.Location.Name \== "Simpsons House" &&  
        this.ParentOf.Count \== 3  
    };  
}

**What’s Happening?**

* `IsSimpsonsHouseParent` uses the `SimpsonsHouseParent` Shadow to check if `Ned` fits the subtype.  
* `Ned` fails because `Location = FlandersHouse`, not `SimpsonsHouse`, and `ParentOf` has two children, not three.  
* **Graph View**: The `->` operator traverses `Ned`’s properties, comparing them to the Shadow’s structure.  
* **Learning Outcome**: The Shadow categorizes `Homer` and `Marge` but excludes `Ned`, demonstrating learned discrimination.

**Beyond Ontologies**: This unsupervised categorization is more flexible than OWL’s static class membership, adapting to new instances dynamically.

## **Mechanics of Shadows**

Shadows are generated by the ProtoScript runtime:

1. **Comparison Operator**: `Compare(prototype1, prototype2)` triggers LGG, analyzing graph structures.  
2. **Graph Traversal**: Examines nodes, properties, and edges, applying LGG rules.  
3. **Node Creation**: Produces a new Prototype node (the Shadow) with generalized properties.  
4. **Categorization**: The Shadow’s structure defines a subtype, tested via the `->` operator.

**Scalability**:

* Naïvely, LGG could require many pairwise comparisons; runtime stays bounded by drawing candidates from indexed shortlists and feedback-ranked matches.
* Pruning (e.g., removing trivial Shadows) and clustering (grouping similar Shadows) manage complexity using configurable feedback thresholds.  
* LGG is deterministic structural comparison that produces graph artifacts, not iterative numeric optimization.

**Non-Supervised Learning**:

* Shadows learn by structural similarity, not labeled data, making them ideal for sparse or evolving ontologies.  
* **Example**: Generalizing new characters without predefined categories.

### **Scalability levers (implementation-dependent)**

Shadows rely on explicit controls to stay tractable: candidate Prototypes come from indexes or shortlists rather than full graph scans. LGG traversals honor hop limits set by transform templates, bounding how far comparisons spread. Feedback scores drive pruning or merging of low-value Shadows and can split broad ones when precision drops. Hidden Context Prototypes store deltas instead of full copies, reducing traversal and storage cost during learning. Clustering groups similar Shadows so follow-up comparisons reuse prior matches. Caching materialized graph fragments is optional and can keep frequently used Shadows or Paths ready for transforms. These levers are implementation-dependent; operators choose thresholds to balance fidelity and cost. The learning runtime uses the same controls when applying Paths and Subtypes so downstream transforms reuse the bounded candidate sets.

## **Why Shadows Support Learning**

Shadows anchor the learning pipeline in ProtoScript’s ontology, offering:

* **Unsupervised Learning**: Discover subtypes without training data, unlike supervised ontology tools.  
* **Scalability Controls**: Candidate shortlisting and pruning keep pairwise LGG comparisons focused on promising structures.  
* **Interpretability**: Shadows are explicit Prototypes, traceable via graph paths, unlike neural networks’ black boxes.  
* **Dynamic Reasoning**: Ad-hoc subtypes enable flexible querying and transformation.

**Structural Contrast with Gradient Descent**:

* Gradient descent updates numeric weights; Shadows rely on explicit structural comparisons and feedback scores, which suits sparse knowledge graphs.  
* Shadows use structural LGG, learning from few examples with deterministic results, ideal for knowledge representation.

### **Example 4: Cross-Domain Generalization**

**Scenario**: Generalize a C\# variable and a database column.

**Input Prototypes**:

prototype Database\_Column {  
    string ColumnName \= "";  
    string DataType \= "";  
}  
prototype ID\_Column : Database\_Column {  
    ColumnName \= "ID";  
    DataType \= "int";  
}

**Shadow Creation** (with `Int_Declaration_I` from Example 1):

* **Comparison**:  
  * `TypeName` (`"int"`) vs. `DataType` (`"int"`): Exact match, retained as `string`.  
  * `VariableName` (`"i"`) vs. `ColumnName` (`"ID"`): Differ, generalize to `string`.  
  * Other properties (e.g., `Initializer`, `IsNullable`): Absent in one, omitted.  
* **Resulting Shadow**:

prototype IntDataElement {  
    string Name \= "";  
    string Type \= "int";  
}

**What’s Happening?**

* The Shadow defines a subtype for “integer data elements” (e.g., variables or columns).  
* **Graph View**: Links to `"int"` and a generic `string` for `Name`.  
* **Learning Outcome**: Categorizes `Int_Declaration_I` and `ID_Column`, revealing cross-domain type consistency.  
* **Beyond Ontologies**: Unifies code and database domains, unlike OWL’s separate ontologies.

## **Moving Forward**

Shadows and LGG are ProtoScript’s core learning mechanism, enabling unsupervised categorization through indexed pairwise comparisons and pruning-based generalization controls.

# **Prototype Paths and Parameterization**

In ProtoScript, **Prototype Paths** and **Parameterization** extend the power of Shadows by identifying how individual Prototypes diverge from their generalized structures, enabling precise categorization, reasoning, and transformation within the graph-based ontology. Building on Shadows’ ability to create ad-hoc subtypes through Least General Generalization (LGG), Paths isolate the specific subgraphs where a Prototype differs from a Shadow, marking these differences with a `Compare.Entity` indicator. This process, known as Parameterization, refines ProtoScript’s unsupervised learning, allowing the system to not only generalize patterns but also pinpoint unique characteristics of individual instances. This section explains Prototype Paths and Parameterization with clarity, using step-by-step examples and analogies to familiar programming concepts, ensuring developers understand their critical role in ProtoScript’s dynamic, scalable ontology framework.

## **Why Prototype Paths Are Critical**

Prototype Paths and Parameterization are essential for:

* **Refining Categorization**: Identify exactly how a Prototype fits or deviates from a Shadow’s subtype, enabling fine-grained classification.  
* **Enabling Transformations**: Isolate specific properties for mapping across domains (e.g., transforming a C\# variable to a database column).  
* **Enhancing Reasoning**: Provide a detailed view of instance-specific subgraphs, supporting precise queries and pattern discovery.  
* **Supporting Unsupervised Learning**: Complement Shadows by extracting instance-specific details, completing the learning cycle without labeled data.

**Significance in ProtoScript’s Ontology**:

* **Learning Refinement**: Shadows generalize Prototypes into subtypes; Paths specify how each instance conforms to or diverges from these subtypes, making ProtoScript’s learning mechanism both broad and precise.  
* **Scalable and Interpretable**: Paths operate on explicit graph traversals with bounded hop counts and cached comparisons, keeping the analysis transparent.  
* **Cross-Domain Power**: By isolating divergent properties, Paths enable transformations between domains (e.g., code to natural language), leveraging the ontology’s unified graph.

### **Beyond Traditional Ontologies**

Traditional ontologies (e.g., OWL, RDF) use static property assertions and axioms, with limited ability to dynamically analyze instance differences. ProtoScript’s Prototype Paths offer:

1. **Dynamic Analysis**: Paths identify instance-specific subgraphs at runtime, unlike OWL’s fixed property definitions.  
2. **Unsupervised Precision**: No labeled data is needed to pinpoint differences, unlike supervised ontology tools.  
3. **Graph-Centric Reasoning**: Paths traverse the graph to isolate divergences, ensuring interpretability.  
4. **Transformation Enablement**: Paths provide the data needed to map Prototypes across domains, surpassing OWL’s domain-specific mappings.

### **Analogy to Familiar Concepts**

For **C\# developers**:

* Paths are like diffing two objects to find their unique fields, with the Shadow as the common base. Parameterization is like extracting those differences into a reusable template.  
* Think of Paths as a LINQ query that selects properties not covered by a base interface, highlighting what makes an object unique.

For **JavaScript developers**:

* Paths resemble extracting the unique keys of an object compared to a shared prototype, with Parameterization creating a map of those differences.  
* LGG is like merging objects; Paths are like listing the properties that didn’t merge.

For **database developers**:

* Paths are like identifying the fields in a record that differ from a common schema, with Parameterization marking those fields for further processing.  
* Think of Paths as a graph query that returns the subgraph unique to a node.

## **What Are Prototype Paths and Parameterization?**

**Prototype Paths** are one-dimensional navigations through a Prototype’s graph, identifying the properties or subgraphs where it diverges from a Shadow’s generalized structure. **Parameterization** is the process of using a Shadow to categorize a Prototype and extracting these divergent subgraphs as Paths, marked by a `Compare.Entity` node to indicate the point of mismatch. Together, they:

* **Categorize**: Confirm if a Prototype fits a Shadow’s subtype and highlight its unique features.  
* **Isolate Differences**: Extract subgraphs (e.g., a specific variable name) that distinguish the Prototype.  
* **Enable Transformation**: Provide the data needed to map or modify Prototypes (e.g., changing a property value).

### **How Parameterization Works**

Parameterization involves:

1. **Categorization**: Test if a Prototype satisfies a Shadow’s subtype using the `->` operator, ensuring it matches the generalized structure.  
2. **Path Extraction**: Identify properties where the Prototype diverges from the Shadow, tracing a path to the mismatched subgraph.  
3. **Marking**: Use `Compare.Entity` to flag the root of each divergent subgraph.  
4. **Output**: Produce one or more Paths, each representing a specific difference as a subgraph.

**Syntax** (Conceptual, executed by runtime):

Parameterize(prototype, shadow) // Returns a set of Prototype Paths

**C\# Analogy**: Like comparing an object to a base class and returning a dictionary of properties that differ, but operating on graph nodes and edges.

## **Mechanics of Prototype Paths**

The runtime generates Paths by:

1. **Matching**: Align the Prototype’s graph with the Shadow’s, confirming shared properties (e.g., same type).  
2. **Divergence Detection**: Identify properties or subgraphs that differ (e.g., unique variable name).  
3. **Path Construction**: Trace from the Prototype’s root to the divergent node, marking it with `Compare.Entity`.  
4. **Subgraph Extraction**: Return the divergent subgraph as a Path, preserving its structure.

**Key Indicator**: `Compare.Entity` is a special Prototype that marks the point where the Shadow’s generalization stops, pointing to the specific subgraph (e.g., a unique value or structure).

### **Example 1: Parameterizing C\# Variable Declarations**

**Scenario**: Parameterize `int i = 0` against its Shadow from the previous section.

**Shadow** (from `int i = 0` and `int j = -1`):

prototype InitializedIntVariable : CSharp\_VariableDeclaration {  
    Type.TypeName \= "int";  
    Type.IsNullable \= false;  
    VariableName \= "";  
    Initializer \= new CSharp\_Expression();  
}

**Input Prototype**:

prototype CSharp\_VariableDeclaration {  
    CSharp\_Type Type \= new CSharp\_Type();  
    string VariableName \= "";  
    CSharp\_Expression Initializer \= new CSharp\_Expression();  
}  
prototype CSharp\_Type {  
    string TypeName \= "";  
    bool IsNullable \= false;  
}  
prototype CSharp\_Expression {  
    string Value \= "";  
}  
prototype Int\_Declaration\_I : CSharp\_VariableDeclaration {  
    Type.TypeName \= "int";  
    VariableName \= "i";  
    Initializer \= IntegerLiteral\_0;  
}  
prototype IntegerLiteral\_0 : CSharp\_Expression {  
    Value \= "0";  
}

**Parameterization**:

* **Categorization**: `Int_Declaration_I` matches `InitializedIntVariable` (same `TypeName`, `IsNullable`, and `Initializer` presence).  
* **Divergences**:  
  1. `VariableName`: `"i"` vs. `""` (Shadow’s wildcard).  
  2. `Initializer`: `IntegerLiteral_0` vs. generic `CSharp_Expression`.  
* **Paths**:

**Path 1: Variable Name**  
Int\_Declaration\_I.VariableName \= Compare.Entity  
// Result: string\["i"\]

1.   
   * Points to the specific value `"i"`.

**Path 2: Initializer**  
Int\_Declaration\_I.Initializer \= Compare.Entity  
// Result: IntegerLiteral\_0 { Value \= "0" }

2.   
   * Captures the entire `Initializer` subgraph.

**What’s Happening?**

* The Shadow confirms `Int_Declaration_I` is an “initialized integer variable.”  
* Paths isolate `"i"` and the `Initializer` subgraph as unique to `Int_Declaration_I`.  
* **Graph View**: Paths trace from `Int_Declaration_I` to `"i"` and `IntegerLiteral_0`, marked by `Compare.Entity`.  
* **Use Case**: Paths enable transformations (e.g., renaming the variable) or queries (e.g., finding all initializers with value `"0"`).

**Beyond Ontologies**: Unlike OWL’s static property assertions, Paths dynamically identify instance-specific differences, enabling flexible reasoning without predefined rules.

### **Example 2: Parameterizing Simpsons Characters**

**Scenario**: Parameterize `Homer` against the `SimpsonsHouseParent` Shadow.

**Shadow** (from `Homer` and `Marge`):

prototype SimpsonsHouseParent : Person {  
    Name \= "";  
    Gender \= "";  
    Location \= SimpsonsHouse;  
    ParentOf \= \[Bart, Lisa, Maggie\];  
    Spouse \= new Person();  
    Age \= 0;  
}

**Input Prototype**:

prototype Person {  
    string Name \= "";  
    string Gender \= "";  
    Location Location \= new Location();  
    Collection ParentOf \= new Collection();  
    Person Spouse \= new Person();  
    int Age \= 0;  
}  
prototype Location {  
    string Name \= "";  
}  
prototype SimpsonsHouse : Location {  
    Name \= "Simpsons House";  
}  
prototype Homer : Person {  
    Name \= "Homer Simpson";  
    Gender \= "Male";  
    Location \= SimpsonsHouse;  
    ParentOf \= \[Bart, Lisa, Maggie\];  
    Spouse \= Marge;  
    Age \= 39;  
}  
prototype Marge : Person {  
    Name \= "Marge Simpson";  
}

**Parameterization**:

* **Categorization**: `Homer` matches `SimpsonsHouseParent` (same `Location`, `ParentOf`).  
* **Divergences**:  
  1. `Name`: `"Homer Simpson"` vs. `""`.  
  2. `Gender`: `"Male"` vs. `""`.  
  3. `Spouse`: `Marge` vs. generic `Person`.  
  4. `Age`: `39` vs. `0`.  
* **Paths**:

**Path 1: Name**  
Homer.Name \= Compare.Entity  
// Result: string\["Homer Simpson"\]

1. 

**Path 2: Gender**  
Homer.Gender \= Compare.Entity  
// Result: string\["Male"\]

2. 

**Path 3: Spouse**  
Homer.Spouse \= Compare.Entity  
// Result: Marge

3. 

**Path 4: Age**  
Homer.Age \= Compare.Entity  
// Result: int\[39\]

4. 

**What’s Happening?**

* The Shadow confirms `Homer` is a “Simpsons house parent.”  
* Paths isolate `Name`, `Gender`, `Spouse`, and `Age` as unique features.  
* **Graph View**: Paths trace to `"Homer Simpson"`, `"Male"`, `Marge`, and `39`, marked by `Compare.Entity`.  
* **Use Case**: Paths support queries (e.g., finding parents with specific ages) or transformations (e.g., updating `Gender`).

**Beyond Ontologies**: Paths provide a granular view of instance differences, enabling dynamic reasoning without OWL’s complex SPARQL queries.

### **Example 3: Cross-Domain Transformation**

**Scenario**: Use Paths to transform a C\# variable to a database column.

**Shadow** (from `Int_Declaration_I` and `ID_Column`, previous section):

prototype IntDataElement {  
    string Name \= "";  
    string Type \= "int";  
}

**Input Prototype**:

prototype Int\_Declaration\_I : CSharp\_VariableDeclaration {  
    Type.TypeName \= "int";  
    VariableName \= "i";  
    Initializer \= IntegerLiteral\_0;  
}

**Parameterization**:

* **Paths**:

**Path 1: Name**  
Int\_Declaration\_I.VariableName \= Compare.Entity  
// Result: string\["i"\]

1. 

**Path 2: Initializer**  
Int\_Declaration\_I.Initializer \= Compare.Entity  
// Result: IntegerLiteral\_0 { Value \= "0" }

2. 

**Transformation**:

prototype Database\_Column {  
    string ColumnName \= "";  
    string DataType \= "";  
}  
function ToDatabaseColumn(CSharp\_VariableDeclaration var) : Database\_Column {  
    Database\_Column col \= new Database\_Column();  
    col.DataType \= var.Type.TypeName;  
    Parameterize(var, IntDataElement);  
    col.ColumnName \= var.VariableName; // From Path 1  
    return col;  
}

**What’s Happening?**

* The function uses the `Name` Path (`"i"`) to set `ColumnName`, mapping `Int_Declaration_I` to a database column.  
* **Result**: `Database_Column { ColumnName = "i", DataType = "int" }`.  
* **Graph View**: Paths extract `"i"` for transformation, linking to the new column node.  
* **Beyond Ontologies**: Paths enable seamless cross-domain mapping, unlike OWL’s need for external mappings.

## **Internal Mechanics**

The ProtoScript runtime manages Parameterization:

* **Categorization**: Uses `->` to match the Prototype against the Shadow’s structure.  
* **Traversal**: Walks the Prototype’s graph, identifying divergences.  
* **Path Generation**: Creates Paths as subgraphs, marked by `Compare.Entity`.  
* **Storage**: Paths are transient or stored for reuse, linked to the Prototype and Shadow.

**Scalability**:

* Paths focus on divergent subgraphs, reducing computational overhead compared to full graph comparisons.  
* Pruning (e.g., ignoring trivial Paths) ensures efficiency, supporting large ontologies.

## **Why Prototype Paths Are Essential**

Prototype Paths and Parameterization:

* **Complete the Learning Cycle**: Shadows generalize; Paths specify, enabling precise unsupervised learning.  
* **Enable Transformations**: Provide the data needed for cross-domain mappings (e.g., code to database).  
* **Enhance Reasoning**: Allow detailed queries about instance differences.  
* **Maintain Interpretability**: Paths are explicit subgraphs, traceable via graph traversal.

**Structural Contrast with Gradient Descent**: Paths refine Shadows’ learning without iterative optimization, offering a deterministic, scalable alternative for ontology reasoning.

## **Moving Forward**

Prototype Paths and Parameterization refine ProtoScript’s unsupervised learning, isolating instance-specific details to enhance categorization and transformation. 

# **Subtypes**

Subtypes in ProtoScript are a powerful feature that enables dynamic reclassification of Prototypes at runtime, allowing them to adopt more specific types based on context-sensitive conditions. Unlike traditional ontologies, which rely on static class hierarchies, Subtypes use runtime-evaluated functions to categorize Prototypes, making ProtoScript’s graph-based ontology exceptionally adaptive and flexible. Building on Shadows and Prototype Paths, Subtypes refine ProtoScript’s unsupervised learning by creating precise, ad-hoc categories that evolve with the data. This section explains Subtypes, their syntax, mechanics, and significance, using clear analogies, step-by-step examples, and practical applications to ensure developers familiar with C\# or JavaScript can grasp their role in dynamic ontology reasoning.

## **Why Subtypes Are Critical**

Subtypes are a cornerstone of ProtoScript’s dynamic ontology, enabling:

* **Dynamic Categorization**: Reclassify Prototypes into specific subtypes (e.g., “in-stock items”) based on runtime conditions, without predefined schemas.  
* **Unsupervised Learning Refinement**: Leverage Shadows and Paths to create and apply new categories, enhancing ProtoScript’s ability to learn from data without labeled inputs.  
* **Flexible Reasoning**: Support context-sensitive queries and transformations (e.g., identifying all “parents” in a family ontology).  
* **Cross-Domain Adaptability**: Apply Subtypes across domains (e.g., code, natural language), unifying diverse data types in the graph.

**Significance in ProtoScript’s Ontology**:

* **Core Learning Mechanism**: Subtypes, alongside Shadows and Paths, complete ProtoScript’s unsupervised learning cycle, allowing the system to generalize (Shadows), specify differences (Paths), and categorize dynamically (Subtypes).  
* **Scalable and Interpretable**: Subtypes use bounded graph traversals and reuse cached Shadow/Path results, keeping categorization explainable.  
* **Context-Sensitive**: By evaluating conditions at runtime, Subtypes adapt to evolving data, making them ideal for real-world, dynamic ontologies.

### **Beyond Traditional Ontologies**

Traditional ontologies (e.g., OWL, RDF) use static class definitions and axioms, requiring upfront design and external inference engines for categorization. ProtoScript’s Subtypes offer:

1. **Runtime Flexibility**: Subtypes are created and applied dynamically, unlike OWL’s fixed class hierarchies.  
2. **Unsupervised Categorization**: No labeled data is needed, unlike supervised ontology learning tools.  
3. **Graph-Centric Reasoning**: Subtypes use graph traversals for categorization, ensuring interpretability.  
4. **Simplified Development**: C\#-like syntax makes Subtype creation intuitive, unlike ontology editors like Protégé.

### **Analogy to Familiar Concepts**

For **C\# developers**:

* Subtypes are like dynamically applying an interface to an object at runtime based on a condition, but with the ability to create the interface on-the-fly.  
* Think of Subtypes as a LINQ query that filters objects into a new category (e.g., “parents”) based on runtime properties.

For **JavaScript developers**:

* Subtypes resemble adding a new prototype to an object dynamically, based on its properties, within a graph structure.  
* They’re like filtering JSON objects into a new group using a runtime condition.

For **database developers**:

* Subtypes are like creating a dynamic view in a database that groups records based on a query, but integrated into the graph.  
* Think of Subtypes as a graph query that tags nodes with a new type based on their properties.

## **What Are Subtypes?**

A **Subtype** in ProtoScript is a Prototype that defines a dynamic category, applied to other Prototypes at runtime if they satisfy a categorization function. Unlike static inheritance (e.g., `prototype Child : Parent`), Subtypes are evaluated dynamically using the `IsCategorized` function, which checks properties and relationships via the `->` operator. Subtypes:

* **Reclassify Prototypes**: Add a new type to a Prototype’s inheritance chain (e.g., making a `Person` a `Parent`).  
* **Leverage Shadows**: Use generalized structures from Shadows to define subtype conditions.  
* **Integrate Paths**: Identify divergent properties to refine categorization.

### **How Subtypes Work**

Subtypes are defined with the `[SubType]` annotation and an `IsCategorized` function, executed by the runtime to determine membership:

1. **Definition**: Create a Subtype Prototype with `[SubType]` and an `IsCategorized` function.  
2. **Categorization**: The runtime tests a Prototype against the Subtype using `->` and the function’s conditions.  
3. **Reclassification**: If the Prototype satisfies the conditions, the runtime adds the Subtype to its inheritance chain via an `isa` edge.  
4. **Application**: Use the Subtype for queries, transformations, or further reasoning.

**Syntax**:

\[SubType\]

prototype SubtypeName : Parent {

    function IsCategorized(Parent proto) : bool {

        return proto \-\> Parent { /\* Conditions \*/ };

    }

}

**C\# Analogy**: Like defining a dynamic interface with a method to check if an object qualifies, but integrated into the graph and applied at runtime.

### **Example 1: Subtyping C\# Variables**

**Scenario**: Define a Subtype for “initialized integer variables” and apply it to `int i = 0`.

**Shadow Reference** (from previous section):

prototype InitializedIntVariable : CSharp\_VariableDeclaration {

    Type.TypeName \= "int";

    Type.IsNullable \= false;

    VariableName \= "";

    Initializer \= new CSharp\_Expression();

}

**Subtype Definition**:

prototype CSharp\_VariableDeclaration {

    CSharp\_Type Type \= new CSharp\_Type();

    string VariableName \= "";

    CSharp\_Expression Initializer \= new CSharp\_Expression();

}

prototype CSharp\_Type {

    string TypeName \= "";

    bool IsNullable \= false;

}

prototype CSharp\_Expression {

    string Value \= "";

}

\[SubType\]

prototype InitializedIntVariable\_SubType : CSharp\_VariableDeclaration {

    function IsCategorized(CSharp\_VariableDeclaration var) : bool {

        return var \-\> CSharp\_VariableDeclaration {

            this.Type.TypeName \== "int" &&

            this.Type.IsNullable \== false &&

            this.Initializer \!= new CSharp\_Expression()

        };

    }

}

**Application**:

prototype Int\_Declaration\_I : CSharp\_VariableDeclaration {

    Type.TypeName \= "int";

    VariableName \= "i";

    Initializer \= IntegerLiteral\_0;

}

prototype IntegerLiteral\_0 : CSharp\_Expression {

    Value \= "0";

}

UnderstandUtil.SubType(Int\_Declaration\_I, \_interpreter);

**What’s Happening?**

* `InitializedIntVariable_SubType` defines a category for non-nullable `int` variables with an initializer.  
* `IsCategorized` checks if a Prototype has `TypeName = "int"`, `IsNullable = false`, and a non-default `Initializer`.  
* The runtime call `UnderstandUtil.SubType` tests `Int_Declaration_I`, adding `InitializedIntVariable_SubType` to its inheritance chain if it passes.  
* **Graph View**: `Int_Declaration_I` gains an `isa` edge to `InitializedIntVariable_SubType`.  
* **Use Case**: Enables queries like “find all initialized integer variables” or transformations (e.g., to another language).

**Beyond Ontologies**: Unlike OWL’s static class membership, Subtypes dynamically reclassify Prototypes, leveraging Shadows for unsupervised categorization.

### **Example 2: Subtyping Simpsons Characters**

**Scenario**: Define a Subtype for “parents in the Simpsons’ house” and apply it to `Homer`.

**Shadow Reference** (SimpsonsHouseParent):

prototype SimpsonsHouseParent : Person {

    Name \= "";

    Gender \= "";

    Location \= SimpsonsHouse;

    ParentOf \= \[Bart, Lisa, Maggie\];

    Spouse \= new Person();

    Age \= 0;

}

**Subtype Definition**:

prototype Person {

    string Name \= "";

    string Gender \= "";

    Location Location \= new Location();

    Collection ParentOf \= new Collection();

    Person Spouse \= new Person();

    int Age \= 0;

}

prototype Location {

    string Name \= "";

}

prototype SimpsonsHouse : Location {

    Name \= "Simpsons House";

}

\[SubType\]

prototype SimpsonsHouseParent\_SubType : Person {

    function IsCategorized(Person person) : bool {

        return person \-\> Person {

            this.Location.Name \== "Simpsons House" &&

            this.ParentOf.Count \> 0

        };

    }

}

**Application**:

prototype Homer : Person {

    Name \= "Homer Simpson";

    Gender \= "Male";

    Location \= SimpsonsHouse;

    ParentOf \= \[Bart, Lisa, Maggie\];

    Spouse \= Marge;

    Age \= 39;

}

prototype Marge : Person {

    Name \= "Marge Simpson";

}

UnderstandUtil.SubType(Homer, \_interpreter);

**What’s Happening?**

* `SimpsonsHouseParent_SubType` categorizes `Person` Prototypes living in `SimpsonsHouse` with at least one child.  
* `IsCategorized` uses `->` to check `Location.Name` and `ParentOf.Count`.  
* `Homer` passes, gaining `SimpsonsHouseParent_SubType` in its inheritance chain.  
* **Graph View**: `Homer` links to `SimpsonsHouseParent_SubType` via an `isa` edge.  
* **Use Case**: Supports queries like “who are the parents in the Simpsons’ house?” or transformations (e.g., generating a family report).

**Beyond Ontologies**: Subtypes enable context-sensitive categorization, adapting to runtime data unlike OWL’s predefined classes.

### **Example 3: Cross-Domain Subtyping**

**Scenario**: Define a Subtype for “integer data elements” across C\# variables and database columns.

**Shadow Reference** (IntDataElement):

prototype IntDataElement {

    string Name \= "";

    string Type \= "int";

}

**Subtype Definition**:

prototype DataElement {

    string Name \= "";

    string Type \= "";

}

\[SubType\]

prototype IntDataElement\_SubType : DataElement {

    function IsCategorized(DataElement elem) : bool {

        return elem \-\> DataElement {

            this.Type \== "int"

        };

    }

}

**Application**:

prototype CSharp\_VariableDeclaration : DataElement {

    string Name \= "";

    string Type \= "";

}

prototype Database\_Column : DataElement {

    string Name \= "";

    string Type \= "";

}

prototype Int\_Declaration\_I : CSharp\_VariableDeclaration {

    Name \= "i";

    Type \= "int";

}

prototype ID\_Column : Database\_Column {

    Name \= "ID";

    Type \= "int";

}

UnderstandUtil.SubType(Int\_Declaration\_I, \_interpreter);

UnderstandUtil.SubType(ID\_Column, \_interpreter);

**What’s Happening?**

* `IntDataElement_SubType` categorizes `DataElement` Prototypes with `Type = "int"`.  
* Both `Int_Declaration_I` and `ID_Column` pass, gaining `IntDataElement_SubType`.  
* **Graph View**: `Int_Declaration_I` and `ID_Column` link to `IntDataElement_SubType` via `isa` edges.  
* **Use Case**: Enables cross-domain queries (e.g., “find all integer data elements”) or transformations (e.g., mapping a variable to a column).

**Beyond Ontologies**: Subtypes unify code and database domains dynamically, unlike OWL’s separate ontologies.

## **Integration with Shadows and Paths**

Subtypes build on prior learning mechanisms:

* **Shadows**: Provide the generalized structure (e.g., `InitializedIntVariable`) that Subtypes use as a template for categorization.  
* **Paths**: Identify divergent properties (e.g., `VariableName = "i"`) that Subtypes can evaluate or transform.  
* **Example**: The `SimpsonsHouseParent_SubType` leverages the `SimpsonsHouseParent` Shadow, with Paths isolating `Name` or `Gender` for specific queries.

## **Internal Mechanics**

The ProtoScript runtime manages Subtypes:

* **Definition**: Stores the `[SubType]` Prototype and its `IsCategorized` function.  
* **Evaluation**: Executes `IsCategorized` via `->`, traversing the Prototype’s graph to check conditions.  
* **Reclassification**: Adds an `isa` edge to the Subtype if conditions are met.  
* **Scalability**: Efficient traversals and pruning ensure performance, even with complex ontologies.

## **Why Subtypes Are Essential**

Subtypes:

* **Enhance Learning**: Refine Shadows and Paths by creating precise, context-sensitive categories.  
* **Enable Dynamic Reasoning**: Support runtime queries and transformations without static schemas.  
* **Unify Domains**: Apply consistent categorization across code, data, and language.  
* **Maintain Interpretability**: Use explicit graph traversals, traceable unlike neural networks.

**Structural Contrast with Gradient Descent**: Subtypes offer a deterministic, unsupervised alternative, leveraging graph structures for scalability and clarity.

## **Moving Forward**

Subtypes empower ProtoScript’s ontology with dynamic, context-sensitive categorization, building on Shadows and Paths to create a robust unsupervised learning framework. 

# **Transformation Functions**

Transformation Functions in ProtoScript are runtime-driven operations that map one Prototype to another, enabling seamless transformations across diverse domains, such as converting a natural language (NL) statement to C\# code or a database query to a semantic model. Unlike traditional ontology systems, which rely on static mappings or external tools, Transformation Functions operate within ProtoScript’s graph-based framework, leveraging Shadows, Prototype Paths, and Subtypes to dynamically transform Prototypes based on their structure and context. This section explains the purpose, syntax, mechanics, and significance of Transformation Functions, using clear analogies, step-by-step examples, and practical applications to ensure developers familiar with C\# or JavaScript can harness their power for adaptive, cross-domain reasoning.

## **Why Transformation Functions Are Critical**

Transformation Functions are a key component of ProtoScript’s ontology, enabling:

* **Cross-Domain Mapping**: Transform Prototypes between domains (e.g., NL to SQL, C\# to database schema), unifying diverse data types in a single graph.  
* **Dynamic Adaptation**: Perform context-sensitive transformations at runtime, adapting to evolving data without predefined rules.  
* **Unsupervised Learning Integration**: Build on Shadows, Paths, and Subtypes to learn and apply mappings without labeled data.  
* **Reasoning and Automation**: Support automated tasks like code generation, query creation, or semantic parsing.

**Significance in ProtoScript’s Ontology**:

* **Core Transformation Mechanism**: Transformation Functions operationalize the learning from Shadows, Paths, and Subtypes, enabling practical applications of ProtoScript’s unsupervised learning.  
* **Scalable and Interpretable**: Graph-based operations honor the same hop limits, indexes, and pruning thresholds used during learning, keeping runtime predictable and readable.  
* **Developer-Friendly**: C\#-like syntax simplifies complex transformations, making them accessible to developers.

### **Beyond Traditional Ontologies**

Traditional ontologies (e.g., OWL, RDF) use static property assertions and external mapping tools, limiting their ability to dynamically transform data across domains. ProtoScript’s Transformation Functions offer:

1. **Runtime Flexibility**: Transformations are executed dynamically, adapting to data without redefining schemas.  
2. **Unsupervised Mapping**: No labeled data is needed, unlike supervised ontology alignment tools.  
3. **Graph-Centric Processing**: Transformations leverage graph traversals, ensuring interpretability.  
4. **Unified Framework**: A single mechanism handles code, data, and language, unlike OWL’s domain-specific mappings.

### **Analogy to Familiar Concepts**

For **C\# developers**:

* Transformation Functions are like factory methods that convert one object to another (e.g., DTO to entity), but driven by the runtime and operating on graph nodes.  
* Think of them as a LINQ pipeline that transforms data structures based on their properties, but integrated into a graph.

For **JavaScript developers**:

* They resemble functions that map one JSON object to another, but organized in a graph with dynamic, context-sensitive logic.  
* Imagine a JavaScript function that converts a user input to a database query, guided by the runtime.

For **database developers**:

* Transformation Functions are like stored procedures that convert one table’s data to another, but applied to graph nodes with programmable logic.  
* Think of them as a graph query that transforms a node’s structure into a new form.

## **What Are Transformation Functions?**

A **Transformation Function** is a ProtoScript function marked with the `[TransferFunction]` annotation, executed by the runtime to map an input Prototype to an output Prototype of a different type. Unlike regular functions, Transformation Functions are invoked automatically by the runtime based on a specified **dimension** (e.g., `NL`, `Implication`), forming a computational graph of transformations. They:

* **Map Structures**: Convert a Prototype’s graph (e.g., an NL sentence) to another (e.g., a semantic model).  
* **Leverage Learning**: Use Shadows for generalization, Paths for specific properties, and Subtypes for categorization.  
* **Enable Cross-Domain Tasks**: Support applications like code generation, query translation, or semantic parsing.

### **How Transformation Functions Work**

Transformation Functions are defined with a specific syntax and executed by the runtime in a hierarchical, recursive process:

1. **Definition**: Create a function with `[TransferFunction(Dimension)]`, specifying the transformation domain (e.g., `NL`).  
2. **Execution**: The runtime invokes the function via `UnderstandUtil.TransferToSememesWithDimension`, selecting functions based on the dimension.  
3. **Transformation**: The function constructs a new Prototype, copying or computing properties by traversing the input graph.  
4. **Recursion**: Nested Prototypes (e.g., a sub-object) are transformed recursively, building a tree of transformed nodes.

**Syntax**:

\[TransferFunction(Dimension)\]  
function FunctionName(InputType input) : OutputType {  
    // Construct and return new Prototype  
}

**C\# Analogy**: Like a factory method with dependency injection, where the runtime selects and chains methods based on attributes, but operating on graph nodes.

### **Example 1: Transforming Natural Language to Semantic Model**

**Scenario**: Transform the NL statement “the lady whispered” to a semantic `VerbalCommunication` Prototype.

**Input Prototype**:

prototype WhisperBase {  
    BaseObject Subject \= new BaseObject();  
    string Action \= "";  
}  
prototype Person {  
    string Name \= "";  
}  
prototype Whisper\_Lady : WhisperBase {  
    Subject \= Lady;  
    Action \= "Whisper";  
}  
prototype Lady : Person {  
    Name \= "Lady";  
}

**Transformation Function**:

prototype VerbalCommunication {  
    BaseObject SourceActor \= new BaseObject();  
    string Volume \= "";  
}  
prototype Quietly {  
    string Level \= "Quiet";  
}  
\[TransferFunction(NL)\]  
function Whisper\_To\_VerbalCommunication\_1(WhisperBase action) : VerbalCommunication {  
    VerbalCommunication meaning \= new VerbalCommunication();  
    if (action.Subject \!= new BaseObject()) {  
        meaning.SourceActor \= action.Subject;  
    }  
    meaning.Volume \= Quietly.Level;  
    return meaning;  
}

**Application**:

Collection sememes \= UnderstandUtil.TransferToSememesWithDimension(Whisper\_Lady, NL, \_interpreter);

**What’s Happening?**

* The `[TransferFunction(NL)]` marks the function for NL transformations.  
* `Whisper_To_VerbalCommunication_1` maps `WhisperBase` to `VerbalCommunication`, copying `Subject` to `SourceActor` and setting `Volume` to `"Quiet"`.  
* The runtime invokes the function, producing `VerbalCommunication { SourceActor = Lady, Volume = "Quiet" }`.  
* **Graph View**: `Whisper_Lady` links to a new `VerbalCommunication` node, with edges to `Lady` and `"Quiet"`.  
* **Use Case**: Supports semantic parsing for AI, enabling queries like “who performed quiet actions?”

**Beyond Ontologies**: Unlike OWL’s static mappings, Transformation Functions dynamically convert NL to semantics, leveraging the graph for flexibility.

### **Example 2: Transforming C\# Code to Database Column**

**Scenario**: Transform `int i = 0` to a database column.

**Input Prototype** (from prior sections):

prototype CSharp\_VariableDeclaration {  
    CSharp\_Type Type \= new CSharp\_Type();  
    string VariableName \= "";  
    CSharp\_Expression Initializer \= new CSharp\_Expression();  
}  
prototype CSharp\_Type {  
    string TypeName \= "";  
    bool IsNullable \= false;  
}  
prototype Int\_Declaration\_I : CSharp\_VariableDeclaration {  
    Type.TypeName \= "int";  
    VariableName \= "i";  
    Initializer \= IntegerLiteral\_0;  
}  
prototype IntegerLiteral\_0 : CSharp\_Expression {  
    string Value \= "0";  
}

**Transformation Function**:

prototype Database\_Column {  
    string ColumnName \= "";  
    string DataType \= "";  
}  
\[TransferFunction(Database)\]  
function Variable\_To\_Column(CSharp\_VariableDeclaration var) : Database\_Column {  
    Database\_Column col \= new Database\_Column();  
    col.DataType \= var.Type.TypeName;  
    col.ColumnName \= var.VariableName;  
    return col;  
}

**Application**:

Collection columns \= UnderstandUtil.TransferToSememesWithDimension(Int\_Declaration\_I, Database, \_interpreter);

**What’s Happening?**

* The function maps `Int_Declaration_I` to a `Database_Column`, copying `TypeName` to `DataType` and `VariableName` to `ColumnName`.  
* The runtime produces `Database_Column { ColumnName = "i", DataType = "int" }`.  
* **Graph View**: `Int_Declaration_I` links to a new `Database_Column` node with edges to `"i"` and `"int"`.  
* **Use Case**: Supports code-to-database schema generation, ensuring type consistency.

**Beyond Ontologies**: Transformation Functions unify code and database domains, unlike OWL’s need for separate ontologies.

### **Example 3: Hierarchical Transformation with Dimensions**

**Scenario**: Transform an NL question “Who lives in Springfield?” to a list of `Person` Prototypes, using hierarchical dimensions.

**Input Prototype**:

prototype Query {  
    string Question \= "";  
}  
prototype Springfield\_Query : Query {  
    Question \= "Who lives in Springfield?";  
}

**Transformation Functions**:

prototype Person {  
    string Name \= "";  
    Location Location \= new Location();  
}  
prototype Location {  
    string Name \= "";  
}  
prototype SimpsonsHouse : Location {  
    Name \= "Simpsons House";  
}  
prototype Homer : Person {  
    Name \= "Homer Simpson";  
    Location \= SimpsonsHouse;  
}  
\[TransferFunction(NL)\]  
function Query\_To\_PersonList(Query query) : Collection {  
    Collection people \= new Collection();  
    if (query.Question \== "Who lives in Springfield?") {  
        foreach (Person p in AllPersons) {  
            if (p.Location.Name \== "Simpsons House") {  
                people.Add(p);  
            }  
        }  
    }  
    return people;  
}  
\[TransferFunction(NL.Informative)\]  
function Query\_To\_InformativeList(Query query) : Collection {  
    // Subset of NL, for informative queries  
    return Query\_To\_PersonList(query);  
}

**Application**:

Collection people \= UnderstandUtil.TransferToSememesWithDimension(Springfield\_Query, NL, \_interpreter);

**What’s Happening?**

* `Query_To_PersonList` maps the query to a collection of `Person` nodes living in `SimpsonsHouse`.  
* `NL.Informative` defines a sub-dimension, allowing hierarchical execution (e.g., `NL` runs all NL functions, `NL.Informative` runs a subset).  
* The runtime produces a collection with `Homer` (and others in `SimpsonsHouse`).  
* **Graph View**: `Springfield_Query` links to a `Collection` node containing `Homer`.  
* **Use Case**: Supports NL-driven queries in AI applications, dynamically mapping questions to ontology data.

**Beyond Ontologies**: Hierarchical dimensions enable flexible, runtime-driven transformations, unlike OWL’s static query pipelines.

## **Integration with Shadows, Paths, and Subtypes**

Transformation Functions build on prior learning mechanisms:

* **Shadows**: Provide generalized structures (e.g., `InitializedIntVariable`) to identify input patterns.  
* **Paths**: Isolate specific properties (e.g., `VariableName = "i"`) for mapping to output Prototypes.  
* **Subtypes**: Categorize inputs (e.g., `InitializedIntVariable_SubType`) to select appropriate transformations.  
* **Example**: `Variable_To_Column` uses the `InitializedIntVariable` Shadow and `VariableName` Path to map a variable to a column.

## **Internal Mechanics**

The ProtoScript runtime manages Transformation Functions:

* **Definition**: Stores functions with `[TransferFunction]` and their dimensions.  
* **Execution**: Invokes functions via `UnderstandUtil.TransferToSememesWithDimension`, selecting based on dimension hierarchy.  
* **Graph Traversal**: Functions traverse input graphs, constructing output graphs with new nodes and edges.  
* **Recursion**: Nested Prototypes are transformed recursively, building a computational graph.  
* **Scalability**: Efficient traversals and dimension filtering ensure performance.

## **Why Transformation Functions Are Essential**

Transformation Functions:

* **Operationalize Learning**: Apply insights from Shadows, Paths, and Subtypes to practical tasks like code generation and semantic parsing.  
* **Enable Cross-Domain Integration**: Unify code, data, and language in a single graph.  
* **Support Dynamic Reasoning**: Adapt transformations to runtime data, enhancing flexibility.  
* **Maintain Interpretability**: Use explicit graph operations, traceable unlike neural networks.

**Structural Contrast with Gradient Descent**: Transformation Functions offer a deterministic, unsupervised alternative, leveraging graph structures for scalability and clarity.

## **Moving Forward**

Transformation Functions empower ProtoScript’s ontology with dynamic, cross-domain mappings, building on its unsupervised learning framework to automate complex tasks. 