using BasicUtilities;
using BasicUtilities.Collections;
using Buffaly.NLU;
using Ontology;
using Ontology.Simulation;
using Ontology.Utils;
using ProtoScript.Extensions.SolutionItems;
using ProtoScript.Interpretter;
using ProtoScript.Interpretter.RuntimeInfo;
using ProtoScript.Parsers;
using RooTrax.Cache;
using System.Collections.Concurrent;
using System.Text;
using WebAppUtilities;

namespace ProtoScript.Extensions
{
	public class Diagnostic
	{
		public string Type;
		public string Message;
		public StatementParsingInfo Info;
	}

	public class ProtoScriptWorkbench : JsonWs
	{

		static protected ConcurrentDictionary<string, SessionObject> m_mapSessions = new ConcurrentDictionary<string, SessionObject>();

		// When running inside the web portal, project paths may be
		// relative to the application's wwwroot folder.  SetWebRoot
		// from Program.cs so we can resolve those paths.
		static private string? _webRoot;

		static public void SetWebRoot(string path)
		{
			_webRoot = path;
		}

		static public string GetWebRoot()
		{
			return _webRoot ?? string.Empty;
		}


		static private string EnsureAbsolutePath(string path)
		{
			if (!System.IO.Path.IsPathRooted(path) && !string.IsNullOrEmpty(_webRoot))
			{
				// Combine the web root with the relative path so the
				// parsers always receive an absolute path.
				return System.IO.Path.Combine(_webRoot, path);
			}
			return path;
		}

		static public SessionObject GetOrCreateSession(string strProject)
		{
			strProject = EnsureAbsolutePath(strProject);

			if (StringUtil.IsEmpty(strProject)
				|| !m_mapSessions.TryGetValue(strProject, out SessionObject session))
			{
				return CreateSession(strProject);
			}

			EnterSession(session);
			return session;
		}

		static public void Reset()
		{
			m_mapSessions.Clear();
		}

		static protected void EnterSession(SessionObject session)
		{
			CacheManager.UseAsyncLocal = true;
			ObjectCacheManager.UseAsyncLocal = true;

			TemporaryPrototypes.Cache.InsertLogFrequency = 10000;

			session.Enter();

			//TemporaryPrototypes uses a local pointer to the cache that has to be reloaded
			TemporaryPrototypes.ReloadCache();
		}

		static private SessionObject CreateSession(string strSessionKey)
		{
			SessionObject session = SessionObject.Create(strSessionKey);

			// Enter the session before creating the tagger
			EnterSession(session);

			m_mapSessions[strSessionKey] = session;

			return session;
		}


		static public string LoadFile(string strSessionKey, string strFile)
		{
			SessionObject session = GetOrCreateSession(strSessionKey);
			string strContents = FileUtil.ReadFile(strFile);

			session.Context.SetCurrentFile(strFile);

			return strContents;
		}

		public static bool CreateNewFile(string strProject, string strNewFile)
		{
			// Project path may be relative to wwwroot
			strProject = EnsureAbsolutePath(strProject);

			string rootDir = StringUtil.LeftOfLast(strProject, "\\");
			// e.g. "C:\\dev\\ai\\Ontology\\ProtoScript.Tests\\DevAgent"

			// 2. Build the final absolute path.
			// If the provided file path is absolute, we use it directly;
			// otherwise, we treat it as relative to 'rootDir'.
			string finalPath;
			if (System.IO.Path.IsPathRooted(strNewFile))
			{
				finalPath = strNewFile;
			}
			else
			{
				finalPath = FileUtil.BuildPath(rootDir, strNewFile);
				// e.g. "C:\\dev\\ai\\Ontology\\ProtoScript.Tests\\DevAgent\\File.pts"
			}

			// 3. Check if the file already exists
			if (System.IO.File.Exists(finalPath))
			{
				throw new JsonWsException($"File already exists: {finalPath}");
			}

			try
			{
				// 4. Ensure the directory structure exists
				string directoryPath = StringUtil.LeftOfLast(finalPath, "\\");

				//>if directoryPath doesnt' exist throw a JsonWsException
				if (!System.IO.Directory.Exists(directoryPath))
				{
					throw new JsonWsException($"Directory does not exist: {directoryPath}");
				}

				// 5. Create an empty .pts file
				FileUtil.WriteFile(finalPath, $"//{strNewFile}\n");

				//>append the relative path to the file to the contents of the project file
				FileUtil.AppendFile(strProject, $"include \"{strNewFile}\";");
			}
			catch (Exception ex)
			{
				// Catch any I/O errors or other exceptions
				throw new JsonWsException($"Failed to create file: {ex.Message}");
			}

			return true;
		}


