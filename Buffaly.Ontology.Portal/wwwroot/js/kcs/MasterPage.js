

var UserMessages = null;

Page.AddOnload(function () {
	UserMessages = new UserMessagesClass();
	UserMessages.MessagesContainerID = "divMessages";
	UserMessages.OnFulfillMessage = UserMessages_ToastrFulfill;
	UserMessages.DisplayAll();
});

Page.AddOnUnload(function () {
	if (null != UserMessages)
		UserMessages.Save();
});


function BindJsToCss(oParent) {
	oParent = ControlUtil.GetControl(oParent || document.body);
	SetupInputs(oParent);
}



function SetupInputs(oParent) {
	oParent = _$(oParent || document.body);

	ControlUtil.GetChildren(oParent, ".InputMoney").forEach(function (oInput) {
		oInput.maxlength = 10;
		ControlUtil.AddEvent(oInput, "blur", function () { FormatMoney(oInput); });
		oInput.setValue = function (val) {
			this.value = StringToMoney(val);
		}
		oInput.getValue = function () {
			return CleanDouble(this.value);
		}
		oInput.setValue(oInput.value); //cause formatting for any initial value
	});

	ControlUtil.GetChildren(oParent, ".InputDecimal").forEach(function (oInput) {
		ControlUtil.AddEvent(oInput, "blur", function () { this.value = StringToDecimal(this.value); });
		oInput.setValue = function (val) {
			this.value = StringToDecimal(val);
		}
		oInput.getValue = function () {
			return CleanDouble(this.value);
		}
	});

	ControlUtil.GetChildren(oParent, ".InputInteger").forEach(function (oInput) {
		ControlUtil.AddEvent(oInput, "blur", function () { this.value = StringToInteger(this.value); });
		oInput.setValue = function (val) {
			this.value = StringToInteger(val);
		}
		oInput.getValue = function () {
			return CleanInt(this.value);
		}
	});


	ControlUtil.GetChildren(oParent, ".InputPercent").forEach(function (oInput) {
		var oNewInput = oInput.cloneNode();
		var oParent = oInput.parentElement;
		oInput.remove();

		oNewInput.maxlength = 7;
		ControlUtil.AddEvent(oNewInput, "blur", function () { FormatPercent(oNewInput); });

		oParent.appendChild($$$(["div", { 'class': "input-group" },
			oNewInput,
			["span", { "class": "input-group-addon" }, "%"]
		]));
	});

	ControlUtil.GetChildren(oParent, ".InputPhone").forEach(function (oInput) {
		oInput.maxlength = 255;
		ControlUtil.AddEvent(oInput, "blur", function () { FormatPhone(oInput) });
	});

	ControlUtil.GetChildren(oParent, ".Year").forEach(function (oYear) {
		oYear.innerText = new Date().getFullYear();
	});

	ControlUtil.GetChildren(oParent, ".modal").forEach(function (oModal) {
		Page.Modals[oModal.id] = new ModalControl(oModal.id);
	})
	ControlUtil.GetChildren(oParent, "input[list]").forEach(function (oDdl) {
		ControlUtil.AddEvent(oDdl, "focus", Master_EditableDdlFocus);
		ControlUtil.AddEvent(oDdl, "blur", Master_EditableDdlBlur);
	});

	ControlUtil.GetChildren(oParent, ".InputData").forEach(x => CreateJsonEditor(x));

	ControlUtil.GetChildren(oParent, ".DisplayMarkdown").forEach(x => {

		let oEditor = CodeMirror.fromTextArea(x, {
			mode: "markdown", // Markdown editing mode
			indentWithTabs: true,
			smartIndent: true,
			lineNumbers: true,
			matchBrackets: true,
			readOnly: true,
			lineWrapping: true // Enable line wrapping
		});

		// Update Preview on Blur
		oEditor.on('blur', () => {
			const markdownContent = oEditor.getValue();
			ControlUtil.SetValue(x, markdownContent);
		});

		// Optional: Sync with external changes
		ControlUtil.AddChange(x, function () {
			oEditor.setValue(ControlUtil.GetValue(x));
		});
	});

	ControlUtil.GetChildren(oParent, ".InputMarkdown").forEach(x => {

		let sPreview = x.getAttribute("data-preview");
		let previewElement = null;
		if (!StringUtil.IsEmpty(sPreview)) {
			previewElement = _$(sPreview);

			if (previewElement) {
				const htmlContent = marked.parse(ControlUtil.GetValue(x));
				previewElement.innerHTML = htmlContent;
			}
		}

		let oEditor = CodeMirror.fromTextArea(x, {
			mode: "markdown", // Markdown editing mode
			indentWithTabs: true,
			smartIndent: true,
			lineNumbers: true,
			matchBrackets: true,
			lineWrapping: true // Enable line wrapping
		});

		// Update Preview on Blur
		oEditor.on('blur', () => {
			const markdownContent = oEditor.getValue();
			ControlUtil.SetValue(x, markdownContent);

			if (previewElement) {
				const htmlContent = marked.parse(markdownContent);
				previewElement.innerHTML = htmlContent;
			}
		});

		// Optional: Sync with external changes
		ControlUtil.AddChange(x, function () {
			oEditor.setValue(ControlUtil.GetValue(x));
			if (previewElement) {
				const htmlContent = marked.parse(ControlUtil.GetValue(x));
				previewElement.innerHTML = htmlContent;
			}
		});
	});

	ControlUtil.GetChildren(oParent, ".InputJavascript").forEach(x => {


		let oEditor = CodeMirror.fromTextArea(x, {
			mode: "javascript", 
			indentWithTabs: true,
			smartIndent: true,
			lineNumbers: true,
			matchBrackets: true,
			lineWrapping: true // Enable line wrapping
		});

		// Update Preview on Blur
		oEditor.on('blur', () => {
			const sContent = oEditor.getValue();
			ControlUtil.SetValue(x, sContent);
		});

		// Optional: Sync with external changes
		ControlUtil.AddChange(x, function () {
			oEditor.setValue(ControlUtil.GetValue(x));
		});
	});

	ControlUtil.GetChildren(oParent, ".InputkScript").forEach(x => {

		CodeMirror.defineMode("kscript", function (config) {
			return CodeMirror.multiplexingMode(
				CodeMirror.getMode(config, "htmlmixed"),
				{
					open: "<" + "%", close: "%" + ">",
					mode: CodeMirror.getMode(config, "text/plain"),
					delimStyle: "delimit"
				}
			);
		});

		let oEditor = CodeMirror.fromTextArea(x, {
			mode: "kscript",
			indentWithTabs: true,
			smartIndent: true,
			lineNumbers: true,
			matchBrackets: true,
			lineWrapping: true // Enable line wrapping
		});

		// Update Preview on Blur
		oEditor.on('blur', () => {
			const sContent = oEditor.getValue();
			ControlUtil.SetValue(x, sContent);
		});

		// Optional: Sync with external changes
		ControlUtil.AddChange(x, function () {
			oEditor.setValue(ControlUtil.GetValue(x));
		});

		x.Editor = oEditor;
	});
}

