using BasicUtilities;
using ProtoScript.Extensions.SolutionItems;
using ProtoScript.Extensions.Suggestions;
using ProtoScript.Interpretter;
using ProtoScript.Interpretter.RuntimeInfo;
using ProtoScript.Interpretter.Symbols;

namespace ProtoScript.Extensions
{
	public class CodeContext
	{
		private Compiler m_Compiler = null;

		public Compiler Compiler
		{
			get
			{
				return m_Compiler;
			}
			set
			{
				m_Compiler = value;
			}
		}

		private Project m_Project = null;
		public Project Project
		{
			get
			{
				return m_Project;
			}

			set
			{
				m_Project = value;
			}
		}

		public SymbolTable Symbols
		{
			get
			{
				return Compiler.Symbols;
			}
		}

		public ProtoScriptFile ProtoScriptFile { get; set; }

		public bool SetCurrentFile(string strFileName)
		{
			bool bIsSet = false;

			if (null != Project)
			{
				foreach (ProjectFile file in Project.Files)
				{
					if (file is ProtoScriptFile)
					{
						ProtoScriptFile protoScriptFile = file as ProtoScriptFile;
						if (StringUtil.EqualNoCase(protoScriptFile.FileName, strFileName))
						{
							this.ProtoScriptFile = protoScriptFile;
							bIsSet = true;
							break;
						}
					}
				}
			}

			return bIsSet;
		}

		public List<Statement> EnterScopeAtContext(int iPos)
		{
			if (null == this.ProtoScriptFile || null == this.Symbols)
				return new List<Statement>();

			SymbolTable symbols = this.Symbols;
			symbols.ReturnToGlobalScope();

			//symbols.EnterScope(this.CSharpFile.Scope);
			List<ProtoScript.Statement> lstStatements = new List<Statement>();

			ProtoScript.File file = this.Compiler.Files.FirstOrDefault(x => StringUtil.EqualNoCase(x.Info.FullName, this.ProtoScriptFile.FileName));
			if (null == file)
			{
				Logs.DebugLog.WriteEvent("Compiled File Not Found", this.ProtoScriptFile.FileName);
			}
			else
			{
				lstStatements = ContextUtil.GetContextAtCursor(file, iPos);
				List<Statement> lstContext = new List<Statement>();
				lstContext.AddRange(lstStatements);

				foreach (Statement statement in lstStatements)
				{
					if (statement is PrototypeDefinition)
					{
						PrototypeDefinition prototypeDefinition = (PrototypeDefinition)statement;
						PrototypeTypeInfo prototypeTypeInfo = symbols.GetGlobalScope().GetSymbol(prototypeDefinition.PrototypeName.TypeName) as PrototypeTypeInfo;

						if (null != prototypeTypeInfo)
							symbols.EnterScope(prototypeTypeInfo.Scope);
					}

					else if (statement is FunctionDefinition)
					{
						FunctionDefinition methodDef = (FunctionDefinition)statement;
						FunctionRuntimeInfo infoMethod = symbols.GetSymbol(methodDef.FunctionName) as FunctionRuntimeInfo;
						symbols.EnterScope(infoMethod.Scope);
					}

					else if (statement is CodeBlockStatement)
					{
						Scope scope = new Scope(Scope.ScopeTypes.Block);
						symbols.EnterScope(scope);
					}

					else
					{
						//ContextUtil.AddStatementToScope(statement, symbols);
						//ContextUtil.AddStatementToScope(statement, symbols);
					}
				}
			}

			return lstStatements;
		}

		public class Symbol
		{
			public string SymbolName;
			public string SymbolType;
			public StatementParsingInfo Info;

			public override string ToString()
			{
				return $"CodeContext.Symbol[{SymbolName}]";
			}
		}
		public List<Symbol> GetSymbolsAtCursor(int iPos)
		{
			this.EnterScopeAtContext(iPos);

			List<Symbol> lstSymbols = new List<Symbol>();

			Scope scope = this.Symbols.ActiveScope();
			if (scope.ScopeType == Scope.ScopeTypes.Global)
			{
				lstSymbols.AddRange(GetGlobalSymbols(scope));
			}
			else
			{
				lstSymbols.AddRange(GetSuggestableSymbols(scope));

				for (int i = 0; i < this.Symbols.ActiveScopes.Count; i++)
				{
					scope = this.Symbols.ActiveScopes[i];
					lstSymbols.AddRange(GetSuggestableSymbols(scope));
				}
			}

			return lstSymbols;
		}