		static public List<string> LoadProject(string strProject)
		{
			// Resolve project paths relative to wwwroot if needed
			SessionObject session = GetOrCreateSession(strProject);
			return LoadProjectInternal(session);
		}
		static private List<string> LoadProjectInternal(SessionObject session)
		{
			try
			{
				string strProject = session.SessionKey;
				File fileProject = ProtoScript.Parsers.Files.Parse(strProject);

				string strRootDir = StringUtil.LeftOfLast(strProject, "\\");

				Project project = new Project();
				project.FileName = strProject;
				project.RootDirectory = strRootDir;

				//TODO: This needs to not load everything separate from the compilation process
				List<File> lstFiles = Compiler.GetAllIncludedFiles(fileProject, false);

				foreach (File file in lstFiles.OrderBy(x => x.Info.Name))
				{
					project.Files.Add(new ProtoScriptFile()
					{
						FileName = file.Info.FullName,
						Length = file.Info.Length
					});
				}

				project.Files.Add(new ProtoScriptFile()
				{
					FileName = strProject,
					Length = fileProject.Info.Length
				});

				session.Context.Project = project;

				return project.Files.Select(x => x.FileName).ToList();
			}
			catch (ProtoScriptParsingException err)
			{
				throw new JsonWsException(err.Message + ", " + err.Explanation);
			}
			catch (Exception)
			{
				throw;
			}

		}

		static public void SaveCurrentCode(string strSessionKey, string strFile, string Code)
		{
			string strFileName = StringUtil.RightOfLast(strFile, "\\");

			if (StringUtil.IsEmpty(Code))
				throw new JsonWsException("Empty code being saved");

			for (int i = 50; i > 0; i--)
			{
				string strSourceFile = FileUtil.BuildPath(@"C:\temp\protoscriptscript_back", strFileName + "_" + (i - 1));
				if (System.IO.File.Exists(strSourceFile))
				{
					string strTargetFile = FileUtil.BuildPath(@"C:\temp\protoscriptscript_back", strFileName + "_" + i);

					System.IO.File.Copy(strSourceFile, strTargetFile, true);
				}
			}

			string strBackup = FileUtil.BuildPath(@"C:\temp\protoscriptscript_back", strFileName + "_0");
			FileUtil.WriteFile(strBackup, FileUtil.ReadFile(strFile));
			FileUtil.WriteFile(strFile, Code);

			Logs.DebugLog.WriteEvent("ProtoScriptWorkbench.SaveCurrentCode", strFile);
		}

		static public Diagnostic ParseCode(string Code)
		{
			try
			{
				File file = ProtoScript.Parsers.Files.ParseFileContents(Code);
			}
			catch (ProtoScriptTokenizingException err)
			{
				return new Diagnostic() { Type = "Parsing", Message = "Expected: " + err.Expected, Info = new StatementParsingInfo() { StartingOffset = err.Cursor } };
			}

			return null;
		}

