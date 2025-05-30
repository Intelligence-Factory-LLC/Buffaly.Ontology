using ProtoScript.Interpretter.Symbols;

namespace ProtoScript.Extensions
{
	public class ContextUtil
	{
		public static List<Statement> GetContainingBlocks(List<Statement> lstContext)
		{
			List<Statement> lstStatements = new List<Statement>();
			Statement statementCurrent = lstContext.Last();

			foreach (Statement statement in lstContext)
			{
				if (statement is NamespaceDefinition || statement is PrototypeDefinition || statement is FunctionDefinition || statement is PropertyDefinition)
					lstStatements.Add(statement);

				else if (statement.Info.IsInside(statementCurrent.Info.StartingOffset))
					lstStatements.Add(statement);
			}

			return lstStatements;
		}


		static public Scope GetParentScope(SymbolTable symbols)
		{
			Scope scopeCurrent = symbols.ActiveScope();

			//Don't return the Method scope if we are declaring a Method, return the Parent scope
			//if (scopeCurrent.ScopeType == Scope.ScopeTypes.Method || scopeCurrent.ScopeType == Scope.ScopeTypes.Class || scopeCurrent.ScopeType == Scope.ScopeTypes.Namespace)
			//	return scopeCurrent;

			for (int i = symbols.ActiveScopes.Count - 1; i >= 0; i--)
			{
				Scope scope = symbols.ActiveScopes[i];
				if (scope.ScopeType == Scope.ScopeTypes.Method || scope.ScopeType == Scope.ScopeTypes.Class || scope.ScopeType == Scope.ScopeTypes.Namespace)
					return scope;
			}

			return null;
		}

		//static public Class ResolveType(CSharp.Type type, SymbolTable symbols)
		//{
		//	if (null == type || StringUtil.IsEmpty(type.TypeName))
		//		return null;

		//	string[] strParts = StringUtil.Split(type.TypeName, ".");

		//	Scope scope = null;
		//	object obj = null;
		//	Class cls = null;

		//	//N20200701-01 - Namespace and type can be ambiguous
		//	if (strParts.Length == 1)
		//	{
		//		symbols.TryGetSymbol<Class>(strParts.First(), out cls);
		//	}
		//	else
		//	{
		//		for (int i = 0; i < strParts.Length; i++)
		//		{
		//			string strPart = strParts[i];

		//			if (null == scope)
		//			{
		//				obj = symbols.GetSymbol(strPart);
		//			}

		//			else
		//			{
		//				obj = scope.GetSymbol(strPart);
		//			}

		//			if (obj is IScope)
		//			{
		//				scope = ((IScope)obj).GetScope();
		//			}
		//		}

		//		cls = obj as Class;
		//	}

		//	return cls;
		//}

		//static public object ResolveExpressionScope(CSharp.Expression expression, SymbolTable symbols)
		//{
		//	object obj = null;

		//	Scope scope = symbols.ActiveScope();

		//	bool bLocalScopeOnly = false;
		//	for (int i = 0; i < expression.Terms.Count; i++)
		//	{
		//		Expression term = expression.Terms[i];

		//		if (term is Identifier)
		//		{
		//			Identifier identifier = term as Identifier;
		//			obj = ResolveIdentifier(identifier.Value, symbols);

		//			if (null == obj)
		//				return null;

		//			if (obj is IScope)
		//			{
		//				scope = (obj as IScope).GetScope();
		//			}

		//			else if (obj is INameValue)
		//			{
		//				INameValue nv = obj as INameValue;
		//				TypeInfo info = nv.GetTypeInfo(symbols);
		//				scope = info.ResolvedClass.Scope;
		//			}
		//		}

		//		if (term is BinaryOperator)
		//			bLocalScopeOnly = true;

		//		if (term is MethodEvaluation)
		//		{
		//			MethodEvaluation evaluation = term as MethodEvaluation;
		//			List<Method> lstMethods = null;

		//			if (bLocalScopeOnly)
		//				lstMethods = scope.GetSymbol(evaluation.MethodName) as List<Method>;
		//			else
		//				lstMethods = symbols.GetSymbol(evaluation.MethodName) as List<Method>;

		//			obj = lstMethods;
		//		}
		//	}

		//	return obj;
		//}

		//static public object ResolveIdentifier(string strValue, SymbolTable symbols)
		//{
		//	object obj = null;
		//	Scope scopeCurrent = null;

		//	foreach (string strTerm in StringUtil.Split(strValue, "."))
		//	{
		//		object symbol = null;
		//		if (null == scopeCurrent)
		//			symbol = symbols.GetSymbol(strTerm);
		//		else
		//			symbol = scopeCurrent.GetSymbolRecursively(strTerm);

		//		if (null == symbol)
		//			return null;

		//		obj = symbol;

		//		if (symbol is IScope)
		//		{
		//			scopeCurrent = (symbol as IScope).GetScope();
		//		}

		//		else if (symbol is INameValue)
		//		{
		//			INameValue nv = symbol as INameValue;
		//			TypeInfo info = nv.GetTypeInfo(symbols);

