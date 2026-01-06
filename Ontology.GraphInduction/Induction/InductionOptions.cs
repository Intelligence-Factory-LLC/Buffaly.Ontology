namespace Ontology.GraphInduction.Induction;

public sealed class InductionOptions
{
	// Namespace used when creating hidden prototypes / learned types.
	public string HiddenNamespace { get; set; } = "CSharp.Code.Hidden";

	// Minimum number of occurrences before we consider something learnable.
	public int MinSupport { get; set; } = 3;

	// Iterative passes over the same graph using overlay typing (Option B).
	public int MaxPasses { get; set; } = 3;

	// Hard cap to avoid huge compare/generalization costs on massive families.
	public int MaxExamplesPerFamily { get; set; } = 50;

	// Ignore very small nodes to reduce noise early.
	public int MinNodeSize { get; set; } = 8;

	// Dirichlet smoothing for online scoring.
	public double DirichletAlpha { get; set; } = 0.5;

	// EMA smoothing for online gain.
	public double GainEmaAlpha { get; set; } = 0.2;
}