function CreateJsonEditor(oInput) {
	if (typeof CodeMirror !== 'undefined') {
		var oEditor = CodeMirror.fromTextArea(oInput, {
			lineNumbers: true,
			mode: 'application/ld+json',
			indentWithTabs: true,
			indentUnit: 4,
			tabSize: 4,
			lineWrapping: true,
		});

		function formatJson(jsonString) {
			try {
				if (!StringUtil.IsEmpty(jsonString)) {
					const parsedJson = JSON.parse(jsonString);
					const formattedJson = JSON.stringify(parsedJson, null, 4);
					jsonString = formattedJson;
				}
			} catch (e) {
				console.error('Invalid JSON:', e);
			}
			return jsonString;
		}

		// Function to format and set editor value
		function formatAndSetEditorValue(jsonString) {
			const formattedJson = formatJson(jsonString);
			oEditor.setValue(formattedJson);
		}

		// Initial formatting
		formatAndSetEditorValue(oInput.value);

		// Format JSON on input change
		ControlUtil.AddChange(oInput, function () {
			const formattedJson = formatJson(oInput.value);
			oEditor.setValue(formattedJson);
		});

		// Update input on editor change
		oEditor.on('change', function () {
			const formattedJson = formatJson(oEditor.getValue());
			oInput.value = formattedJson;
		});

		// Update input on editor blur
		oEditor.on('blur', function () {
			const formattedJson = formatJson(oEditor.getValue());
			oInput.value = formattedJson;
		});

		return oEditor;
	}
}
function Master_EditableDdlFocus(evt) {

	let ctrl = evt.target;
	let sValue = ctrl.value;
	if (!StringUtil.IsEmpty(sValue))
		ctrl.setAttribute('placeholder', sValue);
	ctrl.value = ''

}


function Master_EditableDdlBlur(evt) {

	let ctrl = evt.target;
	let sValue = ctrl.value;
	if (StringUtil.IsEmpty(sValue))
		ctrl.value = ctrl.getAttribute('placeholder');
}