		//			//Can be null if the declaration is "var"
		//			if (info.ResolvedClass == null)
		//				return null;

		//			scopeCurrent = info.ResolvedClass.Scope;
		//			if (null != info.ResolvedClass.ParentClassType)
		//			{
		//				TypeInfo infoParent = TypeInfo.Get(info.ResolvedClass.ParentClassType, symbols);
		//				scopeCurrent.Parent = infoParent.ResolvedClass?.Scope;
		//			}
		//		}

		//	}

		//	return obj;
		//}

		//static public void AddStatementToScope(Statement statement, SymbolTable symbols)
		//{
		//	if (statement is VariableDeclaration)
		//	{
		//		Variable variable = SourceConverters.FromVariableDeclaration((VariableDeclaration)statement, symbols);
		//		symbols.InsertSymbol(variable.VariableName, variable);
		//	}

		//	if (statement is ForEachStatement)
		//	{
		//		ForEachStatement forEach = statement as ForEachStatement;
		//		Variable variable = new Variable();
		//		variable.Type = forEach.Type;
		//		variable.VariableName = forEach.IteratorName;
		//		symbols.InsertSymbol(variable.VariableName, variable);
		//	}

		//}

		static public List<ProtoScript.Statement> GetContextAtCursor(List<ProtoScript.Statement> statements, int iOffset)
		{
			List<ProtoScript.Statement> lstContext = new List<ProtoScript.Statement>();

			foreach (ProtoScript.Statement statement in statements)
			{
				if (statement.Info.StartingOffset > iOffset)
					break;

				if (statement.Info.IsInside(iOffset))
					lstContext.Add(statement);
			}

			return lstContext;
		}

		public static List<Statement> GetContextAtCursor(ProtoScript.File file, int iOffset)
		{
			List<Statement> lstContext = new List<Statement>();

			PrototypeDefinition prototypeDefinition = file.PrototypeDefinitions.FirstOrDefault(x => x.Info.IsInside(iOffset));

			if (null != prototypeDefinition)
			{
				lstContext = GetContextAtCursor(prototypeDefinition, iOffset);
			}

			else
			{
				Statement statement = file.Statements.FirstOrDefault(x => x.Info.IsInside(iOffset));
				if (null != statement)
				{
					lstContext = GetContextAtCursor(statement, iOffset);
				}
			}

			return lstContext;
		}


		public static List<Statement> GetContextAtCursor(Statement statement, int iPos)
		{
			List<Statement> lstContext = new List<Statement>();

			if (statement.Info.IsInside(iPos))
			{
				lstContext.Add(statement);

				if (statement is IfStatement)
				{
					lstContext.AddRange(GetContextAtCursor(statement as IfStatement, iPos));
				}

				else
				{
					foreach (Statement subStatement in statement.GetChildrenStatements().OrderBy(x => x.Info.StartingOffset))
					{
						if (subStatement.Info == null || subStatement.Info.StartingOffset > iPos)
							break;

						if (!(statement is PrototypeDefinition) && subStatement.Info.StoppingOffset < iPos)
						{
							//This adds the statements preceeding the current to the context
							lstContext.Add(subStatement);
						}

						else if (subStatement.Info.IsInside(iPos))
						{
							lstContext.AddRange(GetContextAtCursor(subStatement, iPos));
						}
					}
				}
			}

			return lstContext;
		}

		public static List<Statement> GetContextAtCursor(IfStatement statement, int iPos)
		{
			List<Statement> lstContext = new List<Statement>();

			if (statement.TrueBody.Info.IsInside(iPos))
			{
				lstContext.Add(new CodeBlockStatement());

				foreach (Statement subStatement in statement.TrueBody)
				{
					if (subStatement.Info.StoppingOffset <= iPos)
					{
						lstContext.Add(subStatement);
					}
					else if (subStatement.Info.IsInside(iPos))
					{
						lstContext.AddRange(GetContextAtCursor(subStatement, iPos));
					}
				}
			}
			else if (null != statement.ElseBody && statement.ElseBody.Info.IsInside(iPos))
			{
				lstContext.Add(new CodeBlockStatement());

				foreach (Statement subStatement in statement.ElseBody)
				{
					if (subStatement.Info.StoppingOffset <= iPos)
					{
						lstContext.Add(subStatement);
					}
					else if (subStatement.Info.IsInside(iPos))
					{
						lstContext.AddRange(GetContextAtCursor(subStatement, iPos));
					}
				}
			}
			else
			{
				foreach (CodeBlock codeBlock in statement.ElseIfBodies)
				{
					if (codeBlock.Info.IsInside(iPos))
					{
						lstContext.Add(new CodeBlockStatement());

						foreach (Statement subStatement in codeBlock)
						{
							if (subStatement.Info.StoppingOffset <= iPos)
							{
								lstContext.Add(subStatement);
							}
							else if (subStatement.Info.IsInside(iPos))
							{
								lstContext.AddRange(GetContextAtCursor(subStatement, iPos));
							}
						}
					}
				}
			}

			return lstContext;
		}