		static public List<Diagnostic> CompileCodeWithProject(string strCode, string strProjectName)
		{
			strProjectName = EnsureAbsolutePath(strProjectName);
			SessionObject session = GetOrCreateSession(strProjectName);

			List<Diagnostic> lstDiagnostics = new List<Diagnostic>();
			ProtoScriptTagger tagger = new ProtoScriptTagger();
			session.Tagger = tagger;

			tagger.EnableDatabase = false;
			UnderstandUtil.SetupDatabaseDisconnectedMode();

			tagger.Interpretter.InsertGlobalObject("_sockets", new Sockets());  //make it avaialble for compilation only
																				//tagger.Interpretter.InsertGlobalObject("_session", new SessionsRow());
			tagger.Interpretter.InsertGlobalObject("_sessionObject", session);

			Compiler compiler = tagger.Compiler;

			try
			{
				session.Tagger.Statements = compiler.CompileProject(strProjectName);
				lstDiagnostics = GetCompilerDiagnostics(compiler);

				session.Context.Compiler = compiler;
			}
			catch (ProtoScriptTokenizingException err)
			{
				lstDiagnostics.Add(new Diagnostic() { Type = "Parsing", Message = "Expected: " + err.Expected + " " + err.Explanation, Info = new StatementParsingInfo() { StartingOffset = err.Cursor, Length = 1, File = err.File } });
			}
			catch (Exception)
			{
				lstDiagnostics = GetCompilerDiagnostics(compiler);
				if (lstDiagnostics.Count == 0)
					throw;

			}

			return lstDiagnostics;
		}

		static private List<Diagnostic> GetCompilerDiagnostics(Compiler compiler)
		{
			List<Diagnostic> lstDiagnostics = new List<Diagnostic>();
			foreach (CompilerDiagnostic compilerDiagnostic in compiler.Diagnostics)
			{
				Diagnostic diagnostic = new Diagnostic();
				diagnostic.Message = compilerDiagnostic.Diagnostic.Message;
				if (null != compilerDiagnostic.Statement)
				{
					diagnostic.Info = compilerDiagnostic.Statement.Info;
					diagnostic.Message += " - " + SimpleGenerator.Generate(compilerDiagnostic.Statement);
				}
				else if (null != compilerDiagnostic.Expression)
				{
					diagnostic.Info = compilerDiagnostic.Expression.Info;
					diagnostic.Message += " - " + SimpleGenerator.Generate(compilerDiagnostic.Expression);
				}

				lstDiagnostics.Add(diagnostic);
			}
			return lstDiagnostics;
		}

		public class Symbol
		{
			public string SymbolName;
			public string SymbolType;
			public StatementParsingInfo Info;
		}

		static public List<Symbol> GetSymbols(string strSessionKey)
		{
			try
			{
				List<Symbol> lstSymbols = new List<Symbol>();

				SessionObject session = GetOrCreateSession(strSessionKey);
				if (null != session.Tagger?.Statements)
				{
					foreach (var statement in session.Tagger.Statements)
					{
						if (statement is ProtoScript.Interpretter.Compiled.PrototypeDeclaration)
						{
							ProtoScript.Interpretter.Compiled.PrototypeDeclaration declaration = statement as ProtoScript.Interpretter.Compiled.PrototypeDeclaration;
							lstSymbols.Add(new Symbol { SymbolName = declaration.PrototypeName, SymbolType = "PrototypeDeclaration", Info = declaration.Info });
						}
						else if (statement is FunctionRuntimeInfo)
						{
							FunctionRuntimeInfo declaration = statement as FunctionRuntimeInfo;
							lstSymbols.Add(new Symbol { SymbolName = declaration.FunctionName, SymbolType = "FunctionRuntimeInfo", Info = declaration.Info });
						}
					}
				}

				return lstSymbols.OrderBy(x => x.SymbolName).ToList();
			}
			catch (Exception err)
			{
				throw new JsonWsException(err.Message);
			}
		}

		static public List<CodeContext.Symbol> GetSymbolsAtCursor(string strSessionKey, string strFileName, int iPos)
		{
			SessionObject session = GetOrCreateSession(strSessionKey);

			List<CodeContext.Symbol> lstSymbols = new List<CodeContext.Symbol>();

			if (null != session.Context.Compiler)
			{
				try
				{
					session.Context.SetCurrentFile(strFileName);
					lstSymbols = session.Context.GetSymbolsAtCursor(iPos);
				}
				catch (Exception err)
				{
					throw new JsonWsException(err.Message);
				}
			}

			return lstSymbols;
		}

		static public List<CodeContext.Symbol> Suggest(string strSessionKey, string strLine, int iPos)
		{
			SessionObject session = GetOrCreateSession(strSessionKey);
			List<CodeContext.Symbol> lstSymbols = new List<CodeContext.Symbol>();

			if (null != session.Context.Compiler)
			{
				try
				{
					lstSymbols = session.Context.Suggest(iPos, strLine);
				}
				catch (Exception err)
				{
					throw new JsonWsException(err.Message);
				}
			}

			return lstSymbols;
		}

