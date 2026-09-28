import { ballotKeyFileName, generateBallotKeyPair, readBallotKeyFile, toPem } from './ballot-key.util';

describe('ballot-key.util', () => {
    // FR-39: the private key never needs the server, and the public key is what the server seals under.
    it('makes a key pair whose public half seals what the private half opens', async () => {
        const pair = await generateBallotKeyPair();
        const bytes = (value: string) => Uint8Array.from(atob(value), c => c.charCodeAt(0));
        const algorithm = { name: 'RSA-OAEP', hash: 'SHA-256' };
        const publicKey = await crypto.subtle.importKey('spki', bytes(pair.publicKey), algorithm, false, ['encrypt']);
        const privateKey = await crypto.subtle.importKey('pkcs8', bytes(readBallotKeyFile(toPem(pair.privateKey))), algorithm, false, ['decrypt']);

        const sealed = await crypto.subtle.encrypt({ name: 'RSA-OAEP' }, publicKey, new TextEncoder().encode('ballot'));
        const opened = await crypto.subtle.decrypt({ name: 'RSA-OAEP' }, privateKey, sealed);

        expect(new TextDecoder().decode(opened)).toBe('ballot');
    }, 30000);

    it('reads a key file with or without PEM lines', () => {
        expect(readBallotKeyFile('-----BEGIN PRIVATE KEY-----\nAB\r\nCD\n-----END PRIVATE KEY-----\n')).toBe('ABCD');
        expect(readBallotKeyFile('  ABCD \n')).toBe('ABCD');
    });

    it('names the key file after the election', () => {
        expect(ballotKeyFileName('EC Election 2026!')).toBe('ec-election-2026-returning-officer-key.pem');
        expect(ballotKeyFileName('  ')).toBe('election-returning-officer-key.pem');
    });
});
