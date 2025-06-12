namespace Buffaly.Ontology.Portal
{
    /// <summary>
    /// Shared base class for Razor Page models across Buffaly portals.
    /// Provides helper methods for accessing request parameters.
    /// </summary>
    public class BaseModel : Buffaly.Common.BaseModel
	{
        protected ISession Session => HttpContext.Session;

		public string ? Logo { get; set; }
    }
}

