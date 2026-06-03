// API Base URL
// Docker ile çalışırken Nginx reverse proxy (/api) kullanılacak. 
// Dosyaya çift tıklanarak (file://) açıldıysa veya farklı bir geliştirme ortamıysa yerel API adresine (localhost:5265) gidecek.
const API_BASE = window.location.protocol === 'file:' || window.location.port === '5500' 
    ? 'http://localhost:5265/api/parser' 
    : '/api/parser';

// Initialize Ace Editor
const editor = ace.edit("htmlEditor");
const Range = ace.require("ace/range").Range;
editor.setTheme("ace/theme/chrome");
editor.session.setMode("ace/mode/html");
editor.setOptions({
    fontSize: "14px",
    showPrintMargin: false,
    wrap: true,
    scrollPastEnd: 0.5
});
editor.session.setUseWorker(false); // Disable syntax validation worker

// DOM Elements
const parseBtn = document.getElementById('parseBtn');
const searchBtn = document.getElementById('searchBtn');
const searchInput = document.getElementById('searchInput');
const searchType = document.getElementById('searchType');
const searchHelp = document.getElementById('searchHelp');
const searchExamples = document.getElementById('searchExamples');
const treeContainer = document.getElementById('treeContainer');
const statusBadge = document.getElementById('statusBadge');

// State
let lastRenderedDom = null;
let parserErrorMarker = null;
let parserErrorRow = null;

const searchGuides = {
    id: {
        placeholder: 'Örn: header-section',
        help: 'ID değeri ile direkt arama yapın.',
        examples: ['header-section', 'content', 'deep-node', 'footer']
    },
    bfs: {
        placeholder: 'Örn: tag="div", class="card", id="deep-node"',
        help: 'DOM ağacını seviye seviye gezer.',
        examples: ['tag="div"', 'class="card"', 'id="deep-node"', 'class="item"']
    },
    dfs: {
        placeholder: 'Örn: tag="div", class="card", id="deep-node"',
        help: 'DOM ağacında bir dalı sonuna kadar takip eder.',
        examples: ['tag="div"', 'class="card"', 'id="deep-node"', 'href="#home"']
    }
};

// Dropdown değiştiğinde placeholder'ı güncelle
searchType.addEventListener('change', updateSearchGuide);

updateSearchGuide();

// Template Buttons Logic
const templateBtns = document.querySelectorAll('.template-btn');

templateBtns.forEach(btn => {
    btn.addEventListener('click', async (e) => {
        // Remove active class from all
        templateBtns.forEach(b => b.classList.remove('active'));
        // Add active class to clicked
        const clickedBtn = e.target.closest('.template-btn');
        clickedBtn.classList.add('active');

        const template = clickedBtn.dataset.template;

        if (template === 'custom') {
            const customTemplate = `<div id="root" class="container" data-version="1.0">
    <header id="header-section" class="nav-bar">
        <h1>Custom HTML</h1>
        <nav>
            <a href="#home" class="item active">Home</a>
            <a href="#about" class="item">About</a>
        </nav>
    </header>
    <main id="content">
        <section class="card">
            <p>Buraya kendi HTML kodunuzu yazabilirsiniz.</p>
            <div id="deep-node" class="item">Arama örneği düğümü</div>
        </section>
    </main>
    <footer id="footer">Custom footer</footer>
</div>`;
            editor.setValue(customTemplate, -1);
        } else {
            try {
                setLoading(true);
                const response = await fetch(`ornek/${template}.html`);
                if (!response.ok) {
                    throw new Error(`${template}.html dosyası bulunamadı. Lütfen dosyayı ekleyin.`);
                }
                const htmlContent = await response.text();
                editor.setValue(htmlContent, -1);
                showStatus(`${template}.html başarıyla yüklendi`, 'success');
            } catch (error) {
                showStatus(error.message, 'error');
                console.error(error);
            } finally {
                setLoading(false);
            }
        }
    });
});

// Event Listeners
parseBtn.addEventListener('click', async () => {
    try {
        setLoading(true);
        await parseCurrentHtml();
    } finally {
        setLoading(false);
    }
});

