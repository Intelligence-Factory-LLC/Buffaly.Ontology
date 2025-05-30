﻿using BasicUtilities;
using System.Text;

namespace ProtoScript.Parsers
{
	[Serializable]
	public class GenerateFailedException : Exception
	{
		public GenerateFailedException() { }
		public GenerateFailedException(string message) : base(message) { }
		public GenerateFailedException(string message, Exception inner) : base(message, inner) { }
		protected GenerateFailedException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}

	public class SimpleGenerator
	{
		int m_iTabs = 0;
		StringBuilder m_sb = new StringBuilder();

		static public string Generate(ProtoScript.File file)
		{
			SimpleGenerator generator = new Parsers.SimpleGenerator();

			foreach (UsingStatement statement in file.Usings)
				generator.ToString(statement);

			foreach (NamespaceDefinition statement in file.Namespaces)
				generator.ToString(statement);

			foreach (PrototypeDefinition statement in file.PrototypeDefinitions)
			{
				generator.ToString(statement);
//				generator.WriteStop();
			}

			return generator.m_sb.ToString();
		}

		static public string Generate(Expression oExpression)
		{
			SimpleGenerator g = new SimpleGenerator();
			if (oExpression is MethodEvaluation)
				g.ToString(oExpression as MethodEvaluation);
			else if (oExpression is Literal)
				g.ToString(oExpression as Literal);
			else if (oExpression is NewObjectExpression)
				g.ToString(oExpression as NewObjectExpression);
			else if (oExpression is CategorizationOperator)
				g.ToString(oExpression as CategorizationOperator);
			else
				g.ToString(oExpression);
			return g.m_sb.ToString();
		}

		static public string Generate(Statement statement)
		{
			SimpleGenerator g = new SimpleGenerator();
			g.ToString(statement);
			return g.m_sb.ToString();
		}

		static public string Generate(CodeBlock statements, bool bNaked)
		{
			SimpleGenerator g = new SimpleGenerator();
			g.ToString(statements, bNaked);
			return g.m_sb.ToString();
		}

		static public string Generate(ProtoScript.Type type)
		{
			SimpleGenerator g = new SimpleGenerator();
			g.ToString(type);
			return g.m_sb.ToString();
		}

		void WriteStart()
		{
			m_sb.Append(new string('\t', m_iTabs));
		}

		void WriteStart(string str)
		{
			WriteStart();
			Write(str);
		}

		void WriteStop()
		{
			m_sb.AppendLine();
		}

		void WriteStop(string str)
		{
			m_sb.AppendLine(str);
		}

		void WriteStatementStop()
		{
			m_sb.AppendLine(";");
		}

		void WriteLine(string str)
		{
			WriteStart();
			Write(str);
			WriteStop();
		}

		void Write(string str)
		{
			m_sb.Append(str);
		}

		void ToString(CaseStatement oCase)
		{
			WriteStart("case ");
			ToString(oCase.Value);
			WriteStop(":");
		}

		void ToString(ArrayLiteral oArray)
		{
			Write("[");

			for (var i = 0; i < oArray.Values.Count; i++)
			{
				if (i != 0)
					Write(", ");

				ToString(oArray.Values[i]);
			}

			Write("]");
		}


		void ToString(BinaryOperator op)
		{
			if (op is IndexOperator)
				ToString(op as IndexOperator);

			else
			{
				ToString(op.Left);
				if (op.Value == ".")
					Write(op.Value);
				else
					Write(" " + op.Value + " ");
				ToString(op.Right);
			}
		}

		void ToString(BreakStatement statement)
		{
			WriteLine("break;");
		}

        void ToString(CastingOperator expression)
        {
            Write("(");
            ToString(expression.Type);
            Write(")");
        }

		void ToString(CategorizationOperator expression)
		{
			ToString(expression.Left);
			Write(" -> ");
			ToString(expression.Middle);
			Write(" {");
			WriteStop();

			bool bFirst = true;

			m_iTabs++;

			foreach (Expression expr in expression.Right.Expressions)
			{
				if (!bFirst)
				{
					Write(",");
					WriteStop();
				}
				else
					bFirst = false;

				WriteStart();
				ToString(expr);
			}
			WriteStop();
			m_iTabs--;
			WriteStart("}");
		}

		void ToString(ContinueStatement statement)
		{
			WriteLine("continue;");
		}

