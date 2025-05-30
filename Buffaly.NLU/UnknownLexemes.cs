using BasicUtilities;
using Ontology;
using Ontology.BaseTypes;
using Ontology.Simulation;
using ProtoScript.Interpretter;
using ProtoScript.Interpretter.RuntimeInfo;

namespace Buffaly.NLU
{
	public class UnknownLexemes
	{
		static public Prototype ResolveAll(Prototype protoLexemes, NativeInterpretter interpretter)
		{
			for (int i = 0; i < protoLexemes.Children.Count; i++)
			{
				Prototype protoLexeme = protoLexemes.Children[i];
				if (!Prototypes.TypeOf(protoLexeme, Lexeme.Prototype))
					protoLexemes.Children[i] = Resolve(protoLexeme, interpretter);
			}

			return protoLexemes;
		}

		static public Prototype Resolve(Prototype protoLexeme, NativeInterpretter interpretter)
		{
			if (Prototypes.TypeOf(protoLexeme, System_String.Prototype))
			{
				string strValue = StringWrapper.ToString(protoLexeme);
				if (StringUtil.IsInteger(strValue))
				{
					if (null != TemporaryPrototypes.GetTemporaryPrototypeOrNull(strValue))
						throw new Exception("Unexpected");

					Prototype prototype = TemporaryPrototypes.GetOrCreateTemporaryPrototype("_" + strValue);
					Prototype protoDecimal = TemporaryPrototypes.GetOrCreateTemporaryPrototype("NumericString");

					prototype.InsertTypeOf(protoDecimal);

					PrototypeTypeInfo infoPrototype = interpretter.Symbols.GetGlobalScope().GetSymbol("Lexeme") as PrototypeTypeInfo;
					FunctionRuntimeInfo infoFunction = infoPrototype.Scope.GetSymbol("NumericString") as FunctionRuntimeInfo;

					object oResult = interpretter.RunMethod(infoFunction, null, new List<object> { prototype, strValue, Convert.ToInt32(strValue)  });

					return interpretter.GetAsPrototype(oResult);
				}

				else
				{
					Prototype protoUnknown = TemporaryPrototypes.GetOrCreateTemporaryPrototype("UnknownLexeme");
					if (null != protoUnknown)
					{
						Prototype prototype = TemporaryPrototypes.GetOrCreateTemporaryPrototype(strValue);
						prototype.InsertTypeOf(protoUnknown);

						return TemporaryLexemes.GetOrInsertLexeme(strValue, prototype);
					}
					//else
					//{
					//	TemporaryPrototype prototype = TemporaryPrototypes.GetOrCreateTemporaryPrototype(strValue);
					//	QuantumPrototype q = new QuantumPrototype(prototype);

					//	return TemporaryLexemes.GetOrInsertLexeme(strValue, q);
					//}
				}

			}

			return protoLexeme;
		}
	}
}
