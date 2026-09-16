import 'dart:io';

import 'package:dio/dio.dart';
import 'package:dio/io.dart';

import 'ssl_pinning.dart';

/// Wires certificate pinning into [dio] for VM/native targets (Android, iOS,
/// desktop). No-op unless [isPinningEnforceable] is true for [environment].
///
/// Uses `SecurityContext(withTrustedRoots: false)` so the platform's normal
/// CA trust store is not consulted at all — every connection then goes
/// through [HttpClient.badCertificateCallback], where it's accepted only if
/// the presented certificate's SHA-1 fingerprint matches one of
/// [kProductionCertificateSha1Pins]. A plain `badCertificateCallback` without
/// this would only fire for certs already failing normal validation, so a
/// valid cert from a rogue-but-trusted CA would sail through unpinned.
void configureDioCertificatePinning(Dio dio, String environment) {
  if (!isPinningEnforceable(environment)) return;

  final adapter = IOHttpClientAdapter(
    createHttpClient: () {
      final client = HttpClient(context: SecurityContext(withTrustedRoots: false));
      client.badCertificateCallback = (X509Certificate cert, String host, int port) {
        return kProductionCertificateSha1Pins.contains(hexFingerprint(cert.sha1));
      };
      return client;
    },
  );
  dio.httpClientAdapter = adapter;
}
