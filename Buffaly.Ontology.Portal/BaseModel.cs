using BasicUtilities;
using RooTrax.Common;

namespace Portal.Pages
{
    /// <summary>
    /// Shared base class for Razor Page models across Buffaly portals.
    /// Provides helper methods for accessing request parameters.
    /// </summary>
    public class BaseModel : Buffaly.Common.BaseModel
	{
		public static string GetSidebarMenu(kScript3.kScriptControl oHandler)
		{
			string rootDir = oHandler.GetRootDir() ?? throw new Exception("kScript Root Directory not set");
			return oHandler.EvaluateFile(FileUtil.BuildPath(rootDir, "MasterPage\\Administrator.LeftMenu.ks.html"));
		}

		public static string GetChatSidebar(kScript3.kScriptControl oHandler)
		{
			string rootDir = oHandler.GetRootDir() ?? throw new Exception("kScript Root Directory not set");
			return oHandler.EvaluateFile(FileUtil.BuildPath(rootDir, "MasterPage\\ChatSidebar.ks.html"));
		}

		public static string GetShortcutsMenu(kScript3.kScriptControl oHandler)
		{
			string rootDir = oHandler.GetRootDir() ?? throw new Exception("kScript Root Directory not set");
			return oHandler.EvaluateFile(FileUtil.BuildPath(rootDir, "MasterPage\\ShortcutsMenu.ks.html"));
		}

		public static string GetUserMenu(kScript3.kScriptControl oHandler)
		{
			string rootDir = oHandler.GetRootDir() ?? throw new Exception("kScript Root Directory not set");
			return oHandler.EvaluateFile(FileUtil.BuildPath(rootDir, "MasterPage\\UserMenu.ks.html"));
		}
		public static string GetFileMenu(kScript3.kScriptControl oHandler)
		{
			string rootDir = oHandler.GetRootDir() ?? throw new Exception("kScript Root Directory not set");
			return oHandler.EvaluateFile(FileUtil.BuildPath(rootDir, "MasterPage\\Administrator.FileMenu.ks.html"));
		}


		public string ChatSidebar
		{
			get
			{
				return GetChatSidebar(RooTraxState.kScriptControl);
			}
		}

		public string ShortcutsMenu
		{
			get
			{
				return GetShortcutsMenu(RooTraxState.kScriptControl);
			}
		}

		public string UserMenu
		{
			get
			{
				return GetUserMenu(RooTraxState.kScriptControl);
			}
		}

		public string SidebarMenu
		{
			get
			{
				return GetSidebarMenu(RooTraxState.kScriptControl);
			}
		}

		public string FileMenu 
		{
			get
			{
				return GetFileMenu(RooTraxState.kScriptControl);
			}
		}
	}
}

