using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Ontology;
using Ontology.BaseTypes;
using Ontology.Simulation;
using static Ontology.Compare;

namespace Ontology.GraphInduction.Induction;

//public sealed class GraphInductionEngine
//{
//	public InductionOptions Options { get; } = new();

//	// Learned patterns keyed by PatternId
//	private readonly Dictionary<string, HcpPattern> _patterns = new();

//	public IReadOnlyDictionary<string, HcpPattern> Patterns => _patterns;

//	// We cache the hidden-base prototype-ids we have introduced, so we can treat them as "atomic leaves"
//	// in later passes (this is the "overlay typing" / "restart entity search with holes" behavior).
//	private readonly HashSet<string> _stopTypeIds = new();

//	/// <summary>
//	/// Mine a single "file graph" (the whole file parsed into one Prototype graph) and induce a hierarchy of HCP-like patterns.
//	///
//	/// This is intentionally NOT a "shape-bucket" algorithm.
//	/// It follows your older approach:
//	///   - start from leaf sentinels (especially concrete string leaves),
//	///   - walk up to root,
//	///   - look for repeated subgraphs via categorization (TemporaryPrototypeCategorization),
//	///   - keep the *longest* repeated subgraphs (structure emerges naturally),
//	///   - learn an HCP shadow by anti-unifying occurrences (differences become Compare.Entity holes),
//	///   - overlay-type all occurrences with a learned hidden base (do NOT rewrite file, just "stop here" in later passes),
//	///   - repeat (now those learned nodes behave like new leaves / holes).
//	/// </summary>
//	public List<HcpPattern> MineFileGraph(Prototype fileGraph)
//	{
//		fileGraph.SetParents();

//		List<HcpPattern> learned = new List<HcpPattern>();

//		for (int pass = 0; pass < Options.MaxPasses; pass++)
//		{
//			// Pass 0:
//			//   - Leaves are concrete tokens from the code graph (string leaves, identifiers-as-string leaves, etc.)
//			// Later passes:
//			//   - Leaves also include previously learned hidden nodes (overlay typing makes them atomic),
//			//     so larger patterns can emerge without us inventing any new "signature" language.
//			bool stopAtHidden = pass > 0;

//			// 1) Discover candidate families using *leaf-anchored* repeated-subgraph detection.
//			//    This is basically GetEntityPathsByLeaf2 generalized to a whole-file graph and with overlay-typing support.
//			List<CandidateFamily> families = DiscoverCandidateFamiliesByLeaf(fileGraph, stopAtHidden);

//			// Rank families:
//			// - first by support (# occurrences),
//			// - then by node size (prefer richer structure),
//			// - and you can later add your DIGS/Dirichlet-based usefulness score here.
//			families = families
//			.OrderByDescending(x => x.Support)
//			.ThenByDescending(x => x.TemplateSize)
//			.ToList();

//			int learnedThisPass = 0;

//			foreach (CandidateFamily family in families)
//			{
//				// Pull a bounded number of examples for anti-unification.
//				// IMPORTANT:
//				// - Clone because we may overlay-type occurrences in the live graph.
//				// - We do NOT want the learned shadow to be impacted by later mutations.
//				List<Prototype> examples = family.Occurrences
//				.Take(Options.MaxExamplesPerFamily)
//				.Select(x => x.Clone())
//				.ToList();

//				HcpPattern? pattern = TryLearnPattern(examples, family.Support);
//				if (pattern == null)
//				continue;

//				// De-dupe by PatternId (hash of the learned shadow).
//				if (_patterns.ContainsKey(pattern.PatternId))
//				continue;

//				_patterns[pattern.PatternId] = pattern;
//				learned.Add(pattern);
//				learnedThisPass++;

//				// 2) Overlay typing (Option B):
//				//    Mark each occurrence as "typeof <HiddenBase>" so subsequent passes can treat it as an atomic leaf.
//				//    This is the exact analog of "introduce Compare.Entity and restart entity search", except:
//				//      - we do it with a learned hidden base type, not by destructively Revalue()'ing the node.
//				//      - the stop logic is in our traversal helpers (IsStopNode).
//				_stopTypeIds.Add(pattern.HiddenBase.PrototypeID);

//				foreach (Prototype occ in family.Occurrences)
//				{
//					occ.InsertTypeOf(pattern.HiddenBase.PrototypeID);
//				}
//			}

//			// If we learned nothing new, the hierarchy has saturated for this file graph under current thresholds.
//			if (learnedThisPass == 0)
//			break;

//			// Parents may have shifted meaningfully after overlay typing; refresh parent pointers.
//			fileGraph.SetParents();
//		}

