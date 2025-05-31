Page.PreserveViewState();

Page.LocalSettings = {
	Solution: null,
	SolutionHistory: [],
	File: null,
	FileHistory: [],
	ImmediateHistory: [],
	WindowSize: "Halfize",
	WebRoot: null
}
Page.GetLocalSettings();
Page.PreserveLocalSettings();

Page.AddOnUnload(function () {
	if (!IsCurrentFileSaved())
		return "You have unsaved changes, continue?"
});



function IsCurrentFileSaved() {
	return StringUtil.IsEmpty(sLastSaved) || GetCode() == sLastSaved;
}


Page.HandleUnexpectedError = function (oErr) {
	if (oErr instanceof ReferenceError) {
		// specific handling if you want it
		Output(`ReferenceError: ${oErr.message}\n${oErr.stack ?? ""}`);
	}
	else if (oErr instanceof Error) {
		// any other Error subtype (TypeError, SyntaxError, etc.)
		Output(`${oErr.name}: ${oErr.message}\n${oErr.stack ?? ""}`);
	}
	else {
		Output(oErr);
	}
}


Page.AddOnLoad(async function () {

	BindSolutionHistory();
	BindFileHistory();
	_$("page-heading")?.addClass("hidden");
	BindImmediateHistory();

	AdjustWindowSizes();

	document.addEventListener('keydown', function (event) {
		if (event.ctrlKey && event.key === 's') {
			OnSaveCurrentFile();
			event.preventDefault();
		}
                else if (event.ctrlKey && event.key == ',') {
                        event.preventDefault();
                        OnStartSymbolSearch(event);
                }
		else if (event.ctrlKey && event.key == 'b') {
			event.preventDefault();
			CompileCode();
		}
		else if (event.ctrlKey && event.key == 'p') {
			event.preventDefault();
			OnStartProjectSearch();
		}
	});

	_$("txtSymbolSearch").addEvent("keydown:keys(down)", function () {
		OnSelectNextSymbol();
	});

	_$("txtSymbolSearch").addEvent("keydown:keys(up)", function () {
		OnSelectPreviousSymbol();
	});

	_$("txtSymbolSearch").addEvent("keydown:keys(enter)", function () {
		OnNavigateToSelectedSymbol();
	});

	_$("txtProjectFileSearch").addEvent("keydown:keys(down)", function () {
		OnSelectNextProjectFile();
	});

	_$("txtProjectFileSearch").addEvent("keydown:keys(up)", function () {
		OnSelectProjectFile();
	});

	_$("txtProjectFileSearch").addEvent("keydown:keys(enter)", function () {
		OnNavigateToSelectedProjectFile();
	});

	_$("txtImmediate").addEvent("keydown:keys(enter)", function () {
		OnProcessImmediate();
	});

	try {
		await GetWebRoot();

		if (!StringUtil.IsEmpty(Page.LocalSettings.Solution)) {
			ControlUtil.SetValue("txtSolution", Page.LocalSettings.Solution);
			await OnLoadProject();
			ShowTab("tab-project");

			if (!StringUtil.IsEmpty(Page.LocalSettings.File)) {
				ControlUtil.SetValue("txtFileName", Page.LocalSettings.File);
				await OnLoadFile();
			}

			await CompileCode();

			let sSymbol = Page.QueryString()["Symbol"];
			if (!StringUtil.IsEmpty(sSymbol)) {
				let oExact = Symbols.filter(x => StringUtil.EqualNoCase(x.SymbolName, sSymbol));
				if (oExact.length > 0) {
					NavigateTo(oExact[0].Info, oExact[0].SymbolName);
				}
			}
		}
	}
	catch (err) {
		Output(err);
	}





	ShowTab("tab-solution");

})

async function GetWebRoot() {
	let f = new Promise(resolve => {
		ProtoScriptWorkbench.GetWebRoot(function (sRes) {
			if (!StringUtil.IsEmpty(sRes)) {
				Page.LocalSettings.WebRoot = sRes;
				if (!StringUtil.EndsWith(sRes, "\\"))
					Page.LocalSettings.WebRoot += "\\";
			}

			resolve();
		});
	})

	await f;

	return Page.LocalSettings.WebRoot;
}

//////////////////Solution and File Loading/////////////////////////////

