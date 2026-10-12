'use strict';
const crypto = require('node:crypto');

const WRAPPED_KEY_BYTES = 384; // RSA 3072
const NONCE_BYTES = 12;
const TAG_BYTES = 16;

function ballotAssociatedData(electionId) {
    return Buffer.from(`ghcaa-ref-ballot:election:${electionId}`, 'utf8');
}

function fingerprintOfSpki(spkiDer) {
    return crypto.createHash('sha256').update(spkiDer).digest('hex');
}

function fingerprintOfPublicKeyBase64(spkiBase64) {
    return fingerprintOfSpki(Buffer.from(spkiBase64, 'base64'));
}

function isAcceptablePublicKey(spkiBase64) {
    try {
        const key = crypto.createPublicKey({ key: Buffer.from(spkiBase64, 'base64'), format: 'der', type: 'spki' });
        const details = key.asymmetricKeyDetails;
        return key.asymmetricKeyType === 'rsa'
            && details.modulusLength === 3072
            && Number(details.publicExponent) === 65537;
    } catch {
        return false;
    }
}

// The key file is PKCS#8 in PEM armour. Raw base64 is accepted too.
function parsePrivateKey(text) {
    const body = String(text).replace(/-----[A-Z ]+-----/g, '').replace(/\s+/g, '');
    return crypto.createPrivateKey({ key: Buffer.from(body, 'base64'), format: 'der', type: 'pkcs8' });
}

function fingerprintOfPrivateKey(privateKey) {
    const spki = crypto.createPublicKey(privateKey).export({ format: 'der', type: 'spki' });
    return fingerprintOfSpki(spki);
}

// Layout: wrapped AES key | 12-byte nonce | 16-byte tag | ciphertext.
function openBallot(sealedBase64, privateKey, electionId) {
    const raw = Buffer.from(sealedBase64, 'base64');
    if (raw.length < WRAPPED_KEY_BYTES + NONCE_BYTES + TAG_BYTES + 1) throw new Error('sealed ballot too short');
    const wrapped = raw.subarray(0, WRAPPED_KEY_BYTES);
    const nonce = raw.subarray(WRAPPED_KEY_BYTES, WRAPPED_KEY_BYTES + NONCE_BYTES);
    const tag = raw.subarray(WRAPPED_KEY_BYTES + NONCE_BYTES, WRAPPED_KEY_BYTES + NONCE_BYTES + TAG_BYTES);
    const cipherText = raw.subarray(WRAPPED_KEY_BYTES + NONCE_BYTES + TAG_BYTES);

    const aesKey = crypto.privateDecrypt(
        { key: privateKey, padding: crypto.constants.RSA_PKCS1_OAEP_PADDING, oaepHash: 'sha256' },
        wrapped
    );
    const decipher = crypto.createDecipheriv('aes-256-gcm', aesKey, nonce);
    decipher.setAAD(ballotAssociatedData(electionId));
    decipher.setAuthTag(tag);
    const plain = Buffer.concat([decipher.update(cipherText), decipher.final()]).toString('utf8');
    return JSON.parse(plain);
}

// Ballots are opened in random order, never the order they were cast.
function shuffle(list) {
    const copy = [...list];
    for (let i = copy.length - 1; i > 0; i--) {
        const j = crypto.randomInt(i + 1);
        [copy[i], copy[j]] = [copy[j], copy[i]];
    }
    return copy;
}

// A ballot that will not open, or names an unknown choice, is spoiled.
function tallyBallots(sealedList, privateKey, electionId, choices) {
    const counts = Object.fromEntries(choices.map(c => [c, 0]));
    let spoiled = 0;
    for (const sealed of shuffle(sealedList)) {
        try {
            const { choice } = openBallot(sealed, privateKey, electionId);
            if (Object.hasOwn(counts, choice)) counts[choice]++;
            else spoiled++;
        } catch {
            spoiled++;
        }
    }
    return { counts, spoiled, total: sealedList.length };
}

module.exports = {
    fingerprintOfPublicKeyBase64,
    fingerprintOfPrivateKey,
    isAcceptablePublicKey,
    parsePrivateKey,
    openBallot,
    tallyBallots
};
