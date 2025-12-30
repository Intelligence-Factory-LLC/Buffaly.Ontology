# **NativeValuePrototypes**

In ProtoScript, **NativeValuePrototypes** are specialized Prototypes that encapsulate primitive values—such as strings, booleans, integers, and doubles—as nodes within the graph-based Buffaly system. For developers familiar with C\# or JavaScript, NativeValuePrototypes are akin to primitive values (e.g., `int`, `string`) elevated to full-fledged graph entities, enabling them to participate in relationships, inheritance, and runtime operations like any other Prototype. This section explores the purpose, syntax, and mechanics of NativeValuePrototypes, with examples illustrating their role and analogies to familiar programming concepts. If you need a refresher on the surrounding syntax, hop over to [ProtoScript Syntax and Features](syntax-and-features.md) or see how these primitives show up in [Examples of Prototype Creation](examples-of-prototype-creation.md).

## **What Are NativeValuePrototypes?**

A **NativeValuePrototype** is a Prototype that wraps a primitive value, such as `"hello"`, `true`, or `42`, as a node in the graph, complete with a type identifier and metadata. Unlike raw primitives in C\# (e.g., `int x = 5`), which lack structure, NativeValuePrototypes integrate seamlessly into ProtoScript’s graph model, allowing uniform querying, serialization, and computation. ProtoScript’s runtime translates literal values (e.g., string literals, boolean literals) into NativeValuePrototypes, making them easy to use while preserving their graph-based nature.

### **Key Characteristics**

1. **Encapsulation of Primitive Values**

   * NativeValuePrototypes hold a single primitive value and its type (e.g., `string`, `System.String`).
   * **Example**: `"Buffalo"` represents the string "Buffalo" as a graph node; the runtime stores it as a NativeValuePrototype.
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

* NativeValuePrototypes are like boxed primitives (e.g., `object x = 5`), but as graph nodes. Using `string` with `"hello"` is like `string s = "hello"`, while the runtime retains metadata alongside the value, akin to a lightweight class node.
* Unlike C\#’s `struct` types, NativeValuePrototypes are graph-integrated, not standalone.

For **JavaScript developers**:

* They resemble primitive wrapper objects (e.g., `new String("hello")`), but persist as graph nodes, not transient wrappers, designed for traversal and relationships.

For **database developers**:

* Think of NativeValuePrototypes as graph database nodes with a single value property, linked to others, like scalar values in a relational database but with graph capabilities.

## **Syntax and Usage**

NativeValuePrototypes are used as property values or function inputs/outputs, with ProtoScript allowing direct literal initializers (`"hello"`, `true`, `42`). Literal syntax is the canonical authoring form. The runtime translates literals into NativeValuePrototypes, ensuring graph consistency.

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
  * Literal syntax is the canonical way to write primitive values. Internally, the runtime represents these literals as NativeValuePrototype nodes so primitives participate uniformly in graph traversal and serialization.
  * The runtime ensures literals are treated as NativeValuePrototypes, preserving type and value for graph operations.

**C\# Analogy**: Assigning `string s = "hello"` in C\# is like `string Name = "hello"` in ProtoScript, but the latter creates a graph node. Using `System.String` for the property highlights the Prototype metadata around the literal value.

**Example**:

prototype City {  
    string Name \= "";  
    bool IsCapital \= false;  
}  
prototype Buffalo\_City : City {  
    Name \= "Buffalo";  
    IsCapital \= false;  
}

**What’s Happening?**

* `City` defines `Name` as a `string` with a literal `""` and `IsCapital` as a `bool` with `false`.
* `Buffalo_City` sets `Name` to `"Buffalo"` (runtime translates to a `string` node) and `IsCapital` to `false` (runtime translates to a boolean node).
* **Graph View**: `Buffalo_City` links to nodes for `"Buffalo"` and `false`.

## **Common Native Types**

ProtoScript supports native types aligned with C\# primitives, available in two forms:

* **Primitive Types** (lowercase):  
  * `string`: Text values (e.g., `"hello"`).  
  * `bool`: True/false values (e.g., `true`).  
  * `int`: 32-bit integers (e.g., `42`).  
  * `double`: Floating-point numbers (e.g., `3.14`).  
* **Prototype Types** (uppercase):  
  * `System.String`: Text nodes (e.g., the literal `"hello"` stored with string metadata).
  * `System.Boolean`: Boolean nodes (e.g., the literal `true` stored with boolean metadata).
  * `System.Int32`: Integer nodes (e.g., the literal `42` stored with int metadata).
  * `System.Double`: Floating-point nodes (e.g., the literal `3.14` stored with double metadata).

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
  * **Example**: `0` serializes with its type metadata.
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
    string Pronoun \= "I";
}  
prototype BuyAction : Action {  
    Infinitive \= "ToBuy";  
    BaseObject Object \= TestKit;  
}  
prototype TestKit : COVID\_TestKit {  
    Quantity \= "Some";  
}

**What’s Happening?**

* `Need_BuyTestKits` uses `string` for `Infinitive` (`"ToBuy"`), `Quantity` (`"Some"`), and `Pronoun` (`"I"`).
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

---

**Previous:** [**ProtoScript Syntax and Features**](syntax-and-features.md) | **Next:** [**Examples of Prototype Creation**](examples-of-prototype-creation.md)
