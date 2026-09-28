// Spec 023 FR-001. The returning officer's key pair is made in the browser. Only the public half
// is sent to the server. The private half is saved to the officer's own device and is needed
// again at the count.
export const BALLOT_KEY_BITS = 3072;

const ALGORITHM: RsaHashedKeyGenParams = {
    name: 'RSA-OAEP',
    modulusLength: BALLOT_KEY_BITS,
    publicExponent: new Uint8Array([1, 0, 1]),
    hash: 'SHA-256'
};

export interface BallotKeyPair {
    publicKey: string;
    privateKey: string;
}

export async function generateBallotKeyPair(): Promise<BallotKeyPair> {
    const pair = await crypto.subtle.generateKey(ALGORITHM, true, ['encrypt', 'decrypt']);
    const spki = await crypto.subtle.exportKey('spki', pair.publicKey);
    const pkcs8 = await crypto.subtle.exportKey('pkcs8', pair.privateKey);
    return { publicKey: toBase64(spki), privateKey: toBase64(pkcs8) };
}

// The key file holds the base64 PKCS#8 text. PEM armour is accepted too, in case the officer
// made the key with openssl.
export function readBallotKeyFile(text: string): string {
    return text.replace(/-----(BEGIN|END) [A-Z ]+-----/g, '').replace(/\s+/g, '');
}

export function ballotKeyFileName(electionTitle: string): string {
    const slug = electionTitle.trim().toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-+|-+$/g, '');
    return `${slug || 'election'}-returning-officer-key.pem`;
}

export function toPem(privateKey: string): string {
    const lines = privateKey.match(/.{1,64}/g) ?? [];
    return `-----BEGIN PRIVATE KEY-----\n${lines.join('\n')}\n-----END PRIVATE KEY-----\n`;
}

function toBase64(buffer: ArrayBuffer): string {
    let binary = '';
    for (const byte of new Uint8Array(buffer)) binary += String.fromCharCode(byte);
    return btoa(binary);
}
