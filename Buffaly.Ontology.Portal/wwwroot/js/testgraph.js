	Page.LocalSettings =
{
	Solution: "projects/Simpsons.pts"
	}

Page.AddOnload(function () {
	ProtoScriptWorkbench.InterpretImmediate(Page.LocalSettings.Solution, "SimpsonsOntology.Homer", {}, function () {
	projectInput.property("value", Page.LocalSettings.Solution);
	UserMessages.DisplayNow("Ready", "Success");
	})
})

function searchPrototypes(searchTerm) {
	console.log(`API: Searching for prototypes containing: "${searchTerm}"`);
	return new Promise((resolve) => {
		ProtoScriptWorkbench.GetPrototypesBySearch(Page.LocalSettings.Solution, searchTerm, function (result) {
			if (result && result.length > 0) {
				console.log(`API: Found ${result.length} matches for "${searchTerm}".`);
				resolve(result);
			} else {
				console.warn(`API: No matches found for "${searchTerm}".`);
				resolve([]);
			}
		});
	});
}

function fetchPrototypeData(prototypeName) {
	console.log(`API: Requesting data for: ${prototypeName}`);
	return new Promise((resolve, reject) => {
		ProtoScriptWorkbench.GetPrototypeAndDescendants.DisableRequestCancelling = true;
		ProtoScriptWorkbench.GetPrototypeAndDescendants(Page.LocalSettings.Solution, prototypeName, function (result) {
			if (result) {
				console.log(`API: Found data for ${prototypeName}. Returning.`);
				console.log(result);
				resolve(result);
			} else {
				console.warn(`API: No data found for ${prototypeName}.`);
				reject(`Prototype not found: ${prototypeName}`);
			}
		});

	});
}

// --- Constants ---
const nodeWidth = 170; const nodeHeight = 75; const textPadding = 12;
const titleBarHeight = 24; const layerHeight = 190; const yStrength = 0.7;
const xStrength = 0.05; const initialYOffset = 90; const debounceDelay = 300;
const maxSuggestions = 10; const rootNodeName = null;
const defaultFillColor = '#e9ecef'; const defaultStrokeColor = '#ced4da';
const defaultTitleTextColor = '#212529';

const linkColorMap = new Map();
const linkColorScale = d3.scaleOrdinal(d3.schemeTableau10);
function linkColor(label) {
	if (!linkColorMap.has(label)) {
		linkColorMap.set(label, linkColorScale(linkColorMap.size));
	}
	return linkColorMap.get(label);
}

const prototypeDataStore = {
};

const structuralKeys = new Set([
	"PrototypeID", "PrototypeName", "Descendants", "TypeOfs"
]);

function getRenderableProps(data = {}) {
	return Object.entries(data)
		.filter(([k]) => !structuralKeys.has(k) && !k.startsWith("_"))
		.map(([fullKey, v]) => {
			const key = fullKey.includes(".")
				? fullKey.slice(fullKey.lastIndexOf(".") + 1)
				: fullKey;
			const suffix = str =>
				str.includes(".") ? str.slice(str.lastIndexOf(".") + 1) : str;

			if (Array.isArray(v)) {
				v = v.map(x => {
					if (x && typeof x === "object" && "PrototypeName" in x)
						return suffix(x.PrototypeName);
					return x;
				}).join(", ");
			}
			else if (v && typeof v === "object") {
				v = ("PrototypeName" in v) ? suffix(v.PrototypeName) : "[object]";
			}

			return { key, value: String(v) };
		})
		.filter(p => p.value && p.value.trim() !== "");
}

