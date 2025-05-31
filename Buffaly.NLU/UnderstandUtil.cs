//added
using BasicUtilities;
using BasicUtilities.Collections;
using Ontology;
using Ontology.Simulation;
using ProtoScript.Interpretter;
using System.Text;
using Buffaly.NLU.Tagger;
using Buffaly.NLU.Tagger.Nodes;
using WebAppUtilities;
using ProtoScript;

namespace Buffaly.NLU
{
	public class UnderstandUtil
	{
		public class TaggingSettings
		{
			public bool AllowRanges = false;
			public int MaxIterations = 1000;
			public bool EnableLogging = true;
			public string Project = null;
			public bool Fragment = false;

			public bool TagAfterFragment = true;
			public bool TagIteratively = false;

			public bool IncludeMeaning = false;
			public string MeaningDimension = null;
			public bool TransferRecursively = true;

			public bool AllowAlreadyLinkedSequences = true;
			public bool AllowUnknownLexemes = true;
			public Map<string, object> GlobalObjects = new Map<string, object>();
			public bool Debug = false;
			public bool PathToStart = true;
            public bool EnableDatabase = false;
			public bool FailOnDiagnosticErrors = true;
			public bool Resume = false;
			public bool AllowPrecompiled = false;


			public delegate void OnInitializedDelegate();
			public OnInitializedDelegate OnInitialized = null;

		}

		static public Prototype Understand(string strPhrase)
		{
			return Understand(strPhrase, new TaggingSettings() { AllowRanges = false, MaxIterations = 1000, Project = @"C:\dev\ai\Ontology.Simulation\ProtoScript.Tests\NL_Project\Project.pts" });
		}


		static public Prototype Understand(string strPhrase, TaggingSettings settings)
		{
			return Understand2(strPhrase, settings).Result;
		}

		static public Prototype Understand3(string strPhrase, ProtoScriptTagger tagger)
		{
			return Understand2(strPhrase, tagger).Result;
		}

		public class UnderstandResult
		{
			public Prototype Result = null;
			public int Iterations = 0;
			public string Error = null;
			public StatementParsingInfo ErrorStatement = null;
			public ProtoScriptTagger Tagger = null;
			public Collection Sememes = new Collection();
			public string PathToStart = null;

		}
		static public ProtoScriptTagger GetDefaultProtoScriptTagger()
		{
			Ontology.Initializer.Initialize();
			return GetAndInitializeProtoScriptTagger(new TaggingSettings() { AllowRanges = false, MaxIterations = 1000, Project = @"C:\dev\ai\Ontology.Simulation\ProtoScript.Tests\NL_Project\Project.pts" });
		}

		static public ProtoScriptTagger GetAndInitializeProtoScriptTagger(TaggingSettings settings)
		{
			string strFile = settings.Project;

			ProtoScriptTagger tagger = new ProtoScriptTagger(settings.Debug);

			foreach (var pair in settings.GlobalObjects)
			{
				tagger.Interpretter.InsertGlobalObject(pair.Key, pair.Value);
			}



            if (!settings.EnableDatabase)
            {
                SetupDatabaseDisconnectedMode();
            }

			tagger.EnableDatabase = settings.EnableDatabase;
			tagger.FailOnDiagnosticErrors = settings.FailOnDiagnosticErrors;

			ProtoScript.Parsers.Settings.AllowPrecompiled = settings.AllowPrecompiled;

			tagger.InterpretProject(strFile);
			tagger.InitializeTagger();
			if (!settings.EnableLogging)
			{
				Buffaly.NLU.Tagger.RolloutController.Controller.Log.DebugLevel = DebugLog.debug_level_t.SILENT;
				tagger.Interpretter.LogMethodCalls = false;
			}


			Buffaly.NLU.Tagger.Nodes.GetExpectationsNode.FilterPossiblePrototypes = true;
			Buffaly.NLU.Tagger.Nodes.RangeNode.AllowRanges = settings.AllowRanges;

			tagger.MaxIterations = settings.MaxIterations;
			tagger.TagIteratively = settings.TagIteratively;
			tagger.AllowAlreadyLinkedSequences = settings.AllowAlreadyLinkedSequences;
			tagger.AllowUnknownLexemes = settings.AllowUnknownLexemes;
			tagger.IncludePathToStart = settings.PathToStart;

			if (null != settings.OnInitialized)
				settings.OnInitialized();

			return tagger;
		}

