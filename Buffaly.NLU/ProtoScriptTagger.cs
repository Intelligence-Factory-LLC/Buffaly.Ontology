//added
using Ontology;
using ProtoScript.Interpretter;
using ProtoScript.Interpretter.RuntimeInfo;
using ProtoScript.Parsers;
using Buffaly.NLU.Tagger;
using Buffaly.NLU.Tagger.Nodes;
using WebAppUtilities;
using Debugger = ProtoScript.Interpretter.Debugger;
using ProtoScript;
using File = ProtoScript.File;
using Ontology.Simulation;

namespace Buffaly.NLU
{

	[Serializable]
	public class CompilationException : Exception
	{
		public StatementParsingInfo Info = null;
		public CompilationException() { }
		public CompilationException(string message) : base(message) { }
		public CompilationException(string message, Exception inner) : base(message, inner) { }
		protected CompilationException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
	public class ProtoScriptTagger : Buffaly.NLU.Tagger.Tagger2
	{
		public NativeInterpretter Interpretter;
		public Compiler Compiler;
		public Debugger Debugger;
		public List<global::ProtoScript.Interpretter.Compiled.Statement> Statements; 

		public bool AllowLexemeShortCircuiting = true;
		public bool KeepLexemes = false;
		public bool AllowSubTypingDuringTagging = true;     //N20220915-03 Always true now
		public bool TagIteratively = false;
		public bool AllowAlreadyLinkedSequences = false;
		public bool AllowUnknownLexemes = true;
		public bool IncludePathToStart = true;
		public bool EnableDatabase = true;
		public bool FailOnDiagnosticErrors = true;

		public bool IsInterpretted = false; 

		public string ProjectFile = string.Empty;

		public FunctionRuntimeInfo ExitCondition = null;

		public ProtoScriptTagger()
		{
			Compiler = new Compiler();
			Compiler.Initialize();

			Interpretter = new NativeInterpretter(Compiler);

			//Make the interpretter available. 
			Interpretter.InsertGlobalObject("_interpretter", Interpretter);

			//Make the tagger available 
			Interpretter.InsertGlobalObject("_tagger", this);


			this.MaxIterations = 1000;
		}

		public ProtoScriptTagger(bool bDebug)
		{
			Compiler = new Compiler();
			Compiler.Initialize();

			if (!bDebug)
			{
				Interpretter = new NativeInterpretter(Compiler);				
			}
			else
			{
				this.Debugger = new Debugger(Compiler);
				this.Interpretter = this.Debugger.Interpretter;
			}

			//Make the interpretter available. 
			Interpretter.InsertGlobalObject("_interpretter", Interpretter);

			//Make the tagger available 
			Interpretter.InsertGlobalObject("_tagger", this);


			this.MaxIterations = 1000;
		}

		public void InterpretProject(string strProjectFile)
		{
			List<ProtoScript.Interpretter.Compiled.Statement> lstStatements = null;

			try
			{
				this.IsInterpretted = false;
				this.ProjectFile = strProjectFile;
				lstStatements = Compiler.CompileProject(strProjectFile);
			}
			catch (ProtoScriptTokenizingException err)
			{
				CompilationException compilationException = new CompilationException("Parsing error, Expected: " + err.Expected + ", " + err.Explanation);
				compilationException.Info = new StatementParsingInfo() { StartingOffset = err.Cursor, Length = 1, File = err.File };
				throw compilationException;
			}
			catch (Exception)
			{
				if (Compiler.Diagnostics.Count > 0)
					throw GetCompilationException();

				throw;
			}

			if (Compiler.Diagnostics.Count > 0)
				throw GetCompilationException();


			Logs.DebugLog.CreateTimer("CompileProject.InterpretStatements");

			InterpretStatementList(lstStatements);

			Logs.DebugLog.WriteTimer("CompileProject.InterpretStatements");
		}

