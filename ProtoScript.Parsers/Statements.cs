namespace ProtoScript.Parsers
{
	public class Statements
	{
		static public ProtoScript.Statement ParseBestCase(string strStatement)
		{
			ProtoScript.Parsers.Settings.BestCaseExpressions = true;
			Statement statement = Parse(strStatement);
			ProtoScript.Parsers.Settings.BestCaseExpressions = false;
			return statement;
		}

		static public ProtoScript.Statement Parse(string strStatement)
		{
			Tokenizer tok = new Tokenizer(strStatement);
			return Parse(tok);
		}

		static public ProtoScript.Statement Parse(Tokenizer tok)
		{
			string strTok = tok.peekNextToken();
			int iCursor = tok.getCursor();

			try
			{

				switch (strTok)
				{
					case "return":
						return ProtoScript.Parsers.ReturnStatements.Parse(tok);

					case "switch":
						return ProtoScript.Parsers.SwitchStatements.Parse(tok);

					case "case":
						return ProtoScript.Parsers.SwitchStatements.ParseCaseStatement(tok);

					case "default":
						return ProtoScript.Parsers.SwitchStatements.ParseDefaultStatement(tok);

					case "continue":
						ContinueStatement contStatement = new ContinueStatement();
						contStatement.Info.StartStatement(iCursor);
						contStatement.Info.File = Files.CurrentFile;

						tok.getNextToken();

						contStatement.Info.StopStatement(tok.getCursor());

						tok.MustBeNext(";");
						return contStatement;

					case "break":
						BreakStatement breakStatement = new BreakStatement();
						breakStatement.Info.StartStatement(iCursor);
						breakStatement.Info.File = Files.CurrentFile;

						tok.getNextToken();
						breakStatement.Info.StopStatement(tok.getCursor());
						tok.MustBeNext(";");

						return breakStatement;

					case "throw":
						return ProtoScript.Parsers.ThrowStatements.Parse(tok);


					case "try":
						return ProtoScript.Parsers.TryStatements.Parse(tok);

					case "while":
						return ProtoScript.Parsers.WhileStatements.Parse(tok);
					case "do":
						return ProtoScript.Parsers.DoStatements.Parse(tok);

					case "foreach":
						return ProtoScript.Parsers.ForEachStatements.Parse(tok);

					case "if":
						return ProtoScript.Parsers.IfStatements.Parse(tok);

					case "for":
						return ProtoScript.Parsers.ForStatements.Parse(tok);


					case "{":
						CodeBlockStatement statement = new CodeBlockStatement(ProtoScript.Parsers.CodeBlocks.Parse(tok));
						return statement;

					case ";":
						tok.getNextToken();
						throw new ProtoScriptParsingException(tok.getString(), tok.getCursor(), "not empty statement");

					case "yield":
						return ProtoScript.Parsers.YieldStatements.Parse(tok);

					case "function":
						return ProtoScript.Parsers.FunctionDefinitions.Parse(tok);

					default:
						return ParseDeclarationOrExpression(tok, false);
				}
			}
			catch (Exception err)
			{
				if (Settings.FailOnParsingErrors)
					throw Logs.LogError(err);
				else
				{
					tok.setCursor(iCursor); //go back to the start of the line, then move past the end of the line, in case we consumed the end of the line already
					tok.movePast("\n");       //N20200505-02 - Try to recover on the next line
					Logs.DebugLog.WriteErrorPretty(err);
				}
			}

			return null;

		}

		public static ProtoScript.Statement ParseDeclarationOrExpression(Tokenizer tok, bool bNaked)
		{
			int iCursor = tok.getCursor();
			ProtoScript.Statement statement = null;
			ProtoScriptTokenizingException savedException = null;
			try
			{
				statement = ProtoScript.Parsers.VariableDeclarations.ParseWithoutExceptions(tok, bNaked);

				if (null != statement && (statement as VariableDeclaration).Type == null)
					statement = null;
			}
			catch (ProtoScriptTokenizingException err)
			{
				savedException = err;
				statement = null; 
			}

			if (null == statement)
			{
				try
				{
					tok.setCursor(iCursor);
					statement = ProtoScript.Parsers.ExpressionStatements.Parse(tok, bNaked);
				}
				catch (ProtoScriptTokenizingException err2)
				{
					tok.setCursor(iCursor);

					if (null != savedException)
						err2.Explanation = savedException.Explanation + ", " + err2.Explanation;

					throw err2;
				}
			}

			return statement;
		}
	}

}
