//added
using ProtoScript.Interpretter.RuntimeInfo;
using ProtoScript.Interpretter.Symbols;

namespace ProtoScript.Extensions.Suggestions
{
	public class PrototypeTypeInfoSuggestions
	{
		static public List<CodeContext.Symbol> Suggest(PrototypeTypeInfo info, SymbolTable symbols)
		{
			List<CodeContext.Symbol> lstSuggestions = new List<CodeContext.Symbol>();

			foreach (var pair in info.Scope)
			{
				if (pair.Value is PrototypeTypeInfo)
				{
					CodeContext.Symbol symbol = new CodeContext.Symbol();
					symbol.SymbolName = pair.Key;
					symbol.SymbolType = "Property";
					lstSuggestions.Add(symbol);
				}
				if (pair.Value is FunctionRuntimeInfo)
				{
					{
						CodeContext.Symbol symbol = new CodeContext.Symbol();
						symbol.SymbolName = pair.Key;
						symbol.SymbolType = "Method";
						lstSuggestions.Add(symbol);
					}
					{
						FunctionRuntimeInfo functionRuntimeInfo = pair.Value as FunctionRuntimeInfo;
						CodeContext.Symbol symbol = new CodeContext.Symbol();
						symbol.SymbolName = pair.Key + "(" + string.Join(", ", functionRuntimeInfo.Parameters.Select(x => x.ParameterName ?? "(null)")) + ")";
						symbol.SymbolType = "Method";
						lstSuggestions.Add(symbol);
					}
				}

			}

			return lstSuggestions;
		}
	}
}