function update() {
	updateGraphWidth();

	const currentNodes = visibleNodes;
	const currentLinks = visibleLinks;
	const { pathNodes, pathLinks } = focusNode
		? getAncestorPath(focusNode)
		: { pathNodes: new Set(), pathLinks: new Set() };

	simulation.nodes(currentNodes);
	simulation.force('link').links(currentLinks);

	currentNodes.forEach(n => {
		if (isNaN(n.x) || isNaN(n.y)) {
			n.y = n.fy ?? (n.depth * layerHeight + initialYOffset);
			n.x = n.fx ?? (n._parentRef?.x
				? n._parentRef.x + (Math.random() - .5) * 10
				: graphWidth / 2);
		}
	});

	const link = linkGroup.selectAll('g.link-group')
		.data(currentLinks, d => `${d.source.id}-${d.target.id}`);

	link.exit()
		.transition().duration(300).style('opacity', 0).remove();

	const linkEnter = link.enter().append('g')
		.attr('class', 'link-group link')
		.style('opacity', 0);

	linkEnter.append('path')
		.attr('id', d =>
			`linkpath-${d.source.id.replace(/\W/g, '_')}-${d.target.id.replace(/\W/g, '_')}`);

	linkEnter.append('text').attr('dy', -3.5)
		.append('textPath')
		.attr('href', d =>
			`#linkpath-${d.source.id.replace(/\W/g, '_')}-${d.target.id.replace(/\W/g, '_')}`)
		.style('text-anchor', 'middle')
		.attr('startOffset', '50%')
		.text(d => d.label || 'Descendant');

	const linkUpdate = link.merge(linkEnter)
		.classed('ancestor-path', d => pathLinks.has(d))
		.each(function (d) {
			const c = linkColor(d.label || 'Descendant');
			const markerId = `arrow-${c.replace(/[^\w-]/g, '')}`;
			if (!document.getElementById(markerId)) {
				defs.append('marker')
					.attr('id', markerId)
					.attr('viewBox', '-0 -5 10 10')
					.attr('refX', 10).attr('refY', 0)
					.attr('orient', 'auto')
					.attr('markerWidth', 5).attr('markerHeight', 5)
					.attr('xoverflow', 'visible')
					.append('path')
					.attr('d', 'M 0,-5 L 10 ,0 L 0,5')
					.attr('fill', c).attr('stroke', 'none');
			}

			d3.select(this).select('path')
				.attr('marker-end', `url(#${markerId})`)
				.style('stroke', c);

			d3.select(this).select('text')
				.style('fill', c);

		});

	linkUpdate.transition().duration(300).style('opacity', 1);

	const node = nodeGroup.selectAll('g.node')
		.data(currentNodes, d => d.id);

	node.exit()
		.transition().duration(400)
		.attr('transform', d =>
			`translate(${d._parentRef?.x ?? d.x}, ${d._parentRef?.y ?? d.y}) scale(0)`)
		.style('opacity', 0)
		.remove();

	const nodeEnter = node.enter().append('g')
		.attr('class', 'node')
		.attr('transform', d => {
			const px = d._parentRef?.x ?? (d.x || graphWidth / 2);
			const py = d._parentRef?.y ?? (d.y || initialYOffset);
			return `translate(${px}, ${py}) scale(0.1)`;
		})
		.style('opacity', 0)
		.on('click', nodeClicked)
		.call(d3.drag()
			.on('start', dragstarted)
			.on('drag', dragged)
			.on('end', dragended));

	nodeEnter.append('rect')
		.attr('class', 'node-bg')
		.attr('width', nodeWidth)
		.attr('height', nodeHeight)
		.attr('x', -nodeWidth / 2)
		.attr('y', -nodeHeight / 2);

	nodeEnter.append('rect')
		.attr('class', 'node-title-bar')
		.attr('width', nodeWidth)
		.attr('height', titleBarHeight)
		.attr('x', -nodeWidth / 2)
		.attr('y', -nodeHeight / 2);

	nodeEnter.append('text')
		.attr('class', 'node-name')
		.attr('x', -nodeWidth / 2 + textPadding)
		.attr('fill-opacity', 0);

	nodeEnter.append('g')
		.attr('class', 'node-properties')
		.attr('transform',
			`translate(${-nodeWidth / 2 + textPadding},
                     ${-nodeHeight / 2 + titleBarHeight + textPadding})`);

	nodeEnter.append('text')
		.attr('class', 'node-indicator')
		.attr('x', nodeWidth / 2 - textPadding)
		.attr('y', -nodeHeight / 2 + textPadding)
		.style('dominant-baseline', 'hanging')
		.attr('fill-opacity', 0);

	const nodeUpdate = node.merge(nodeEnter);

	nodeUpdate
		.classed('ancestor-path', d => pathNodes.has(d))
		.classed('is-fetching', d => d._isFetching)
		.classed('has-fetched', d => d._hasFetched)
		.classed('expanded', d => d._expanded)
		.transition().duration(500).delay(50)
		.attr('transform', d => `translate(${d.x ?? 0}, ${d.y ?? 0}) scale(1)`)
		.style('opacity', 1);

	nodeUpdate.select('rect.node-bg')
		.transition().duration(400)
		.style('stroke', d => d.strokeColor || defaultStrokeColor);

	nodeUpdate.select('rect.node-title-bar')
		.transition().duration(400)
		.style('fill', d => d.fillColor || defaultFillColor);

	nodeUpdate.select('text.node-name')
		.text(d => d.data?.PrototypeName || d.id)
		.call(wrap, nodeWidth - 2 * textPadding, 1)
		.transition().duration(300)
		.style('fill', d => d.titleTextColor || defaultTitleTextColor)
		.attr('fill-opacity', 1);

	nodeUpdate.select('text.node-indicator')
		.text(d => d._isFetching ? 'Fetching…' : '')
		.transition().duration(300)
		.attr('fill-opacity', d => d._isFetching ? 1 : 0);

	nodeUpdate.each(function (d) {
		const group = d3.select(this).select('g.node-properties');
		const props = d.data ? getRenderableProps(d.data) : [];

		const lines = group.selectAll('text.prop')
			.data(props, p => p.key);

		lines.exit()
			.transition().duration(200)
			.attr('fill-opacity', 0)
			.remove();

		const enter = lines.enter().append('text')
			.attr('class', 'prop')
			.style('font-size', '11px')
			.style('fill-opacity', 0);

		function render(lineSel, p) {
			lineSel.selectAll('*').remove();
			lineSel.append('tspan')
				.attr('class', 'prop-key')
				.style('font-weight', '600')
				.text(p.key + ': ');
			lineSel.append('tspan')
				.attr('class', 'prop-val')
				.style('font-weight', 'normal')
				.text(p.value);
		}
		enter.each(function (p) { render(d3.select(this), p); });
		lines.each(function (p) { render(d3.select(this), p); });

		enter.merge(lines)
			.attr('y', (_, i) => i * 14)
			.transition().duration(300)
			.style('fill', d.titleTextColor || defaultTitleTextColor)
			.style('fill-opacity', 1);
	});

	resizeNodeBoxes(nodeUpdate);

	simulation.force('collide')
		.radius(d => (d.nodeWidth ?? nodeWidth) / 1.6);

	thawSimulation(0.4);

	clearTimeout(idleFreezeTimer);
	idleFreezeTimer = setTimeout(() => {
		freezeSimulation();
	}, 1200);
}