        static public void SetupDatabaseDisconnectedMode()
        {
            TemporaryPrototypes.GetOrCreateTemporaryPrototype(Ontology.Lexeme.PrototypeName);
            TemporaryPrototypes.GetOrCreateTemporaryPrototype(Ontology.Collection.PrototypeName);
            TemporaryPrototypes.GetOrCreateTemporaryPrototype(Ontology.Sequence.PrototypeName);
			TemporaryPrototypes.GetOrCreateTemporaryPrototype(Possibilities.PrototypeName);
			


			TemporaryPrototypes.GetOrCreateTemporaryPrototype(Ontology.BaseTypes.System_String.PrototypeName);
            TemporaryPrototypes.GetOrCreateTemporaryPrototype(Ontology.BaseTypes.System_Boolean.PrototypeName);
            TemporaryPrototypes.GetOrCreateTemporaryPrototype(Ontology.BaseTypes.System_Double.PrototypeName);
            TemporaryPrototypes.GetOrCreateTemporaryPrototype(Ontology.BaseTypes.System_Int32.PrototypeName);

            TemporaryPrototypes.GetOrCreateTemporaryPrototype(Compare.Comparison.PrototypeName);
			TemporaryPrototypes.GetOrCreateTemporaryPrototype(Compare.Exact.PrototypeName);
			TemporaryPrototypes.GetOrCreateTemporaryPrototype(Compare.Ignore.PrototypeName);
			TemporaryPrototypes.GetOrCreateTemporaryPrototype(Compare.StartsWith.PrototypeName);
            TemporaryPrototypes.GetOrCreateTemporaryPrototype(Compare.Intersection.PrototypeName);

            //These are created by ProtoScript but only after checking the database
            TemporaryPrototypes.GetOrCreateTemporaryPrototype("Ontology.Prototype");
            TemporaryPrototypes.GetOrCreateTemporaryPrototype("Ontology.Simulation.StringWrapper");
            TemporaryPrototypes.GetOrCreateTemporaryPrototype("Ontology.Simulation.BoolWrapper");
            TemporaryPrototypes.GetOrCreateTemporaryPrototype("Ontology.Simulation.IntWrapper");
            TemporaryPrototypes.GetOrCreateTemporaryPrototype("Ontology.Simulation.IntWrapper");
            TemporaryPrototypes.GetOrCreateTemporaryPrototype("Ontology.Simulation.TypedCollection");

			ProtoScript.Interpretter.Compiling.PrototypeCompiler.EnableDatabase = false;
        }

		static public UnderstandResult Understand2(string strPhrase, TaggingSettings settings)
		{
            if (StringUtil.IsEmpty(settings.Project))
                throw new Exception("No project specified"); 

			TemporaryActions.Clear();
			TemporarySubTypes.Clear();

			ProtoScriptTagger tagger = GetAndInitializeProtoScriptTagger(settings);

			return Understand2(strPhrase, settings, tagger);
		}

		static public UnderstandResult Understand2(string strPhrase, TaggingSettings settings, ProtoScriptTagger tagger)
		{
			UnderstandResult result = new UnderstandResult();


			if (settings.Fragment)
			{
				FragmentingTagger fragmentingTagger = new FragmentingTagger(tagger);
				fragmentingTagger.TagAfterFragment = settings.TagAfterFragment;
				result.Result = fragmentingTagger.Tag(strPhrase);
				result.Iterations = tagger.CurrentIterations;
				result.Tagger = tagger;
			}

			else
			{
				result = Understand2(strPhrase, tagger);
			}

			if (settings.IncludeMeaning)
			{
				Prototype ? protoDimension = StringUtil.IsEmpty(settings.MeaningDimension) ? null : TemporaryPrototypes.GetTemporaryPrototypeOrNull(settings.MeaningDimension);
				if (settings.TransferRecursively)
					result.Sememes = UnderstandUtil.TransferToSememesRecursiveWithDimension(result.Result, protoDimension, result.Tagger.Interpretter);
				else if (null != protoDimension)
					result.Sememes = UnderstandUtil.TransferToSememesWithDimension(result.Result, protoDimension, result.Tagger.Interpretter);
				else 
					result.Sememes = new Collection(UnderstandUtil.TransferToSememes(result.Result, result.Tagger.Interpretter));
			}

			return result;
		}

