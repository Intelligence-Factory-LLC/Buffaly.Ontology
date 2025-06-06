﻿using BasicUtilities;
using RooTrax.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;
using System.Net;

namespace Buffaly.Admin.Portal.Pages
{
    public class kModel : PageModel
	{
		public kModel()
		{
		}


		private string? GetStringOrNull(string strKey)
		{
			string? strResult = null;
			StringValues sv = Request.Query[strKey];

			if (sv.Count >= 1)
				strResult = sv[0];

			return strResult;
		}

		public string Output
		{
			get
			{
				if (RouteData.Values["Output"] != null)
					return RouteData.Values["Output"]!.ToString() ?? string.Empty;

				return GetStringOrNull("Output") ?? String.Empty;
			}
		}

		public string Class
		{
			get
			{
				if (RouteData.Values["Class"] != null)
					return RouteData.Values["Class"]!.ToString() ?? string.Empty;

				return GetStringOrNull("Class") ?? String.Empty;
			}
		}

		public string Handler
		{
			get
			{
				return GetStringOrNull("Handler") ?? String.Empty;

			}
		}
		public string? Logo
		{
			get
			{
				return BasicUtilities.Settings.GetStringOrDefault("AppSettings:LogoUrl", "/images/bf_logo-v2.png");
			}
		}


		public string? Title { get; set; }

		public string? Head { get; set; }
		public string? Breadcrumbs { get; set; }
		public string? MainPanelContent { get; set; }
		public string? MainPanelTitle { get; set; }
		public string? MainPanelTitleOptions { get; set; }
		public string? BottomScripts { get; set; }
		public string? SidebarMenu { get; set; }

		public string? FileMenu { get; set; }
		public string? ErrorMessage { get; set; }
        public IActionResult OnGet()
		{
			Logs.DebugLog.WriteEvent("Index", "Get");

			try
			{

				if (StringUtil.IsEmpty(this.Output))
					throw new ArgumentException("Output");

				if (StringUtil.IsEmpty(this.Class))
					throw new ArgumentException("Class");

				//TODO: Remove
				RooTraxState.Reset();

				//kScript3.kScriptControl kScript = RooTraxState.kScriptControl;


                kScript3.kScriptControl? kScript = null;
                if (StringUtil.IsEmpty(this.Handler) || StringUtil.EqualNoCase(this.Handler, "Buffaly.SemanticDB.UI"))
                {
                    kScript = Buffaly.SemanticDB.UI.RooTraxState.kScriptControl; ;
                }


                if (!StringUtil.IsEmpty(this.Handler))
				{
					//kScript = Application[this.Handler] as kScript3.kScriptControl;
				}


                                if (kScript == null)
                                    throw new InvalidOperationException("kScript is not initialized");
                                kScript3.SymbolTable oSymbolTable = kScript.GetSymbolTable();

				try
				{
					oSymbolTable.EnterBlock();

					if (this.Output.Contains("\\") && null != RooTraxState.Settings.kScriptRootDir)
						kScript.AddIncludePath(DirectoryUtil.BuildPath(RooTraxState.Settings.kScriptRootDir, StringUtil.LeftOfLast(this.Output, "\\")));

					oSymbolTable.GetGlobalScope().Symbols.Remove(this.Class);


					JsonObject jsonRequest = new JsonObject();
					foreach (string strKey in this.Request.Query.Keys)
					{
						if (!StringUtil.IsEmpty(strKey))
							jsonRequest[strKey] = new JsonValue(this.Request.Query[strKey].ToString(), false);
					}

					oSymbolTable.ActiveScope().InsertSymbol("Request", jsonRequest);
					oSymbolTable.ActiveScope().InsertSymbol("AppSettings", BasicUtilities.Settings.AppSettings);

					kScript.EvaluateString("<%include " + this.Class.Replace("Page", "") + ".ks.html%>", ref oSymbolTable);
					kScript.EvaluateString("<%include " + this.Output + "%>", ref oSymbolTable);

					bool bHideExtras = false;

					if (StringUtil.EqualNoCase(this.Class, "SimpleObjectDetailsPage"))
					{
						string? strObjectID = null;

						try
						{
							strObjectID = TryEvaluate(this.Class, "ObjectID", ref oSymbolTable, ref kScript);
						}
						catch
						{
							strObjectID = "(null)";
						}

						object? oCachedObject = null;

						if (!StringUtil.IsNumber(strObjectID))
						{

						}
						else
						{
							oCachedObject = TryEvaluateEx("SimpleObjectDetailsPage", "Object", ref oSymbolTable, ref kScript);
						}

						bHideExtras = (null == oCachedObject);

						oSymbolTable.ActiveScope().InsertSymbol("CachedObject", oCachedObject);
					}


					string strTitle = TryEvaluate(this.Class, "Title", ref oSymbolTable, ref kScript);
					if (!StringUtil.IsEmpty(strTitle))
						this.Title = strTitle;

                    this.Head = TryEvaluate(this.Class, "Head", ref oSymbolTable, ref kScript);
					this.Breadcrumbs = TryEvaluate(this.Class, "Breadcrumbs", ref oSymbolTable, ref kScript);
					this.MainPanelContent = TryEvaluate(this.Class, "MainPanelContent", ref oSymbolTable, ref kScript);
					this.MainPanelTitle = TryEvaluate(this.Class, "MainPanelTitle", ref oSymbolTable, ref kScript);
					this.MainPanelTitleOptions = TryEvaluate(this.Class, "MainPanelTitleOptions", ref oSymbolTable, ref kScript);
					this.SidebarMenu = this.GetSidebarMenu(kScript);
					this.FileMenu = this.GetFileMenu(kScript);

					if (!bHideExtras)
					{
						this.BottomScripts = TryEvaluate(this.Class, "BottomScripts", ref oSymbolTable, ref kScript);
					}
				}
				finally
				{
					oSymbolTable.Leave();
				}
			}
			catch (Exception err)
			{
                if (StringUtil.InString(err.Message, "Session expired")
                                    || StringUtil.InString(err.InnerException?.Message, "Session expired"))
                {
                    return RedirectToPage("/login");
                }

                if (err is kScript3.RuntimeException || err is kScript3.SyntaxException)
                {
                    Logs.DebugLog.WriteEvent("kScript Error", $"Output: {this.Output}, Handler: {this.Handler}, Class: {this.Class}");

                    if (BasicUtilities.Settings.GetBoolOrFalse("ReturnExceptions"))
                    {
                        this.ErrorMessage = BuildErrorMessage(err);
                    }

					Logs.DebugLog.WriteError(err, true);
                }

                else
                {
                    Logs.LogError(err);
                }
            }

			return Page();
		}