function resizeNodeBoxes(selection) {
	selection.each(function (d) {
		const sel = d3.select(this);

		let maxW = 0;
		sel.selectAll('text').each(function () {
			maxW = Math.max(maxW, this.getComputedTextLength());
		});

		const lineHeight = 14;
		const propLines = sel.select('g.node-properties')
			.selectAll('text.prop')
			.size();
		const propsHeight = propLines * lineHeight;
		const w = Math.max(maxW + textPadding * 2, 90);
		const h = Math.max(titleBarHeight + propsHeight + textPadding, 50);

		d.nodeWidth = w;
		d.nodeHeight = h;

		const halfW = w / 2;
		const halfH = h / 2;
		const top = -halfH;

		sel.select('rect.node-bg')
			.attr('width', w)
			.attr('height', h)
			.attr('x', -halfW)
			.attr('y', -halfH);

		sel.select('rect.node-title-bar')
			.attr('width', w)
			.attr('x', -halfW)
			.attr('y', -halfH);

		sel.select('text.node-name')
			.attr('x', -halfW + textPadding)
			.attr('y', top + titleBarHeight / 2)
			.attr('dy', null)
			.style('dominant-baseline', 'middle');

		sel.select('text.node-indicator')
			.attr('x', halfW - textPadding)
			.attr('y', top + textPadding)
			.attr('dy', null)
			.style('dominant-baseline', 'hanging');

		sel.select('g.node-properties')
			.attr('transform',
				`translate(${-halfW + textPadding},
                            ${top + titleBarHeight + textPadding})`);
	});
}

async function nodeClicked(event, d) {
	if (event.defaultPrevented) return;
	event.stopPropagation();

	const wasExpanded = d._expanded;
	d._expanded = !wasExpanded;
	focusNode = d;
	displayNodeInfo(d);

	if (d._expanded && (!wasExpanded || d._descendantNodes.size === 0)) {

		if (!d._hasFetched && !d._isFetching) {
			d._isFetching = true;
			update();

			try {
				d.data = await fetchPrototypeData(d.id);
				d._hasFetched = true;
				d._isFetching = false;
			} catch (e) {
				console.error(`Fetch failed for ${d.id}`, e);
				d._isFetching = false;
				d._expanded = false;
				update();
				return;
			}
		}

		const relInfos = extractRelatedPrototypes(d.data);
		const newKids = [];

		for (const rel of relInfos) {
			const child = await processPrototype(rel.prototypeName, null, d);
			if (!child) continue;

			d._descendantNodes.add(child);
			child._relationLabel = rel.label;

			if (!visibleNodes.includes(child)) {
				child.x = d.x + (Math.random() - 0.5) * 20;
				child.y = d.y + layerHeight * 0.9;
				visibleNodes.push(child);
				newKids.push(child);
			}
			if (!visibleLinks.some(l => l.source === d && l.target === child)) {
				visibleLinks.push({ source: d, target: child, label: rel.label });
			}
		}

		if (newKids.length) {
			newKids.forEach((c, i) =>
				calculateAndAssignChildColor(c, d, newKids, i));
		}

		expandNode(d);
		update();
		return;
	}

	if (!d._expanded && wasExpanded) {
		collapseNode(d);
		update();
	}
}

function extractRelatedPrototypes(obj) {
	const out = [];

	const suffix = key =>
		key.includes('.') ? key.slice(key.lastIndexOf('.') + 1) : key;

	Object.entries(obj).forEach(([k, v]) => {

		if ((k !== 'Descendants') &&
			(structuralKeys.has(k) || k.startsWith('_')))
			return;

		function visit(val, label) {
			if (val && typeof val === 'object') {
				if ('PrototypeName' in val &&
					val.PrototypeName !== 'Ontology.Collection') {

					out.push({ prototypeName: val.PrototypeName, label });
				}
				else if (val.PrototypeName === 'Ontology.Collection' &&
					Array.isArray(val.Children)) {

					val.Children.forEach(child => visit(child, label));
				}
			}
		}
		visit(v, suffix(k));
	});

	if (Array.isArray(obj.Descendants)) {
		obj.Descendants.forEach(d =>
			out.push({ prototypeName: d.PrototypeName, label: 'Descendant' }));
	}

	return out;
}

let nodeMap = new Map();
let visibleNodes = [];
let visibleLinks = [];
let focusNode = null;
let searchDebounceTimer = null;

let idleFreezeTimer = null;

function thawSimulation(alpha = 0.9) {
	simulation.force('charge').strength(-900);
	simulation.alpha(alpha).restart();
}

function freezeSimulation() {
	simulation.force('charge').strength(0);
	simulation.alphaTarget(0);
}