		static public List<string> PredictNextLine(string strSessionKey, int iPos)
		{
			SessionObject session = GetOrCreateSession(strSessionKey);
			return session.Context.PredictNextLine(iPos);
		}

		static public string GetSymbolInfo(string strSessionKey, string strSymbol, StatementParsingInfo info)
		{
			SessionObject session = GetOrCreateSession(strSessionKey);

			if (null == session.Context.Compiler)
				return null;

			File file = session.Context.Compiler.Files.First(x => StringUtil.EqualNoCase(x.Info.FullName, info.File));
			StringBuilder sb = new StringBuilder(file.RawCode.Substring(info.StartingOffset, info.Length));
			sb.AppendLine();


			PrototypeTypeInfo infoType = session.Context.Compiler.Symbols.GetSymbol(strSymbol) as PrototypeTypeInfo;
			if (null != infoType)
			{
				Prototype prototype = infoType.Prototype;

				List<TemporarySubType> lstIncomingSubTypes = TemporarySubTypes.Where(x => x.SubType.PrototypeID == prototype.PrototypeID);
				List<TemporarySubType> lstOutgoingSubTypes = TemporarySubTypes.Where(x => x.SuperType.PrototypeID == prototype.PrototypeID);

				List<TemporaryAction> lstIncomingActions = TemporaryActions.Where(x => x.Target?.PrototypeID == prototype.PrototypeID);
				List<TemporaryAction> lstOutgoingActions = TemporaryActions.Where(x => x.Source.PrototypeID == prototype.PrototypeID);

				foreach (TemporarySubType subtype in lstIncomingSubTypes)
				{
					sb.Append(subtype.SuperType.PrototypeName).Append(" -> ").Append(subtype.SubType.PrototypeName).AppendLine();
				}

				foreach (TemporarySubType subtype in lstOutgoingSubTypes)
				{
					sb.Append(subtype.SuperType.PrototypeName).Append(" -> ").Append(subtype.SubType.PrototypeName).AppendLine();
				}

				foreach (TemporaryAction action in lstIncomingActions)
				{
					sb.AppendLine(action.ToString());
				}

				foreach (TemporaryAction action in lstOutgoingActions)
				{
					sb.AppendLine(action.ToString());
				}

				sb.AppendLine("\r\nSymbols:");
				foreach (var pair in infoType.Scope)
				{
					sb.Append(pair.Key).Append(" = ").AppendLine(pair.Value.GetType().ToString());

				}

				sb.AppendLine("\r\nChildren:");

				foreach (var pair in session.Context.Compiler.Symbols.GetGlobalScope())
				{
					if (pair.Value is PrototypeTypeInfo)
					{
						Prototype protoChild = (pair.Value as PrototypeTypeInfo).Prototype;
						if (protoChild.TypeOf(prototype))
						{
							sb.Append(pair.Key).Append(" = ").AppendLine(protoChild.PrototypeName);
						}
					}
				}

			}


			return sb.ToString();
		}


		public class TaggingSettings
		{
			public bool AllowRanges = false;
			public bool IncludeTypeOfs = false;
			public int MaxIterations = 1000;
			public string Project;

			public bool IncludeMeaning = false;
			public bool TransferRecursively = false;
			public string MeaningDimension = null;
			public bool Debug = false;
			public bool Fragment = false;
			public bool TagAfterFragment = true;
			public bool TagIteratively = false;
			public bool AllowAlreadyLinkedSequences = true;
			public bool PathToStart = true;
			public bool EnableDatabase = false;
			public bool Resume = false;
			public bool AllowPrecompiled = false;

			public string SessionKey = null;

			public List<StatementParsingInfo> Breakpoints = new List<StatementParsingInfo>();
		}

		static public Debugger m_Debugger = null;
		static public Sockets m_Sockets = new Sockets();

