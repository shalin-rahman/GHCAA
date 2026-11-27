import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:ghcaa_mobile/core/models/financial_models.dart';
import 'package:ghcaa_mobile/core/storage/storage_service.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:ghcaa_mobile/screens/member/financial_portal_screen.dart';

import 'helpers/fake_services.dart';

Map<String, dynamic> _payment(String status, String? notes, String? reason) {
  final entry = LedgerEntry.fromJson({
    'id': 1,
    'amount': 500,
    'paidAt': '2026-11-20T10:00:00Z',
    'financialCategory': 'MembershipFee',
    'status': status,
    'notes': notes,
    'rejectionReason': reason,
  });
  return {
    ...entry.raw,
    'date': entry.raw['paidAt'],
    'description': 'MembershipFee',
    'isFailed': entry.isFailed,
    'rejectionReason': entry.rejectionReason,
  };
}

void main() {
  group('84.92: rejection reason', () {
    test('reads the reason from its own field, not the note', () {
      final e = LedgerEntry.fromJson({
        'amount': 500,
        'paidAt': '2026-11-20T10:00:00Z',
        'financialCategory': 'MembershipFee',
        'status': 'Failed',
        'notes': 'Annual dues',
        'rejectionReason': 'Reference not found | bank checked',
      });
      expect(e.rejectionReason, 'Reference not found | bank checked');
      expect(e.notes, 'Annual dues');
    });

    test('a gateway failure with a piped note has no reason', () {
      final e = LedgerEntry.fromJson({
        'amount': 500,
        'paidAt': '2026-11-20T10:00:00Z',
        'financialCategory': 'MembershipFee',
        'status': 'Failed',
        'notes': 'Annual dues | gateway timeout',
        'rejectionReason': null,
      });
      expect(e.rejectionReason, isNull);
      expect(e.isFailed, isTrue);
    });

    test('a blank reason counts as none', () {
      final e = LedgerEntry.fromJson({
        'amount': 500,
        'paidAt': '2026-11-20T10:00:00Z',
        'financialCategory': 'MembershipFee',
        'status': 'Failed',
        'rejectionReason': '  ',
      });
      expect(e.rejectionReason, isNull);
    });

    testWidgets('shows the rejection reason on a failed payment', (tester) async {
      tester.view.physicalSize = const Size(520, 900);
      tester.view.devicePixelRatio = 1;
      addTearDown(tester.view.reset);

      await tester.pumpWidget(ProviderScope(
        overrides: [
          ledgerProvider.overrideWith((ref) async => [
                _payment('Failed', 'Annual dues', 'Reference not found'),
              ]),
          duesProvider.overrideWith((ref) async => 0.0),
          savedMethodsProvider.overrideWith((ref) async => []),
          storageServiceProvider.overrideWith((ref) => FakeStorageService()),
          authServiceProvider.overrideWith((ref) => FakeAuthService()),
          roleProvider.overrideWith((ref) => Future.value('Member')),
        ],
        child: MaterialApp.router(
          theme: AppTheme.midnightTheme,
          routerConfig: GoRouter(routes: [
            GoRoute(path: '/', builder: (context, state) => const FinancialPortalScreen()),
          ]),
        ),
      ));
      await tester.pump();
      await tester.pump();

      await tester.scrollUntilVisible(find.text('Reason: Reference not found'), 200,
          scrollable: find.byType(Scrollable).first);
      expect(find.text('FAILED'), findsOneWidget);
      expect(find.text('Reason: Reference not found'), findsOneWidget);
    });
  });
}
