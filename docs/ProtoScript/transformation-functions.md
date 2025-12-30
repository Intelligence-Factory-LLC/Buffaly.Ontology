# Chapter 7 — Transformation Functions

Transformation Functions are ProtoScript’s mechanism for mapping one graph-shaped meaning into another graph-shaped meaning: code to code, natural language to semantics, semantics to SQL, schema to API calls, and so on.

This chapter is written with two constraints in mind:

- You want progressive capability: start with transforms that are easy to learn/search (copy and bounded hops), then add power in controlled steps.
- You want named conceptual anchors: how each tier relates to known ideas (compiler passes, schema mapping, graph queries, Datalog fragments), without turning ProtoScript into a logic engine or a neural model.

The through-line is simple:

A transform is MATCH + BIND + CONSTRUCT over a typed property graph.

ProtoScript’s advantage is that you can keep MATCH bounded and auditable, and you can make CONSTRUCT explicit (reification), so the system scales without embeddings and without gradient descent.

---

## 7.1 What a Transform Is in ProtoScript

A Transformation Function is just a ProtoScript function annotated for runtime dispatch under a “dimension” (domain).

### 7.1.1 The annotation

```
	[TransferFunction(NL)]
	function SomeTransform(SomeInput input) : SomeOutput {
	// build and return output graph
	}
```

The runtime collects functions tagged with `[TransferFunction(...)]` and uses the requested dimension to select and execute them.

### 7.1.2 Invoking transforms

A typical invocation looks like:

```
	Collection outputs = UnderstandUtil.TransferToSememesWithDimension(
	inputPrototype,
	NL,
	_interpreter
	);
```

If multiple transfer functions apply, the runtime may return multiple outputs (or a single output containing explicit underconstraint objects; more on that later).

### 7.1.3 Transforms are not “methods on objects”

A normal method answers “what can this object do?”

A transform answers “how do I project this graph into another representation?”

Named concepts:

- Compiler: a lowering or rewriting pass (AST → IR, IR → IR, IR → AST)
- ETL / schema mapping: projection + renaming + join + construction
- Graph query: MATCH … CONSTRUCT …
- Logic (bounded): a nonrecursive rule fragment (conjunctive query + construction)

ProtoScript keeps it practical by making the output a concrete graph you can store, inspect, subtype, and reuse.

---

## 7.2 The Execution Model: MATCH, BIND, CONSTRUCT

A transform is easiest to reason about if you separate these phases explicitly.

### 7.2.1 MATCH: decide applicability

```
	function IsApplicable(Prototype p) : bool {
	return p typeof SomeInputType;
	}
```

You can also require subtype categories or structural constraints using ->:

```
	function IsApplicable(SomeInput input) : bool {
	return input -> SomeInput {
	this.RequiredProperty != new Prototype()
	};
	}
```

### 7.2.2 BIND: extract values by graph traversal

You bind “roles” (slots) by following properties.

```
	Prototype x = input.SomeEdge;
	string name = input.Name;
	Prototype y = input.SubGraph.OtherEdge;
```

### 7.2.3 CONSTRUCT: build the output graph

Construction is explicit: create nodes, assign properties, link edges.

```
	SomeOutput out = new SomeOutput();
	out.FieldA = name;
	out.FieldB = x;
	return out;
```

The key discipline for learnability is: avoid unconstrained computation during CONSTRUCT. Prefer copying bound values or reifying structure.

---

## 7.3 A Canonical “Level 0” Transform: Pure Copy and Renaming

This is the base tier: it is maximally learnable and maximally auditable.

### 7.3.1 Example: database row normalization

Input domain

```
	prototype RawPersonRow {
	string FName = "";
	string LName = "";
	int YearsOld = 0;
	}
```

Output domain

```
	prototype NormalizedPersonRow {
	string FirstName = "";
	string LastName = "";
	int Age = 0;
	}
```

Transform

```
	[TransferFunction(Database.Normalize)]
	function RawToNormalized(RawPersonRow row) : NormalizedPersonRow {
	NormalizedPersonRow out = new NormalizedPersonRow();
	out.FirstName = row.FName;
	out.LastName = row.LName;
	out.Age = row.YearsOld;
	return out;
	}
```