		//void ToString(ClassDefinition cls)
		//{
		//	WriteStart();

		//	if (cls.IsStatic)
		//		Write("static ");

		//	ToString(cls.Visibility);

		//	if (cls.IsPartial)
		//		Write(" partial ");

		//	Write(" class ");
		//	ToString(cls.ClassName);

		//	if (cls.Inherits.Count > 0)
		//	{
		//		Write(" : ");

		//		for (int i = 0; i < cls.Inherits.Count; i++)
		//		{
		//			if (i > 0)
		//				Write(", ");
		//			ToString(cls.Inherits[i]);
		//		}
		//	}

		//	WriteStop();

		//	WriteLine("{");

		//	m_iTabs++;

		//	foreach (var en in cls.Enums)
		//		ToString(en);

		//	foreach (var field in cls.Fields)
		//		ToString(field);

		//	foreach (var prop in cls.Properties)
		//		ToString(prop);

		//	foreach (var method in cls.Methods)
		//		ToString(method);

		//	m_iTabs--;
		//	WriteLine("}");
		//}

		void ToString(CodeBlock oStatements, bool bNaked = false)
		{
			if (!bNaked)
			{
				WriteLine("{");
				m_iTabs++;
			}

			for (var i = 0; i < oStatements.Count; i++)
			{
				ToString(oStatements[i]);
			}

			if (!bNaked)
			{
				m_iTabs--;
				WriteLine("}");
			}
		}

		void ToString(DefaultStatement statement)
		{
			WriteLine("default:");
		}

		void ToString(DoStatement oDo)
		{
			WriteLine("do");
			ToString(oDo.Statements);
			WriteStart("while (");
			ToString(oDo.Expression);
			WriteStop(");");
		}

		void ToString(EnumDefinition en)
		{
			WriteStart("enum ");
			Write(ToString(en.EnumName));
			Write(" {");
			for (int i = 0; i < en.EnumTypes.Count; i++)
			{
				if (i > 0)
					Write(", ");

				Write(ToString(en.EnumTypes[i]));
			}
			WriteStop("}");
		}



		void ToString(Expression oExpression)
		{
			if (null == oExpression)
				Write("_");

			else if (null != oExpression.Terms && oExpression.Terms.Count > 0)
			{
				if (oExpression.IsParenthesized)
					Write("(");

				for (var i = 0; i < oExpression.Terms.Count; i++)
				{
					TermToString(oExpression.Terms[i]);
				}

				if (oExpression.IsParenthesized)
					Write(")");
			}
			else
			{
				TermToString(oExpression);
			}
		}

		void ToString(ExpressionList oExpression)
		{
			Write("(");
			for (var i = 0; i < oExpression.Expressions.Count; i++)
			{
				if (i != 0)
					Write(", ");

				ToString(oExpression.Expressions[i]);
			}
			Write(")");
		}

		void ToString(ExpressionStatement expression)
		{
			WriteStart();
			ToString(expression.Expression);
			WriteStatementStop();
		}

		void ToString(FieldDefinition field)
		{
			if (field.Annotations != null && field.Annotations.Count > 0)
			{
				foreach (AnnotationExpression annotationExpression in field.Annotations)
				{
					ToString(annotationExpression);
				}
			}

			WriteStart();

			if (field.IsStatic)
				Write("static ");
			if (field.IsReadonly)
				Write("readonly ");

			if (field.Visibility != Visibility.None)
				ToString(field.Visibility + " ");

			if (field.IsConst)
				Write(" const ");

			ToString(field.Type);

			Write(" " + ToString(field.FieldName)); 

			if (null != field.Initializer)
			{
				Write(" = ");
				ToString(field.Initializer);
			}

			WriteStop(";");
		}
		void ToString(FunctionDefinition oFunction)
		{
			WriteStart();

			Write("function " + ToString(oFunction.FunctionName) + "(");

			for (var i = 0; i < oFunction.Parameters.Count; i++)
			{
				if (i != 0)
					Write(", ");

				ToString(oFunction.Parameters[i]);
			}
			Write(")");

			if (null != oFunction.ReturnType)
			{
				Write(" : ");
				ToString(oFunction.ReturnType);
			}

			WriteStop();

			ToString(oFunction.Statements);
		}
		void ToString(Identifier identifier)
		{
			Write(ToString(identifier.Value));
		}

