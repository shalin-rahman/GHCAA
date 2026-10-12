import { generateElectionKey } from './ballot-browser.mjs';

const $ = id => document.getElementById(id);
const boxes = ['ack-lost', 'ack-hold', 'ack-store'].map($);

function showResult(text, ok) {
    const box = $('result');
    box.textContent = text;
    box.className = `status ${ok ? 'ok' : 'fail'}`;
}

function downloadOnce(text, fileName) {
    const url = URL.createObjectURL(new Blob([text], { type: 'application/x-pem-file' }));
    const link = Object.assign(document.createElement('a'), { href: url, download: fileName });
    document.body.append(link);
    link.click();
    link.remove();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
}

for (const box of boxes) {
    box.addEventListener('change', () => { $('continue').disabled = !boxes.every(b => b.checked); });
}

$('continue').addEventListener('click', () => {
    if (!boxes.every(b => b.checked)) return;
    $('step-warning').classList.add('hidden');
    $('step-form').classList.remove('hidden');
});

$('create').addEventListener('click', async () => {
    const title = $('title').value.trim();
    const choices = $('choices').value.split('\n').map(c => c.trim()).filter(Boolean);
    if (!title || choices.length < 2) return showResult('Give a title and at least two choices.', false);

    $('create').disabled = true;
    try {
        const key = await generateElectionKey();
        const response = await fetch('/api/admin/initialize-election', {
            method: 'POST',
            headers: { 'content-type': 'application/json', 'x-user': $('user').value },
            body: JSON.stringify({ title, choices, publicKey: key.publicKey })
        });
        const body = await response.json();
        if (!response.ok) return showResult(body.error, false);
        if (body.fingerprint !== key.fingerprint) return showResult('The server fingerprint differs from the browser fingerprint. Nothing was saved to disk.', false);

        const slug = title.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '') || 'election';
        downloadOnce(key.privateKeyPem, `${slug}-returning-officer-key.pem`);
        showResult(`Election ${body.id} created. Fingerprint ${body.fingerprint}. The key file has been downloaded. This is the only copy. Announce the fingerprint to the committee.`, true);
    } catch (err) {
        showResult(`Could not create the election: ${err.message}`, false);
    } finally {
        $('create').disabled = false;
    }
});
