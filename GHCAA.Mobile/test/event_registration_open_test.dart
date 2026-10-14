import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/utils/app_utils.dart';

void main() {
  group('FR-002 (spec 014): AppUtils.isEventRegistrationOpen', () {
    final now = DateTime(2026, 10, 14, 12);

    test('open when no dates are set', () {
      expect(AppUtils.isEventRegistrationOpen({'isActive': true}, now: now),
          isTrue);
    });

    test('closed when the event is inactive', () {
      expect(AppUtils.isEventRegistrationOpen({'isActive': false}, now: now),
          isFalse);
    });

    test('closed before the registration window opens', () {
      final ev = {'registrationStartDate': '2026-10-15T00:00:00'};
      expect(AppUtils.isEventRegistrationOpen(ev, now: now), isFalse);
    });

    test('closed after the registration end date', () {
      final ev = {'registrationEndDate': '2026-10-13T00:00:00'};
      expect(AppUtils.isEventRegistrationOpen(ev, now: now), isFalse);
    });

    test('closed after the event ends when no registration end date is set',
        () {
      final ev = {
        'endDate': '2026-10-14T11:00:00',
        'registrationEndDate': null
      };
      expect(AppUtils.isEventRegistrationOpen(ev, now: now), isFalse);
    });

    test('open while the event is still running', () {
      final ev = {'endDate': '2026-10-14T18:00:00'};
      expect(AppUtils.isEventRegistrationOpen(ev, now: now), isTrue);
    });
  });
}