		void ToString(IndexOperator op)
		{
			ToString(op.Left);
			Write("[");
			ToString(op.Right);
			Write("]");
		}


		void ToString(Literal literal)
		{
			if (StringUtil.IsEmpty(literal.Value))
			{
				if (literal is AtPrefixedStringLiteral)
					Write("@\"_\"");
				else if (literal is StringLiteral)
					Write("\"_\"");
				else
					Write("_");
			}
			else
				Write(literal.Value);
		}

		void ToString(Operator op)
		{
			if (op is IsInitializedOperator)
				ToString((op as IsInitializedOperator));

			else if (op is CategorizationOperator)
				ToString(op as CategorizationOperator);

			else if (op.Value == "?" || op.Value == ":")
				Write(" " + op.Value + " ");

			else if (op.Value == "out" || op.Value == "ref" || op.Value == "await")

				Write(" " + op.Value + " ");
			else 
				Write(op.Value ?? "#");
		}

		void TermToString(Expression expression)
		{
			if (expression.IsParenthesized)
				Write("(");

			if (expression is ArrayLiteral)
				ToString(expression as ArrayLiteral);

			else if (expression is Literal)
				ToString(expression as Literal);

			else if (expression is BinaryOperator)
				ToString(expression as BinaryOperator);

			else if (expression is CastingOperator)
				ToString(expression as CastingOperator);

			else if (expression is Operator)
				ToString(expression as Operator);

			else if (expression is Identifier)
				ToString(expression as Identifier);

			else if (expression is ExpressionList)
				ToString(expression as ExpressionList);

			else if (expression is MethodEvaluation)
				ToString(expression as MethodEvaluation);

			else if (expression is NewObjectExpression)
				ToString(expression as NewObjectExpression);

			else if (expression is ProtoScript.Type)
				ToString(expression as ProtoScript.Type);

			else if (expression is CodeBlockExpression)
				ToString((expression as CodeBlockExpression).Statements);

			//This can cause stack overflow
			//else
			//	ToString(expression);

			if (expression.IsParenthesized)
				Write(")");

		}


		void ToString(ProtoScript.Type type)
		{
			if (null == type)
				Write("_");

			else
			{
				Write(ToString(type.TypeName));
				if (type.IsNullable)
					Write("?");
				if (type.ElementTypes.Count > 0)
				{
					Write("<");
					for (int i = 0; i < type.ElementTypes.Count; i++)
					{
						if (i > 0)
							Write(", ");

						ToString(type.ElementTypes[i]);
					}

					Write(">");
				}

				if (type.IsArray)
				{
					if (type.ArraySize.Terms.Count == 0)
						Write("[]");
					else
					{
						foreach (Expression expression in type.ArraySize.Terms)
						{
							Write("[");
							if (null != expression)
								ToString(type.ArraySize);
							Write("]");
						}
					}
				}
			}
		}

		void ToString(VariableDeclaration oDeclaration, bool bNaked = false)
		{
			if (!bNaked)
				WriteStart();

			if (oDeclaration.IsConst)
				Write("const ");

			ToString(oDeclaration.Type);
			Write(" " + ToString(oDeclaration.VariableName));

			if (oDeclaration.Initializer != null)
			{
				Write(" = ");
				ToString(oDeclaration.Initializer);
			}

			if (oDeclaration.ChainedDeclarations != null)
			{
				foreach (VariableDeclaration chained in oDeclaration.ChainedDeclarations)
				{
					Write(", ");
					Write(ToString(chained.VariableName));
					if (chained.Initializer != null)
					{
						Write(" = ");
						ToString(chained.Initializer);
					}
				}
			}

			if (!bNaked)
				WriteStatementStop();
		}

		string ToString(string strValue)
		{
			return StringUtil.IsEmpty(strValue) ? "_" : strValue;
		}

		void ToString(Visibility visibility)
		{
			switch (visibility)
			{
				case Visibility.Internal:
					Write("internal");
					break;
				case Visibility.Public:
					Write("public");
					break;
				case Visibility.Private:
					Write("private");
					break;
				case Visibility.Protected:
					Write("protected");
					break;
				case Visibility.PrivateProtected:
					Write("private protected");
					break;
				case Visibility.ProtectedInternal:
					Write("protected internal");
					break;
			}
		}