//		return learned;
//	}

//	private int GetNodeSize(Prototype node)
//	{
//		int count = 0;
//		PrototypeGraphs.DepthFirstOnNormal(node, x =>
//		{
//			count++;
//			return x;
//		});
//		return count;
//	}

//	/// <summary>
//	/// Core pattern learning:
//	/// Given N occurrences (examples) of "the same repeated structure", produce:
//	///   - a Shadow template where differences are Compare.Entity (holes),
//	///   - a set of Paths to those holes (entity slots),
//	///   - a stable HiddenBase prototype derived from the Shadow hash.
//	///
//	/// This is explicitly anti-unification (LGG) over the occurrences.
//	/// That matches your requirement:
//	///   - HCPs contain entities (holes),
//	///   - we are NOT "building HCPs by comparing entities";
//	///     we are building an HCP shadow first, then the entity slots are *defined by their positions* in that shadow.
//	/// </summary>
//	private HcpPattern? TryLearnPattern(List<Prototype> examples, int support)
//	{
//		if (examples.Count == 0)
//		return null;

//		if (support < Options.MinSupport)
//		return null;

//		// Start from the first example; fold the rest in via anti-unification.
//		Prototype shadow = examples[0].Clone();

//		for (int i = 1; i < examples.Count; i++)
//		{
//			shadow = AntiUnify(shadow, examples[i]);
//		}

//		shadow.SetParents();

//		// Collect entity-slot paths: any Compare.Entity leaf in the shadow is a "hole".
//		List<Prototype> entityPaths = CollectEntityPaths(shadow);

//		// Usefulness gating (foundation heuristics; later you can replace with DIGS/Dirichlet):
//		// - If there are no holes, it's not an HCP in your sense (it's just a constant tree).
//		// - If there are no concrete sentinels, it's too generic (blank-statement-like).
//		//   (Blank statements are "always predictable" but not useful.)
//		if (entityPaths.Count == 0)
//		return null;

//		int sentinelLeaves = CountConcreteStringLeaves(shadow);
//		if (sentinelLeaves == 0)
//		return null;

//		int templateSize = GetNodeSize(shadow);
//		if (templateSize < Options.MinNodeSize)
//		return null;

//		// Hidden base naming:
//		// In your existing system, hidden prototypes live under Dimensions.CSharp_Code_Hidden (e.g., CSharp.Code.Hidden.<HASH>).
//		// Hash the shadow so the same induced pattern is stable across runs.
//		string shadowHash = PrototypeGraphs.GetHash(shadow);
//		Prototype hiddenBase = Prototypes.GetOrInsertPrototype(Dimensions.CSharp_Code_Hidden + "." + shadowHash);

//		// OPTIONAL (but important conceptually, and matches your "typed by spot in the HCP"):
//		// Build an entity-shadow for each slot by looking at what actually filled that slot across occurrences.
//		// This is how "entities get typed by their spot in the HCP" and how you get second-order relationships later.
//		List<Prototype> entityShadows = new List<Prototype>();
//		foreach (Prototype path in entityPaths)
//		{
//			List<Prototype> slotValues = new List<Prototype>();
//			foreach (Prototype ex in examples)
//			{
//				Prototype? v = PrototypeGraphs.GetValue(ex, path);
//				if (v != null)
//				slotValues.Add(v);
//			}

//			// If nothing filled it, skip.
//			if (slotValues.Count == 0)
//			{
//				entityShadows.Add(Entity.Prototype.ShallowClone());
//				continue;
//			}

//			// A lightweight entity-shadow (anti-unify the values themselves).
//			// If you want to exactly match your older behavior, swap this for:
//			//   EntityUtil.CreateEntityShadowFromValues(slotValues, Dimensions.CSharp_Code_Hidden);
//			Prototype eShadow = slotValues[0].Clone();
//			for (int i = 1; i < slotValues.Count; i++)
//			eShadow = AntiUnify(eShadow, slotValues[i]);

//			entityShadows.Add(eShadow);
//		}

//		// NOTE:
//		// This assumes HcpPattern has (at minimum) PatternId + HiddenBase, and ideally also Shadow + EntityPaths (+ EntityShadows).
//		// If your current HcpPattern differs, adjust the model accordingly.
//		HcpPattern pattern = new HcpPattern
//		{
//			PatternId = shadowHash,
//			HiddenBase = hiddenBase,
//			Shadow = shadow,
//			EntityPaths = entityPaths,
//			EntityShadows = entityShadows,
//			Support = support,
//			TemplateSize = templateSize
//		};

//		return pattern;
//	}

