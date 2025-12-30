using BasicUtilities.Collections;
using BasicUtilities;
using Ontology.Simulation;
using Ontology;
using ProtoScript.Parsers;
using ProtoScript;
using Ontology.BaseTypes;

namespace Buffaly.NLU
{
	public class PrototypeDefinitionHelpers
	{
		static public string MaterializePrototypeToString(Prototype prototype, bool bGenerateLexemes)
		{
			PrototypeDefinition protoDef = PrototypeDefinitionHelpers.PrototypeToPrototypeDefinition(prototype, bGenerateLexemes);
			return SimpleGenerator.Generate(protoDef);
		}

		static public void AppendCodeToLocalData(string strCode, ProtoScriptTagger tagger)
		{
			string strDirectory = StringUtil.LeftOfLast(tagger.ProjectFile, "\\") + "\\LocalData.pts";
			FileUtil.AppendFile(strDirectory, strCode);
		}

		static public PrototypeDefinition? GetPrototypeDefinitionOrNull(string strFile, string strPrototypeName)
		{
			if (!System.IO.File.Exists(strFile))
				return null;

			ProtoScript.File parsedFile = ProtoScript.Parsers.Files.Parse(strFile);
			PrototypeDefinition? protoExisting = null;
			foreach (var proto in parsedFile.PrototypeDefinitions)
			{
				if (StringUtil.EqualNoCase(proto.PrototypeName.TypeName, strPrototypeName))
				{
					return proto;

				}
			}

			return null;
		}

		static public void InsertOrUpdatePrototypeDefinition(PrototypeDefinition protoDef, string strFile)
		{
			//>check if the file exists, if not create the file with just this protoDef in it
			if (!System.IO.File.Exists(strFile))
			{
				string strNewDefinition = SimpleGenerator.Generate(protoDef);
				FileUtil.WriteFile(strFile, strNewDefinition);
			}
			else
			{
				//>+ parse the strFile into a ProtoScript.File. Then look for the prototype definition and replace it
				ProtoScript.File parsedFile = ProtoScript.Parsers.Files.Parse(strFile);
				PrototypeDefinition? protoExisting = null;
				foreach (var proto in parsedFile.PrototypeDefinitions)
				{
					if (StringUtil.EqualNoCase(proto.PrototypeName.TypeName, protoDef.PrototypeName.TypeName))
					{
						protoExisting = proto;
						break;
					}
				}

				//> if the protoExisting is null just append this definition
				if (protoExisting == null)
				{
					string strNewDefinition = SimpleGenerator.Generate(protoDef);

					string strContents = FileUtil.ReadFile(strFile);
					if (strContents.Contains("namespace"))
					{
						strContents = StringUtil.LeftOfLast(strContents, "}") + strNewDefinition + "\r\n}";
						FileUtil.WriteFile(strFile, strContents);
					}
					else
					{
						FileUtil.AppendFile(strFile, strNewDefinition);
					}
				}
				else
				{
					//>read the file contents
					string strFileContents = FileUtil.ReadFile(strFile);

					//>get the start offset as the starting offset of the earliest annotation, or of the definition if that doesn't exist
					int iStartOffset = protoExisting.Annotations.Any()
						? protoExisting.Annotations.Min(annotation => annotation.Info.StartingOffset)
						: protoExisting.Info.StartingOffset;

					//>remove the old PrototypeDefinition
					strFileContents = strFileContents.Remove(iStartOffset, protoExisting.Info.StoppingOffset - iStartOffset);

					string strNewDefinition = SimpleGenerator.Generate(protoDef);
					strFileContents = strFileContents.Insert(iStartOffset, strNewDefinition);

					//>write the new file contents
					FileUtil.WriteFile(strFile, strFileContents);
				}
			}
		}

		public static List<PrototypeDefinition> GetNewPrototypeDefinitions(int iCount, bool bGenerateLexemes)
		{
			List<PrototypeDefinition> listPrototypeDefinition = new List<PrototypeDefinition>();
			List<Prototype> lstNewPrototypes = TemporaryPrototypes.GetAllTemporaryPrototypes().Where(x => x.PrototypeID <= -1 - iCount).ToList();


			foreach (Prototype prototype in lstNewPrototypes)
			{
				if (Prototypes.TypeOf(prototype, Lexeme.Prototype))
				{
					continue;
				}

				if (bGenerateLexemes && Prototypes.TypeOf(prototype, "MultiTokenPhrase"))
				{
					continue;
				}

				if (prototype.IsInstance())
				{
					continue;
				}

				if (Prototypes.TypeOf(prototype, System_String.Prototype))
				{
					continue;
				}

				PrototypeDefinition prototypeDefinition2 = PrototypeToPrototypeDefinition(prototype, bGenerateLexemes);

				listPrototypeDefinition.Add(prototypeDefinition2);
			}

			return listPrototypeDefinition;
		}

