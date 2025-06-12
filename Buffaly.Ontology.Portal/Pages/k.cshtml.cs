using BasicUtilities;
using Buffaly.Common;
using Buffaly.Common.Models;
using kScript3;
using Microsoft.AspNetCore.Authorization;
using System.Reflection.Metadata;

namespace Buffaly.Ontology.Portal.Pages
{
    public class kModel : KScriptPageModel<BaseUserState>
    {
        public kModel() : base()
        {
			this.m_bEnableContent = false;
        }

        protected override BaseUserState UserState => BaseUserState.Current;

        protected override string? GetSidebarMenu(kScriptControl oHandler)
        {
            return oHandler.EvaluateFile(FileUtil.BuildPath(oHandler.GetRootDir(), "Administrator\\LeftMenu.ks.html"));
        }

		protected override string GetFileMenu(kScriptControl handler)
		{
			return handler.EvaluateFile(FileUtil.BuildPath(handler.GetRootDir(), "Administrator\\FileMenu.ks.html"));
		}
    }
}
