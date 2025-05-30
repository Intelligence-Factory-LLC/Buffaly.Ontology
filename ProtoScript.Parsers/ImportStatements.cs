namespace ProtoScript.Parsers
{
	public class ImportStatements
	{
		static public ProtoScript.ImportStatement Parse(string strStatement)
		{
			Tokenizer tok = new Tokenizer(strStatement);
			return Parse(tok);
		}

		public static ProtoScript.ImportStatement Parse(Tokenizer tok)
		{
			ProtoScript.ImportStatement result = new ProtoScript.ImportStatement();

			tok.movePastWhitespace(); result.Info.StartStatement(tok.getCursor());
			result.Info.File = Files.CurrentFile;

			tok.MustBeNext("import");

			result.Reference = ProtoScript.Parsers.Identifiers.ParseMultiple(tok);
			result.Type = ProtoScript.Parsers.Identifiers.ParseMultiple(tok);
			result.Import = ProtoScript.Parsers.Identifiers.ParseMultiple(tok);

			tok.MustBeNext(";");

			result.Info.StopStatement(tok.getCursor());

			return result;
		}
	}
}