		static public List<Expression> GetExpressionContextAtCursor(Expression expression, int iPos)
		{
			List<Expression> lstExpression = new List<Expression>();

			if (expression != null && expression.Info.IsInside(iPos))
			{
				lstExpression.Add(expression);

				if (expression is MethodEvaluation)
				{
					MethodEvaluation evaluation = expression as MethodEvaluation;
					foreach (Expression param in evaluation.Parameters)
					{
						lstExpression.AddRange(GetExpressionContextAtCursor(param, iPos));
					}
				}

				else if (null != expression.Terms)
				{
					foreach (Expression term in expression.Terms)
					{
						lstExpression.AddRange(GetExpressionContextAtCursor(term, iPos));
					}
				}
			}

			return lstExpression;
		}



		//static public List<Expression> GetExpressionContextByIncomplete(Expression expression)
		//{
		//	List<Expression> lstExpression = new List<Expression>();

		//	if (null != expression && ExpressionUtil.IsIncomplete(expression))
		//	{
		//		lstExpression.Add(expression);

		//		if (expression is MethodEvaluation)
		//		{
		//			MethodEvaluation evaluation = expression as MethodEvaluation;
		//			foreach (Expression param in evaluation.Parameters)
		//			{
		//				lstExpression.AddRange(GetExpressionContextByIncomplete(param));
		//			}
		//		}

		//		else if (null != expression.Terms)
		//		{
		//			foreach (Expression term in expression.Terms)
		//			{
		//				lstExpression.AddRange(GetExpressionContextByIncomplete(term));
		//			}
		//		}
		//	}

		//	return lstExpression;
		//}

		//static public CSharp.Type GetRelativeCSharpType(TypeInfo info, SymbolTable symbols)
		//{
		//	CSharp.Type type = new CSharp.Type();
		//	type.TypeName = GetRelativeTypeName(info.FullyQualifiedName, symbols);

		//	if (info.ResolvedClass != null && info.ResolvedClass.GenericParameters != null)
		//	{
		//		foreach (var v in info.ResolvedClass.GenericParameters)
		//		{
		//			type.ElementTypes.Add(new CSharp.Type() { TypeName = GetRelativeTypeName(v, symbols) });
		//		}
		//	}

		//	return type;
		//}

		//static public string GetRelativeTypeName(string strFullyQualifiedName, SymbolTable symbols)
		//{
		//	string[] strParts = StringUtil.Split(strFullyQualifiedName, ".");
		//	string strPath = string.Empty;
		//	for (int i = strParts.Length - 1; i >= 0; i--)
		//	{
		//		if (StringUtil.IsEmpty(strPath))
		//		{
		//			strPath = strParts[i];
		//		}
		//		else
		//		{
		//			strPath = strParts[i] + "." + strPath;
		//		}

		//		if (symbols.ResolveObject(strPath) != null)
		//			break;
		//	}

		//	return strPath;
		//}

		//static public CSharp.File ReplaceStatement(CSharp.File file, int iOriginalStartingOffset, Statement statementNew)
		//{
		//	List<Statement> lstContext = ContextUtil.GetContextAtCursor(file, iOriginalStartingOffset);
		//	List<Statement> lstContaining = ContextUtil.GetContainingBlocks(lstContext);

		//	Statement statementParent = lstContaining[lstContaining.Count - 2];

		//	if (statementParent is CodeBlockStatement)
		//	{
		//		CodeBlockStatement codeBlock = statementParent as CodeBlockStatement;
		//		for (int i = 0; i < codeBlock.Statements.Count; i++)
		//		{
		//			Statement statement = codeBlock.Statements[i];
		//			if (statement.Info.StartingOffset == iOriginalStartingOffset)
		//			{
		//				file.RawCode = file.RawCode.Remove(statement.Info.StartingOffset, statement.Info.Length);
		//				string strNewCode = CSharp.Parsers.SimpleGenerator.Generate(statementNew);
		//				if (strNewCode.EndsWith("\r\n"))
		//					strNewCode = StringUtil.LeftOfLast(strNewCode, "\r\n");

		//				file.RawCode = file.RawCode.Insert(statement.Info.StartingOffset, strNewCode);

		//				statementNew.Info = null;
		//				codeBlock.Statements[i] = statementNew;
		//				break;
		//			}
		//		}
		//	}
		//	return file;
		//}

		//static public string ResolveMethodName(string strIdentifier, SymbolTable symbols)
		//{
		//	object objResolved = ContextUtil.ResolveIdentifier(strIdentifier, symbols);
		//	string strEvaluationName = null;

		//	if (null != objResolved)
		//	{
		//		List<global::CSharp.Model.Method> lstMethods = objResolved as List<global::CSharp.Model.Method>;
		//		global::CSharp.Model.Method method = lstMethods.FirstOrDefault();
		//		strEvaluationName = method.Class.FullyQualifiedName + "." + method.MethodName;
		//	}
		//	return strEvaluationName;
		//}
	}
}
