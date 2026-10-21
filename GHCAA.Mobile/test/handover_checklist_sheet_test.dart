import 'dart:async';

import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:ghcaa_mobile/core/constants/app_constants.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/features/admin/admin_service.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:ghcaa_mobile/features/theme/dynamic_theme_service.dart';
import 'package:ghcaa_mobile/screens/admin/governance_registry_screen.dart';

class _FakeAdminService extends AdminService {
  _FakeAdminService() : super(Dio());

  List<dynamic> checks = [
    {'item': 'Constitution', 'isChecked': true, 'setByUserId': 1, 'setAt': '2026-10-21T08:00:00Z'},
  ];
  final calls = <String>[];
  Completer<List<dynamic>>? pendingSave;
  bool failSave = false;

  @override
  Future<List<dynamic>> getHandoverChecks(int periodId) async {
    calls.add('get $periodId');
    return checks;
  }

  @override
  Future<List<dynamic>> setHandoverCheck(int periodId, String item, bool isChecked) async {
    calls.add('set $periodId $item $isChecked');
    if (pendingSave != null) return pendingSave!.future;
    if (failSave) throw Exception('403');
    checks = [
      ...checks.where((c) => c['item'] != item),
      {'item': item, 'isChecked': isChecked, 'setByUserId': 1, 'setAt': '2026-10-21T09:00:00Z'},
    ];
    return checks;
  }
}

// TODO 37.14m. ER-34 handover checklist on the committee term.
void main() {
  setUpAll(() => dotenv.testLoad(fileInput: 'PORTAL_TITLE=GHCAA'));

  Future<void> settle(WidgetTester tester) async {
    for (var i = 0; i < 5; i++) {
      await tester.pump(const Duration(milliseconds: 100));
    }
  }

  Future<_FakeAdminService> openSheet(WidgetTester tester) async {
    tester.view.physicalSize = const Size(390, 1400);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.reset);
    final admin = _FakeAdminService();
    await tester.pumpWidget(ProviderScope(
      overrides: [
        adminServiceProvider.overrideWith((ref) => admin),
        ecPeriodsAdminProvider.overrideWith((ref) async => [
              {'id': 4, 'title': 'EC 2026', 'startDate': '2026-01-01T00:00:00', 'endDate': null, 'isActive': true},
            ]),
        roleProvider.overrideWith((ref) => Future.value('Admin')),
        activeSpecialThemeProvider.overrideWith((ref) async => null),
      ],
      child: MaterialApp.router(
        theme: AppTheme.midnightTheme,
        routerConfig: GoRouter(routes: [
          GoRoute(path: '/', builder: (_, __) => const AdminGovernanceScreen()),
        ]),
      ),
    ));
    await settle(tester);
    await tester.tap(find.byKey(const Key('handover-4')));
    await settle(tester);
    return admin;
  }

  CheckboxListTile tile(WidgetTester tester, String item) =>
      tester.widget<CheckboxListTile>(find.byKey(Key('handover-$item')));

  testWidgets('ER-34: loads the ticks for the term and lists every item', (tester) async {
    final admin = await openSheet(tester);

    expect(admin.calls, ['get 4']);
    expect(find.byType(CheckboxListTile), findsNWidgets(GovernanceConstants.handoverCheckItems.length));
    expect(tile(tester, 'Constitution').value, isTrue);
    expect(tile(tester, 'MinuteBooks').value, isFalse);
  });

  testWidgets('ER-34: a tick saves and shows the list the server returns', (tester) async {
    final admin = await openSheet(tester);

    await tester.tap(find.byKey(const Key('handover-MinuteBooks')));
    await settle(tester);

    expect(admin.calls.last, 'set 4 MinuteBooks true');
    expect(tile(tester, 'MinuteBooks').value, isTrue);
  });

  testWidgets('ER-34: boxes are off while a tick saves', (tester) async {
    final admin = await openSheet(tester);
    admin.pendingSave = Completer<List<dynamic>>();

    await tester.tap(find.byKey(const Key('handover-MinuteBooks')));
    await tester.pump();
    expect(tile(tester, 'Constitution').onChanged, isNull);

    admin.pendingSave!.complete(admin.checks);
    await settle(tester);
    expect(tile(tester, 'Constitution').onChanged, isNotNull);
  });

  testWidgets('ER-34: a failed save is reported and the boxes come back on', (tester) async {
    final admin = await openSheet(tester);
    admin.failSave = true;

    await tester.tap(find.byKey(const Key('handover-MinuteBooks')));
    await settle(tester);

    expect(find.text('Could not save the tick. Please try again.'), findsOneWidget);
    expect(tile(tester, 'MinuteBooks').value, isFalse);
    expect(tile(tester, 'MinuteBooks').onChanged, isNotNull);
  });
}
