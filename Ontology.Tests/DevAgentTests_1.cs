using BasicUtilities;
using Buffaly.Common;
using Buffaly.NLU;
using Buffaly.NLU.Tagger.Nodes;
using Microsoft.Extensions.Configuration;
using Ontology.Agents;
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
			Buffaly.Data.DataAccess.SetConnectionString(config.GetConnectionStringOrFail("buffaly.readwrite"));
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




		[TestMethod]
		[TestCategory("Integration")]
		[TestProperty("Category", "Integration")]
		public async Task Test_GenerateSyntheticProgram()
		{
			JsonObject jsonResult = await DevAgent.GenerateProgram("test_session", @"
To Replace Grid Count

This method is going to take a parameter:

	File - Full path to the *.ks.html file

It will look within the file for the following pattern:

```kscript
		(Grid.Count (GetGridCount  (Search)))
```

and it will replace it the `Count` property with `RowCount` because the former is reserved 
.NET property, taking care to fix the surrounding code as well:

```kscript

		(declare Grid {Grid: '', RowCount: 0})		
		...
		(Grid.RowCount (GetGridCount  (Search)))
		(return (Grid.ToJSON))
```

The overall plan: 

	Load the file into a string 
	Construct a prompt to look for this pattern
	Append a diff generating prompt
	Apply the prompt
	Get the diffs
	Apply the diffs
	Verify the file or reject 

", new JsonObject());

			string ? strProgram = jsonResult.GetStringOrNull("Program");


		}



	}
}