		static public UnderstandResult Understand2(string strPhrase, ProtoScriptTagger tagger)
		{
			Prototype protoLexemes = ConvertPhraseToLexemes(strPhrase, tagger);
			return Understand2(protoLexemes, tagger);
		}
		static public UnderstandResult Understand2(Prototype protoLexemes, ProtoScriptTagger tagger)
		{
			UnderstandResult result = new UnderstandResult();

			result.Tagger = tagger;


			tagger.AllowLexemeShortCircuiting = true;

			try
			{

				Prototype protoResult = tagger.Tag(protoLexemes);

				result.Result = protoResult;
				result.Iterations = tagger.CurrentIterations;

				if (null != result.Result && tagger.TagIteratively)
				{
					List<Prototype> lstPrototypes = GetAllTaggings(tagger);
					if (!lstPrototypes.Any(x => PrototypeGraphs.AreEquivalentCircular(x, result.Result)))
						lstPrototypes.Add(result.Result);

					result.Result = new Collection(lstPrototypes);
				}

				if (null != result.Result && !tagger.TagIteratively && tagger.IncludePathToStart)
					result.PathToStart = tagger.GetPathToStart(tagger.TaggingNode);

			}
			catch (RuntimeException ex)
			{
				result.Error = ex.Message;
				result.ErrorStatement = ex.Info;
			}

			return result;
		}

		public static Prototype ConvertPhraseToLexemes(string strPhrase, ProtoScriptTagger tagger)
		{
			Prototype protoTokens = Tokenize(strPhrase);
			Prototype protoLexemes = ConvertToLexemes(protoTokens, tagger.KeepLexemes);

			if (protoLexemes.Children.Any(x => !Prototypes.TypeOf(x, Lexeme.Prototype)))
			{
				if (tagger.AllowUnknownLexemes)
					protoLexemes = UnknownLexemes.ResolveAll(protoLexemes, tagger.Interpretter);

				if (protoLexemes.Children.Any(x => !Prototypes.TypeOf(x, Lexeme.Prototype)))
					throw new JsonWsException("Unrecognized lexeme: " + string.Join(", ", protoLexemes.Children.Where(x => !Prototypes.TypeOf(x, Lexeme.Prototype)).Select(x => x.PrototypeName).ToArray()));
			}

			return protoLexemes;
		}

		static public List<Prototype> GetAllTaggings(ProtoScriptTagger tagger)
		{
			//N20250420-01 - Changed to use AreEquivalent
			List<Prototype> lstPrototypes = new List<Prototype>();
			do
			{
				var res2 = tagger.TagNext();
				if (null == res2)
					break;

				//Equivalent ignores the Instance ID
				if (lstPrototypes.Any(x => PrototypeGraphs.AreEquivalentCircular(x, res2)))
					continue;

				lstPrototypes.Add(res2);
			}
			while (true);

			return lstPrototypes;
		}

		static public Collection TransferToSememesRecursive(Prototype prototype, NativeInterpretter interpretter)
		{
			Collection lstSememes = new Collection();

			PrototypeGraphs.DepthFirst(prototype, x =>
			{
				lstSememes.Children.AddRange(UnderstandUtil.TransferToSememes(x, interpretter));
				return x;
			});

			return lstSememes;
		}

		static public Collection TransferToSememesRecursiveWithDimension(Prototype prototype, Prototype protoDimension, NativeInterpretter interpretter)
		{
			Collection lstSememes = new Collection();

			PrototypeGraphs.DepthFirstOnNormal(prototype, x =>
			{
				lstSememes.AddRange(UnderstandUtil.TransferToSememesWithDimension(x, protoDimension, interpretter));
				return x;
			});

			return lstSememes;
		}

