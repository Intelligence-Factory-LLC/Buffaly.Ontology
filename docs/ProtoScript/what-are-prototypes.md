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

```protoscript
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

```protoscript
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

```protoscript
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

```protoscript
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

Prototypes are the foundation of ProtoScript, providing a flexible, graph-based way to model entities and relationships. Next: ProtoScript syntax and features, covering how to define, manipulate, and extend Prototypes using C\#-inspired constructs.

---

**Previous:** [**ProtoScript Reference Manual**](ontology-context.md) | **Next:** [**ProtoScript Syntax and Features**](syntax-and-features.md)