		//void ToString(MethodDefinition oFunction)
		//{
		//	WriteStart();
		//	if (oFunction.IsNew)
		//		Write("new");

		//	if (oFunction.IsStatic)
		//		Write("static ");
		//	ToString(oFunction.Visibility);
		//	Write(" ");

		//	if (oFunction.IsOverride)
		//		Write("override ");

		//	if (!oFunction.IsConstructor)
		//		ToString(oFunction.ReturnType);

		//	Write(" " + ToString(oFunction.MethodName) + "(");

		//	for (var i = 0; i < oFunction.Parameters.Count; i++)
		//	{
		//		if (i != 0)
		//			Write(", ");

		//		ToString(oFunction.Parameters[i]);
		//	}
		//	Write(")");

		//	if (oFunction.IsConstructor && null != oFunction.BaseConstructor)
		//	{
		//		Write(":");
		//		ToString(oFunction.BaseConstructor);
		//	}

		//	WriteStop();

		//	ToString(oFunction.Statements);
		//}


		void ToString(MethodEvaluation oFunction)
		{
			Write(ToString(oFunction.MethodName) + "(");
			ToString(oFunction.Parameters);
			Write(")");
		}

		void ToString(ParameterDeclaration declaration)
		{
			if (declaration.IsThis)
				Write("this ");

			if (declaration.IsOut)
				Write("out ");
			else if (declaration.IsRef)
				Write("ref ");

			ToString(declaration.Type);
			Write(" ");
			Write(ToString(declaration.ParameterName));

			if (declaration.DefaultValue != null)
			{
				Write(" = ");
				ToString(declaration.DefaultValue);
			}
		}

		void ToString(PropertyDefinition prop)
		{
			WriteStart();

			if (prop.IsNew)
				Write("new ");

			if (prop.IsStatic)
				Write("static ");
			//if (field.IsReadonly)
			//	Write("readonly ");
			ToString(prop.Visibility);

			if (prop.IsOverride)
				Write(" override ");

			Write(" ");

			ToString(prop.Type);

			Write(" " + ToString(prop.PropertyName));

			if (null != prop.Indexer)
			{
				Write("[");
				ToString(prop.Indexer);
				Write("]");
			}

			WriteStop();
			WriteLine("{");
			m_iTabs++;

			if (null != prop.Getter)
			{
				WriteStart("get");
				if (prop.Getter.Count != 0)
				{
					WriteStop();
					ToString(prop.Getter);
				}
				else
					WriteStatementStop();
			}

			if (null != prop.Setter)
			{
				WriteStart("set");
				if (prop.Setter.Count != 0)
				{
					WriteStop();
					ToString(prop.Setter);
				}
				else
					WriteStatementStop();
			}

			m_iTabs--;
			WriteLine("}");


		}

		void ToString(AnnotationExpression exp)
		{
			if (exp.IsExpanded)
			{
				MethodEvaluation methodEvaluation = exp.GetAnnotationMethodEvaluation();
				methodEvaluation.Parameters.RemoveAt(0);
				exp.IsExpanded = false;
			}

			WriteStart();
			Write("[");
			ToString(exp as Expression);
			Write("]");
			WriteStop();
		}

		void ToString(PrototypeDefinition cls)
		{
			if (!StringUtil.IsEmpty(cls.Comments))
				Write(cls.Comments);

			WriteStart();

			if (cls.Annotations != null && cls.Annotations.Count > 0)
			{ 
				foreach (AnnotationExpression annotationExpression in cls.Annotations)
				{
					ToString(annotationExpression);
				}
			}

			if (cls.IsExternal)
				Write("extern ");

			if (cls.IsPartial)
				Write("partial ");

			Write("prototype ");
			ToString(cls.PrototypeName);

			if (cls.Inherits.Count > 0)
			{
				Write(" : ");

				for (int i = 0; i < cls.Inherits.Count; i++)
				{
					if (i > 0)
						Write(", ");
					ToString(cls.Inherits[i]);
				}
			}


			bool bEmpty = cls.GetChildrenStatements().Count() == 0;

			if (bEmpty)
			{
				WriteLine(";");
				return;
			}

			WriteStop();
			WriteLine("{");

			m_iTabs++;

			foreach (var en in cls.Enums)
				ToString(en);

			foreach (var field in cls.Fields)
			{
				ToString(field as Statement);
				WriteStop();
			}
			
			foreach (var prop in cls.Properties)
				ToString(prop as Statement);

			bool bFirst = true;
			foreach (var method in cls.Methods)
			{
				if (bFirst)
					bFirst = false;
				else
					WriteStop(); 

				ToString(method as Statement);
			}

			foreach (var initializer in cls.Initializers)
			{
				//WriteLine("init");
				ToString(initializer.Statements, true);
				//WriteStop();
			}

			m_iTabs--;
			WriteLine("}");
		}

