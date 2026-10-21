import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:ghcaa_mobile/core/api/governance_api.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/features/admin/admin_service.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:ghcaa_mobile/features/theme/dynamic_theme_service.dart';
import 'package:ghcaa_mobile/screens/admin/governance_registry_screen.dart';

class _FakeAdminService extends AdminService {
  _FakeAdminService() : super(Dio());

  @override
  Future<List<dynamic>> getCommitteeMembers(int periodId) async => [];
}

class _FakeGovernanceApi extends GovernanceApi {
  _FakeGovernanceApi(this.positions) : super(Dio());

  final List<dynamic>? positions;

  @override
  Future<List<dynamic>> getPositions() async {
    if (positions == null) throw Exception('down');
    return positions!;
  }
}

void main() {
  setUpAll(() => dotenv.testLoad(fileInput: 'PORTAL_TITLE=GHCAA'));

  Future<void> openPanel(WidgetTester tester, List<dynamic>? positions) async {
    tester.view.physicalSize = const Size(390, 1400);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.reset);
    await tester.pumpWidget(ProviderScope(
      overrides: [
        adminServiceProvider.overrideWith((ref) => _FakeAdminService()),
        governanceApiProvider.overrideWith((ref) => _FakeGovernanceApi(positions)),
        ecPeriodsAdminProvider.overrideWith((ref) async => [
              {'id': 1, 'title': 'EC 2026', 'startDate': '2026-01-01T00:00:00', 'endDate': null, 'isActive': true},
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
    for (var i = 0; i < 5; i++) {
      await tester.pump(const Duration(milliseconds: 100));
    }
    await tester.tap(find.text('GOVERNANCE NODES'));
    for (var i = 0; i < 5; i++) {
      await tester.pump(const Duration(milliseconds: 100));
    }
  }

  IconButton assignButton(WidgetTester tester) =>
      tester.widget<IconButton>(find.byKey(const Key('assignButton')));

  testWidgets('FR-34: a failed positions load is reported and Assign stays off', (tester) async {
    await openPanel(tester, null);

    expect(find.text('Could not load committee positions.'), findsOneWidget);
    expect(assignButton(tester).onPressed, isNull);
  });

  testWidgets('FR-34: Assign is on once positions load', (tester) async {
    await openPanel(tester, [
      {'id': 1, 'key': 'President', 'name': 'President', 'seatCount': 1},
    ]);

    expect(assignButton(tester).onPressed, isNotNull);
  });
}