Named concepts:

- Schema mapping: renaming + projection
- Compiler refactor: purely syntactic rewrite (field rename)

This tier is the foundation for learning-by-example because it depends on role alignment, not semantics.

---

## 7.4 Progressive Capability: The Transform Ladder

Below are the tiers you should treat as a capability ladder. Each tier includes a realistic ProtoScript pattern with code.

### 7.4.1 Level 1: Duplication and Omission

Still copy-only. You may duplicate an input binding into multiple output fields; you may ignore unused input fields.

```
	prototype RawUserRow {
	string UserName = "";
	string Role = "";
	string Notes = ""; // noisy, not needed
	}

	prototype ProfileRow {
	string ID = "";
	string DisplayName = "";
	string Access = "";
	}

	[TransferFunction(Database.Normalize)]
	function RawUserToProfile(RawUserRow row) : ProfileRow {
	ProfileRow out = new ProfileRow();
	out.ID = row.UserName;          // copy
	out.DisplayName = row.UserName; // duplicate copy
	out.Access = row.Role;          // copy
	// row.Notes ignored
	return out;
	}
```

Named concepts:

- ETL: fan-out mapping and column dropping
- Relational algebra: projection (with repeated columns)

---

### 7.4.2 Level 2: Selection Policy (Controlled Choice, No New Values)

This tier matters because “the graph contains multiple candidates” is normal.

Example: pick primary identifier if present

```
	prototype RawItemRow {
	string PrimaryID = "";
	string SecondaryID = "";
	string Desc = "";
	}

	prototype CleanItemRow {
	string ID = "";
	string Type = "";
	}

	function ChooseNonEmpty(string a, string b) : string {
	if (a != "") return a;
	return b;
	}

	[TransferFunction(Database.Normalize)]
	function RawToClean(RawItemRow row) : CleanItemRow {
	CleanItemRow out = new CleanItemRow();
	out.ID = ChooseNonEmpty(row.PrimaryID, row.SecondaryID); // selection policy
	out.Type = row.Desc;
	return out;
	}
```

This is not “binary computation” in the dangerous sense; it is a choice policy over existing candidates.

Named concepts:

- Overload / binding resolution (compiler intuition): choose the best candidate under rules
- Data integration: conflict resolution policy
- Logic: choosing one satisfying witness vs returning the full relation

If you want maximum auditability, don’t hide this choice. Either emit provenance (below) or keep explicit underconstraint (Level 7).

---

### 7.4.3 Level 3: One-Hop Lookup (Dictionary Edge)

Use the input as a key into a fact graph. This is the first tier where you “retrieve something you didn’t explicitly carry in the input,” but it is still graph-copy at heart.

Example: error code to status

```
	prototype ErrorLogRow {
	string LogID = "";
	string Code = "";
	string Timestamp = "";
	}

	prototype ReadableLogRow {
	string EntryID = "";
	string Status = "";
	string Time = "";
	}

	// Dictionary graph
	prototype ErrorCodeDictionary {
	// keys are codes, values are statuses
	// e.g., Dict["404"] = "Not Found"
	// represented as graph edges for auditability
	Collection Entries = new Collection();
	}

	prototype ErrorCodeEntry {
	string Code = "";
	string Status = "";
	}

	prototype ErrorDict : ErrorCodeDictionary {
	Entries = [E404, E500, E200];
	}
	prototype E404 : ErrorCodeEntry { Code = "404"; Status = "Not Found"; }
	prototype E500 : ErrorCodeEntry { Code = "500"; Status = "Server Error"; }
	prototype E200 : ErrorCodeEntry { Code = "200"; Status = "Success"; }

	function LookupStatus(string code) : string {
	foreach (ErrorCodeEntry e in ErrorDict.Entries) {
	if (e.Code == code) return e.Status;
	}
	return "Unknown";
	}

	[TransferFunction(Logs.Readable)]
	function ErrorToReadable(ErrorLogRow row) : ReadableLogRow {
	ReadableLogRow out = new ReadableLogRow();
	out.EntryID = row.LogID;
	out.Time = row.Timestamp;
	out.Status = LookupStatus(row.Code); // one-hop conceptual lookup
	return out;
	}
```

