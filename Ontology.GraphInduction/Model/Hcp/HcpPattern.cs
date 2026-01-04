using System.Collections.Generic;
using Ontology;

namespace Ontology.GraphInduction.Model.Hcp;

public sealed class HcpPattern
{
	// Stable identifier for the pattern. Use hash of shadow.
	public required string PatternId { get; init; }

	// The “holey shadow” (Compare.Entity holes) used for categorization.
	public required Prototype Shadow { get; init; }

	// The hidden base prototype that we attach as TypeOf during overlay typing.
	public required Prototype HiddenBase { get; init; }

	// Paths to holes in the shadow.
	public required List<Prototype> Paths { get; init; }

	// Optional: stable anchor leaf strings used for recognition indexing later.
	public List<string> AnchorStrings { get; init; } = new();
}
