using BasicUtilities;
using Buffaly.Common;
using Microsoft.AspNetCore.HttpOverrides;
using Ontology;
using RooTrax.Common;
using WebAppUtilities;

public class Program
{
	public static async Task Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		var configurationBuilder = new ConfigurationBuilder();
		configurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
		var config = configurationBuilder.Build();

		BasicUtilities.Settings.SetAppSettings(config);
		ConfigureLogs(config);
		SetConnectionString(config);

		MiddlewareDebugOptions.Enabled = true;

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

		ConfigureRooTraxState(config, app.Environment.WebRootPath);

		// Make ProtoScriptWorkbench aware of wwwroot for relative project paths
		ProtoScript.Extensions.ProtoScriptWorkbench.SetWebRoot(app.Environment.WebRootPath);

		// Configure the HTTP request pipeline.
		if (!app.Environment.IsDevelopment())
		{

			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			app.UseHsts();
		}

		app.UseExceptionHandler("/Error");

		try
		{
			Buffaly.Common.BaseUserState.Configure(app.Services.GetRequiredService<IHttpContextAccessor>());

			RooTraxStateSettings? rooTraxStateSettings = config.GetSection("RooTraxStateSettings").Get<RooTraxStateSettings>();
			if (null == rooTraxStateSettings)
				throw new Exception("Could not load configuration: RooTraxStateSettings");


			JsonWsOptions jsonWsOptions = config.GetSection("JsonWs").Get<JsonWsOptions>() ?? new JsonWsOptions();

			JsonSerializers.RegisterSerializer(typeof(Prototype), new PrototypeJsonSerializer());

			app.UseHttpsRedirection();

			app.UseStaticFiles();

			app.UseRouting();

			app.MapRazorPages();

			//For retrieving the IP Address
			app.UseForwardedHeaders(new ForwardedHeadersOptions
			{
				ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
			});


			app.UseSession();

			MapJsonWs(app, jsonWsOptions);

			// Register the RewriteOptionsService
			app.MapGet("/protoscript", ctx =>
			{
				ctx.Response.Redirect(
					"/k?Output=ProtoScriptWorkbench\\ProtoScript.Workbench.ks.html" +
					"&Class=SimplePage&Handler=Buffaly.SemanticDB.UI",
					permanent: false);
				return Task.CompletedTask;
			});


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
					MapAPIs(endpoints, jsonWsOptions);

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

	private static void MapJsonWs(WebApplication app, JsonWsOptions jsonWsOptions)
	{
		JsonWsHandlerService.RegisterJsonWs(app, jsonWsOptions, x => { return true; });
	}

	private static void MapAPIs(IEndpointRouteBuilder endpoints, JsonWsOptions jsonWsOptions)
	{
		JsonWsHandlerService.RegisterApis(endpoints, jsonWsOptions);
	}


	private static void ConfigureRooTraxState(IConfigurationRoot config, string strWebRoot)
	{
		RooTraxStateSettings? rooTraxStateSettings = config.GetSection("RooTraxStateSettings").Get<RooTraxStateSettings>();
		if (null == rooTraxStateSettings)
			throw new Exception("Could not load configuration: RooTraxStateSettings");

		if (!StringUtil.IsEmpty(rooTraxStateSettings.RooTraxConnect))
			rooTraxStateSettings.RooTraxConnect = config.GetConnectionStringOrFail(rooTraxStateSettings.RooTraxConnect);

		string relative = rooTraxStateSettings.kScriptRootDir?.TrimStart('\\', '/') ?? string.Empty;
		rooTraxStateSettings.kScriptRootDir = Path.Combine(strWebRoot, relative);

		RooTrax.Common.RooTraxState.Configure(rooTraxStateSettings);
		Buffaly.SemanticDB.UI.RooTraxState.Configure(rooTraxStateSettings);
	}

	private static void ConfigureLogs(IConfigurationRoot config)
	{
		Logs.LogSettings? logSettings = config.GetSection("LogSettings").Get<Logs.LogSettings>();
		if (null == logSettings)
			throw new Exception("Could not load configuration: LogSettings");

		Logs.Config(logSettings);
	}

	private static void SetConnectionString(IConfigurationRoot config)
	{
		Buffaly.SemanticDB.Data.DataAccess.SetConnectionString(config.GetConnectionStringOrFail("buffaly_semanticdb.readwrite"));
	}
}