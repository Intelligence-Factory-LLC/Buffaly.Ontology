using Ontology.Simulation;
using ProtoScript.Interpretter;

namespace Ontology.Agents.Utilities
{
	//These methods should only be used in test methods since they are slower
	public static class PrototypePropertiesCollectionExtensions2
	{
		public static Prototype GetOrDefault2(this PrototypePropertiesCollection collection, string strPropertyName, Prototype defaultValue = null)
		{
			var tuple = SimpleInterpretter.ResolveProperty(collection.GetParent(), strPropertyName);
			var prototype = collection.GetOrNull(tuple.Item1);
			if (prototype != null)
			{
				// Assuming `prototype` can return an instance of `StringWrapper`
				return prototype;
			}

			return defaultValue;
		}
		public static string GetStringOrDefault2(this PrototypePropertiesCollection collection, string strPropertyName, string defaultValue = null)
		{
			var tuple = SimpleInterpretter.ResolveProperty(collection.GetParent(), strPropertyName);
			return collection.GetStringOrDefault(tuple.Item1, defaultValue);
		}
	}
}
