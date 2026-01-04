using System.Collections.Generic;
using Ontology;

namespace Ontology.GraphInduction.Model.Hidden;

public static class HiddenInstanceUtil
{
	public static Prototype CreateHiddenInstance(Prototype hiddenBase, IReadOnlyList<Prototype?> entities)
	{
		// Ensure it is a Hidden.Base type
		PrototypeParents.GetOrInsertPrototypeParent(hiddenBase, Hidden.Base.Prototype);

		Prototype inst = hiddenBase.Clone();

		for (int i = 0; i < entities.Count; i++)
		{
			Prototype field = Prototypes.GetOrInsertPrototype(hiddenBase.PrototypeName + ".Field." + i);
			inst.Properties[field.PrototypeID] = entities[i];
		}

		return inst;
	}
}
