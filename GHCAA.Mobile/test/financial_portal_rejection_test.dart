import 'dart:convert';
import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:ghcaa_mobile/core/models/financial_models.dart';
import 'package:ghcaa_mobile/core/storage/storage_service.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:ghcaa_mobile/features/financials/financial_service.dart';
import 'package:ghcaa_mobile/screens/member/financial_portal_screen.dart';

import 'helpers/fake_services.dart';

// Answers every request with 200 and one JSON body.
class _FixedAdapter implements HttpClientAdapter {
  _FixedAdapter(this.body);

  final Object body;

  @override
  void close({bool force = false}) {}

  @override
  Future<ResponseBody> fetch(RequestOptions options, Stream<Uint8List>? requestStream, Future<void>? cancelFuture) async =>
      ResponseBody.fromString(jsonEncode(body), 200, headers: {
        Headers.contentTypeHeader: [Headers.jsonContentType],
      });
}

Map<String, dynamic> _payment(String status, String? notes, String? reason, {int id = 1, int amount = 500}) {
  final entry = LedgerEntry.fromJson({
    'id': id,
    'amount': amount,
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
    'hasReceipt': entry.hasReceipt,
    'rejectionReason': entry.rejectionReason,
  };
}

Future<void> _pumpPortal(WidgetTester tester, List<Map<String, dynamic>> ledger) async {
  tester.view.physicalSize = const Size(520, 900);
  tester.view.devicePixelRatio = 1;
  addTearDown(tester.view.reset);

  await tester.pumpWidget(ProviderScope(
    overrides: [
      ledgerProvider.overrideWith((ref) async => ledger),
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
      await _pumpPortal(tester, [_payment('Failed', 'Annual dues', 'Reference not found')]);

      await tester.scrollUntilVisible(find.text('Reason: Reference not found'), 200,
          scrollable: find.byType(Scrollable).first);
      expect(find.text('FAILED'), findsOneWidget);
      expect(find.text('Reason: Reference not found'), findsOneWidget);
    });
  });

  group('84.106: receipt link', () {
    Map<String, dynamic> json(Object status) =>
        {'amount': 500, 'paidAt': '2026-11-20T10:00:00Z', 'financialCategory': 'MembershipFee', 'status': status};

    test('Completed and Refunded have a receipt, by name, number or any case', () {
      for (final s in <Object>['Completed', 'completed', 1, 'Refunded', 'REFUNDED', 3]) {
        expect(LedgerEntry.fromJson(json(s)).hasReceipt, isTrue, reason: '$s');
      }
      for (final s in <Object>['Pending', 0, 'Failed', 'failed', 2]) {
        expect(LedgerEntry.fromJson(json(s)).hasReceipt, isFalse, reason: '$s');
      }
    });

    test('isFailed ignores case', () {
      expect(LedgerEntry.fromJson(json('failed')).isFailed, isTrue);
      expect(LedgerEntry.fromJson(json(2)).isFailed, isTrue);
      expect(LedgerEntry.fromJson(json('Completed')).isFailed, isFalse);
    });

    test('getLedger passes hasReceipt to each row', () async {
      final dio = Dio()
        ..httpClientAdapter = _FixedAdapter([
          {...json('Completed'), 'id': 1},
          {...json('Pending'), 'id': 2},
          {...json(3), 'id': 3},
        ]);
      final rows = await FinancialService(dio).getLedger();
      expect(rows.map((r) => r['hasReceipt']).toList(), [true, false, true]);
    });

    testWidgets('receipt link only on paid rows', (tester) async {
      await _pumpPortal(tester, [
        _payment('Completed', null, null, id: 1, amount: 111),
        _payment('Pending', null, null, id: 2, amount: 222),
        _payment('Refunded', null, null, id: 4, amount: 444),
        _payment('Failed', null, 'Reference not found', id: 3, amount: 333),
      ]);

      await tester.scrollUntilVisible(find.text('Reason: Reference not found'), 200,
          scrollable: find.byType(Scrollable).first);
      expect(find.text('GET RECEIPT', skipOffstage: false), findsNWidgets(2));
    });
  });
}
