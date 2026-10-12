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
import 'package:ghcaa_mobile/screens/member/election_screen.dart';

class _FakeElectionService extends ElectionService {
  _FakeElectionService() : super(Dio());

  Map<int, List<int>>? cast;

  @override
  Future<Ballot> getBallot(int id) async => const Ballot(
        electionId: 12,
        title: 'EC 2026',
        phase: 'Polling',
        hasVoted: false,
        seats: [
          BallotSeat(seatId: 3, positionTitle: 'Committee', places: 2, candidates: [
            BallotCandidate(nominationId: 10, name: 'Rina'),
            BallotCandidate(nominationId: 11, name: 'Tanvir'),
            BallotCandidate(nominationId: 12, name: 'Maya'),
          ]),
        ],
      );

  @override
  Future<ElectionBoard?> getBoard(int id) async => const ElectionBoard(
        isTest: false,
        groups: [
          BoardGroup(groupName: 'Commission', members: [
            BoardMember(
                personaName: 'Chief Election Commissioner',
                displayName: 'Rahim Uddin'),
          ]),
        ],
      );

  @override
  Future<String> castBallot(int id, Map<int, List<int>> choices) async {
    cast = choices;
    return 'ABCD-EFGH';
  }
}

void main() {
  late _FakeElectionService service;

  setUpAll(() => dotenv.testLoad(fileInput: 'PORTAL_TITLE=GHCAA'));

  Future<void> pump(WidgetTester tester) async {
    tester.view.physicalSize = const Size(390, 1400);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.reset);
    service = _FakeElectionService();
    await tester.pumpWidget(ProviderScope(
      overrides: [
        electionServiceProvider.overrideWith((ref) => service),
        currentElectionProvider.overrideWith((ref) async => const ElectionSummary(
              id: 12,
              title: 'EC 2026',
              phase: 'Polling',
              ecPeriodId: 2,
              voterCount: 0,
              eligibleVoterCount: 10,
            )),
        roleProvider.overrideWith((ref) => Future.value('Member')),
        activeSpecialThemeProvider.overrideWith((ref) async => null),
      ],
      child: MaterialApp.router(
        theme: AppTheme.midnightTheme,
        routerConfig: GoRouter(routes: [
          GoRoute(path: '/', builder: (_, __) => const ElectionScreen()),
        ]),
      ),
    ));
    for (var i = 0; i < 5; i++) {
      await tester.pump(const Duration(milliseconds: 100));
    }
  }

  testWidgets('a third pick on a two-place seat is blocked', (tester) async {
    await pump(tester);

    expect(find.textContaining('Pick up to 2'), findsOneWidget);
    await tester.tap(find.text('Rina'));
    await tester.pump();
    await tester.tap(find.text('Tanvir'));
    await tester.pump();
    await tester.tap(find.text('Maya'));
    await tester.pump();

    expect(find.byIcon(Icons.check_box), findsNWidgets(2));
    expect(find.textContaining('2 picked'), findsOneWidget);
  });

  // FR-33
  testWidgets('the officials list shows names and roles at phone width',
      (tester) async {
    await pump(tester);

    expect(find.text('Who runs this election'), findsOneWidget);
    expect(find.text('Commission'), findsOneWidget);
    expect(
        find.text('Rahim Uddin, Chief Election Commissioner'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });

  testWidgets('abstain clears the picks on the seat', (tester) async {
    await pump(tester);

    await tester.tap(find.text('Rina'));
    await tester.pump();
    await tester.tap(find.text('Abstain on this seat'));
    await tester.pump();

    expect(find.byIcon(Icons.check_box), findsNothing);
    expect(find.textContaining('0 picked'), findsOneWidget);
  });
}
