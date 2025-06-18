// ───────────────────────────────────────────────────────────
// Element references (do not rename these variables)
// ───────────────────────────────────────────────────────────
const divSolution = document.getElementById("divSolution");
const divImmediateHistory = document.getElementById("divImmediateHistory");
const fileList = document.getElementById("divProject");
const symbolList = document.getElementById("divSymbolTable");
const searchFilesInput = document.getElementById("txtProjectFileSearch");
const searchSymbolsInput = document.getElementById("txtSymbolSearch");
const searchHistoryInput = document.getElementById("txtFileSearch");
const immediateCommandInput = document.getElementById("txtImmediate");
const runImmediateCmdBtn = document.getElementById("runImmediateCmdBtn");
const headerProjectName = document.getElementById("headerProjectName");
const consoleOutput = document.getElementById("txtResults2");
const outputResizer = document.getElementById("outputResizer");
const outputWindow = document.getElementById("outputWindow");

// Modal elements
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

// ───────────────────────────────────────────────────────────
// State variables
// ───────────────────────────────────────────────────────────
let recentNavigations = [];
let selectedSymbolIdx = -1;
let filteredSymbols = [];
let selectedProjectFileIdx = -1;
let filteredProjectFiles = [];

const LocalSettings = JSON.parse(localStorage.getItem("EditorSettings") || "{}");
if (!LocalSettings.FileHistory) LocalSettings.FileHistory = [];
if (!LocalSettings.ImmediateHistory) LocalSettings.ImmediateHistory = [];
if (!LocalSettings.WindowSize) LocalSettings.WindowSize = "Halfize";

let editor;
//let sessionKey = null;
//let sLastSaved = null;
//let bIsTagging = false;
//let timerUpdate = null;
let currentProjectName = "Untitled Project";
let currentFileSymbols = [];

// ───────────────────────────────────────────────────────────
// Initialization
// ───────────────────────────────────────────────────────────
function updateTitle() {
        document.title = `ProtoScript Editor - ${currentProjectName}`;
        headerProjectName.textContent = `- ${currentProjectName}`;
}
updateTitle();

function saveLocalSettings() {
	localStorage.setItem("EditorSettings", JSON.stringify(LocalSettings));
}

// ───────────────────────────────────────────────────────────
// Utility functions
// ───────────────────────────────────────────────────────────
function queueFront(arr, item, max) {
	const idx = arr.findIndex(x => JSON.stringify(x) === JSON.stringify(item));
	if (idx !== -1) arr.splice(idx, 1);
	arr.unshift(item);
	if (arr.length > max) arr.pop();
	return arr;
}

function getRelativePath(root, path) {
	return path.toLowerCase().startsWith(root.toLowerCase() + "\\")
		? path.substring(root.length + 1)
		: path;
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
	let existing = LocalSettings.FileHistory
		.find(x => x.File && x.File.toLowerCase() === file.toLowerCase());
	if (!existing) existing = { File: file };
	LocalSettings.FileHistory = queueFront(LocalSettings.FileHistory, existing, 50);
	saveLocalSettings();
	return existing;
}

function bindFileHistory() {
        if (!divSolution) return;
        divSolution.innerHTML = "";
        const root = LocalSettings.Solution
                ? LocalSettings.Solution.substring(0, LocalSettings.Solution.lastIndexOf("\\"))
                : "";
        LocalSettings.FileHistory.forEach(item => {
                const div = createFileItem(item.File, getRelativePath(root, item.File));
                divSolution.appendChild(div);
        });
}

function bindImmediateHistory() {
	if (!divImmediateHistory) return;
	divImmediateHistory.innerHTML = "";
	LocalSettings.ImmediateHistory.forEach(cmd => {
		const div = document.createElement("div");
		div.className = "cursor-pointer px-1 hover:bg-gray-100";
		div.textContent = cmd;
		div.onclick = () => {
			immediateCommandInput.value = cmd;
		};
		divImmediateHistory.appendChild(div);
	});
}

function isCurrentFileSaved() {
	return !sLastSaved || editor.getValue() === sLastSaved;
}

function appendToConsole(message, type = "LOG") {
	const timestamp = new Date().toLocaleTimeString();
	let prefix = "";
	switch (type) {
		case "CMD": prefix = "CMD> "; break;
		case "RESULT": prefix = "=> "; break;
		case "WARN": prefix = "WARN: "; break;
		case "ERROR": prefix = "ERROR: "; break;
		case "INFO": prefix = "INFO: "; break;
	}
	consoleOutput.value += `[${timestamp}] ${prefix}${message}\n`;
	consoleOutput.scrollTop = consoleOutput.scrollHeight;
}

