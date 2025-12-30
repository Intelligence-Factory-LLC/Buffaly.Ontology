# **Prototype Paths and Parameterization**

In ProtoScript, **Prototype Paths** and **Parameterization** extend the power of Shadows by identifying how individual Prototypes diverge from their generalized structures, enabling precise categorization, reasoning, and transformation within the graph-based ontology. Building on Shadows’ ability to create ad-hoc subtypes through Least General Generalization (LGG), Paths isolate the specific subgraphs where a Prototype differs from a Shadow, marking these differences with a `Compare.Entity` indicator. This process, known as Parameterization, refines ProtoScript’s unsupervised learning, allowing the system to not only generalize patterns but also pinpoint unique characteristics of individual instances. This section explains Prototype Paths and Parameterization with clarity, using step-by-step examples and analogies to familiar programming concepts, ensuring developers understand their critical role in ProtoScript’s dynamic, scalable ontology framework. For the operators that make these traversals possible, revisit [Relationships in ProtoScript](relationships.md), and see how paths feed into downstream mapping in [Transformation Functions](transformation-functions.md).

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

---

**Previous:** [**Shadows and Least General Generalization (LGG)**](shadows-and-lgg.md) | **Next:** [**Subtypes**](subtypes.md)