function BindSolutionHistory() {
	// history -------------------------------------------------------------
	const ulHist = ["ul", { class: "list-group" }];

	Page.LocalSettings.SolutionHistory.each(function (item) {
		ulHist.push([
			"li",
			{ class: "list-group-item list-group-item-action" },
			[
				"a",
				{
					click() {
						ControlUtil.SetValue("txtSolution", item);
						OnLoadProject();
					}
				},
				item
			]
		]);
	});

	$$$(ulHist).inject(_$("divSolutionHistory"));

	// examples ------------------------------------------------------------
	const examples = ["projects\\hello.pts", "projects\\Simpsons.pts"];
	const ulEx = ["ul", { class: "list-group" }];

	examples.forEach(p =>
		ulEx.push([
			"li",
			{ class: "list-group-item list-group-item-action" },
			[
				"a",
				{
					click() {
						ControlUtil.SetValue("txtSolution", p);
						OnLoadProject();
					}
				},
				p
			]
		])
	);

	const container = [
		"div",
		{},
		["h6", { class: "fw-bold" }, "Examples"],
		ulEx
	];

	$$$(container).inject(_$("divSolutionExamples"));
}


async function OnLoadProject() {
	let sProjectFile = ControlUtil.GetValue("txtSolution");
	if (!StringUtil.InString(sProjectFile, ":")) {
		sProjectFile = Page.LocalSettings.WebRoot + sProjectFile;
	}
	let sRoot = StringUtil.LeftOfLast(sProjectFile, "\\");


	Output("Loading Project: " + sProjectFile);


	let f = new Promise((resolve) => {
		ProtoScriptWorkbench.LoadProject(sProjectFile, function (oRes) {

			let allFiles = oRes.map(filePath => {
				return {
					fullPath: filePath,
					displayPath: GetRelativePath(sRoot, filePath)
				};
			});

			// Now sort the array by displayPath (case-insensitive or locale-based sort)
			allFiles.sort((a, b) => a.displayPath.toLowerCase().localeCompare(b.displayPath.toLowerCase()));

			// Create the <ul> structure
			var oUl = ["ul",
				{ class: "list-group" }
			];

			// For each file in the project, create an <li> with a tooltip showing the full path
			allFiles.each(function (obj) {
				// By default, display it as-is:
				let item = obj.fullPath;
				let displayPath = obj.displayPath;

				// Check if item is within sRoot (case-insensitive if needed)
				// If so, remove the root part plus the trailing "\"
				if (StringUtil.StartsWith(item, sRoot + "\\")) {
					// Remove the root path, leaving a relative path
					// e.g. "SomeSub\AnotherFile.pts"
					displayPath = item.substring(sRoot.length + 1);
				}
				oUl.push([
					"li",
					{ class: "list-group-item list-group-item-action" },
					[
						"a",
						{
							title: item, /* Show full path on hover */
							style: "text-decoration: none; color: #0062cc; cursor: pointer;",
							click: function () {
								ControlUtil.SetValue("txtFileName", item);
								OnLoadFile();
							}
						},
						displayPath
					]
				]);
			});

			// Inject the new list into our container
			_$("divProject").innerHTML = "";
			$$$(oUl).inject(_$("divProject"));

			ShowTab("tab-project");
                        if ($$(".navbar-header").length > 0) {
                                var header = $$(".navbar-header")[0];
                                while (header.firstChild)
                                        header.removeChild(header.firstChild);
                                var h3 = document.createElement("h3");
                                h3.style.color = "white";
                                h3.textContent = sProjectFile;
                                header.appendChild(h3);
                        }

			Page.LocalSettings.Solution = sProjectFile;
			Page.LocalSettings.SolutionHistory = QueueFront(
				Page.LocalSettings.SolutionHistory,
				sProjectFile,
				10
			);

			resolve();
		});
	});

	await f;

	Output("Project Loaded");
}



async function OnLoadFile() {
        var sFile = ControlUtil.GetValue("txtFileName");
        try {
                await LoadFile(sFile);
        }
        catch (err) {
                Output(err);
        }
}

