using Microsoft.Extensions.Configuration;
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
	public sealed class MedOntTests_1
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

	
		private static string GetPortalProject(string file)
		{
			//string path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Buffaly.Ontology.Portal", "wwwroot", "projects", file);

			string path = Path.Combine("C:\\dev\\FairPath\\FairPath.Portal.Admin\\wwwroot\\ProtoScript\\", file);
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


		[TestMethod]
		[TestCategory("Integration")]
		[TestProperty("Category", "Integration")]
		public void Test_GetSpecialtyCapability()
		{
			ProtoScriptTagger tagger = GetProjectSmallTagger();

			string strCode = "I10";

			Prototype ? protoCode = tagger.Interpretter.RunMethodAsPrototype(null, "GetICDCodeByCode", new List<object> { strCode });
			if (null != protoCode)
			{
				Prototype ? protoSpecialtyCapabilities = tagger.Interpretter.RunMethodAsPrototype("SpecialityCapabilities", "GetByICD10", new List<object> { protoCode });
				if (null != protoSpecialtyCapabilities)
				{
					SpecialityCapabilities capabilities = SpecialityCapabilities.FromJson(protoSpecialtyCapabilities.ToFriendlyJsonObject());

				}
			}


		}


	}

	
	public class SpecialityCapabilities
	{
		public bool AllowRTM = false;
		public bool AllowCCM = false;
		public bool AllowRPM = false;

		public int AllowCCMCount = 0;
		public int AllowRTMCount = 0;
		public int AllowRPMCount = 0;


		public SpecialityCapabilities()
		{
		}

		public static SpecialityCapabilities FromJson(JsonObject jsonObject)
		{
			var capabilities = new SpecialityCapabilities();

			try
			{
				// Extract boolean fields from nested structure  
				if (jsonObject.GetJsonObjectOrDefault("SpecialityCapabilities.Field.AllowRTM").GetStringOrNull("PrototypeName").Contains("True"))
				{
					capabilities.AllowRTM = true;
					capabilities.AllowRTMCount = 1;
				}

				if (jsonObject.GetJsonObjectOrDefault("SpecialityCapabilities.Field.AllowCCM").GetStringOrNull("PrototypeName").Contains("True"))
				{
					capabilities.AllowCCM = true;
					capabilities.AllowCCMCount = 1;
				}

				if (jsonObject.GetJsonObjectOrDefault("SpecialityCapabilities.Field.AllowRPM").GetStringOrNull("PrototypeName").Contains("True"))
				{
					capabilities.AllowRPM = true;
					capabilities.AllowRPMCount = 1;
				}

			}
			catch
			{
				// Return default instance on parsing failure  
			}

			return capabilities;
		}

		public static SpecialityCapabilities operator |(SpecialityCapabilities left, SpecialityCapabilities right)
		{
			return new SpecialityCapabilities
			{
				AllowRTM = left.AllowRTM || right.AllowRTM,
				AllowCCM = left.AllowCCM || right.AllowCCM,
				AllowRPM = left.AllowRPM || right.AllowRPM,

				// Combine counts if needed
				AllowRTMCount = left.AllowRTMCount + right.AllowRTMCount,
				AllowCCMCount = left.AllowCCMCount + right.AllowCCMCount,
				AllowRPMCount = left.AllowRPMCount + right.AllowRPMCount
			};
		}
	}
}