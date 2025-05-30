//added
using BasicUtilities.Collections;
using Ontology;
using ProtoScript.Interpretter;
using ProtoScript.Interpretter.RuntimeInfo;
using Buffaly.NLU.Tagger;

namespace Buffaly.NLU.Nodes
{
	public class ControllerInfo
	{
		public Prototype Prototype;
		public FunctionRuntimeInfo StartFunction;
		public List<FunctionRuntimeInfo> ContinueFunctions = new List<FunctionRuntimeInfo>();
		public List<FunctionRuntimeInfo> ExitConditions = new List<FunctionRuntimeInfo>();
	}
	public class ProtoScriptControllers
	{
		static private Map<int, ControllerInfo> m_mapControllers = new Map<int, ControllerInfo>();

		public static ControllerInfo Insert(Prototype prototype)
		{
			if (null != GetOrNull(prototype))
				throw new Exception("Controller already exits: " + prototype.PrototypeName);

			ControllerInfo info = new ControllerInfo();
			info.Prototype = prototype;

			m_mapControllers[prototype.PrototypeID] = info;

			return info;
		}

		static public ControllerInfo GetOrNull(Prototype prototype)
		{
			ControllerInfo info = null;
			m_mapControllers.TryGetValue(prototype.PrototypeID, out info);

			return info;
		}

		static public ControllerInfo Get(Prototype prototype)
		{
			ControllerInfo info = GetOrNull(prototype);
			if (null == info)
				throw new Exception("Controller does not exist: " + prototype.PrototypeName);

			return info;
		}

		static public ControllerInfo StartFunction(FunctionRuntimeInfo f)
		{
			ControllerInfo info = Get(f.ParentPrototype);
			info.StartFunction = f;
			return info;
		}

		static public ControllerInfo ContinueFunction(FunctionRuntimeInfo f)
		{
			ControllerInfo info = Get(f.ParentPrototype);
			info.ContinueFunctions.Add(f);
			return info;
		}

		static public ControllerInfo ExitCondition(FunctionRuntimeInfo f)
		{
			ControllerInfo info = Get(f.ParentPrototype);
			info.ExitConditions.Add(f);
			return info;
		}


		static public ProtoScriptControlNode Create(Prototype controller, Prototype source, NativeInterpretter interpretter)
		{
			ControllerInfo info = ProtoScriptControllers.Get(controller);
			StartNode startNode = new StartNode(source, info, interpretter);
			return startNode;
		}

		static public void Clear()
		{
			m_mapControllers.Clear();
		}

		static public Prototype Tag(Prototype controller, Prototype source, ProtoScriptTagger tagger)
		{
			ProtoScriptControlNode node = Create(controller, source, tagger.Interpretter);

			var res = tagger.Tag2(node);

			if (res is SingleResult)
				return (res as SingleResult).Result;

			return null;
		}

		static public Collection TagAll(Prototype controller, Prototype source, ProtoScriptTagger tagger)
		{
			PrototypeSet lstPrototypes = new PrototypeSet();

			ProtoScriptControlNode node = Create(controller, source, tagger.Interpretter);

			do
			{

				var res = tagger.Tag2(node);

				if (res is SingleResult)
					lstPrototypes.Add((res as SingleResult).Result);

				else
					break;
			}
			while (true);

			return new Collection(lstPrototypes);
		}


	}
}