function ClearOutput() {
	consoleOutput.value = "";
}

function Output(msg) {
	appendToConsole(msg, "RESULT");
}

// ───────────────────────────────────────────────────────────
// CodeMirror helpers
// ───────────────────────────────────────────────────────────
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
	ProtoScriptWorkbench.PredictNextLine(pos, function (res) {
		appendToConsole(res.length + " results found", "INFO");
		if (res && res.length > 0) {
			const newPart = StringUtil.ReplaceAll(res[0], "\n", "\r\n");
			editor.setValue(before + "\r\n" + newPart + "\r\n" + after);
		}
	});
}

function OnSuggest(cm) {
	const cursor = cm.getCursor();
	const line = cm.getLine(cursor.line).substr(0, cursor.ch);
	if (StringUtil.InString(line, "//")) return null;
	const pos = cm.indexFromPos(cursor);
	let results = [], offset = line.length, search = "";
	const lastSavedOffset = GetCode().length - (sLastSaved ? sLastSaved.length : 0);

	if (!StringUtil.IsEmpty(line)) {
		let type = "";
		for (let i = line.length - 1; i >= 0; i--) {
			const ch = line[i];
			if (isLetter(ch) || isNumber(ch) || ch === "_") {
				search = ch + search; offset--;
			} else if (ch === ".") {
				type = "SubObject"; break;
			} else if (ch === "(" || ch === ",") {
				type = "Parameter"; break;
			} else {
				type = "Symbol"; break;
			}
		}

		let res2 = [], res3 = [], res = [];
		if (["Symbol", "Parameter"].includes(type) && !StringUtil.IsEmpty(search)) {
			res2 = ProtoScriptWorkbench.GetSymbolsAtCursor(LocalSettings.Solution, LocalSettings.File, pos - lastSavedOffset) || [];
			res2 = res2.filter(x => StringUtil.StartsWith(x.SymbolName, search));
		}
		if (type === "SubObject") {
			res3 = ProtoScriptWorkbench.Suggest(LocalSettings.Solution, line, pos) || [];
		}
		if (["Symbol", "Parameter"].includes(type) && !StringUtil.IsEmpty(search)) {
			res = Symbols.filter(x => StringUtil.StartsWith(x.SymbolName, search));
		}
		res3.forEach(x => results.push(x.SymbolName));
		res2.forEach(x => results.push(x.SymbolName));
		res.slice(0, 10).forEach(x => results.push(x.SymbolName));
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
	"8": "backspace", "9": "tab", "13": "enter", "16": "shift", "17": "ctrl", "18": "alt", "19": "pause", "20": "capslock",
	"27": "escape", "33": "pageup", "34": "pagedown", "35": "end", "36": "home", "37": "left", "38": "up", "39": "right",
	"40": "down", "45": "insert", "46": "delete", "91": "left window", "92": "right window", "93": "select",
	"107": "add", "109": "subtract", "110": "decimal point", "111": "divide", "112": "f1", "113": "f2", "114": "f3",
	"115": "f4", "116": "f5", "117": "f6", "118": "f7", "119": "f8", "120": "f9", "121": "f10", "122": "f11", "123": "f12",
	"144": "numlock", "145": "scrolllock", "186": "semicolon", "187": "equalsign", "188": "comma", "189": "dash",
	"191": "slash", "192": "graveaccent", "220": "backslash", "222": "quote"
};

// ───────────────────────────────────────────────────────────
// Initialize CodeMirror editor
// ───────────────────────────────────────────────────────────
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
	extraKeys: {
		"Ctrl-Space": "autocomplete",
		"Ctrl-Enter": OnEnter,
		"Alt-Space": OnPredictNextLine
	},
	hintOptions: { hint: OnSuggest, completeSingle: false },
	gutters: ["gutter-error", "breakpoints", "CodeMirror-linenumbers"]
});

editor.on("gutterClick", (cm, n) => {
	const info = cm.lineInfo(n);
	if (info.gutterMarkers) {
		cm.setGutterMarker(n, "breakpoints", null);
		RemoveBreakPoint(info);
	} else {
		cm.setGutterMarker(n, "breakpoints", makeBreakpoint());
		SetBreakPoint(info);
	}
});