Named concepts:

- Join to reference table (ETL)
- Property lookup (knowledge graph)
- Symbol table lookup (compiler intuition)

---

### 7.4.4 Level 4: Multi-Hop Traversal (n-Hop Copy)

This is the “everything is an ontology” insight made operational: copy is n-hop traversal.

Helper: follow a labeled path (bounded)

```
	function Hop(Prototype start, Collection props) : Prototype {
	Prototype cur = start;
	foreach (Prototype p in props) {
	if (cur == new Prototype()) return new Prototype();
	cur = cur.Properties[p];
	}
	return cur;
	}
```

Example: product → supplier → location → country (3 hops)

```
	prototype Product { string Name = ""; Prototype Supplier = new Prototype(); }
	prototype Supplier { Prototype Location = new Prototype(); }
	prototype Location { string Country = ""; }

	prototype OriginResultRow { string Country = ""; }

	[TransferFunction(SupplyChain.Origin)]
	function ProductToCountry(Product prod) : OriginResultRow {
	OriginResultRow out = new OriginResultRow();

	// Conceptual path: Supplier.Location.Country
	Prototype supplier = prod.Supplier;
	Prototype loc = supplier.Location;
	out.Country = loc.Country;

	return out;
	}
```

Named concepts:

- Bounded property path query (graph querying)
- Nonrecursive rule chaining (bounded Datalog fragment intuition)

Critical boundary:

- bounded n is safe and searchable
- “keep hopping until…” introduces recursion/loops (a different computational regime)

---

### 7.4.5 Level 5: Joins and Structural Constraints (Select–Project–Join over Graph)

This tier is where transforms start behaving like real queries.

Example: map semantic “Need/Buy” graph into a SQL select with constraints

Semantic input

```
	prototype Need {
	Prototype Subject = new Prototype();
	Prototype Action = new Prototype();
	}

	prototype BuyAction {
	Prototype Object = new Prototype();
	string Quantity = "";
	}

	prototype Product { string SKU = ""; string Name = ""; }
```

SQL output

```
	prototype SQL_Select {
	Prototype Table = new Prototype();
	Collection Columns = new Collection();
	Prototype Where = new Prototype();
	}

	prototype SQL_Table { string TableName = ""; }
	prototype SQL_WhereEquals { string Column = ""; string Value = ""; }

	prototype SQL_Column { string ColumnName = ""; }

	prototype Wildcard_Column : SQL_Column { ColumnName = "*"; }
```

Transform with constraints

```
	[TransferFunction(NL.ToSQL)]
	function NeedBuyToSQL(Need n) : SQL_Select {
	SQL_Select q = new SQL_Select();
	q.Table = new SQL_Table();
	q.Columns = [Wildcard_Column];

	// Constraint: Need.Action must be a BuyAction
	if (!(n.Action typeof BuyAction)) {
	// not applicable: return empty query prototype (or omit result)
	return new SQL_Select();
	}

	BuyAction buy = (BuyAction)n.Action;

	// Constraint: BuyAction.Object must be a Product with an SKU
	if (!(buy.Object typeof Product)) {
	return new SQL_Select();
	}

	Product p = (Product)buy.Object;

	q.Table.TableName = "Products";

	// Join-like constraint: bind SKU into WHERE
	SQL_WhereEquals w = new SQL_WhereEquals();
	w.Column = "SKU";
	w.Value = p.SKU;
	q.Where = w;

	return q;
	}
```

Named concepts:

- Relational algebra: selection + projection (and joins when multiple bindings must agree)
- Graph pattern matching: basic graph patterns
- Logic: conjunctive query with guards

The key is that the “join” is equality over bindings, not arbitrary computation.

---

### 7.4.6 Level 6: Graph Construction (Reification) as the Primary Power Move

Reification lets you create durable, queryable objects that did not exist explicitly in the input.

This is the tier where ProtoScript stops being “mapping” and becomes “ontology building.”

Example: reify a PurchaseRequest from a Need/Buy structure

