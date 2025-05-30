using System.Reflection;

namespace ProtoScript.Extensions.Suggestions
{
	public class DotNetSuggestions
	{
		static public List<CodeContext.Symbol> Suggest(System.Type type)
		{
			List<CodeContext.Symbol> lstSuggestions = new List<CodeContext.Symbol>();

			foreach (MethodInfo info in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
			{
				{
					CodeContext.Symbol symbol = new CodeContext.Symbol();
					symbol.SymbolName = info.Name;
					symbol.SymbolType = "Method";
					lstSuggestions.Add(symbol);
				}
				{
					CodeContext.Symbol symbol = new CodeContext.Symbol();
					symbol.SymbolName = info.Name + "(" + string.Join(", ", info.GetParameters().Select(x => x.Name)) + ")";
					symbol.SymbolType = "Method";
					lstSuggestions.Add(symbol);
				}

			}

			return lstSuggestions;		
		}
	}
}
