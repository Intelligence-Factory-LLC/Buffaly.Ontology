using BasicUtilities.Collections;
using WebAppUtilities;

namespace Ontology
{
	public partial class Prototypes : JsonWs
	{
		static public bool AreShallowEqual(Prototype prototype1, Prototype prototype2)
		{
			if (null == prototype1 || null == prototype2)
			{
				return null == prototype1 && null == prototype2;
			}

			return prototype2.ShallowEqual(prototype1);
		}

		static public Prototype CloneCircular(Prototype prototype)
		{
			Map<int, Prototype> mapPrototypes = new Map<int, Prototype>();
			return CloneCircular(prototype, mapPrototypes);
		}

		static public Prototype CloneCircular(Prototype prototype, Map<int, Prototype> mapPrototypes)
		{
			int iHash = prototype.GetHashCode();
			Prototype copy = null;
			if (mapPrototypes.TryGetValue(iHash, out copy))
			{
				return copy;
			}

			copy = prototype.ShallowClone();

			mapPrototypes[iHash] = copy;

			copy.PrototypeID = prototype.PrototypeID;
			copy.PrototypeName = prototype.PrototypeName;
			copy.Value = prototype.Value;

			PrototypePropertiesCollection clone = new PrototypePropertiesCollection(copy);
			copy.Properties = clone;

			foreach (var pair in prototype.Properties)
			{
				if (pair.Value != null)
					clone[pair.Key] = CloneCircular(pair.Value, mapPrototypes);
				else
					clone[pair.Key] = null;
			}

			foreach (Prototype child in prototype.Children)
			{
				copy.Children.Add(CloneCircular(child, mapPrototypes));
			}

			return copy;
		}


		public static Prototype GetPrototype(int PrototypeID)
		{
			return TemporaryPrototypes.GetTemporaryPrototype(PrototypeID);
		}

		public static string GetPrototypeName(int PrototypeID)
		{
			return TemporaryPrototypes.GetTemporaryPrototype(PrototypeID).PrototypeName;
		}

		public static Prototype GetPrototypeByPrototypeName(string PrototypeName)
		{
			return TemporaryPrototypes.GetTemporaryPrototype(PrototypeName);
		}
		static public Prototype GetOrInsertPrototype(string PrototypeName, string PrototypeParentName)
		{
			Prototype protoParent = GetOrInsertPrototype(PrototypeParentName);
			return TemporaryPrototypes.GetOrCreateTemporaryPrototype(PrototypeName, protoParent);
		}

		static public Prototype GetOrInsertPrototype(string PrototypeName)
		{
			return TemporaryPrototypes.GetOrCreateTemporaryPrototype(PrototypeName);
		}

		static public bool TypeOf(int iPrototypeID, Prototype parent)
		{
			Prototype prototype = Prototypes.GetPrototype(iPrototypeID);

			return TypeOf(prototype, parent);
		}

		static public bool TypeOf(Prototype prototype, Prototype parent)
		{
			if (null == prototype || null == parent)
				return false;

			if (prototype.TypeOf(parent))
				return true;

			return (AreShallowEqual(prototype, parent));
		}
	}
}    
		