```
	prototype PurchaseRequest {
	Prototype Requestor = new Prototype();
	Prototype Item = new Prototype();
	string Quantity = "";
	Prototype Provenance = new Prototype();
	}

	prototype Provenance {
	Prototype Source = new Prototype();
	string TransformName = "";
	}

	[TransferFunction(NL.ToCanonical)]
	function NeedBuyToPurchaseRequest(Need n) : PurchaseRequest {
	if (!(n.Action typeof BuyAction)) return new PurchaseRequest();

	BuyAction buy = (BuyAction)n.Action;

	PurchaseRequest pr = new PurchaseRequest();
	pr.Requestor = n.Subject;
	pr.Item = buy.Object;
	pr.Quantity = buy.Quantity;

	Provenance prov = new Provenance();
	prov.Source = n;
	prov.TransformName = "NeedBuyToPurchaseRequest";
	pr.Provenance = prov;

	return pr;
	}
```

Named concepts:

- CONSTRUCT queries in graph systems
- Compiler IR construction (lowering high-level shape into canonical IR)
- Knowledge representation: reification (turn “a relation” into “an entity”)

This is often the right alternative to binary functions: instead of “computing” a new scalar, you create a node that represents the concept.

---

### 7.4.7 Level 7: Underconstraint as a First-Class Output (No Hidden Guessing)

When the graph provides multiple valid bindings, you either:

- return multiple outputs, or
- return one output that contains an explicit “choice object.”

The second is usually better for auditability and later disambiguation.

Example: multiple suppliers → explicit choice

```
	prototype Choice {
	Collection Options = new Collection();
	Prototype Justification = new Prototype();
	}

	prototype OriginResult {
	Prototype Country = new Prototype(); // either string or Choice
	}

	function CountriesForProduct(Product prod) : Collection {
	Collection countries = new Collection();

	// Suppose prod.Suppliers is a collection and each supplier has Location.Country
	foreach (Supplier s in prod.Suppliers) {
	if (s.Location != new Prototype()) {
	countries.Add(s.Location.Country);
	}
	}
	return countries;
	}

	[TransferFunction(SupplyChain.Origin)]
	function ProductToCountry_Underconstrained(Product prod) : OriginResult {
	OriginResult out = new OriginResult();

	Collection countries = CountriesForProduct(prod);

	if (countries.Count == 0) {
	out.Country = "Unknown";
	return out;
	}

	if (countries.Count == 1) {
	out.Country = countries[0];
	return out;
	}

	Choice c = new Choice();
	c.Options = countries;

	// Optional justification node
	prototype MultiSupplierJustification;
	c.Justification = MultiSupplierJustification;

	out.Country = c;
	return out;
	}
```

Named concepts:

- Logic: returning a relation (set of witnesses) instead of collapsing to one
- Data integration: ambiguous matches preserved for downstream resolution
- Safety: explicit uncertainty objects rather than silent heuristics

If you combine this with your clustering/HCP machinery, you can later learn what additional constraints collapse the choice.

---

### 7.4.8 Level 8: Composition and Pipelines (Higher-Order, but Controlled)

Instead of synthesizing huge transforms, compose small ones:

- NL → canonical request graph
- canonical request → SQL
- canonical request → API call graph
- SQL → explanation graph

Example: a two-stage pipeline in code

```
	[TransferFunction(NL.Pipeline)]
	function NL_To_SQL(Need n) : SQL_Select {
	// Stage 1: NL to canonical
	PurchaseRequest pr = (PurchaseRequest)UnderstandUtil.TransferToSememesWithDimension(
	n,
	NL.ToCanonical,
	_interpreter
	)[0];

	// Stage 2: canonical to SQL
	SQL_Select q = (SQL_Select)UnderstandUtil.TransferToSememesWithDimension(
	pr,
	Canonical.ToSQL,
	_interpreter
	)[0];

	return q;
	}
```

Named concepts:

- Compiler pipeline: pass composition
- Program synthesis best practice: learn primitives, then compose
- IR discipline: canonical intermediate forms reduce combinatorial search

The reason this matters: it gives you high expressivity while keeping each piece enumerable and testable.

---

