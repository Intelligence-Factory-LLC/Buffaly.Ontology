using BasicUtilities;

namespace ProtoScript.Parsers
{
	public class IncludeStatements
	{
		static public ProtoScript.IncludeStatement Parse(string strStatement)
		{
			Tokenizer tok = new Tokenizer(strStatement);
			return Parse(tok);
		}

		public static ProtoScript.IncludeStatement Parse(Tokenizer tok)
		{
			ProtoScript.IncludeStatement result = new ProtoScript.IncludeStatement();

			tok.movePastWhitespace(); result.Info.StartStatement(tok.getCursor());
			result.Info.File = Files.CurrentFile;

			tok.MustBeNext("include");

			if (tok.CouldBeNext("recursive"))
			{
				result.Recursive = true;
			}

			result.FileName = ProtoScript.Parsers.Identifiers.ParseMultiple(tok);

			tok.MustBeNext(";");

			if (result.FileName.StartsWith("@"))
				result.FileName = result.FileName.Substring(1);

			if (result.FileName.StartsWith("\"") && result.FileName.EndsWith("\""))
				result.FileName = StringUtil.BetweenQuotes(result.FileName);



			result.Info.StopStatement(tok.getCursor());

			return result;
		}
	}
}
