namespace ProtoScript.Parsers
{
	public class ReferenceStatements
	{
		static public ProtoScript.ReferenceStatement Parse(string strStatement)
		{
			Tokenizer tok = new Tokenizer(strStatement);
			return Parse(tok);
		}

		public static ProtoScript.ReferenceStatement Parse(Tokenizer tok)
		{
			ProtoScript.ReferenceStatement result = new ProtoScript.ReferenceStatement();
			
			tok.movePastWhitespace();
			result.Info.StartStatement(tok.getCursor());
			result.Info.File = Files.CurrentFile;
			
			tok.MustBeNext("reference");
			
			result.AssemblyName = ProtoScript.Parsers.Identifiers.ParseMultiple(tok);
			if (tok.CouldBeNext(";"))
			{
				result.Reference = result.AssemblyName;
			}
			else
			{
				result.Reference = ProtoScript.Parsers.Identifiers.ParseMultiple(tok);
				tok.MustBeNext(";");
			}
			
			
			result.Info.StopStatement(tok.getCursor());
			
			return result;
		}
	}
}
