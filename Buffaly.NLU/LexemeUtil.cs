using Ontology.Simulation;
using Ontology;
using Ontology.BaseTypes;

namespace Buffaly.NLU
{
	public class LexemeUtil
	{
		static public List<Prototype> InsertLexemesForSemanticEntity(Prototype prototype, string strEntity, ProtoScriptTagger tagger)
		{
			var tuple = CreateUnknownWordsAsTokens(strEntity);

			List<Prototype> lstNewPrototypes = tuple.Item1;
			Prototype protoLexemes = tuple.Item2;

			//Insert a multi-token lexeme 
			if (protoLexemes.Children.Count > 1 /*&& lstNewPrototypes.Count > 0*/)
			{
				List<Prototype> lstParts = new List<Prototype>();

				for (int i = 0; i < protoLexemes.Children.Count; i++)
				{
					bool bIsLast = i == protoLexemes.Children.Count - 1;

					Prototype protoLexeme = protoLexemes.Children[i];
					if (!Prototypes.TypeOf(protoLexeme, Lexeme.Prototype))
						throw new Exception("Unexpected");

					Prototype protoRelated = GetRelatedEntityAsObjectOrTokenOrDefault((TemporaryLexeme)protoLexeme);
					if (null == protoRelated)
					{
						//The code above here should always create the related entity
						throw new Exception("Unexpected");
					}

					if (protoRelated.IsInstance())
						protoRelated = protoRelated.GetBaseType();

					lstParts.Add(protoRelated);
				}



				tagger.Interpretter.RunMethodAsPrototype("Lexeme", "MultiToken", new List<object> { prototype, new Collection(lstParts) });


				lstNewPrototypes.Add(prototype);
			}
			else
			{
				TemporaryLexemes.InsertRelatedPrototype(protoLexemes.Children.First(), prototype, false);
			}

			return lstNewPrototypes;
		}

		static public Tuple<List<Prototype>, Prototype> CreateUnknownWordsAsTokens(string strPhrase)
		{
			Prototype protoTokens = UnderstandUtil.Tokenize(strPhrase);
			Prototype protoLexemes = UnderstandUtil.ConvertToLexemes(protoTokens);

			List<Prototype> lstNewPrototypes = new List<Prototype>();

			for (int i = 0; i < protoLexemes.Children.Count; i++)
			{
				Prototype protoToken = protoLexemes.Children[i];
				if (Prototypes.TypeOf(protoToken, System_String.Prototype))
				{
					string strToken = StringWrapper.ToString(protoToken);
					var tuple = CreateUnknownWordAsToken(strToken);
					lstNewPrototypes.Add(tuple.Item1);
					protoLexemes.Children[i] = tuple.Item2;
				}
			}

			return new Tuple<List<Prototype>, Prototype>(lstNewPrototypes, protoLexemes);
		}


		public static Tuple<Prototype, Prototype> CreateUnknownWordAsToken(string strSingular)
		{
			Prototype protoUnknown = TemporaryPrototypes.GetOrCreateTemporaryPrototype(strSingular);
			protoUnknown.InsertTypeOf("Token");

			Prototype protoLexeme = TemporaryLexemes.GetOrInsertLexeme(strSingular, protoUnknown);
			return new Tuple<Prototype, Prototype>(protoUnknown, protoLexeme);
		}


		public static Prototype ? GetRelatedEntityAsObjectOrToken(string strPart)
		{
			TemporaryLexeme? lexeme = TemporaryLexemes.GetLexemeByLexeme(strPart);
			if (null == lexeme || lexeme.LexemePrototypes.Count == 0)
				return null;

			Prototype protoRelated = lexeme.LexemePrototypes.FirstOrDefault(x => Prototypes.TypeOf(x.Key, "BaseObject")).Key;

			if (null == protoRelated)
				protoRelated = lexeme.LexemePrototypes.FirstOrDefault(x => Prototypes.TypeOf(x.Key, "Token")).Key;

			return protoRelated;
		}

		public static Prototype GetRelatedEntityAsObjectOrTokenOrDefault(string strPart)
		{
			TemporaryLexeme? lexeme = TemporaryLexemes.GetLexemeByLexeme(strPart);
			if (null == lexeme || lexeme.LexemePrototypes.Count == 0)
				return null;

			return GetRelatedEntityAsObjectOrTokenOrDefault(lexeme);
		}



		//>Consolidate the two previous methods by extracting a common third method taking rowLexeme
		public static Prototype GetRelatedEntityAsObjectOrTokenOrDefault(TemporaryLexeme rowLexeme)
		{
			Prototype protoRelated = rowLexeme.LexemePrototypes.FirstOrDefault(x => Prototypes.TypeOf(x.Key, "BaseObject")).Key;

			if (null == protoRelated)
				protoRelated = rowLexeme.LexemePrototypes.FirstOrDefault(x => Prototypes.TypeOf(x.Key, "Token")).Key;

			if (null == protoRelated)
				protoRelated = rowLexeme.LexemePrototypes.First().Key;

			return protoRelated;
		}
	}
}
