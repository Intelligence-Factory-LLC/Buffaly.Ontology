/* ---------- FILE ---------- */
function OnMenuLoadProject() { showModal('loadProjectModal'); }
function OnMenuAddFile() { showModal('addFileModal'); }
function OnMenuSaveFile() { (window.OnSaveCurrentFile || (() => { }))(); }
function OnMenuExit() { window.close?.(); }

/* ---------- EDIT ---------- */
function OnMenuUndo() { editor.undo(); appendToConsole('Undo', 'DEBUG'); }
function OnMenuRedo() { editor.redo(); appendToConsole('Redo', 'DEBUG'); }
function OnMenuCut() { document.execCommand('cut'); appendToConsole('Cut', 'DEBUG'); }
function OnMenuCopy() { document.execCommand('copy'); appendToConsole('Copy', 'DEBUG'); }
function OnMenuPaste() { document.execCommand('paste'); appendToConsole('Paste', 'DEBUG'); }
function OnMenuFind() { CodeMirror.commands.find(editor); appendToConsole('Find opened', 'DEBUG'); }

/* ---------- BUILD ---------- */
function OnMenuCompile() {
	if (typeof CompileCode === 'function') CompileCode();
	else appendToConsole('CompileCode not found.', 'WARN');
}

/* ---------- VIEW ---------- */
function OnMenuToggleSolutionExplorer() {
	const p = document.getElementById('rightPanel');
	const hidden = getComputedStyle(p).display === 'none';
	p.style.display = hidden ? 'flex' : 'none';
	appendToConsole(`Solution Explorer ${hidden ? 'shown' : 'hidden'}.`, 'DEBUG');
}
function OnMenuToggleOutputWindow() {
	const w = document.getElementById('outputWindow');
	const r = document.getElementById('outputResizer');
	const hidden = getComputedStyle(w).display === 'none';
	w.style.display = hidden ? 'flex' : 'none';
	r.style.display = hidden ? 'block' : 'none';
	appendToConsole(`Output Panel ${hidden ? 'shown' : 'hidden'}.`, 'DEBUG');
}

/* ---------- SETTINGS ---------- */
function OnSettingShowTypeOfs(cb) {
	document.getElementById('chkIncludeTypeOfs').checked = cb.checked;
	appendToConsole(`Show Type Ofs setting: ${cb.checked ? 'Enabled' : 'Disabled'}`, 'INFO');
}
function OnSettingDebugMode(cb) {
	document.getElementById('chkDebug').checked = cb.checked;
	appendToConsole(`Debug Mode setting: ${cb.checked ? 'Enabled' : 'Disabled'}`, 'INFO');
}
function OnSettingResumeExecution(cb) {
	document.getElementById('chkResume').checked = cb.checked;
	appendToConsole(`Resume Execution setting: ${cb.checked ? 'Enabled' : 'Disabled'}`, 'INFO');
}

/* ---------- tiny modal helpers ---------- */
function showModal(id) { document.getElementById(id).style.display = 'block'; }
function hideModal() { ['loadProjectModal', 'addFileModal'].forEach(id => document.getElementById(id).style.display = 'none'); }

/* ---------- initialise menu checkboxes from hidden controls ---------- */
(function initMenuSettings() {
	const sync = (src, tgt) => document.getElementById(src).checked = document.getElementById(tgt).checked;
	sync('settingShowTypeOfs', 'chkIncludeTypeOfs');
	sync('settingDebugMode', 'chkDebug');
	sync('settingResumeExecution', 'chkResume');
})();