editor.on("keyup", (cm, event) => {
	const code = (event.keyCode || event.which).toString();
	if (!cm.state.completionActive && !ExcludedIntelliSenseTriggerKeys[code]) {
		CodeMirror.commands.autocomplete(cm, null, { completeSingle: false });
	}
});

// ───────────────────────────────────────────────────────────
// Breakpoint marker
// ───────────────────────────────────────────────────────────
function makeBreakpoint() {
	const marker = document.createElement("div");
	marker.className = "breakpoint";
	marker.innerHTML = "●";
	return marker;
}

// ───────────────────────────────────────────────────────────
// Initial binding of histories
// ───────────────────────────────────────────────────────────
bindFileHistory();
bindImmediateHistory();

// ───────────────────────────────────────────────────────────
// Window sizing controls
// ───────────────────────────────────────────────────────────
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

// ───────────────────────────────────────────────────────────
// Tab switching
// ───────────────────────────────────────────────────────────
function switchTab(panePrefix, tabIdToShow) {
	document.querySelectorAll(`button[data-pane='${panePrefix}']`)
		.forEach(b => b.classList.remove("active"));
	document.querySelectorAll(`.${panePrefix}-tab-content`)
		.forEach(c => c.classList.remove("active"));
	const btn = document.querySelector(
		`button[data-pane='${panePrefix}'][data-tab='${tabIdToShow}']`);
	if (btn) btn.classList.add("active");
	const pane = document.getElementById(tabIdToShow);
	if (pane) pane.classList.add("active");
}

document.querySelectorAll(".tab-button")
	.forEach(button => button.addEventListener("click", () => {
		const pane = button.dataset.pane;
		const tab = button.dataset.tab;
		switchTab(pane, tab);
		appendToConsole(
			`Switched to ${pane} panel, tab: ${button.textContent}`,
			"DEBUG"
		);
	}));

switchTab("right", "tab-solution");
switchTab("bottom", "consoleContent");

// ───────────────────────────────────────────────────────────
// Highlight active file in solution explorer
// ───────────────────────────────────────────────────────────
function highlightActiveFile(selectedItem) {
	document.querySelectorAll("#divProject .file-item")
		.forEach(item => item.classList.remove("active"));
	if (selectedItem) selectedItem.classList.add("active");
}