		void ToString(List<Expression> lstExpressions)
		{
			for (var i = 0; i < lstExpressions.Count; i++)
			{
				if (i != 0)
					Write(", ");

				ToString(lstExpressions[i]);
			}
		}

		void ToString(ReturnStatement oReturn)
		{
			WriteStart("return ");
			if (null != oReturn.Expression)
				ToString(oReturn.Expression);
			WriteStatementStop();
		}

		void ToString(NamespaceDefinition ns)
		{
			WriteStart("namespace ");
			for (int i = 0; i < ns.Namespaces.Count; i++)
			{
				if (i > 0)
					Write(".");
				Write(ToString(ns.Namespaces[i]));
			}

			WriteStop();
			WriteLine("{");
			m_iTabs++;

			foreach (EnumDefinition en in ns.Enums)
			{
				ToString(en);
			}

			foreach (PrototypeDefinition cls in ns.PrototypeDefinitions)
			{
				ToString(cls);
			}

			m_iTabs--;
			WriteLine("}");
		}

		void ToString(NewObjectExpression oNew)
		{
			Write("new ");

			if (null == oNew.Type)
			{
				Write("_(");
				if (null != oNew.Parameters)
				{
					ToString(oNew.Parameters);
				}
				Write(")");
			}

			if (null != oNew.Type && oNew.Type.TypeName != "(anonymous)")
			{
				ToString(oNew.Type);
				if (oNew.Type.IsArray)
				{
					Write(" ");
					ToString(oNew.ArrayInitializer);
				}
				else
				{
					Write("(");
					ToString(oNew.Parameters);
					Write(")");
				}
			}


			for (int i = 0; null != oNew.Initializers && i < oNew.Initializers.Count; i++)
			{
				if (i != 0)
					WriteStop(", ");
				else
					Write("{");

				if (oNew.Initializers[i] is NewObjectExpression.ObjectInitializer)
				{
					NewObjectExpression.ObjectInitializer initializer = oNew.Initializers[i] as NewObjectExpression.ObjectInitializer;
					if (i != 0)
						WriteStart();

					Write(initializer.Name);
					Write(" = ");
					ToString(initializer.Value);
				}
								

				if (i == oNew.Initializers.Count - 1)
					Write("}");
			}
		}

		void ToString(ThrowStatement oThrow)
		{
			WriteStart("throw ");
			if (null != oThrow.Expression)
				ToString(oThrow.Expression);
			WriteStatementStop();
		}

		void ToString(TryStatement oTry)
		{
			WriteLine("try");
			ToString(oTry.TryBlock);

			foreach (var oCatch in oTry.CatchBlocks)
			{
				WriteStart("catch");
				if (oCatch.Type != null)
				{
					Write("(");
					ToString(oCatch.Type);

					if (oCatch.ExceptionName != null)
					{
						Write(" " + oCatch.ExceptionName);
					}

					Write(")");
				}
				WriteStop();

				ToString(oCatch.Statements);
			}

			if (null != oTry.FinallyBlock)
			{
				WriteLine("finally");
				ToString(oTry.FinallyBlock);
			}
		}

		void ToString(IfStatement oIf)
		{
			WriteStart("if (");
			ToString(oIf.Condition);
			WriteStop(")");
			ToString(oIf.TrueBody);

			for (int i = 0; i < oIf.ElseIfBodies.Count; i++)
			{
				WriteStart("else if (");
				ToString(oIf.ElseIfConditions[i]);
				WriteStop(")");
				ToString(oIf.ElseIfBodies[i]);
			}

			if (null != oIf.ElseBody)
			{
				WriteLine("else");
				ToString(oIf.ElseBody);
			}
		}

		void ToString(IsInitializedOperator op)
		{
			Write("initialized(");
			ToString(op.Right);
			Write(")");
		}