//	/// <summary>
//	/// Anti-unification over Prototype graphs:
//	/// - Keep shared structure (same shallow prototype),
//	/// - Turn disagreements into Compare.Entity (a hole).
//	///
//	/// This produces an HCP-like shadow:
//	///   "the same" parts stay literal (including method names, stable sentinels),
//	///   variable parts become Compare.Entity.
//	///
//	/// IMPORTANT:
//	/// We DO NOT want to "sterilize everything" by default:
//	/// - If two strings are equal, keep the literal.
//	/// - Only differing leaves become holes.
//	/// Families are discovered leaf-first, so method names/class names tend to be stable within a family.
//	/// </summary>
//	private Prototype AntiUnify(Prototype a, Prototype b)
//	{
//		// If either side is already a hole, result stays a hole.
//		if (a == null || b == null)
//		return Entity.Prototype.ShallowClone();

//		if (a.ShallowEqual(Entity.Prototype) || b.ShallowEqual(Entity.Prototype))
//		return Entity.Prototype.ShallowClone();

//		// If we previously overlay-typed a subgraph as a learned hidden base, treat it as atomic.
//		// (This is the "restart entity search with holes" behavior across passes.)
//		if (IsStopNode(a, true) || IsStopNode(b, true))
//		{
//			// If both are categorized the same way at the atomic level, keep one; otherwise hole.
//			if (TemporaryPrototypeCategorization.IsCategorized(a, b) || TemporaryPrototypeCategorization.IsCategorized(b, a))
//			return a.ShallowClone();

//			return Entity.Prototype.ShallowClone();
//		}

//		// If the shallow prototypes differ, generalize to a hole.
//		if (!Prototypes.AreShallowEqual(a, b))
//		return Entity.Prototype.ShallowClone();

//		// Special handling for native values:
//		// Same shallow type (e.g., System.String), but value differs => hole.
//		// Same value => keep literal.
//		if (a is NativeValuePrototype av && b is NativeValuePrototype bv)
//		{
//			if (Equals(av.NativeValue, bv.NativeValue))
//			return a.Clone();

//			return Entity.Prototype.ShallowClone();
//		}

//		// Otherwise preserve the node "shape" and recurse into normal properties/children.
//		Prototype g = a.ShallowClone();

//		// Intersect normal properties.
//		// (Keeping only shared keys avoids inventing structure that doesn't exist in all examples.)
//		HashSet<int> keysA = a.NormalProperties.Select(x => x.Key).ToHashSet();
//		HashSet<int> keysB = b.NormalProperties.Select(x => x.Key).ToHashSet();

//		foreach (int key in keysA.Intersect(keysB))
//		{
//			Prototype va = a.Properties[key];
//			Prototype vb = b.Properties[key];
//			g.Properties[key] = AntiUnify(va, vb);
//		}

//		// Align children by index (ProtoScript/C# AST lists are positional).
//		int n = Math.Min(a.Children.Count, b.Children.Count);
//		for (int i = 0; i < n; i++)
//		{
//			g.Children.Add(AntiUnify(a.Children[i], b.Children[i]));
//		}

//		return g;
//	}

//	private List<Prototype> CollectEntityPaths(Prototype shadow)
//	{
//		// PrototypeGraphs.GetLeafPaths returns "path prototypes" you can use with PrototypeGraphs.GetValue(root, path).
//		List<Prototype> allLeafPaths = PrototypeGraphs.GetLeafPaths(shadow);

//		List<Prototype> entityPaths = new List<Prototype>();

//		foreach (Prototype path in allLeafPaths)
//		{
//			Prototype leaf = PrototypeGraphs.GetValue(shadow, path);
//			if (leaf != null && leaf.ShallowEqual(Entity.Prototype))
//			entityPaths.Add(path);
//		}

//		return entityPaths;
//	}

//	private int CountConcreteStringLeaves(Prototype root)
//	{
//		// "Concrete string leaf" means: a NativeValuePrototype whose NativeValue is string, and is NOT punctuation-ish.
//		// This is a cheap proxy for "has sentinel information" so we don't learn blank-statement patterns.
//		int count = 0;

//		foreach (Prototype leaf in GetLeaves(root, stopAtHidden: false))
//		{
//			if (leaf is NativeValuePrototype nv && nv.NativeValue is string s)
//			{
//				// crude filters; tune later
//				if (string.IsNullOrWhiteSpace(s))
//				continue;
//				if (s.Length == 1)
//				continue;

//				count++;
//			}
//		}

//		return count;
//	}