// ───────────────────────────────────────────────────────────
// Symbol parsing & rendering
// ───────────────────────────────────────────────────────────
function parseAndDisplaySymbols(fileName, codeContent) {
	currentFileSymbols = [];
	symbolList.innerHTML = "";
	const fnRe = /function\s+([a-zA-Z0-9_]+)\s*\(/g;
	const varRe = /(?:var|let|const)\s+([a-zA-Z0-9_]+)/g;
	let match;

	// Title
	const title = document.createElement("p");
	title.className = "text-xs text-gray-500 px-1 pt-1 pb-0.5 font-semibold";
	title.textContent = `Symbols in ${fileName.split(/[\\/]/).pop()}:`;
	symbolList.appendChild(title);

	// Functions
	while ((match = fnRe.exec(codeContent)) !== null) {
		const pos = editor.posFromIndex(match.index);
		currentFileSymbols.push({
			name: match[1], type: "function", line: pos.line + 1
		});
	}
	// Variables
	while ((match = varRe.exec(codeContent)) !== null) {
		const name = match[1];
		if (!["var", "let", "const"].includes(name)) {
			const pos = editor.posFromIndex(match.index);
			currentFileSymbols.push({
				name, type: "variable", line: pos.line + 1
			});
		}
	}
	// Extras for Base and Articles
	if (fileName.includes("Base.pts")) {
		["BaseObject", "InitializeSystem"].forEach(fn => {
			const idx = codeContent.indexOf(fn);
			if (idx !== -1) {
				const pos = editor.posFromIndex(idx);
				currentFileSymbols.push({
					name: fn, type: fn === "BaseObject" ? "class" : "method", line: pos.line + 1
				});
			}
		});
	}
	if (fileName.includes("Articles.pts")) {
		["Article", "fetchContent", "displayArticle"].forEach(fn => {
			const idx = codeContent.indexOf(fn);
			if (idx !== -1) {
				const pos = editor.posFromIndex(idx);
				currentFileSymbols.push({
					name: fn, type: fn === "Article" ? "class" : "method", line: pos.line + 1
				});
			}
		});
	}

	if (currentFileSymbols.length === 0) {
		const msg = document.createElement("p");
		msg.className = "text-gray-500 text-xs p-1";
		msg.textContent = "No symbols found in this file.";
		symbolList.appendChild(msg);
	} else {
		renderSymbolList(currentFileSymbols);
	}
}

function getSymbolIcon(type) {
	switch (type) {
		case "function": return `<svg viewBox="0 0 16 16" fill="#805AD5" class="symbol-icon"><path d="M4 14V2h5.5l3.5 3.5V14H4zm1-1h7V6H9V3H5v10zM6.5 9H8V7.5h2V9h1.5v1H10v1.5H8V13H6.5v-1.5H5V10h1.5V9z"/></svg>`;
		case "variable": return `<svg viewBox="0 0 16 16" fill="#3182CE" class="symbol-icon"><path d="M4 2v12h8V2H4zm1 1h6v2L8 7l-3-2V3zm0 5l3 2 3-2v5H5V8z"/></svg>`;
		case "class": return `<svg viewBox="0 0 16 16" fill="#DD6B20" class="symbol-icon"><path d="M8 1a7 7 0 100 14A7 7 0 008 1zm0 1.5a5.5 5.5 0 014.606 8.036L5.964 3.964A5.466 5.466 0 018 2.5zM3.964 12.036L10.036 5.964A5.5 5.5 0 013.964 12.036z"/></svg>`;
		case "method": return `<svg viewBox="0 0 16 16" fill="#319795" class="symbol-icon"><path d="M3 3h10v1H3V3zm0 2h10v1H3V5zm0 3h4v1H3V8zm0 2h4v1H3v-1zm5-2h5v1H8V8zm0 2h5v1H8v-1zM3 13h10v1H3v-1z"/></svg>`;
		default: return `<svg viewBox="0 0 16 16" fill="#718096" class="symbol-icon"><circle cx="8" cy="8" r="3"/></svg>`;
	}
}

function renderSymbolList(list) {
	symbolList.querySelectorAll(".symbol-item, .no-symbols-message").forEach(e => e.remove());
	filteredSymbols = list.slice();
	selectedSymbolIdx = -1;

	if (list.length === 0) {
		const msg = document.createElement("p");
		msg.className = "text-gray-500 text-xs p-1 no-symbols-message";
		msg.textContent = "No symbols to display.";
		symbolList.appendChild(msg);
		return;
	}

	list.forEach(sym => {
		const item = document.createElement("div");
		item.className = "symbol-item flex items-center p-1 text-xs cursor-pointer";
		item.innerHTML =
			getSymbolIcon(sym.type) +
			`<span class="ml-1">${sym.name}</span>` +
			`<span class="ml-auto text-gray-400 text-xxs">L:${sym.line}</span>`;
		item.onclick = () => {
			appendToConsole(`Symbol clicked: ${sym.name}`, "DEBUG");
			if (sym.line > 0) {
				editor.setCursor({ line: sym.line - 1, ch: 0 });
				editor.focus();
				const lh = editor.getLineHandle(sym.line - 1);
				editor.addLineClass(lh, "background", "bg-yellow-200");
				setTimeout(() => editor.removeLineClass(lh, "background", "bg-yellow-200"), 1500);
			}
		};
		symbolList.appendChild(item);
	});
}

// ───────────────────────────────────────────────────────────
// Project loading & file opening
// ───────────────────────────────────────────────────────────
async function OnLoadProject() {
	const path = modalProjectPathInput.value.trim();
	if (!path) {
		appendToConsole("Project path cannot be empty.", "WARN");
		return;
	}
	appendToConsole(`Loading project: ${path}`, "INFO");
	const root = path.substring(0, path.lastIndexOf("\\"));
	const files = await new Promise(r => ProtoScriptWorkbench.LoadProject(path, r));
	fileList.innerHTML = "";
	let firstDiv = null;
	files.forEach((f, i) => {
		const div = createFileItem(f, getRelativePath(root, f));
		fileList.appendChild(div);
		if (i === 0) firstDiv = div;
	});
	filteredProjectFiles = [];
	currentProjectName = path.split(/[\\/]/).pop();
	updateTitle();
	LocalSettings.Solution = path;
	saveLocalSettings();

	if (LocalSettings.File || firstDiv) {
		const fileToOpen = LocalSettings.File || firstDiv.title;
		document.getElementById("txtFileName").value = fileToOpen;
		await OnLoadFile();
		const match = Array.from(fileList.children)
			.find(d => d.title.toLowerCase() === fileToOpen.toLowerCase());
		if (match) highlightActiveFile(match);
	}
	bindFileHistory();
	loadProjectModal.style.display = "none";
	modalProjectPathInput.value = "";
}

async function OnLoadFile() {
	const file = document.getElementById("txtFileName").value;
	if (!isCurrentFileSaved() &&
		!confirm("You have unsaved changes, continue?")) return;
	const content = await new Promise(r =>
		ProtoScriptWorkbench.LoadFile(LocalSettings.Solution, file, r)
	);
	editor.setValue(content);
	sLastSaved = content;
	parseAndDisplaySymbols(file, content);
	LocalSettings.File = file;
	addFileToHistory(file);
	bindFileHistory();
	saveLocalSettings();
}

// ───────────────────────────────────────────────────────────
// Save current file
// ───────────────────────────────────────────────────────────
async function SaveCurrentFile() {
	const code = editor.getValue();
	sLastSaved = code;
	await new Promise(r =>
		ProtoScriptWorkbench.SaveCurrentCode(
			LocalSettings.Solution, LocalSettings.File, code, r
		)
	);
	appendToConsole("File saved.", "INFO");
}

function OnSaveCurrentFile() {
	SaveCurrentFile();
}

// ───────────────────────────────────────────────────────────
// Error handling & compilation
// ───────────────────────────────────────────────────────────
function clearErrors() {
	editor.clearGutter("gutter-error");
}
function makeMarker(msg) {
	const m = document.createElement("div");
	m.className = "error-marker";
	m.innerHTML = "&nbsp;";
	const e = document.createElement("div");
	e.className = "error-message";
	e.innerHTML = msg;
	m.appendChild(e);
	return m;
}
function highlightError(err) {
	if (err.Info && err.Info.File.toLowerCase() === LocalSettings.File.toLowerCase()) {
		const line = editor.posFromIndex(err.Info.StartingOffset).line;
		editor.setGutterMarker(line, "gutter-error",
			makeMarker(`(Error ${err.Type}) ${err.Message}`)
		);
	}
}

async function CompileCode() {
	if (!isCurrentFileSaved()) await SaveCurrentFile();
	appendToConsole("Compilation starting...", "INFO");
	clearErrors();
	const projectFile = LocalSettings.Solution;
	const code = editor.getValue();

	const runCompile = (cb) => {
		if (projectFile) {
			ProtoScriptWorkbench.CompileCodeWithProject(code, projectFile, cb);
		} else {
			ProtoScriptWorkbench.CompileCode(code, cb);
		}
	};

	await new Promise(r => {
		runCompile(res => {
			res.forEach(err => {
				highlightError(err);
				appendToConsole(err.Message, "ERROR");
			});
			appendToConsole(`${res.length} errors`, "INFO");
			r();
		});
	});
	appendToConsole("Compilation finished", "INFO");
}

// ───────────────────────────────────────────────────────────
// Scrolling utilities
// ───────────────────────────────────────────────────────────
function ScrollTo(info) {
	if (info.line) {
		editor.scrollIntoView({ line: info.line - 1, ch: 0 });
		editor.setCursor({ line: info.line - 1, ch: 0 });
	} else {
		const pos = editor.posFromIndex(info.StartingOffset);
		editor.scrollIntoView({ line: pos.line, ch: pos.ch });
		editor.setCursor(pos);
	}
}

// ───────────────────────────────────────────────────────────
// Recent navigations
// ───────────────────────────────────────────────────────────
function bindRecentNavigations() {
	const c = document.getElementById("divRecentSymbols");
	if (!c) return;
	c.innerHTML = "";
	recentNavigations.forEach(nav => {
		const d = document.createElement("div");
		d.className = "symbol-item text-xs cursor-pointer";
		d.textContent = nav.SymbolName;
		d.onclick = () => NavigateTo(nav, nav.SymbolName);
		c.appendChild(d);
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

// ───────────────────────────────────────────────────────────
// Searches & navigation via keyboard
// ───────────────────────────────────────────────────────────
function startSymbolSearch() {
        switchTab("right", "tab-symbols");
        searchSymbolsInput.value = "";
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
	items.forEach(i => i.classList.remove("active"));
	if (items[selectedSymbolIdx]) {
		items[selectedSymbolIdx].classList.add("active");
		items[selectedSymbolIdx].scrollIntoView({ block: "nearest" });
	}
}
function navigateToSelectedSymbol() {
	if (filteredSymbols[selectedSymbolIdx]) {
		const sym = filteredSymbols[selectedSymbolIdx];
		NavigateTo({ File: LocalSettings.File, line: sym.line }, sym.name);
	}
}

function startProjectSearch() {
        switchTab("right", "tab-solution");
        searchFilesInput.value = "";
        filterProjectFiles();
        searchFilesInput.focus();
}
function selectNextProjectFile() {
	if (selectedProjectFileIdx < filteredProjectFiles.length - 1) selectedProjectFileIdx++;
	updateProjectFileSelection();
}
function selectPreviousProjectFile() {
	if (selectedProjectFileIdx > 0) selectedProjectFileIdx--;
	updateProjectFileSelection();
}
function updateProjectFileSelection() {
	const items = filteredProjectFiles;
	fileList.querySelectorAll(".file-item").forEach(i => i.classList.remove("active"));
	if (items[selectedProjectFileIdx]) {
		items[selectedProjectFileIdx].classList.add("active");
		items[selectedProjectFileIdx].scrollIntoView({ block: "nearest" });
	}
}
function navigateToSelectedProjectFile() {
	if (filteredProjectFiles[selectedProjectFileIdx]) {
		filteredProjectFiles[selectedProjectFileIdx].click();
	}
}

function filterProjectFiles() {
	const term = searchFilesInput.value.toLowerCase();
	filteredProjectFiles = [];
	fileList.querySelectorAll(".file-item").forEach(item => {
		const show = item.textContent.toLowerCase().includes(term);
		item.style.display = show ? "block" : "none";
		if (show) filteredProjectFiles.push(item);
	});
	selectedProjectFileIdx = -1;
}

// Output resizing
let initialOutputHeight, initialMouseY;
AdjustWindowSizes();

function resizeOutput(e) {
	let newH = initialOutputHeight - (e.clientY - initialMouseY);
	const minH = 100, maxH = window.innerHeight * 0.8;
	newH = Math.max(minH, Math.min(newH, maxH));
	outputWindow.style.height = `${newH}px`;
}
function stopResizeOutput() {
	document.removeEventListener("mousemove", resizeOutput);
	document.removeEventListener("mouseup", stopResizeOutput);
}


// Keyboard shortcuts
document.addEventListener("keydown", e => {
	if (e.ctrlKey && e.key === ',') {
		e.preventDefault();
		startSymbolSearch();
	} else if (e.ctrlKey && /[pP]/.test(e.key)) {
		e.preventDefault();
		startProjectSearch();
	}
});

// Immediate command processing
runImmediateCmdBtn.addEventListener("click", OnProcessImmediate);
immediateCommandInput.addEventListener("keydown", e => {
	if (e.key === "Enter") {
		OnProcessImmediate(); e.preventDefault();
	}
});
function OnProcessImmediate() {
	const cmd = immediateCommandInput.value.trim();
	if (!cmd) {
		appendToConsole("No command entered.", "WARN"); return;
	}
	appendToConsole(cmd, "CMD");
	LocalSettings.ImmediateHistory = queueFront(LocalSettings.ImmediateHistory, cmd, 50);
	saveLocalSettings(); bindImmediateHistory();
	if (cmd.toLowerCase() === "clear") {
		ClearOutput(); appendToConsole("Console cleared.", "RESULT");
	} else if (/\w+\(.*\)$/.test(cmd)) {
		OnInterpretImmediate();
	} else {
		OnTagImmediate();
	}
	immediateCommandInput.value = "";
}

// OnInterpretImmediate, OnTagImmediate omitted for brevity;
// assume they remain unchanged from prior implementation.
// ───────────────────────────────────────────────────────────
// End of JavaScript file


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



function ShowTab(tabId) {
	const pane = "right";
	// deactivate all buttons
	document
		.querySelectorAll(`#rightPanel button[data-pane="${pane}"]`)
		.forEach(btn => btn.classList.remove("active"));

	// hide all tab-content
	document
		.querySelectorAll(`#rightPanel .right-tab-content`)
		.forEach(div => div.classList.remove("active"));

	// activate the requested button
	const button = document.querySelector(
		`#rightPanel button[data-pane="${pane}"][data-tab="${tabId}"]`
	);
	if (button) button.classList.add("active");

	// show the requested pane
	const content = document.getElementById(tabId);
	if (content) content.classList.add("active");
}
