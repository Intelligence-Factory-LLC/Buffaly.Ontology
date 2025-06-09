	const consoleOutput = document.getElementById("txtResults2"); // Unified console output
	const immediateCommandInput = document.getElementById("txtImmediate");
	const runImmediateCmdBtn = document.getElementById("runImmediateCmdBtn");
	const headerProjectName = document.getElementById("headerProjectName");
	const fileList = document.getElementById("divProject");
	const symbolList = document.getElementById("divSymbolTable");
	const searchFilesInput = document.getElementById("txtProjectFileSearch");
	const searchSymbolsInput = document.getElementById("txtSymbolSearch");
	const fileHistoryList = document.getElementById("divSolution");
	const searchHistoryInput = document.getElementById("txtFileSearch");
	const immediateHistoryDiv = document.getElementById("divImmediateHistory");
	const taggingProgressSpan = document.getElementById("txtTaggingProgress");
	const btnMaximize = document.getElementById("btnMaximize");
	const btnHalfize = document.getElementById("btnHalfize");
	const btnMinimize = document.getElementById("btnMinimize");

	let recentNavigations = [];
	let selectedSymbolIdx = -1;
	let filteredSymbols = [];
	let selectedProjectFileIdx = -1;
	let filteredProjectFiles = [];

	const LocalSettings = JSON.parse(localStorage.getItem("EditorSettings") || "{}");
	if (!LocalSettings.FileHistory) LocalSettings.FileHistory = [];
	if (!LocalSettings.ImmediateHistory) LocalSettings.ImmediateHistory = [];
	if (!LocalSettings.WindowSize) LocalSettings.WindowSize = "Halfize";
	let sLastSaved = null;
	let editor;
	let sessionKey = null;
	let bIsTagging = false;
	let timerUpdate = null;
	let currentProjectName = "Untitled Project";
	let currentFileSymbols = [];

	function updateTitle() {
	document.title = `SemDB IDE - ${currentProjectName}`;
	headerProjectName.textContent = `- ${currentProjectName}`;
}
updateTitle();

	function saveLocalSettings() {
localStorage.setItem("EditorSettings", JSON.stringify(LocalSettings));
}

	function queueFront(arr, item, max) {
	const idx = arr.findIndex(x => JSON.stringify(x) === JSON.stringify(item));
	if (idx !== -1) arr.splice(idx, 1);
arr.unshift(item);
	if (arr.length > max) arr.pop();
	return arr;
}

	function getRelativePath(root, path) {
	return path.toLowerCase().startsWith(root.toLowerCase() + "\\") ? path.substring(root.length + 1) : path;
}

	function createFileItem(fullPath, displayPath) {
	const fileDiv = document.createElement("div");
fileDiv.className = "file-item p-1.5 rounded-md cursor-pointer text-gray-700";
fileDiv.textContent = displayPath || fullPath;
fileDiv.title = fullPath;
fileDiv.onclick = () => {
document.getElementById("txtFileName").value = fullPath;
OnLoadFile();
highlightActiveFile(fileDiv);
};
	return fileDiv;
}

	function addFileToHistory(file) {
	let existing = LocalSettings.FileHistory.find(x => x.File && x.File.toLowerCase() === file.toLowerCase());
	if (!existing) existing = { File: file };
LocalSettings.FileHistory = queueFront(LocalSettings.FileHistory, existing, 50);
saveLocalSettings();
	return existing;
}

	function bindFileHistory() {
	if (!fileHistoryList) return;
fileHistoryList.innerHTML = "";
LocalSettings.FileHistory.forEach(item => {
	const div = createFileItem(item.File);
fileHistoryList.appendChild(div);
});
}

	function bindImmediateHistory() {
	if (!immediateHistoryDiv) return;
       immediateHistoryDiv.innerHTML = "";
       LocalSettings.ImmediateHistory.forEach(cmd => {
	const div = document.createElement("div");
               div.className = "cursor-pointer px-1 hover:bg-gray-100";
               div.textContent = cmd;
               div.onclick = () => {
                       immediateCommandInput.value = cmd;
               };
               immediateHistoryDiv.appendChild(div);
       });
}

	function isCurrentFileSaved() {
	return !sLastSaved || editor.getValue() === sLastSaved;
}

	function appendToConsole(message, type = "LOG") {
	const timestamp = new Date().toLocaleTimeString();
	let prefix = "";
	if (type === "CMD") {
		prefix = "CMD> ";
	} else if (type === "RESULT") {
		prefix = "=> ";
	} else if (type === "WARN") {
		prefix = "WARN: ";
	} else if (type === "ERROR") {
		prefix = "ERROR: ";
	} else if (type === "INFO") {
		prefix = "INFO: ";
	}
	// For 'LOG', no specific prefix, just the timestamped message

	consoleOutput.value += `[${timestamp}] ${prefix}${message}\n`;
        consoleOutput.scrollTop = consoleOutput.scrollHeight;
}

	function ClearOutput() {
       consoleOutput.value = "";
}

	function Output(msg) {
	appendToConsole(msg, "RESULT");
}

	function isLetter(ch) {
	return ch.length === 1 && /[a-z]/i.test(ch);
}

	function isNumber(ch) {
	return ch.length === 1 && /\d/.test(ch);
}

	function OnPredictNextLine(cm) {
	const doc = editor.getDoc();
	const cursor = doc.getCursor();
	const pos = doc.indexFromPos(cursor);
	const before = doc.getRange({ line: 0, ch: 0 }, cursor);
	const after = doc.getRange(cursor, { line: Infinity, ch: Infinity });
	appendToConsole("Thinking", "INFO");
	ProtoScriptWorkbench.PredictNextLine(pos, function(res) {
	appendToConsole(res.length + " results found", "INFO");
	if (res && res.length > 0) {
	const newPart = StringUtil.ReplaceAll(res[0], "\n", "\r\n");
	editor.setValue(before + "\r\n" + newPart + "\r\n" + after);
}
});
}

	function OnSuggest(cm) {
	const cursor = cm.getCursor();
	const lineText = cm.getLine(cursor.line).substr(0, cursor.ch);
	if (StringUtil.InString(lineText, "//")) return null;
	const pos = cm.indexFromPos(cursor);
	let results = [];
	let offset = lineText.length;
	let search = "";
	const lastSavedOffset = GetCode().length - (sLastSaved ? sLastSaved.length : 0);
	if (!StringUtil.IsEmpty(lineText)) {
	let type = "";
for (let i = lineText.length - 1; i >= 0; i--) {
	const ch = lineText[i];
	if (isLetter(ch) || isNumber(ch) || ch === "_") {
search = ch + search;
offset--;
} else if (ch === ".") {
type = "SubObject";
break;
} else if (ch === "(" || ch === ",") {
type = "Parameter";
break;
} else {
type = "Symbol";
break;
}
}
	let res2 = [];
	if (!StringUtil.IsEmpty(search) && ["Symbol", "Parameter"].includes(type)) {
res2 = ProtoScriptWorkbench.GetSymbolsAtCursor(LocalSettings.Solution, LocalSettings.File, pos - lastSavedOffset) || [];
res2 = res2.filter(x => StringUtil.StartsWith(x.SymbolName, search));
}
	let res3 = [];
	if (type === "SubObject" && !StringUtil.IsEmpty(lineText)) {
res3 = ProtoScriptWorkbench.Suggest(LocalSettings.Solution, lineText, pos) || [];
}
	let res = [];
	if (!StringUtil.IsEmpty(search) && ["Symbol", "Parameter"].includes(type)) {
res = Symbols.filter(x => StringUtil.StartsWith(x.SymbolName, search));
}
for (let i = 0; i < res3.length; i++) results.push(res3[i].SymbolName);
for (let i = 0; i < res2.length; i++) results.push(res2[i].SymbolName);
for (let i = 0; i < res.length && i < 10; i++) results.push(res[i].SymbolName);
}
	if (results.length > 0) {
	return {
list: results,
from: CodeMirror.Pos(cursor.line, offset),
to: CodeMirror.Pos(cursor.line, offset + search.length)
};
}
	return null;
}

	const ExcludedIntelliSenseTriggerKeys = {
"8": "backspace",
"9": "tab",
"13": "enter",
"16": "shift",
"17": "ctrl",
"18": "alt",
"19": "pause",
"20": "capslock",
"27": "escape",
"33": "pageup",
"34": "pagedown",
"35": "end",
"36": "home",
"37": "left",
"38": "up",
"39": "right",
"40": "down",
"45": "insert",
"46": "delete",
"91": "left window key",
"92": "right window key",
"93": "select",
"107": "add",
"109": "subtract",
"110": "decimal point",
"111": "divide",
"112": "f1",
"113": "f2",
"114": "f3",
"115": "f4",
"116": "f5",
"117": "f6",
"118": "f7",
"119": "f8",
"120": "f9",
"121": "f10",
"122": "f11",
"123": "f12",
"144": "numlock",
"145": "scrolllock",
"186": "semicolon",
"187": "equalsign",
"188": "comma",
"189": "dash",
"191": "slash",
"192": "graveaccent",
"220": "backslash",
"222": "quote"
};

	editor = CodeMirror.fromTextArea(document.getElementById("codeEditor"), {
mode: "text/x-csharp",
indentWithTabs: true,
smartIndent: true,
lineNumbers: true,
matchBrackets: true,
autoCloseBrackets: true,
lineWrapping: true,
lineSeparator: "\r\n",
newline: "crlf",
extraKeys: { "Ctrl-Space": "autocomplete", "Ctrl-Enter": OnEnter, "Alt-Space": OnPredictNextLine },
hintOptions: { hint: OnSuggest, completeSingle: false },
gutters: ["gutter-error", "breakpoints", "CodeMirror-linenumbers"]
});
	editor.on("gutterClick", function(cm, n) {
	const info = cm.lineInfo(n);
	if (info.gutterMarkers) {
cm.setGutterMarker(n, "breakpoints", null);
RemoveBreakPoint(info);
} else {
cm.setGutterMarker(n, "breakpoints", makeBreakpoint());
SetBreakPoint(info);
}
});
	editor.on("keyup", function(cm, event) {
	if (!cm.state.completionActive && !ExcludedIntelliSenseTriggerKeys[(event.keyCode || event.which).toString()]) {
CodeMirror.commands.autocomplete(cm, null, { completeSingle: false });
}
});

