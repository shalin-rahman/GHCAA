import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:ghcaa_mobile/features/elections/election_service.dart';
import 'package:ghcaa_mobile/features/theme/dynamic_theme_service.dart';
import 'package:ghcaa_mobile/screens/officials/officials_dashboard_screen.dart';

AdminElection _election(List<String> permissions, {String phase = 'Polling'}) =>
    AdminElection(
      id: 1,
      title: 'EC 2026',
      phase: phase,
      isActive: true,
      eligibleVoterCount: 40,
      ballotKeyFingerprint: 'ab12',
      myPermissions: permissions,
    );

void main() {
  setUpAll(() => dotenv.testLoad(fileInput: 'PORTAL_TITLE=GHCAA'));

  Future<void> pump(WidgetTester tester, AdminElection election,
      {ElectionBoard? board}) async {
    tester.view.physicalSize = const Size(390, 1400);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.reset);
    await tester.pumpWidget(ProviderScope(
      overrides: [
        officialsElectionsProvider.overrideWith((ref) async => [election]),
        officialsBoardProvider.overrideWith((ref, id) async => board),
        roleProvider.overrideWith((ref) => Future.value('ElectionOfficial')),
        activeSpecialThemeProvider.overrideWith((ref) async => null),
      ],
      child: MaterialApp.router(
        theme: AppTheme.midnightTheme,
        routerConfig: GoRouter(routes: [
          GoRoute(
              path: '/', builder: (_, __) => const OfficialsDashboardScreen()),
        ]),
      ),
    ));
    for (var i = 0; i < 5; i++) {
      await tester.pump(const Duration(milliseconds: 100));
    }
  }

  testWidgets('shows only the phase to someone with no view permission',
      (tester) async {
    await pump(tester, _election(['ChangePhase']));

    expect(find.byKey(const Key('card-phase')), findsOneWidget);
    expect(find.byKey(const Key('card-roll')), findsNothing);
    expect(find.byKey(const Key('card-key')), findsNothing);
    expect(find.byKey(const Key('card-approvals')), findsNothing);
  });

  testWidgets('shows the roll, key and approvals link to a dashboard viewer',
      (tester) async {
    await pump(tester, _election(['ViewDashboard']));

    expect(find.byKey(const Key('card-roll')), findsOneWidget);
    expect(find.byKey(const Key('card-key')), findsOneWidget);
    expect(find.byKey(const Key('card-approvals')), findsOneWidget);
  });

  testWidgets('hides turnout while polling because the board sends none',
      (tester) async {
    await pump(tester, _election(['ViewDashboard']),
        board: const ElectionBoard(isTest: false, groups: []));

    expect(find.byKey(const Key('card-turnout')), findsNothing);
  });

  testWidgets('shows turnout once the board sends it', (tester) async {
    await pump(tester, _election(['ViewDashboard'], phase: 'Closed'),
        board: const ElectionBoard(
            isTest: false, groups: [], ballotsCast: 12, rollSize: 40));

    expect(find.text('Ballots cast: 12 of 40'), findsOneWidget);
  });
}