async function LoadFile(sFile) {
	if (!IsCurrentFileSaved() && !confirm("You have unsaved changes, continue?")) {
		return false;
	}

	ControlUtil.SetValue("txtFileName", sFile);

	ShowTab("tab-file");
	_$("divFileContent").style.height = (window.innerHeight - 380) + "px";

	if (!StringUtil.IsEmpty(Page.LocalSettings.File)) {
		let cur = _$("divFileContent").scrollTop;
		if (cur > 0 && Page.LocalSettings.FileHistory.length > 0) {
			let fCurrent = Page.LocalSettings.FileHistory[0];
			if ($type(fCurrent) == "object") {
				fCurrent.Recent = [cur];
			}
		}
	}

        try {
                let f = new Promise((resolve) => {
                        ProtoScriptWorkbench.LoadFile(Page.LocalSettings.Solution, sFile, function (sFileContents) {
                                SetCode(sFileContents);
                                sLastSaved = sFileContents;
                                Page.LocalSettings.File = sFile;

                                let fCurrent = AddFileToHistory(sFile);

                                if (fCurrent.Recent.length > 0) {
                                        let cur = fCurrent.Recent[0];
                                        _$("divFileContent").scrollTop = cur;
                                }

                                BindFileHistory();
                                OnBindFileSymbols();
                                AdjustWindowSizes();
                                resolve();
                        })
                });

                await f;
        }
        catch (err) {
                Output(err);
        }

	return true;
}

function AddFileToHistory(sFile) {
	let fCurrent = { File: sFile, Recent: [] };
	let fCurrents = Page.LocalSettings.FileHistory.filter(x => StringUtil.EqualNoCase(x.File, sFile));
	if (fCurrents.length > 0) {
		fCurrent = fCurrents[0];
	}

	Page.LocalSettings.FileHistory = QueueFront(Page.LocalSettings.FileHistory, fCurrent, 500,
		function (item, x) {
			return item.File !== x.File;
		}
	);

	return fCurrent;
}

function OnBindFileSymbols() {
	let sFile = Page.LocalSettings.File;
	let oFileSymbols = Symbols.filter(x => StringUtil.EqualNoCase(x.Info.File, sFile));
	OnBindSymbols(oFileSymbols);
}


function BindFileHistory() {
	var oUl = ["ul", { class: "list-group" }]; // Use Bootstrap 5 list group for file history

	Page.LocalSettings.FileHistory.each(function (item) {
		oUl.push([
			"li",
			{ class: "list-group-item list-group-item-action" },
			[
				"a",
				{
					href: "javascript:void(0);",
					class: "text-decoration-none",
					click: function () {
						ControlUtil.SetValue("txtFileName", item.File);
						OnLoadFile();
					}
				},
				StringUtil.RightOfLast(item.File, "\\")
			]
		]);
	});

	_$("divSolution").innerHTML = ""; // Clear existing file history

	$$$(oUl).inject(_$("divSolution")); // Inject new file history

	// Remove existing dynamically created file 
	$$("#divDirective .nav-tabs .FileTab").forEach((x) => x.remove());

	// Add file history tabs dynamically
	if (Page.LocalSettings.FileHistory.length > 0) {
		$$("#divDirective .nav-tabs")[0].children[1].children[0].innerText = StringUtil.RightOfLast(
			Page.LocalSettings.FileHistory[0].File,
			"\\"
		);
	}

	for (let i = 1; i < Page.LocalSettings.FileHistory.length && i < 5; i++) {
		let item = Page.LocalSettings.FileHistory[i];

		let oLi = [
			"li",
			{ class: "nav-item FileTab" },
			[
				"a",
				{
					href: "javascript:void(0);",
					class: "nav-link",
					title: item.File,
					click: function () {
						ControlUtil.SetValue("txtFileName", item.File);
						OnLoadFile();
					}
				},
				StringUtil.RightOfLast(item.File, "\\")
			]
		];

		$$$(oLi).inject($$("#divDirective .nav-tabs")[0]);
	}
}

var sLastSaved = null;
async function OnSaveCurrentFile() {
        try {
                await SaveCurrentFile();
        }
        catch (err) {
                Output(err);
        }
}

async function SaveCurrentFile() {
        var sCode = GetCode();
        sLastSaved = sCode;

        try {
                let f = new Promise(async function (resolve) {

                        ProtoScriptWorkbench.SaveCurrentCode(Page.LocalSettings.Solution, Page.LocalSettings.File, sCode, function (oRes) {
                                UserMessages.DisplayNow("File saved", "Success")
                                resolve();
                        })
                });

                await f;
        }
        catch (err) {
                Output(err);
        }
}

///////////////////File and Symbol Panels///////////////////////////////


function OnFilterFiles() {
	var sSearch = ControlUtil.GetValue("txtFileSearch");

	$$("#divSolution li").each(function (oLi) {
		if (StringUtil.InString(oLi.innerText, sSearch))
			oLi.removeClass("hidden");
		else
			oLi.addClass("hidden");
	})

	iSelected = -1;
}



var Symbols = [];

