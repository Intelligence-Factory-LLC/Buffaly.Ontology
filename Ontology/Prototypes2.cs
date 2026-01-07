using BasicUtilities;
using BasicUtilities.Collections;
using Ontology.BaseTypes;
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

		static public object ? FromPrototype(Prototype prototype)
		{
			return FromPrototypeByReflection(prototype, new Map<int, object>());
		}


		static private object? FromPrototypeByReflection(Prototype prototype, Map<int, object> m_mapPrototypeToObjects)
		{
			if (prototype == null)
				return null;

			object existing;
			if (m_mapPrototypeToObjects.TryGetValue(prototype.PrototypeID, out existing))
				return existing;

			// Recover the CLR type name from "Namespace.Type[...]" or "Namespace.Type"
			string strTypeName = prototype.PrototypeName;
			int idx = strTypeName.IndexOf('[');
			if (idx >= 0)
				strTypeName = strTypeName.Substring(0, idx);

			System.Type type = ResolveTypeByFullName(strTypeName);
			if (type == null)
				throw new Exception("Could not resolve type from prototype name: " + strTypeName);

			// Mirror your ToPrototype policy: block System/Microsoft non-atoms
			string ns = type.Namespace;
			if (ns != null && (ns.StartsWith("System", StringComparison.Ordinal) || ns.StartsWith("Microsoft", StringComparison.Ordinal)))
			{
				// Atoms were already handled by FromPrototype(...) before calling reflection.
				return null;
			}

			object? obj;
			if (type == typeof(string))
				obj = string.Empty;
			else
				obj = Activator.CreateInstance(type);

			// circular break
			m_mapPrototypeToObjects[prototype.PrototypeID] = obj;

			// Populate members based on the property-key prototypes you generated:
			//   "{TypeFullName}.Property.{Name}" and "{TypeFullName}.Field.{Name}"
			foreach (var pair in prototype.Properties)
			{
				int iKey = pair.Key;
				Prototype protoKey = Prototypes.GetPrototype(iKey);
				string strKeyName = protoKey.PrototypeName;

				// Only accept keys that are in this type's namespace (or nested types that used the same base)
				// You can relax this if you intentionally allow cross-type keys.
				if (!strKeyName.StartsWith(strTypeName + ".", StringComparison.Ordinal))
					continue;

				Prototype protoValue = pair.Value;
				object oValue = FromPrototypeCircular(protoValue, m_mapPrototypeToObjects);

				string prefixProp = strTypeName + ".Property.";
				string prefixField = strTypeName + ".Field.";

				if (strKeyName.StartsWith(prefixProp, StringComparison.Ordinal))
				{
					string propName = strKeyName.Substring(prefixProp.Length);
					ReflectionUtil.SetPropertyOrIgnore(obj, oValue, propName);
				}
				else if (strKeyName.StartsWith(prefixField, StringComparison.Ordinal))
				{
					string fieldName = strKeyName.Substring(prefixField.Length);
					ReflectionUtil.SetFieldOrIgnore(obj, oValue, fieldName);
				}
			}

			// If the object is a list and the prototype has children, populate the list.
			if (obj is System.Collections.IList list && prototype.Children.Count > 0)
			{
				foreach (Prototype child in prototype.Children)
				{
					object oChild = FromPrototypeCircular(child, m_mapPrototypeToObjects);
					if (oChild != null)
						list.Add(oChild);
				}
			}

			return obj;
		}

		static private object FromPrototypeCircular(Prototype prototype, Map<int, object> m_mapPrototypeToObjects)
		{
			if (prototype == null)
				return null;

			// circular break by prototype id (graph identity)
			object existing;
			if (m_mapPrototypeToObjects.TryGetValue(prototype.PrototypeID, out existing))
				return existing;

			if (prototype is NativeValuePrototype nvp)
			{
				if (nvp.NativeValue is string)
				{
					return nvp.NativeValue;
				}
				if (nvp.NativeValue is int)
				{
					return nvp.NativeValue;
				}
				if (nvp.NativeValue is bool)
				{
					return nvp.NativeValue;
				}
				if (nvp.NativeValue is double)
				{
					return nvp.NativeValue;
				}
			}

			// Base-type instances encoded via PrototypeName "System.String[...]" etc.
			// Prefer your existing native-value path when possible.
			if (prototype.IsInstance())
			{
				if (prototype.TypeOf(System_String.Prototype))
					return StringUtil.Between(prototype.PrototypeName, "[", "]");

				if (prototype.TypeOf(System_Int32.Prototype))
					return Convert.ToInt32(StringUtil.Between(prototype.PrototypeName, "[", "]"));

				if (prototype.TypeOf(System_Boolean.Prototype))
					return Convert.ToBoolean(StringUtil.Between(prototype.PrototypeName, "[", "]"));

				if (prototype.TypeOf(System_Double.Prototype))
					return Convert.ToDouble(StringUtil.Between(prototype.PrototypeName, "[", "]"));
			}


			// Collections
			if (prototype.TypeOf(Ontology.Collection.Prototype))
			{
				var lst = new List<object>();
				m_mapPrototypeToObjects[prototype.PrototypeID] = lst;

				foreach (Prototype child in prototype.Children)
				{
					object oChild = FromPrototypeCircular(child, m_mapPrototypeToObjects);
					if (oChild != null)
						lst.Add(oChild);
				}

				return lst;
			}

			// Otherwise: treat as an object-like prototype and reconstruct by reflection
			// (If it isn't a NativeValuePrototype, we still can attempt if its name maps to a CLR type.)
			if (prototype is not NativeValuePrototype)
			{
				// If you want to strictly require NativeValuePrototype for reflection objects, throw here.
				// For now, attempt to wrap it.
				NativeValuePrototype wrapper = prototype as NativeValuePrototype;
				if (wrapper == null)
					throw new Exception("Cannot reflect FromPrototype for non-NativeValuePrototype: " + prototype.PrototypeName);
			}

			return FromPrototypeByReflection((NativeValuePrototype)prototype, m_mapPrototypeToObjects);
		}

		static private System.Type ResolveTypeByFullName(string fullName)
		{
			// Fast path: works if assembly-qualified or in mscorlib/System.Private.CoreLib for some.
			System.Type t = System.Type.GetType(fullName, throwOnError: false);
			if (t != null)
				return t;

			// Search loaded assemblies (this is what you need for domain types like CSharp.File, etc.)
			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					t = asm.GetType(fullName, throwOnError: false, ignoreCase: false);
					if (t != null)
						return t;
				}
				catch
				{
					// ignore reflection load issues
				}
			}

			return null;
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
		