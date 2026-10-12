// Browser side of the flow. It also loads in Node 20+, where crypto is global, so the test can run it.
const WRAPPED_KEY_BYTES = 384;
const NONCE_BYTES = 12;
const TAG_BYTES = 16;
const BLOCK_BYTES = 512;

const enc = new TextEncoder();

export function toBase64(bytes) {
    let text = '';
    for (const b of new Uint8Array(bytes)) text += String.fromCharCode(b);
    return btoa(text);
}

export function fromBase64(text) {
    return Uint8Array.from(atob(text), c => c.charCodeAt(0));
}

export function associatedData(electionId) {
    return enc.encode(`ghcaa-ref-ballot:election:${electionId}`);
}

// RSA-OAEP 3072 with SHA-256. The private key is extractable once, so it can be saved to a file.
export async function generateElectionKey() {
    const pair = await crypto.subtle.generateKey(
        { name: 'RSA-OAEP', modulusLength: 3072, publicExponent: new Uint8Array([1, 0, 1]), hash: 'SHA-256' },
        true,
        ['encrypt', 'decrypt']
    );
    const spki = await crypto.subtle.exportKey('spki', pair.publicKey);
    const pkcs8 = await crypto.subtle.exportKey('pkcs8', pair.privateKey);
    const digest = await crypto.subtle.digest('SHA-256', spki);
    return {
        publicKey: toBase64(spki),
        fingerprint: [...new Uint8Array(digest)].map(b => b.toString(16).padStart(2, '0')).join(''),
        privateKeyPem: toPem(pkcs8)
    };
}

export function toPem(pkcs8) {
    const lines = toBase64(pkcs8).match(/.{1,64}/g).join('\n');
    return `-----BEGIN PRIVATE KEY-----\n${lines}\n-----END PRIVATE KEY-----\n`;
}

// Seals one choice. A fresh AES-256-GCM key encrypts it, and the election public key wraps that key.
// Layout: wrapped key | 12-byte nonce | 16-byte tag | ciphertext. The JSON is padded to a 512-byte
// multiple so the length does not hint at the choice.
export async function sealBallot(choice, electionId, publicKeyBase64) {
    const publicKey = await crypto.subtle.importKey(
        'spki', fromBase64(publicKeyBase64), { name: 'RSA-OAEP', hash: 'SHA-256' }, false, ['encrypt']);
    const aesKey = await crypto.subtle.generateKey({ name: 'AES-GCM', length: 256 }, true, ['encrypt']);
    const nonce = crypto.getRandomValues(new Uint8Array(NONCE_BYTES));

    const json = JSON.stringify({ choice });
    const padded = enc.encode(json.padEnd(Math.ceil(json.length / BLOCK_BYTES) * BLOCK_BYTES, ' '));
    const sealed = new Uint8Array(await crypto.subtle.encrypt(
        { name: 'AES-GCM', iv: nonce, additionalData: associatedData(electionId), tagLength: TAG_BYTES * 8 }, aesKey, padded));
    const cipherText = sealed.slice(0, sealed.length - TAG_BYTES);
    const tag = sealed.slice(sealed.length - TAG_BYTES);

    const wrapped = new Uint8Array(await crypto.subtle.encrypt(
        { name: 'RSA-OAEP' }, publicKey, await crypto.subtle.exportKey('raw', aesKey)));
    if (wrapped.length !== WRAPPED_KEY_BYTES) throw new Error('Unexpected wrapped key size.');

    const out = new Uint8Array(wrapped.length + nonce.length + tag.length + cipherText.length);
    out.set(wrapped, 0);
    out.set(nonce, wrapped.length);
    out.set(tag, wrapped.length + nonce.length);
    out.set(cipherText, wrapped.length + nonce.length + tag.length);
    return toBase64(out);
}
