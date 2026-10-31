import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/screens/member/profile_edit_screen.dart';

void main() {
  group('NID validation', () {
    test('accepts 10, 13 and 17 digits', () {
      expect(isValidNid('1234567890'), isTrue);
      expect(isValidNid('1234567890123'), isTrue);
      expect(isValidNid('12345678901234567'), isTrue);
    });

    test('rejects other values', () {
      expect(isValidNid('123456789'), isFalse);
      expect(isValidNid('12345678901'), isFalse);
      expect(isValidNid('1234567890123456'), isFalse);
      expect(isValidNid('IMPORT-1-12345'), isFalse);
      expect(isValidNid(null), isFalse);
    });
  });
}