//	/// <summary>
//	/// Leaf-anchored candidate discovery:
//	/// This is the "elegant emergence" piece you liked in GetEntityPathsByLeaf2.
//	/// We are not clustering by a precomputed shape-signature.
//	///
//	/// Mechanically:
//	///   - collect leaves (treat learned hidden nodes as leaves when stopAtHidden=true),
//	///   - for each leaf, walk up to root,
//	///   - at each ancestor, ask: "does this subgraph occur elsewhere in the fileGraph (by categorization)?"
//	///   - keep only the longest repeated ancestor on each chain,
//	///   - group by template-hash into CandidateFamily objects.
//	/// </summary>
//	private List<CandidateFamily> DiscoverCandidateFamiliesByLeaf(Prototype fileGraph, bool stopAtHidden)
//	{
//		fileGraph.SetParents();

//		List<Prototype> leaves = GetLeaves(fileGraph, stopAtHidden);

//		// We'll hold candidate *nodes* (references into fileGraph) first.
//		List<Prototype> candidateNodes = new List<Prototype>();

//		foreach (Prototype leaf in leaves)
//		{
//			if (!IsKeyLeaf(leaf))
//			continue;

//			// Walk up to root, looking for "the longest ancestor that repeats elsewhere".
//			List<Prototype> chain = GetPathToRoot(leaf);

//			foreach (Prototype candidate in chain)
//			{
//				// Skip pure holes; they match everything and aren't useful by themselves.
//				if (candidate.ShallowEqual(Entity.Prototype))
//				continue;

//				// Skip patterns that are too small to matter (but keep walking upward).
//				int size = GetNodeSize(candidate);
//				if (size < Options.MinNodeSize)
//				continue;

//				// If we've already recorded this exact node as a candidate, stop climbing this chain.
//				// This matches the behavior in your original GetEntityPathsByLeaf2.
//				if (candidateNodes.Any(x => ReferenceEquals(x, candidate)))
//				break;

//				// Find other occurrences in the graph.
//				// NOTE:
//				// We deliberately use TemporaryPrototypeCategorization here (your existing matching semantics),
//				// not strict AreEqual; this is the "hidden hierarchy smoothing" hook.
//				List<Prototype> occs = FindOccurrences(fileGraph, candidate, stopAtHidden);

//				if (occs.Count < Options.MinSupport)
//				{
//					// Nothing else matches at this level; don't keep going further up.
//					// Same as your original: once no match found, higher parents won't match either.
//					break;
//				}

//				// Keep only the longest repeated node along this chain:
//				// remove any previously kept node whose parent is this candidate.
//				candidateNodes.RemoveAll(x => ReferenceEquals(x.Parent, candidate));
//				candidateNodes.Add(candidate);
//			}
//		}

//		// Convert candidate nodes into families grouped by template hash.
//		Dictionary<string, CandidateFamily> byHash = new Dictionary<string, CandidateFamily>();

//		foreach (Prototype candidate in candidateNodes)
//		{
//			Prototype template = candidate.Clone();
//			string hash = PrototypeGraphs.GetHash(template);

//			if (!byHash.TryGetValue(hash, out CandidateFamily? fam))
//			{
//				fam = new CandidateFamily(template);
//				byHash[hash] = fam;
//			}

//			// For each family, gather occurrences using the shared template.
//			// (We do this once per family hash rather than once per candidate node.)
//		}

//		foreach (CandidateFamily fam in byHash.Values)
//		{
//			fam.Occurrences = FindOccurrences(fileGraph, fam.Template, stopAtHidden);
//			fam.Support = fam.Occurrences.Count;
//			fam.TemplateSize = GetNodeSize(fam.Template);
//		}

//		// Filter out weak families.
//		return byHash.Values
//		.Where(x => x.Support >= Options.MinSupport)
//		.Where(x => x.TemplateSize >= Options.MinNodeSize)
//		.ToList();
//	}

//	private List<Prototype> FindOccurrences(Prototype fileGraph, Prototype template, bool stopAtHidden)
//	{
//		List<Prototype> occs = new List<Prototype>();

//		foreach (Prototype node in EnumerateNodes(fileGraph, stopAtHidden))
//		{
//			// Don't match the template object reference itself if it happens to be in the live graph.
//			if (ReferenceEquals(node, template))
//			continue;

//			if (TemporaryPrototypeCategorization.IsCategorized(node, template))
//			occs.Add(node);
//		}

//		return occs;
//	}

