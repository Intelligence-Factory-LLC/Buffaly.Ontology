using Microsoft.Extensions.Configuration;
using Buffaly.NLU;
using ProtoScript.Extensions;

namespace Ontology.Tests
{


	[TestClass]
	public sealed class BasicInterpretter_Tests_2
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
		}


		public static string GetProjectHello()
		{
			return @"C:\dev\ai\Ontology8\Buffaly.Ontology.Portal\wwwroot\projects\hello.pts";
		}

		public static string GetProjectSimpsons()
		{
			return @"C:\dev\ai\Ontology8\Buffaly.Ontology.Portal\wwwroot\projects\Simpsons.pts";
		}


		public static ProtoScriptTagger GetProject(string strProject)
		{
			UnderstandUtil.TaggingSettings settings = new UnderstandUtil.TaggingSettings();
			settings.Project = strProject;
			settings.EnableDatabase = false;
			settings.MaxIterations = 100;
			return UnderstandUtil.GetAndInitializeProtoScriptTagger(settings);
		}




		[TestMethod]
		public void Test_InterpretHelloWorld()
		{
			ProtoScriptWorkbench.TaggingSettings settings = new ProtoScriptWorkbench.TaggingSettings();
			settings.Project = GetProjectHello();

			var res = ProtoScriptWorkbench.InterpretImmediate(settings.Project, "main()", settings);

			Logs.DebugLog.WriteEvent("Result", res.Result);

			Prototype prototype = res.ResultPrototype;

			string strPrototype = PrototypeLogging.ToFriendlyString2(prototype);
			Logs.DebugLog.WriteEvent("Result from Prototype", strPrototype);
		}


		[TestMethod]
		public void Test_InterpretSimpsons()
		{
			ProtoScriptWorkbench.TaggingSettings settings = new ProtoScriptWorkbench.TaggingSettings();
			settings.Project = GetProjectSimpsons();

			var res = ProtoScriptWorkbench.InterpretImmediate(settings.Project, "SimpsonsOntology.Bart", settings);

			Logs.DebugLog.WriteEvent("Result", res.Result);

			Prototype prototype = res.ResultPrototype;
			
			PrototypeLogging.IncludeTypeOfs = true;
			
			string strPrototype = PrototypeLogging.ToFriendlyString2(prototype);
			Logs.DebugLog.WriteEvent("Result from Prototype", strPrototype);
		}


	}
}