		static public List<Prototype> TransferToSememes(Prototype prototype, NativeInterpretter interpretter)
		{
			Prototype protoSubType = SubType(prototype, interpretter);

			List<TemporaryAction> lstTemporaryActions = TemporaryActions.GetTemporaryActions(prototype);
			List<Prototype> lstSememes = new List<Prototype>();

			foreach (TemporaryAction action in lstTemporaryActions)
			{
				Prototype protoSememe = TemporaryActions.RunAction(action, prototype, interpretter);
				if (null != protoSememe)
					lstSememes.Add(protoSememe);
			}

			for (int i = 0; i < lstSememes.Count; i++)
			{
				Prototype protoSememe = lstSememes[i];
				protoSememe = SubType(protoSememe, interpretter);

				//Prevent infinite loops
				if (!PrototypeGraphs.AreEqualCircular(protoSememe, prototype))
				{
					foreach (Prototype protoTransfered in TransferToSememes(protoSememe, interpretter))
					{
						if (!lstSememes.Any(x => PrototypeGraphs.AreEqualCircular(x, protoTransfered)))
						{
							lstSememes.Add(protoTransfered);
						}
					}
				}
				
			}


			return lstSememes;
		}

		static public Collection TransferToSememesWithDimension(Prototype prototype, Prototype protoDimension, NativeInterpretter interpretter)
		{
			Prototype protoSubType = SubType(prototype, interpretter);

			//N20221025-04 - Changed this from prototype -> protoSubType here to prevent it from duplicating the lookup. 
			List<TemporaryAction> lstTemporaryActions = TemporaryActions.GetTemporaryActionsWithDimension(protoSubType, protoDimension);
			Collection lstSememes = new Collection();

			foreach (TemporaryAction action in lstTemporaryActions)
			{
				Prototype protoSememe = TemporaryActions.RunAction(action, prototype, interpretter);
				if (null != protoSememe)
				{

					//N20220901-02 - Unrolling functions return a sequence that should not be collapsed
					//I removed this code, revisit if it is needed
					//	if (Prototypes.TypeOf(protoSememe, Ontology.Collection.Prototype))
					//		lstSememes.Children.AddRange(protoSememe.Children);
					//	else
					lstSememes.Add(protoSememe);

					Logs.DebugLog.WriteEvent(action.ToString(),
						"\r\n" + PrototypeLogging.ToFriendlyString2(prototype) + "\r\n->\r\n" +
						PrototypeLogging.ToFriendlyString2(protoSememe));
				}
				else
				{
					Logs.DebugLog.WriteEvent(action.ToString(),
	"\r\n" + PrototypeLogging.ToFriendlyString2(prototype) + "\r\n->\r\n" + "(null)");
				}
			}

			Collection lstResults = new Collection();
			lstResults.AddRange(lstSememes);

			for (int i = 0; i < lstSememes.Children.Count; i++)
			{
				Prototype protoSememe = lstSememes.Children[i];
				protoSememe = SubType(protoSememe, interpretter);

				//Prevent infinite loops
				if (!PrototypeGraphs.AreEqualCircular(protoSememe, prototype))
				{
					foreach (Prototype protoTransfered in TransferToSememesWithDimension(protoSememe, protoDimension, interpretter).Children)
					{
						if (!lstSememes.Any(x => PrototypeGraphs.AreEqualCircular(x, protoTransfered)))
						{
							lstSememes.Add(protoTransfered);
							lstResults.Add(protoTransfered);
						}
					}
				}
			}


			return lstResults;
		}

		static public Prototype SubType(Prototype prototype, NativeInterpretter interpretter)
		{
			List<TemporarySubType> lstSubTypes = TemporarySubTypes.GetPotentialSubTypes(prototype);

			for (int i = 0; i < lstSubTypes.Count; i++)
			{
				TemporarySubType subType = lstSubTypes[i];

				prototype = SubTypeSingle(prototype, interpretter, lstSubTypes, subType);

				//N20250323-01
				//if (prototype is QuantumPrototype qp)
				//{
				//	foreach (Prototype child in qp.PossiblePrototypes)
				//	{
				//		SubTypeSingle(child, interpretter, lstSubTypes, subType);
				//	}

				//}

			}

			return prototype;
		}

