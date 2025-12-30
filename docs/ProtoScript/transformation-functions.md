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

---

**Previous:** [**Subtypes**](subtypes.md) | **Next:** [Overview](README.md)
