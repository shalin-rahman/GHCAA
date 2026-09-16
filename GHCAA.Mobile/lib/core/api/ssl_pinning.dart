// 7.16: certificate pinning for the mobile API connection.
//
// Pinning is enabled only when AppConfig.environment == 'production'. Dev
// talks to plain HTTP on localhost/10.0.2.2, and preprod sits behind a
// shared cert that rotates independently of app releases, so pinning either
// of those would break the build without protecting anything real.
//
// This file holds the environment gate and the fingerprint constants/helpers
// only, so it can be imported and unit-tested from both web and VM/mobile
// targets. The dart:io wiring that actually applies the pin to Dio's
// HttpClient lives in ssl_pinning_io.dart (real) / ssl_pinning_stub.dart
// (web no-op), selected by a conditional import in api_client.dart.
//
// HOW TO GENERATE THE REAL PRODUCTION PIN (not yet done — see kProductionCertificateSha1Pins):
//   openssl s_client -connect <prod-host>:443 -servername <prod-host> < /dev/null 2>/dev/null \
//     | openssl x509 -fingerprint -sha1 -noout
// That prints `SHA1 Fingerprint=AA:BB:CC:...`. Strip the colons and
// lowercase the result, then add it as an entry below. Add the next
// certificate's fingerprint too before the current one expires or is
// rotated, so a renewal doesn't lock out every installed build at once.
//
// This pins the leaf certificate's SHA-1 fingerprint (via dart:io's
// X509Certificate.sha1), not the public key (SPKI). A cert renewal, even one
// that reuses the same key pair, changes this fingerprint and needs an app
// update with a new pin. Pinning the SPKI hash instead would survive
// renewal, but dart:io doesn't expose the raw SPKI without a DER/ASN.1
// parser, and that's more machinery than this app's threat model calls for
// right now.
const List<String> kProductionCertificateSha1Pins = [
  // TODO(7.16): add the real production certificate fingerprint here before
  // this is ever enforced. Empty on purpose — see isPinningEnforceable below.
];

/// True when the given environment should have its API connection pinned.
bool shouldPinCertificates(String environment) => environment == 'production';

/// True only when pinning should actually be turned on: the environment
/// calls for it AND at least one real pin is configured. Without the second
/// half of this check, shipping with an empty [kProductionCertificateSha1Pins]
/// would reject every connection in production — the "locks out all users
/// until an app update" failure mode this item's instructions warn about.
/// Until a real pin is added, this stays false and the app falls back to
/// normal system trust-store validation.
bool isPinningEnforceable(String environment) =>
    shouldPinCertificates(environment) && kProductionCertificateSha1Pins.isNotEmpty;

/// Hex-encodes a fingerprint the way `openssl x509 -fingerprint` prints it
/// (lowercase, no colons), so a certificate's byte fingerprint can be
/// compared against [kProductionCertificateSha1Pins].
String hexFingerprint(List<int> bytes) =>
    bytes.map((b) => b.toRadixString(16).padLeft(2, '0')).join();