		private void InterpretStatementList(List<ProtoScript.Interpretter.Compiled.Statement> lstStatements)
		{
			Interpretter.Source = Compiler.Source;
			this.Statements = lstStatements;

			try
			{
				foreach (ProtoScript.Interpretter.Compiled.Statement statement in lstStatements)
				{
					try
					{
						Interpretter.Evaluate(statement);
					}
					catch (RuntimeException)
					{
						throw;
					}
					catch (Exception err)
					{
						if (null != statement.Info)
						{
							string strStatement = Interpretter.Source.Substring(statement.Info.StartingOffset, statement.Info.Length);
							throw new RuntimeException(strStatement, statement.Info, err);
						}
					}
				}

				this.IsInterpretted = true;
			}
			catch (RuntimeException)
			{
				throw;
			}
			catch (Exception err)
			{
				throw new JsonWsException("Interpretter Error", err);
			}
		}

		private CompilationException GetCompilationException()
		{
			CompilationException compilationException = null;
			
			if (Compiler.Diagnostics.Count > 0)
			{
				CompilerDiagnostic diagnostic = Compiler.Diagnostics.First();
				compilationException = new CompilationException(diagnostic.Diagnostic.Message);

				if (null != diagnostic.Statement)
					compilationException.Info = diagnostic.Statement.Info;
				else if (null != diagnostic.Expression)
					compilationException.Info = diagnostic.Expression.Info;				
			}

			return compilationException;
		}

		public void InterpretFile(string strFile)
		{
			List<ProtoScript.Interpretter.Compiled.Statement> lstStatements = null; 

			try
			{
				lstStatements = Compiler.CompileSingleFile(strFile);
			}
			catch (ProtoScriptTokenizingException err)
			{
				CompilationException compilationException = new CompilationException("Parsing error, Expected: " + err.Expected + ", " + err.Explanation);
				compilationException.Info = new StatementParsingInfo() { StartingOffset = err.Cursor, Length = 1, File = err.File };
				throw compilationException;
			}
			catch (Exception)
			{
				if (Compiler.Diagnostics.Count > 0)
					throw GetCompilationException();

				throw;
			}

			InterpretStatementList(lstStatements);
		}

		public void InterpretCode(string strCode)
		{
			File file = ProtoScript.Parsers.Files.ParseFileContents(strCode);
			ProtoScript.Interpretter.Compiled.File compiledFile = Compiler.Compile(file);


			if (Compiler.Diagnostics.Count > 0)
				throw new JsonWsException("Compilation Error", GetCompilationException());

			Interpretter.Source = Compiler.Source;

			Interpretter.Evaluate(compiledFile);
		}

		public object InterpretImmediate(string strImmediate)
		{
			ProtoScript.Expression statementImmediate = ProtoScript.Parsers.Expressions.Parse(strImmediate);
			ProtoScript.Interpretter.Compiled.Expression compiledImmediate = Compiler.Compile(statementImmediate);

			if (Compiler.Diagnostics.Count > 0)
			{
				throw new Exception("Error in immediate: " + Compiler.Diagnostics.First().Diagnostic.Message);
			}

			object obj = Interpretter.Evaluate(compiledImmediate);

			if (null == obj)
				return null;

			if (obj is bool)
				return obj;

			Prototype protoValue = Interpretter.GetAsPrototype(obj);
			if (null == protoValue)
			{
				if (obj is ValueRuntimeInfo)
				{
					ValueRuntimeInfo info = obj as ValueRuntimeInfo;
					return info.Value;
				}
				
				return obj;
			}

			return protoValue;
		}

