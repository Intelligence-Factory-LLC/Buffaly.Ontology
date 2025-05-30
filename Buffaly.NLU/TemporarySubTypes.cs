using Ontology;
using ProtoScript.Interpretter;
using ProtoScript.Interpretter.RuntimeInfo;

namespace Buffaly.NLU
{
	public class TemporarySubType
	{
		public Prototype SuperType;
		public FunctionRuntimeInfo Function;
		public Prototype SubType;

		public override string ToString()
		{
			return $"TemporarySubType[{SuperType.PrototypeName} -> {SubType.PrototypeName}]";
		}
	}

	public class TemporarySubTypes
	{
		// Private nested class to encapsulate the list and its associated lock
		private class SubTypeList
		{
			public List<TemporarySubType> SubTypes { get; } = new List<TemporarySubType>();
			public object LockObject { get; } = new object();
		}

		// AsyncLocal holds an instance of SubTypeList per async context
		private static readonly AsyncLocal<SubTypeList> m_SubTypes = new AsyncLocal<SubTypeList>();

		private static SubTypeList SubTypes
		{
			get
			{
				if (m_SubTypes.Value == null)
				{
					m_SubTypes.Value = new SubTypeList();
				}
				return m_SubTypes.Value;
			}
		}

		//>Create a Clear method to clear the list of subtypes
		public static void Clear()
		{
			lock(SubTypes.LockObject)
			{
				SubTypes.SubTypes.Clear();
			}
		}

		//>add a Where method to run the Where method on the list of subtypes safely and return a List
		public static List<TemporarySubType> Where(Predicate<TemporarySubType> predicate)
		{
			lock (SubTypes.LockObject)
			{
				return SubTypes.SubTypes.Where(predicate.Invoke).ToList();
			}
		}

		// Inserts a subtype without dimension
		public static void InsertSubType(Prototype protoSubType, NativeInterpretter interpretter)
		{
			if (protoSubType == null)
				throw new ArgumentNullException(nameof(protoSubType));

			if (interpretter == null)
				throw new ArgumentNullException(nameof(interpretter));

			// Retrieve the PrototypeTypeInfo from the global scope
			PrototypeTypeInfo info = interpretter.Symbols.GetGlobalScope().GetSymbol(protoSubType.PrototypeName) as PrototypeTypeInfo;
			if (info == null)
				throw new Exception($"PrototypeTypeInfo not found for prototype '{protoSubType.PrototypeName}'.");

			// Retrieve the categorization function
			FunctionRuntimeInfo infoFunc = info.Scope.GetSymbol("IsCategorized") as FunctionRuntimeInfo;
			if (infoFunc == null)
				throw new Exception($"SubType requires a categorization function: {protoSubType.PrototypeName}");

			// Determine the super type
			int ? protoSuperType = protoSubType.GetTypeOfs().FirstOrDefault();
			if (protoSuperType == null)
				throw new Exception($"SubType must inherit from a primary prototype: {protoSubType.PrototypeName}");

			InsertOrUpdateTemporarySubType(Prototypes.GetPrototype(protoSuperType.Value), protoSubType, infoFunc);
		}

		// Inserts or updates a temporary subtype without dimension
		public static void InsertOrUpdateTemporarySubType(Prototype protoSource, Prototype protoTarget, FunctionRuntimeInfo function)
		{
			if (protoSource == null)
				throw new ArgumentNullException(nameof(protoSource));
			if (protoTarget == null)
				throw new ArgumentNullException(nameof(protoTarget));
			if (function == null)
				throw new ArgumentNullException(nameof(function));

			var subType = new TemporarySubType
			{
				SuperType = protoSource,
				SubType = protoTarget,
				Function = function
			};

			var subTypeList = SubTypes;
			lock (subTypeList.LockObject)
			{
				int iIndex = subTypeList.SubTypes.FindIndex(x =>
					Prototypes.AreShallowEqual(x.SuperType, protoSource) &&
					Prototypes.AreShallowEqual(x.SubType, protoTarget));

				if (iIndex >= 0)
					subTypeList.SubTypes[iIndex] = subType;
				else
					subTypeList.SubTypes.Add(subType);
			}
		}

		// Retrieves potential subtypes based on protoSource
		public static List<TemporarySubType> GetPotentialSubTypes(Prototype protoSource)
		{
			if (protoSource == null)
				throw new ArgumentNullException(nameof(protoSource));

			var subTypeList = SubTypes;
			lock (subTypeList.LockObject)
			{
				return subTypeList.SubTypes
					.Where(x => Prototypes.TypeOf(protoSource, x.SuperType))
					.ToList();
			}
		}

		// Categorizes a prototype as a subtype using the associated function
		public static bool CategorizeAsSubType(Prototype protoSource, TemporarySubType subType, NativeInterpretter interpretter)
		{
			if (protoSource == null)
				throw new ArgumentNullException(nameof(protoSource));
			if (subType == null)
				throw new ArgumentNullException(nameof(subType));
			if (interpretter == null)
				throw new ArgumentNullException(nameof(interpretter));

			object obj;
			
			//allow use to call this as IsCategorized(protoSource) or protoSource.IsCategorized()
			if (subType.Function.Parameters.Count == 0)
				obj = interpretter.RunMethod(subType.Function, protoSource, new List<object> {  });
			else
				obj = interpretter.RunMethod(subType.Function, null, new List<object> { protoSource });

			if (obj == null)
				throw new RuntimeException("Categorization method did not return a boolean", subType.Function.Info);

			if (!(obj is bool result))
				throw new RuntimeException("Categorization method did not return a boolean", subType.Function.Info);

			return result;
		}
	}

}