ProtoScriptWorkbench.GetSymbols.Serialize = { Info: true };
async function OnLoadSymbols() {
	let f = new Promise(resolve => {
		ProtoScriptWorkbench.GetSymbols(Page.LocalSettings.Solution, function (oRes) {

			//Keep old symbols if the last compile failed
			if (null != oRes && oRes.length > 0)
				Symbols = oRes;

			OnBindFileSymbols();

			resolve();
		});
	});

	await f;
}

function OnBindSymbols(oSymbols) {
	var oUl = ["ul", { class: "list-group" }];

	let sCurrentFile = Page.LocalSettings.File;


	oSymbols.each(x => {
		oUl.push(["li", { class: "list-group-item list-group-item-action" }, ["a", { click: function () { NavigateTo(x.Info, x.SymbolName); } }, x.SymbolName]]);
	});

	_$("divSymbolTable").innerHTML = "";

	$$$(oUl).inject(_$("divSymbolTable"));
	$$$(["div", Symbols.length + " symbols"]).inject(_$("divSymbolTable"));
}

let oRecentNavigations = [];

async function NavigateTo(info, sSymbol, markerClass) {
	if (sSymbol) {
		info.SymbolName = sSymbol;
		oRecentNavigations = QueueFront(oRecentNavigations, info, 5);
	}

	let sFile = info.File;
	ControlUtil.SetValue("txtFileName", sFile);

	if (!await LoadFile(sFile))
		return false;

	ScrollTo(info, markerClass);
	BindRecentNavigations();

	return true;
}

function BindRecentNavigations() {
	var oUl = ["ul", { class: "list-group" }];

	oRecentNavigations.each(x => {
		oUl.push(["li", { class: "list-group-item list-group-item-action" }, ["a", { click: function () { NavigateTo(x, x.SymbolName); } }, x.SymbolName]]);
	});



	_$("divRecentSymbols").innerHTML = "";

	$$$(oUl).inject(_$("divRecentSymbols"));

}

function OnFilterProjectFiles() {
	var sSearch = ControlUtil.GetValue("txtProjectFileSearch");

	$$("#divProject li").each(function (oLi) {
		if (StringUtil.InString(oLi.innerText, sSearch)) {
			oLi.removeClass("hidden");
			oLi.addClass("nothidden");
		}
		else {
			oLi.addClass("hidden");
			oLi.removeClass("nothidden");
		}
	})

	iSelectedProjectFile = -1;
}

function OnAddNewFile() {
	Page.Modals.addFileModal.ShowContent();
}

function SubmitNewFile() {
	// Get user input from the form fields
	const fileName = document.getElementById("newFileName").value.trim();

	// If fileName is empty, show the error message and return
	if (!fileName) {
		const errorDiv = document.getElementById("addFileError");
		errorDiv.textContent = "Please provide a valid file name.";
		errorDiv.style.display = "block";
		return;
	}

	// Hide any previous errors
	document.getElementById("addFileError").style.display = "none";


	// Call your API/method to create the new file
	ProtoScriptWorkbench.CreateNewFile(Page.LocalSettings.Solution, fileName, function (res) {
		if (res) {
			UserMessages.Display("File created successfully.", "Success");
			Page.Reload();

		}
	});
}

function OnStartProjectSearch() {
	ShowTab("tab-project");
	ControlUtil.SetValue("txtProjectFileSearch", "");
	OnFilterProjectFiles();
	_$("txtProjectFileSearch").scrollIntoView(false)
	_$("txtProjectFileSearch").focus();
}

let iSelectedProjectFile = -1;
function OnSelectNextProjectFile() {
	let lst = $$("#divProject li.nothidden a");
	$$("#divProject li a").removeClass("Selected");
	if (++iSelectedProjectFile < lst.length) {
		lst[iSelectedProjectFile].addClass("Selected");
	}

}

function OnSelectProjectFile() {
	if (iSelectedProjectFile > 0)
		iSelectedProjectFile = iSelectedProjectFile - 1;

	let lst = $$("#divProject li.nothidden a");
	$$("#divProject li a").removeClass("Selected");
	lst[iSelectedProjectFile].addClass("Selected");
}

function OnNavigateToSelectedProjectFile() {
	let oA = $$("#divProject a.Selected")[0];
	if (oA)
		oA.click();
}


async function OnStartSymbolSearch(evt) {
	ShowTab("tab-symbols");
	ControlUtil.SetValue("txtSymbolSearch", "");

	if (Symbols.length == 0) {
		await CompileCode()
	}

        OnFilterSymbols(evt);
	_$("txtSymbolSearch").scrollIntoView(false)
	_$("txtSymbolSearch").focus();
}