		public void InitializeTagger()
		{
			OntologicalCategorizationNode.KeepLexemes = this.KeepLexemes;
			MatchNode.AllowActions = false;
			MatchNode.AllowHypothesizedMatch = false;

			//These are probably OK during a Resume (when we don't reinitialize the tagger) because
			//they use strings as the key, which probably won't match for different runs, or 
			//they will match and be valid
			RangeNode.ResetCache();
			LeafSequenceMatchNode.ResetCache();
			ExactMatchNode.ResetCache();
			FollowExpectationsNode.ResetCache();

			//N20220126-02 - TODO: Still needs work with mult-token lexemes
			RangeNode.AllowRanges = false;
			LeafSequenceMatchNode.AllowCaching = false;


			FollowExpectationsNode.OnUnderstand = protoSequence =>
			{
				Prototype protoResult = null;

				try
				{
					protoSequence = protoSequence.Clone();

					protoResult = OnUnderstand(protoSequence, this);

					if (null != protoResult && !Prototypes.TypeOf(protoResult, Ontology.Collection.Prototype))
					{
						if (AllowSubTypingDuringTagging)
						{
							UnderstandUtil.SubType(protoResult, Interpretter);
							BagOfFeatures.Categorize(protoResult);
						}

						if (AlreadyLinkedSequenceMatchNode.IsEnabled)
						{
							//N20221104-02 - Detect if the last item (the candidate for Already Linked)
							//is preserved or has changed. If it has changed -- such as being passed
							//to SimpleIntepretter.Bind -- then it can't be linked anymore
							Prototype protoLastElement = protoSequence.Children.Last();

							if (PrototypeGraphs.Find(protoResult, x => x == protoLastElement).Count > 0)
							{
								//Note: This adds duplicates [he, ate] [ate, food] -> [he, ate, ate, food]
								//but I don't think it's an issue
								protoResult.Children.AddRange(protoSequence.Children);
							}
						}
					}
				}
				catch (IncompatiblePrototypeParameter)
				{
					//N20220118-03 - Ignore the error as we didn't match the requirements of the method
				}
				return protoResult;
			};

			AlreadyLinkedSequenceMatchNode.OnUnderstand = protoSequence =>
			{
				Prototype protoResult = null;

				try
				{
					//Note: Do not clone protoSequence here or the node cannot determine 
					//if the returned object is part of the linked sequence. 
					protoResult = OnUnderstand(protoSequence, this);

					if (null != protoResult)
					{
						if (AllowSubTypingDuringTagging && !Prototypes.TypeOf(protoResult, Ontology.Collection.Prototype))
						{
							UnderstandUtil.SubType(protoResult, Interpretter);
						}
					}

					//Adding the children must happen in the AlreadyLinkedSequenceMatchNode 
					//so it can check if this is validly linked object
				}
				catch (IncompatiblePrototypeParameter)
				{
					//N20220118-03 - Ignore the error as we didn't match the requirements of the method
				}
				return protoResult;
			};
		}

		static private Prototype OnUnderstand(Prototype prototype, ProtoScriptTagger tagger)
		{
			PrototypeTypeInfo infoPrototype = null;
			object obj = tagger.Interpretter.Symbols.GetGlobalScope().GetSymbol(prototype.PrototypeName);

			//The PreQualifiable annotation creates an object to handle the understand
			if (obj is ValueRuntimeInfo)
				infoPrototype = (obj as ValueRuntimeInfo).Type as PrototypeTypeInfo;
			else if (obj is PrototypeTypeInfo)
				infoPrototype = obj as PrototypeTypeInfo;
			else
				infoPrototype = new PrototypeTypeInfo() { Prototype = obj as Prototype };

			FunctionRuntimeInfo infoFunction = tagger.Interpretter.FindOverriddenMethod(infoPrototype.Prototype, "Understand");
			//FunctionRuntimeInfo infoFunction = infoPrototype.Scope.GetSymbol("Understand") as FunctionRuntimeInfo;
			object oRes = null;
			if (null != infoFunction)
			{
				if (infoFunction.Parameters.Count == 1)
					oRes = tagger.Interpretter.RunMethod(infoFunction, null, new List<object>() { prototype });
				else
					oRes = tagger.Interpretter.RunMethod(infoFunction, prototype, new List<object>() { });
			}

			Prototype protoResult = SimpleInterpretter.GetAsPrototype(oRes);

			if (null != protoResult)
			{
				Logs.DebugLog.WriteEvent(infoFunction.ParentPrototype?.PrototypeName + "." + infoFunction.FunctionName, "\r\n" + PrototypeLogging.ToFriendlyShadowString2(protoResult));
			}

			return protoResult;
		}

