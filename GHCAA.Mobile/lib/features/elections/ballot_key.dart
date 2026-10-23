import 'dart:convert';
import 'dart:math';

import 'package:file_picker/file_picker.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:pointycastle/asn1.dart';
import 'package:pointycastle/export.dart';

// Mirrors GHCAA.Web/src/app/core/utils/ballot-key.util.ts. The web makes an
// RSA-OAEP SHA-256 key of 3072 bits with exponent 65537, sends the public key
// as base64 SPKI DER, and saves the private key as a PKCS#8 PEM file. The
// server opens sealed ballots with OAEP SHA-256, so the file made here must be
// one the web could have made.
const int ballotKeyBits = 3072;

final BigInt _publicExponent = BigInt.from(65537);
const String _rsaEncryptionOid = '1.2.840.113549.1.1.1';

class BallotKeyPair {
  const BallotKeyPair({required this.publicKey, required this.privateKey});

  /// Base64 SPKI DER. This is the only half sent to the server.
  final String publicKey;

  /// Base64 PKCS#8 DER. Goes into the key file and nowhere else.
  final String privateKey;
}

/// Top level so it can run under [compute]. 3072-bit prime search takes
/// seconds on a phone and would freeze the UI on the main isolate.
BallotKeyPair generateBallotKeyPair(int bits) {
  final seed = Random.secure();
  final random = FortunaRandom()
    ..seed(KeyParameter(
        Uint8List.fromList(List.generate(32, (_) => seed.nextInt(256)))));
  final generator = RSAKeyGenerator()
    ..init(ParametersWithRandom(
        RSAKeyGeneratorParameters(_publicExponent, bits, 64), random));
  final pair = generator.generateKeyPair();
  final public = pair.publicKey;
  final private = pair.privateKey;
  return BallotKeyPair(
    publicKey: base64.encode(_spki(public)),
    privateKey: base64.encode(_pkcs8(private)),
  );
}

ASN1Sequence _algorithm() => ASN1Sequence(elements: [
      ASN1ObjectIdentifier.fromIdentifierString(_rsaEncryptionOid),
      ASN1Null(),
    ]);

Uint8List _spki(RSAPublicKey key) {
  final rsa = ASN1Sequence(elements: [
    ASN1Integer(key.modulus),
    ASN1Integer(key.publicExponent),
  ]);
  return ASN1Sequence(elements: [
    _algorithm(),
    ASN1BitString(stringValues: rsa.encode()),
  ]).encode();
}

Uint8List _pkcs8(RSAPrivateKey key) {
  final p = key.p!;
  final q = key.q!;
  final d = key.privateExponent!;
  final rsa = ASN1Sequence(elements: [
    ASN1Integer(BigInt.zero),
    ASN1Integer(key.modulus),
    ASN1Integer(key.publicExponent),
    ASN1Integer(d),
    ASN1Integer(p),
    ASN1Integer(q),
    ASN1Integer(d % (p - BigInt.one)),
    ASN1Integer(d % (q - BigInt.one)),
    ASN1Integer(q.modInverse(p)),
  ]);
  return ASN1Sequence(elements: [
    ASN1Integer(BigInt.zero),
    _algorithm(),
    ASN1OctetString(octets: rsa.encode()),
  ]).encode();
}

/// Same text as the web's toPem: 64-character lines and a trailing newline.
String ballotKeyPem(String base64Pkcs8) {
  final lines = <String>[];
  for (var i = 0; i < base64Pkcs8.length; i += 64) {
    lines.add(base64Pkcs8.substring(i, min(i + 64, base64Pkcs8.length)));
  }
  return '-----BEGIN PRIVATE KEY-----\n${lines.join('\n')}'
      '\n-----END PRIVATE KEY-----\n';
}

final RegExp _pemMarker = RegExp(r'-----(BEGIN|END) [A-Z ]+-----');
final RegExp _whitespace = RegExp(r'\s+');

/// Turns key file text back into the base64 the count route takes. Accepts
/// the PEM the web or this app writes, or bare base64.
String readBallotKeyFile(String text) =>
    text.replaceAll(_pemMarker, '').replaceAll(_whitespace, '');

/// Same name the web downloads, for example
/// ec-election-2026-returning-officer-key.pem.
String ballotKeyFileName(String title) {
  final slug = title
      .trim()
      .toLowerCase()
      .replaceAll(RegExp(r'[^a-z0-9]+'), '-')
      .replaceAll(RegExp(r'^-+|-+$'), '');
  return '${slug.isEmpty ? 'election' : slug}-returning-officer-key.pem';
}

/// Device side of the ballot key steps. Wrapped in a provider so widget
/// tests can swap out the isolate and the platform file dialogs.
class BallotKeyTools {
  const BallotKeyTools();

  Future<BallotKeyPair> generate() =>
      compute(generateBallotKeyPair, ballotKeyBits);

  /// Returns false when the officer cancels the save dialog.
  Future<bool> save(String fileName, String pem) async {
    final path = await FilePicker.saveFile(
      dialogTitle: 'Save returning officer key',
      fileName: fileName,
      bytes: Uint8List.fromList(utf8.encode(pem)),
    );
    return path != null;
  }

  /// Returns the key file text, or null when the officer cancels.
  Future<String?> pick() async {
    final result = await FilePicker.pickFiles(
      dialogTitle: 'Choose returning officer key',
      type: FileType.custom,
      allowedExtensions: const ['pem', 'txt'],
      withData: true,
    );
    final bytes = result?.files.single.bytes;
    // On a phone the picker leaves a copy of the private key in the app cache.
    if (result != null && !kIsWeb &&
        (defaultTargetPlatform == TargetPlatform.android ||
            defaultTargetPlatform == TargetPlatform.iOS)) {
      await FilePicker.clearTemporaryFiles();
    }
    return bytes == null ? null : utf8.decode(bytes);
  }
}

final ballotKeyToolsProvider =
    Provider<BallotKeyTools>((ref) => const BallotKeyTools());