searchBtn.addEventListener('click', async () => {
    const query = searchInput.value.trim();
    if (!query) return;

    const type = searchType.value;
    if (!isValidSearchQuery(type, query)) {
        showStatus('BFS/DFS için format: key="value" veya key=value. Örn: class="container"', 'error');
        return;
    }

    const htmlContent = editor.getValue();

    try {
        setLoading(true);

        if (!lastRenderedDom) {
            const parsed = await parseCurrentHtml();
            if (!parsed) return;
        }

        const response = await fetch(`${API_BASE}/search`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ htmlContent, query, searchType: type })
        });

        if (!response.ok) {
            throwApiError(await getApiErrorMessage(response, 'Arama Hatası'));
        }

        const data = await response.json();
        highlightResults(data.results);
        
        if(data.count > 0) {
            showStatus(`${data.elapsedMs.toFixed(2)} ms sürede ${data.count} sonuç bulundu`, 'success');
        } else {
            showStatus(`Sonuç bulunamadı: ${getSearchTypeLabel(type)}`, 'error');
        }
    } catch (error) {
        console.error(error);
        if (error.isApiError) {
            lastRenderedDom = null;
            clearStats();
            showStatus('HTML Hatası', 'error');
            renderParserError(error.message);
            highlightParserErrorLine(error.message);
        } else {
            showStatus('Arama Başarısız', 'error');
        }
    } finally {
        setLoading(false);
    }
});

function updateSearchGuide() {
    const guide = searchGuides[searchType.value] || searchGuides.id;
    searchInput.placeholder = guide.placeholder;
    searchHelp.textContent = guide.help;
    searchExamples.innerHTML = '';

    guide.examples.forEach(example => {
        const button = document.createElement('button');
        button.type = 'button';
        button.className = 'example-chip';
        button.textContent = example;
        button.addEventListener('click', () => {
            searchInput.value = example;
            searchInput.focus();
        });
        searchExamples.appendChild(button);
    });
}

function isValidSearchQuery(type, query) {
    if (type === 'id') return true;
    return /^[a-zA-Z][\w:-]*\s*=\s*("[^"]+"|'[^']+'|[^\s=]+)$/.test(query);
}

function getSearchTypeLabel(type) {
    if (type === 'bfs') return 'BFS ile seviye seviye arandı';
    if (type === 'dfs') return 'DFS ile derinlemesine arandı';
    return 'ID hash tablosu ile direkt arandı';
}

async function parseCurrentHtml() {
    const htmlContent = editor.getValue();
    
    if (!htmlContent.trim()) {
        lastRenderedDom = null;
        clearStats();
        clearParserErrorHighlight();
        showStatus('HTML içeriği boş!', 'error');
        return false;
    }

    try {
        const response = await fetch(`${API_BASE}/parse`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ htmlContent })
        });

        if (!response.ok) {
            throwApiError(await getApiErrorMessage(response, 'API Hatası'));
        }

        const data = await response.json();
        lastRenderedDom = data.tree;
        clearParserErrorHighlight();
        renderTree(data.tree);
        showStats(data.totalNodes, data.treeDepth, data.elapsedMs);
        showStatus('Ağaç Oluşturuldu', 'success');
        return true;
    } catch (error) {
        console.error(error);
        lastRenderedDom = null;
        clearStats();
        const message = error.isApiError ? error.message : 'Sunucuya Bağlanılamadı';
        showStatus(error.isApiError ? 'HTML Hatası' : message, 'error');
        renderParserError(message);
        highlightParserErrorLine(message);
        return false;
    }
}

function setLoading(isLoading) {
    parseBtn.disabled = isLoading;
    searchBtn.disabled = isLoading;
    if (isLoading) {
        parseBtn.innerHTML = 'İşleniyor...';
    } else {
        parseBtn.innerHTML = 'Ağacı Oluştur';
    }
}

function showStatus(text, type) {
    statusBadge.textContent = text;
    if (type === 'error') {
        statusBadge.style.color = '#b91c1c';
        statusBadge.style.background = '#fee2e2';
        statusBadge.style.borderColor = '#fecaca';
    } else {
        statusBadge.style.color = '#15803d';
        statusBadge.style.background = '#dcfce7';
        statusBadge.style.borderColor = '#bbf7d0';
    }
}

