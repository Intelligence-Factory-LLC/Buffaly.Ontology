using BasicUtilities;
using BasicUtilities.Collections;
using Ontology;
using Ontology.Simulation;
using ProtoScript.Diagnostics;
using ProtoScript.Interpretter.Compiled;
using ProtoScript.Interpretter.Compiling;
using ProtoScript.Interpretter.RuntimeInfo;
using ProtoScript.Interpretter.Symbols;
using ProtoScript.Parsers;
using System.Collections.Concurrent;

namespace ProtoScript.Interpretter
{
	public class CompilerDiagnostic
	{
		public Statement ? Statement;
		public Expression ? Expression;
		public Diagnostic Diagnostic;

		public override string ToString()
		{
			return $"CompilerDiagnostic: {Diagnostic} at {Statement?.Info?.ToString() ?? "unknown"}";
		}
	}
	public class Compiler
	{
		public SymbolTable Symbols = new SymbolTable();
		public Map<string, object> References = new Map<string, object>();

		public List<CompilerDiagnostic> Diagnostics = new List<CompilerDiagnostic>();
		public string Source = string.Empty;
		public List<File> Files = new List<File>();
		public bool AllowParallelism = false;

		public void AddDiagnostic(Diagnostic diagnostic, Statement ? statement, Expression ? expression)
		{
			Diagnostics.Add(new CompilerDiagnostic() { Diagnostic = diagnostic, Statement = statement, Expression = expression });
		}

		public void AddDiagnostic(string strMessage, Statement? statement, Expression?expression)
		{
			Diagnostics.Add(new CompilerDiagnostic() { Diagnostic = new Diagnostic(strMessage), Statement = statement, Expression = expression });
		}
		public void Initialize()
		{
			this.Symbols.EnterGlobalScope();

			//Base types
			this.Symbols.InsertSymbol("bool", new TypeInfo(typeof(bool)));
			this.Symbols.InsertSymbol("string", new TypeInfo(typeof(string)));
			this.Symbols.InsertSymbol("int", new TypeInfo(typeof(int)));
			this.Symbols.InsertSymbol("Function", new TypeInfo(typeof(FunctionRuntimeInfo)));

			//Default imports
			string strCode = @"
reference Ontology Ontology; 
reference Ontology.Simulation Ontology.Simulation;

import Ontology Ontology.Collection Collection;
import Ontology Ontology.Prototype Prototype;

import Ontology.Simulation Ontology.Simulation.StringWrapper String;
import Ontology.Simulation Ontology.Simulation.IntWrapper Int;
import Ontology.Simulation Ontology.Simulation.IntWrapper Integer;
import Ontology.Simulation Ontology.Simulation.DoubleWrapper Double;
import Ontology.Simulation Ontology.Simulation.BoolWrapper Bool;
import Ontology.Simulation Ontology.Simulation.BoolWrapper Boolean;

";
			File file = ProtoScript.Parsers.Files.ParseFileContents(strCode);
			this.Compile(file);
		}

		public List<Compiled.Statement> CompileProject(string strProjectFile)
		{
			Compiler compiler = this;

			File file = ProtoScript.Parsers.Files.Parse(strProjectFile);

			Logs.DebugLog.CreateTimer("CompileProject.ParseFiles");

			List<File> lstFiles = GetAllIncludedFiles(file, AllowParallelism);
			lstFiles.Remove(file);
			lstFiles.Insert(0, file);

			Logs.DebugLog.WriteTimer("CompileProject.ParseFiles");

			return CompileFileList(lstFiles);
		}

		//>Compile a single file
		public List<Compiled.Statement> CompileSingleFile(string strFilePath)
		{
			File file = ProtoScript.Parsers.Files.Parse(strFilePath);
			List<File> lstFiles = new List<File> { file };
			lstFiles.AddRange(GetAllIncludedFiles(file, AllowParallelism));

			return CompileFileList(lstFiles);
		}


		protected List<Compiled.Statement> CompileFileList(List<File> lstFiles)
		{
			Logs.DebugLog.CreateTimer("CompileProject.CompileFileList");

			Compiler compiler = this;
			compiler.Files = lstFiles;

			List<Compiled.Statement> lstStatements = new List<Compiled.Statement>();

			//──────────────────────────── Precompiled ────────────────────────────
			Logs.DebugLog.CreateTimer("CompileProject.Precompiled");
			foreach (File fileCurrent in lstFiles.Where(x => x.IsPrecompiled))
			{
				PreCompiler.LoadPrecompiled(fileCurrent.RawCode, this.Symbols);
			}
			Logs.DebugLog.WriteTimer("CompileProject.Precompiled");

			//──────────────────────────── Declare-Namespaces ──────────────────────
			Logs.DebugLog.CreateTimer("CompileProject.DeclareNamespaces");
			foreach (File fileCurrent in lstFiles)
			{
				try
				{
					foreach (NamespaceDefinition ns in fileCurrent.Namespaces)
					{
						lstStatements.AddRange(NamespaceCompiler.DeclarePrototypes(ns, this));
					}
				}
				catch (ProtoScriptCompilerException ex)
				{
					ex.m_strProtoScript = fileCurrent.RawCode;
					ex.File = fileCurrent.Info?.FullName;
					throw;
				}
			}
			Logs.DebugLog.WriteTimer("CompileProject.DeclareNamespaces");

			//──────────────────────────── Declare-FilePrototypes ──────────────────
			Logs.DebugLog.CreateTimer("CompileProject.DeclareFilePrototypes");
			foreach (File fileCurrent in lstFiles)
			{
				try
				{
					lstStatements.AddRange(compiler.DeclareFilePrototypes(fileCurrent));
				}
				catch (ProtoScriptCompilerException ex)
				{
					ex.m_strProtoScript = fileCurrent.RawCode;
					ex.File = fileCurrent.Info?.FullName;
					throw;
				}
			}
			Logs.DebugLog.WriteTimer("CompileProject.DeclareFilePrototypes");

			//──────────────────────────── Declare-Namespaces-2 ────────────────────
			Logs.DebugLog.CreateTimer("CompileProject.DeclareNamespaces2");
			foreach (File fileCurrent in lstFiles)
			{
				try
				{
					foreach (NamespaceDefinition ns in fileCurrent.Namespaces)
					{
						NamespaceCompiler.Declare(ns, this);
					}
				}
				catch (ProtoScriptCompilerException ex)
				{
					ex.m_strProtoScript = fileCurrent.RawCode;
					ex.File = fileCurrent.Info?.FullName;
					throw;
				}
			}
			Logs.DebugLog.WriteTimer("CompileProject.DeclareNamespaces2");

			//──────────────────────────── Declare-FileTypeOfs ─────────────────────
			Logs.DebugLog.CreateTimer("CompileProject.DeclareFileTypeOfs");
			foreach (File fileCurrent in lstFiles)
			{
				try
				{
					compiler.DeclareFileTypeOfs(fileCurrent);
				}
				catch (ProtoScriptCompilerException ex)
				{
					ex.m_strProtoScript = fileCurrent.RawCode;
					ex.File = fileCurrent.Info?.FullName;
					throw;
				}
			}
			Logs.DebugLog.WriteTimer("CompileProject.DeclareFileTypeOfs");

			//──────────────────────────── Define-PrototypeFields ──────────────────
			Logs.DebugLog.CreateTimer("CompileProject.DefinePrototypeFields");
			foreach (File fileCurrent in lstFiles)
			{
				try
				{
					compiler.DefineFilePrototypeFields(fileCurrent);
				}
				catch (ProtoScriptCompilerException ex)
				{
					ex.m_strProtoScript = fileCurrent.RawCode;
					ex.File = fileCurrent.Info?.FullName;
					throw;
				}
			}
			Logs.DebugLog.WriteTimer("CompileProject.DefinePrototypeFields");

			//──────────────────────────── Declare-PrototypeFunctions ──────────────
			Logs.DebugLog.CreateTimer("CompileProject.DeclarePrototypeFunctions");
			foreach (File fileCurrent in lstFiles)
			{
				try
				{
					compiler.DeclareFilePrototypeFunctions(fileCurrent);
				}
				catch (ProtoScriptCompilerException ex)
				{
					ex.m_strProtoScript = fileCurrent.RawCode;
					ex.File = fileCurrent.Info?.FullName;
					throw;
				}
			}
			Logs.DebugLog.WriteTimer("CompileProject.DeclarePrototypeFunctions");

			//──────────────────────────── Declare-FileFunctions ───────────────────
			Logs.DebugLog.CreateTimer("CompileProject.DeclareFileFunctions");
			foreach (File fileCurrent in lstFiles)
			{
				lstStatements.AddRange(compiler.DeclareFileFunctions(fileCurrent));
			}
			Logs.DebugLog.WriteTimer("CompileProject.DeclareFileFunctions");

			//──────────────────────────── Compile-FileFunctions ───────────────────
			Logs.DebugLog.CreateTimer("CompileProject.CompileFileFunctions");
			foreach (File fileCurrent in lstFiles)
			{
				try
				{
					lstStatements.AddRange(compiler.CompileFileFunctions(fileCurrent));
				}
				catch (ProtoScriptCompilerException ex)
				{
					ex.m_strProtoScript = fileCurrent.RawCode;
					ex.File = fileCurrent.Info?.FullName;
					throw;
				}
			}
			Logs.DebugLog.WriteTimer("CompileProject.CompileFileFunctions");

			//──────────────────────────── Define-Namespaces ───────────────────────
			Logs.DebugLog.CreateTimer("CompileProject.DefineNamespaces");
			foreach (File fileCurrent in lstFiles)
			{
				try
				{
					foreach (NamespaceDefinition ns in fileCurrent.Namespaces)
					{
						lstStatements.AddRange(NamespaceCompiler.Define(ns, this));
					}
				}
				catch (ProtoScriptCompilerException ex)
				{
					ex.m_strProtoScript = fileCurrent.RawCode;
					ex.File = fileCurrent.Info?.FullName;
					throw;
				}
			}
			Logs.DebugLog.WriteTimer("CompileProject.DefineNamespaces");

			//──────────────────────────── Define-Prototypes ───────────────────────
			Logs.DebugLog.CreateTimer("CompileProject.DefinePrototypes");
			//if (!AllowParallelism)
			//{
				foreach (File fileCurrent in lstFiles)
				{
					try
					{
						lstStatements.AddRange(PrototypeCompiler.DefinePrototypes(fileCurrent, this));
					}
					catch (ProtoScriptCompilerException ex)
					{
						ex.m_strProtoScript = fileCurrent.RawCode;
						ex.File = fileCurrent.Info?.FullName;
						throw;
					}
				}
			//}
			//else
			//{
			//	//This doesn't work because of ActiveScope
			//	ConcurrentBag<Compiled.Statement> bagPrototypes = new ConcurrentBag<Compiled.Statement>();

			//	Parallel.ForEach(lstFiles, fileCurrent =>
			//	{
			//		try
			//		{
			//			foreach (Compiled.Statement stmt in PrototypeCompiler.DefinePrototypes(fileCurrent, this))
			//				bagPrototypes.Add(stmt);
			//		}
			//		catch (ProtoScriptCompilerException ex)
			//		{
			//			ex.m_strProtoScript = fileCurrent.RawCode;
			//			ex.File = fileCurrent.Info?.FullName;
			//			throw;
			//		}
			//	});

			//	lstStatements.AddRange(bagPrototypes);
			//}

			Logs.DebugLog.WriteTimer("CompileProject.DefinePrototypes");

			//──────────────────────────── Compile-Annotations ─────────────────────
			Logs.DebugLog.CreateTimer("CompileProject.CompileAnnotations");
			foreach (File fileCurrent in lstFiles)
			{
				try
				{
					lstStatements.AddRange(compiler.CompileFileFunctionAnnotations(fileCurrent));
				}
				catch (ProtoScriptCompilerException ex)
				{
					ex.m_strProtoScript = fileCurrent.RawCode;
					ex.File = fileCurrent.Info?.FullName;
					throw;
				}
			}
			Logs.DebugLog.WriteTimer("CompileProject.CompileAnnotations");

			//──────────────────────────── Compile-Statements ──────────────────────
			Logs.DebugLog.CreateTimer("CompileProject.CompileStatements");
			foreach (File fileCurrent in lstFiles)
			{
				try
				{
					lstStatements.AddRange(compiler.CompileFileStatements(fileCurrent));
				}
				catch (ProtoScriptCompilerException ex)
				{
					ex.m_strProtoScript = fileCurrent.RawCode;
					ex.File = fileCurrent.Info?.FullName;
					throw;
				}
			}
			Logs.DebugLog.WriteTimer("CompileProject.CompileStatements");

			Logs.DebugLog.WriteTimer("CompileProject.CompileFileList");

			return lstStatements;
		}