let iSelected = -1;
function OnSelectNextSymbol() {
	let lst = $$("#divSymbolTable li a");
	$$("#divSymbolTable li a").removeClass("Selected");
	if (++iSelected < lst.length) {
		lst[iSelected].addClass("Selected");
	}

}

function OnSelectPreviousSymbol() {
	if (iSelected > 0)
		iSelected = iSelected - 1;

	let lst = $$("#divSymbolTable li a");
	$$("#divSymbolTable li a").removeClass("Selected");
	lst[iSelected].addClass("Selected");
}

function OnNavigateToSelectedSymbol() {
	let oA = $$("#divSymbolTable a.Selected")[0];
	if (oA) {
		oA.click();
		OnBindFileSymbols();
	}
}

function OnFilterSymbols(evt) {
        //(function () {
        if (!evt || (evt.code != "ArrowDown" && evt.code != "ArrowUp")) {
		var sSearch = ControlUtil.GetValue("txtSymbolSearch");
		if (StringUtil.IsEmpty(sSearch))
			OnBindFileSymbols();
		else {
			var oMatch = null;
			var oSymbols = Symbols.filter(x => StringUtil.InString(x.SymbolName, sSearch));
			var oExact = Symbols.filter(x => StringUtil.EqualNoCase(x.SymbolName, sSearch));
			if (oExact.length > 0)
				oSymbols.insertAt(0, oExact[0]);

			OnBindSymbols(oSymbols);

			iSelected = -1;
		}
	}

	//}).delay(100);
}



////////////////////////////Extensions//////////////////

function countCR(sTxt, iOffset) {
	var iCount = 0;
	for (var i = 0; i < sTxt.length && i < iOffset; i++) {
		if (sTxt.charAt(i) == '\n')
			iCount++
	}
	return iCount;
}

function OnSelectedText(evt) {
	OnOpenContextActions();
}


function ParseFile() {
	var sCode = GetCode();
	ProtoScriptWorkbench.ParseCode(sCode, function (oRes) {
		clearErrors();

		if (null != oRes)
			highlightError(oRes);
	})
}

async function CompileCode() {
        try {
                if (!IsCurrentFileSaved())
                        await SaveCurrentFile();

                Output("Compilation starting...");

                var sCode = GetCode();
                ProtoScriptWorkbench.CompileCode.onErrorReceived = function (oErr) {
                        Output("Compilation failed");
                };

                ProtoScriptWorkbench.CompileCodeWithProject.onErrorReceived = function (oErr) {
                        Output("Compilation failed");
                        Output(oErr);
                };

        let sProjectFile = ControlUtil.GetValue("txtSolution");
        if (!StringUtil.IsEmpty(sProjectFile)) {

		clearErrors();

		let f = new Promise(resolve => {
			ProtoScriptWorkbench.CompileCodeWithProject.Serialize = { Info: true }
			ProtoScriptWorkbench.CompileCodeWithProject(sCode, sProjectFile, async function (oRes) {

				let bFirst = true;
				oRes.each(function (err) {
					if (bFirst && null != err.Info) {
						bFirst = false;
						NavigateTo(err.Info);
					}

					//Something is happening inside code mirror to clear this if not delayed
					(function () {
						highlightError(err);
					}).delay(1000);

					if (null != err.Info)
						Output(err.Message + ", " + err.Info.File);
					else
						Output(err.Message);

					//					["a", { click: function () { NavigateTo(x.Info, x.SymbolName); } }, x.SymbolName ]]
				})

				Output(oRes.length + " errors");

				await OnLoadSymbols();

				resolve();
			})
		});

                await f;

        }
        else {

		ProtoScriptWorkbench.CompileCode(sCode, function (oRes) {
			clearErrors();

			oRes.each(function (err) {
				highlightError(err);
			})

			Output(oRes.length + " errors");

                })
        }

        Output("Compilation finished");
        }
        catch (err) {
                Output(err);
        }
}

function ClearOutput() {
	ControlUtil.SetValue("txtResults2", "");
}

function Output(sMessage) {
	if ($type(sMessage) == 'object')
		sMessage = JSON.stringify(sMessage, null, 2);

	ControlUtil.SetValue("txtResults2", sMessage + "\r\n" + ControlUtil.GetValue("txtResults2"));
}

