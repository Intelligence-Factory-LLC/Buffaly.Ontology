using BasicUtilities;
using Buffaly.Common;
using Buffaly.NLU;
using Buffaly.NLU.Tagger.Nodes;
using Microsoft.Extensions.Configuration;
using Ontology.Simulation;
using ProtoScript.Extensions;
using System;
using System.IO;

namespace Ontology.Tests
{


	[TestClass]
	public sealed class DevAgentTests_1
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

			Buffaly.SemanticDB.Data.DataAccess.SetConnectionString(config.GetConnectionStringOrFail("buffaly_semanticdb.readwrite"));
		}

	
		private static string GetPortalProject(string file)
		{
			//string path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Buffaly.Ontology.Portal", "wwwroot", "projects", file);

			string path = Path.Combine("C:\\dev\\ai\\Ontology8\\Buffaly.Ontology.Portal\\wwwroot\\projects\\DevAgent\\", file);
			return Path.GetFullPath(path);
		}
		public static string GetProjectSmall()
		{
			return GetPortalProject("Project.pts");
		}

		public static ProtoScriptTagger GetProjectSmallTagger()
		{
			UnderstandUtil.TaggingSettings settings = new UnderstandUtil.TaggingSettings();
			settings.Project = GetProjectSmall();
			settings.EnableDatabase = false;
			settings.MaxIterations = 100;
			return UnderstandUtil.GetAndInitializeProtoScriptTagger(settings);
		}


		[TestMethod]
		[TestCategory("Integration")]
		[TestProperty("Category", "Integration")]
		public void Test_InterpretImmediate()
		{
			try
			{
				ProtoScriptWorkbench.LoadProject(GetProjectSmall());
			}
			catch (Exception ex)
			{
				throw;
			}

			ProtoScriptWorkbench.TaggingSettings settings = new ProtoScriptWorkbench.TaggingSettings();
			settings.Project = GetProjectSmall();
			settings.Debug = false;
			settings.Resume = false;
			settings.AllowPrecompiled = false;
			settings.EnableDatabase = false;

			var res = ProtoScriptWorkbench.InterpretImmediate(settings.Project, "Kangaroo", settings);

		}


	}
}