function showStats(totalNodes, treeDepth, elapsedMs) {
    const statsContainer = document.getElementById('statsContainer');
    if (statsContainer) {
        statsContainer.innerHTML = `
            <div class="stat-item">
                <span class="stat-label">Düğüm Sayısı</span>
                <span class="stat-value">${totalNodes}</span>
            </div>
            <div class="stat-item">
                <span class="stat-label">Ağaç Derinliği</span>
                <span class="stat-value">${treeDepth}</span>
            </div>
            <div class="stat-item">
                <span class="stat-label">Parse Süresi</span>
                <span class="stat-value">${elapsedMs.toFixed(2)} ms</span>
            </div>
        `;
    }
}

function clearStats() {
    const statsContainer = document.getElementById('statsContainer');
    if (statsContainer) {
        statsContainer.innerHTML = '';
    }
}

async function getApiErrorMessage(response, fallback) {
    const contentType = response.headers.get('content-type') || '';

    if (contentType.includes('application/json')) {
        const body = await response.json();
        return body.message || body.title || body.detail || fallback;
    }

    const message = await response.text();
    return message || fallback;
}

function throwApiError(message) {
    const error = new Error(message);
    error.isApiError = true;
    throw error;
}

function renderParserError(message) {
    treeContainer.innerHTML = '';

    const emptyState = document.createElement('div');
    emptyState.className = 'empty-state';

    const errorText = document.createElement('p');
    errorText.style.color = '#ef4444';
    errorText.textContent = message;

    emptyState.appendChild(errorText);
    treeContainer.appendChild(emptyState);
}

function highlightParserErrorLine(message) {
    clearParserErrorHighlight();

    const match = message.match(/Satır:\s*(\d+)/);
    if (!match) return;

    const lineNumber = Number(match[1]);
    if (!Number.isInteger(lineNumber) || lineNumber < 1) return;

    const row = lineNumber - 1;
    const lineLength = editor.session.getLine(row).length;
    parserErrorMarker = editor.session.addMarker(new Range(row, 0, row, Math.max(lineLength, 1)), 'parser-error-line', 'fullLine', false);
    parserErrorRow = row;
    editor.session.addGutterDecoration(row, 'parser-error-gutter');
    editor.session.setAnnotations([{ row, column: 0, text: message, type: 'error' }]);
    editor.gotoLine(lineNumber, 0, true);
    editor.focus();
}

function clearParserErrorHighlight() {
    if (parserErrorMarker !== null) {
        editor.session.removeMarker(parserErrorMarker);
        parserErrorMarker = null;
    }

    if (parserErrorRow !== null) {
        editor.session.removeGutterDecoration(parserErrorRow, 'parser-error-gutter');
        parserErrorRow = null;
    }

    editor.session.clearAnnotations();
}

// Tree Rendering Logic
function renderTree(nodeData) {
    treeContainer.innerHTML = '';
    const rootEl = createTreeNode(nodeData);
    treeContainer.appendChild(rootEl);
}

const selfClosingTags = ['img', 'br', 'hr', 'input', 'meta', 'link'];