async function processPrototype(prototypeName, rawData = null, parentNode = null) {
	if (nodeMap.has(prototypeName)) {
		const existingNode = nodeMap.get(prototypeName);
		if (parentNode && (!existingNode._parentRef || parentNode.depth + 1 < existingNode.depth)) {
			existingNode._parentRef = parentNode; existingNode.depth = parentNode.depth + 1;
			existingNode.color = null; existingNode.fillColor = null; existingNode.strokeColor = null; existingNode.titleTextColor = null;
		}
		if (!existingNode.data && rawData) existingNode.data = rawData;
		else if (!existingNode.data) {
			try { existingNode.data = await fetchPrototypeData(prototypeName); }
			catch (error) { console.error(`Failed to fetch data for ${prototypeName} during processing:`, error); }
		}
		return existingNode;
	}
	if (!rawData) {
		try { rawData = await fetchPrototypeData(prototypeName); }
		catch (error) { console.error(`Failed to fetch data for ${prototypeName}:`, error); return null; }
	}
	const node = {
		id: prototypeName, data: rawData, _expanded: false, _isFetching: false, _hasFetched: false,
		_parentRef: parentNode, depth: parentNode ? parentNode.depth + 1 : 0,
		_descendantNodes: new Set(), color: null, fillColor: null, strokeColor: null, titleTextColor: null
	};
	nodeMap.set(prototypeName, node);
	return node;
}

let graphWidth; let graphHeight = window.innerHeight;
const graphContainerElement = document.getElementById('graph-container');
const svg = d3.select("#graph-container").append('svg');

const zoom = d3.zoom().scaleExtent([0.05, 8]).on("zoom", zoomed);

svg.call(zoom)
	.on("start.zoom", function (event) {
		const sourceTarget = event.sourceEvent.target;
		const isNode = sourceTarget.closest('.node');
		if (!isNode && (sourceTarget === svg.node() || sourceTarget === graphContainerElement || sourceTarget === container.node())) {
			svg.classed("grabbing", true);
		}
	})
	.on("end.zoom", function () {
		svg.classed("grabbing", false);
	})
	.on("dblclick.zoom", null);

const defs = svg.append("defs");
defs.append("marker").attr("id", "arrowhead").attr("viewBox", "-0 -5 10 10").attr("refX", 10).attr("refY", 0).attr("orient", "auto").attr("markerWidth", 5).attr("markerHeight", 5).attr("xoverflow", "visible").append("svg:path").attr("d", "M 0,-5 L 10 ,0 L 0,5");
defs.append("marker").attr("id", "arrowhead-highlight").attr("viewBox", "-0 -5 10 10").attr("refX", 10).attr("refY", 0).attr("orient", "auto").attr("markerWidth", 5).attr("markerHeight", 5).attr("xoverflow", "visible").append("svg:path").attr("d", "M 0,-5 L 10 ,0 L 0,5");

const container = svg.append("g");
const linkGroup = container.append("g").attr("class", "links");
const nodeGroup = container.append("g").attr("class", "nodes");

const infoPanel = d3.select("#info-panel");
const infoContent = d3.select("#info-content");
const closePanelButton = d3.select("#close-panel-button");
const projectInput = d3.select("#project-input");
const loadProjectButton = d3.select("#load-project-button");
const searchInput = d3.select("#search-input");
const searchButton = d3.select("#search-button");
const suggestionsList = d3.select("#suggestions-list");
const summarizeAIButton = d3.select("#summarize-ai-button");
const aiSummarySection = d3.select("#ai-summary-section");

const simulation = d3.forceSimulation()
	.force('link', d3.forceLink().id(d => d.id).distance(130).strength(0.5))
	.force('charge', d3.forceManyBody().strength(-900))
	.force('forceX', d3.forceX(() => graphWidth / 2).strength(xStrength))
	.force('forceY', d3.forceY(d => d.depth * layerHeight + initialYOffset).strength(yStrength))
	.force('collide', d3.forceCollide().radius(nodeWidth / 1.6).strength(1))
	.on('tick', ticked);

function wrap(textSelection, width, maxLines = 1) {
	textSelection.each(function () { var text = d3.select(this), words = text.text().split(/\s+/).reverse(), word, line = [], lineNumber = 0, lineHeight = 1.1, x = text.attr("x"), y = text.attr("y"), dy = parseFloat(text.attr("dy")) || 0, tspan = text.text(null).append("tspan").attr("x", x).attr("y", y).attr("dy", dy + "em"); while (word = words.pop()) { line.push(word); tspan.text(line.join(" ")); if (tspan.node().getComputedTextLength() > width && line.length > 1) { if (lineNumber + 1 >= maxLines) { line.pop(); let truncatedText = line.join(" "); while (tspan.text(truncatedText + "...").node().getComputedTextLength() > width && truncatedText.length > 0) { truncatedText = truncatedText.slice(0, -1); } tspan.text(truncatedText + "..."); words = []; break; } else { line.pop(); tspan.text(line.join(" ")); line = [word]; tspan = text.append("tspan").attr("x", x).attr("y", y).attr("dy", ++lineNumber * lineHeight + dy + "em").text(word); } } } });
}
function getAncestorPath(node) { const pathNodes = new Set(); const pathLinks = new Set(); let currentNode = node; while (currentNode) { pathNodes.add(currentNode); if (currentNode._parentRef) { const parentLink = visibleLinks.find(l => l.target === currentNode && l.source === currentNode._parentRef); if (parentLink) { pathLinks.add(parentLink); } } currentNode = currentNode._parentRef; } return { pathNodes, pathLinks }; }