		static private bool IsResumable(TaggingSettings settings, SessionObject session)
		{
			bool bResume = settings.Resume && session.Tagger != null;

			if (bResume && settings.Debug && null == session.Tagger.Debugger)
			{
				Logs.DebugLog.WriteEvent("Not Resumable", "Debugging");
				bResume = false;
			}
			else if (bResume && null != session.Context?.Project)
			{
				Logs.DebugLog.WriteEvent("Not Resumable", "Project has changed on disk");
				bResume = (session.Context.Project.HasProjectChangedOnDisk() == false);
			}
			else if (bResume && !session.Tagger.IsInterpretted)
			{
				Logs.DebugLog.WriteEvent("Not Resumable", "Tagger is not interpreted");
				bResume = false;
			}

			return bResume;
		}

		static public TagImmediateResult InterpretImmediate(string strProject, string strImmediate, TaggingSettings taggingSettings)
		{
			JsonSerializers.RegisterSerializer(typeof(TagImmediateResult), new NestedJsonSerializer<TagImmediateResult>());

			taggingSettings.SessionKey = EnsureAbsolutePath(strProject);
			SessionObject session = GetOrCreateSession(taggingSettings.SessionKey);

			TagImmediateResult result = new TagImmediateResult();
			try
			{
				bool bResume = IsResumable(taggingSettings, session);

				ProtoScriptTagger tagger;
				if (bResume)
				{
					tagger = session.Tagger;

					//Clear diagnostics that (may have been) generated from the last immediate expression
					tagger.Compiler.Diagnostics.Clear();
				}
				else
				{
					Initializer.ResetCache();

					//Populate the context
					//Too slow here, conflicts with precompiled
					//LoadProjectInternal(strProject, session);

					UnderstandUtil.TaggingSettings settings = new UnderstandUtil.TaggingSettings();
					settings.Project = taggingSettings.SessionKey;
					settings.MaxIterations = taggingSettings.MaxIterations;
					settings.AllowRanges = taggingSettings.AllowRanges;
					settings.Fragment = taggingSettings.Fragment;
					settings.TagAfterFragment = taggingSettings.TagAfterFragment;
					settings.Debug = taggingSettings.Debug;
					settings.EnableDatabase = taggingSettings.EnableDatabase;
					settings.AllowPrecompiled = taggingSettings.AllowPrecompiled;

					m_Sockets = new Sockets();
					settings.GlobalObjects.Add("_sockets", m_Sockets);
					//settings.GlobalObjects.Add("_session", new SessionsRow());
					settings.GlobalObjects.Add("_sessionObject", session);

					tagger = UnderstandUtil.GetAndInitializeProtoScriptTagger(settings);
					session.Tagger = tagger;
				}

				object? obj = null;

				ProtoScript.Expression statementImmediate = ProtoScript.Parsers.Expressions.Parse(strImmediate);
				ProtoScript.Interpretter.Compiled.Expression compiledImmediate = tagger.Compiler.Compile(statementImmediate);

				if (tagger.Compiler.Diagnostics.Count > 0)
				{
					result.Error = "Error in immediate: " + tagger.Compiler.Diagnostics.First().Diagnostic.Message;
					return result;
				}

				if (!taggingSettings.Debug)
				{
					obj = tagger.Interpretter.Evaluate(compiledImmediate);
				}
				else
				{
					Debugger debugger = tagger.Debugger;
					m_Debugger = debugger;

					foreach (StatementParsingInfo breakpoint in taggingSettings.Breakpoints)
					{
						debugger.Interpretter.Breakpoints.Add(breakpoint);
					}

					debugger.StartDebugging(compiledImmediate);

					obj = debugger.WaitForEndOfExecution();
				}

				if (null == obj)
					result.Result = "(null)";

				else
				{
					if (null == obj)
					{
						result.Result = "null";
					}

					else if (obj is bool)
					{
						result.Result = ((bool)obj).ToString();
					}

					else if (obj is StringWrapper sw)
					{
						result.Result = sw.GetStringValue();
					}



					else
					{
						Prototype? protoValue = tagger.Interpretter.GetOrConvertToPrototype(obj);
						if (null == protoValue)
						{
							if (obj is ValueRuntimeInfo info)
							{
								result.Result = info.Value == null ? "null" : info.Value.ToString();
							}
							else
								result.Result = obj == null ? "null" : obj.ToString();
						}
						else
						{
							PrototypeLogging.IncludeTypeOfs = taggingSettings.IncludeTypeOfs;
							if (Prototypes.TypeOf(protoValue, Ontology.Collection.Prototype) && protoValue.Children.Count == 1)
								result.Result = PrototypeLogging.ToFriendlyString(protoValue.Children[0]).ToString();
							else
								result.Result = PrototypeLogging.ToFriendlyString(protoValue).ToString();
							result.ResultPrototype = protoValue;
						}
					}
				}
			}
			catch (ProtoScriptTokenizingException err)
			{
				result.Error = err.Message + ", " + err.Explanation;
				result.ErrorStatement = new StatementParsingInfo() { StartingOffset = err.Cursor, Length = 1, File = err.File };
			}
			catch (RuntimeException err)
			{
				result.Error = err.Message;
				result.ErrorStatement = err.Info;

				Exception? innerErr = err.InnerException;
				while (null != innerErr)
				{
					result.Error = innerErr.Message + " - " + result.Error;
					innerErr = innerErr.InnerException;
				}
			}
			catch (CompilationException err)
			{
				result.Error = err.Message;
				result.ErrorStatement = err.Info;
			}
			catch (Exception err)
			{
				result.Error = err.Message;
			}

			return result;
		}