function createTreeNode(node) {
    const container = document.createElement('div');
    container.className = 'tree-node';
    
    // Benzersiz ID imzasını oluştur (Arama vurgusu için)
    const nodeSignature = btoa(encodeURIComponent(node.tagName + (node.id || '') + (node.className || '')));
    container.setAttribute('data-sig', nodeSignature);

    const item = document.createElement('div');
    item.className = 'tree-item';
    
    // Durumları kontrol et
    const hasChildren = node.children && node.children.length > 0;
    const hasText = node.innerText && node.innerText.trim().length > 0;
    const isSelfClosing = selfClosingTags.includes(node.tagName.toLowerCase());
    
    // İçinde metin veya çocuk varsa açılır kapanır (collapsible) olsun
    const isCollapsible = hasChildren || hasText;
    
    if (isCollapsible) {
        const caret = document.createElement('span');
        caret.className = 'caret open';
        item.appendChild(caret);
        
        // Sadece ok ikonuna değil, tüm satıra tıklandığında açılıp kapanmasını sağlıyoruz
        item.addEventListener('click', (e) => {
            e.stopPropagation();
            caret.classList.toggle('open');
            const innerContainer = container.querySelector(':scope > .children-container');
            if (innerContainer) innerContainer.classList.toggle('open');
        });
    } else {
        // İkona denk gelen hizalama boşluğu
        const spacer = document.createElement('span');
        spacer.style.width = '11px';
        spacer.style.display = 'inline-block';
        item.appendChild(spacer);
    }

    // Açılış Etiketi ve Nitelikler (Attributes)
    const tagContent = document.createElement('span');
    let attrsHtml = '';
    if (node.attributes) {
        for (const [key, value] of Object.entries(node.attributes)) {
            attrsHtml += `<span class="attr-key">${key}</span>=<span class="attr-value">"${value}"</span>`;
        }
    }

    const closeChar = isSelfClosing ? ' /&gt;' : '&gt;';
    tagContent.innerHTML = `<span class="tag-bracket">&lt;</span><span class="tag-name">${node.tagName}</span>${attrsHtml}<span class="tag-bracket">${closeChar}</span>`;
    item.appendChild(tagContent);
    container.appendChild(item);

    // Eğer içi doluysa (Metin veya Çocuk)
    if (isCollapsible) {
        const innerContainer = document.createElement('div');
        innerContainer.className = 'children-container open';
        
        // Önce metni (Inner Text) ekle
        if (hasText) {
            const textNode = document.createElement('div');
            textNode.className = 'node-text';
            textNode.textContent = node.innerText.trim();
            innerContainer.appendChild(textNode);
        }
        
        // Sonra alt çocukları (Children) ekle
        if (hasChildren) {
            node.children.forEach(child => {
                innerContainer.appendChild(createTreeNode(child));
            });
        }
        // Eğer içi doluysa kapanış etiketini de içine koyalım ki kapanınca o da gizlensin
        if (!isSelfClosing) {
            const closingItem = document.createElement('div');
            closingItem.className = 'tree-item';
            closingItem.style.marginLeft = '11px'; // Girinti hizalaması
            closingItem.innerHTML = `<span class="tag-bracket">&lt;/</span><span class="tag-name">${node.tagName}</span><span class="tag-bracket">&gt;</span>`;
            innerContainer.appendChild(closingItem);
        }
        
        container.appendChild(innerContainer);
    } else {
        // İçi boş ama kapanması gereken etiketler (örn: <div></div>)
        if (!isSelfClosing) {
            const closingItem = document.createElement('div');
            closingItem.className = 'tree-item';
            closingItem.style.marginLeft = '11px'; // Girinti hizalaması
            closingItem.innerHTML = `<span class="tag-bracket">&lt;/</span><span class="tag-name">${node.tagName}</span><span class="tag-bracket">&gt;</span>`;
            container.appendChild(closingItem);
        }
    }

    return container;
}

function highlightResults(results) {
    // Remove old highlights
    document.querySelectorAll('.tree-item.highlighted').forEach(el => el.classList.remove('highlighted'));
    
    if (!results || results.length === 0) return;

    // We need to match the returned nodes with the DOM nodes. 
    // Since we created signatures based on tag+id+class, we'll try to find them.
    results.forEach(res => {
        const sig = btoa(encodeURIComponent(res.tagName + (res.id || '') + (res.className || '')));
        const matchedNodes = document.querySelectorAll(`[data-sig="${sig}"] > .tree-item`);
        
        matchedNodes.forEach(item => {
            item.classList.add('highlighted');
            // Expand all parents
            let parent = item.parentElement;
            while (parent && parent.id !== 'treeContainer') {
                if (parent.classList.contains('children-container')) {
                    parent.classList.add('open');
                    const prevSibling = parent.previousElementSibling; // text or item
                    // if item has caret
                    const caret = parent.parentElement.querySelector('.caret');
                    if (caret) caret.classList.add('open');
                }
                parent = parent.parentElement;
            }
            // Scroll to view
            item.scrollIntoView({ behavior: 'smooth', block: 'center' });
        });
    });
}
