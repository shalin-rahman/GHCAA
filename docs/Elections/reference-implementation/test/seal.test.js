'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const lib = require('../lib/ballot-crypto');

const browser = import('../public/ballot-browser.mjs');

test('a ballot sealed the browser way opens on the server', async () => {
    const { generateElectionKey, sealBallot } = await browser;
    const key = await generateElectionKey();
    const sealed = await sealBallot('Asha', 7, key.publicKey);

    const opened = lib.openBallot(sealed, lib.parsePrivateKey(key.privateKeyPem), 7);
    assert.equal(opened.choice, 'Asha');
});

test('the browser fingerprint equals the server fingerprint of both keys', async () => {
    const { generateElectionKey } = await browser;
    const key = await generateElectionKey();

    assert.equal(lib.fingerprintOfPublicKeyBase64(key.publicKey), key.fingerprint);
    assert.equal(lib.fingerprintOfPrivateKey(lib.parsePrivateKey(key.privateKeyPem)), key.fingerprint);
    assert.ok(lib.isAcceptablePublicKey(key.publicKey));
    assert.match(key.privateKeyPem, /^-----BEGIN PRIVATE KEY-----\n/);
});

test('a key from another election has another fingerprint', async () => {
    const { generateElectionKey } = await browser;
    const a = await generateElectionKey();
    const b = await generateElectionKey();

    assert.notEqual(lib.fingerprintOfPrivateKey(lib.parsePrivateKey(b.privateKeyPem)), a.fingerprint);
});

test('a ballot cannot be replayed into another election', async () => {
    const { generateElectionKey, sealBallot } = await browser;
    const key = await generateElectionKey();
    const sealed = await sealBallot('Asha', 7, key.publicKey);

    assert.throws(() => lib.openBallot(sealed, lib.parsePrivateKey(key.privateKeyPem), 8));
});

test('a changed ballot is refused', async () => {
    const { generateElectionKey, sealBallot } = await browser;
    const key = await generateElectionKey();
    const raw = Buffer.from(await sealBallot('Asha', 7, key.publicKey), 'base64');
    raw[raw.length - 1] ^= 1;

    assert.throws(() => lib.openBallot(raw.toString('base64'), lib.parsePrivateKey(key.privateKeyPem), 7));
});

test('sealed ballots for different choices have the same length', async () => {
    const { generateElectionKey, sealBallot } = await browser;
    const key = await generateElectionKey();
    const short = await sealBallot('A', 1, key.publicKey);
    const long = await sealBallot('A much longer candidate name', 1, key.publicKey);

    assert.equal(Buffer.from(short, 'base64').length, Buffer.from(long, 'base64').length);
});

test('the tally counts choices and spoils what it cannot read', async () => {
    const { generateElectionKey, sealBallot } = await browser;
    const key = await generateElectionKey();
    const other = await generateElectionKey();
    const choices = ['Asha', 'Ben'];
    const sealed = [];
    for (const c of ['Asha', 'Asha', 'Ben', 'Asha', 'Nobody']) sealed.push(await sealBallot(c, 3, key.publicKey));
    sealed.push(await sealBallot('Ben', 3, other.publicKey));
    sealed.push('not-a-ballot');

    const result = lib.tallyBallots(sealed, lib.parsePrivateKey(key.privateKeyPem), 3, choices);

    assert.deepEqual(result.counts, { Asha: 3, Ben: 1 });
    assert.equal(result.spoiled, 3);
    assert.equal(result.total, 7);
});

test('a public key of the wrong size is refused', () => {
    const crypto = require('node:crypto');
    const weak = crypto.generateKeyPairSync('rsa', { modulusLength: 2048 }).publicKey.export({ format: 'der', type: 'spki' });

    assert.equal(lib.isAcceptablePublicKey(weak.toString('base64')), false);
    assert.equal(lib.isAcceptablePublicKey('garbage'), false);
});