		static public PrototypeDefinition PrototypeToPrototypeDefinition(Prototype prototype, bool bGenerateLexemes)
		{
			PrototypeDefinition prototypeDefinition = new PrototypeDefinition();
			prototypeDefinition.PrototypeName = new ProtoScript.Type() { TypeName = prototype.PrototypeName };

			foreach (int protoTypeOf in prototype.GetTypeOfs())
			{
				prototypeDefinition.Inherits.Add(new ProtoScript.Type() { TypeName = Prototypes.GetPrototypeName(protoTypeOf) });
			}

			if (bGenerateLexemes)
			{
				foreach (TemporaryLexeme protoLexeme in TemporaryLexemes.GetLexemesByRelatedPrototype(prototype))
				{
					Prototype ? protoRelated = protoLexeme.LexemePrototypes.FirstOrDefault(x => Prototypes.TypeOf(x.Key, prototype)).Key;

					//Look for Plural form 
					if (!prototype.ShallowEqual(protoRelated))
						prototypeDefinition.Annotations.Add(AnnotationExpressions.Parse($"[global.Lexeme.Plural(\"{protoLexeme.Lexeme.ToLower()}\")]"));
					else
						prototypeDefinition.Annotations.Add(AnnotationExpressions.Parse($"[global.Lexeme.Singular(\"{protoLexeme.Lexeme.ToLower()}\")]"));
				}

				//Get the multi-token phrases, but not to the parent
				List<Prototype> lstSequences = MultiTokenPhrases.GetMultiTokenPhrasesTargeting(prototype, false);

				Set<string> setAdded = new Set<string>();

				foreach (Prototype sequence in lstSequences)
				{
					string strKey = string.Join(", ", sequence.Children.Select(x => x.PrototypeName).ToArray());
					if (!setAdded.Contains(strKey))
					{
						prototypeDefinition.Annotations.Add(AnnotationExpressions.Parse($"[global.Lexeme.MultiToken([{strKey}])]"));
						setAdded.Add(strKey);
					}
				}
			}

			PrototypeInitializer prototypeInitializer = new PrototypeInitializer();

			foreach (var pair in prototype.NormalProperties)
			{
				BinaryOperator assignmentOperator = new BinaryOperator();
				assignmentOperator.Value = "=";
				assignmentOperator.Left = new Identifier(StringUtil.RightOfLast(Prototypes.GetPrototypeName(pair.Key), "."));
				assignmentOperator.Right = PrototypeToExpression(pair.Value);

				prototypeInitializer.Statements.Add(new ExpressionStatement { Expression = assignmentOperator });
			}

			if (prototype.Children.Count > 0)
			{
				BinaryOperator assignmentOperator = new BinaryOperator();
				assignmentOperator.Value = "=";
				assignmentOperator.Left = new Identifier("Children");

				ArrayLiteral arrayLiteral = new ArrayLiteral();
				foreach (Prototype child in prototype.Children)
				{
					arrayLiteral.Values.Add(PrototypeToExpression(child));
				}

				assignmentOperator.Right = arrayLiteral;								

				prototypeInitializer.Statements.Add(new ExpressionStatement { Expression = assignmentOperator });

			}

			if (prototypeInitializer.Statements.Count > 0)
				prototypeDefinition.Initializers.Add(prototypeInitializer);


			foreach (var pair in prototype.Associations)
			{
				//Later check for uni-directional associations
				prototypeDefinition.Annotations.Add(AnnotationExpressions.Parse($"[BidirectionalAssociation({pair.Key.PrototypeName})]"));
			}

			prototypeDefinition.IsPartial = true;

			return prototypeDefinition;
		}

		static private ArrayLiteral CollectionToArrayLiteral(Prototype protoCollection)
		{
			ArrayLiteral arrayLiteral = new ArrayLiteral();
			foreach (Prototype proto in protoCollection.Children)
			{
				arrayLiteral.Values.Add(PrototypeToExpression(proto));
			}

			return arrayLiteral;
		}

		static private Expression PrototypeToExpression(Prototype prototype)
		{
			Expression ? expression = null;

			if (prototype.TypeOf(System_String.Prototype))
			{
				string strValue = StringWrapper.ToString(prototype);
				if (strValue.Contains("\n") && !strValue.Contains("\""))
					expression = new StringLiteral($"@\"{strValue}\"");
				else
					expression = new StringLiteral(StringHelper.EscapeStringForCSharpLiteral(strValue));
			}
			else if (prototype.TypeOf(System_Int32.Prototype))
			{
				expression = new IntegerLiteral(IntWrapper.ToInteger(prototype).ToString());
			}
			else if (prototype.TypeOf(System_Boolean.Prototype))
			{
				expression = new BooleanLiteral(BoolWrapper.ToBoolean(prototype));
			}
			else if (Prototypes.TypeOf(prototype, System_Double.Prototype))
			{
				expression = new DoubleLiteral(DoubleWrapper.ToDouble(prototype).ToString("G17"));
			}
			
			else if (Prototypes.TypeOf(prototype, Collection.Prototype))
				return CollectionToArrayLiteral(prototype);

			if (expression != null)
				return expression;

			return new Identifier(prototype.PrototypeName);


		}

	}
}