function updateGraphWidth() {
	const panelWidth = infoPanel.classed("hidden") ? 0 : infoPanel.node().getBoundingClientRect().width;
	graphWidth = graphContainerElement.clientWidth - panelWidth;
	if (simulation) simulation.force('forceX').x(graphWidth / 2);
}

function getNodeDisplayProperties(nodeData) {
	let name = nodeData.PrototypeName || nodeData.id || "Unknown Node";
	let code = nodeData["ICD10CM.Code.Field.CodeValue"] || nodeData["SnoMed.ClinicalConcept.Field.ConceptId"] || nodeData["SnoMed.ClinicalConcept.Field.PreferredTerm"] || "";
	let desc = nodeData["ICD10CM.Code.Field.Description"] || nodeData["SnoMed.ClinicalConcept.Field.FullySpecifiedName"] || "";
	if (code === name) code = "";
	if (desc === name) desc = "";
	let kangaroo = "kangaroo";
	return { name, code, desc, kangaroo };
}

function displayNodeInfo(node) {
	aiSummarySection.classed("hidden", true);
	summarizeAIButton.classed("hidden", true);
	if (!node) {
		infoContent.html("Select a node to see details.");
		if (!infoPanel.classed("hidden")) { infoPanel.classed("hidden", true); updateGraphWidth(); }
		return;
	}
	let html = `<h3>${node.id}</h3>`;
	const nodeData = node.data;
	if (!nodeData) {
		html += `<p><i>${node._isFetching ? 'Loading details...' : 'Click node again or expand to load details.'}</i></p>`;
	} else {
		for (const key in nodeData) {
			if (nodeData.hasOwnProperty(key) && key !== "PrototypeName" && !key.startsWith("_")) {
				let value = nodeData[key];
				if (Array.isArray(value)) {
					if (value.length > 0 && typeof value[0] === 'object' && value[0] !== null && value[0].PrototypeName) {
						value = value.map(item => item.PrototypeName).join(', ');
					} else { value = value.join(', '); }
				} else if (typeof value === 'object' && value !== null) {
					if (value.PrototypeName === "Ontology.Collection" && Array.isArray(value.Children)) {
						value = value.Children.join(', ');
					} else {
						value = JSON.stringify(value, null, 2);
						html += `<div class="property-item"><strong>${key}:</strong> <pre>${value}</pre></div>`;
						continue;
					}
				}
				html += `<div class="property-item"><strong>${key}:</strong> ${value}</div>`;
			}
		}
		summarizeAIButton.classed("hidden", false).datum(nodeData);
	}
	html += `<p><strong>Graph Depth:</strong> ${node.depth}</p>`;
	html += `<p><strong>Status:</strong> ${node._expanded ? 'Expanded' : 'Collapsed'}</p>`;
	if (node._hasFetched || node._isFetching) { html += `<p><strong>Fetch Status:</strong> ${node._isFetching ? 'Fetching...' : (node._hasFetched ? 'Fetched' : 'Not Fetched')}</p>`; }
	infoContent.html(html);
	if (infoPanel.classed("hidden")) {
		infoPanel.classed("hidden", false);
		updateGraphWidth();
	}
}

function calculateAndAssignChildColor(childNode, parentNode, siblings, index) { if (childNode.fillColor && childNode.strokeColor && childNode.titleTextColor) return; const parentColor = parentNode?.color || { h: 0, c: 50, l: 85, range: 360, depth: -1 }; const numSiblings = siblings.length; if (numSiblings <= 0) { childNode.fillColor = defaultFillColor; childNode.strokeColor = defaultStrokeColor; childNode.titleTextColor = defaultTitleTextColor; return; } const hueStep = parentColor.range / numSiblings; const startHue = (parentColor.h - parentColor.range / 2 + hueStep / 2 + 360) % 360; const targetHue = (startHue + index * hueStep) % 360; const childDepth = childNode.depth; const baseChroma = 65; const childChroma = Math.max(25, baseChroma - childDepth * 5); const baseLuminance = 80; const childLuminance = Math.max(35, baseLuminance - childDepth * 4); const childRange = Math.max(15, hueStep * 0.65); childNode.color = { h: targetHue, c: childChroma, l: childLuminance, range: childRange, depth: childDepth }; try { const hclColor = d3.hcl(targetHue, childChroma, childLuminance); let finalColor = hclColor.displayable() ? hclColor : hclColor.rgb(); childNode.fillColor = finalColor.toString(); childNode.strokeColor = finalColor.darker(1.3).toString(); const finalRgb = d3.rgb(childNode.fillColor); const lum = (0.299 * finalRgb.r + 0.587 * finalRgb.g + 0.114 * finalRgb.b) / 255; childNode.titleTextColor = lum > 0.52 ? '#000000' : '#ffffff'; } catch (e) { console.error("Error processing color:", { targetHue, childChroma, childLuminance }, e); childNode.fillColor = defaultFillColor; childNode.strokeColor = defaultStrokeColor; childNode.titleTextColor = defaultTitleTextColor; } }

