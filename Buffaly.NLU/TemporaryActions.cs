using Ontology;
using ProtoScript.Interpretter;
using ProtoScript.Interpretter.RuntimeInfo;

namespace Buffaly.NLU
{
	public class TemporaryAction
	{
		public Prototype Source;
		public Prototype Target;
		public Prototype Dimension; 
		public FunctionRuntimeInfo Function;

		public override string ToString()
		{
			return $"{Source.PrototypeName} -> {Target?.PrototypeName ?? "Prototype"} ({Function?.FunctionName ?? "None"})";
		}
	}

	public class TemporaryActions
	{
		// Define a class to encapsulate the list and its associated lock
		private class ActionList
		{
			public List<TemporaryAction> Actions { get; } = new List<TemporaryAction>();
			public object LockObject { get; } = new object();
		}

		// AsyncLocal holds an instance of ActionList per async context
		private static readonly AsyncLocal<ActionList> m_Actions = new AsyncLocal<ActionList>();

		private static ActionList Actions
		{
			get
			{
				if (m_Actions.Value == null)
				{
					m_Actions.Value = new ActionList();
				}
				return m_Actions.Value;
			}
		}

		//>create a clear method to clear the list of actions

		public static void Clear()
		{
			lock (Actions.LockObject)
			{
				Actions.Actions.Clear();
			}
		}

		//>add a Where method to run the Where method on the list of actions safely and return a List
		public static List<TemporaryAction> Where(Func<TemporaryAction, bool> predicate)
		{
			lock (Actions.LockObject)
			{
				return Actions.Actions.Where(predicate).ToList();
			}
		}

		// Inserts a temporary action without dimension
		public static void InsertAction(FunctionRuntimeInfo function)
		{
			if (function.Parameters.Count != 1)
				throw new RuntimeException("The function passed to InsertAction requires 1 parameter", function.Info);

			Prototype protoSource = (function.Parameters[0].Type as PrototypeTypeInfo).Prototype;
			Prototype protoTarget = (function.ReturnType as PrototypeTypeInfo).Prototype;
			InsertOrUpdateTemporaryAction(protoSource, protoTarget, function);
		}

		// Inserts a temporary action with dimension
		public static void InsertActionWithDimension(FunctionRuntimeInfo function, Prototype protoDimension)
		{
			// Uncomment and adjust parameter count check as needed
			// if (function.Parameters.Count != 1)
			//     throw new RuntimeException("The function passed to InsertActionWithDimension requires 1 parameter", function.Info);

			Prototype protoSource = null;
			if (function.Parameters.Count == 1)
				protoSource = (function.Parameters[0].Type as PrototypeTypeInfo).Prototype;
			else
				protoSource = function.ParentPrototype;

			Prototype protoTarget = null;
			if (function.ReturnType is PrototypeTypeInfo prototypeTypeInfo)
				protoTarget = prototypeTypeInfo.Prototype;
			else if (function.ReturnType.Type == typeof(Ontology.Collection))
				protoTarget = Ontology.Collection.Prototype;
			else
			{
				// It's a Prototype here, but we can't create a base prototype 
				// protoTarget = new Prototype();
				// Handle as needed
			}

			if (Prototypes.AreShallowEqual(protoSource, protoTarget))
				throw new Exception("Using the same source and target without a subtype will cause an infinite loop");

			InsertOrUpdateTemporaryActionWithDimension(protoSource, protoTarget, protoDimension, function);
		}

		// Inserts or updates a temporary action without dimension
		public static void InsertOrUpdateTemporaryAction(Prototype protoSource, Prototype protoTarget, FunctionRuntimeInfo function)
		{
			var action = new TemporaryAction
			{
				Source = protoSource,
				Target = protoTarget,
				Function = function
			};

			var actionList = Actions;
			lock (actionList.LockObject)
			{
				int iIndex = actionList.Actions.FindIndex(x =>
					Prototypes.AreShallowEqual(x.Source, protoSource) &&
					string.Equals(function?.FunctionName, x.Function?.FunctionName, StringComparison.Ordinal));

				if (iIndex >= 0)
					actionList.Actions[iIndex] = action;
				else
					actionList.Actions.Add(action);
			}
		}

		// Inserts or updates a temporary action with dimension
		public static void InsertOrUpdateTemporaryActionWithDimension(Prototype protoSource, Prototype protoTarget, Prototype protoDimension, FunctionRuntimeInfo function)
		{
			var action = new TemporaryAction
			{
				Source = protoSource,
				Target = protoTarget,
				Dimension = protoDimension,
				Function = function
			};

			var actionList = Actions;
			lock (actionList.LockObject)
			{
				int iIndex = actionList.Actions.FindIndex(x =>
					Prototypes.AreShallowEqual(x.Source, protoSource) &&
					Prototypes.AreShallowEqual(x.Dimension, protoDimension) &&
					string.Equals(function?.FunctionName, x.Function?.FunctionName, StringComparison.Ordinal) &&
					string.Equals(function?.ParentPrototype?.PrototypeName, x.Function?.ParentPrototype?.PrototypeName, StringComparison.Ordinal));

				if (iIndex >= 0)
					actionList.Actions[iIndex] = action;
				else
					actionList.Actions.Add(action);
			}
		}

		// Retrieves temporary actions based on protoSource
		public static List<TemporaryAction> GetTemporaryActions(Prototype protoSource)
		{
			var actionList = Actions;
			lock (actionList.LockObject)
			{
				return actionList.Actions.Where(x => Prototypes.TypeOf(protoSource, x.Source)).ToList();
			}
		}

		// Retrieves temporary actions based on protoSource and protoDimension
		public static List<TemporaryAction> GetTemporaryActionsWithDimension(Prototype protoSource, Prototype protoDimension)
		{
			var actionList = Actions;
			lock (actionList.LockObject)
			{
				return actionList.Actions.Where(x =>
					Prototypes.TypeOf(protoSource, x.Source) &&
					Prototypes.TypeOf(x.Dimension, protoDimension)).ToList();
			}
		}

		// Executes a temporary action
		public static Prototype RunAction(TemporaryAction action, Prototype protoSource, NativeInterpretter interpretter)
		{
			object obj = null;

			if (action.Function == null)
				obj = action.Target;
			else if (action.Function.Parameters.Count == 1)
				obj = interpretter.RunMethod(action.Function, null, new List<object> { protoSource });
			else
				obj = interpretter.RunMethod(action.Function, protoSource, new List<object>());

			return interpretter.GetAsPrototype(obj);
		}
	}

}