function BindImmediateHistory() {
	var oUl = ["ul", { class: "list-group" }];

	Page.LocalSettings.ImmediateHistory.each(x => {
		oUl.push(["li", { class: "list-group-item list-group-item-action" }, ["a", { click: function () { ControlUtil.SetValue("txtImmediate", x) } }, x]]);
	});

	_$("divImmediateHistory").innerHTML = "";

	$$$(oUl).inject(_$("divImmediateHistory"));

}

async function OnInterpretImmediate() {
	if (!IsCurrentFileSaved())
		await SaveCurrentFile();

	let sProjectName = ControlUtil.GetValue("txtSolution");
	let sImmediate = ControlUtil.GetValue("txtImmediate").trim();
	let oOptions = BlindUnbind("divTaggingControls");
	oOptions.SessionKey = Page.LocalSettings.Solution;//StringUtil.UniqueID();
	sessionKey = oOptions.SessionKey;

	Output("Interpretting...");

	Page.LocalSettings.ImmediateHistory = QueueFront(Page.LocalSettings.ImmediateHistory, sImmediate, 50);

	if (StringUtil.IsEmpty(oOptions.MaxIterations))
		oOptions.MaxIterations = 100;

	if (oOptions.Debug) {
		oOptions.Breakpoints = Breakpoints;
		OnStartDebugging();
	}

	console.log(oOptions);

	ProtoScriptWorkbench.InterpretImmediate.onErrorReceived = function (oErr) {
		OnStopDebugging();
		Output(oErr.Error);
		if (oErr.ErrorStatement)
			NavigateTo(oErr.ErrorStatement);
	};

	ProtoScriptWorkbench.InterpretImmediate(sProjectName, sImmediate, oOptions, function (sRes) {
		OnStopDebugging();
		Output(sRes.Result);
		BindImmediateHistory();
	});
}

function OnResetProtoScript() {
	ProtoScriptWorkbench.Reset(function () {
		UserMessages.DisplayNow("Reset", "Success");
	});
}

function OnExecuteSelected() {
	let sSelected = getSelectedText();
	ControlUtil.SetValue("txtImmediate", sSelected);
	OnInterpretImmediate();
}

async function OnProcessImmediate() {
	let sImmediate = ControlUtil.GetValue("txtImmediate").trim();

	if (StringUtil.EndsWith(sImmediate, ")") && sImmediate.contains("(")) {
		OnInterpretImmediate();
	}

	else {
		await OnTagImmediate();
	}
}

var bIsTagging = false;
var timerUpdate = null;
async function OnTagImmediate(ctrl) {

	if (bIsTagging) {

		ProtoScriptWorkbench.StopTagging(sessionKey);

		ctrl.children[0].className = ctrl.children[0].className.replace("bi-arrow-repeat bi-spin", "bi-tag");
		bIsTagging = false;
		return;
	}

	bIsTagging = true;
	let sImmediate = ControlUtil.GetValue("txtImmediate").trim();
	let sProjectName = ControlUtil.GetValue("txtSolution");
	let oOptions = BlindUnbind("divTaggingControls");

	sessionKey = Page.LocalSettings.Solution;//StringUtil.UniqueID();
	oOptions.SessionKey = sessionKey;

	Page.LocalSettings.ImmediateHistory = QueueFront(Page.LocalSettings.ImmediateHistory, sImmediate, 50);

	ClearOutput();

	if (oOptions.Resume && oOptions.Debug) {
		Output("Can't resume and debug together");
	}

	if (!IsCurrentFileSaved())
		await SaveCurrentFile();

	ProtoScriptWorkbench.TagImmediate.onErrorReceived = function (oErr) {
		clearInterval(timerUpdate);
		Output(oErr.Error);
		if (oErr.ErrorStatement)
			NavigateTo(oErr.ErrorStatement);
	};

	ctrl.children[0].className = ctrl.children[0].className.replace("bi-tag", "bi-arrow-repeat bi-spin");

	timerUpdate = setInterval(function () {
		ProtoScriptWorkbench.GetTaggingProgress(sessionKey, function (progress) {
			if (progress) {
				_$("txtResults2").value = progress.Iterations + ":\r\n " + progress.CurrentInterpretation;
			}
		});
	}, 1000);

	ProtoScriptWorkbench.TagImmediate(sImmediate.trim(), sProjectName, oOptions, function (oRes) {

		bIsTagging = false;
		ctrl.children[0].className = ctrl.children[0].className.replace("bi-arrow-repeat bi-spin", "bi-tag")
		clearInterval(timerUpdate);

		if (!StringUtil.IsEmpty(oRes.Result)) {
			Output(oRes.Result);
		}
		else if (!StringUtil.IsEmpty(oRes.Error)) {
			Output(oRes.Error);
			NavigateTo(oRes.ErrorStatement);
		}
		BindImmediateHistory();
	});
}

