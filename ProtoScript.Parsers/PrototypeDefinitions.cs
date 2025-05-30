﻿namespace ProtoScript.Parsers
{
	public class PrototypeDefinitions
	{
		static public ProtoScript.PrototypeDefinition Parse(string strStatement)
		{
			Tokenizer tok = new Tokenizer(strStatement);
			return Parse(tok);
		}

		static public ProtoScript.PrototypeDefinition Parse(Tokenizer tok)
		{
			ProtoScript.PrototypeDefinition result = new ProtoScript.PrototypeDefinition();

			//Added so this can be called independently of a namespace (like the PtsGenerator)
			while (tok.peekNextToken() == "[")
			{
				result.Annotations.Add(ProtoScript.Parsers.AnnotationExpressions.Parse(tok));
			}

			tok.movePastWhitespace(); result.Info.StartStatement(tok.getCursor());
			result.Info.File = Files.CurrentFile;

			ModifiersState(tok, result);

			if (tok.peekNextToken() == "enum")
				throw new LookAheadException("enum");

			tok.MustBeNext("prototype");

			result.PrototypeName = ProtoScript.Parsers.Types.Parse(tok);

			InheritanceState(tok, result);

			if (tok.CouldBeNext("where"))		//ignroe the where clause for now
				tok.moveTo("{");

			if (tok.CouldBeNext("{"))
			{
				List<AnnotationExpression> lstAnnotations = new List<AnnotationExpression>();

				while (!tok.CouldBeNext("}"))
				{
					string strToken = tok.peekNextToken();

					switch (strToken)
					{
						case "function":
							{
								FunctionDefinition function = FunctionDefinitions.Parse(tok);

								if (lstAnnotations.Count > 0)
								{
									function.Annotations = lstAnnotations;
									lstAnnotations = new List<AnnotationExpression>();
								}

								result.Methods.Add(function);
								break;
							}

						case "[":
							{
								lstAnnotations.Add(ProtoScript.Parsers.AnnotationExpressions.Parse(tok));
								break;
							}

						case "init":
							{
								PrototypeInitializer initializer = PrototypeInitializers.Parse(tok);
								result.Initializers.Add(initializer);
								break;
							}

						case "prototype":
							{
								PrototypeDefinition prototypeDefinition = PrototypeDefinitions.Parse(tok);
								if (lstAnnotations.Count > 0)
								{
									prototypeDefinition.Annotations = lstAnnotations;
									lstAnnotations = new List<AnnotationExpression>();
								}

								result.PrototypeDefinitions.Add(prototypeDefinition);
								break;
							}

						default:
							{
								int iCursor = tok.getCursor();
								FieldDefinition field = FieldDefinitions.Parse(tok);
                                if (field == null)
                                {
                                    tok.setCursor(iCursor);

									//Initializer short form
									Statement statement = Statements.Parse(tok);
									if (result.Initializers.Count == 0)
                                    {
                                        //This is a short form initializer
                                        PrototypeInitializer initializer = new PrototypeInitializer();
                                        result.Initializers.Add(initializer);
                                    }

									result.Initializers.Last().Statements.Add(statement);

                                    break;
                                }

                                if (lstAnnotations.Count > 0)
								{
									field.Annotations = lstAnnotations;
									lstAnnotations = new List<AnnotationExpression>();
								}

								result.Fields.Add(field);
								break;
							}
					}
				}
			}
			else
			{
				//Short form
				tok.MustBeNext(";");

				//Check for a common mistak 
				//prototype Test;
				//{
				// unreachable
				//}

				if (tok.CouldBeNext("{"))
					throw new ProtoScriptParsingException(tok.getString(), tok.getCursor(), "not a {", "Short form followed by declaration block is most likely an error");
			}
			
			result.Info.StopStatement(tok.getCursor());
			return result;

		}


		static void ModifiersState(Tokenizer tok, ProtoScript.PrototypeDefinition result)
		{
			ProtoScript.Modifiers modifiers = ProtoScript.Parsers.Modifiers.Parse(tok);
			result.SetModifiers(modifiers);
		}

		static void InheritanceState(Tokenizer tok, ProtoScript.PrototypeDefinition result)
		{
			if (tok.CouldBeNext(":"))
			{
				do
				{
					ProtoScript.Type type = ProtoScript.Parsers.Types.Parse(tok);
					result.Inherits.Add(type);
				}
				while (tok.CouldBeNext(","));
			}
		}
	}
}