		private static Prototype SubTypeSingle(Prototype prototype, NativeInterpretter interpretter, List<TemporarySubType> lstSubTypes, TemporarySubType subType)
		{
			if (TemporarySubTypes.CategorizeAsSubType(prototype, subType, interpretter))
			{
				Logs.DebugLog.WriteEvent(subType.Function.ParentPrototype?.PrototypeName + "." + subType.Function.FunctionName, "true");
				//N20220513-01 - New more specific subtypes should be inserted before the parent 
				if (!prototype.IsInstance())
					prototype = prototype.CreateInstance();

				List<int> lstTypeOfs = prototype.GetTypeOfs();
				int iIndex = lstTypeOfs.FindIndex(x => x == subType.SuperType.PrototypeID);
				if (iIndex >= 0)
				{
					//Don't duplicate types
					if (!lstTypeOfs.Any(x => x == subType.SubType.PrototypeID))
					{
						prototype.InsertTypeOf(subType.SubType.PrototypeID);
					}
				}
				else
					prototype.InsertTypeOf(subType.SubType.PrototypeID);

				//New subtypes may become available after the first subtype
				List<TemporarySubType> lstNewPotential = TemporarySubTypes.GetPotentialSubTypes(subType.SubType);
				foreach (TemporarySubType temporarySubType in lstNewPotential)
				{
					if (!lstSubTypes.Any(x => Prototypes.AreShallowEqual(x.SubType, temporarySubType.SubType)))
					{
						lstSubTypes.Add(temporarySubType);
					}
				}

			}

			//During fragmenting tagging we can subtype multiple times, causing the 
			//valid subtypes to change as the prototype is rolled up
			else if (prototype.TypeOf(subType.SubType))
			{
				TypeOfs.Remove(prototype, subType.SubType);
			}

			return prototype;
		}

		static public Prototype Rollup(Prototype prototype)
		{
			RolloutNode node = new RolloutNode(prototype);
			var result = Tagger2.Tag(node);
			return (result as SingleResult)?.Result;
		}

		static public Prototype Tokenize(string strInput)
		{
			Prototype prototype = new Ontology.Collection();
			foreach (string strToken in Split(strInput))
			{
				prototype.Children.Add(NativeValuePrototype.GetOrCreateNativeValuePrototype(strToken));
			}

			return prototype;
		}

		const string SYMBOLS = "\\.:,?!/-\"\'\n’";

		public static List<string> Split(string strLexeme)
		{
			List<string> lstSplits = new List<string>();

			StringBuilder sb = new StringBuilder();
			foreach (char c in strLexeme)
			{
				if (Char.IsLetterOrDigit(c))
					sb.Append(c);
				else
				{
					if (sb.Length > 0)
					{
						lstSplits.Add(sb.ToString());
						sb = new StringBuilder();
					}					
					
					if (!Char.IsWhiteSpace(c)
						//|| c == '\n' //not doing fragmenting now
						)
					{
						lstSplits.Add(new string(c, 1));
					}
				}
			}

			if (sb.Length > 0)
			{
				lstSplits.Add(sb.ToString());
			}

			return lstSplits;
		}

		public static Prototype ConvertToLexemes(Prototype protoTokens, bool bKeepTokens = false)
		{
			Collection protoLexemes = new Collection();

			foreach (Prototype child in protoTokens.Children)
			{
				Prototype prototype = child;

				string strLexeme = (child as NativeValuePrototype)?.NativeValue as string;
				if (null != strLexeme)
				{
					TemporaryLexeme ? lexeme = TemporaryLexemes.GetLexemeByLexeme(strLexeme);
					if (null != lexeme)
					{
						if (bKeepTokens)
							prototype.Children.Add(child.ShallowClone());
					}
				}

				protoLexemes.Children.Add(prototype);
			}

			return protoLexemes;
		}

		public static bool HasUnknownLexemes(string strPhrase)
		{
			Prototype protoTokens = Tokenize(strPhrase);
			Prototype protoLexemes = ConvertToLexemes(protoTokens);

			return protoLexemes.Children.Any(x => !Prototypes.TypeOf(x, Lexeme.Prototype));
		}

	}
}
