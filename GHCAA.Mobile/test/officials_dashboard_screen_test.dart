import 'package:dio/dio.dart';
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

class _FakeElectionService extends ElectionService {
  _FakeElectionService() : super(Dio());

  final attested = <String>[];

  @override
  Future<void> attestReadiness(int id, String condition) async {
    attested.add('$id $condition');
  }
}

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
      {ElectionBoard? board,
      ElectionReadiness? readiness,
      ElectionService? service}) async {
    tester.view.physicalSize = const Size(390, 1400);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.reset);
    await tester.pumpWidget(ProviderScope(
      overrides: [
        officialsElectionsProvider.overrideWith((ref) async => [election]),
        officialsBoardProvider.overrideWith((ref, id) async => board),
        officialsReadinessProvider.overrideWith((ref, id) async => readiness),
        roleProvider.overrideWith((ref) => Future.value('ElectionOfficial')),
        activeSpecialThemeProvider.overrideWith((ref) async => null),
        if (service != null)
          electionServiceProvider.overrideWith((ref) => service),
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

  group('readiness (37.12w)', () {
    const readiness = ElectionReadiness(ready: false, items: [
      ElectionReadinessItem(key: 'ballot-key', met: true),
      ElectionReadinessItem(key: 'mock-election', met: false, attestation: true),
      ElectionReadinessItem(key: 'approvers', met: false, advisory: true),
    ]);

    testWidgets('lists each item before polling, folding in the approver warning',
        (tester) async {
      await pump(tester, _election(['ViewDashboard'], phase: 'Campaign'),
          readiness: readiness);

      expect(find.text('Polling cannot open until each item below is met.'),
          findsOneWidget);
      expect(find.text('Done: The ballot key is set'), findsOneWidget);
      expect(find.text('Not yet: A mock election was run'), findsOneWidget);
      expect(find.text('Warning: $electionTooFewApproversWarning'),
          findsOneWidget);
    });

    // 84.16. Attested items are recorded from the phone too.
    testWidgets('records an attested item for someone who can change phase',
        (tester) async {
      final service = _FakeElectionService();
      await pump(
          tester, _election(['ViewDashboard', 'ChangePhase'], phase: 'Campaign'),
          readiness: readiness, service: service);

      expect(find.byKey(const Key('attest-ballot-key')), findsNothing);
      expect(find.byKey(const Key('attest-approvers')), findsNothing);
      await tester.tap(find.byKey(const Key('attest-mock-election')));
      for (var i = 0; i < 5; i++) {
        await tester.pump(const Duration(milliseconds: 100));
      }

      expect(service.attested, ['1 mock-election']);
      expect(find.text('Recorded in the audit log.'), findsOneWidget);
    });

    testWidgets('offers no attest button without the phase permission',
        (tester) async {
      await pump(tester, _election(['ViewDashboard'], phase: 'Campaign'),
          readiness: readiness);

      expect(find.byKey(const Key('readiness-mock-election')), findsOneWidget);
      expect(find.text('Record as done'), findsNothing);
    });

    testWidgets('hides the list once polling is open', (tester) async {
      await pump(tester, _election(['ViewDashboard']), readiness: readiness);

      expect(find.byKey(const Key('card-readiness')), findsNothing);
    });

    testWidgets('hides the list without the dashboard permission',
        (tester) async {
      await pump(tester, _election(['ChangePhase'], phase: 'Campaign'),
          readiness: readiness);

      expect(find.byKey(const Key('card-readiness')), findsNothing);
    });
  });

  // FR-39 (37.13t). A met approvers item must not read as the warning next to Done.
  test('the approvers item reads as a warning only when it is not met', () {
    expect(electionReadinessLabel('approvers', met: true),
        'At least two officials can approve on this election');
    expect(electionReadinessLabel('approvers', met: false),
        electionTooFewApproversWarning);
  });

  test('an unknown readiness key shows as itself', () {
    expect(electionReadinessLabel('new-item'), 'new-item');
    expect(
        ElectionReadiness.fromJson({
          'ready': true,
          'items': [
            {'key': 'paper-stock', 'met': true, 'attestation': true}
          ]
        }).items.single.attestation,
        isTrue);
  });
}
