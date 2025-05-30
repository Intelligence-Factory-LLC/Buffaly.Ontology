

//To override these validators, define them in another file and include that file before this one
if (typeof(window) == "undefined" || !ObjectUtil.HasValue(window["ProtoScriptWorkbenchValidatorsFields"])) {
	var ProtoScriptWorkbenchValidatorsFields = { };
}

if (!ObjectUtil.HasValue(ProtoScriptWorkbenchValidatorsFields.settings)) {
	ProtoScriptWorkbenchValidatorsFields.settings = {Validators : [Validators.Object], InvalidMessage: "Invalid settings"};
}
if (!ObjectUtil.HasValue(ProtoScriptWorkbenchValidatorsFields.Code)) {
	ProtoScriptWorkbenchValidatorsFields.Code = {Validators : [Validators.Text], InvalidMessage: "Invalid Code"};
}
if (!ObjectUtil.HasValue(ProtoScriptWorkbenchValidatorsFields.strImmediate)) {
	ProtoScriptWorkbenchValidatorsFields.strImmediate = {Validators : [Validators.Text], InvalidMessage: "Invalid strImmediate"};
}
if (!ObjectUtil.HasValue(ProtoScriptWorkbenchValidatorsFields.taggingSettings)) {
	ProtoScriptWorkbenchValidatorsFields.taggingSettings = {Validators : [Validators.Object], InvalidMessage: "Invalid taggingSettings"};
}
if (!ObjectUtil.HasValue(ProtoScriptWorkbenchValidatorsFields.strProjectName)) {
	ProtoScriptWorkbenchValidatorsFields.strProjectName = {Validators : [Validators.Text], InvalidMessage: "Invalid strProjectName"};
}
if (!ObjectUtil.HasValue(ProtoScriptWorkbenchValidatorsFields.strProject)) {
	ProtoScriptWorkbenchValidatorsFields.strProject = {Validators : [Validators.Text], InvalidMessage: "Invalid strProject"};
}
if (!ObjectUtil.HasValue(ProtoScriptWorkbenchValidatorsFields.strSessionKey)) {
	ProtoScriptWorkbenchValidatorsFields.strSessionKey = {Validators : [Validators.Text], InvalidMessage: "Invalid strSessionKey"};
}
if (!ObjectUtil.HasValue(ProtoScriptWorkbenchValidatorsFields.iLastMessageID)) {
	ProtoScriptWorkbenchValidatorsFields.iLastMessageID = {Validators : [Validators.Integer], InvalidMessage: "Invalid iLastMessageID"};
}
if (!ObjectUtil.HasValue(ProtoScriptWorkbenchValidatorsFields.strCode)) {
	ProtoScriptWorkbenchValidatorsFields.strCode = {Validators : [Validators.Text], InvalidMessage: "Invalid strCode"};
}
if (!ObjectUtil.HasValue(ProtoScriptWorkbenchValidatorsFields.strSymbol)) {
	ProtoScriptWorkbenchValidatorsFields.strSymbol = {Validators : [Validators.Text], InvalidMessage: "Invalid strSymbol"};
}
if (!ObjectUtil.HasValue(ProtoScriptWorkbenchValidatorsFields.strSearch)) {
	ProtoScriptWorkbenchValidatorsFields.strSearch = {Validators : [Validators.Text], InvalidMessage: "Invalid strSearch"};
}
if (!ObjectUtil.HasValue(ProtoScriptWorkbenchValidatorsFields.strPrototypeName)) {
	ProtoScriptWorkbenchValidatorsFields.strPrototypeName = {Validators : [Validators.Text], InvalidMessage: "Invalid strPrototypeName"};
}
if (!ObjectUtil.HasValue(ProtoScriptWorkbenchValidatorsFields.strFileName)) {
	ProtoScriptWorkbenchValidatorsFields.strFileName = {Validators : [Validators.Text], InvalidMessage: "Invalid strFileName"};
}
if (!ObjectUtil.HasValue(ProtoScriptWorkbenchValidatorsFields.info)) {
	ProtoScriptWorkbenchValidatorsFields.info = {Validators : [Validators.Object], InvalidMessage: "Invalid info"};
}
if (!ObjectUtil.HasValue(ProtoScriptWorkbenchValidatorsFields.strNewFile)) {
	ProtoScriptWorkbenchValidatorsFields.strNewFile = {Validators : [Validators.Text], InvalidMessage: "Invalid strNewFile"};
}
if (!ObjectUtil.HasValue(ProtoScriptWorkbenchValidatorsFields.iMessageID)) {
	ProtoScriptWorkbenchValidatorsFields.iMessageID = {Validators : [Validators.Integer], InvalidMessage: "Invalid iMessageID"};
}
if (!ObjectUtil.HasValue(ProtoScriptWorkbenchValidatorsFields.strShortForm)) {
	ProtoScriptWorkbenchValidatorsFields.strShortForm = {Validators : [Validators.Text], InvalidMessage: "Invalid strShortForm"};
}
if (!ObjectUtil.HasValue(ProtoScriptWorkbenchValidatorsFields.strFile)) {
	ProtoScriptWorkbenchValidatorsFields.strFile = {Validators : [Validators.Text], InvalidMessage: "Invalid strFile"};
}
if (!ObjectUtil.HasValue(ProtoScriptWorkbenchValidatorsFields.strFragment)) {
	ProtoScriptWorkbenchValidatorsFields.strFragment = {Validators : [Validators.Text], InvalidMessage: "Invalid strFragment"};
}
if (!ObjectUtil.HasValue(ProtoScriptWorkbenchValidatorsFields.iPos)) {
	ProtoScriptWorkbenchValidatorsFields.iPos = {Validators : [Validators.Integer], InvalidMessage: "Invalid iPos"};
}
if (!ObjectUtil.HasValue(ProtoScriptWorkbenchValidatorsFields.strLine)) {
	ProtoScriptWorkbenchValidatorsFields.strLine = {Validators : [Validators.Text], InvalidMessage: "Invalid strLine"};
}

		
var ProtoScriptWorkbench = {	
	Url : "/JsonWs/ProtoScript.Extensions.ProtoScriptWorkbench.ashx"

	,
	CompileCode : function(strCode, Callback) {
        return ProtoScriptWorkbench.CompileCodeObject({ strCode : strCode}, Callback);
    },

	CompileCodeObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.CompileCode)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.CompileCode.onValidationError))
					ProtoScriptWorkbench.CompileCode.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "CompileCode", 
					Params : { strCode : oObject.strCode}, 
					Serialize : ProtoScriptWorkbench.CompileCode.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.CompileCode.onErrorReceived) ? ProtoScriptWorkbench.CompileCode.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "CompileCode", { strCode : oObject.strCode}, ProtoScriptWorkbench.CompileCode.Serialize || {});
    },
	CompileCodeWithProject : function(strCode, strProjectName, Callback) {
        return ProtoScriptWorkbench.CompileCodeWithProjectObject({ strCode : strCode,strProjectName : strProjectName}, Callback);
    },

	CompileCodeWithProjectObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.CompileCodeWithProject)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.CompileCodeWithProject.onValidationError))
					ProtoScriptWorkbench.CompileCodeWithProject.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "CompileCodeWithProject", 
					Params : { strCode : oObject.strCode,strProjectName : oObject.strProjectName}, 
					Serialize : ProtoScriptWorkbench.CompileCodeWithProject.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.CompileCodeWithProject.onErrorReceived) ? ProtoScriptWorkbench.CompileCodeWithProject.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "CompileCodeWithProject", { strCode : oObject.strCode,strProjectName : oObject.strProjectName}, ProtoScriptWorkbench.CompileCodeWithProject.Serialize || {});
    },
	CreateNewFile : function(strProject, strNewFile, Callback) {
        return ProtoScriptWorkbench.CreateNewFileObject({ strProject : strProject,strNewFile : strNewFile}, Callback);
    },

	CreateNewFileObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.CreateNewFile)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.CreateNewFile.onValidationError))
					ProtoScriptWorkbench.CreateNewFile.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "CreateNewFile", 
					Params : { strProject : oObject.strProject,strNewFile : oObject.strNewFile}, 
					Serialize : ProtoScriptWorkbench.CreateNewFile.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.CreateNewFile.onErrorReceived) ? ProtoScriptWorkbench.CreateNewFile.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "CreateNewFile", { strProject : oObject.strProject,strNewFile : oObject.strNewFile}, ProtoScriptWorkbench.CreateNewFile.Serialize || {});
    },
	GetBlockedOn : function(Callback) {
        return ProtoScriptWorkbench.GetBlockedOnObject({ }, Callback);
    },

	GetBlockedOnObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.GetBlockedOn)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.GetBlockedOn.onValidationError))
					ProtoScriptWorkbench.GetBlockedOn.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "GetBlockedOn", 
					Params : { }, 
					Serialize : ProtoScriptWorkbench.GetBlockedOn.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.GetBlockedOn.onErrorReceived) ? ProtoScriptWorkbench.GetBlockedOn.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "GetBlockedOn", { }, ProtoScriptWorkbench.GetBlockedOn.Serialize || {});
    },
	GetCallStack : function(Callback) {
        return ProtoScriptWorkbench.GetCallStackObject({ }, Callback);
    },

	GetCallStackObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.GetCallStack)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.GetCallStack.onValidationError))
					ProtoScriptWorkbench.GetCallStack.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "GetCallStack", 
					Params : { }, 
					Serialize : ProtoScriptWorkbench.GetCallStack.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.GetCallStack.onErrorReceived) ? ProtoScriptWorkbench.GetCallStack.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "GetCallStack", { }, ProtoScriptWorkbench.GetCallStack.Serialize || {});
    },
	GetCurrentException : function(Callback) {
        return ProtoScriptWorkbench.GetCurrentExceptionObject({ }, Callback);
    },

	GetCurrentExceptionObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.GetCurrentException)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.GetCurrentException.onValidationError))
					ProtoScriptWorkbench.GetCurrentException.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "GetCurrentException", 
					Params : { }, 
					Serialize : ProtoScriptWorkbench.GetCurrentException.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.GetCurrentException.onErrorReceived) ? ProtoScriptWorkbench.GetCurrentException.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "GetCurrentException", { }, ProtoScriptWorkbench.GetCurrentException.Serialize || {});
    },
	GetNextMessage : function(iLastMessageID, Callback) {
        return ProtoScriptWorkbench.GetNextMessageObject({ iLastMessageID : iLastMessageID}, Callback);
    },

	GetNextMessageObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.GetNextMessage)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.GetNextMessage.onValidationError))
					ProtoScriptWorkbench.GetNextMessage.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "GetNextMessage", 
					Params : { iLastMessageID : oObject.iLastMessageID}, 
					Serialize : ProtoScriptWorkbench.GetNextMessage.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.GetNextMessage.onErrorReceived) ? ProtoScriptWorkbench.GetNextMessage.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "GetNextMessage", { iLastMessageID : oObject.iLastMessageID}, ProtoScriptWorkbench.GetNextMessage.Serialize || {});
    },
	GetOrCreateSession : function(strSessionKey, Callback) {
        return ProtoScriptWorkbench.GetOrCreateSessionObject({ strSessionKey : strSessionKey}, Callback);
    },

	GetOrCreateSessionObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.GetOrCreateSession)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.GetOrCreateSession.onValidationError))
					ProtoScriptWorkbench.GetOrCreateSession.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "GetOrCreateSession", 
					Params : { strSessionKey : oObject.strSessionKey}, 
					Serialize : ProtoScriptWorkbench.GetOrCreateSession.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.GetOrCreateSession.onErrorReceived) ? ProtoScriptWorkbench.GetOrCreateSession.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "GetOrCreateSession", { strSessionKey : oObject.strSessionKey}, ProtoScriptWorkbench.GetOrCreateSession.Serialize || {});
    },
	GetPrototypeAndDescendants : function(strSessionKey, strPrototypeName, Callback) {
        return ProtoScriptWorkbench.GetPrototypeAndDescendantsObject({ strSessionKey : strSessionKey,strPrototypeName : strPrototypeName}, Callback);
    },

	GetPrototypeAndDescendantsObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.GetPrototypeAndDescendants)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.GetPrototypeAndDescendants.onValidationError))
					ProtoScriptWorkbench.GetPrototypeAndDescendants.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "GetPrototypeAndDescendants", 
					Params : { strSessionKey : oObject.strSessionKey,strPrototypeName : oObject.strPrototypeName}, 
					Serialize : ProtoScriptWorkbench.GetPrototypeAndDescendants.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.GetPrototypeAndDescendants.onErrorReceived) ? ProtoScriptWorkbench.GetPrototypeAndDescendants.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "GetPrototypeAndDescendants", { strSessionKey : oObject.strSessionKey,strPrototypeName : oObject.strPrototypeName}, ProtoScriptWorkbench.GetPrototypeAndDescendants.Serialize || {});
    },
	GetPrototypesBySearch : function(strSessionKey, strSearch, Callback) {
        return ProtoScriptWorkbench.GetPrototypesBySearchObject({ strSessionKey : strSessionKey,strSearch : strSearch}, Callback);
    },

	GetPrototypesBySearchObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.GetPrototypesBySearch)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.GetPrototypesBySearch.onValidationError))
					ProtoScriptWorkbench.GetPrototypesBySearch.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "GetPrototypesBySearch", 
					Params : { strSessionKey : oObject.strSessionKey,strSearch : oObject.strSearch}, 
					Serialize : ProtoScriptWorkbench.GetPrototypesBySearch.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.GetPrototypesBySearch.onErrorReceived) ? ProtoScriptWorkbench.GetPrototypesBySearch.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "GetPrototypesBySearch", { strSessionKey : oObject.strSessionKey,strSearch : oObject.strSearch}, ProtoScriptWorkbench.GetPrototypesBySearch.Serialize || {});
    },
	GetSymbol : function(strSessionKey, strSymbol, Callback) {
        return ProtoScriptWorkbench.GetSymbolObject({ strSessionKey : strSessionKey,strSymbol : strSymbol}, Callback);
    },

	GetSymbolObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.GetSymbol)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.GetSymbol.onValidationError))
					ProtoScriptWorkbench.GetSymbol.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "GetSymbol", 
					Params : { strSessionKey : oObject.strSessionKey,strSymbol : oObject.strSymbol}, 
					Serialize : ProtoScriptWorkbench.GetSymbol.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.GetSymbol.onErrorReceived) ? ProtoScriptWorkbench.GetSymbol.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "GetSymbol", { strSessionKey : oObject.strSessionKey,strSymbol : oObject.strSymbol}, ProtoScriptWorkbench.GetSymbol.Serialize || {});
    },
	GetSymbolInfo : function(strSessionKey, strSymbol, info, Callback) {
        return ProtoScriptWorkbench.GetSymbolInfoObject({ strSessionKey : strSessionKey,strSymbol : strSymbol,info : info}, Callback);
    },

	GetSymbolInfoObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.GetSymbolInfo)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.GetSymbolInfo.onValidationError))
					ProtoScriptWorkbench.GetSymbolInfo.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "GetSymbolInfo", 
					Params : { strSessionKey : oObject.strSessionKey,strSymbol : oObject.strSymbol,info : oObject.info}, 
					Serialize : ProtoScriptWorkbench.GetSymbolInfo.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.GetSymbolInfo.onErrorReceived) ? ProtoScriptWorkbench.GetSymbolInfo.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "GetSymbolInfo", { strSessionKey : oObject.strSessionKey,strSymbol : oObject.strSymbol,info : oObject.info}, ProtoScriptWorkbench.GetSymbolInfo.Serialize || {});
    },
	GetSymbols : function(strSessionKey, Callback) {
        return ProtoScriptWorkbench.GetSymbolsObject({ strSessionKey : strSessionKey}, Callback);
    },

	GetSymbolsObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.GetSymbols)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.GetSymbols.onValidationError))
					ProtoScriptWorkbench.GetSymbols.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "GetSymbols", 
					Params : { strSessionKey : oObject.strSessionKey}, 
					Serialize : ProtoScriptWorkbench.GetSymbols.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.GetSymbols.onErrorReceived) ? ProtoScriptWorkbench.GetSymbols.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "GetSymbols", { strSessionKey : oObject.strSessionKey}, ProtoScriptWorkbench.GetSymbols.Serialize || {});
    },
	GetSymbolsAtCursor : function(strSessionKey, strFileName, iPos, Callback) {
        return ProtoScriptWorkbench.GetSymbolsAtCursorObject({ strSessionKey : strSessionKey,strFileName : strFileName,iPos : iPos}, Callback);
    },

	GetSymbolsAtCursorObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.GetSymbolsAtCursor)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.GetSymbolsAtCursor.onValidationError))
					ProtoScriptWorkbench.GetSymbolsAtCursor.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "GetSymbolsAtCursor", 
					Params : { strSessionKey : oObject.strSessionKey,strFileName : oObject.strFileName,iPos : oObject.iPos}, 
					Serialize : ProtoScriptWorkbench.GetSymbolsAtCursor.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.GetSymbolsAtCursor.onErrorReceived) ? ProtoScriptWorkbench.GetSymbolsAtCursor.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "GetSymbolsAtCursor", { strSessionKey : oObject.strSessionKey,strFileName : oObject.strFileName,iPos : oObject.iPos}, ProtoScriptWorkbench.GetSymbolsAtCursor.Serialize || {});
    },
	GetTaggingProgress : function(strSessionKey, Callback) {
        return ProtoScriptWorkbench.GetTaggingProgressObject({ strSessionKey : strSessionKey}, Callback);
    },

	GetTaggingProgressObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.GetTaggingProgress)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.GetTaggingProgress.onValidationError))
					ProtoScriptWorkbench.GetTaggingProgress.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "GetTaggingProgress", 
					Params : { strSessionKey : oObject.strSessionKey}, 
					Serialize : ProtoScriptWorkbench.GetTaggingProgress.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.GetTaggingProgress.onErrorReceived) ? ProtoScriptWorkbench.GetTaggingProgress.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "GetTaggingProgress", { strSessionKey : oObject.strSessionKey}, ProtoScriptWorkbench.GetTaggingProgress.Serialize || {});
    },
	InterpretImmediate : function(strProject, strImmediate, taggingSettings, Callback) {
        return ProtoScriptWorkbench.InterpretImmediateObject({ strProject : strProject,strImmediate : strImmediate,taggingSettings : taggingSettings}, Callback);
    },

	InterpretImmediateObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.InterpretImmediate)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.InterpretImmediate.onValidationError))
					ProtoScriptWorkbench.InterpretImmediate.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "InterpretImmediate", 
					Params : { strProject : oObject.strProject,strImmediate : oObject.strImmediate,taggingSettings : oObject.taggingSettings}, 
					Serialize : ProtoScriptWorkbench.InterpretImmediate.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.InterpretImmediate.onErrorReceived) ? ProtoScriptWorkbench.InterpretImmediate.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "InterpretImmediate", { strProject : oObject.strProject,strImmediate : oObject.strImmediate,taggingSettings : oObject.taggingSettings}, ProtoScriptWorkbench.InterpretImmediate.Serialize || {});
    },
	InterpretImmediateSingleFile : function(strCode, strImmediate, Callback) {
        return ProtoScriptWorkbench.InterpretImmediateSingleFileObject({ strCode : strCode,strImmediate : strImmediate}, Callback);
    },

	InterpretImmediateSingleFileObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.InterpretImmediateSingleFile)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.InterpretImmediateSingleFile.onValidationError))
					ProtoScriptWorkbench.InterpretImmediateSingleFile.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "InterpretImmediateSingleFile", 
					Params : { strCode : oObject.strCode,strImmediate : oObject.strImmediate}, 
					Serialize : ProtoScriptWorkbench.InterpretImmediateSingleFile.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.InterpretImmediateSingleFile.onErrorReceived) ? ProtoScriptWorkbench.InterpretImmediateSingleFile.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "InterpretImmediateSingleFile", { strCode : oObject.strCode,strImmediate : oObject.strImmediate}, ProtoScriptWorkbench.InterpretImmediateSingleFile.Serialize || {});
    },
	LoadFile : function(strSessionKey, strFile, Callback) {
        return ProtoScriptWorkbench.LoadFileObject({ strSessionKey : strSessionKey,strFile : strFile}, Callback);
    },

	LoadFileObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.LoadFile)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.LoadFile.onValidationError))
					ProtoScriptWorkbench.LoadFile.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "LoadFile", 
					Params : { strSessionKey : oObject.strSessionKey,strFile : oObject.strFile}, 
					Serialize : ProtoScriptWorkbench.LoadFile.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.LoadFile.onErrorReceived) ? ProtoScriptWorkbench.LoadFile.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "LoadFile", { strSessionKey : oObject.strSessionKey,strFile : oObject.strFile}, ProtoScriptWorkbench.LoadFile.Serialize || {});
    },
	LoadProject : function(strProject, Callback) {
        return ProtoScriptWorkbench.LoadProjectObject({ strProject : strProject}, Callback);
    },

	LoadProjectObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.LoadProject)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.LoadProject.onValidationError))
					ProtoScriptWorkbench.LoadProject.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "LoadProject", 
					Params : { strProject : oObject.strProject}, 
					Serialize : ProtoScriptWorkbench.LoadProject.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.LoadProject.onErrorReceived) ? ProtoScriptWorkbench.LoadProject.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "LoadProject", { strProject : oObject.strProject}, ProtoScriptWorkbench.LoadProject.Serialize || {});
    },
	ParseCode : function(Code, Callback) {
        return ProtoScriptWorkbench.ParseCodeObject({ Code : Code}, Callback);
    },

	ParseCodeObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.ParseCode)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.ParseCode.onValidationError))
					ProtoScriptWorkbench.ParseCode.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "ParseCode", 
					Params : { Code : oObject.Code}, 
					Serialize : ProtoScriptWorkbench.ParseCode.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.ParseCode.onErrorReceived) ? ProtoScriptWorkbench.ParseCode.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "ParseCode", { Code : oObject.Code}, ProtoScriptWorkbench.ParseCode.Serialize || {});
    },
	PredictNextLine : function(strSessionKey, iPos, Callback) {
        return ProtoScriptWorkbench.PredictNextLineObject({ strSessionKey : strSessionKey,iPos : iPos}, Callback);
    },

	PredictNextLineObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.PredictNextLine)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.PredictNextLine.onValidationError))
					ProtoScriptWorkbench.PredictNextLine.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "PredictNextLine", 
					Params : { strSessionKey : oObject.strSessionKey,iPos : oObject.iPos}, 
					Serialize : ProtoScriptWorkbench.PredictNextLine.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.PredictNextLine.onErrorReceived) ? ProtoScriptWorkbench.PredictNextLine.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "PredictNextLine", { strSessionKey : oObject.strSessionKey,iPos : oObject.iPos}, ProtoScriptWorkbench.PredictNextLine.Serialize || {});
    },
	Reset : function(Callback) {
        return ProtoScriptWorkbench.ResetObject({ }, Callback);
    },

	ResetObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.Reset)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.Reset.onValidationError))
					ProtoScriptWorkbench.Reset.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "Reset", 
					Params : { }, 
					Serialize : ProtoScriptWorkbench.Reset.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.Reset.onErrorReceived) ? ProtoScriptWorkbench.Reset.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "Reset", { }, ProtoScriptWorkbench.Reset.Serialize || {});
    },
	Respond : function(iMessageID, strShortForm, Callback) {
        return ProtoScriptWorkbench.RespondObject({ iMessageID : iMessageID,strShortForm : strShortForm}, Callback);
    },

	RespondObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.Respond)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.Respond.onValidationError))
					ProtoScriptWorkbench.Respond.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "Respond", 
					Params : { iMessageID : oObject.iMessageID,strShortForm : oObject.strShortForm}, 
					Serialize : ProtoScriptWorkbench.Respond.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.Respond.onErrorReceived) ? ProtoScriptWorkbench.Respond.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "Respond", { iMessageID : oObject.iMessageID,strShortForm : oObject.strShortForm}, ProtoScriptWorkbench.Respond.Serialize || {});
    },
	Resume : function(Callback) {
        return ProtoScriptWorkbench.ResumeObject({ }, Callback);
    },

	ResumeObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.Resume)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.Resume.onValidationError))
					ProtoScriptWorkbench.Resume.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "Resume", 
					Params : { }, 
					Serialize : ProtoScriptWorkbench.Resume.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.Resume.onErrorReceived) ? ProtoScriptWorkbench.Resume.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "Resume", { }, ProtoScriptWorkbench.Resume.Serialize || {});
    },
	SaveCurrentCode : function(strSessionKey, strFile, Code, Callback) {
        return ProtoScriptWorkbench.SaveCurrentCodeObject({ strSessionKey : strSessionKey,strFile : strFile,Code : Code}, Callback);
    },

	SaveCurrentCodeObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.SaveCurrentCode)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.SaveCurrentCode.onValidationError))
					ProtoScriptWorkbench.SaveCurrentCode.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "SaveCurrentCode", 
					Params : { strSessionKey : oObject.strSessionKey,strFile : oObject.strFile,Code : oObject.Code}, 
					Serialize : ProtoScriptWorkbench.SaveCurrentCode.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.SaveCurrentCode.onErrorReceived) ? ProtoScriptWorkbench.SaveCurrentCode.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "SaveCurrentCode", { strSessionKey : oObject.strSessionKey,strFile : oObject.strFile,Code : oObject.Code}, ProtoScriptWorkbench.SaveCurrentCode.Serialize || {});
    },
	StepNext : function(Callback) {
        return ProtoScriptWorkbench.StepNextObject({ }, Callback);
    },

	StepNextObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.StepNext)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.StepNext.onValidationError))
					ProtoScriptWorkbench.StepNext.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "StepNext", 
					Params : { }, 
					Serialize : ProtoScriptWorkbench.StepNext.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.StepNext.onErrorReceived) ? ProtoScriptWorkbench.StepNext.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "StepNext", { }, ProtoScriptWorkbench.StepNext.Serialize || {});
    },
	StepOver : function(Callback) {
        return ProtoScriptWorkbench.StepOverObject({ }, Callback);
    },

	StepOverObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.StepOver)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.StepOver.onValidationError))
					ProtoScriptWorkbench.StepOver.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "StepOver", 
					Params : { }, 
					Serialize : ProtoScriptWorkbench.StepOver.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.StepOver.onErrorReceived) ? ProtoScriptWorkbench.StepOver.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "StepOver", { }, ProtoScriptWorkbench.StepOver.Serialize || {});
    },
	StopDebugging : function(Callback) {
        return ProtoScriptWorkbench.StopDebuggingObject({ }, Callback);
    },

	StopDebuggingObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.StopDebugging)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.StopDebugging.onValidationError))
					ProtoScriptWorkbench.StopDebugging.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "StopDebugging", 
					Params : { }, 
					Serialize : ProtoScriptWorkbench.StopDebugging.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.StopDebugging.onErrorReceived) ? ProtoScriptWorkbench.StopDebugging.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "StopDebugging", { }, ProtoScriptWorkbench.StopDebugging.Serialize || {});
    },
	StopTagging : function(strSessionKey, Callback) {
        return ProtoScriptWorkbench.StopTaggingObject({ strSessionKey : strSessionKey}, Callback);
    },

	StopTaggingObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.StopTagging)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.StopTagging.onValidationError))
					ProtoScriptWorkbench.StopTagging.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "StopTagging", 
					Params : { strSessionKey : oObject.strSessionKey}, 
					Serialize : ProtoScriptWorkbench.StopTagging.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.StopTagging.onErrorReceived) ? ProtoScriptWorkbench.StopTagging.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "StopTagging", { strSessionKey : oObject.strSessionKey}, ProtoScriptWorkbench.StopTagging.Serialize || {});
    },
	Suggest : function(strSessionKey, strLine, iPos, Callback) {
        return ProtoScriptWorkbench.SuggestObject({ strSessionKey : strSessionKey,strLine : strLine,iPos : iPos}, Callback);
    },

	SuggestObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.Suggest)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.Suggest.onValidationError))
					ProtoScriptWorkbench.Suggest.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "Suggest", 
					Params : { strSessionKey : oObject.strSessionKey,strLine : oObject.strLine,iPos : oObject.iPos}, 
					Serialize : ProtoScriptWorkbench.Suggest.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.Suggest.onErrorReceived) ? ProtoScriptWorkbench.Suggest.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "Suggest", { strSessionKey : oObject.strSessionKey,strLine : oObject.strLine,iPos : oObject.iPos}, ProtoScriptWorkbench.Suggest.Serialize || {});
    },
	TagImmediate : function(strFragment, strProject, settings, Callback) {
        return ProtoScriptWorkbench.TagImmediateObject({ strFragment : strFragment,strProject : strProject,settings : settings}, Callback);
    },

	TagImmediateObject : function(oObject, Callback) {
        if (!ObjectUtil.HasValue(oObject.IsValidated) || !oObject.IsValidated)
        {
            if (!Validators.Validate(oObject, ProtoScriptWorkbenchValidators.TagImmediate)) {
				var oError = { Error: "Invalid data", Data: oObject };
				if (ObjectUtil.HasValue(ProtoScriptWorkbench.TagImmediate.onValidationError))
					ProtoScriptWorkbench.TagImmediate.onValidationError(oError)
					
				else if (Page.HandleValidationErrors)
					Page.HandleValidationErrors(oError);	
								
				throw "Invalid data";
            }
        }
        
        if (Callback)
        {
            JsonMethod.callWithInitializer({Page: ProtoScriptWorkbench.Url, 
					Method : "TagImmediate", 
					Params : { strFragment : oObject.strFragment,strProject : oObject.strProject,settings : oObject.settings}, 
					Serialize : ProtoScriptWorkbench.TagImmediate.Serialize || {},
					onDataReceived : Callback, 
					onErrorReceived : (ObjectUtil.HasValue(ProtoScriptWorkbench.TagImmediate.onErrorReceived) ? ProtoScriptWorkbench.TagImmediate.onErrorReceived : Page.HandleUnexpectedError) });
        }
        else
            return JsonMethod.callSync(ProtoScriptWorkbench.Url, "TagImmediate", { strFragment : oObject.strFragment,strProject : oObject.strProject,settings : oObject.settings}, ProtoScriptWorkbench.TagImmediate.Serialize || {});
    }
};