function ticked() { linkGroup.selectAll('path').attr('d', d => { if (d.source && d.target && !isNaN(d.source.x) && !isNaN(d.source.y) && !isNaN(d.target.x) && !isNaN(d.target.y)) { return `M ${d.source.x},${d.source.y} L ${d.target.x},${d.target.y}`; } else { return ""; } }); nodeGroup.selectAll('g.node').attr('transform', d => `translate(${d.x ?? 0},${d.y ?? 0})`); }
function expandNode(node) { if (!node._descendantNodes || node._descendantNodes.size === 0) return; node._descendantNodes.forEach(childNode => { if (!visibleNodes.some(n => n === childNode)) { childNode.x = node.x + (Math.random() - 0.5) * 20; childNode.y = node.y + layerHeight * 0.9; childNode.vx = (node.vx || 0) / 2; childNode.vy = (node.vy || 0) / 2; childNode._expanded = false; visibleNodes.push(childNode); } if (!visibleLinks.some(l => l.source === node && l.target === childNode)) { visibleLinks.push({ source: node, target: childNode }); } }); }
function collapseNode(node) { if (!node._descendantNodes || node._descendantNodes.size === 0) return; const nodesToRemove = new Set(); const linksToRemove = new Set(); const queue = []; node._descendantNodes.forEach(childNode => { if (visibleNodes.includes(childNode)) { queue.push(childNode); const directLink = visibleLinks.find(l => l.source === node && l.target === childNode); if (directLink) linksToRemove.add(directLink); } }); const visitedForRemoval = new Set(); while (queue.length > 0) { const currentNodeToRemove = queue.shift(); if (visitedForRemoval.has(currentNodeToRemove)) continue; visitedForRemoval.add(currentNodeToRemove); nodesToRemove.add(currentNodeToRemove); currentNodeToRemove._expanded = false; visibleLinks.forEach(link => { if (link.source === currentNodeToRemove || link.target === currentNodeToRemove) { linksToRemove.add(link); } }); if (currentNodeToRemove._descendantNodes) { currentNodeToRemove._descendantNodes.forEach(grandChildNode => { if (visibleNodes.includes(grandChildNode) && !visitedForRemoval.has(grandChildNode)) { queue.push(grandChildNode); } }); } } visibleNodes = visibleNodes.filter(n => !nodesToRemove.has(n)); visibleLinks = visibleLinks.filter(l => !linksToRemove.has(l)); }

function dragstarted(event, d) {
	clearTimeout(idleFreezeTimer);
	idleFreezeTimer = null;

	d.fx = d.x;
	d.fy = d.y;
	d3.select(this).raise();
}

function dragged(event, d) {
	d.fx = d.x = event.x;
	d.fy = d.y = event.y;

	ticked();
}

function dragended(event, d) {
	d.fx = d.x;
	d.fy = d.y;

	idleFreezeTimer = setTimeout(freezeSimulation, 1200);
}

function zoomed(event) {
	container.attr("transform", event.transform);
}

async function addNodeAndAncestors(targetNodeName) { let targetNode = nodeMap.get(targetNodeName); const nodesToAdd = []; const linksToAdd = []; let currentNode = targetNode && !visibleNodes.some(n => n.id === targetNodeName) ? targetNode : null; if (!currentNode) { const targetNodeData = await fetchPrototypeData(targetNodeName); targetNode = await processPrototype(targetNodeName, targetNodeData, null); if (!targetNode) throw new Error(`Failed to process target node ${targetNodeName}`); currentNode = targetNode; } const path = []; while (currentNode && !visibleNodes.some(n => n.id === currentNode.id)) { path.unshift(currentNode); if (!currentNode.fillColor) { currentNode.fillColor = defaultFillColor; currentNode.strokeColor = defaultStrokeColor; currentNode.titleTextColor = defaultTitleTextColor; } const parentName = currentNode.data?.TypeOfs?.[0]; if (!parentName) break; let parentNode = nodeMap.get(parentName); if (!parentNode || !parentNode.data) { const parentData = await fetchPrototypeData(parentName); parentNode = await processPrototype(parentName, parentData, null); if (!parentNode) throw new Error(`Failed to process parent node ${parentName}`); } currentNode._parentRef = parentNode; currentNode.depth = parentNode.depth + 1; currentNode = parentNode; } for (let i = 0; i < path.length; i++) { const nodeInPath = path[i]; const parentNode = nodeInPath._parentRef; const parentIsVisible = parentNode && visibleNodes.some(n => n.id === parentNode.id); nodeInPath.x = parentIsVisible ? parentNode.x + (Math.random() - 0.5) * 10 : graphWidth / 2; nodeInPath.y = parentIsVisible ? parentNode.y + layerHeight * 0.8 : nodeInPath.depth * layerHeight + initialYOffset; if (!visibleNodes.some(n => n.id === nodeInPath.id)) { nodesToAdd.push(nodeInPath); } if (parentNode && !visibleLinks.some(l => l.source === parentNode && l.target === nodeInPath)) { linksToAdd.push({ source: parentNode, target: nodeInPath }); } if (parentNode && parentNode._hasFetched && parentNode.data?.Descendants) { const siblingNames = parentNode.data.Descendants.map(d => d.PrototypeName); const siblingNodes = await Promise.all(siblingNames.map(name => processPrototype(name, null, parentNode))); const validSiblings = siblingNodes.filter(n => n); const nodeIndex = validSiblings.findIndex(n => n.id === nodeInPath.id); if (nodeIndex !== -1 && parentNode.color) { calculateAndAssignChildColor(nodeInPath, parentNode, validSiblings, nodeIndex); } } } visibleNodes.push(...nodesToAdd); visibleLinks.push(...linksToAdd); return targetNode; }
function handleSearchInput() { clearTimeout(searchDebounceTimer); const searchTerm = searchInput.property("value"); if (!searchTerm || searchTerm.length < 1) { suggestionsList.html("").classed("hidden", true); return; } searchDebounceTimer = setTimeout(() => { searchPrototypes(searchTerm).then(results => { displaySuggestions(results); }); }, debounceDelay); }
function displaySuggestions(suggestions) { suggestionsList.html(""); if (suggestions.length === 0) { suggestionsList.classed("hidden", true); return; } suggestions.forEach(suggestionName => { const nodeData = prototypeDataStore[suggestionName]; const props = getNodeDisplayProperties(nodeData || { PrototypeName: suggestionName }); const displayText = props.code ? `${props.name} (${props.code})` : props.name; suggestionsList.append("li").text(displayText).attr("title", displayText).on("click", () => { searchInput.property("value", suggestionName); suggestionsList.html("").classed("hidden", true); addNodeByNameOrId(suggestionName); }); }); suggestionsList.classed("hidden", false); }
function centerNode(node) { if (!node || isNaN(node.x) || isNaN(node.y)) return; const currentTransform = d3.zoomTransform(svg.node()); const scale = currentTransform.k; const targetX = graphWidth / 2 - node.x * scale; const targetY = graphHeight / 2 - node.y * scale; svg.transition().duration(750).call(zoom.transform, d3.zoomIdentity.translate(targetX, targetY).scale(scale)); }