function GetRelativePath(sRoot, sFile) {


	// Split both root and file paths into segments
	const rootParts = sRoot.split("\\");
	const fileParts = sFile.split("\\");

	// Find the longest common prefix
	let i = 0;
	while (i < rootParts.length && i < fileParts.length &&
		rootParts[i].toLowerCase() === fileParts[i].toLowerCase()) {
		i++;
	}

	// Number of leftover segments in root => how many "../" we need
	const leftoverFromRoot = rootParts.length - i;

	// Build the relative path
	// Step 1: add '..\' for each leftover from root
	let relativePathSegments = [];
	for (let k = 0; k < leftoverFromRoot; k++) {
		relativePathSegments.push("..");
	}

	// Step 2: add the leftover file parts
	for (let x = i; x < fileParts.length; x++) {
		relativePathSegments.push(fileParts[x]);
	}

	// Join them back using "\"
	// e.g. ['..','SubFolder','File.pts'] => '..\SubFolder\File.pts'
	let relativePath = relativePathSegments.join("\\");

	// If the file is the same as the root or somehow ends up empty, 
	// fallback to the last known segment
	if (!relativePath) {
		relativePath = fileParts[fileParts.length - 1];
	}

	return relativePath;
}





function OnOpenContextActions() {
	let text = getSelectedText();
	let oExact = Symbols.filter(x => StringUtil.EqualNoCase(x.SymbolName, text));
	let bSymbolFound = oExact.length > 0;
	if (!bSymbolFound) {
		oExact = Symbols.filter(x => StringUtil.InString(x.SymbolName, text));
		if (oExact.length > 0) {
			text = oExact[0].SymbolName;
			bSymbolFound = true;
		}
	}

	if (IsDebugging()) {
		OpenDebugContext(text);
	}
	else if (bSymbolFound) {
		OpenContextActions(text);

		$$("#divContextActions button").addClass("hidden");

		if (oExact.length > 0) {
			_$("btnNavigateToHighlightedSymbol").removeClass("hidden");
		}
		else {
			_$("btnNavigateToHighlightedSymbol").addClass("hidden");
		}

		let bMultiToken = text.contains(" ");
		let bFunctionCall = text.contains("(") && text.contains(")");

		if (bMultiToken && !bFunctionCall)
			_$('btnGenerateSequence').removeClass("hidden");
		else if (!bFunctionCall) {
			_$('btnGenerateVerb').removeClass("hidden");
			_$('btnGenerateLexeme').removeClass("hidden");
		}

		if (bFunctionCall) {
			_$('btnExecuteSelected').removeClass("hidden");
		}
	}
	else if (oExact.length > 0)
		OpenContextActions(text);

	else {
		$$("#divContextActions button").addClass("hidden");
		_$('btnOpenChatWindow').removeClass("hidden")
		OpenContextActions(text, true);
	}
}

function OnNavigateToHighlightedSymbol() {
	let sSymbol = _$("spanContextActions").innerText;
	let oExact = Symbols.filter(x => StringUtil.EqualNoCase(x.SymbolName, sSymbol));
	if (oExact.length > 0) {
		NavigateTo(oExact[0].Info, oExact[0].SymbolName);
	}

}

function OpenContextActions(text, bChatOnly) {
	if (!_$("divContextActions").hasClass('hidden'))
		return;

	let coords = window.editor.cursorCoords(true);
	if (bChatOnly)
		_$("spanContextActions").innerText = "";
	else
		_$("spanContextActions").innerText = text;

	_$("divContextActions").removeClass("hidden");
	_$("divContextActions").style.left = coords.left + "px";
	_$("divContextActions").style.top = (coords.top + 20) + "px";
	_$("divSymbolInfo").innerText = "";

	let oExact = Symbols.filter(x => StringUtil.EqualNoCase(x.SymbolName, text));
	if (oExact.length > 0) {
		ProtoScriptWorkbench.GetSymbolInfo(Page.LocalSettings.Solution, text, oExact[0].Info, function (res) {
			(function () {
				_$("divSymbolInfo").innerText = res;
			}).delay(1000);//don't show right away
		});
	}


}