		public List<Compiled.Statement> Compile(PrototypeDefinition prototypeDefinition)
		{
			Compiler compiler = this;
			List<Compiled.Statement> lstStatements = new List<Compiled.Statement>();

			lstStatements.Add(PrototypeCompiler.DeclarePrototype(prototypeDefinition, compiler));
			PrototypeCompiler.DeclarePrototypeTypeOfs(prototypeDefinition, compiler);
			PrototypeCompiler.DefinePrototypeFields(prototypeDefinition, compiler);
			PrototypeCompiler.DeclarePrototypeFunctions(prototypeDefinition, compiler);
			lstStatements.AddRange(PrototypeCompiler.DefinePrototype(prototypeDefinition, compiler));


			return lstStatements;
		}


		static public List<File> GetAllIncludedFiles(File file, bool bAllowParallelism)
		{
			List<File> lstFiles = new List<File>();

			//Note: testing shows parallelism has no effect on performance.
			if (bAllowParallelism)
			{
				GetIncludedFilesRecursive(file, lstFiles);
			}
			else
			{
				GetIncludedFiles(file, lstFiles);
			}

			return lstFiles;
		}

		///	Recursively gather all included files in parallel.
		static private void GetIncludedFilesRecursive(File file, List<File> lstFiles)
		{
			//TODO: Doesn't currently work because the order of defining prototypes still matters. 

			ConcurrentDictionary<string, byte> seen = new(StringComparer.OrdinalIgnoreCase);
			ConcurrentQueue<File> queue = new();
			ConcurrentBag<File> bag = new();

			// ── seed ───────────────────────────────────────────────────────────────
			seen.TryAdd(file.Info.FullName, 0);
			queue.Enqueue(file);
			bag.Add(file);

			const int BatchSize = 32;

			// ── breadth-first expansion ────────────────────────────────────────────
			while (!queue.IsEmpty)
			{
				List<File> batch = new(BatchSize);
				while (batch.Count < BatchSize && queue.TryDequeue(out File f))
					batch.Add(f);

				Parallel.ForEach(batch, fileCurrent =>
				{
					string rootDir = StringUtil.LeftOfLast(fileCurrent.Info.FullName, "\\");

					foreach (IncludeStatement inc in fileCurrent.Includes)
					{
						IEnumerable<string> paths =
							inc.FileName.Contains('*')
								? Directory.GetFiles(rootDir,
													 inc.FileName,
													 inc.Recursive ? SearchOption.AllDirectories
																   : SearchOption.TopDirectoryOnly)
								: new[] { FileUtil.BuildPath(rootDir, inc.FileName) };

						foreach (string path in paths)
						{
							File? sub = TryParse(path);
							if (sub == null) continue;

							if (seen.TryAdd(sub.Info.FullName, 0))
							{
								bag.Add(sub);
								queue.Enqueue(sub);
							}
							else
							{
								Logs.DebugLog.WriteEvent("**** WARNING **** File already included", sub.Info.FullName);
							}
						}
					}
				});
			}

			lstFiles.AddRange(bag);
		}



