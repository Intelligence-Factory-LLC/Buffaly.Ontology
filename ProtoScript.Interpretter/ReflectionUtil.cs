using Ontology.Simulation;
using System.Reflection;

namespace ProtoScript.Interpretter
{
        public class ReflectionUtil
        {
                /// <summary>
                /// Enumerate all candidate methods that have the specified name and arity.
                /// </summary>
                static public IEnumerable<MethodInfo> GetCandidateMethods(System.Type type, string name, int arity)
                {
                        return type.GetMethods()
                                .Where(m => m.Name == name && m.GetParameters().Length == arity);
                }

                /// <summary>
                /// Try to resolve the best matching method for the provided argument types.
                /// </summary>
                static public MethodInfo? GetMethod(System.Type type, string strMethodName, List<System.Type> lstParameters)
                {
                        if (lstParameters.Any(x => x == null))
                                return GetCandidateMethods(type, strMethodName, lstParameters.Count).FirstOrDefault();

                        // First try an exact match using the runtime types.
                        MethodInfo? methodInfo = type.GetMethod(strMethodName, lstParameters.ToArray());
                        if (null != methodInfo)
                                return methodInfo;

                        // Attempt again after normalising wrappers to their primitive counterparts.
                        List<System.Type> normalized = new List<System.Type>(lstParameters);
                        for (int i = 0; i < normalized.Count; i++)
                        {
                                System.Type typeParam = normalized[i];

                                if (typeParam == typeof(IntWrapper))
                                        normalized[i] = typeof(int);
                                else if (typeParam == typeof(StringWrapper))
                                        normalized[i] = typeof(string);
                                else if (typeParam == typeof(DoubleWrapper))
                                        normalized[i] = typeof(double);
                                else if (typeParam == typeof(BoolWrapper))
                                        normalized[i] = typeof(bool);
                        }

                        methodInfo = type.GetMethod(strMethodName, normalized.ToArray());
                        if (null != methodInfo)
                                return methodInfo;

                        // Enumerate candidates and pick the most compatible one.
                        MethodInfo? best = null;
                        int bestScore = int.MaxValue;
                        foreach (MethodInfo candidate in GetCandidateMethods(type, strMethodName, lstParameters.Count))
                        {
                                ParameterInfo[] parameters = candidate.GetParameters();
                                int score = 0;
                                bool compatible = true;

                                for (int i = 0; i < parameters.Length; i++)
                                {
                                        System.Type argType = lstParameters[i];
                                        System.Type paramType = parameters[i].ParameterType;

                                        if (argType == paramType)
                                                continue;

                                        // handle wrappers
                                        if (argType == typeof(IntWrapper) && paramType == typeof(int))
                                        {
                                                score += 1;
                                                continue;
                                        }
                                        if (argType == typeof(StringWrapper) && paramType == typeof(string))
                                        {
                                                score += 1;
                                                continue;
                                        }
                                        if (argType == typeof(DoubleWrapper) && paramType == typeof(double))
                                        {
                                                score += 1;
                                                continue;
                                        }
                                        if (argType == typeof(BoolWrapper) && paramType == typeof(bool))
                                        {
                                                score += 1;
                                                continue;
                                        }

                                        if (paramType.IsAssignableFrom(argType))
                                        {
                                                score += 2;
                                                continue;
                                        }

                                        compatible = false;
                                        break;
                                }

                                if (compatible && score < bestScore)
                                {
                                        best = candidate;
                                        bestScore = score;
                                }
                        }

                        return best;
                }

               static public bool HasBaseType(System.Type typeTarget, System.Type type)
               {
                       if (typeTarget == null)
                               return false;

                       return type.IsAssignableFrom(typeTarget);
               }

	}
}