function makeBreakpoint() {
	const marker = document.createElement("div");
	marker.className = "breakpoint";
	marker.innerHTML = "●";
	return marker;
}
bindFileHistory();
bindImmediateHistory();

	function OnMaximize() {
outputWindow.style.height = '90vh';
LocalSettings.WindowSize = 'Maximize';
saveLocalSettings();
}

	function OnHalfize() {
outputWindow.style.height = '50vh';
LocalSettings.WindowSize = 'Halfize';
saveLocalSettings();
}

	function OnMinimize() {
outputWindow.style.height = '150px';
LocalSettings.WindowSize = 'Minimize';
saveLocalSettings();
}

	function AdjustWindowSizes() {
	if (LocalSettings.WindowSize === 'Maximize') OnMaximize();
else if (LocalSettings.WindowSize === 'Minimize') OnMinimize();
else OnHalfize();
}

	function switchTab(panePrefix, tabIdToShow) {
	document
		.querySelectorAll(`button[data-pane='${panePrefix}']`)
		.forEach((button) => {
			button.classList.remove("active");
		});
	document
		.querySelectorAll(`.${panePrefix}-tab-content`)
		.forEach((content) => {
			content.classList.remove("active");
		});
	const activeButton = document.querySelector(
		`button[data-pane='${panePrefix}'][data-tab='${tabIdToShow}']`,
	);
	if (activeButton) {
		activeButton.classList.add("active");
	}
	const activeTabContent = document.getElementById(tabIdToShow);
	if (activeTabContent) {
		activeTabContent.classList.add("active");
	}
}

