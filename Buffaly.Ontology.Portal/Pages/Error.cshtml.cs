using BasicUtilities;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using Buffaly.Common;
namespace Buffaly.Admin.Portal.Pages
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