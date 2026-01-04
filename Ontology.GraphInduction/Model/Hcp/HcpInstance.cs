using System.Collections.Generic;
using Ontology;

namespace Ontology.GraphInduction.Model.Hcp;

public sealed class HcpInstance
{
	public required HcpPattern Pattern { get; init; }

	// The occurrence root in the original graph (not cloned).
	public required Prototype OccurrenceRoot { get; init; }

	// Slot index -> value prototype extracted from occurrence.
	public required List<Prototype?> Slots { get; init; }

	// Optional scope key (file/method) for scoring.
	public string? ScopeKey { get; init; }
}
