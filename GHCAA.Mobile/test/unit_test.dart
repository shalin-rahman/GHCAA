import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/utils/app_utils.dart';

void main() {
  group('AppUtils.formatDate', () {
    test('renders an ISO wire date as dd-MM-yyyy', () {
      expect(AppUtils.formatDate('2026-08-11'), '11-08-2026');
      expect(AppUtils.formatDate('2026-08-11T14:30:00Z'), '11-08-2026');
    });

    test('accepts a dd-MM-yyyy string that is already in display format', () {
      expect(AppUtils.formatDate('11-08-2026'), '11-08-2026');
    });

    test('accepts a DateTime directly', () {
      expect(AppUtils.formatDate(DateTime(2026, 8, 11)), '11-08-2026');
    });

    test('renders null as N/A', () {
      expect(AppUtils.formatDate(null), 'N/A');
    });

    test('falls back to the leading date part when nothing parses', () {
      expect(AppUtils.formatDate('not a date'), 'not a date');
    });

    // Pins current behaviour, not desired behaviour: the dd-MM-yyyy fallback
    // parse is lenient, so an out-of-range date rolls over into a plausible
    // wrong one instead of being rejected. Tracked as TODO 35.A9 — update this
    // expectation when that is fixed.
    test('silently rolls an out-of-range date over', () {
      expect(AppUtils.formatDate('2026-13-45T00:00:00'), '14-02-2027');
    });
  });

  group('AppUtils.parseDate', () {
    test('parses both the wire and the display format', () {
      expect(AppUtils.parseDate('2026-08-11'), DateTime(2026, 8, 11));
      expect(AppUtils.parseDate('11-08-2026'), DateTime(2026, 8, 11));
    });

    test('returns null for empty or unparseable input', () {
      expect(AppUtils.parseDate(null), isNull);
      expect(AppUtils.parseDate(''), isNull);
      expect(AppUtils.parseDate('sometime next week'), isNull);
    });
  });

  group('AppUtils.toWire', () {
    test('converts display and ISO input to yyyy-MM-dd', () {
      expect(AppUtils.toWire('11-08-2026'), '2026-08-11');
      expect(AppUtils.toWire('2026-08-11T14:30:00Z'), '2026-08-11');
      expect(AppUtils.toWire(DateTime(2026, 8, 11)), '2026-08-11');
    });

    test('returns null for null and empty input', () {
      expect(AppUtils.toWire(null), isNull);
      expect(AppUtils.toWire('   '), isNull);
    });

    test('passes unparseable input through untouched', () {
      expect(AppUtils.toWire('sometime next week'), 'sometime next week');
    });
  });

  group('AppUtils formatting helpers', () {
    test('formatCurrency prefixes the taka symbol and defaults to zero', () {
      expect(AppUtils.formatCurrency(null), '৳0.00');
      expect(AppUtils.formatCurrency('not a number'), '৳0.00');
      expect(AppUtils.formatCurrency(1500), startsWith('৳'));
      expect(AppUtils.formatCurrency(1500), endsWith('.00'));
    });

    test('getInitials takes the first and last name', () {
      expect(AppUtils.getInitials('Ashutosh Ganguly'), 'AG');
      expect(AppUtils.getInitials('Shalin Ahmed Chowdhury'), 'SC');
      expect(AppUtils.getInitials('Shalin'), 'S');
      expect(AppUtils.getInitials(null), '?');
      expect(AppUtils.getInitials('   '), '?');
    });
  });
}
