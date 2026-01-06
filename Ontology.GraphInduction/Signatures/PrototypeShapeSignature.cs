using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ontology;

namespace Ontology.GraphInduction.Signatures;

public static class PrototypeShapeSignature
{
	public static string Compute(Prototype prototype, bool stopAtHidden)
	{
		StringBuilder sb = new StringBuilder();
		Append(prototype, sb, stopAtHidden);
		return sb.ToString();
	}

	private static void Append(Prototype prototype, StringBuilder sb, bool stopAtHidden)
	{
		if (prototype == null)
		{
			sb.Append("null");
			return;
		}

		if (stopAtHidden && Prototypes.TypeOf(prototype, Hidden.Base.Prototype))
		{
			sb.Append("Hidden:").Append(prototype.PrototypeID);
			return;
		}

		Prototype p = prototype;

		sb.Append(p.PrototypeID);

		foreach (KeyValuePair<int, Prototype> pair in p.NormalProperties.OrderBy(x => x.Key))
		{
			sb.Append("|P").Append(pair.Key).Append(":");
			Append(pair.Value, sb, stopAtHidden);
		}

		sb.Append("|C[");
		for (int i = 0; i < p.Children.Count; i++)
		{
			Append(p.Children[i], sb, stopAtHidden);
			sb.Append(",");
		}
		sb.Append("]");
	}
}