## 7.5 Transform Design Patterns That Scale Without Embeddings

These patterns are what keep the system “searchable” and stable.

### 7.5.1 Keep “hop depth” explicit and bounded

Do not allow transforms that effectively do unbounded search. If you need that someday, introduce it deliberately as a separate capability tier with strict cost controls.

### 7.5.2 Prefer reification over scalar computation

If you feel tempted to add a binary operator, ask whether you should instead:

- reify the relationship,
- attach the operands as edges,
- let later transforms interpret it.

Example: instead of computing `FullName = FName + " " + LName` (string concatenation), create:

```
	prototype FullName {
	string First = "";
	string Last = "";
	}

	prototype PersonName {
	Prototype Full = new Prototype(); // may be FullName node
	}
```

Then downstream code can render it as needed.

That preserves enumerability and auditability.

### 7.5.3 Emit provenance by default

Transforms that write ontology facts should attach a provenance node:

- source prototype
- transform name
- key bindings used

This is your antidote to poisoning and drift.

---

## 7.6 Relating the Ladder to Named Concepts (without turning ProtoScript into Prolog)

It’s useful to know where you are in the landscape:

- Levels 0–2 correspond to schema mapping / refactoring.
- Levels 3–5 correspond to bounded graph query + guarded joins (think conjunctive queries).
- Level 6 corresponds to graph construction / IR lowering / reification.
- Level 7 corresponds to explicit nondeterminism / relation-valued results.
- Level 8 corresponds to pass pipelines.

If you squint, Levels 3–5 resemble fragments of Datalog, but ProtoScript differs in two crucial ways:

1. You are not trying to be a general theorem prover.
2. You are building concrete graph artifacts (which can become Prototypes, subtypes, HCP deltas, and future anchors).

That is why the system is practical for large knowledge bases.

---

## 7.7 “Transforms as Learned Objects” (How This Connects to Shadows, Paths, HCPs)

A transform becomes dramatically easier to learn when you don’t learn it over raw graphs, but over delta-coded representations.

A productive mental model:

- A Shadow defines the invariant structure of a cluster.
- An HCP defines the variable slots (deltas) for instances in that cluster.
- A transform becomes “map input slots to output slots, plus bounded hops from those slots.”

### 7.7.1 A transform stored as a first-class prototype

This is a publishable pattern because it makes transforms inspectable and learnable.

```
	prototype TransferStep {
	// A step says: bind from an input role, traverse optional path, write to output role
	string InputRole = "";
	Collection HopPath = new Collection(); // list of property prototypes
	string OutputRole = "";
	}

	prototype TransferMap {
	string Name = "";
	Collection Steps = new Collection();
	}

	prototype RawToNormalized_Map : TransferMap {
	Name = "RawToNormalized_Map";
	Steps = [Step_FirstName, Step_LastName, Step_Age];
	}

	prototype Step_FirstName : TransferStep {
	InputRole = "FName";
	HopPath = [];          // 0-hop
	OutputRole = "FirstName";
	}
	prototype Step_LastName : TransferStep {
	InputRole = "LName";
	HopPath = [];
	OutputRole = "LastName";
	}
	prototype Step_Age : TransferStep {
	InputRole = "YearsOld";
	HopPath = [];
	OutputRole = "Age";
	}
```

Now you can write a single interpreter that applies a TransferMap to any compatible input:

```
	function ApplyMap(TransferMap map, Prototype input, Prototype outputTemplate) : Prototype {
	Prototype out = outputTemplate;

	foreach (TransferStep s in map.Steps) {
	Prototype v = input.Properties[s.InputRole];

	// bounded hop path
	foreach (Prototype p in s.HopPath) {
	if (v == new Prototype()) break;
	v = v.Properties[p];
	}

	out.Properties[s.OutputRole] = v;
	}

	return out;
	}
```

This is the key bridge to your “learn without gradient descent” objective:

- learning becomes searching over TransferStep candidates (role correspondences + hop paths),
- not synthesizing arbitrary programs.

Named concepts:

- schema mapping language
- graph query compilation
- restricted program synthesis (library-of-steps, not arbitrary ASTs)

---