		void ToString(ForStatement oFor)
		{
			WriteStart("for (");
			if (oFor.Start != null)
			{
				if (oFor.Start is VariableDeclaration)
					ToString(oFor.Start as VariableDeclaration, true);
				else if  (oFor.Start is ExpressionStatement)
					ToString((oFor.Start as ExpressionStatement).Expression);
			}
			Write("; ");
			if (oFor.Condition != null)
				ToString(oFor.Condition);
			Write("; ");
			if (oFor.Iteration != null)
			{
				for (int i = 0; i < oFor.Iteration.Expressions.Count; i++)
				{
					if (i > 0)
						Write(", ");

					ToString(oFor.Iteration.Expressions[i]);
				}
			}
			WriteStop(")");
			ToString(oFor.Statements);
		}

		void ToString(ForEachStatement oFor)
		{
			WriteStart("foreach (");
			ToString(oFor.Type);
			Write(" ");
			Write(oFor.IteratorName);
			Write(" in ");
			ToString(oFor.Expression);
			Write(")");
			WriteStop();
			ToString(oFor.Statements);
		}




		void ToString(SwitchStatement oSwitch)
		{
			WriteStart("switch (");
			ToString(oSwitch.Expression);
			WriteStop(")");
			ToString(oSwitch.Statements);
		}

	
		void ToString(WhileStatement oWhile)
		{
			WriteStart("while (");
			ToString(oWhile.Expression);
			WriteStop(")");
			ToString(oWhile.Statements);
		}

		void ToString(UsingStatement statement)
		{
			Write("using ");
			if (statement.IsStatic)
				Write("static ");
			Write(string.Join(".", statement.Namespaces));
			WriteStop(";");
		}

		void ToString(YieldStatement statement)
		{
			Write("yield ");
			if (statement.Expression == null)
				Write("break");
			else
			{
				Write("return ");

				ToString(statement.Expression);
			}

			WriteStop(";");
		}

		void ToString(Statement o)
		{
			if (!StringUtil.IsEmpty(o.Comments))
				WriteLine(o.Comments);

			if (o is VariableDeclaration)
				ToString(o as VariableDeclaration);

			else if (o is IfStatement)
				ToString(o as IfStatement);

			//else if (o is MethodDefinition)
			//	ToString(o as MethodDefinition);

			else if (o is FunctionDefinition)
				ToString(o as FunctionDefinition);

			else if (o is ReturnStatement)
				ToString(o as ReturnStatement);

			else if (o is BreakStatement)
				ToString(o as BreakStatement);

			else if (o is ContinueStatement)
				ToString(o as ContinueStatement);

			else if (o is TryStatement)
				ToString(o as TryStatement);
			else if (o is ThrowStatement)
				ToString(o as ThrowStatement);
			else if (o is ForEachStatement)
				ToString(o as ForEachStatement);
			else if (o is ForStatement)
				ToString(o as ForStatement);
			else if (o is WhileStatement)
				ToString(o as WhileStatement);
			else if (o is DefaultStatement)
				ToString(o as DefaultStatement);
			else if (o is CaseStatement)
				ToString(o as CaseStatement);
			else if (o is SwitchStatement)
				ToString(o as SwitchStatement);

			else if (o is DoStatement)
				ToString(o as DoStatement);

			else if (o is ExpressionStatement)
				ToString(o as ExpressionStatement);

			else if (o is CodeBlockStatement)
				ToString((o as CodeBlockStatement).Statements);

			else if (o is YieldStatement)
				ToString((o as YieldStatement));

			else if (o is PropertyDefinition)
				ToString(o as PropertyDefinition);

			else if (o is FieldDefinition)
				ToString(o as FieldDefinition);

			else if (o is ParameterDeclaration)
				ToString(o as ParameterDeclaration);

			//else if (o is ClassDefinition)
			//	ToString(o as ClassDefinition);

			else if (o is PrototypeDefinition)
				ToString(o as PrototypeDefinition);

			else if (o is NamespaceDefinition)
				ToString(o as NamespaceDefinition);

			else if (o is UsingStatement)
				ToString(o as UsingStatement);

			else if (o is Statement)        //From a shadow
				WriteLine("_");

			else
				throw new GenerateFailedException("Could not process object: " + (o == null ? "(null)" : TypeUtil.GetTypeName(o.GetType())));

		}
	}


}
