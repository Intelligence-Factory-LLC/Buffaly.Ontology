﻿using Microsoft.Extensions.Configuration;
using Buffaly.NLU;
using Buffaly.NLU.Tagger.Nodes;
using Ontology.Simulation;
using ProtoScript.Extensions;
using System;
using System.IO;
using BasicUtilities;

namespace Ontology.Tests
{


	[TestClass]
	public sealed class BasicInterpretter_Tests
	{
		[TestInitialize]
		public void TestInit()
		{
			var builder = new ConfigurationBuilder();
			builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
			var config = builder.Build();

			Logs.LogSettings? logSettings = config.GetSection("LogSettings").Get<Logs.LogSettings>();
			if (null == logSettings)
				throw new Exception("Could not load configuration: LogSettings");

			Logs.Config(logSettings);


			BasicUtilities.Settings.SetAppSettings(config);

			// This method is called before each test method.
			Initializer.Initialize();

			TemporaryPrototypes.Cache.InsertLogFrequency = 10000;
		}

		public static ProtoScriptTagger GetProjectTagger(bool bAllowPrecompiled = false)
		{
			UnderstandUtil.TaggingSettings settings = new UnderstandUtil.TaggingSettings();
			settings.Project = GetProject();
			settings.MaxIterations = 100;
			settings.AllowPrecompiled = bAllowPrecompiled;

			return UnderstandUtil.GetAndInitializeProtoScriptTagger(settings);
		}

		private static string GetPortalProject(string file)
		{
			//string path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Buffaly.Ontology.Portal", "wwwroot", "projects", file);

			string path = Path.Combine("C:\\dev\\ai\\ontology\\ProtoScript.Tests\\Medical", file);
			return Path.GetFullPath(path);
		}
		public static string GetProjectSmall()
		{
			return GetPortalProject("ProjectSmall8.pts");
		}

		public static ProtoScriptTagger GetProjectSmallTagger()
		{
			UnderstandUtil.TaggingSettings settings = new UnderstandUtil.TaggingSettings();
			settings.Project = GetProjectSmall();
			settings.EnableDatabase = false;
			settings.MaxIterations = 100;
			return UnderstandUtil.GetAndInitializeProtoScriptTagger(settings);
		}

		public static string GetProject()
		{
			return GetPortalProject("Project.pts");
		}

		public static ProtoScriptTagger GetSnoMedOnlyProjectTagger()
		{
			UnderstandUtil.TaggingSettings settings = new UnderstandUtil.TaggingSettings();

			settings.Project = GetPortalProject("ProjectSnoMedOnly.pts");

			return UnderstandUtil.GetAndInitializeProtoScriptTagger(settings);
		}

		[TestMethod]
		[TestCategory("Integration")]
		[TestProperty("Category", "Integration")]
		public void Test_TagImmediate()
		{
			ProtoScriptTagger tagger = GetProjectSmallTagger();
			tagger.TagIteratively = false;
			OntologicalCategorizationNode.UseQuantumPrototypes = false;

			Logs.DebugLog.CreateTimer("Tagging Buffalo");
			Prototype prototype1 = UnderstandUtil.Understand3("cardiac dilatation", tagger);
			Logs.DebugLog.WriteTimer("Tagging Buffalo");

			prototype1 = BagOfFeatures.CategorizeAndUnderstand(prototype1);
		}

		[TestMethod]
		[TestCategory("Integration")]
		[TestProperty("Category", "Integration")]
		public void Test_BenchmarkSpeed()
		{
			ProtoScriptWorkbench.TaggingSettings settings = new ProtoScriptWorkbench.TaggingSettings();
			settings.Project = GetProjectSmall();
			settings.Debug = false;
			settings.Resume = true;
			settings.AllowPrecompiled = false;
			settings.EnableDatabase = false;

			Logs.DebugLog.CreateTimer("GetDescendantsAsDistance");
			var res = ProtoScriptWorkbench.InterpretImmediate(settings.Project, "GetDescendantsAsDistance(ICD10CM.Section_E08_E13)", settings);
			Logs.DebugLog.WriteTimer("GetDescendantsAsDistance");
		}


		[TestMethod]
		[TestCategory("Integration")]
		[TestProperty("Category", "Integration")]
		public void Test_InterpretImmediate()
		{
			ProtoScriptWorkbench.TaggingSettings settings = new ProtoScriptWorkbench.TaggingSettings();
			settings.Project = GetProjectSmall();
			settings.Debug = false;
			settings.Resume = false;
			settings.AllowPrecompiled = false;
			settings.EnableDatabase = false;

			var res = ProtoScriptWorkbench.InterpretImmediate(settings.Project, "SpecialityCapabilities.GetByICD10(ICD10CM.D51_9)", settings);
			var res2 = ProtoScriptWorkbench.InterpretImmediate(settings.Project, "ICD10CM.A00)", settings);

		}

	}
}