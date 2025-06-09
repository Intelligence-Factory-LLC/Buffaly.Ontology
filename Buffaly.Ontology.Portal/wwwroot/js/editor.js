const consoleOutput = document.getElementById("consoleOutput"); // Unified console output
const immediateCommandInput = document.getElementById("immediateCommandInput");
const runImmediateCmdBtn = document.getElementById("runImmediateCmdBtn");
const headerProjectName = document.getElementById("headerProjectName");
const fileList = document.getElementById("fileList");
const symbolList = document.getElementById("symbolList");
const searchFilesInput = document.getElementById("searchFiles");
const searchSymbolsInput = document.getElementById("searchSymbols");
let editor;
let currentProjectName = "Untitled Project";
let currentFileSymbols = [];

function updateTitle() {
	document.title = `SemDB IDE - ${currentProjectName}`;
	headerProjectName.textContent = `- ${currentProjectName}`;
}
updateTitle();

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

editor = CodeMirror.fromTextArea(document.getElementById("codeEditor"), {
	lineNumbers: true,
	mode: "javascript",
	theme: "neat",
	matchBrackets: true,
	autoCloseBrackets: true,
	lineWrapping: true,
});

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
		.querySelectorAll("#fileList .file-item")
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

searchFilesInput.addEventListener("input", (e) => {
	const searchTerm = e.target.value.toLowerCase();
	document.querySelectorAll("#fileList .file-item").forEach((item) => {
		item.style.display = item.textContent.toLowerCase().includes(searchTerm)
			? "block"
			: "none";
	});
});

document.getElementById("menuLoadProject").addEventListener("click", () => {
	document.getElementById("loadProjectModal").style.display = "block";
	appendToConsole("Opened Load Project dialog.", "INFO");
});
document.getElementById("menuAddFile").addEventListener("click", () => {
	const newFileName = prompt("Enter new file name (e.g., myFile.pts):");
	if (newFileName && newFileName.trim() !== "") {
		const fileDiv = document.createElement("div");
		fileDiv.className =
			"file-item p-1.5 rounded-md cursor-pointer text-gray-700";
		fileDiv.textContent = newFileName;
		fileDiv.onclick = () => {
			loadFileContent(newFileName);
			highlightActiveFile(fileDiv);
		};
		fileList.appendChild(fileDiv);
		appendToConsole(`Added file: ${newFileName}`, "INFO");
		loadFileContent(newFileName);
		highlightActiveFile(fileDiv);
	} else if (newFileName !== null) {
		appendToConsole("File name cannot be empty.", "WARN");
	}
});
document.getElementById("menuCompile").addEventListener("click", () => {
	appendToConsole("Compile action triggered.", "INFO");
	const code = editor.getValue();
	if (code.trim() === "") {
		appendToConsole("Editor is empty. Nothing to compile.", "WARN");
		return;
	}
	appendToConsole("Compilation started...", "INFO");
	setTimeout(
		() => appendToConsole("Compilation finished (simulated).", "INFO"),
		1500,
	);
});

runImmediateCmdBtn.addEventListener("click", () => {
	const command = immediateCommandInput.value.trim();
	if (command === "") {
		appendToConsole("No command entered.", "WARN");
		return;
	}
	appendToConsole(command, "CMD");
	if (command.toLowerCase() === "help") {
		appendToConsole(
			"Available: help, clear, run_script, get_active_file",
			"RESULT",
		);
	} else if (command.toLowerCase() === "clear") {
		consoleOutput.value = "";
		appendToConsole("Console cleared.", "RESULT");
	} else if (command.toLowerCase() === "run_script") {
		appendToConsole("Interpreting current script in editor...", "LOG");
		const code = editor.getValue();
		if (code.trim() === "") {
			appendToConsole("Editor is empty. Nothing to interpret.", "WARN");
		} else {
			setTimeout(() => {
				appendToConsole(
					"Interpretation finished (simulated). Result: OK",
					"RESULT",
				);
			}, 1000);
		}
	} else if (command.toLowerCase() === "get_active_file") {
		const activeFile = document.querySelector(
			"#fileList .file-item.active",
		);
		if (activeFile) {
			appendToConsole(`Active file: ${activeFile.textContent}`, "RESULT");
		} else {
			appendToConsole("No file is currently active.", "RESULT");
		}
	} else {
		appendToConsole(`Simulating: ${command}`, "LOG");
		setTimeout(
			() =>
				appendToConsole(`'${command}' executed. Output: OK`, "RESULT"),
			500,
		);
	}
	immediateCommandInput.value = "";
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
const modalProjectPathInput = document.getElementById("modalProjectPath");
closeModalBtn.onclick = () => (loadProjectModal.style.display = "none");
modalCancelLoadBtn.onclick = () => (loadProjectModal.style.display = "none");
window.onclick = (event) => {
	if (event.target == loadProjectModal)
		loadProjectModal.style.display = "none";
};
modalConfirmLoadBtn.addEventListener("click", () => {
	const projectPath = modalProjectPathInput.value;
	if (projectPath && projectPath.trim() !== "") {
		currentProjectName =
			projectPath.split("\\").pop().split("/").pop() || "Loaded Project";
		updateTitle();
		appendToConsole(`Loading project: ${projectPath}`, "INFO");
		fileList.innerHTML = "";
		const newFiles = [
			`${projectPath}\\file1.pts`,
			`${projectPath}\\file2.pts`,
			`${projectPath}\\module\\main.pts`,
		];
		let firstNewFileDiv = null;
		newFiles.forEach((fileName, index) => {
			const fileDiv = document.createElement("div");
			fileDiv.className =
				"file-item p-1.5 rounded-md cursor-pointer text-gray-700";
			fileDiv.textContent = fileName;
			fileDiv.onclick = () => {
				loadFileContent(fileName);
				highlightActiveFile(fileDiv);
			};
			fileList.appendChild(fileDiv);
			if (index === 0) firstNewFileDiv = fileDiv;
		});
		if (firstNewFileDiv) {
			loadFileContent(firstNewFileDiv.textContent);
			highlightActiveFile(firstNewFileDiv);
		} else {
			editor.setValue(
				`// Project ${currentProjectName} loaded. No files to display initially.`,
			);
			symbolList.innerHTML =
				'<p class="text-gray-500 text-xs p-1">No files in project or project empty.</p>';
			currentFileSymbols = [];
		}
		loadProjectModal.style.display = "none";
		modalProjectPathInput.value = "";
	} else {
		appendToConsole("Project path cannot be empty in modal.", "WARN");
		const tempAlert = document.createElement("div");
		tempAlert.textContent = "Please enter a project path.";
		tempAlert.style.cssText =
			"position:fixed; top:10px; left:50%; transform:translateX(-50%); background:red; color:white; padding:10px; border-radius:5px; z-index:2000;";
		document.body.appendChild(tempAlert);
		setTimeout(() => tempAlert.remove(), 3000);
	}
});
document.getElementById("clearConsoleBtn").addEventListener("click", () => {
	consoleOutput.value = "";
	appendToConsole("Console cleared.", "INFO");
});
const outputResizer = document.getElementById("outputResizer");
const outputWindow = document.getElementById("outputWindow");
let initialOutputHeight, initialMouseY;
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