class ModalControl {

	ContentElementID = '';
	DisableBackground = true;

	constructor(sID) {
		this.ContentElementID = sID;
	}

	IsVisible = false;


        ShowContent(e) {
                if (window.jQuery && jQuery.fn.modal)
                        jQuery("#"+this.ContentElementID).modal('show');
                _$(this.ContentElementID).getElements(".ModalFocus").forEach(TryFocus);
                this.IsVisible=true;
                return false;
        }

        HideContent() {
                if (window.jQuery && jQuery.fn.modal)
                        jQuery("#"+this.ContentElementID).modal('hide');
                this.IsVisible=false;
                return false;
        }
}

const MasterPage={

	m_bOnloadRun: false,
	m_Onloads: [],

	//Use this method if a script should be loaded after the MasterPage's js is loaded (which is later than Page.AddOnload)
	AddOnload: function (oVar) {
		if (MasterPage.m_bOnloadRun) { //Scripts loaded after page load should still run correctly
			if (Globals.ReportErrors) {
				try {
					oVar();
				}
				catch (err) {
					Page.LogError(err);
				}
			}
			else
				oVar();
		}
		else {
			MasterPage.m_Onloads.push(oVar);
		}
	},

	_RunOnLoads: function () {
		MasterPage.m_Onloads.forEach(function (oVar) {
			MasterPage.m_bOnloadRun = true;
			if (Globals.ReportErrors) {
				try {
					oVar();
				}
				catch (err) {
					Page.LogError(err);
				}
			}
			else
				oVar();
		});
	}
}

//Added to alleviate simple errors
MasterPage.AddOnLoad = MasterPage.AddOnload;


function Master_Load(e) {
	//trace("Master_Load Starting");
	SetupLoginPopup();
	BindJsToCss();
	AddAltToGrids();

	if (typeof Page.LocalStorage === 'undefined') {
		Page.LocalStorage = new LocalStorage();
		Page.LocalStorage.GetValue = function (sKey) {
			return Page.LocalStorage.get(UrlUtil.GetUrlWithoutParams(document.location.href) + sKey);
		}
		Page.LocalStorage.SetValue = function (sKey, oValue) {
			Page.LocalStorage.set(UrlUtil.GetUrlWithoutParams(document.location.href) + sKey, oValue);
		}
	}

	var bShowQuickHelp = Page.LocalStorage.get("ShowQuickHelp");

	if (bShowQuickHelp && typeof ShowQuickHelp !== 'undefined')
		ShowQuickHelp();
	//trace("Master_Load Stopping");

	MasterPage._RunOnLoads();
}

/**
 * Striping helper for
 *   • table-based “Grid” layouts
 *   • div-based “Inputs” layouts (rows of .row)
 *
 * New in this revision
 *   • Removes margin-bottom from .row (replaced by padding)
 *   • Adds Bootstrap 5 borders so each Inputs row looks like a table row
 *   • Background colour spans full width (label + editor column)
 */
function AddAltToGrids() {

	const grids = document.querySelectorAll(".Grid, .Inputs");

	grids.forEach(function (grid) {

		/* ───────────────────── tables ───────────────────── */
		if (grid.tagName.toLowerCase() === "table") {

			let stripe = false;

			grid.querySelectorAll(":scope > tbody > tr, :scope > tr")
				.forEach(function (tr) {

					if (tr.classList.contains("Hidden")) return;

					tr.classList.remove("Alt", "table-light");
					tr.classList.toggle("table-light", stripe);

					stripe = !stripe;
				});
			return;
		}

		/* ──────────────── div-based Inputs ──────────────── */
		const rows = grid.querySelectorAll(":scope > .row");
		if (rows.length === 0) return;

		let stripe = false;

		rows.forEach(function (row, idx) {

			if (row.classList.contains("Hidden")) return;

			/* layout tweaks: remove mb-3, add uniform padding */
			row.classList.remove("mb-3");
			row.classList.add("py-1");

			/* borders: full-width top/bottom like a table row */
			row.classList.remove("border-top", "border-bottom");
			if (idx === 0) row.classList.add("border-top");
			if (idx < rows.length - 1) row.classList.add("border-bottom");

			/* clear previous stripe, then apply */
			row.classList.remove("Alt", "bg-light", "table-light");
			Array.from(row.children).forEach(function (child) {
				child.classList.remove("bg-light", "table-light");
			});

			if (stripe) {
				row.classList.add("bg-light");
				Array.from(row.children).forEach(function (child) {
					child.classList.add("bg-light");
				});
			}

			stripe = !stripe;
		});
	});
}



