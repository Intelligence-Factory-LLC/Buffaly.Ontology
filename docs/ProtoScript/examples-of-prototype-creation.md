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

---

**Previous:** [**NativeValuePrototypes**](native-value-prototypes.md) | **Next:** [**Simpsons Example for Prototype Modeling**](simpsons-example.md)