		public class TagImmediateResult
		{
			public string ? Result = null;
			public string ? Error = null;
			public StatementParsingInfo ? ErrorStatement = null;
			public Prototype ? ResultPrototype = null;
		}

		static public void StopTagging(string strSessionKey)
		{
			SessionObject session = GetOrCreateSession(strSessionKey);
			if (null != session.Tagger)
				session.Tagger.IsStopped = true;
		}

		static public JsonObject GetTaggingProgress(string strSessionKey)
		{
			JsonObject jsonObject = new JsonObject();

			SessionObject session = GetOrCreateSession(strSessionKey);

			if (null != session.Tagger)
			{
				jsonObject["Iterations"] = session.Tagger.CurrentIterations;

				Buffaly.NLU.Tagger.BaseFunctionNode? node2 = session.Tagger.TaggingNode.Possibilities.OrderByDescending(x => x.Value).FirstOrDefault() as Buffaly.NLU.Tagger.BaseFunctionNode;

				if (null != node2 && null != node2.Source)
					jsonObject["CurrentInterpretation"] = PrototypeLogging.ToFriendlyString(node2.Source).ToString();
			}

			return jsonObject;
		}

		static public TagImmediateResult TagImmediate(string strFragment, string strProject, TaggingSettings settings)
		{
			JsonSerializers.RegisterSerializer(typeof(TagImmediateResult), new NestedJsonSerializer<TagImmediateResult>());

			settings.SessionKey = strProject;

			SessionObject session = GetOrCreateSession(settings.SessionKey);
			bool bResume = IsResumable(settings, session);

			if (!bResume)
			{
				//TODO: Make threadsafe
				Buffaly.NLU.Nodes.ProtoScriptControllers.Clear();

				//Populate the context
				LoadProjectInternal(session);
			}

			UnderstandUtil.TaggingSettings settings2 = new UnderstandUtil.TaggingSettings
			{
				AllowRanges = settings.AllowRanges,
				MaxIterations = settings.MaxIterations,
				Project = strProject,
				Fragment = settings.Fragment,
				IncludeMeaning = settings.IncludeMeaning,
				TransferRecursively = settings.TransferRecursively,
				MeaningDimension = settings.MeaningDimension,
				TagAfterFragment = settings.TagAfterFragment,
				TagIteratively = settings.TagIteratively,
				AllowAlreadyLinkedSequences = settings.AllowAlreadyLinkedSequences,
				GlobalObjects = new Map<string, object>() {
					{ "_sockets", m_Sockets }, 
				//	{ "_session", new SessionsRow() { EnableLazyLoadProperties = false } },
					{ "_sessionObject", session }
				},
				PathToStart = settings.PathToStart,
				EnableDatabase = settings.EnableDatabase,
				Resume = bResume
			};

			UnderstandUtil.UnderstandResult result = null;
			TagImmediateResult res = new ProtoScriptWorkbench.TagImmediateResult();

			try
			{
				result = TagImmediateInternal(strFragment, settings2, session);
				session.Tagger = result.Tagger;
				res.ResultPrototype = result.Result;
			}
			catch (CompilationException err)
			{
				res.Error = err.Message;
				res.ErrorStatement = err.Info;
			}
			catch (IncompatiblePrototypeParameter err)
			{
				res.Error = err.Message;
				res.ErrorStatement = err.Info;
			}
			catch (RuntimeException err)
			{
				res.Error = err.Message;
				res.ErrorStatement = err.Info;
			}


			if (null != result && StringUtil.IsEmpty(result.Error))
			{

				PrototypeLogging.IncludeTypeOfs = settings.IncludeTypeOfs;
				PrototypeLogging.IncludePrototypeIDs = false;
				StringBuilder sb = null;

				if (Prototypes.TypeOf(result.Result, Ontology.Collection.Prototype) && result.Result.Children.Count == 1)
					sb = PrototypeLogging.ToFriendlyString(result.Result.Children[0]);
				else
					sb = PrototypeLogging.ToFriendlyString(result.Result);

				sb.AppendLine().AppendLine();
				sb.Append(result.Iterations).Append(" iterations.");

				if (!StringUtil.IsEmpty(result.PathToStart))
				{
					sb.Append("\r\n\r\nPath to Start:\r\n\r\n");
					sb.Append(result.PathToStart);
				}


				if (settings.IncludeMeaning && null != result.Result)
				{
					Prototype protoOriginal = result.Result.Clone();

					sb.Append("\r\n\r\nSememes:\r\n\r\n");

					try
					{
						foreach (Prototype protoSememe in result.Sememes.Children)
						{
							sb.Append('\t').Append(PrototypeLogging.ToFriendlyShadowString(protoSememe, 1)).AppendLine();
						}

						sb.Append("\r\n\r\nSubTyped:\r\n\r\n");
						foreach (Prototype protoChanged in PrototypeGraphs.Minus(result.Result, protoOriginal))
						{
							sb.Append('\t').Append(PrototypeLogging.ToFriendlyShadowString(protoChanged, 1)).AppendLine();
						}
					}
					catch (RuntimeException err)
					{
						res.Error = err.Message;
						res.ErrorStatement = err.Info;
					}
				}

				res.Result = sb.ToString();
			}
			else if (null != result)
			{
				res.Error = result.Error;
				res.ErrorStatement = result.ErrorStatement;
			}

			return res;
		}

