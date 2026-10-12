'use strict';
const path = require('node:path');
const express = require('express');
const { Pool } = require('pg');
const lib = require('./lib/ballot-crypto');

const MIN_BALLOTS = 10;
const pool = new Pool({ connectionString: process.env.DATABASE_URL || 'postgres://postgres:postgres@localhost:5432/ballotref' });
const app = express();
app.use(express.json({ limit: '256kb' }));
app.use(express.static(path.join(__dirname, 'public')));

const wrap = handler => (req, res, next) => handler(req, res, next).catch(err => {
    console.error(err);
    res.status(500).json({ error: 'Server error.' });
});

// Demo sign-in. The caller names a user in x-user. A real system puts its own login here.
const signedIn = wrap(async (req, res, next) => {
    const { rows } = await pool.query('SELECT id, set_ballot_key FROM users WHERE username = $1', [req.get('x-user')]);
    if (!rows.length) return res.status(401).json({ error: 'Unknown user.' });
    req.user = rows[0];
    next();
});

function needsKeyPermission(req, res, next) {
    if (!req.user.set_ballot_key) return res.status(403).json({ error: 'The SetBallotKey permission is required.' });
    next();
}

app.post('/api/admin/initialize-election', signedIn, needsKeyPermission, wrap(async (req, res) => {
    const { title, choices, publicKey } = req.body;
    if (typeof title !== 'string' || !title.trim()) return res.status(400).json({ error: 'A title is required.' });
    if (!Array.isArray(choices) || choices.length < 2 || new Set(choices).size !== choices.length
        || choices.some(c => typeof c !== 'string' || !c.trim())) {
        return res.status(400).json({ error: 'Give at least two different choices.' });
    }
    if (typeof publicKey !== 'string' || !lib.isAcceptablePublicKey(publicKey)) {
        return res.status(400).json({ error: 'The public key must be RSA 3072 with exponent 65537.' });
    }
    const fingerprint = lib.fingerprintOfPublicKeyBase64(publicKey);
    const { rows } = await pool.query(
        `INSERT INTO elections (title, choices, public_key_spki, fingerprint, created_by)
         VALUES ($1, $2, $3, $4, $5) RETURNING id`,
        [title.trim(), JSON.stringify(choices), publicKey, fingerprint, req.user.id]
    );
    res.status(201).json({ id: rows[0].id, fingerprint });
}));

app.get('/api/election/:id', wrap(async (req, res) => {
    const id = Number(req.params.id);
    if (!Number.isInteger(id)) return res.status(404).json({ error: 'No such election.' });
    const { rows } = await pool.query(
        'SELECT id, title, choices, public_key_spki, fingerprint, status FROM elections WHERE id = $1', [id]);
    if (!rows.length) return res.status(404).json({ error: 'No such election.' });
    const e = rows[0];
    res.json({ id: e.id, title: e.title, choices: e.choices, publicKey: e.public_key_spki, fingerprint: e.fingerprint, status: e.status });
}));

app.post('/api/voter/cast-ballot', signedIn, wrap(async (req, res) => {
    const { electionId, sealed } = req.body;
    if (!Number.isInteger(electionId) || typeof sealed !== 'string' || sealed.length > 4096) {
        return res.status(400).json({ error: 'A valid election and sealed ballot are required.' });
    }
    const client = await pool.connect();
    try {
        await client.query('BEGIN');
        const { rows } = await client.query('SELECT status FROM elections WHERE id = $1 FOR SHARE', [electionId]);
        if (!rows.length) { await client.query('ROLLBACK'); return res.status(404).json({ error: 'No such election.' }); }
        if (rows[0].status !== 'open') { await client.query('ROLLBACK'); return res.status(409).json({ error: 'Polling is closed.' }); }
        const marked = await client.query(
            'INSERT INTO voters_voted (election_id, user_id) VALUES ($1, $2) ON CONFLICT DO NOTHING', [electionId, req.user.id]);
        if (marked.rowCount === 0) { await client.query('ROLLBACK'); return res.status(409).json({ error: 'This voter has already voted.' }); }
        await client.query('INSERT INTO ballots (election_id, sealed) VALUES ($1, $2)', [electionId, sealed]);
        await client.query('COMMIT');
        res.status(201).json({ cast: true });
    } catch (err) {
        await client.query('ROLLBACK');
        throw err;
    } finally {
        client.release();
    }
}));

app.post('/api/admin/tally-votes', signedIn, needsKeyPermission, wrap(async (req, res) => {
    const { electionId, privateKey } = req.body;
    if (!Number.isInteger(electionId) || typeof privateKey !== 'string') {
        return res.status(400).json({ error: 'An election and a key file are required.' });
    }
    const election = (await pool.query('SELECT * FROM elections WHERE id = $1', [electionId])).rows[0];
    if (!election) return res.status(404).json({ error: 'No such election.' });
    if (election.status === 'tallied') return res.status(409).json({ error: 'This election is already tallied.', result: election.result });

    let key;
    try {
        key = lib.parsePrivateKey(privateKey);
    } catch {
        return res.status(400).json({ match: false, error: 'The file is not a readable private key.' });
    }
    if (lib.fingerprintOfPrivateKey(key) !== election.fingerprint) {
        return res.status(400).json({ match: false, error: 'This key does not match the election fingerprint.' });
    }

    const sealed = (await pool.query('SELECT sealed FROM ballots WHERE election_id = $1', [electionId])).rows.map(r => r.sealed);
    if (sealed.length < MIN_BALLOTS) {
        return res.status(409).json({ match: true, error: `Fewer than ${MIN_BALLOTS} ballots were cast. Count by hand.` });
    }
    const result = lib.tallyBallots(sealed, key, electionId, election.choices);
    await pool.query(`UPDATE elections SET status = 'tallied', tallied_at = now(), result = $2 WHERE id = $1`,
        [electionId, JSON.stringify(result)]);
    res.json({ match: true, result });
}));

if (require.main === module) {
    const port = Number(process.env.PORT) || 3000;
    app.listen(port, () => console.log(`Reference implementation on http://localhost:${port}`));
}

module.exports = app;