## 7.8 End-to-End Example: NL → Canonical → SQL with Underconstraint

This example ties multiple tiers together and is close to “publishable” because it shows the system doing real work with explicit uncertainty and without embeddings.

### 7.8.1 Input semantics (NL parse output)

```
	prototype Need {
	Prototype Subject = new Prototype();
	Prototype Action = new Prototype();
	}

	prototype BuyAction {
	Prototype Object = new Prototype();
	string Quantity = "";
	}

	prototype Person { string Name = ""; }
	prototype Product { string SKU = ""; string Name = ""; }

	prototype Person_I : Person { Name = "I"; }
	prototype TestKit : Product { Name = "covid-19 test kit"; }

	prototype Need_BuyTestKits : Need {
	Subject = Person_I;
	Action = Buy;
	}

	prototype Buy : BuyAction {
	Object = TestKit;
	Quantity = "Some";
	}
```

### 7.8.2 Transform NL → canonical request (Level 6)

```
	prototype PurchaseRequest {
	Prototype Requestor = new Prototype();
	Prototype Item = new Prototype();
	string Quantity = "";
	Prototype Provenance = new Prototype();
	}

	[TransferFunction(NL.ToCanonical)]
	function NeedBuy_To_PurchaseRequest(Need n) : PurchaseRequest {
	if (!(n.Action typeof BuyAction)) return new PurchaseRequest();

	BuyAction b = (BuyAction)n.Action;

	PurchaseRequest pr = new PurchaseRequest();
	pr.Requestor = n.Subject;
	pr.Item = b.Object;
	pr.Quantity = b.Quantity;

	Provenance prov = new Provenance();
	prov.Source = n;
	prov.TransformName = "NeedBuy_To_PurchaseRequest";
	pr.Provenance = prov;

	return pr;
	}
```

### 7.8.3 Canonical → SQL (Level 5, with possible underconstraint)

Assume Product sometimes has multiple SKUs (or multiple candidate DB matches). We keep it explicit.

```
	prototype SQL_Select {
	Prototype Table = new Prototype();
	Collection Columns = new Collection();
	Prototype Where = new Prototype();
	}

	prototype SQL_Table { string TableName = ""; }
	prototype SQL_WhereEquals { string Column = ""; Prototype Value = new Prototype(); }
	prototype SQL_Column { string ColumnName = ""; }
	prototype Wildcard_Column : SQL_Column { ColumnName = "*"; }

	[TransferFunction(Canonical.ToSQL)]
	function PurchaseRequest_To_SQL(PurchaseRequest pr) : SQL_Select {
	SQL_Select q = new SQL_Select();
	q.Table = new SQL_Table();
	q.Table.TableName = "Inventory";
	q.Columns = [Wildcard_Column];

	SQL_WhereEquals w = new SQL_WhereEquals();
	w.Column = "ProductSKU";

	// If SKU missing, keep underconstraint or fall back to name
	if (pr.Item typeof Product) {
	Product p = (Product)pr.Item;
	if (p.SKU != "") {
	w.Value = p.SKU;
	} else {
	// Underconstrained: query by name (less precise) or emit Choice
	w.Column = "ProductName";
	w.Value = p.Name;
	}
	}

	q.Where = w;
	return q;
	}
```

This is not “LLM-style guessing.” It is controlled degradation (or explicit underconstraint) under missing structure.

---

## 7.9 Summary: What Makes ProtoScript Transforms Publishable

If you want this chapter (and the system) to be taken seriously by people who build real transformation and ontology systems, the publishable stance is:

1. Transforms are graph-to-graph programs: MATCH/BIND/CONSTRUCT.
2. Expressivity increases progressively: copy → lookup → bounded hops → joins/constraints → reification → explicit underconstraint → composition.
3. The dangerous frontier is binary operators and unbounded recursion: you treat them as advanced, constrained extensions, not defaults.
4. Learning is feasible without embeddings because the transform hypothesis space is structured around:
- role correspondences (slots),
- bounded hop paths,
- explicit constraints,
- and reusable step libraries (maps/macros).

---

**Previous:** [Subtypes](subtypes.md) | **Next:** [Overview](README.md)