		static private UnderstandUtil.UnderstandResult TagImmediateInternal(string strFragment, UnderstandUtil.TaggingSettings settings, SessionObject session)
		{
			//Reinitialize if we are not resuming, otherwise use the loaded tagger
			if (null == session.Tagger || !settings.Resume)
			{
				Initializer.ResetCache();

				if (StringUtil.IsEmpty(settings.Project))
					throw new Exception("No project is currently set");

				session.Tagger = UnderstandUtil.GetAndInitializeProtoScriptTagger(settings);
			}

			return UnderstandUtil.Understand2(strFragment, settings, session.Tagger);
		}

		//Visualization

		static public JsonObject GetPrototypeAndDescendants(string strSessionKey, string strPrototypeName)
		{
			SessionObject session = GetOrCreateSession(strSessionKey);
			Prototype prototype = TemporaryPrototypes.GetTemporaryPrototype(strPrototypeName);

			JsonObject ? jsonRoot = PrototypeLogging.ToFriendlyJsonObject(prototype);
			if (null != jsonRoot)
			{
				JsonArray jsonArray = new JsonArray();
				foreach (Prototype proto in prototype.GetDescendants())
				{
					jsonArray.Add(PrototypeLogging.ToFriendlyJsonObject(proto));
				}
				jsonRoot["Descendants"] = jsonArray;
			}

			return jsonRoot;
		}

