using Microsoft.AspNetCore.Mvc;
namespace Portal.Pages
{

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
	public class ErrorModel : Buffaly.Common.ErrorModel
	{
		public ErrorModel(
			IConfiguration configuration,
			IWebHostEnvironment env) : base(configuration, env)
		{

		}
	}
}