async function initializeGraph() {
	updateGraphWidth(); graphHeight = graphContainerElement.clientHeight;
	nodeMap.clear(); visibleNodes = []; visibleLinks = []; focusNode = null;
	if (!rootNodeName)
		return;
	try {
		const rootData = await fetchPrototypeData(rootNodeName);
		const rootNode = await processPrototype(rootNodeName, rootData);
		if (rootNode) {
			rootNode.color = { h: 180, c: 10, l: 90, range: 360, depth: 0 };
			rootNode.fillColor = defaultFillColor; rootNode.strokeColor = defaultStrokeColor; rootNode.titleTextColor = defaultTitleTextColor;
			rootNode.x = graphWidth / 2; rootNode.y = rootNode.depth * layerHeight + initialYOffset;
			visibleNodes.push(rootNode); displayNodeInfo(null);
		} else { console.error("Failed to process root node!"); displayNodeInfo(null); }

		const initialScale = 0.7;
		const initialTranslateX = graphWidth / 2 - (rootNode ? rootNode.x * initialScale : graphWidth / 2 * initialScale);
		const initialTranslateY = (graphHeight / 2 - (rootNode ? rootNode.y * initialScale : (initialYOffset + nodeHeight / 2) * initialScale)) + 30;
		svg.call(zoom.transform, d3.zoomIdentity.translate(initialTranslateX, initialTranslateY).scale(initialScale));
		update();
	} catch (error) { console.error("Failed to fetch root node data:", error); window.alert("Failed to load initial graph data. Check console for details."); }
}

searchButton.on("click", () => { addNodeByNameOrId(searchInput.property("value")); suggestionsList.html("").classed("hidden", true); searchInput.node().blur(); });
loadProjectButton.on("click", () => {
	Page.LocalSettings.Solution = projectInput.property("value");
	ProtoScriptWorkbench.InterpretImmediate(Page.LocalSettings.Solution, "SimpsonsOntology.Homer", {}, () => {
	initializeGraph();
	UserMessages.DisplayNow("Project Loaded", "Success");
	});
});
projectInput.on("keydown", (event) => {
	if (event.key === "Enter") {
		event.preventDefault();
	loadProjectButton.dispatch("click");
}
});
searchInput.on("input", handleSearchInput);
searchInput.on("keydown", (event) => { if (event.key === "Enter") { event.preventDefault(); addNodeByNameOrId(event.target.value); suggestionsList.html("").classed("hidden", true); searchInput.node().blur(); } else if (event.key === "Escape") { suggestionsList.html("").classed("hidden", true); searchInput.node().blur(); } });
closePanelButton.on("click", () => { infoPanel.classed("hidden", true); focusNode = null; updateGraphWidth(); update(); });
summarizeAIButton.on("click", function () {
	const nodeDataForAI = d3.select(this).datum();
	if (nodeDataForAI) {
		getAISummary(nodeDataForAI);
	} else {
		console.warn("No data found for AI summary button.");
		d3.select("#ai-summary-text").text("Error: Node data not available for summary.");
		aiSummarySection.classed("hidden", false);
	}
});

