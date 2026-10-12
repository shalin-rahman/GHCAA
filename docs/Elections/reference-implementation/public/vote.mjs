import { sealBallot } from './ballot-browser.mjs';

const $ = id => document.getElementById(id);
let election = null;

function showResult(text, ok) {
    const box = $('result');
    box.textContent = text;
    box.className = `status ${ok ? 'ok' : 'fail'}`;
}

$('load').addEventListener('click', async () => {
    const response = await fetch(`/api/election/${encodeURIComponent($('election-id').value)}`);
    const body = await response.json();
    if (!response.ok) return showResult(body.error, false);
    if (body.status !== 'open') return showResult('Polling is closed.', false);

    election = body;
    $('election-title').textContent = body.title;
    $('fingerprint').textContent = body.fingerprint;
    $('choice').replaceChildren(...body.choices.map(c => Object.assign(document.createElement('option'), { value: c, textContent: c })));
    $('ballot').classList.remove('hidden');
    $('result').className = 'hidden';
});

$('cast').addEventListener('click', async () => {
    if (!election) return;
    $('cast').disabled = true;
    try {
        const sealed = await sealBallot($('choice').value, election.id, election.publicKey);
        const response = await fetch('/api/voter/cast-ballot', {
            method: 'POST',
            headers: { 'content-type': 'application/json', 'x-user': $('user').value },
            body: JSON.stringify({ electionId: election.id, sealed })
        });
        const body = await response.json();
        if (!response.ok) return showResult(body.error, false);
        showResult('Your ballot was sealed in this browser and cast.', true);
    } catch (err) {
        showResult(`Could not cast the ballot: ${err.message}`, false);
    } finally {
        $('cast').disabled = false;
    }
});
