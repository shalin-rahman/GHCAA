import 'dart:convert';
import 'dart:typed_data';

import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/features/elections/ballot_key.dart';
import 'package:pointycastle/asn1.dart';
import 'package:pointycastle/export.dart';

// Reads the key halves back the way the API does: ImportSubjectPublicKeyInfo
// and ImportPkcs8PrivateKey on the DER bytes.
RSAPublicKey _publicFromSpki(String base64Spki) {
  final spki = ASN1Sequence.fromBytes(base64.decode(base64Spki));
  final algorithm = spki.elements![0] as ASN1Sequence;
  expect((algorithm.elements![0] as ASN1ObjectIdentifier).objectIdentifierAsString,
      '1.2.840.113549.1.1.1');
  final bits = (spki.elements![1] as ASN1BitString).stringValues!;
  final rsa = ASN1Sequence.fromBytes(Uint8List.fromList(bits));
  return RSAPublicKey((rsa.elements![0] as ASN1Integer).integer!,
      (rsa.elements![1] as ASN1Integer).integer!);
}

RSAPrivateKey _privateFromPkcs8(String base64Pkcs8) {
  final pkcs8 = ASN1Sequence.fromBytes(base64.decode(base64Pkcs8));
  final octets = (pkcs8.elements![2] as ASN1OctetString).octets!;
  final rsa = ASN1Sequence.fromBytes(octets);
  BigInt at(int i) => (rsa.elements![i] as ASN1Integer).integer!;
  return RSAPrivateKey(at(1), at(3), at(4), at(5));
}

// Encrypts the way BallotSeal does on the server and the web spec does in its
// round trip: RSA-OAEP with SHA-256 and MGF1 SHA-256, no label.
Uint8List _seal(RSAPublicKey key, List<int> data) =>
    (OAEPEncoding.withSHA256(RSAEngine())
          ..init(true, PublicKeyParameter<RSAPublicKey>(key)))
        .process(Uint8List.fromList(data));

Uint8List _open(RSAPrivateKey key, Uint8List data) =>
    (OAEPEncoding.withSHA256(RSAEngine())
          ..init(false, PrivateKeyParameter<RSAPrivateKey>(key)))
        .process(data);

void main() {
  group('FR-39 (spec 023, 84.44): ballot key file', () {
    test('the file name matches the web for a title and for a blank one', () {
      expect(ballotKeyFileName('EC Election 2026!'),
          'ec-election-2026-returning-officer-key.pem');
      expect(ballotKeyFileName('  '), 'election-returning-officer-key.pem');
    });

    test('the PEM has 64-character lines and reads back to the same base64',
        () {
      final body = List.filled(130, 'A').join();
      final pem = ballotKeyPem(body);

      final lines = pem.split('\n');
      expect(lines.first, '-----BEGIN PRIVATE KEY-----');
      expect(lines[1].length, 64);
      expect(lines[2].length, 64);
      expect(lines[3].length, 2);
      expect(lines[4], '-----END PRIVATE KEY-----');
      expect(pem.endsWith('-----END PRIVATE KEY-----\n'), isTrue);
      expect(readBallotKeyFile(pem), body);
    });

    test('a file saved on Windows with CRLF line ends still reads', () {
      final pem = ballotKeyPem('QUJD' * 20).replaceAll('\n', '\r\n');
      expect(readBallotKeyFile(pem), 'QUJD' * 20);
    });

    test('bare base64 passes through', () {
      expect(readBallotKeyFile('  QUJD\n'), 'QUJD');
    });
  });

  group('FR-39 (spec 023, 84.44): ballot key pair', () {
    late BallotKeyPair pair;

    setUpAll(() => pair = generateBallotKeyPair(ballotKeyBits));

    test('the key is RSA 3072 with exponent 65537, as the server requires', () {
      final public = _publicFromSpki(pair.publicKey);
      expect(public.modulus!.bitLength, 3072);
      expect(public.publicExponent, BigInt.from(65537));
    });

    test('PKCS#8 holds the same modulus and the CRT values check out', () {
      final der = base64.decode(pair.privateKey);
      final pkcs8 = ASN1Sequence.fromBytes(der);
      expect((pkcs8.elements![0] as ASN1Integer).integer, BigInt.zero);
      final rsa = ASN1Sequence.fromBytes(
          (pkcs8.elements![2] as ASN1OctetString).octets!);
      BigInt at(int i) => (rsa.elements![i] as ASN1Integer).integer!;
      final p = at(4), q = at(5), d = at(3);
      expect(at(1), _publicFromSpki(pair.publicKey).modulus);
      expect(p * q, at(1));
      expect(at(6), d % (p - BigInt.one));
      expect(at(7), d % (q - BigInt.one));
      expect((q * at(8)) % p, BigInt.one);
    });

    test('a ballot sealed with the uploaded public key opens with the saved file',
        () {
      final ballot = utf8.encode('{"seat":1,"nominationId":7}');
      final sealed = _seal(_publicFromSpki(pair.publicKey), ballot);

      final fromFile = readBallotKeyFile(ballotKeyPem(pair.privateKey));
      expect(fromFile, pair.privateKey);
      expect(_open(_privateFromPkcs8(fromFile), sealed), ballot);
    });
  });
}
