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

  // 37.14m. Times the API stores in UTC.
  group('AppUtils UTC handling', () {
    test('reads a time with no zone as UTC', () {
      final expected = DateTime.utc(2026, 10, 21, 8, 30);
      expect(AppUtils.parseUtc('2026-10-21T08:30:00'), expected.toLocal());
      expect(AppUtils.parseUtc('2026-10-21T08:30:00Z'), expected.toLocal());
      expect(AppUtils.parseUtc('2026-10-21T14:30:00+06:00'), expected.toLocal());
      expect(AppUtils.parseUtc('2026-10-21T08:30:00')!.isUtc, isFalse);
    });

    test('gives null for empty or bad values', () {
      expect(AppUtils.parseUtc(null), isNull);
      expect(AppUtils.parseUtc('  '), isNull);
      expect(AppUtils.parseUtc('not a date'), isNull);
    });

    test('round-trips a local time through the wire', () {
      final local = DateTime(2026, 10, 21, 9, 5);
      final sent = AppUtils.toWireUtc(local);
      expect(sent, endsWith('Z'));
      expect(AppUtils.parseUtc(sent), local);
    });
  });
}
