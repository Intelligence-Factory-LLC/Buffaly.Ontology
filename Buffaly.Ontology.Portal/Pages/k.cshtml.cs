using Buffaly.Common;
using Buffaly.Common.Models;
using Buffaly.UI;
using kScript3;
using Microsoft.AspNetCore.Authorization;

namespace Portal.Pages
{
	    public class kModel : KScriptPageModel<BaseUserState>
    {
        public kModel() : base()
        {
        }

        protected override BaseUserState UserState => new BaseUserState(HttpContext);

		protected override kScriptControl GetKScript()
		{
			return RooTraxState.kScriptControl;
		}
		public string RoleName
		{
			get
			{
				return "Administrator";
			}
		}

		public string UserName 		
		{
			get
			{
				return "Administrator";
			}
		}
	}
}