var ProtoScriptWorkbenchValidators = {
	

	CompileCode : {
			strCode : ProtoScriptWorkbenchValidatorsFields.strCode	
	},

	CompileCodeWithProject : {
			strCode : ProtoScriptWorkbenchValidatorsFields.strCode,
			strProjectName : ProtoScriptWorkbenchValidatorsFields.strProjectName	
	},

	CreateNewFile : {
			strProject : ProtoScriptWorkbenchValidatorsFields.strProject,
			strNewFile : ProtoScriptWorkbenchValidatorsFields.strNewFile	
	},

	GetBlockedOn : {	
	},

	GetCallStack : {	
	},

	GetCurrentException : {	
	},

	GetNextMessage : {
			iLastMessageID : ProtoScriptWorkbenchValidatorsFields.iLastMessageID	
	},

	GetOrCreateSession : {
			strSessionKey : ProtoScriptWorkbenchValidatorsFields.strSessionKey	
	},

	GetPrototypeAndDescendants : {
			strSessionKey : ProtoScriptWorkbenchValidatorsFields.strSessionKey,
			strPrototypeName : ProtoScriptWorkbenchValidatorsFields.strPrototypeName	
	},

	GetPrototypesBySearch : {
			strSessionKey : ProtoScriptWorkbenchValidatorsFields.strSessionKey,
			strSearch : ProtoScriptWorkbenchValidatorsFields.strSearch	
	},

	GetSymbol : {
			strSessionKey : ProtoScriptWorkbenchValidatorsFields.strSessionKey,
			strSymbol : ProtoScriptWorkbenchValidatorsFields.strSymbol	
	},

	GetSymbolInfo : {
			strSessionKey : ProtoScriptWorkbenchValidatorsFields.strSessionKey,
			strSymbol : ProtoScriptWorkbenchValidatorsFields.strSymbol,
			info : ProtoScriptWorkbenchValidatorsFields.info	
	},

	GetSymbols : {
			strSessionKey : ProtoScriptWorkbenchValidatorsFields.strSessionKey	
	},

	GetSymbolsAtCursor : {
			strSessionKey : ProtoScriptWorkbenchValidatorsFields.strSessionKey,
			strFileName : ProtoScriptWorkbenchValidatorsFields.strFileName,
			iPos : ProtoScriptWorkbenchValidatorsFields.iPos	
	},

	GetTaggingProgress : {
			strSessionKey : ProtoScriptWorkbenchValidatorsFields.strSessionKey	
	},

	InterpretImmediate : {
			strProject : ProtoScriptWorkbenchValidatorsFields.strProject,
			strImmediate : ProtoScriptWorkbenchValidatorsFields.strImmediate,
			taggingSettings : ProtoScriptWorkbenchValidatorsFields.taggingSettings	
	},

	InterpretImmediateSingleFile : {
			strCode : ProtoScriptWorkbenchValidatorsFields.strCode,
			strImmediate : ProtoScriptWorkbenchValidatorsFields.strImmediate	
	},

	LoadFile : {
			strSessionKey : ProtoScriptWorkbenchValidatorsFields.strSessionKey,
			strFile : ProtoScriptWorkbenchValidatorsFields.strFile	
	},

	LoadProject : {
			strProject : ProtoScriptWorkbenchValidatorsFields.strProject	
	},

	ParseCode : {
			Code : ProtoScriptWorkbenchValidatorsFields.Code	
	},

	PredictNextLine : {
			strSessionKey : ProtoScriptWorkbenchValidatorsFields.strSessionKey,
			iPos : ProtoScriptWorkbenchValidatorsFields.iPos	
	},

	Reset : {	
	},

	Respond : {
			iMessageID : ProtoScriptWorkbenchValidatorsFields.iMessageID,
			strShortForm : ProtoScriptWorkbenchValidatorsFields.strShortForm	
	},

	Resume : {	
	},

	SaveCurrentCode : {
			strSessionKey : ProtoScriptWorkbenchValidatorsFields.strSessionKey,
			strFile : ProtoScriptWorkbenchValidatorsFields.strFile,
			Code : ProtoScriptWorkbenchValidatorsFields.Code	
	},

	StepNext : {	
	},

	StepOver : {	
	},

	StopDebugging : {	
	},

	StopTagging : {
			strSessionKey : ProtoScriptWorkbenchValidatorsFields.strSessionKey	
	},

	Suggest : {
			strSessionKey : ProtoScriptWorkbenchValidatorsFields.strSessionKey,
			strLine : ProtoScriptWorkbenchValidatorsFields.strLine,
			iPos : ProtoScriptWorkbenchValidatorsFields.iPos	
	},

	TagImmediate : {
			strFragment : ProtoScriptWorkbenchValidatorsFields.strFragment,
			strProject : ProtoScriptWorkbenchValidatorsFields.strProject,
			settings : ProtoScriptWorkbenchValidatorsFields.settings	
	}
};

    