		static public List<string> GetPrototypesBySearch(string strSessionKey, string strSearch)
		{
			SessionObject session = GetOrCreateSession(strSessionKey);

			List<Prototype> lstTemporary = TemporaryPrototypes.GetAllTemporaryPrototypes();
			List<string> lstResults = new List<string>();
			int iMax = 100;
			foreach (Prototype proto in lstTemporary)
			{
				if (proto.PrototypeName.StartsWith("System."))
					continue;

				if (StringUtil.InString(proto.PrototypeName, strSearch))
				{
					lstResults.Add(proto.PrototypeName);
				}

				if (lstResults.Count > iMax)
					break;
			}

			return lstResults.OrderBy(x => x).ToList();
		}





		//Debugging methods
		static public string GetSymbol(string strSessionKey, string strSymbol)
		{
			SessionObject session = GetOrCreateSession(strSessionKey);

			ProtoScript.Interpretter.Symbols.Scope scope = null;
			object obj = m_Debugger.Interpretter.Symbols.GetSymbolAndScope(strSymbol, out scope);
			if (obj is ValueRuntimeInfo)
			{
				obj = scope.Stack[(obj as ValueRuntimeInfo).Index];
			}
			bool bIncludeTypeOfs = PrototypeLogging.IncludeTypeOfs;
			PrototypeLogging.IncludeTypeOfs = true;
			string strRes = string.Empty;

			Prototype protoValue = SimpleInterpretter.GetAsPrototype(obj);
			if (null == protoValue)
			{
				ValueRuntimeInfo valueRuntimeInfo = obj as ValueRuntimeInfo;
				if (null == valueRuntimeInfo || null == valueRuntimeInfo.Value)
					strRes = "(null)";
				else
					strRes = valueRuntimeInfo.Value.ToString();
			}
			else
			{
				strRes = PrototypeLogging.ToFriendlyString(protoValue).ToString();
			}
			PrototypeLogging.IncludeTypeOfs = bIncludeTypeOfs;
			return strRes;

		}



		static public void Resume()
		{
			m_Debugger.Resume();
		}

		static public StatementParsingInfo GetBlockedOn()
		{
			if (m_Debugger == null || m_Debugger.Interpretter == null)
				return null;

			return m_Debugger.Interpretter.BlockedOn;
		}

		static public void StepNext()
		{
			m_Debugger.Interpretter.Step = DebuggingInterpretter.StepTypes.StepNext;
			m_Debugger.Resume();
		}

		static public void StepOver()
		{
			m_Debugger.Interpretter.Step = DebuggingInterpretter.StepTypes.StepOver;
			m_Debugger.Resume();
		}


		static public void StopDebugging()
		{
			m_Debugger.Interpretter.Step = DebuggingInterpretter.StepTypes.Stop;
			m_Debugger.Resume();
		}

		static public string GetCallStack()
		{
			StringBuilder sb = new StringBuilder();

			foreach (string strCall in m_Debugger.Interpretter.CallStack)
			{
				sb.AppendLine(strCall);
			}

			return sb.ToString();
		}

		static public string GetCurrentException()
		{
			Exception err = m_Debugger.Interpretter.Exception;
			StringBuilder sb = new StringBuilder();

			while (err != null)
			{
				sb.AppendLine(err.Message);
				err = err.InnerException;
			}

			return sb.ToString();
		}



		//Sockets
		static public void Respond(int iMessageID, string strShortForm)
		{
			Prototype prototype = TemporaryPrototypeShortFormParser.FromShortString(strShortForm);
			m_Sockets.Respond(iMessageID, prototype);
		}

		public class MessageResponse
		{
			public Prototype MessageValue = null;
			public string MessageText;
			public int MessageID;
		}

		static public MessageResponse GetNextMessage(int iLastMessageID)
		{
			JsonSerializers.RegisterSerializer(typeof(MessageResponse), new NestedJsonSerializer<MessageResponse>());

			if (null != m_Sockets)
			{
				Sockets.Message message = m_Sockets.GetNextMessage(iLastMessageID);
				if (null != message)
				{
					MessageResponse messageResponse = new MessageResponse();
					messageResponse.MessageValue = message.MessageValue;
					messageResponse.MessageID = message.MessageID;
					messageResponse.MessageText = PrototypeLogging.ToFriendlyString2(message.MessageValue);
					return messageResponse;
				}
			}
			return null;
		}


	}
}