function OpenDebugContext(text) {
	let coords = window.editor.cursorCoords(true);
	_$("spanDebugContext").innerText = text;
	_$("divDebugContext").removeClass("hidden");
	_$("divDebugContext").style.left = coords.left + "px";
	_$("divDebugContext").style.top = (coords.top + 20) + "px";
	_$("divQuickWatch").innerText = "";
	ProtoScriptWorkbench.GetSymbol(sessionKey, text, function (res) {
		_$("divQuickWatch").innerText = res;
	});
}

function OnCloseContextActions() {
	_$("divContextActions").addClass("hidden");
}

function OnCloseDebugContext() {
	_$("divDebugContext").addClass("hidden");
}

function OnEnter(cm) {
	let cur = cm.getCursor();
	let line = cm.getLine(cur.line);

	if (!StringUtil.IsEmpty(line)) {
		let splits = StringUtil.Split(line, " ");

		if (splits[0] == "linknaked") {
			ProtoScriptWorkbench.GenerateLinkNaked(splits[1], splits[2], function (sRes) {
				ReplaceCurrentLine(sRes);
			});
		}
	}
	else {
		OnPredictNextLine();
	}
}

function ProcessCodeOperation(op) {
	if (op.Type == "Insert") {
		if (op.Info) {
			InsertAtOffset(op.Code, op.Info);
		}
		else {
			ReplaceCurrentLine(op.Code);
		}
	}
}


var Breakpoints = [];

function SetBreakPoint(info) {
	console.log(info);
	Breakpoints.push({
		StartingOffset: window.editor.indexFromPos(CodeMirror.Pos(info.line, 0)),
		Length: info.text.length,
		File: Page.LocalSettings.File,
		Line: info.line
	});
}

function RemoveBreakPoint(info) {
	Breakpoints = Breakpoints.filter(x => x.Line != info.line);
	console.log(info);
}

function OnStartDebugging() {
	$$(".DebuggingControls").removeClass("hidden");
	oCurrentBlockedOn = null;
	OnBlockedOn.periodical(1000);
	OnSockets.periodical(1000);
	iSocketMessageID = 0;
}

function OnStopDebugging() {
	$$(".DebuggingControls").addClass("hidden");
	if (BrokeOnMarker)
		BrokeOnMarker.clear();
	Breakpoints = [];
}
function OnResumeDebugging() {
	ProtoScriptWorkbench.Resume();
}

function IsDebugging() {
	return !($$(".DebuggingControls")[0].hasClass("hidden"));
}

var oCurrentBlockedOn = null;

function OnBlockedOn() {
	if (IsDebugging()) {
		ProtoScriptWorkbench.GetBlockedOn(function (oInfo) {
			if (null != oInfo && (null == oCurrentBlockedOn || oInfo.StartingOffset != oCurrentBlockedOn.StartingOffset)) {
				oCurrentBlockedOn = oInfo;
				NavigateToDebugging(oInfo, null, 'debug-break-marker');

				let sStack = ProtoScriptWorkbench.GetCallStack();
				sStack += "\r\n" + ProtoScriptWorkbench.GetCurrentException();

				ControlUtil.SetValue("txtResults2", sStack);
			}
		})
	}
}

function OnStepNext() {
	if (BrokeOnMarker)
		BrokeOnMarker.clear();
	ProtoScriptWorkbench.StepNext();
}

function OnStepOver() {
	if (BrokeOnMarker)
		BrokeOnMarker.clear();
	ProtoScriptWorkbench.StepOver();
}

function OnForceStopDebugging() {
	ProtoScriptWorkbench.StopDebugging();
}


async function NavigateToDebugging(info, sSymbol, markerClass) {
	if (BrokeOnMarker)
		BrokeOnMarker.clear();

	if (sSymbol) {
		info.SymbolName = sSymbol;
		oRecentNavigations = QueueFront(oRecentNavigations, info, 5);
	}

	let sFile = info.File;
	ControlUtil.SetValue("txtFileName", sFile);


	await LoadFile(sFile);
	ScrollToDebugging(info, markerClass);
	BindRecentNavigations();
}

var BrokeOnMarker = null;

function ScrollToDebugging(info, markerClass) {
	let lineStart = editor.posFromIndex(info.StartingOffset);
	let lineStop = editor.posFromIndex(info.StartingOffset + Math.max(info.Length, 2));

	ScrollToLine(lineStart.line);

	//editor.scrollIntoView({ line: lineStart.line, char: lineStart.ch }, 500);
	BrokeOnMarker = editor.markText(lineStart, lineStop, { clearOnEnter: true, className: markerClass == null ? 'found-text-marker' : markerClass })
}
