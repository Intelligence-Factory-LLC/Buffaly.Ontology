# **ProtoScript Syntax and Features**

ProtoScript’s syntax and features form the backbone of its graph-based programming paradigm, enabling developers to define, manipulate, and query Prototypes within the Buffaly system. If you’re familiar with C\#, ProtoScript’s syntax will feel intuitive, with a structure reminiscent of classes, methods, and attributes, but tailored for a dynamic, graph-oriented model. Unlike C\#’s static, class-based approach, ProtoScript is prototype-based, allowing runtime flexibility, multiple inheritance, and graph traversal operations. This section introduces ProtoScript’s core syntax and features, providing examples to illustrate their use and drawing analogies to familiar programming concepts. We’ll cover how these elements work together to model complex relationships, with a focus on clarity for developers new to ProtoScript. For details on how primitives are represented, see [NativeValuePrototypes](native-value-prototypes.md), and revisit [What Are Prototypes?](what-are-prototypes.md) for foundational context.

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

---

**Previous:** [**What Are Prototypes?**](what-are-prototypes.md) | **Next:** [**NativeValuePrototypes**](native-value-prototypes.md)
