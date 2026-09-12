import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/utils/app_utils.dart';

void main() {
  group('AppUtils date handling', () {
    final value = DateTime(2026, 8, 22, 14, 30);

    test('formats date-only values using each supported format', () {
      expect(
        AppUtils.formatDate(value, format: 'dd-MM-yyyy'),
        '22-08-2026',
      );
      expect(
        AppUtils.formatDate(value, format: 'MM/dd/yyyy'),
        '08/22/2026',
      );
    });

    test('formats time values through the shared helper', () {
      expect(AppUtils.formatTime(value), '14:30');
    });

    test('formats date-time values without losing the time', () {
      expect(
        AppUtils.formatDate(
          value,
          format: 'MM/dd/yyyy',
          includeTime: true,
        ),
        '08/22/2026 14:30',
      );
    });

    test('parses both configured formats and ISO values', () {
      expect(AppUtils.parseDate('22-08-2026'), DateTime(2026, 8, 22));
      expect(AppUtils.parseDate('08/22/2026'), DateTime(2026, 8, 22));
      expect(
        AppUtils.parseDate('2026-08-22T14:30:00'),
        DateTime(2026, 8, 22, 14, 30),
      );
    });

    test('writes date-only and date-time ISO values separately', () {
      expect(AppUtils.toWire(value), '2026-08-22');
      expect(
        AppUtils.toWire(value, includeTime: true),
        '2026-08-22T14:30:00.000',
      );
    });
  });
}
