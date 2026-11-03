import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/utils/app_utils.dart';

void main() {
  group('FR-05: AppUtils.batchText', () {
    test('shows the server label with every college record', () {
      expect(
          AppUtils.batchText({
            'batchLabel': 'HSC (Science) 2001 / BSc (Physics) 2004',
            'passingYear': 2001,
          }),
          'HSC (Science) 2001 / BSc (Physics) 2004');
    });

    test('trims the label', () {
      expect(AppUtils.batchText({'batchLabel': '  HSC 2001 '}), 'HSC 2001');
    });

    test('falls back to the year when the label is null, empty or blank', () {
      expect(AppUtils.batchText({'batchLabel': null, 'passingYear': 1999}), '1999');
      expect(AppUtils.batchText({'batchLabel': '', 'passingYear': 1999}), '1999');
      expect(AppUtils.batchText({'batchLabel': '  ', 'passingYear': 1999}), '1999');
      expect(AppUtils.batchText({'ghcLastCertificatePassingYear': 1985}), '1985');
    });

    test('treats year 0 and missing data as no batch', () {
      expect(AppUtils.batchText({'passingYear': 0}), 'N/A');
      expect(AppUtils.batchText({}), 'N/A');
      expect(AppUtils.batchText(null, fallback: '-'), '-');
    });
  });
}