		public List<Symbol> Suggest(int iPos, string strLine)
		{
			List<Symbol> lstSymbols = new List<Symbol>();

			this.EnterScopeAtContext(iPos - strLine.Length);
			strLine = strLine.Trim();
			if (strLine.Contains(" "))
				strLine = StringUtil.SplitOnSpaces(strLine).Last();

			string strSymbol = strLine;
			string strSearch = null;
			if (strSymbol.Contains("."))
			{
				string[] strSplits = StringUtil.Split(strLine, ".", false);
				strSymbol = strSplits[strSplits.Length - 2];
				strSearch = strSplits[strSplits.Length - 1];
			}

			//if (strSearch.Contains("("))
			//{
			//	strSearch = StringUtil.RightOfLast(strSearch, "(");
			//}

			Logs.DebugLog.WriteEvent("Suggesting On", $"[{strSymbol}]");
			object obj = this.Symbols.GetSymbol(strSymbol);
			if (obj is ValueRuntimeInfo)
				obj = (obj as ValueRuntimeInfo).Type;

			if (obj is DotNetTypeInfo)
			{
				foreach (Symbol symbol in Suggestions.DotNetSuggestions.Suggest((obj as DotNetTypeInfo).Type))
				{
					if (StringUtil.IsEmpty(strSearch) || StringUtil.InString(symbol.SymbolName, strSearch))
						lstSymbols.Add(symbol);
				}
			}
			else if (obj is ValueRuntimeInfo)
			{
				PrototypeTypeInfo prototypeTypeInfo = (obj as ValueRuntimeInfo).Type as PrototypeTypeInfo;
				foreach (Symbol symbol in Suggestions.PrototypeTypeInfoSuggestions.Suggest(prototypeTypeInfo, Symbols))
				{
					if (StringUtil.IsEmpty(strSearch) || StringUtil.InString(symbol.SymbolName, strSearch))
						lstSymbols.Add(symbol);
				}

			}
			else if (obj is PrototypeTypeInfo)
			{
				PrototypeTypeInfo prototypeTypeInfo = (obj as PrototypeTypeInfo);
				foreach (Symbol symbol in Suggestions.PrototypeTypeInfoSuggestions.Suggest(prototypeTypeInfo, Symbols))
				{
					if (StringUtil.IsEmpty(strSearch) || StringUtil.InString(symbol.SymbolName, strSearch))
						lstSymbols.Add(symbol);
				}

			}
			else if (obj is Interpretter.Compiled.Namespace ns)
			{
				foreach (var entry in ns.Scope)
				{
					if (StringUtil.IsEmpty(strSearch) || StringUtil.InString(entry.Key, strSearch))
					{
						lstSymbols.Add(new Symbol() { SymbolName = entry.Key, SymbolType = "Namespace" });
					}
				}

			}
			return lstSymbols;
			
		}

		public List<string> PredictNextLine(int iPos)
		{
			List<Statement> lstContext = this.EnterScopeAtContext(iPos);
			return LLMSuggestions.Suggest(iPos, this, lstContext);
		}

		private static List<Symbol> GetSuggestableSymbols(Scope scope)
		{
			List<Symbol> lstSymbols = new List<Symbol>();
			if (scope.ScopeType != Scope.ScopeTypes.File && scope.ScopeType != Scope.ScopeTypes.Global)
			{
				foreach (var pair in scope)
				{
					if (pair.Value is PrototypeTypeInfo)
						lstSymbols.Add(new Symbol() { SymbolName = pair.Key, SymbolType = "Prototype" });
					else if (pair.Value is FunctionRuntimeInfo)
						lstSymbols.Add(new Symbol() { SymbolName = pair.Key, SymbolType = "Function" });
					else if (pair.Value is ValueRuntimeInfo)
						lstSymbols.Add(new Symbol() { SymbolName = pair.Key, SymbolType = "Variable" });
					else if (pair.Value is DotNetTypeInfo)
						lstSymbols.Add(new Symbol() { SymbolName = pair.Key, SymbolType = "DotNet" });
				}
			}
			else if (scope.ScopeType == Scope.ScopeTypes.Global)
			{
				foreach (var pair in scope)
				{
					//The UI handles global prototypes
					if (pair.Value is FunctionRuntimeInfo)
						lstSymbols.Add(new Symbol() { SymbolName = pair.Key, SymbolType = "Function" });
					else if (pair.Value is DotNetTypeInfo)
						lstSymbols.Add(new Symbol() { SymbolName = pair.Key, SymbolType = "DotNet" });
				}
			}
			return lstSymbols;
		}

		private static List<Symbol> GetGlobalSymbols(Scope scope)
		{
			List<Symbol> lstSymbols = new List<Symbol>();

			string[] strGlobalKeywords = { "prototype", "function" };

			foreach (string strKeyword in strGlobalKeywords)
			{
				lstSymbols.Add(new Symbol(){ SymbolName = strKeyword, SymbolType = "Keyword" });				
			}

			return lstSymbols;
		}

	}
}