Page.AddOnLoad(Master_Load);

function TryFocus(oCtrl) {
	if (oCtrl && ObjectUtil.HasValue(oCtrl.focus))
		oCtrl.focus();
}


Page.AddOnload(function () {
	$$(".Focus").forEach(TryFocus);
});

function SetupLoginPopup() {
	Page.HandleSessionExpired = function (fOnLogin) {
		ControlUtil.AddClick("txtLoginPopup_Submit", function () {
			OnLoginPopup(fOnLogin)
		});
		jQuery("#divSessionExpired").modal('show');
	}
}

function OnLoginPopup(fOnLogin) {
	let oObj = BlindUnbind("divSessionExpired");

	JsonMethod.call("/JsonWs/Buffaly.Business.Authentication.ashx", "Login", { Email: oObj.Login, Password: oObj.Password },
		function (oRes) {
			if (oRes.IsAuthorized) {
				jQuery("#divSessionExpired").modal('hide');
				if (fOnLogin)
					fOnLogin();
			}
			else {
				alert("Login attempt failed");
			}
		});
}


function IsInteger(s) {
	var valid = false;
	if (null != s && !StringUtil.IsEmpty(s) && !isNaN(s)) {
		valid = parseInt(s, 10) == s;
	}
	return valid;
}


function IsValidQuantity(Quantity, mAx, min, allowPartial) {
	if (isNaN(Quantity) || Quantity.toInt() > mAx || Quantity.toInt() < min) {
		return false;
	}

	if (!allowPartial && !IsInteger(Quantity)) {
		return false;
	}
	return true;
}


Page.HandleValidationErrors=function (err) {
	if (err.Error=="Invalid data") {

		console.log("Validation Errors:");
		console.log(err);

		let bFirst=true;
		for (let i in err.Data.ExtendedValidationErrors) {

			let oParent;
			if (err.Data.ParentElementID)
				oParent=_$(err.Data.ParentElementID);
			else
				oParent=_$(document.body);

			let oInputs=ControlUtil.GetChildren(oParent, "[kcs:FieldName='"+i+"']");
			if (oInputs.length>0) {
				let oInput=oInputs[0];
				if (oInput.hasClass("Hidden")||oInput.style.display =="none")
					oInput=oInput.parentNode;

				oInput.addClass("input_error");
				if (null != oInput.setCustomValidity) {
					oInput.setCustomValidity(err.Data.Validators[i].InvalidMessage);
					oInput.reportValidity();
				}

				/*oInput.tooltip=new mBox.Tooltip({
					content: '<p style="color: #457699; font-size:14px">Invalid Input</p><p style="margin-top:10px">'+err.Data.Validators[i].InvalidMessage+'</p>',
					attach: oInput,
					width: 300,
					position: { x: 'right', y: 'center' },
					offset: { x: 10 },
					pointer: ['top', 15],
					setStyles: {
						content: { fontSize: 13, lineHeight: 18, padding: '13px 15px' }
					},
					closeOnBodyClick: true
				});*/
			}
		}

		let sMessages=["Please correct validation errors to continue."];
		err.Data.ValidationErrors.forEach(function (sError) {
			if (!sMessages.contains(sError)) {
				sMessages.push(sError);
			}
		});

		sMessages.forEach(x => {
			UserMessages.DisplayNow(x, "Error");
		});


	}
	else
		throw err;
}

Page.ClearValidationErrors = function () {
	UserMessages.ClearDiv();
	$$(".input_error").forEach(function (oInput) {
		oInput.removeClass("input_error");
		if (ObjectUtil.HasValue(oInput.tooltip)) {
			oInput.tooltip.close();
			oInput.tooltip.block = true;
		}
	});
}




Element.implement({
	bindHTML: function (sHtml) {
		this.innerHTML = sHtml;

		BindJsToCss(this);

		return this;
	}
});



Page.AddOnload(function () {
	if (jQuery('.datetimepicker').datetimepicker)
		jQuery('.datetimepicker').datetimepicker();
});



function OnFormatDate(sParent) {

	let date=null;
	let actualdate=null;

	if (sParent!=undefined||sParent!=null) {
		ControlUtil.GetChildren(sParent, ".Date").forEach(GetDateTimeLocal);
		ControlUtil.GetChildren(sParent, ".DateOnly").forEach(GetDateLocal);
	}
	else {
		$$(".Date").forEach(GetDateTimeLocal);
		$$(".DateOnly").forEach(GetDateLocal);
		$$(".DateOnlyFriendly").forEach(GetDateLocalFriendly);
	}

};