        private string BuildErrorMessage(Exception? err)
        {
            if (err == null) return string.Empty;

            var innerMessage = string.Empty;
            if (err.InnerException != null)
            {
                innerMessage = BuildErrorMessage(err.InnerException);
            }

            string errorMessage = @$"
        <div class=""alert alert-danger"" role=""alert"">
            {WebUtility.HtmlEncode(err.Message)}
            <h3>Source</h3>
            <pre>{WebUtility.HtmlEncode(err.Source ?? string.Empty)}</pre>
            <h3>Stack Trace</h3>
            <pre>{err.StackTrace ?? string.Empty}</pre>
        </div>
        {innerMessage}";

            return errorMessage;
        }
        private string TryEvaluate(string strClass, string strMember, ref kScript3.SymbolTable oSymbolTable, ref kScript3.kScriptControl oHandler)
		{
			if (oSymbolTable.GetScope(strClass).Symbols.ContainsKey(strMember))
			{
				return oHandler.EvaluateString("<%" + strClass + "." + strMember + "%>", ref oSymbolTable);
			}

			return string.Empty;
		}

                private object? TryEvaluateEx(string strClass, string strMember, ref kScript3.SymbolTable oSymbolTable, ref kScript3.kScriptControl oHandler)
                {
                        if (oSymbolTable.GetScope(strClass).Symbols.ContainsKey(strMember))
                        {
                                return oHandler.EvaluateFunctionNObjectsEx(strClass + "." + strMember, new List<object>(), ref oSymbolTable);
                        }

			return string.Empty;
		}
                public string GetSidebarMenu(kScript3.kScriptControl oHandler)
                {
                        string rootDir = oHandler.GetRootDir() ?? string.Empty;
                        return oHandler.EvaluateFile(FileUtil.BuildPath(rootDir, "Administrator\\LeftMenu.ks.html")) ?? string.Empty;
                }
                public string GetFileMenu(kScript3.kScriptControl oHandler)
                {
                        string rootDir = oHandler.GetRootDir() ?? string.Empty;
                        return oHandler.EvaluateFile(FileUtil.BuildPath(rootDir, "Administrator\\FileMenu.ks.html")) ?? string.Empty;
                }


	}
}