//	private bool IsKeyLeaf(Prototype leaf)
//	{
//		// We treat concrete strings as the main leaf sentinels in pass 0.
//		// In later passes, learned hidden nodes appear as leaves (stop nodes) and will be included by GetLeaves(...).
//		if (Prototypes.TypeOf(leaf, System_String.Prototype))
//		{
//			if (leaf is NativeValuePrototype nv && nv.NativeValue is string s)
//			{
//				// Filter punctuation-only tokens early; they create tons of useless matches.
//				if (string.IsNullOrWhiteSpace(s))
//				return false;
//				if (s.Length == 1)
//				return false;
//			}

//			return true;
//		}

//		// If the leaf is already a hole, it can be a key anchor in later passes.
//		if (leaf.ShallowEqual(Entity.Prototype) || Prototypes.TypeOf(leaf, Entity.Prototype))
//		return true;

//		// Learned hidden nodes are also valid anchors (they are your "HCP-as-entity" idea).
//		if (IsStopNode(leaf, stopAtHidden: true))
//		return true;

//		return false;
//	}

//	private bool IsStopNode(Prototype node, bool stopAtHidden)
//	{
//		if (!stopAtHidden)
//		return false;

//		// Compare.Entity is a universal "hole" / wildcard.
//		if (node.ShallowEqual(Entity.Prototype) || Prototypes.TypeOf(node, Entity.Prototype))
//		return true;

//		// If your solution has a shared hidden base prototype, treat those as atomic too.
//		if (Prototypes.TypeOf(node, Hidden.Base.Prototype))
//		return true;

//		// Overlay typing: if the node has any of the induced hidden-base types, stop here.
//		Collection types = TypeOfs.Get(node);
//		foreach (Prototype t in types.Children)
//		{
//			if (_stopTypeIds.Contains(t.PrototypeID))
//			return true;
//		}

//		return false;
//	}

//	private List<Prototype> GetLeaves(Prototype root, bool stopAtHidden)
//	{
//		List<Prototype> leaves = new List<Prototype>();
//		HashSet<Prototype> visited = new HashSet<Prototype>(RefEq.Instance);

//		void Walk(Prototype n)
//		{
//			if (n == null)
//			return;

//			if (!visited.Add(n))
//			return;

//			// If we are stopping at hidden, then "hidden nodes behave like leaves"
//			// even if they still have structure under them.
//			if (IsStopNode(n, stopAtHidden))
//			{
//				leaves.Add(n);
//				return;
//			}

//			bool hasNormalChildren = false;

//			foreach (Prototype c in n.Children)
//			{
//				hasNormalChildren = true;
//				Walk(c);
//			}

//			foreach (KeyValuePair<int, Prototype> pair in n.NormalProperties)
//			{
//				if (pair.Value == null)
//				continue;

//				hasNormalChildren = true;
//				Walk(pair.Value);
//			}

//			if (!hasNormalChildren)
//			leaves.Add(n);
//		}

//		Walk(root);
//		return leaves;
//	}

//	private IEnumerable<Prototype> EnumerateNodes(Prototype root, bool stopAtHidden)
//	{
//		HashSet<Prototype> visited = new HashSet<Prototype>(RefEq.Instance);
//		Stack<Prototype> stack = new Stack<Prototype>();
//		stack.Push(root);

//		while (stack.Count > 0)
//		{
//			Prototype node = stack.Pop();
//			if (node == null)
//			continue;

//			if (!visited.Add(node))
//			continue;

//			yield return node;

//			// If we stop at hidden, do not traverse into its internals.
//			if (IsStopNode(node, stopAtHidden))
//			continue;

//			foreach (Prototype c in node.Children)
//			stack.Push(c);

//			foreach (KeyValuePair<int, Prototype> pair in node.NormalProperties)
//			{
//				if (pair.Value != null)
//				stack.Push(pair.Value);
//			}
//		}
//	}

//	private List<Prototype> GetPathToRoot(Prototype prototype)
//	{
//		List<Prototype> lstPath = new List<Prototype>();

//		while (prototype != null)
//		{
//			lstPath.Add(prototype);
//			prototype = prototype.Parent;
//		}

//		return lstPath;
//	}

//	private sealed class CandidateFamily
//	{
//		public CandidateFamily(Prototype template)
//		{
//			Template = template;
//		}

//		public Prototype Template { get; }

//		public List<Prototype> Occurrences { get; set; } = new();

//		public int Support { get; set; }

//		public int TemplateSize { get; set; }
//	}

//	private sealed class RefEq : IEqualityComparer<Prototype>
//	{
//		public static RefEq Instance { get; } = new();

//		public bool Equals(Prototype? x, Prototype? y) => ReferenceEquals(x, y);

//		public int GetHashCode(Prototype obj) => RuntimeHelpers.GetHashCode(obj);
//	}
//}
