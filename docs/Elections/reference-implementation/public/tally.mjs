const $ = id => document.getElementById(id);
let keyText = null;

function showStatus(text, ok) {
    const box = $('status');
    box.textContent = text;
    box.className = `status ${ok ? 'ok' : 'fail'}`;
}

function loadFile(file) {
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => {
        keyText = String(reader.result);
        $('file-name').textContent = file.name;
        $('tally').disabled = !keyText.includes('BEGIN');
        if ($('tally').disabled) showStatus('This does not look like a key file.', false);
    };
    reader.readAsText(file);
}

function drawChart(result) {
    const most = Math.max(1, ...Object.values(result.counts));
    const rows = Object.entries(result.counts).map(([choice, count]) => {
        const row = document.createElement('div');
        row.className = 'bar-row';
        const label = Object.assign(document.createElement('span'), { className: 'bar-label', textContent: choice });
        const track = document.createElement('div');
        track.className = 'bar-track';
        const fill = document.createElement('div');
        fill.className = 'bar-fill';
        fill.style.width = `${(count / most) * 100}%`;
        track.append(fill);
        const number = Object.assign(document.createElement('span'), { className: 'bar-count', textContent: String(count) });
        row.append(label, track, number);
        return row;
    });
    $('chart').replaceChildren(...rows);
    $('summary').textContent = `${result.total} ballots opened, ${result.spoiled} spoiled.`;
    $('chart-card').classList.remove('hidden');
}

const zone = $('dropzone');
zone.addEventListener('click', () => $('file').click());
zone.addEventListener('keydown', e => { if (e.key === 'Enter') $('file').click(); });
zone.addEventListener('dragover', e => { e.preventDefault(); zone.classList.add('over'); });
zone.addEventListener('dragleave', () => zone.classList.remove('over'));
zone.addEventListener('drop', e => {
    e.preventDefault();
    zone.classList.remove('over');
    loadFile(e.dataTransfer.files[0]);
});
$('file').addEventListener('change', e => loadFile(e.target.files[0]));

$('tally').addEventListener('click', async () => {
    $('tally').disabled = true;
    try {
        const response = await fetch('/api/admin/tally-votes', {
            method: 'POST',
            headers: { 'content-type': 'application/json', 'x-user': $('user').value },
            body: JSON.stringify({ electionId: Number($('election-id').value), privateKey: keyText })
        });
        const body = await response.json();
        if (!response.ok) {
            showStatus(body.match === false ? `Fingerprint mismatch. ${body.error}` : body.error, false);
            if (body.result) drawChart(body.result);
            return;
        }
        showStatus('Fingerprint matches. The ballots were opened in memory and counted.', true);
        drawChart(body.result);
        keyText = null;
        $('file-name').textContent = '';
    } catch (err) {
        showStatus(`Could not count: ${err.message}`, false);
    } finally {
        $('tally').disabled = keyText === null;
    }
});
