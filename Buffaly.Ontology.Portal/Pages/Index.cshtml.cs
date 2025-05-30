using BasicUtilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RooTrax.Common;

namespace Buffaly.Admin.Portal.Pages
{
	public class IndexModel : PageModel
	{
		private readonly ILogger<IndexModel> _logger;

		public IndexModel(ILogger<IndexModel> logger)
		{
			_logger = logger;
		}

		public IActionResult OnGet()
		{
			return Redirect("/protoscript");
		}

		public string? SidebarMenu
		{
			get
			{
				return GetSidebarMenu(RooTraxState.kScriptControl);
			}
		}

		public string GetSidebarMenu(kScript3.kScriptControl oHandler)
		{
			return oHandler.EvaluateFile(FileUtil.BuildPath(oHandler.GetRootDir(), "Administrator\\LeftMenu.ks.html"));
		}
	}
}