		public Prototype TagLexemes(Prototype protoFragment)
		{
			//Test short circuiting ontological categorization
			for (int i = 0; i < protoFragment.Children.Count; i++)
			{
				Prototype protoLexeme = protoFragment.Children[i];
				if (Prototypes.TypeOf(protoLexeme, Lexeme.Prototype))
				{
					TemporaryLexeme? lexeme = protoLexeme as TemporaryLexeme;
					if (null == lexeme)
						throw new Exception("Lexeme prototype is not a TemporaryLexeme: " + protoLexeme.PrototypeName);

					if (lexeme.LexemePrototypes.Count == 1)
					{
						Prototype protoLexemePrototype = lexeme.LexemePrototypes.First().Key.Clone();

						if (this.KeepLexemes)
							protoLexemePrototype.Children.Add(protoFragment.Children[i].Clone());	//Clone so it preserves any tokens

						protoFragment.Children[i] = protoLexemePrototype;
					}

				}
			}

			return protoFragment;
		}

		private TaggingNode m_TaggingNode = null;
		public TaggingNode TaggingNode
		{
			get
			{
				if (null == m_TaggingNode)
					m_TaggingNode = new TaggingNode();

				return m_TaggingNode;
			}
		}

		public Prototype Tag(Prototype protoFragment)
		{
			WriteEvent("Settings", "");
			WriteEvent("AllowLexemeShortCircuiting", this.AllowLexemeShortCircuiting.ToString());
			WriteEvent("AllowAlreadyLinkedSequences", this.AllowAlreadyLinkedSequences.ToString());
			WriteEvent("AllowSubTypingDuringTagging", this.AllowSubTypingDuringTagging.ToString());
			WriteEvent("AllowUnknownLexemes", this.AllowUnknownLexemes.ToString());
			if (null != this.ExitCondition)
				WriteEvent("ExitCondition", this.ExitCondition.FunctionName.ToString());

			WriteEvent("TagIteratively", this.TagIteratively.ToString());
			WriteEvent("Interpretter", this.Interpretter.GetType().Name);

			AlreadyLinkedSequenceMatchNode.IsEnabled = this.AllowAlreadyLinkedSequences;

			if (AllowLexemeShortCircuiting)
			{
				protoFragment = TagLexemes(protoFragment);
			}

			this.m_TaggingNode = null; //reset each run
			this.CurrentIterations = 0;

			if (null != this.ExitCondition)
			{
				this.TaggingNode.ExitCondition = x =>
				{
					return ExitConditionWrapper(x);
				};
			}

			TaggingNode node = this.TaggingNode;
			node.Source = protoFragment;

			this.IsStopped = false;
			SingleResult result = this.ControlRoot(node) as SingleResult;
			
			if (null != result)
				return result.Result;

			var node2 = node.Possibilities.OrderByDescending(x => x.Value).First();

			foreach (var nodeChild in node.Possibilities.OrderByDescending(x => x.Value))
			{
				Logs.DebugLog.WriteEvent("Node", nodeChild.ToString());
			}

			return (node2 as Buffaly.NLU.Tagger.BaseFunctionNode).Source;
		}

		public Prototype TagNext()
		{
			TaggingNode node = m_TaggingNode;
			SingleResult result = this.ControlRoot(node) as SingleResult;
			return result?.Result;
		}

		private bool ExitConditionWrapper(Prototype prototype)
		{
			object oRes = this.Interpretter.RunMethod(this.ExitCondition, null, new List<object>() { prototype });

			return (bool)oRes;
		}
	}
}
