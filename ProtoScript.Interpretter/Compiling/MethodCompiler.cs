using BasicUtilities;
using Ontology;
using ProtoScript.Diagnostics;
using ProtoScript.Interpretter.Compiled;
using ProtoScript.Interpretter.RuntimeInfo;
using ProtoScript.Interpretter.Symbols;

namespace ProtoScript.Interpretter.Compiling
{
	public class MethodCompiler
	{
		public static Compiled.Expression CompileMethodEvaluationInternal(MethodEvaluation methodEval, Compiled.Expression expression, Compiler compiler)
		{
			List<Compiled.Expression> lstParameters = new List<Compiled.Expression>();
			List<System.Type> lstParameterTypes = new List<System.Type>();

			object obj = expression.InferredType;
			string strMethod = methodEval.MethodName;

			//N20231031-01 - Removed to support this.From<CSharp.Code.Hidden.2D68F5D82DBB2F6AE40A3B22EFBC399E>()
			//if (strMethod.Contains("."))
			//	strMethod = StringUtil.RightOfLast(strMethod, ".");

			for (int i = 0; i < methodEval.Parameters.Count; i++)
			{
				Compiled.Expression exp = compiler.Compile(methodEval.Parameters[i]);
				if (null == exp)
				{
					compiler.AddDiagnostic(new Diagnostic("Unknown Parameter"), null, methodEval.Parameters[i]);
					return null;
				}

				exp.Info = methodEval.Parameters[i].Info;

				lstParameters.Add(exp);

				if (exp is LambdaOperator)
					lstParameterTypes.Add(typeof(Predicate<Prototype>));
				else if (exp is Compiled.Literal && (exp as Compiled.Literal).Value == null)
					lstParameterTypes.Add(null);
				else
					lstParameterTypes.Add(exp.InferredType.Type);
			}


			if (obj is PrototypeTypeInfo)
			{
				PrototypeTypeInfo typeInfo = obj as PrototypeTypeInfo;
				FunctionRuntimeInfo functionRuntimeInfo= ResolveMethod2(typeInfo.Prototype, strMethod, compiler.Symbols);

				if (null != functionRuntimeInfo)
				{
					FunctionEvaluation functionEvaluation = new FunctionEvaluation();
					functionEvaluation.Info = methodEval.Info;
					functionEvaluation.Parameters = lstParameters;

					//TODO: allow for function overloading here
					if (functionEvaluation.Parameters.Count != functionRuntimeInfo.Parameters.Count)
					{
						compiler.AddDiagnostic(new Diagnostic("Incorrect number of parameters"), null, methodEval);
						return null;
					}
					for (int i = 0; i < functionRuntimeInfo.Parameters.Count; i++)
					{
						ParameterRuntimeInfo destParam = functionRuntimeInfo.Parameters[i];
						Compiled.Expression exp = functionEvaluation.Parameters[i];

						//We should be able to pass null to a method
						//if (exp.InferredType == null)
						//{
						//	compiler.AddDiagnostic($"Parameter {destParam.ParameterName} is null", null, methodEval);
						//	return null;
						//}
						//else 
						if (!SimpleInterpretter.IsAssignableFrom(exp.InferredType, destParam.Type))
						{
							compiler.AddDiagnostic(new CannotConvert(exp.InferredType.ToString(), destParam.Type.ToString()), null, methodEval);
							return null;
						}

					}

					functionEvaluation.Function = functionRuntimeInfo;
					functionEvaluation.InferredType = functionRuntimeInfo.ReturnType;
					functionEvaluation.Object = expression;

					return functionEvaluation;
				}

				if (null != typeInfo.Generic && strMethod.Contains("<"))
				{
					functionRuntimeInfo = typeInfo.Scope.GetSymbol(strMethod) as FunctionRuntimeInfo;

					//Create an instance of the method from the generic 
					if (null == functionRuntimeInfo)
					{
						string strGenericMethod = StringUtil.LeftOfFirst(strMethod, "<") + "<>";
						string strGenericParameter = StringUtil.Between(strMethod, "<", ">");
						TypeInfo typeGenericParameter = compiler.Symbols.GetTypeInfo(strGenericParameter);

						FunctionRuntimeInfo genericInfo = typeInfo.Generic.Scope.GetSymbol(strGenericMethod) as FunctionRuntimeInfo;

						if (null != genericInfo)
						{
							functionRuntimeInfo = new FunctionRuntimeInfo();
							functionRuntimeInfo.ReturnType = genericInfo.ReturnType;
							functionRuntimeInfo.Scope = genericInfo.Scope;

							//Use the original name, so it can override later
							functionRuntimeInfo.FunctionName = strMethod;
							if (functionRuntimeInfo.ReturnType is GenericTypeInfo)
								functionRuntimeInfo.ReturnType = typeGenericParameter;

							typeInfo.Scope.Symbols[strMethod] = functionRuntimeInfo;
						}
					}

					if (null == functionRuntimeInfo)
					{
						compiler.AddDiagnostic(new Diagnostic("Could not find method: " + strMethod), null, methodEval);
						return null;
					}

					FunctionEvaluation functionEvaluation = new FunctionEvaluation();
					functionEvaluation.Info = methodEval.Info;
					functionEvaluation.Parameters = lstParameters;

					//TODO: allow for function overloading here
					if (functionEvaluation.Parameters.Count != functionRuntimeInfo.Parameters.Count)
					{
						compiler.AddDiagnostic(new Diagnostic("Incorrect number of parameters"), null, methodEval);
						return null;
					}
					for (int i = 0; i < functionRuntimeInfo.Parameters.Count; i++)
					{
						ParameterRuntimeInfo destParam = functionRuntimeInfo.Parameters[i];
						Compiled.Expression exp = functionEvaluation.Parameters[i];

						if (exp.InferredType == null)
						{
							compiler.AddDiagnostic($"Parameter {destParam.ParameterName} is null", null, methodEval);
							return null;
						}
						else if (!SimpleInterpretter.IsAssignableFrom(exp.InferredType, destParam.Type))
						{
							compiler.AddDiagnostic(new CannotConvert(exp.InferredType.ToString(), destParam.Type.ToString()), null, methodEval);
							return null;
						}

					}

					functionEvaluation.Function = functionRuntimeInfo;
					functionEvaluation.InferredType = functionRuntimeInfo.ReturnType;
					functionEvaluation.Object = expression;

					return functionEvaluation;
				}
			}

			{
				System.Type type = (obj is TypeInfo) ? (obj as TypeInfo).Type : obj.GetType();
				System.Reflection.MethodInfo method = ReflectionUtil.GetMethod(type, strMethod, lstParameterTypes);

				if (null != method)
				{



DotNetMethodEvaluation dotNetMethodEval = new DotNetMethodEvaluation();
dotNetMethodEval.Info = methodEval.Info;
dotNetMethodEval.Method = method;
dotNetMethodEval.Parameters = lstParameters;
dotNetMethodEval.Object = expression;
dotNetMethodEval.InferredType = new TypeInfo(method.ReturnType);
dotNetMethodEval.IsNullConditional = methodEval.IsNullConditional;

					return dotNetMethodEval;
				}

				method = type.GetMethod(strMethod);
				if (null != method)
				{
					compiler.AddDiagnostic(new Diagnostic($"Method {strMethod} exists but parameters don't match"), null, methodEval);
					return null;
				}

			}

			compiler.AddDiagnostic(new Diagnostic($"Cannot find compatible method {strMethod} on object"), null, methodEval);
			return null;
		}

		public static FunctionRuntimeInfo ResolveMethod2(Prototype prototype, string strSubObj, SymbolTable symbols)
		{
			PrototypeTypeInfo prototypeTypeInfo = symbols.GetTypeInfo(prototype.PrototypeName) as PrototypeTypeInfo;
			if (null != prototypeTypeInfo && null != prototypeTypeInfo.Scope)
			{
				FunctionRuntimeInfo infoFunc = prototypeTypeInfo.Scope.GetSymbol(strSubObj) as FunctionRuntimeInfo;
				if (null != infoFunc)
					return infoFunc;
			}
			foreach (int protoTypeOf in prototype.GetTypeOfs())
			{
				var tuple = ResolveMethod2(Prototypes.GetPrototype(protoTypeOf), strSubObj, symbols);

				if (null != tuple)
				{
					return tuple;
				}
			}

			return null;
		}
	}
}
