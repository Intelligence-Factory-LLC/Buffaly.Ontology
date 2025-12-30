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

Subtypes categorize Prototypes dynamically through graph traversals, providing runtime flexibility without the static class definitions or external inference engines used in traditional ontology stacks.

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

> Note: The following snippet is conceptual pseudocode. The actual runtime API is very similar and equivalent in behavior; treat this as an explanatory representation of the operation.

UnderstandUtil.SubType(Int\_Declaration\_I, \_interpreter);

**What’s Happening?**

* `InitializedIntVariable_SubType` defines a category for non-nullable `int` variables with an initializer.  
* `IsCategorized` checks if a Prototype has `TypeName = "int"`, `IsNullable = false`, and a non-default `Initializer`.  
* The runtime call `UnderstandUtil.SubType` tests `Int_Declaration_I`, adding `InitializedIntVariable_SubType` to its inheritance chain if it passes.  
* **Graph View**: `Int_Declaration_I` gains an `isa` edge to `InitializedIntVariable_SubType`.  
* **Use Case**: Enables queries like “find all initialized integer variables” or transformations (e.g., to another language).

Subtypes dynamically reclassify Prototypes, leveraging Shadows for unsupervised categorization, rather than relying on static class membership in OWL.

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

Subtypes enable context-sensitive categorization, adapting to runtime data instead of predefined classes.

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

Subtypes unify code and database domains dynamically, avoiding the separation found in static ontology models.

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

---

**Previous:** [**Prototype Paths and Parameterization**](prototype-paths.md) | **Next:** [**Transformation Functions**](transformation-functions.md)
