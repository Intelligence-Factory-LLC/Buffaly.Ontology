using Ontology.Simulation;
using System.Reflection;

namespace ProtoScript.Interpretter
{
	public class ReflectionUtil
	{
		static public MethodInfo GetMethod(System.Type type, string strMethodName, List<System.Type> lstParameters)
		{
			if (lstParameters.Any(x => x == null))
				return type.GetMethods().FirstOrDefault(x => x.Name == strMethodName && x.GetParameters().Length == lstParameters.Count);

			MethodInfo methodInfo = type.GetMethod(strMethodName, lstParameters.ToArray());
			if (null == methodInfo)
			{
				for (int i = 0; i < lstParameters.Count; i++)
				{
					System.Type typeParam = lstParameters[i];

					if (typeParam == typeof(IntWrapper))
						lstParameters[i] = typeof(int);

					if (typeParam == typeof(StringWrapper))
						lstParameters[i] = typeof(string);
				}

				methodInfo = type.GetMethod(strMethodName, lstParameters.ToArray());
			}

			return methodInfo;
		}

               static public bool HasBaseType(System.Type typeTarget, System.Type type)
               {
                       if (typeTarget == null)
                               return false;

                       return type.IsAssignableFrom(typeTarget);
               }

	}
}
