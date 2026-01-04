using System.Collections.Generic;
using System.Linq;
using Ontology;
using Ontology.GraphInduction.Model.Hcp;
using Ontology.GraphInduction.Signatures;

namespace Ontology.GraphInduction.Induction;

public sealed class GraphInductionEngine
{
	public InductionOptions Options { get; } = new();

	// Learned patterns keyed by PatternId
	private readonly Dictionary<string, HcpPattern> _patterns = new();

	public IReadOnlyDictionary<string, HcpPattern> Patterns => _patterns;

	public List<HcpPattern> MineFileGraph(Prototype fileGraph)
	{
		fileGraph.SetParents();

		List<HcpPattern> learned = new List<HcpPattern>();

		for (int pass = 0; pass < Options.MaxPasses; pass++)
		{
			// 1) Group nodes by shape signature (stop at hidden on pass>0)
			bool stopAtHidden = pass > 0;

			Dictionary<string, List<Prototype>> buckets = new Dictionary<string, List<Prototype>>();

			PrototypeGraphs.DepthFirstOnNormal(fileGraph, node =>
			{
				if (stopAtHidden && Prototypes.TypeOf(node, Hidden.Base.Prototype))
					return node;

				int size = GetNodeSize(node);
				if (size < Options.MinNodeSize)
					return node;

				string sig = PrototypeShapeSignature.Compute(node, stopAtHidden);

				if (!buckets.TryGetValue(sig, out List<Prototype>? list))
				{
					list = new List<Prototype>();
					buckets[sig] = list;
				}

				list.Add(node);
				return node;
			});

			List<KeyValuePair<string, List<Prototype>>> candidates = buckets
				.Where(kv => kv.Value.Count >= Options.MinSupport)
				.OrderByDescending(kv => kv.Value.Count)
				.ToList();

			int learnedThisPass = 0;

			foreach (KeyValuePair<string, List<Prototype>> kv in candidates)
			{
				List<Prototype> examples = kv.Value.Take(Options.MaxExamplesPerFamily).Select(x => x.Clone()).ToList();

				HcpPattern? pattern = TryLearnPattern(examples);
				if (pattern == null)
					continue;

				if (_patterns.ContainsKey(pattern.PatternId))
					continue;

				_patterns[pattern.PatternId] = pattern;
				learned.Add(pattern);
				learnedThisPass++;

				// 2) Overlay typing: mark all occurrences as typeof hidden base
				foreach (Prototype occ in kv.Value)
				{
					occ.InsertTypeOf(pattern.HiddenBase.PrototypeID);
				}
			}

			if (learnedThisPass == 0)
				break;
		}

		return learned;
	}

	private int GetNodeSize(Prototype node)
	{
		int count = 0;
		PrototypeGraphs.DepthFirstOnNormal(node, x =>
		{
			count++;
			return x;
		});
		return count;
	}

	private HcpPattern? TryLearnPattern(List<Prototype> examples)
	{
		// Placeholder: Codex will fill in real implementation once we confirm
		// ComparePrototypes / Parameterize behavior in this solution.
		// For now, return null so the skeleton compiles.
		return null;
	}
}
