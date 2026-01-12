// app.js - handles Upload/Parse/Import and populates the tabs
async function postForm(url, formData) {
  const res = await fetch(url, { method: 'POST', body: formData });
  return res.json();
}

function logAction(msg) {
  const ul = document.getElementById('actionLog');
  const li = document.createElement('li');
  li.textContent = `${new Date().toLocaleTimeString()} — ${msg}`;
  ul.prepend(li);
}

function renderFields(containerId, record) {
  const c = document.getElementById(containerId);
  if (!record || Object.keys(record).length === 0) {
    c.innerHTML = '<div class="text-muted">No data</div>';
    return;
  }
  let html = '';
  for (const [k,v] of Object.entries(record)) {
    html += `<div class="key">${k}</div><div class="val">${(v===null||v===undefined)?'':String(v)}</div>`;
  }
  c.innerHTML = html;
}

document.getElementById('btnParse').addEventListener('click', async () => {
  const fileEl = document.getElementById('pdfFile');
  const pathVal = document.getElementById('pdfPath').value.trim();
  if (!fileEl.files.length && !pathVal) { alert('Choose a PDF file or enter a server path'); return; }

  const fd = new FormData();
  if (fileEl.files.length) fd.append('file', fileEl.files[0]);
  if (pathVal) fd.append('path', pathVal);

  document.getElementById('rawOut').textContent = 'Parsing...';
  logAction('Parsing started');

  try {
    const json = await postForm('/upload', fd);
    document.getElementById('rawOut').textContent = JSON.stringify(json, null, 2);

    const model = json.model || json.parsed || json;
    const table = model && (model.table || (model.record && model.record.doc_type) || '');
    const rec = model && (model.record || model.parsed || {});

    if (table && table.toLowerCase() === 'wps') {
      renderFields('wps-fields', rec);
      new bootstrap.Tab(document.querySelector('#tab-wps')).show();
      document.getElementById('detectedBadge').textContent = 'Detected: WPS';
    } else if (table && table.toLowerCase() === 'pqr') {
      renderFields('pqr-fields', rec);
      new bootstrap.Tab(document.querySelector('#tab-pqr')).show();
      document.getElementById('detectedBadge').textContent = 'Detected: PQR';
    } else if (table && table.toLowerCase() === 'wpq') {
      renderFields('wpq-fields', rec);
      new bootstrap.Tab(document.querySelector('#tab-wpq')).show();
      document.getElementById('detectedBadge').textContent = 'Detected: WPQ';
    } else {
      document.getElementById('detectedBadge').textContent = 'Detected: Unknown';
    }

    logAction('Parsing completed');
  } catch (err) {
    document.getElementById('rawOut').textContent = 'Error: ' + err;
    logAction('Parsing error');
  }
});

document.getElementById('btnImport').addEventListener('click', async () => {
  const fileEl = document.getElementById('pdfFile');
  const pathVal = document.getElementById('pdfPath').value.trim();
  if (!fileEl.files.length && !pathVal) { alert('Choose a PDF file or enter a server path'); return; }

  const fd = new FormData();
  if (fileEl.files.length) fd.append('file', fileEl.files[0]);
  if (pathVal) fd.append('path', pathVal);

  document.getElementById('rawOut').textContent = 'Importing...';
  logAction('Import started');

  try {
    const json = await postForm('/import', fd);
    document.getElementById('rawOut').textContent = JSON.stringify(json, null, 2);
    if (json.result || json.imported) {
      alert('Import successful');
      logAction('Import successful');
    } else {
      logAction('Import finished with no result');
    }
  } catch (err) {
    document.getElementById('rawOut').textContent = 'Error: ' + err;
    logAction('Import error');
  }
});
