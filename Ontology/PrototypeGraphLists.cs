namespace Ontology
{
	public class PrototypeGraphLists
	{
		static public List<Prototype> Clone(List<Prototype> prototypes)
		{
			List<Prototype> lstCloned = new List<Prototype>();
			foreach (Prototype prototype in prototypes)
			{
				lstCloned.Add(prototype.Clone());
			}
			return lstCloned;
		}



        private static List<Prototype> GetCommonRoots(Prototype prototype, List<Prototype> prototypes)
        {
            List<Prototype> lstResults = new List<Prototype>();

            if (prototypes.Any(x => x == null))
                return lstResults;

            bool bIsTypeOf = true;
            for (int i = 1; i < prototypes.Count && bIsTypeOf; i++)
            {
                if (!Prototypes.TypeOf(prototypes[i], prototype))
                    bIsTypeOf = false;
            }

			if (bIsTypeOf)
			{
				if (!lstResults.Any(x => Prototypes.AreShallowEqual(x, prototype)))
					lstResults.Add(prototype.ShallowClone());
			}

            foreach (int protoParent in prototype.GetParents())
            {
				List<Prototype> lstParentRoots = GetCommonRoots(Prototypes.GetPrototype(protoParent), prototypes);
				foreach (Prototype protoParentRoot in lstParentRoots)
				{
					if (!lstResults.Any(x => Prototypes.AreShallowEqual(x, protoParentRoot)))
						lstResults.Add(protoParentRoot);
				}
            }

            return lstResults;
        }

		public static List<List<Prototype>> MinusValues(List<Prototype> lstSources, List<Prototype> lstShadows)
		{
			List<List<Prototype>> lstResults = new List<List<Ontology.Prototype>>();

			for (int i = 0; i < lstSources.Count; i++)
			{
				lstResults.Add(PrototypeGraphs.MinusValues(lstSources[i], lstShadows[i]));
			}

			return lstResults;
		}

		public static List<List<Prototype>> Minus(List<Prototype> lstSources, List<Prototype> lstShadows)
		{
			List<List<Prototype>> lstResults = new List<List<Ontology.Prototype>>();

			for (int i = 0; i < lstSources.Count; i++)
			{
				lstResults.Add(PrototypeGraphs.Minus(lstSources[i], lstShadows[i], true));
			}

			return lstResults;
		}

		public static bool TypeOf(List<Prototype> lstSources, List<Prototype> lstTargets)
		{
			for (int i = 0; i < lstSources.Count; i++)
			{
				if (!Prototypes.TypeOf(lstSources[i], lstTargets[i]))
					return false; 
			}

			return true; 
		}

		public static bool TypeOf(List<Prototype> lstSources, Prototype protoTarget)
		{
			for (int i = 0; i < lstSources.Count; i++)
			{
				if (!Prototypes.TypeOf(lstSources[i], protoTarget))
					return false;
			}

			return true;
		}

		static public List<Prototype> SelectIndex(List<List<Prototype>> lstCollection, int iIndex)
		{
			List<Prototype> lstPrototypes = new List<Prototype>();
			foreach (List<Prototype> prototypes in lstCollection)
			{
				lstPrototypes.Add(prototypes[iIndex]);
			}
			return lstPrototypes;
		}

		static public bool AreEqual(List<Prototype> protoInputs)
		{
			for (int i = 0; i < protoInputs.Count - 1; i++)
			{
				Prototype protoInput = protoInputs[i];
				Prototype protoOutput = protoInputs[i +1 ];

				if (null == protoInput || null == protoOutput || !PrototypeGraphs.AreEqual(protoInput, protoOutput))
					return false;
			}

			return true;
		}
		static public bool AreEqual(List<Prototype> protoInputs, List<Prototype> protoOutputs)
		{
			if (protoInputs.Count != protoOutputs.Count)
				throw new Exception("Input and Output lists must be the same size");

			for (int i = 0; i < protoInputs.Count; i++)
			{
				Prototype protoInput = protoInputs[i];
				Prototype protoOutput = protoOutputs[i];

				if (null == protoInput || null == protoOutput || !PrototypeGraphs.AreEqual(protoInput, protoOutput, false))
					return false;
			}

			return true;
		}

	}
}