d3.select(window).on('click', (event) => {
	const controlsElement = d3.select('#controls').node();
	const suggestionsElement = suggestionsList.node();
	if (controlsElement && !controlsElement.contains(event.target) && suggestionsElement && !suggestionsElement.contains(event.target)) {
		suggestionsList.html("").classed("hidden", true);
	}

	const targetIsNode = event.target.closest('.node');
	const targetIsInInfoPanel = infoPanel.node().contains(event.target);
	const targetIsInControls = controlsElement.contains(event.target);

	if (!targetIsNode && !targetIsInInfoPanel && !targetIsInControls &&
		(event.target === svg.node() || event.target === graphContainerElement || event.target === container.node())) {
		if (focusNode) { focusNode = null; update(); }
	}
});
window.addEventListener('resize', () => { updateGraphWidth(); graphHeight = graphContainerElement.clientHeight; simulation.force('forceY').y(d => d.depth * layerHeight + initialYOffset); simulation.alpha(0.3).restart(); });

async function addNodeByNameOrId(searchTerm) { if (!searchTerm) return; searchTerm = searchTerm.trim(); searchButton.attr("disabled", true); try { let targetNode = nodeMap.get(searchTerm); if (targetNode && visibleNodes.some(n => n.id === targetNode.id)) { focusNode = targetNode; displayNodeInfo(focusNode); update(); centerNode(targetNode); searchButton.attr("disabled", null); return; } const targetNodeData = await fetchPrototypeData(searchTerm); targetNode = await processPrototype(searchTerm, targetNodeData, null); if (!targetNode) { throw new Error("Failed to process target node after fetch"); } const nodesToAdd = []; const linksToAdd = []; let parentNode = null; const parentName = targetNode.data.TypeOfs?.[0]; if (parentName) { parentNode = nodeMap.get(parentName); if (!parentNode) { const parentData = await fetchPrototypeData(parentName); parentNode = await processPrototype(parentName, parentData); } if (parentNode) { targetNode._parentRef = parentNode; targetNode.depth = parentNode.depth + 1; if (!visibleNodes.some(n => n.id === parentNode.id)) { nodesToAdd.push(parentNode); parentNode.x = graphWidth / 2 + (Math.random() - 0.5) * 50; parentNode.y = parentNode.depth * layerHeight + initialYOffset; if (!parentNode.color && parentNode._parentRef) { const grandparent = parentNode._parentRef; if (grandparent.data && grandparent.data.Descendants) { const siblings = await Promise.all(grandparent.data.Descendants.map(d => processPrototype(d.PrototypeName, null, grandparent))); const validSiblings = siblings.filter(s => s); const index = validSiblings.findIndex(s => s.id === parentNode.id); if (index !== -1) calculateAndAssignChildColor(parentNode, grandparent, validSiblings, index); } } else if (!parentNode.color) { parentNode.fillColor = defaultFillColor; parentNode.strokeColor = defaultStrokeColor; parentNode.titleTextColor = defaultTitleTextColor; } } } } if (parentNode && parentNode.color && parentNode.data && parentNode.data.Descendants) { const siblings = await Promise.all(parentNode.data.Descendants.map(d => processPrototype(d.PrototypeName, null, parentNode))); const validSiblings = siblings.filter(s => s); const index = validSiblings.findIndex(s => s.id === targetNode.id); if (index !== -1) calculateAndAssignChildColor(targetNode, parentNode, validSiblings, index); } else if (!targetNode.color) { targetNode.fillColor = defaultFillColor; targetNode.strokeColor = defaultStrokeColor; targetNode.titleTextColor = defaultTitleTextColor; } visibleNodes.push(...nodesToAdd); if (!visibleNodes.some(n => n.id === targetNode.id)) { targetNode.x = parentNode?.x ? parentNode.x + (Math.random() - 0.5) * 20 : graphWidth / 2; targetNode.y = parentNode?.y ? parentNode.y + layerHeight * 0.9 : targetNode.depth * layerHeight + initialYOffset; visibleNodes.push(targetNode); } if (parentNode && !visibleLinks.some(l => l.source === parentNode && l.target === targetNode)) { linksToAdd.push({ source: parentNode, target: targetNode }); } visibleLinks.push(...linksToAdd); if (targetNode.data.Descendants && targetNode.data.Descendants.length > 0) { for (const descendantInfo of targetNode.data.Descendants) { const descendantName = descendantInfo.PrototypeName; const descendantNode = nodeMap.get(descendantName); if (descendantNode && visibleNodes.some(n => n.id === descendantNode.id)) { descendantNode._parentRef = targetNode; descendantNode.depth = targetNode.depth + 1; descendantNode.fy = targetNode.y + layerHeight; descendantNode.y = targetNode.y + layerHeight; descendantNode.fx = targetNode.x + (Math.random() - 0.5) * 20; if (!visibleLinks.some(l => l.source === targetNode && l.target === descendantNode)) { visibleLinks.push({ source: targetNode, target: descendantNode }); } setTimeout(() => { if (descendantNode) { descendantNode.fx = null; descendantNode.fy = null; } }, 1000); } } } focusNode = targetNode; displayNodeInfo(focusNode); if (!targetNode._expanded) { setTimeout(() => { if (visibleNodes.some(n => n.id === targetNode.id)) { nodeClicked({ stopPropagation: () => { }, defaultPrevented: false }, targetNode); } }, 100); } else { update(); } centerNode(targetNode); } catch (error) { console.error("Error adding node:", error); window.alert(`Failed to add node "${searchTerm}". It might not exist. Check console.`); } finally { searchButton.attr("disabled", null); } }

document.addEventListener('DOMContentLoaded', () => { initializeGraph(); });