		static private void GetIncludedFiles(File file, List<File> lstFiles)
		{
			string strRootDir = StringUtil.LeftOfLast(file.Info.FullName, "\\");

			foreach (IncludeStatement include in file.Includes)
			{
				if (include.FileName.Contains("*"))
				{
					foreach (string strFile in Directory.GetFiles(strRootDir, include.FileName, include.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
					{
						File? fileSub = TryParse(strFile);
						if (null != fileSub)
						{
							if (!lstFiles.Any(x => StringUtil.EqualNoCase(x.Info.FullName, fileSub.Info.FullName)))
							{
								lstFiles.Add(fileSub);
								GetIncludedFiles(fileSub, lstFiles);
							}
							else
							{
								//This is just to see if we are wasting time parsing, if so rewrite to check first
								Logs.DebugLog.WriteEvent("**** WARNING **** File already included", fileSub.Info.FullName);
							}
						}
					}
				}
				else
				{
					File? fileSub = TryParse(FileUtil.BuildPath(strRootDir, include.FileName));
					if (null != fileSub)
					{
						if (!lstFiles.Any(x => StringUtil.EqualNoCase(x.Info.FullName, fileSub.Info.FullName)))
						{
							lstFiles.Add(fileSub);
							GetIncludedFiles(fileSub, lstFiles);
						}
						else
						{
							Logs.DebugLog.WriteEvent("**** WARNING **** File already included", fileSub.Info.FullName);
						}
					}
				}
			}
		}

	


		static private ProtoScript.File TryParse(string strFile)
		{
			try
			{
				if (Parsers.Settings.AllowPrecompiled && System.IO.File.Exists(strFile + ".json"))
				{
					return new File() { Info = new FileInfo(strFile), RawCode = FileUtil.ReadFile(strFile + ".json"), IsPrecompiled = true };
				}

				return ProtoScript.Parsers.Files.Parse(strFile);
			}
			catch (Parsers.Files.FileDoesNotExistException err)
			{
				Parsers.ProtoScriptParsingException ex = new Parsers.ProtoScriptParsingException("", 0, "");
				ex.Explanation = "File does not exist: " + err.Message;
				ex.File = strFile;
throw;
			}
		}




		public Compiled.File Compile(ProtoScript.File fileCurrent)
		{
			Compiled.File file = new Compiled.File();
			file.Statements = CompileFileList(new List<File> { fileCurrent });
			return file;
		}

		public List<Compiled.Statement> DeclareFilePrototypes(ProtoScript.File file)
		{
			Compiler compiler = this;
			compiler.Source = file.RawCode;

			List<Compiled.Statement> lstStatements = new List<Compiled.Statement>();

			foreach (ReferenceStatement statement in file.References)
			{
				compiler.Compile(statement);
			}

			foreach (ImportStatement statement in file.Imports)
			{
				compiler.Compile(statement);
			}

			foreach (PrototypeDefinition protoDef in file.PrototypeDefinitions)
			{
				Compiled.Statement statement = PrototypeCompiler.DeclarePrototype(protoDef, compiler);
				if (null != statement)
					lstStatements.Add(statement);
			}

			foreach (Compiled.Statement statement in lstStatements)
			{
				statement.Info.File = file.Info?.FullName;
			}

		

			return lstStatements;
		}

		public MethodEvaluation GetAnnotationMethodEvaluation(AnnotationExpression annotation)
		{
			return annotation.GetAnnotationMethodEvaluation();
		}

		public List<Compiled.Statement> AnnotateFile(ProtoScript.File file)
		{
			Compiler compiler = this;
			compiler.Source = file.RawCode;

			List<Compiled.Statement> lstStatements = new List<Compiled.Statement>();

			foreach (PrototypeDefinition protoDef in file.PrototypeDefinitions)
			{
				lstStatements.AddRange(PrototypeCompiler.AnnotatePrototype(protoDef, this));
			}

			return lstStatements;
		}

		public List<Compiled.Statement> DeclareFileTypeOfs(ProtoScript.File file)
		{
			Compiler compiler = this;
			compiler.Source = file.RawCode;

			foreach (PrototypeDefinition protoDef in file.PrototypeDefinitions)
			{
				PrototypeCompiler.DeclarePrototypeTypeOfs(protoDef, compiler);
			}

			return null;
		}

		public List<Compiled.Statement> DefineFilePrototypeFields(ProtoScript.File file)
		{
			Compiler compiler = this;
			compiler.Source = file.RawCode;

			List<Compiled.Statement> lstStatements = new List<Compiled.Statement>();

			foreach (PrototypeDefinition protoDef in file.PrototypeDefinitions)
			{
				lstStatements.AddRange(PrototypeCompiler.DefinePrototypeFields(protoDef, this));
			}

			return lstStatements;
		}


		public List<Compiled.Statement> DeclareFilePrototypeFunctions(ProtoScript.File file)
		{
			Compiler compiler = this;
			compiler.Source = file.RawCode;

			List<Compiled.Statement> lstStatements = new List<Compiled.Statement>();

			foreach (PrototypeDefinition protoDef in file.PrototypeDefinitions)
			{
				lstStatements.AddRange(PrototypeCompiler.DeclarePrototypeFunctions(protoDef, compiler));
			}

			return lstStatements;
		}
		public List<Compiled.Statement> DeclareFileFunctions(ProtoScript.File file)
		{
			Compiler compiler = this;
			compiler.Source = file.RawCode;

			List<Compiled.Statement> lstStatements = new List<Compiled.Statement>();

			foreach (Statement statement in file.Statements)
			{
				if (statement is FunctionDefinition functionDefinition)
					lstStatements.Add(DeclareFunction(functionDefinition));
			}

			return lstStatements;
		}

		public List<Compiled.Statement> CompileFileFunctions(ProtoScript.File file)
		{
			Compiler compiler = this;
			compiler.Source = file.RawCode;

			List<Compiled.Statement> lstStatements = new List<Compiled.Statement>();


			foreach (Statement statement in file.Statements)
			{
				if (statement is FunctionDefinition functionDefinition)
					Compile(functionDefinition);
			}

			return lstStatements;
		}


		public List<Compiled.Statement> CompileFileFunctionAnnotations(ProtoScript.File file)
		{
			Compiler compiler = this;
			compiler.Source = file.RawCode;

			List<Compiled.Statement> lstStatements = new List<Compiled.Statement>();


			foreach (Statement statement in file.Statements)
			{
				if ((statement is FunctionDefinition functionDefinition))
					lstStatements.AddRange(CompileFunctionAnnotations(functionDefinition));
			}

			return lstStatements;
		}

		public List<Compiled.Statement> CompileFileStatements(ProtoScript.File file)
		{
			Compiler compiler = this;
			compiler.Source = file.RawCode;

			List<Compiled.Statement> lstStatements = new List<Compiled.Statement>();

			//N20240929-01 - Use global scope since the local scope is no longer saved per file
			Scope localScope = Symbols.GetGlobalScope();

			try
			{
				foreach (Statement statement in file.Statements)
				{
					if (statement is FunctionDefinition)
					{
						continue;
					}

					lstStatements.Add(compiler.Compile(statement));
				}
			}
			finally
			{

			}

			return lstStatements;
		}


		public Compiled.Statement Compile(Statement statement)
		{
			switch (statement)
			{
				case VariableDeclaration v:
					return Compile(v);

				case ExpressionStatement e:
					return Compile(e);

				case ReturnStatement r:
					return Compile(r);

				case IfStatement i:
					return Compile(i);

				case ForEachStatement f:
					return Compile(f);

				case CodeBlockStatement c:
					return Compile(c);

				case DoStatement d:
					return DoCompiler.Compile(d, this);

				case WhileStatement w:
					return WhileCompiler.Compile(w, this);

				case TryStatement t:
					return TryCompiler.Compile(t, this);

				case ThrowStatement th:
					return TryCompiler.Compile(th, this);

				default:
					throw new ProtoScriptCompilerException(
						statement.Info,
						"Unknown statement type");
			}
		}


		public Compiled.Expression ? CompileRootIdentifier(string strValue, StatementParsingInfo info)
		{
			if (strValue == "global")
				return new GetGlobalStack() { Index = -1, InferredType = new Namespace() { Scope = Symbols.GetGlobalScope() }, Info = info};

			Scope scope;
			object ? obj = Symbols.GetSymbolAndScope(strValue, out scope);

			if (null == obj)
			{
				if (strValue.Contains("<"))
				{
					obj = Symbols.GetTypeInfo(ProtoScript.Parsers.Types.Parse(strValue));
				}

			}

			if (obj is null)
				return null;

			switch (obj)
			{
				case FieldTypeInfo fieldTypeInfo:

					return new PrototypeFieldReference()
					{
						Left = CompileRootIdentifier("this", info),
						Right = new GetGlobalStack() { Index = fieldTypeInfo.Index, InferredType = fieldTypeInfo.FieldInfo, Info = info },
						InferredType = fieldTypeInfo.FieldInfo,
						FieldInfo = fieldTypeInfo,
						Info = info
					};
				case PrototypeTypeInfo pti:
					return new GetGlobalStack { Index = pti.Index, InferredType = pti, Info = info };

				case ValueRuntimeInfo vri when scope.ScopeType == Scope.ScopeTypes.Method:
					return new GetLocalStack { Index = vri.Index, InferredType = vri.Type, Info = info };

				case ValueRuntimeInfo vri:        // file / block scope
					return new GetStack { Index = vri.Index, InferredType = vri.Type, Scope = scope, Info = info };

				case DotNetTypeInfo dti:
					return new GetGlobalStack { Index = dti.Index, InferredType = dti, Info = info };

				case FunctionRuntimeInfo fri:
					return new GetGlobalStack
					{
						Index = fri.Index,
						InferredType = new TypeInfo(typeof(FunctionRuntimeInfo)),
						Info = info
					};

				case Namespace ns:
					return new GetGlobalStack { Index = ns.Index, InferredType = ns, Info = info };

				// assemblies are ignored by design
				case System.Reflection.Assembly:
					return null;

				default:
					return null;
			}
		}

		public Compiled.ExpressionStatement Compile(ExpressionStatement statement)
		{
			return new Compiled.ExpressionStatement
			{
				Expression = Compile(statement.Expression),
				Info = statement.Info
			};
		}

		public Compiled.VariableDeclaration Compile(VariableDeclaration statement)
		{
			TypeInfo typeInfo = Symbols.GetTypeInfo(statement.Type);

			if (null == typeInfo)
			{
				this.AddDiagnostic(new UnknownType(statement.Type.TypeName), statement, statement.Type);
				return null;
			}


			if (Symbols.ActiveScope().TryGetSymbol(statement.VariableName, out object oExisting))
			{
				this.AddDiagnostic(new Diagnostic($"{statement.VariableName} already declared in local scope"), statement, null);
				return null;
			}

			Compiled.VariableDeclaration declaration = new Compiled.VariableDeclaration();
			declaration.Info = statement.Info;

			if (typeInfo is PrototypeTypeInfo || typeInfo is DotNetTypeInfo)
			{
				VariableRuntimeInfo info = new VariableRuntimeInfo();

				info.Type = typeInfo;
				info.Index = Symbols.LocalStack.Add(info);
				info.OriginalType = typeInfo.Clone();

				Symbols.InsertSymbol(statement.VariableName, info);

				if (null != statement.Initializer)
				{
					declaration.Initializer = Compile(statement.Initializer);


					//This causes every phrase[0] to fail, so let's limit it to just .net types
					if (typeInfo is DotNetTypeInfo)
					{
						if (!SimpleInterpretter.IsAssignableFrom(declaration.Initializer.InferredType, typeInfo))
						{
							this.AddDiagnostic(new CannotConvert(declaration.Initializer.InferredType.ToString(), typeInfo.ToString()), statement, null);
							return null;
						}
					}
				}

				declaration.RuntimeInfo = info;
			}

			else if (typeInfo is TypeInfo)
			{
				VariableRuntimeInfo info = new VariableRuntimeInfo();				
				info.Type = typeInfo as TypeInfo;
				info.OriginalType = info.Type.Clone();
				info.Index = Symbols.LocalStack.Add(info);

				Symbols.InsertSymbol(statement.VariableName, info);

				if (null != statement.Initializer)
				{
					declaration.Initializer = Compile(statement.Initializer);

					if (null != declaration.Initializer && !SimpleInterpretter.IsAssignableFrom(declaration.Initializer.InferredType, typeInfo))
					{
						this.AddDiagnostic(new CannotConvert(declaration.Initializer.InferredType.ToString(), typeInfo.ToString()), statement, null);
						return null;
					}
				}					

				declaration.RuntimeInfo = info;
			}

			else
				throw new NotImplementedException();

			return declaration;
		}

		public Compiled.Expression Compile(Expression expression)
		{
			if (null == expression)
				throw new Exception("Unexpected");

			if (null == expression.Terms || expression.Terms.Count == 0)
				return CompileTerm(expression);

			if (expression.Terms.Count > 1)
				throw new Exception("Unexpected");

			foreach (Expression term in expression.Terms)
			{
				return CompileTerm(term);
			}

			throw new NotImplementedException();
		}

		public Compiled.Expression CompileTerm(Expression term)
		{
			if (term is BinaryOperator)
				return Compile(term as BinaryOperator);

			if (term is UnaryOperator)
				return Compile(term as UnaryOperator);

			if (term is Identifier)
				return Compile(term as Identifier);

			if (term is NewObjectExpression)
				return Compile(term as NewObjectExpression);

			if (term is MethodEvaluation)
				return Compile(term as MethodEvaluation);

			if (term is Literal)
				return Compile(term as Literal);

			if (term is ExpressionList)
				return Compile(term as ExpressionList);

			if (term is CategorizationOperator)
				return Compile(term as CategorizationOperator);

			if (term is Expression && term.Terms.Count > 0)
				return Compile(term as Expression);

			throw new ProtoScriptCompilerException(term.Info, "Unsupported expression term");
		}

		public Compiled.Expression Compile(CategorizationOperator op)
		{
			Compiled.Expression compiledLeft = Compile(op.Left);
			Compiled.Expression compiledMiddle = Compile(op.Middle);
			GetGlobalStack identifier = compiledMiddle as GetGlobalStack;
			if (null == identifier)
			{
				this.AddDiagnostic(new Diagnostic("Categorization must be on a Prototype"), null, op);
				return null;
			}

			if (!(identifier.InferredType is PrototypeTypeInfo))
			{
				if (!ReflectionUtil.HasBaseType(identifier.InferredType.Type, typeof(Prototype)))
				{
					this.AddDiagnostic(new Diagnostic("Categorization must be on a Prototype"), null, op);
					return null;
				}
			}

			Compiled.ScopedExpressionList compiledRight = null;
			Scope scope = new Scope(Scope.ScopeTypes.Block);
			Symbols.EnterScope(scope);

			try
			{
				TypeInfo infoThis = identifier.InferredType;
				ValueRuntimeInfo infoThisInstance = new ValueRuntimeInfo();
				infoThisInstance.Index = scope.Stack.Add(infoThisInstance);
				infoThisInstance.OriginalType = infoThis.Clone();
				infoThisInstance.Type = infoThis.Clone();
				scope.InsertSymbol("this", infoThisInstance);

				compiledRight = Compile(op.Right);
				compiledRight.Scope = scope;

			}
			finally
			{
				Symbols.LeaveScope();
			}

			return new Compiled.CategorizationOperator
			{
				Left = compiledLeft,
				Middle = compiledMiddle,
				Right = compiledRight,
				InferredType = new TypeInfo(typeof(bool)),
				Info = op.Info
			};
		}

		public Compiled.ScopedExpressionList Compile(ScopedExpressionList expr)
		{
			Compiled.ScopedExpressionList lst = new Compiled.ScopedExpressionList();
			lst.Info = expr.Info;
			lst.InferredType = new TypeInfo(typeof(bool));

			foreach (Expression expression in expr.Expressions)
			{
				lst.Expressions.Add(Compile(expression));
			}

			return lst;
		}

		public Compiled.Expression Compile(ExpressionList exp)
		{
			if (exp.Expressions.Count != 1)
				throw new NotImplementedException();

			return Compile(exp.Expressions.First());
		}

		public Compiled.Expression Compile(BinaryOperator exp)
		{
			if (exp.Value == "=")
			{
				return CompileAssignmentOperator(exp);
			}

			if (exp.Value == "typeof")
			{
				return CompileTypeOfOperator(exp);
			}

			if (exp is IndexOperator)
			{
				return Compile(exp as IndexOperator);
			}

			if (exp.Value == "=>")
			{
				return CompileLambda(exp);
			}

			if (exp.Value == "==")
			{
				return CompileEquals(exp);
			}

			if (exp.Value == "!=")
			{
				return CompileNotEquals(exp);
			}

			if (exp.Value == "as")
				return CompileCastingOperator(exp);

			if (exp.Value == "cast")
				return CompileCastingOperator2(exp);

			if (exp.Value == "||")
				return CompileOrOperator(exp);

			if (exp.Value == "&&")
				return CompileAndOperator(exp);

			if (exp.Value == ".")
				return CompileDotOperator(exp);

			if (exp.Value == "+")
				return CompileAddOperator(exp);

			if (exp.Value == ">" || exp.Value == "<" || exp.Value == ">=" || exp.Value == "<=")
				return CompileComparisonOperator(exp);

			if (exp.Value == "+=")
				return CompileAddAssignmentOperator(exp);

			throw new ProtoScriptCompilerException(exp.Info, $"Unsupported operator encountered: '{exp.Value}'");
		}

		public NewInstance.ObjectInitializer Compile(NewObjectExpression.ObjectInitializer initializer, Prototype prototype)
		{
			var tuple = SimpleInterpretter.ResolveProperty(prototype, initializer.Name);
			if (null == tuple || null == tuple.Item1 || null == tuple.Item2)
			{
				this.AddDiagnostic(new Diagnostic("Could not find property field: " + initializer.Name), null, initializer);
				return null;
			}

			Compiled.Expression expr = Compile(initializer.Value);

			return new NewInstance.ObjectInitializer() {
				Property = tuple.Item1, 
				Value = expr
			};

		}

		public Compiled.Expression CompileDotOperator(BinaryOperator op)
		{
			Compiled.Expression compiledLeft = Compile(op.Left);
			if (null == compiledLeft)
				return null;

			if (op.Right is Identifier)
			{
				Identifier identifier = op.Right as Identifier;
				string strPropertyName = identifier.Value;

				if (compiledLeft.InferredType is PrototypeTypeInfo)
				{
					PrototypeTypeInfo prototypeTypeInfo = compiledLeft.InferredType as PrototypeTypeInfo;
					TypeInfo infoType = prototypeTypeInfo.Scope.GetSymbol(strPropertyName) as TypeInfo;

					//Don't use "is" here we only want the base type
					if (null != infoType && infoType.GetType() == typeof(PrototypeTypeInfo))
					{
						return new GetGlobalStack() { Index = infoType.Index, InferredType = infoType };
					}		

					FieldTypeInfo infoField = GetFieldInfo(prototypeTypeInfo, strPropertyName);
					if (null != infoField)
					{
PrototypeFieldReference res = new PrototypeFieldReference()
{
Left = compiledLeft,
Right = new GetGlobalStack() { Index = infoField.Index, InferredType = infoField.FieldInfo, Info = op.Right.Info },
InferredType = infoField.FieldInfo,
FieldInfo = infoField,
Info = op.Info
};
res.IsNullConditional = op.Value == "?.";
return res;
					}


					//Try resolving as a method 
					//{
					//	var tuple2 = SimpleInterpretter.ResolveMethod(prototypeTypeInfo.Prototype, strPropertyName);

					//	if (null != tuple2)
					//	{
					//		Prototype protoProp = tuple2.Item1;
					//		Prototype protoPrototype = tuple2.Item2;

					//		if (!(protoProp.PrototypeName != "ProtoScript.Interpretter.RuntimeInfo.FunctionRuntimeInfo"))
					//			throw new Exception("Unexpected");

					//		PrototypeTypeInfo typeInfoParent = Symbols.GetGlobalScope().GetSymbol(protoPrototype.PrototypeName) as PrototypeTypeInfo;
					//		FunctionRuntimeInfo functionRuntimeInfo = typeInfoParent.Scope.GetSymbol(strPropertyName) as FunctionRuntimeInfo;

					//		return new GetStack() { Scope = typeInfoParent.Scope, Index = functionRuntimeInfo.Index, InferredType = new TypeInfo(typeof(FunctionRuntimeInfo)) };
					//	}
					//}

					{
						FunctionRuntimeInfo functionRuntimeInfo = MethodCompiler.ResolveMethod2(prototypeTypeInfo.Prototype, strPropertyName, this.Symbols);

						if (null != functionRuntimeInfo)
						{							
							PrototypeTypeInfo typeInfoParent = Symbols.GetGlobalScope().GetSymbol(functionRuntimeInfo.ParentPrototype.PrototypeName) as PrototypeTypeInfo;

							return new GetStack() { Scope = typeInfoParent.Scope, Index = functionRuntimeInfo.Index, InferredType = new TypeInfo(typeof(FunctionRuntimeInfo)) };
						}
					}
				}

				else if (compiledLeft.InferredType is Namespace)
				{
					Namespace ns = compiledLeft.InferredType as Namespace;
					TypeInfo infoType = ns.Scope.GetSymbol(strPropertyName) as TypeInfo;
					if (null == infoType)
					{
						this.AddDiagnostic($"Cannot find {strPropertyName} in namespace", null, op);
					}


					if (infoType is PrototypeTypeInfo || infoType is Namespace)
					{
						return new GetGlobalStack() { Index = infoType.Index, InferredType = infoType };
					}
					else
					{
						this.AddDiagnostic("Can only reference prototype or namespace within a namespace", null, op);
						return null;
					}
				}

				Compiled.Expression objCur = compiledLeft;
				//Try as a .NET property 
				{
Compiled.Expression ? memberInfo = GetDotNetMemberReference(op.Info, strPropertyName, objCur);
if (null != memberInfo)
{
if (memberInfo is DotNetFieldReference df)
df.IsNullConditional = op.Value == "?.";
else if (memberInfo is DotNetPropertyReference dp)
dp.IsNullConditional = op.Value == "?.";
return memberInfo;
}
				}

				this.AddDiagnostic(new Diagnostic($"Could not find property {strPropertyName}"), null, op);
				return null;
			}
			else if (op.Right is MethodEvaluation)
			{
Compiled.Expression compiledRight = CompileMethodEvaluationInternal(op.Right as MethodEvaluation, compiledLeft);
if (compiledRight is DotNetMethodEvaluation dme)
dme.IsNullConditional = op.Value == "?.";
return compiledRight;
			}
			else if (op.Right is BinaryOperator && (op.Right as BinaryOperator).Value == ".")
			{

			}

			this.AddDiagnostic(new Diagnostic("Could not compile expression"), null, op);
			return null;			
		}

		internal static Compiled.Expression ? GetDotNetMemberReference(StatementParsingInfo info, string strPropertyName, Compiled.Expression objCur)
		{
			System.Reflection.FieldInfo fieldInfo = objCur.InferredType.Type.GetField(strPropertyName);
			if (null != fieldInfo)
			{
				return new DotNetFieldReference()
				{
					Field = fieldInfo,
					Info = info,
					InferredType = new TypeInfo(fieldInfo.FieldType),
					Object = objCur
				};
			}

			System.Reflection.PropertyInfo propertyInfo = objCur.InferredType.Type.GetProperty(strPropertyName);
			if (null != propertyInfo)
			{
				return new DotNetPropertyReference()
				{
					Property = propertyInfo,
					Info = info,
					InferredType = new TypeInfo(propertyInfo.PropertyType),
					Object = objCur
				};
			}

			return null;
		}

		public FieldTypeInfo GetFieldInfo(PrototypeTypeInfo prototypeTypeInfo, string strPropertyName)
		{

			//Check the primary prototype
			{
				PrototypeTypeInfo prototypeFieldInfo = prototypeTypeInfo.Scope.GetSymbol(strPropertyName) as PrototypeTypeInfo;
				if (null != prototypeFieldInfo)
				{
					FieldTypeInfo infoField = null;

					//check first for an initializer locally
					if (prototypeFieldInfo is FieldTypeInfo)
						infoField = prototypeFieldInfo as FieldTypeInfo;
					else
						infoField = Symbols.GetGlobalScope().GetSymbol(prototypeFieldInfo.Prototype.PrototypeName) as FieldTypeInfo;

					return infoField;
				}
			}

			//Check the typeofs (especially for external, which won't have temporary prototype)
			foreach (int protoTypeOf in prototypeTypeInfo.Prototype.GetAllParents())
			{
				PrototypeTypeInfo parentTypeInfo = Symbols.GetGlobalScope().GetSymbol(Prototypes.GetPrototypeName(protoTypeOf)) as PrototypeTypeInfo;

				//Types created via CSharp don't have Scopes (for now, you may want to fix that later)
				if (null != parentTypeInfo)
				{
					if (null == parentTypeInfo.Scope)
					{
						this.AddDiagnostic("No scope setup for prototype: " + parentTypeInfo.Prototype.PrototypeName, null, null);
						return null;
					}

					PrototypeTypeInfo prototypeFieldInfo = parentTypeInfo.Scope.GetSymbol(strPropertyName) as PrototypeTypeInfo;
					if (null != prototypeFieldInfo)
					{
						if (prototypeFieldInfo is FieldTypeInfo)
							return (FieldTypeInfo)prototypeFieldInfo;
						
						FieldTypeInfo infoField = Symbols.GetGlobalScope().GetSymbol(prototypeFieldInfo.Prototype.PrototypeName) as FieldTypeInfo;
						return infoField;
					}
				}
			}

			var tuple = SimpleInterpretter.ResolveProperty(prototypeTypeInfo.Prototype, strPropertyName);
			if (null != tuple)
			{
				Prototype protoProp = tuple.Item1;

				FieldTypeInfo fieldTypeInfo = Symbols.GetGlobalScope().GetSymbol(protoProp.PrototypeName) as FieldTypeInfo;
				if (null != fieldTypeInfo)
					return fieldTypeInfo;
			}

			return null;
		}

		public Compiled.Expression CompileAddOperator(BinaryOperator op)
		{
			Compiled.Expression compiledLeft = Compile(op.Left);
			Compiled.Expression compiledRight = Compile(op.Right);

			if (!SimpleInterpretter.IsAssignableFrom(compiledLeft.InferredType, new TypeInfo(typeof(string))))
			{
				this.AddDiagnostic(new Diagnostic("Only string concatenation supported"), null, op);
				return null;
			}

			if (!SimpleInterpretter.IsAssignableFrom(compiledRight.InferredType, new TypeInfo(typeof(string))))
			{
				this.AddDiagnostic(new Diagnostic("Only string concatenation supported"), null, op);
				return null;
			}

			return new Compiled.AddOperator
			{
				Left = compiledLeft,
				Right = compiledRight,
				InferredType = new TypeInfo(typeof(string)),
			};
		}
		public Compiled.Expression CompileAndOperator(BinaryOperator op)
		{
			Compiled.Expression compiledLeft = Compile(op.Left);
			Compiled.Expression compiledRight = Compile(op.Right);

			return new Compiled.AndOperator
			{
				Left = compiledLeft,
				Right = compiledRight,
				InferredType = new TypeInfo(typeof(bool)),
			};
		}
		public Compiled.Expression CompileOrOperator(BinaryOperator op)
		{
			Compiled.Expression compiledLeft = Compile(op.Left);
			Compiled.Expression compiledRight = Compile(op.Right);

			return new Compiled.OrOperator
			{
				Left = compiledLeft,
				Right = compiledRight,
				InferredType = new TypeInfo(typeof(bool))
			};
		}

		public Compiled.Expression CompileComparisonOperator(BinaryOperator op)
		{
			Compiled.Expression compiledLeft = Compile(op.Left);
			Compiled.Expression compiledRight = Compile(op.Right);

			if (!SimpleInterpretter.IsAssignableFrom(compiledLeft.InferredType, new TypeInfo(typeof(int))))
			{
				this.AddDiagnostic(new Diagnostic("Only integer comparisons supported"), null, op);
				return null;
			}

			if (!SimpleInterpretter.IsAssignableFrom(compiledRight.InferredType, new TypeInfo(typeof(int))))
			{
				this.AddDiagnostic(new Diagnostic("Only integer comparisons supported"), null, op);
				return null;
			}

			return new Compiled.ComparisonOperator
			{
				Left = compiledLeft,
				Right = compiledRight,
				Operator = op.Value,
				InferredType = new TypeInfo(typeof(bool)),
				Info = op.Info
			};
		}


		public Compiled.Expression CompileCastingOperator(BinaryOperator op)
		{
			Compiled.Expression compiledLeft = Compile(op.Left);

			//The right side must be an Identifier and a Type. We can't resolve it normally 
			//or it won't go to global stack to get the correct symbols
			string strType = null;
			if (op.Right is Identifier)
				strType = (op.Right as Identifier).Value;

			else if (op.Right is PrototypeStringLiteral)
			{
				GetGlobalStack op2 = Compile(op.Right as PrototypeStringLiteral) as GetGlobalStack;
                if (null == op2)
                {
                    this.AddDiagnostic(new Diagnostic("Cannot compile target of casting operator"), null, op.Right);
                    return null;
                }

                return new Compiled.CastingOperator
				{
					Left = compiledLeft,
					Right = op2,
					InferredType = op2.InferredType
				};
			}

			else if (op.Right is BinaryOperator && (op.Right as BinaryOperator).Value == ".")
			{
				GetGlobalStack op2 = Compile(op.Right) as GetGlobalStack;
				if (null == op2)
				{
					this.AddDiagnostic(new Diagnostic("Cannot compile target of casting operator"), null, op.Right);
					return null;
				}

				return new Compiled.CastingOperator
				{
					Left = compiledLeft,
					Right = op2,
					InferredType = op2.InferredType
				};
			}

			else
			{
				this.AddDiagnostic(new Diagnostic("Cannot compile target of casting operator"), null, op);
				return null;
			}

			TypeInfo typeInfo = Symbols.GetTypeInfo(strType);
			if (null == typeInfo)
			{
				this.AddDiagnostic(new UnknownType(strType), null, op);
				return null;
			}

			return new Compiled.CastingOperator
			{
				Left = compiledLeft,
				Right = new GetGlobalStack() {Index = typeInfo.Index, InferredType = typeInfo }, 
				InferredType = typeInfo
			};
		}

		public Compiled.Expression CompileCastingOperator2(BinaryOperator op)
		{
			Compiled.Expression compiledLeft = Compile(op.Left);

			//The right side must be an Identifier and a Type. We can't resolve it normally 
			//or it won't go to global stack to get the correct symbols
			string strType = null;
			if (op.Right is Identifier)
				strType = (op.Right as Identifier).Value;

			else if (op.Right is PrototypeStringLiteral)
			{
				GetGlobalStack op2 = Compile(op.Right as PrototypeStringLiteral) as GetGlobalStack;
				return new Compiled.CastingOperator2
				{
					Left = compiledLeft,
					Right = op2,
					InferredType = op2.InferredType
				};
			}

			else
			{
				this.AddDiagnostic(new Diagnostic("Cannot compile target of casting operator"), null, op);
				return null;
			}

			TypeInfo typeInfo = Symbols.GetTypeInfo(strType);
			if (null == typeInfo)
			{
				this.AddDiagnostic(new UnknownType(strType), null, op);
				return null;
			}

			return new Compiled.CastingOperator2
			{
				Left = compiledLeft,
				Right = new GetGlobalStack() { Index = typeInfo.Index, InferredType = typeInfo },
				InferredType = typeInfo
			};
		}

		public Compiled.Expression CompileEquals(BinaryOperator op)
		{
			Compiled.Expression compiledLeft = Compile(op.Left);
			Compiled.Expression compiledRight = Compile(op.Right);

			return new Compiled.EqualsOperator
			{
				Left = compiledLeft,
				Right = compiledRight,
				InferredType = new TypeInfo(typeof(bool))
			};
		}

		public Compiled.Expression CompileNotEquals(BinaryOperator op)
		{
			Compiled.Expression compiledLeft = Compile(op.Left);
			Compiled.Expression compiledRight = Compile(op.Right);

			return new Compiled.NotOperator
			{
				Right =
					new Compiled.EqualsOperator
					{
						Left = compiledLeft,
						Right = compiledRight,
						InferredType = new TypeInfo(typeof(bool))
					}
			};
		}

		public Compiled.Expression CompileLambda(BinaryOperator exp)
		{
			Compiled.LambdaOperator op = new LambdaOperator();
			FunctionRuntimeInfo funcInfo = new FunctionRuntimeInfo();
			op.Function = funcInfo;

			funcInfo.Scope = new Scope(Scope.ScopeTypes.Lambda);
			Symbols.EnterScope(funcInfo.Scope);

			try
			{
				ParameterRuntimeInfo infoParam = new ParameterRuntimeInfo();
				infoParam.Type = new TypeInfo(typeof(Prototype));
				infoParam.OriginalType = new TypeInfo(typeof(Prototype));
				infoParam.Index = funcInfo.Scope.Stack.Add(infoParam);
				
				funcInfo.Parameters.Add(infoParam);
				funcInfo.Scope.InsertSymbol((exp.Left as Identifier).Value, infoParam);

				Compiled.Expression compiledExpression = Compile(exp.Right);
				Compiled.ExpressionStatement compiledStatement = new Compiled.ExpressionStatement();
				compiledStatement.Expression = compiledExpression;

				funcInfo.Statements.Add(compiledStatement);
			}
			finally
			{
				Symbols.LeaveScope();
			}

			return op;
		}

		public Compiled.Expression Compile(UnaryOperator exp)
		{
			if (exp.Value == "!")
			{
				return CompileNotOperator(exp);
			}

			else if (exp is IsInitializedOperator)
			{
				return Compile(exp as IsInitializedOperator);
			}

			throw new ProtoScriptCompilerException(exp.Info, "Unsupported unary operator");
		}
		public Compiled.Expression Compile(IsInitializedOperator exp)
		{
			Compiled.IsInitializedOperator op = new Compiled.IsInitializedOperator();
			Compiled.Expression compiledRight = Compile(exp.Right);
			if (!(compiledRight is PrototypeFieldReference))
			{
				this.AddDiagnostic(new Diagnostic("Expected a prototype field reference"), null, exp);
				return null;
			}	

			PrototypeFieldReference reference = compiledRight as PrototypeFieldReference;
			reference.AllowLazyInitializaton = false;

			op.Right = reference;

			return op;
		}

		public Compiled.Expression Compile(IndexOperator exp)
		{
			Compiled.Expression compiledLeft = Compile(exp.Left);
			Compiled.Expression compiledRight = Compile(exp.Right);

			return new Compiled.IndexOperator {
				Left = compiledLeft,
				Right = compiledRight,
				InferredType = new TypeInfo(typeof(Prototype)),
				Info = exp.Info
			};
		}

		public Compiled.Expression CompileAssignmentOperator(BinaryOperator exp)
		{
			Compiled.Expression compiledLeft = Compile(exp.Left);
			Compiled.Expression compiledRight = Compile(exp.Right);

			if (null == compiledLeft)
			{
				this.AddDiagnostic(new Diagnostic("Cannot evaluate left side"), null, exp);
				return null;
			}

			if (null == compiledRight)
			{
				this.AddDiagnostic(new Diagnostic("Cannot evaluate right side"), null, exp);
				return null;
			}

			if (!SimpleInterpretter.IsAssignableFrom(compiledRight.InferredType, compiledLeft.InferredType))
			{
				this.AddDiagnostic(new CannotConvert(compiledRight.InferredType.ToString(), compiledLeft.InferredType.ToString()), null, exp);
			}				

			return new AssignmentOperator
			{
				Left = compiledLeft,
				Right = compiledRight,
				Info = exp.Info
			};
		}

		public Compiled.Expression CompileAddAssignmentOperator(BinaryOperator op)
		{
			Compiled.Expression compiledLeft = Compile(op.Left);
			Compiled.Expression compiledRight = Compile(op.Right);

			if (!SimpleInterpretter.IsAssignableFrom(compiledLeft.InferredType, new TypeInfo(typeof(string))))
			{
				this.AddDiagnostic(new Diagnostic("Only string concatenation supported"), null, op);
				return null;
			}

			if (!SimpleInterpretter.IsAssignableFrom(compiledRight.InferredType, new TypeInfo(typeof(string))))
			{
				this.AddDiagnostic(new Diagnostic("Only string concatenation supported"), null, op);
				return null;
			}

			return new AssignmentOperator
			{
				Left = compiledLeft,
				Right = new Compiled.AddOperator
				{
					Left = compiledLeft,
					Right = compiledRight,
					InferredType = new TypeInfo(typeof(string))
				},
				Info = op.Info
			};
		}

		public Compiled.Expression CompileTypeOfOperator(BinaryOperator exp)
		{
			Compiled.Expression compiledLeft = Compile(exp.Left);
			Compiled.Expression compiledRight = Compile(exp.Right);

			//Support for external prototypes with unparseable names like System.String[value]
			//use "System.String[value]" instead
			if (compiledRight is Compiled.Literal)
			{
				Compiled.Literal literal = compiledRight as Compiled.Literal;
				string strValue = literal.Value as string;
				compiledRight = CompileRootIdentifier(strValue, compiledRight.Info);
			}

			if (null == compiledRight)
			{
				this.AddDiagnostic(new Diagnostic("Could not locate right side prototype"), null, exp);
				return null; 
			}

			//N20221230-02 - Prevents a difficult to find bug
			if (compiledRight.InferredType is FieldTypeInfo)
			{
				this.AddDiagnostic(new Diagnostic("Prototype mapped to field"), null, exp);
				return null;
			}

			return new TypeOfOperator
			{
				Left = compiledLeft,
				Right = compiledRight
			};
		}

		public Compiled.Expression CompileNotOperator(UnaryOperator exp)
		{
			Compiled.Expression compiledRight = Compile(exp.Right);

			return new NotOperator
			{
				Right = compiledRight
			};
		}



		public Compiled.Expression Compile(NewObjectExpression expr)
		{

			List<Compiled.Expression> lstParams = expr.Parameters.Select(x => Compile(x)).ToList();

			////Right now we don't support new Converts<String>() as it is meaningless
			//if (expr.Type.IsGeneric)
			//{
			//	this.AddDiagnostic("Cannot construct a generic type", null, expr.Type);
			//	return null;
			//}

			//TypeInfo typeInfo = Symbols.GetTypeInfo(expr.Type.TypeName);
			TypeInfo typeInfo = Symbols.GetTypeInfo(expr.Type);

			if (null == typeInfo)
			{
				this.AddDiagnostic(new UnknownType(expr.Type.TypeName), null, expr.Type);
				return null;
			}

			if (typeInfo is PrototypeTypeInfo)
			{
				NewInstance newInstance = new NewInstance() { InferredType = typeInfo };
				newInstance.Parameters = lstParams;

				PrototypeTypeInfo prototypeTypeInfo = typeInfo as PrototypeTypeInfo;
				
				FunctionRuntimeInfo functionRuntimeInfo = prototypeTypeInfo.Scope.GetSymbol(expr.Type.TypeName) as FunctionRuntimeInfo;
				if (null != functionRuntimeInfo)
				{
					newInstance.Constructor = functionRuntimeInfo;
				}

				if (null != expr.Initializers)
				{
//					Symbols.EnterScope(prototypeTypeInfo.Scope);

					try
					{
						foreach (Expression expInitializer in expr.Initializers)
						{
							NewInstance.ObjectInitializer initializer = Compile(expInitializer as NewObjectExpression.ObjectInitializer, prototypeTypeInfo.Prototype);
							newInstance.Initializers.Add(initializer);
						}
					}
					finally
					{
						//Symbols.LeaveScope();
					}
				}

				return newInstance;

			}

			else if (typeInfo is DotNetTypeInfo)
			{
				System.Type type = (typeInfo as DotNetTypeInfo).Type;
				if (lstParams.Any(x => x == null))
				{
					this.AddDiagnostic(new Diagnostic("Cannot resolve parameter"), null, expr);
					return null;
				}

				List<System.Type> lstParameterTypes = lstParams.Select(x => x.InferredType.Type).ToList();

				System.Reflection.ConstructorInfo constructor = type.GetConstructor(lstParameterTypes.ToArray());

				if (null == constructor)
				{
					this.AddDiagnostic(new Diagnostic("No matching constructor on type: " + type.Name), null, expr);
					return null;
				}
				
				return new DotNetNewInstance() { Constructor = constructor, Parameters = lstParams, InferredType = new TypeInfo(type) };
			}

			throw new NotImplementedException();

		}






		public Compiled.Expression Compile(Identifier identifier)
		{
			return Compile(identifier.Value, identifier);
		}

		public Compiled.Expression Compile(string strPath, Expression exp)
		{

			//Lookup the multi-part identifier first, to support external prototype paths
			{
				Compiled.Expression? objCur = CompileRootIdentifier(strPath, exp.Info);

				if (null != objCur)
					return objCur;
			}

			if (strPath.Contains("."))
			{
				string[] strSplits = StringUtil.Split(strPath, ".");

				Compiled.Expression? objCur = CompileRootIdentifier(strSplits[0], exp.Info);

				if (null == objCur)
				{
					this.AddDiagnostic(new Diagnostic($"Cannot find identifier {strSplits[0]}"), null, exp);
					return null;
				}

				for (int i = 1; i < strSplits.Length; i++)
				{
					string strPropertyName = strSplits[i];

					if (objCur.InferredType is PrototypeTypeInfo)
					{
						PrototypeTypeInfo prototypeTypeInfo = objCur.InferredType as PrototypeTypeInfo;

						//Try the immediate scope first
						FieldTypeInfo fieldTypeInfo = prototypeTypeInfo.Scope.GetSymbol(strPropertyName) as FieldTypeInfo;
						if (null != fieldTypeInfo)
						{
							objCur = new PrototypeFieldReference()
							{
								Left = objCur,
								Right = new GetGlobalStack() { Index = fieldTypeInfo.Index, InferredType = fieldTypeInfo.FieldInfo, Info = exp.Info },
								InferredType = fieldTypeInfo.FieldInfo,
								FieldInfo = fieldTypeInfo, 
								Info = exp.Info
							};

							continue;
						}

						//Try the prototype hierarchy
						{
							var tuple = SimpleInterpretter.ResolveProperty(prototypeTypeInfo.Prototype, strPropertyName);

							if (null != tuple)
							{
								Prototype protoProp = tuple.Item1;

								fieldTypeInfo = Symbols.GetGlobalScope().GetSymbol(protoProp.PrototypeName) as FieldTypeInfo;
								if (null == fieldTypeInfo)
									throw new Exception("Unexpected");

								objCur = new PrototypeFieldReference()
								{
									Left = objCur,
									//TODO: Shouldn't the inferred type be globalStack[index].Type
									Right = new GetGlobalStack() { Index = fieldTypeInfo.Index, InferredType = fieldTypeInfo.FieldInfo, Info = exp.Info },
									InferredType = fieldTypeInfo.FieldInfo,
									FieldInfo = fieldTypeInfo,
									Info = exp.Info
								};

								continue;
							}
						}


						//Try resolving as a method 
						{
							FunctionRuntimeInfo functionRuntimeInfo = MethodCompiler.ResolveMethod2(prototypeTypeInfo.Prototype, strPropertyName, this.Symbols);

							if (null != functionRuntimeInfo)
							{
								PrototypeTypeInfo typeInfoParent = Symbols.GetGlobalScope().GetSymbol(functionRuntimeInfo.ParentPrototype.PrototypeName) as PrototypeTypeInfo;
								objCur = new GetStack() { Scope = typeInfoParent.Scope, Index = functionRuntimeInfo.Index, InferredType = new TypeInfo(typeof(FunctionRuntimeInfo)) };

								continue;
							}
						}
					}

					else if (objCur.InferredType is Namespace)
					{
						throw new Exception("This code shouldn't reachable");
					}

					//Try as a .NET property 
					{
						System.Reflection.FieldInfo fieldInfo = objCur.InferredType.Type.GetField(strPropertyName);
						if (null != fieldInfo)
						{
							objCur = new DotNetFieldReference()
							{
								Field = fieldInfo,
								Info = exp.Info,
								InferredType = new TypeInfo(fieldInfo.FieldType),
								Object = objCur
							};

							continue;
						}

						System.Reflection.PropertyInfo propertyInfo = objCur.InferredType.Type.GetProperty(strPropertyName);
						if (null != propertyInfo)
						{
							objCur = new DotNetPropertyReference()
							{
								Property = propertyInfo,
								Info = exp.Info,
								InferredType = new TypeInfo(propertyInfo.PropertyType),
								Object = objCur
							};

							continue;
						}
					}
		
					this.AddDiagnostic(new Diagnostic($"Cannot find field {strPropertyName}"), null, exp);
					return null;
				}

				return objCur;
			}

			this.AddDiagnostic(new Diagnostic($"Cannot find identifier {strPath}"), null, exp);
			return null;
		}

		public Compiled.Expression Compile(Literal literal)
		{
			if (literal is BooleanLiteral)
			{
				return new Compiled.Literal() { Value = literal.Value == "true" ? true : false, InferredType = new TypeInfo(typeof(bool)) };
			}

			if (literal is IntegerLiteral)
			{
				return new Compiled.Literal() { Value = Convert.ToInt32(literal.Value), InferredType = new TypeInfo(typeof(int)) };
			}

			if (literal is StringLiteral)
			{
				if (literal is PrototypeStringLiteral)
				{
					string strPrototypeName = StringUtil.Between(literal.Value, "\"", "\"");
					return CompileRootIdentifier(strPrototypeName, literal.Info);
				}

				if (literal is AtPrefixedStringLiteral)
					return new Compiled.Literal() { Value = StringUtil.Between(literal.Value, "\"", "\""), InferredType = new TypeInfo(typeof(string)) };


				//N20250511-01 - Testing converting string literals to Prototypes at compilation. This can be avoided using the AtPrefixed literal 
				string strValue = JsonUtil.FromSafeString(StringUtil.Between(literal.Value, "\"", "\""));
				return new Compiled.Literal() { Value = StringWrapper.ToPrototype(strValue), InferredType = new TypeInfo(typeof(StringWrapper)) };

				//return new Compiled.Literal() { Value = JsonUtil.FromSafeString(StringUtil.Between(literal.Value, "\"", "\"")), InferredType = new TypeInfo(typeof(string)) };
			}

			if (literal is NullLiteral)
			{
				return new Compiled.Literal() { Value = null, InferredType = null };
			}

			if (literal is ArrayLiteral)
			{
				List<Compiled.Expression> lstExpressions = new List<Compiled.Expression>();
				foreach (Expression val in (literal as ArrayLiteral).Values)
				{
					Compiled.Expression exp = Compile(val);
					lstExpressions.Add(exp);
				}

				return new Compiled.ArrayLiteral() { Values = lstExpressions, InferredType = new TypeInfo(typeof(Ontology.Collection)) };
			}

			if (literal is DoubleLiteral)
			{
				return new Compiled.Literal() { Value = Convert.ToDouble(literal.Value), InferredType = new TypeInfo(typeof(double)) };
			}

			return new Compiled.Literal() { Value = literal.Value, InferredType = new TypeInfo(typeof(string)) };
		}

		public FunctionRuntimeInfo DeclareFunction(FunctionDefinition funcDef)
		{
			FunctionRuntimeInfo funcInfo = new FunctionRuntimeInfo();

			if (Symbols.GetGlobalScope().TryGetSymbol(funcDef.FunctionName, out object oType))
			{
				this.AddDiagnostic(new Diagnostic($"A function with the same name already exists {funcDef.FunctionName}"), funcDef, null);
				return null;
			}

			funcInfo.FunctionName = funcDef.FunctionName;
			funcInfo.Info = funcDef.Info;

			Symbols.GetGlobalScope().InsertSymbol(funcDef.FunctionName, funcInfo);

			//Insert a generic name (without parameters) so it can be found easy later
			if (funcDef.FunctionName.Contains("<"))    //generic
			{
				string strGenericName = StringUtil.LeftOfFirst(funcInfo.FunctionName, "<") + "<>";
				Symbols.GetGlobalScope().InsertSymbol(strGenericName, funcInfo);
			}


			funcInfo.Index = Symbols.GlobalStack.Add(funcInfo);
			funcInfo.Scope = new Scope(Scope.ScopeTypes.Method);
			funcInfo.Scope.Stack.Add(null);       //return location

			return CompileSignature(funcDef, funcInfo);
		}


		public Compiled.Statement Compile(FunctionDefinition funcDef)
		{
			FunctionRuntimeInfo funcInfo = Symbols.ActiveScope().GetSymbol(funcDef.FunctionName) as FunctionRuntimeInfo;

			if (null == funcInfo)
			{
				this.AddDiagnostic(new Diagnostic("Could not find function: " + funcDef.FunctionName), funcDef, null);
				return null;
			}

			Symbols.EnterScope(funcInfo.Scope);

			try
			{
				Symbols.InsertSymbol("return", funcInfo.ReturnType);
				if (funcDef.ReturnType.TypeName != "void" && !funcDef.IsAbstract)
				{					
					if (!StatementScanner.Any(funcDef, x => x is ReturnStatement))
					{
						this.AddDiagnostic(new Diagnostic("Function does not return a value"), funcDef, null);
						return null;
					}
				}


				foreach (Statement statement in funcDef.Statements)
				{
					funcInfo.Statements.Add(Compile(statement));
				}


			}
			finally
			{
				Symbols.LeaveScope();
			}

			return null;
		}

		public FunctionRuntimeInfo DeclareMethod(FunctionDefinition funcDef, Prototype prototype)
		{
			FunctionRuntimeInfo funcInfo = new FunctionRuntimeInfo();
			funcInfo.FunctionName = funcDef.FunctionName;
			funcInfo.Info = funcDef.Info;
			funcInfo.ParentPrototype = prototype;
			if (funcDef.FunctionName == "that")
				funcDef.FunctionName = prototype.PrototypeName;

			funcInfo.IsConstructor = (prototype.PrototypeName == funcDef.FunctionName);

			Symbols.ActiveScope().InsertSymbol(funcDef.FunctionName, funcInfo);

			//Insert a generic name (without parameters) so it can be found easy later
			if (funcDef.FunctionName.Contains("<"))    //generic
			{
				string strGenericName = StringUtil.LeftOfFirst(funcInfo.FunctionName, "<") + "<>";
				Symbols.ActiveScope().InsertSymbol(strGenericName, funcInfo);
			}


			funcInfo.Index = Symbols.LocalStack.Add(funcInfo);
			funcInfo.Scope = new Scope(Scope.ScopeTypes.Method);
			funcInfo.Scope.Stack.Add(null);       //return location

			PrototypeTypeInfo infoThis = Symbols.GetGlobalScope().GetSymbol(prototype.PrototypeName) as PrototypeTypeInfo;
			ValueRuntimeInfo infoThisInstance = new ValueRuntimeInfo();
			infoThisInstance.Index = funcInfo.Scope.Stack.Add(infoThisInstance);
			infoThisInstance.OriginalType = infoThis.Clone();
			infoThisInstance.Type = infoThis.Clone();
			funcInfo.Scope.InsertSymbol("this", infoThisInstance);

			return CompileSignature(funcDef, funcInfo);
		}

		private FunctionRuntimeInfo CompileSignature(FunctionDefinition funcDef, FunctionRuntimeInfo funcInfo)
		{
			if (funcDef.ReturnType.TypeName != "void")
			{
				funcInfo.ReturnType = Symbols.GetTypeInfo(funcDef.ReturnType);
				if (null == funcInfo.ReturnType)
				{
					this.AddDiagnostic(new UnknownType(funcDef.ReturnType.TypeName), funcDef, funcDef.ReturnType);
					return funcInfo;
				}
			}
			else
			{
				funcInfo.ReturnType = null;
			}
			foreach (ParameterDeclaration paramDec in funcDef.Parameters)
			{
				TypeInfo paramType = Symbols.GetTypeInfo(paramDec.Type);
				if (null == paramType)
				{
					this.AddDiagnostic(new UnknownType(paramDec.Type.TypeName), paramDec, paramDec.Type);
					return null;
				}

				if (paramType is PrototypeTypeInfo)
				{
					ParameterRuntimeInfo info = new ParameterRuntimeInfo();
					info.Type = paramType as PrototypeTypeInfo;
					info.Index = funcInfo.Scope.Stack.Add(info);
					info.OriginalType = info.Type.Clone();
					info.ParameterName = paramDec.ParameterName;

					funcInfo.Scope.InsertSymbol(paramDec.ParameterName, info);

					funcInfo.Parameters.Add(info);
				}
				else if (paramType is DotNetTypeInfo)
				{
					ParameterRuntimeInfo info = new ParameterRuntimeInfo();
					info.Type = paramType as DotNetTypeInfo;
					info.OriginalType = info.Type.Clone();
					info.Index = funcInfo.Scope.Stack.Add(info);
					info.ParameterName = paramDec.ParameterName;

					funcInfo.Scope.InsertSymbol(paramDec.ParameterName, info);

					funcInfo.Parameters.Add(info);
				}
				else if (paramType is TypeInfo)
				{
					ParameterRuntimeInfo info = new ParameterRuntimeInfo();
					info.Type = paramType as TypeInfo;
					info.OriginalType = info.Type.Clone();
					info.Index = funcInfo.Scope.Stack.Add(info);
					info.ParameterName = paramDec.ParameterName;

					funcInfo.Scope.InsertSymbol(paramDec.ParameterName, info);

					funcInfo.Parameters.Add(info);
				}

				else
					throw new NotImplementedException();

			}

			return funcInfo;
		}

		public List<Compiled.Statement> CompileFunctionAnnotations(FunctionDefinition funcDef)
		{
			List<Compiled.Statement> lstStatements = new List<Compiled.Statement>();

			FunctionRuntimeInfo funcInfo = Symbols.ActiveScope().GetSymbol(funcDef.FunctionName) as FunctionRuntimeInfo;

			if (null == funcInfo)
			{
				this.AddDiagnostic(new Diagnostic("Could not find method: " + funcDef.FunctionName), funcDef, null);
				return null;
			}


			foreach (AnnotationExpression annotation in funcDef.Annotations)
			{
				MethodEvaluation method = annotation.GetAnnotationMethodEvaluation();
				if (!annotation.IsExpanded)
				{
					method.Parameters.Insert(0, new Identifier(funcDef.FunctionName));
					annotation.IsExpanded = true;
				}

				Compiled.Expression expression = Compile(annotation);
				if (!(expression is Compiled.FunctionEvaluation))
					throw new Exception("Unexpected");

				Compiled.FunctionEvaluation functionEvaluation = expression as Compiled.FunctionEvaluation;

				lstStatements.Add(new Compiled.PrototypeAnnotation { AnnotationFunction = functionEvaluation, Info = annotation.Info });
			}


			return lstStatements;
		}

		public Compiled.Expression Compile(MethodEvaluation methodEval)
		{
			if (methodEval.MethodName.Contains("."))
			{
				string strIdentifier = StringUtil.LeftOfLast(methodEval.MethodName, ".");
				Compiled.Expression expression = Compile(strIdentifier, methodEval);
				if (null == expression)
				{
					this.AddDiagnostic(new Diagnostic($"Could not find method {methodEval.MethodName}"), null, methodEval);
					return null;
				}
				return CompileMethodEvaluationInternal(methodEval, expression);
			}
			else
			{
				//Simple case, no multi-part identifier
				object obj = Symbols.GetSymbol(methodEval.MethodName);

				if (null == obj)
				{
					if (methodEval.MethodName == "nameof")
					{
						string strValue = methodEval.Parameters[0].ToString();
						if (strValue == "that")
						{
							PrototypeTypeInfo prototypeTypeInfo = Symbols.GetSymbol(strValue) as PrototypeTypeInfo;
							strValue = prototypeTypeInfo.Prototype.PrototypeName;
						}

						return new Compiled.Literal()
						{
							Value = strValue,
							InferredType = new TypeInfo(typeof(string)),
							Info = methodEval.Parameters[0].Info
						};
					}
					else
					{
						this.AddDiagnostic(new UnknownFunction(methodEval.MethodName), null, methodEval);
						return null;
					}
				}

				List<Compiled.Expression> lstParameters = new List<Compiled.Expression>();

				for (int i = 0; i < methodEval.Parameters.Count; i++)
				{
					Compiled.Expression exp = Compile(methodEval.Parameters[i]);
					if (null == exp)
					{
						this.AddDiagnostic(new Diagnostic("Unknown Parameter"), null, methodEval.Parameters[i]);
						return null;
					}

					exp.Info = methodEval.Parameters[i].Info;

					lstParameters.Add(exp);
				}

				FunctionEvaluation info = new FunctionEvaluation();
				info.Parameters = lstParameters;

				FunctionRuntimeInfo functionRuntimeInfo = (obj as FunctionRuntimeInfo);

				//TODO: allow for function overloading here
				if (info.Parameters.Count != functionRuntimeInfo.Parameters.Count)
				{
					this.AddDiagnostic(new Diagnostic("Incorrect number of parameters"), null, methodEval);
					return null;
				}

				for (int i = 0; i < functionRuntimeInfo.Parameters.Count; i++)
				{
					if (i >= info.Parameters.Count)
					{
						this.AddDiagnostic(new Diagnostic("Not enough parameter supplied"), null, methodEval);
						return null;
					}
					ParameterRuntimeInfo destParam = functionRuntimeInfo.Parameters[i];
					Compiled.Expression exp = info.Parameters[i];

					if (!SimpleInterpretter.IsAssignableFrom(exp.InferredType, destParam.Type))
					{
						this.AddDiagnostic(new CannotConvert(exp.InferredType.ToString(), destParam.Type.ToString()), null, methodEval.Parameters[i]);
						return null;
					}

				}

				info.Function = functionRuntimeInfo;
				info.InferredType = functionRuntimeInfo.ReturnType;

				if (null != functionRuntimeInfo.ParentPrototype)
				{
					PrototypeTypeInfo parentTypeInfo = Symbols.GetGlobalScope().GetSymbol(functionRuntimeInfo.ParentPrototype.PrototypeName) as PrototypeTypeInfo;
					info.Object = new GetLocalStack() { Index = 1, InferredType = parentTypeInfo, Info = functionRuntimeInfo.Info };
				}

				return info;
			}
		}

		private Compiled.Expression CompileMethodEvaluationInternal(MethodEvaluation methodEval, Compiled.Expression expression)
		{
			return MethodCompiler.CompileMethodEvaluationInternal(methodEval, expression, this);
		}
		public Compiled.ForEachStatement Compile(ForEachStatement statement)
		{
			Compiled.ForEachStatement compiled = new Compiled.ForEachStatement();
			compiled.Info = statement.Info;
			compiled.Scope = new Scope(Scope.ScopeTypes.Block);

			TypeInfo infoType = Symbols.GetGlobalScope().GetSymbol(statement.Type.TypeName) as TypeInfo;

			VariableRuntimeInfo variableRuntimeInfo = new VariableRuntimeInfo();
			variableRuntimeInfo.Type = infoType;
			variableRuntimeInfo.Index = compiled.Scope.Stack.Add(variableRuntimeInfo);
			compiled.Scope.InsertSymbol(statement.IteratorName, variableRuntimeInfo);

			Symbols.EnterScope(compiled.Scope);

			compiled.Iterator = variableRuntimeInfo;

			try
			{
				compiled.Expression = Compile(statement.Expression);
				compiled.Statements = Compile(statement.Statements);
			}
			finally
			{
				Symbols.LeaveScope();
			}

			return compiled;
		}

		public Compiled.ReturnStatement Compile(ReturnStatement statement)
		{
			TypeInfo info = null;

			if (!Symbols.TryGetSymbol<TypeInfo>("return", out info))
			{
				this.AddDiagnostic(new Diagnostic("return statement not valid in this context"), statement, null);
				return null;
			}

			Compiled.ReturnStatement compiledReturn = new Compiled.ReturnStatement();
			compiledReturn.Info = statement.Info;

			if (statement.Expression == null)
			{
				if (info == null)
				{
					this.AddDiagnostic(new Diagnostic("return type should be void"), statement, null);
					return null;
				}
			}
			else
			{
				compiledReturn.Expression = Compile(statement.Expression);

				if (null == compiledReturn.Expression)
					return compiledReturn;


				if (!SimpleInterpretter.IsAssignableFrom(compiledReturn.Expression.InferredType, info))
				{
					this.AddDiagnostic(new CannotConvert(compiledReturn.Expression.InferredType.ToString(), info.ToString()), statement, null);
				}
			}

			return compiledReturn;
		}

		

		public void Compile(ReferenceStatement statement)
		{
			System.Reflection.Assembly assembly = null;

			try
			{
				assembly = System.Reflection.Assembly.Load(statement.AssemblyName);
			}
			catch
			{
				assembly = System.Reflection.Assembly.LoadFrom(statement.AssemblyName);
			}

			if (null == assembly)
			{
				this.AddDiagnostic(new Diagnostic("Could not load assembly " + statement.AssemblyName), statement, null);
				return;
			}

			if (!References.ContainsKey(statement.Reference))
				References.Add(statement.Reference, assembly);
		}

		public void Compile(ImportStatement statement)
		{
			System.Reflection.Assembly assembly;
			
			if (!References.TryGetValue(statement.Reference, out object obj))
			{
				this.AddDiagnostic(new Diagnostic("Assembly not referenced: " + statement.Reference), statement, null);
				return;
			}

			assembly = obj as System.Reflection.Assembly;

			if (null == assembly)
			{
				this.AddDiagnostic(new Diagnostic("Assembly not referenced: " + statement.Reference), statement, null);
				return;
			}

			System.Type type = assembly.GetType(statement.Type);

			if (null == type)
			{
				this.AddDiagnostic(new Diagnostic($"Type {statement.Type} not found in Assembly: {statement.Reference}"), statement, null);
				return;
			}

			DotNetTypeInfo info = new DotNetTypeInfo(type);
			info.Index = Symbols.GlobalStack.Add(info);

			Symbols.InsertSymbol(statement.Import, info);
		}


		public Compiled.IfStatement Compile(IfStatement statement)
		{
			Compiled.IfStatement compiled = new Compiled.IfStatement();
			compiled.Info = statement.Info;

			compiled.Condition = Compile(statement.Condition);
			compiled.TrueBody = Compile(statement.TrueBody);

			if (statement.ElseBody != null)
				compiled.ElseBody = Compile(statement.ElseBody);

			if (statement.ElseIfConditions.Count > 0)
			{
				foreach (Expression elseIf in statement.ElseIfConditions)
				{
					compiled.ElseIfConditions.Add(Compile(elseIf));
				}
				foreach (CodeBlock codeBlock in statement.ElseIfBodies)
				{
					compiled.ElseIfBodies.Add(Compile(codeBlock));
				}
			}
				


			return compiled;
		}

		public Compiled.CodeBlock Compile(CodeBlock statements)
		{
			Compiled.CodeBlock compiled = new Compiled.CodeBlock();
			compiled.Scope = new Scope(Scope.ScopeTypes.Block);

			try
			{
				Symbols.EnterScope(compiled.Scope);

				foreach (Statement statement in statements)
				{
					compiled.Add(Compile(statement));
				}
			}
			finally
			{
				Symbols.LeaveScope();
			}

			return compiled;
		}

		public Compiled.CodeBlockStatement Compile(CodeBlockStatement statements)
		{
			Compiled.CodeBlock compiled = new Compiled.CodeBlock();
			compiled.Scope = new Scope(Scope.ScopeTypes.Block);

			try
			{
				Symbols.EnterScope(compiled.Scope);

				foreach (Statement statement in statements.Statements)
				{
					compiled.Add(Compile(statement));
				}
			}
			finally
			{
				Symbols.LeaveScope();
			}

			return  new Compiled.CodeBlockStatement(compiled);
		}
	}
}


