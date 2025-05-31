using BasicUtilities;
using Buffaly.Common;
using Microsoft.AspNetCore.HttpOverrides;
using RooTrax.Common;
using System.Reflection;
using WebAppUtilities;
using ProtoScript.Extensions;

public class Program
{
	public static async Task Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.
		builder.Services.AddHttpContextAccessor();
		builder.Services.AddRazorPages();

		builder.Services.AddSession(options =>
		{
			options.IdleTimeout = TimeSpan.FromMinutes(360);
			options.Cookie.HttpOnly = true;
			options.Cookie.IsEssential = true;
		});

		// Add CORS services
		builder.Services.AddCors();

		builder.Services.AddDistributedMemoryCache();

                var app = builder.Build();
                // Make ProtoScriptWorkbench aware of wwwroot for relative project paths
                ProtoScript.Extensions.ProtoScriptWorkbench.SetWebRoot(app.Environment.WebRootPath);

		// Configure the HTTP request pipeline.
		if (!app.Environment.IsDevelopment())
		{

			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			app.UseHsts();
		}

		app.UseExceptionHandler("/Error");

		var configurationBuilder = new ConfigurationBuilder();
		configurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
		var config = configurationBuilder.Build();

		Logs.LogSettings? logSettings = config.GetSection("LogSettings").Get<Logs.LogSettings>();
		if (null == logSettings)
			throw new Exception("Could not load configuration: LogSettings");

		Logs.Config(logSettings);

		try
		{

			RooTraxStateSettings? rooTraxStateSettings = config.GetSection("RooTraxStateSettings").Get<RooTraxStateSettings>();
			if (null == rooTraxStateSettings)
				throw new Exception("Could not load configuration: RooTraxStateSettings");

			Buffaly.SemanticDB.UI.RooTraxState.Configure(rooTraxStateSettings);

			JsonWsOptions jsonWsOptions = config.GetSection("JsonWs").Get<JsonWsOptions>() ?? new JsonWsOptions();


			app.UseHttpsRedirection();

			app.UseStaticFiles();

			app.UseRouting();

			app.MapRazorPages();

			//For retrieving the IP Address
			app.UseForwardedHeaders(new ForwardedHeadersOptions
			{
				ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
			});

			string? strJsonWsRoot = config.GetSection("JsonWs:JsonWsRoot").Value;

			if (null == strJsonWsRoot)
				throw new Exception("appSettings:JsonWs:JsonWsRoot is null");

			app.UseSession();

			JsonWsHandlerService.RegisterJsonWs(app, strJsonWsRoot, jsonWsOptions);

			// Register the RewriteOptionsService
			app.MapGet("/protoscript", ctx =>
			{
				ctx.Response.Redirect(
					"/k?Output=ProtoScriptWorkbench\\ProtoScript.Workbench.ks.html" +
					"&Class=SimplePage&Handler=Buffaly.SemanticDB.UI",
					permanent: false);
				return Task.CompletedTask;
			});

			IConfigurationSection configurationSection =
				config.GetSection("AppSettings");

			Settings.SetAppSettings(config);

			app.MapWhen(context => context.Request.Path.StartsWithSegments("/api"), appBuilder =>
			{
				appBuilder.UseRouting();
				// Apply CORS policy for any origin on the /api/ paths
				appBuilder.UseCors(policy =>
				{
					policy.AllowAnyOrigin()
						  .AllowAnyMethod()
						  .AllowAnyHeader();
				});

				appBuilder.UseEndpoints(endpoints =>
				{
					// Map all /api routes
					MapAPIs(endpoints, jsonWsOptions, strJsonWsRoot);

				});

			});

			app.MapRazorPages();
			app.Run();
		}
		catch (Exception err)
		{
			Logs.LogError(err);
		}

	}
	private static void MapAPIs(IEndpointRouteBuilder endpoints, JsonWsOptions jsonWsOptions, string strJsonWsRoot)
	{
		foreach (string strFile in Directory.GetFiles(strJsonWsRoot, "*.ashx"))
		{
			string strContents = FileUtil.ReadFile(strFile);

			JsonObject jsonObject = new JsonObject(strContents);

			string strAssembly = jsonObject.GetStringOrNull("Assembly") ?? throw new Exception("Assembly not specified in : " + strFile);
			string strType = jsonObject.GetStringOrNull("Type") ?? throw new Exception("Type not specified in : " + strFile);

			Assembly assembly = AssemblyManager.Load(strAssembly);
			Type type = assembly.GetType(strType) ?? throw new Exception("Could not get Type: " + strType + " from assembly " + strAssembly);

			// Use ActivatorUtilities to create the instance with DI support
			object? obj = ActivatorUtilities.CreateInstance(endpoints.ServiceProvider, type);
			if (obj == null)
				throw new Exception("Could not create instance of " + strType);

			JsonWs jsonWs = (JsonWs)obj;
			jsonWs.SetOptions(jsonWsOptions);

			// Map API routes
			foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
			{
				string strUrl = "/api/" + type.Namespace.ToString().ToLower() + "/" + GetUrlSafeName(type.Name) + "/" + GetUrlSafeName(method.Name);
				// Map the API route with authorization
				endpoints.Map(strUrl, async (HttpContext context) =>
				{
					await jsonWs.ProcessAPIRequestAsync(context, method);
				}); // Require authorization for API routes
			}
		}
	}

	static private string GetUrlSafeName(string strName)
	{
		return string.Join("-", StringUtil.SplitUppercaseWords(strName).Select(x => x.ToLower()).ToArray());
	}
}