namespace Buffaly.Admin.Portal.Pages.Shared
{
	public class _TopBarModel
	{
        private HttpContext? _httpContext => new HttpContextAccessor().HttpContext;

        public string? Logo
		{
			get
			{
                return BasicUtilities.Settings.GetStringOrDefault("AppSettings:LogoUrl", "/images/bf_logo-v2.png");
			}
		}

        public bool IsAgentChatEnabled
        {
            get
            {
                return true;
            }
        }
    }
}