document.querySelectorAll(".tab-button").forEach((button) => {
	button.addEventListener("click", () => {
		const pane = button.dataset.pane;
		const tabId = button.dataset.tab;
		switchTab(pane, tabId);
		const tabNameFriendly = button.textContent;
		appendToConsole(
			`Switched to ${pane} panel, tab: ${tabNameFriendly}`,
			"DEBUG",
		); // Log to unified console
	});
});

switchTab("right", "solutionExplorerTab");
switchTab("bottom", "consoleContent");

	function highlightActiveFile(selectedItem) {
	document
.querySelectorAll("#divProject .file-item")
		.forEach((item) => item.classList.remove("active"));
	if (selectedItem) selectedItem.classList.add("active");
}

	function parseAndDisplaySymbols(fileName, codeContent) {
	currentFileSymbols = [];
	symbolList.innerHTML = "";
	const functionRegex = /function\s+([a-zA-Z0-9_]+)\s*\(/g;
	const varRegex = /(?:var|let|const)\s+([a-zA-Z0-9_]+)/g;
	let match;
	const fileTitle = document.createElement("p");
	fileTitle.className =
		"text-xs text-gray-500 px-1 pt-1 pb-0.5 font-semibold";
	fileTitle.textContent = `Symbols in ${fileName.split("\\").pop().split("/").pop()}:`;
	symbolList.appendChild(fileTitle);

	while ((match = functionRegex.exec(codeContent)) !== null) {
		const matchStartPos = editor.posFromIndex(match.index);
		currentFileSymbols.push({
			name: match[1],
			type: "function",
			line: matchStartPos.line + 1,
		});
	}
	while ((match = varRegex.exec(codeContent)) !== null) {
		if (!["var", "let", "const"].includes(match[1])) {
			const matchStartPos = editor.posFromIndex(match.index);
			currentFileSymbols.push({
				name: match[1],
				type: "variable",
				line: matchStartPos.line + 1,
			});
		}
	}

	if (fileName.includes("Base.pts")) {
		const baseObjIndex = codeContent.indexOf("BaseObject");
		const initIndex = codeContent.indexOf("InitializeSystem");
		if (baseObjIndex !== -1)
			currentFileSymbols.push({
				name: "BaseObject",
				type: "class",
				line: editor.posFromIndex(baseObjIndex).line + 1,
			});
		if (initIndex !== -1)
			currentFileSymbols.push({
				name: "InitializeSystem",
				type: "method",
				line: editor.posFromIndex(initIndex).line + 1,
			});
	}
	if (fileName.includes("Articles.pts")) {
		const articleIndex = codeContent.indexOf("Article");
		const fetchIndex = codeContent.indexOf("fetchContent");
		const displayIndex = codeContent.indexOf("displayArticle");
		if (articleIndex !== -1)
			currentFileSymbols.push({
				name: "Article",
				type: "class",
				line: editor.posFromIndex(articleIndex).line + 1,
			});
		if (fetchIndex !== -1)
			currentFileSymbols.push({
				name: "fetchContent",
				type: "method",
				line: editor.posFromIndex(fetchIndex).line + 1,
			});
		if (displayIndex !== -1)
			currentFileSymbols.push({
				name: "displayArticle",
				type: "method",
				line: editor.posFromIndex(displayIndex).line + 1,
			});
	}

	if (currentFileSymbols.length === 0) {
		const noSymbolsMsg = document.createElement("p");
		noSymbolsMsg.className = "text-gray-500 text-xs p-1";
		noSymbolsMsg.textContent = "No symbols found in this file.";
		symbolList.appendChild(noSymbolsMsg);
	} else {
		renderSymbolList(currentFileSymbols);
	}
}

	function getSymbolIcon(type) {
	let iconSvg = "";
	switch (type) {
		case "function":
			iconSvg = `<svg viewBox="0 0 16 16" fill="#805AD5" class="symbol-icon"><path d="M4 14V2h5.5l3.5 3.5V14H4zm1-1h7V6H9V3H5v10zM6.5 9H8V7.5h2V9h1.5v1H10v1.5H8V13H6.5v-1.5H5V10h1.5V9z"/></svg>`;
			break;
		case "variable":
			iconSvg = `<svg viewBox="0 0 16 16" fill="#3182CE" class="symbol-icon"><path d="M4 2v12h8V2H4zm1 1h6v2L8 7l-3-2V3zm0 5l3 2 3-2v5H5V8z"/></svg>`;
			break;
		case "class":
			iconSvg = `<svg viewBox="0 0 16 16" fill="#DD6B20" class="symbol-icon"><path d="M8 1a7 7 0 100 14A7 7 0 008 1zm0 1.5a5.5 5.5 0 014.606 8.036L5.964 3.964A5.466 5.466 0 018 2.5zM3.964 12.036L10.036 5.964A5.5 5.5 0 013.964 12.036z"/></svg>`;
			break;
		case "method":
			iconSvg = `<svg viewBox="0 0 16 16" fill="#319795" class="symbol-icon"><path d="M3 3h10v1H3V3zm0 2h10v1H3V5zm0 3h4v1H3V8zm0 2h4v1H3v-1zm5-2h5v1H8V8zm0 2h5v1H8v-1zM3 13h10v1H3v-1z"/></svg>`;
			break;
		default:
			iconSvg = `<svg viewBox="0 0 16 16" fill="#718096" class="symbol-icon"><circle cx="8" cy="8" r="3"/></svg>`;
	}
	return iconSvg;
}

	function renderSymbolList(symbolsToRender) {
	symbolList
		.querySelectorAll(".symbol-item, .no-symbols-message")
		.forEach((el) => el.remove());
	if (
		symbolsToRender.length === 0 &&
		symbolList.querySelector(
			".text-xs.text-gray-500.px-1.pt-1.pb-0\\.5.font-semibold",
		) === null
	) {
		const noSymbolsMsg = document.createElement("p");
		noSymbolsMsg.className = "text-gray-500 text-xs p-1 no-symbols-message";
		noSymbolsMsg.textContent =
			"No symbols match your search or in the current file.";
		symbolList.appendChild(noSymbolsMsg);
		return;
	}
	if (
		symbolsToRender.length === 0 &&
		searchSymbolsInput.value.trim() !== ""
	) {
		const noMatchMsg = document.createElement("p");
		noMatchMsg.className = "text-gray-500 text-xs p-1 no-symbols-message";
		noMatchMsg.textContent = "No symbols match your search.";
		symbolList.appendChild(noMatchMsg);
		return;
	}
	symbolsToRender.forEach((symbol) => {
		const item = document.createElement("div");
		item.className = "symbol-item";
		item.innerHTML = `${getSymbolIcon(symbol.type)} <span class="symbol-name">${symbol.name}</span> <span class="text-gray-400 ml-auto text-xxs">L:${symbol.line}</span>`;
		item.onclick = () => {
			appendToConsole(
				`Symbol clicked: ${symbol.name} (Type: ${symbol.type}, Line: ${symbol.line})`,
				"DEBUG",
			);
			if (typeof symbol.line === "number" && symbol.line > 0) {
				editor.setCursor({ line: symbol.line - 1, ch: 0 });
				editor.focus();
				const lineHandle = editor.getLineHandle(symbol.line - 1);
				if (lineHandle) {
					editor.addLineClass(
						lineHandle,
						"background",
						"bg-yellow-200",
					);
					setTimeout(() => {
						editor.removeLineClass(
							lineHandle,
							"background",
							"bg-yellow-200",
						);
					}, 1500);
				}
			} else {
				appendToConsole(
					`Invalid line number for symbol ${symbol.name}: ${symbol.line}`,
					"WARN",
				);
			}
		};
               symbolList.appendChild(item);
       });

       filteredSymbols = symbolsToRender.slice();
       selectedSymbolIdx = -1;
}

async function OnLoadProject() {
	const projectPath = document.getElementById("txtSolution").value.trim();
	if (!projectPath) {
	appendToConsole("Project path cannot be empty.", "WARN");
	return;
}
	appendToConsole(`Loading project: ${projectPath}`, "INFO");
	const root = projectPath.substring(0, projectPath.lastIndexOf("\\"));
	const files = await new Promise(r => ProtoScriptWorkbench.LoadProject(projectPath, r));
fileList.innerHTML = "";
	let firstDiv = null;
files.forEach((f, i) => {
	const div = createFileItem(f, getRelativePath(root, f));
fileList.appendChild(div);
	if (i === 0) firstDiv = div;
});
filterProjectFiles();
currentProjectName = projectPath.split("\\").pop().split("/").pop();
updateTitle();
LocalSettings.Solution = projectPath;
saveLocalSettings();
	let fileToOpen = LocalSettings.File || (files.length > 0 ? files[0] : null);
	if (fileToOpen) {
document.getElementById("txtFileName").value = fileToOpen;
await OnLoadFile();
	const item = Array.from(fileList.querySelectorAll('.file-item')).find(d => d.title && d.title.toLowerCase() === fileToOpen.toLowerCase());
	if (item) highlightActiveFile(item);
}
}

async function OnLoadFile() {
	const file = document.getElementById("txtFileName").value;
	if (!isCurrentFileSaved() && !confirm("You have unsaved changes, continue?")) return;
	const content = await new Promise(r => ProtoScriptWorkbench.LoadFile(LocalSettings.Solution, file, r));
	editor.setValue(content);
sLastSaved = content;
parseAndDisplaySymbols(file, content);
LocalSettings.File = file;
addFileToHistory(file);
bindFileHistory();
saveLocalSettings();
}

async function SaveCurrentFile() {
	const code = editor.getValue();
sLastSaved = code;
await new Promise(r => ProtoScriptWorkbench.SaveCurrentCode(LocalSettings.Solution, LocalSettings.File, code, r));
	appendToConsole("File saved.", "INFO");
}

	function OnSaveCurrentFile() {
SaveCurrentFile();
}

	function clearErrors() {
	editor.clearGutter("gutter-error");
}

	function makeMarker(msg) {
	const marker = document.createElement("div");
	marker.classList.add("error-marker");
	marker.innerHTML = "&nbsp;";
	const error = document.createElement("div");
error.innerHTML = msg;
error.classList.add("error-message");
	marker.appendChild(error);
	return marker;
}

	function highlightError(error) {
	if (error && error.Info && error.Info.File && LocalSettings.File &&
error.Info.File.toLowerCase() === LocalSettings.File.toLowerCase()) {
	const line = editor.posFromIndex(error.Info.StartingOffset).line;
	const marker = makeMarker(`(Error ${error.Type}) ${error.Message}`);
	editor.setGutterMarker(line, "gutter-error", marker);
}
}

async function CompileCode() {
try {
	if (!isCurrentFileSaved())
await SaveCurrentFile();
	appendToConsole("Compilation starting...", "INFO");
	const code = editor.getValue();
	ProtoScriptWorkbench.CompileCode.onErrorReceived = () => {
	appendToConsole("Compilation failed", "ERROR");
};
	ProtoScriptWorkbench.CompileCodeWithProject.onErrorReceived = (err) => {
	appendToConsole("Compilation failed", "ERROR");
	appendToConsole(JSON.stringify(err), "ERROR");
};
	const projectFile = LocalSettings.Solution;
	if (projectFile) {
clearErrors();
await new Promise((resolve) => {
	ProtoScriptWorkbench.CompileCodeWithProject.Serialize = { Info: true };
	ProtoScriptWorkbench.CompileCodeWithProject(code, projectFile, (res) => {
	let first = true;
res.forEach((err) => {
	if (first && err.Info) {
first = false;
NavigateTo(err.Info);
}
setTimeout(() => highlightError(err), 1000);
	if (err.Info)
	appendToConsole(`${err.Message}, ${err.Info.File}`, "ERROR");
else
	appendToConsole(err.Message, "ERROR");
});
	appendToConsole(`${res.length} errors`, "INFO");
resolve();
});
});
} else {
	ProtoScriptWorkbench.CompileCode(code, (res) => {
clearErrors();
res.forEach((err) => highlightError(err));
	appendToConsole(`${res.length} errors`, "INFO");
});
}
	appendToConsole("Compilation finished", "INFO");
} catch (err) {
	appendToConsole(err.toString(), "ERROR");
}
}

	function ScrollToLine(line) {
	editor.scrollIntoView({ line, ch: 0 }, 200);
}

	function ScrollTo(info) {
	if (typeof info.line === "number") {
		ScrollToLine(info.line - 1);
		editor.setCursor({ line: info.line - 1, ch: 0 });
	} else if (typeof info.StartingOffset === "number") {
		const pos = editor.posFromIndex(info.StartingOffset);
		ScrollToLine(pos.line);
		editor.setCursor(pos);
	}
}

	function bindRecentNavigations() {
	const container = document.getElementById("divRecentSymbols");
	if (!container) return;
	container.innerHTML = "";
	recentNavigations.forEach((nav) => {
		const div = document.createElement("div");
		div.className = "symbol-item";
		div.textContent = nav.SymbolName;
		div.onclick = () => NavigateTo(nav, nav.SymbolName);
		container.appendChild(div);
	});
}

	function NavigateTo(info, symbolName) {
	if (symbolName) {
info.SymbolName = symbolName;
recentNavigations = queueFront(recentNavigations, info, 5);
}
document.getElementById("txtFileName").value = info.File;
OnLoadFile().then(() => {
ScrollTo(info);
bindRecentNavigations();
});
}

	function startSymbolSearch() {
switchTab("right", "symbolsTab");
searchSymbolsInput.value = "";
searchSymbolsInput.dispatchEvent(new Event("input"));
searchSymbolsInput.focus();
}

	function selectNextSymbol() {
	if (selectedSymbolIdx < filteredSymbols.length - 1) selectedSymbolIdx++;
updateSymbolSelection();
}

	function selectPreviousSymbol() {
	if (selectedSymbolIdx > 0) selectedSymbolIdx--;
updateSymbolSelection();
}

	function updateSymbolSelection() {
	const items = symbolList.querySelectorAll(".symbol-item");
items.forEach((el) => el.classList.remove("active"));
	if (selectedSymbolIdx >= 0 && items[selectedSymbolIdx]) {
items[selectedSymbolIdx].classList.add("active");
items[selectedSymbolIdx].scrollIntoView({ block: "nearest" });
}
}

	function navigateToSelectedSymbol() {
	if (selectedSymbolIdx >= 0 && filteredSymbols[selectedSymbolIdx]) {
NavigateTo({ File: LocalSettings.File, line: filteredSymbols[selectedSymbolIdx].line }, filteredSymbols[selectedSymbolIdx].name);
}
}

	function startProjectSearch() {
switchTab("right", "solutionExplorerTab");
searchFilesInput.value = "";
filterProjectFiles();
searchFilesInput.focus();
}

	function updateProjectFileSelection() {
	const items = filteredProjectFiles;
fileList.querySelectorAll(".file-item").forEach((el) => el.classList.remove("active"));
	if (selectedProjectFileIdx >= 0 && items[selectedProjectFileIdx]) {
items[selectedProjectFileIdx].classList.add("active");
items[selectedProjectFileIdx].scrollIntoView({ block: "nearest" });
}
}

	function selectNextProjectFile() {
	if (selectedProjectFileIdx < filteredProjectFiles.length - 1) selectedProjectFileIdx++;
updateProjectFileSelection();
}

	function selectPreviousProjectFile() {
	if (selectedProjectFileIdx > 0) selectedProjectFileIdx--;
updateProjectFileSelection();
}

	function navigateToSelectedProjectFile() {
	if (selectedProjectFileIdx >= 0 && filteredProjectFiles[selectedProjectFileIdx]) {
filteredProjectFiles[selectedProjectFileIdx].click();
}
}
searchSymbolsInput.addEventListener("input", (e) => {
	const searchTerm = e.target.value.toLowerCase();
	const fileTitleElement = symbolList.querySelector(
		".text-xs.text-gray-500.px-1.pt-1.pb-0\\.5.font-semibold",
	);
	const currentFileTitleHTML = fileTitleElement
		? fileTitleElement.outerHTML
		: "";
	symbolList.innerHTML = currentFileTitleHTML;
	const filteredSymbols = currentFileSymbols.filter((symbol) =>
		symbol.name.toLowerCase().includes(searchTerm),
	);
renderSymbolList(filteredSymbols);
});
searchSymbolsInput.addEventListener("keydown", (e) => {
	if (e.key === "ArrowDown") {
selectNextSymbol();
e.preventDefault();
} else if (e.key === "ArrowUp") {
selectPreviousSymbol();
e.preventDefault();
} else if (e.key === "Enter") {
navigateToSelectedSymbol();
e.preventDefault();
}
});

	function loadFileContent(fileName) {
	appendToConsole(`Loading file: ${fileName}`, "INFO");
	let mockContent = `// Content for ${fileName}\n\n// ProtoScript code goes here...\n\n`;
	if (fileName.includes("Annotations.pts")) {
		mockContent +=
			"function processAnnotation(data) {}\nconst MAX_POINTS = 100;\nvar currentTool = 'select';";
	} else if (fileName.includes("Articles.pts")) {
		mockContent +=
			"function Article() {}\nfunction fetchContent(id) {}\nfunction displayArticle(content) {}\nconst DEFAULT_CATEGORY = 'news';";
	} else if (fileName.includes("Base.pts")) {
		mockContent +=
			"function BaseObject() {}\nfunction InitializeSystem() {}\nvar config = {};\nlet statusFlag = true;";
	} else {
		mockContent += "function genericFunction() {}\n var myData = [];";
	}
	editor.setValue(mockContent);
	parseAndDisplaySymbols(fileName, mockContent);
}

	const initialFiles = [
	"..\\NL_Project_1.0.1\\Annotations.pts",
	"..\\NL_Project_1.0.1\\Articles.pts",
	"..\\NL_Project_1.0.1\\Base.pts",
	"..\\NL_Project_1.0.1\\Utils.pts",
	"..\\NL_Project_1.0.1\\Main.pts",
];
	let firstFileDivToLoad = null;
initialFiles.forEach((fileName, index) => {
	const fileDiv = document.createElement("div");
	fileDiv.className =
		"file-item p-1.5 rounded-md cursor-pointer text-gray-700";
	fileDiv.textContent = fileName;
	fileDiv.onclick = () => {
		loadFileContent(fileName);
		highlightActiveFile(fileDiv);
	};
fileList.appendChild(fileDiv);
	if (index === 0) {
		firstFileDivToLoad = fileDiv;
	}
});
	if (firstFileDivToLoad) {
loadFileContent(firstFileDivToLoad.textContent);
highlightActiveFile(firstFileDivToLoad);
}
filterProjectFiles();

	function filterProjectFiles() {
	const searchTerm = searchFilesInput.value.toLowerCase();
filteredProjectFiles = [];
fileList.querySelectorAll(".file-item").forEach((item) => {
	const match = item.textContent.toLowerCase().includes(searchTerm);
item.style.display = match ? "block" : "none";
	if (match) filteredProjectFiles.push(item);
});
selectedProjectFileIdx = -1;
}

searchFilesInput.addEventListener("input", () => {
filterProjectFiles();
});
searchFilesInput.addEventListener("keydown", (e) => {
	if (e.key === "ArrowDown") {
selectNextProjectFile();
e.preventDefault();
} else if (e.key === "ArrowUp") {
selectPreviousProjectFile();
e.preventDefault();
} else if (e.key === "Enter") {
navigateToSelectedProjectFile();
e.preventDefault();
}
});
	if (searchHistoryInput)
searchHistoryInput.addEventListener("input", (e) => {
	const term = e.target.value.toLowerCase();
document.querySelectorAll("#divSolution .file-item").forEach((item) => {
item.style.display = item.textContent.toLowerCase().includes(term) ? "block" : "none";
});
});

document.getElementById("menuLoadProject").addEventListener("click", () => {
document.getElementById("loadProjectModal").style.display = "block";
	appendToConsole("Opened Load Project dialog.", "INFO");
});

document.getElementById("menuAddFile").addEventListener("click", () => {
addFileModal.style.display = "block";
addFileError.classList.add("hidden");
modalNewFileNameInput.value = "";
	appendToConsole("Opened Add File dialog.", "INFO");
});

document.getElementById("menuSaveFile").addEventListener("click", OnSaveCurrentFile);
document.getElementById("menuCompile").addEventListener("click", CompileCode);

	function OnProcessImmediate() {
	const command = immediateCommandInput.value.trim();
	if (command === "") {
	appendToConsole("No command entered.", "WARN");
               return;
       }
	appendToConsole(command, "CMD");
	if (command.toLowerCase() === "clear") {
               ClearOutput();
	appendToConsole("Console cleared.", "RESULT");
               immediateCommandInput.value = "";
               return;
       }
	if (command.endsWith(")") && command.includes("(")) {
               OnInterpretImmediate();
       } else {
               OnTagImmediate();
       }
       immediateCommandInput.value = "";
}

async function OnInterpretImmediate() {
	if (!isCurrentFileSaved()) await SaveCurrentFile();
	const project = LocalSettings.Solution;
	const cmd = immediateCommandInput.value.trim();
	const options = {
               IncludeTypeOfs: document.getElementById("settingShowTypeOfs").checked,
               Debug: document.getElementById("settingDebugMode").checked,
               Resume: document.getElementById("settingResumeExecution").checked,
               SessionKey: LocalSettings.Solution,
       };
       sessionKey = options.SessionKey;
	appendToConsole("Interpreting...", "INFO");
       LocalSettings.ImmediateHistory = queueFront(LocalSettings.ImmediateHistory, cmd, 50);
       saveLocalSettings();
       bindImmediateHistory();
	ProtoScriptWorkbench.InterpretImmediate.onErrorReceived = (err) => {
               Output(err.Error || JSON.stringify(err));
	if (err.ErrorStatement) NavigateTo(err.ErrorStatement);
       };
       await new Promise((resolve) => {
	ProtoScriptWorkbench.InterpretImmediate(project, cmd, options, (res) => {
                       Output(res.Result || "");
                       resolve();
               });
       });
}

async function OnTagImmediate() {
	if (bIsTagging) {
	ProtoScriptWorkbench.StopTagging(sessionKey);
               bIsTagging = false;
               clearInterval(timerUpdate);
               taggingProgressSpan.textContent = "";
               return;
       }
       bIsTagging = true;
	if (!isCurrentFileSaved()) await SaveCurrentFile();
	const cmd = immediateCommandInput.value.trim();
	const project = LocalSettings.Solution;
	const options = {
               IncludeTypeOfs: document.getElementById("settingShowTypeOfs").checked,
               Debug: document.getElementById("settingDebugMode").checked,
               Resume: document.getElementById("settingResumeExecution").checked,
               SessionKey: LocalSettings.Solution,
       };
       sessionKey = options.SessionKey;
       LocalSettings.ImmediateHistory = queueFront(LocalSettings.ImmediateHistory, cmd, 50);
       saveLocalSettings();
       bindImmediateHistory();
	ProtoScriptWorkbench.TagImmediate.onErrorReceived = (err) => {
               clearInterval(timerUpdate);
               bIsTagging = false;
               Output(err.Error || JSON.stringify(err));
	if (err.ErrorStatement) NavigateTo(err.ErrorStatement);
       };
       timerUpdate = setInterval(() => {
	ProtoScriptWorkbench.GetTaggingProgress(sessionKey, (progress) => {
	if (progress) taggingProgressSpan.textContent = `${progress.Iterations}: ${progress.CurrentInterpretation}`;
               });
       }, 1000);
	ProtoScriptWorkbench.TagImmediate(cmd, project, options, (res) => {
               bIsTagging = false;
               clearInterval(timerUpdate);
               taggingProgressSpan.textContent = "";
	if (res.Result) Output(res.Result); else if (res.Error) { Output(res.Error); if (res.ErrorStatement) NavigateTo(res.ErrorStatement); }
       });
}

runImmediateCmdBtn.addEventListener("click", OnProcessImmediate);
immediateCommandInput.addEventListener("keydown", (e) => {
	if (e.key === "Enter") {
               OnProcessImmediate();
               e.preventDefault();
       }
});

["settingShowTypeOfs", "settingDebugMode", "settingResumeExecution"].forEach(
	(id) => {
		document.getElementById(id).addEventListener("change", (e) => {
			appendToConsole(
				`${e.target.labels[0].textContent.trim()} setting: ${e.target.checked ? "Enabled" : "Disabled"}`,
				"INFO",
			);
		});
	},
);
["editUndo", "editRedo", "editCut", "editCopy", "editPaste"].forEach((id) => {
	const btn = document.getElementById(id);
	if (btn)
		btn.addEventListener("click", () => {
			appendToConsole(`${btn.textContent} action clicked.`, "DEBUG");
			if (id === "editUndo") editor.undo();
			else if (id === "editRedo") editor.redo();
		});
});
	const findBtn = document.getElementById("editFind");
	if (findBtn)
	findBtn.addEventListener("click", () => {
		CodeMirror.commands.find(editor);
		appendToConsole("Find dialog opened.", "DEBUG");
	});

	const loadProjectModal = document.getElementById("loadProjectModal");
	const closeModalBtn = document.getElementById("closeModalBtn");
	const modalConfirmLoadBtn = document.getElementById("modalConfirmLoadBtn");
	const modalCancelLoadBtn = document.getElementById("modalCancelLoadBtn");
	const modalProjectPathInput = document.getElementById("txtSolution");
	const addFileModal = document.getElementById("addFileModal");
	const closeAddFileModalBtn = document.getElementById("closeAddFileModalBtn");
	const modalConfirmAddFileBtn = document.getElementById("modalConfirmAddFileBtn");
	const modalCancelAddFileBtn = document.getElementById("modalCancelAddFileBtn");
	const modalNewFileNameInput = document.getElementById("newFileName");
	const addFileError = document.getElementById("addFileError");
closeModalBtn.onclick = () => (loadProjectModal.style.display = "none");
modalCancelLoadBtn.onclick = () => (loadProjectModal.style.display = "none");
window.onclick = (event) => {
	if (event.target == loadProjectModal)
		loadProjectModal.style.display = "none";
};
modalConfirmLoadBtn.addEventListener("click", async () => {
await OnLoadProject();
loadProjectModal.style.display = "none";
modalProjectPathInput.value = "";
});
closeAddFileModalBtn.onclick = () => (addFileModal.style.display = "none");
modalCancelAddFileBtn.onclick = () => (addFileModal.style.display = "none");
window.addEventListener("click", (e) => {
	if (e.target === addFileModal) addFileModal.style.display = "none";
});
modalConfirmAddFileBtn.addEventListener("click", () => {
	const newFileName = modalNewFileNameInput.value.trim();
	if (!newFileName) {
addFileError.classList.remove("hidden");
	return;
}
addFileError.classList.add("hidden");
	ProtoScriptWorkbench.CreateNewFile(LocalSettings.Solution, newFileName, (res) => {
	if (res) {
	const div = createFileItem(newFileName);
fileList.appendChild(div);
document.getElementById("txtFileName").value = newFileName;
OnLoadFile();
highlightActiveFile(div);
addFileModal.style.display = "none";
modalNewFileNameInput.value = "";
}
});
});
document.getElementById("clearConsoleBtn").addEventListener("click", () => {
	consoleOutput.value = "";
	appendToConsole("Console cleared.", "INFO");
});
	const outputResizer = document.getElementById("outputResizer");
	const outputWindow = document.getElementById("outputWindow");
	let initialOutputHeight, initialMouseY;
AdjustWindowSizes();
outputResizer.addEventListener("mousedown", (e) => {
	e.preventDefault();
	initialOutputHeight = outputWindow.offsetHeight;
	initialMouseY = e.clientY;
	document.addEventListener("mousemove", resizeOutput);
	document.addEventListener("mouseup", stopResizeOutput);
});
	function resizeOutput(e) {
	const deltaY = e.clientY - initialMouseY;
	let newHeight = initialOutputHeight - deltaY;
	const minHeight = 100,
		maxHeight = window.innerHeight * 0.8;
	newHeight = Math.max(minHeight, Math.min(newHeight, maxHeight));
	outputWindow.style.height = `${newHeight}px`;
}
	function stopResizeOutput() {
	document.removeEventListener("mousemove", resizeOutput);
	document.removeEventListener("mouseup", stopResizeOutput);
}
	const rightPanel = document.getElementById("rightPanel");
document
	.getElementById("menuToggleSolutionExplorer")
	.addEventListener("click", () => {
		const isHidden = rightPanel.style.display === "none";
		rightPanel.style.display = isHidden ? "flex" : "none";
		appendToConsole(
			`Solution Explorer ${isHidden ? "shown" : "hidden"}.`,
			"DEBUG",
		);
	});
document
        .getElementById("menuToggleOutputWindow")
        .addEventListener("click", () => {
	const isHidden = outputWindow.style.display === "none";
                outputWindow.style.display = isHidden ? "flex" : "none";
                outputResizer.style.display = isHidden ? "block" : "none";
	appendToConsole(
                        `Output Panel ${isHidden ? "shown" : "hidden"}.`,
                        "DEBUG",
                );
        });

	if (btnMaximize) btnMaximize.addEventListener("click", OnMaximize);
	if (btnHalfize) btnHalfize.addEventListener("click", OnHalfize);
	if (btnMinimize) btnMinimize.addEventListener("click", OnMinimize);

document.addEventListener("keydown", (e) => {
	if (e.ctrlKey && e.key === ",") {
	e.preventDefault();
	startSymbolSearch();
	} else if (e.ctrlKey && (e.key === "p" || e.key === "P")) {
	e.preventDefault();
	startProjectSearch();
	}
	});
	
	appendToConsole("Welcome to the SemDB IDE Console!", "LOG");
	appendToConsole("help", "CMD");
	appendToConsole(
	"Available: help, clear, run_script, get_active_file",
	"RESULT",
);
	appendToConsole("get_active_file", "CMD");
	if (firstFileDivToLoad) {
	appendToConsole(`Active file: ${firstFileDivToLoad.textContent}`, "RESULT");
}

	appendToConsole("SemDB IDE Initialized (Unified Console).", "INFO");
	if (initialFiles.length === 0 && !firstFileDivToLoad) {
	appendToConsole(
                "No project loaded. Use File > Load Project or File > Add File.",
                "INFO",
        );
}

window.addEventListener("load", async () => {
	if (LocalSettings.Solution) {
document.getElementById("txtSolution").value = LocalSettings.Solution;
await OnLoadProject();
	if (LocalSettings.File) {
document.getElementById("txtFileName").value = LocalSettings.File;
await OnLoadFile();
	const item = Array.from(fileList.querySelectorAll('.file-item')).find(d => d.title && d.title.toLowerCase() === LocalSettings.File.toLowerCase());
	if (item) highlightActiveFile(item);
}
}
});

window.addEventListener("beforeunload", (e) => {
saveLocalSettings();
	if (!isCurrentFileSaved()) e.returnValue = "Unsaved changes";
});