function GetDateLocalFriendly(oDate) {
	let actualdate = ControlUtil.GetText(oDate);

	if (!StringUtil.IsEmpty(actualdate)) {
		let date = DateUtil.UTCToLocal(actualdate);

		let TittleDate = DateUtil.IsDateOnly(date) ? moment(date).format('MMMM D, YYYY').toLocaleString() : moment(date).format('MMMM D, YYYY h:mm:ss A').toLocaleString() + " EST ";
		let InnerDate = moment(date).format('MMMM D, YYYY').toLocaleString();

		oDate.title = TittleDate;
		oDate.innerHTML = InnerDate;
	}

	return oDate;
}
function GetDateTimeLocal(oDate) {
	let actualdate=ControlUtil.GetText(oDate);

	if (!StringUtil.IsEmpty(actualdate)) {
		date=DateUtil.UTCToLocal(actualdate);

		let TittleDate=moment(date).format('M/D/YYYY h:mm:ss A').toLocaleString()+" EST ";
		let InnerDate=moment(date).format('M/D/YYYY h:mm A').toLocaleString();

		oDate.title = TittleDate;
		oDate.innerHTML = InnerDate;
	}

	return oDate;
}

function GetDateLocal(oDate) {
	let actualdate=ControlUtil.GetText(oDate);

	if (!StringUtil.IsEmpty(actualdate)) {

		date = DateUtil.UTCToLocal(actualdate);

		let TittleDate=DateUtil.IsDateOnly(date)? moment(date).format('M/D/YYYY').toLocaleString():moment(date).format('M/D/YYYY h:mm:ss A').toLocaleString()+" EST ";
		let InnerDate=moment(date).format('M/D/YYYY').toLocaleString();

		oDate.title=TittleDate;
		oDate.innerHTML=InnerDate;
	}

	return oDate;
}

function OnFormatGrid(ctrl) {
	if (null == ctrl && null == ControlUtil.GetControl(ctrl))
		return;
	OnFormatDate(ctrl)
	ControlUtil.GetControl(ctrl).querySelectorAll("table.Grid").forEach(x => {
		x.addClass("table table-striped table-bordered datatables dataTable");
	});
	ControlUtil.GetControl(ctrl).querySelectorAll('[data-bs-toggle="tooltip"]').forEach(x => {
		new bootstrap.Tooltip(x);
	});
}

Page.Grids.addEvent('grid-inserted', function (oGrid) {
	oGrid.addEvent("complete", function () {
		ControlUtil.GetChildren(oGrid.ContentControlID, "table.Grid").forEach(x => x.addClass("table table-striped table-bordered datatables dataTable"));
	});
});


StringUtil.NormalizeCase = function(str) {
	if (StringUtil.IsEmpty(str) || $type(str) != "string")
		return "";
	return str.toLowerCase().replace(/\b\w/g, function (char) {
		return char.toUpperCase();
	});
}


if (!Page.Tabs) {
	Page.Tabs = {};
};

function SaveTab(id) {
	if (Page.Tabs[id] && Page.Tabs[id].OnLoad) {
		Object.keys(Page.Tabs).forEach(key => {
			Page.Tabs[key].IsActive = false;
		});

		Page.Tabs[id].IsActive = true;
		Page.Tabs[id].OnLoad();
	}

	new LocalStorage().set(UrlUtil.GetUrlWithoutParams(document.location.href) + ".activetab", id)
}

function ShowTab(sTabName) {
	$$('a[href="#' + sTabName + '"]').each(x => x.click())
}


//Page.AddOnload(function () {
//	var activeId = new LocalStorage().get(UrlUtil.GetUrlWithoutParams(document.location.href) + ".activetab")
//	let bActivated = false;
//	if (activeId) {
//		$$(".nav-tabs li").forEach(x => x.removeClass("active"));
//		$$(".tab-content > div").forEach(x => x.removeClass("active"));

//		if (ObjectUtil.HasValue(_$(activeId))) {
//			_$(activeId).addClass("active");
//			var oLi = _$("li-" + activeId);
//			if (ObjectUtil.HasValue(oLi)) {
//				oLi.addClass("active");
//				bActivated = true;
//			}

//			SaveTab(activeId);
//		}
//	}

//	if (!bActivated) {
//		let lstTabs = $$(".tab-content > div");
//		if (lstTabs && lstTabs.length > 0) {
//			SaveTab(lstTabs[0].id);
//		}
//	}
//})