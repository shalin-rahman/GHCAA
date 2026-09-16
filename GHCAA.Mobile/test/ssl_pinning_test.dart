import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/api/ssl_pinning.dart';

void main() {
  group('shouldPinCertificates', () {
    test('is false for development', () {
      expect(shouldPinCertificates('development'), isFalse);
    });

    test('is false for preprod', () {
      expect(shouldPinCertificates('preprod'), isFalse);
    });

    test('is true for production', () {
      expect(shouldPinCertificates('production'), isTrue);
    });
  });

  group('isPinningEnforceable', () {
    test('is false for development even if pins were configured', () {
      expect(isPinningEnforceable('development'), isFalse);
    });

    test('is false for preprod even if pins were configured', () {
      expect(isPinningEnforceable('preprod'), isFalse);
    });

    test(
      'is false for production while kProductionCertificateSha1Pins is still a placeholder',
      () {
        // 7.16: no real production certificate fingerprint has been
        // confirmed yet, so the pin list is intentionally empty. This test
        // documents that state and will start failing (correctly) the
        // moment a real pin is added — at which point it should be updated
        // to assert isPinningEnforceable('production') is true instead.
        expect(kProductionCertificateSha1Pins, isEmpty);
        expect(isPinningEnforceable('production'), isFalse);
      },
    );
  });

  group('hexFingerprint', () {
    test('lowercases and zero-pads each byte with no separators', () {
      expect(hexFingerprint([0xAB, 0x0F, 0x00, 0xFF]), 'ab0f00ff');
    });

    test('matches openssl-style output for a known byte sequence', () {
      expect(hexFingerprint([0x01, 0x02, 0x03]), '010203');
    });
  });
}
