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
import 'package:ghcaa_mobile/screens/member/election_results_screen.dart';

class _FakeElectionService extends ElectionService {
  _FakeElectionService({this.trackingCodes}) : super(Dio());

  int? resultsFor;
  int? trackingCodesFor;
  final List<String>? trackingCodes;

  @override
  Future<ElectionResults?> getResults(int id) async {
    resultsFor = id;
    return null;
  }

  @override
  Future<List<String>?> getTrackingCodes(int id) async {
    trackingCodesFor = id;
    return trackingCodes;
  }
}

void main() {
  setUpAll(() => dotenv.testLoad(fileInput: 'PORTAL_TITLE=GHCAA'));

  Future<_FakeElectionService> pump(
    WidgetTester tester,
    ElectionSummary? latest, {
    List<String>? trackingCodes,
  }) async {
    tester.view.physicalSize = const Size(390, 1400);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.reset);
    final service = _FakeElectionService(trackingCodes: trackingCodes);
    await tester.pumpWidget(ProviderScope(
      overrides: [
        electionServiceProvider.overrideWith((ref) => service),
        latestDeclaredElectionProvider.overrideWith((ref) async => latest),
        roleProvider.overrideWith((ref) => Future.value('Member')),
        activeSpecialThemeProvider.overrideWith((ref) async => null),
      ],
      child: MaterialApp.router(
        theme: AppTheme.midnightTheme,
        routerConfig: GoRouter(routes: [
          GoRoute(path: '/', builder: (_, __) => const ElectionResultsScreen()),
        ]),
      ),
    ));
    for (var i = 0; i < 5; i++) {
      await tester.pump(const Duration(milliseconds: 100));
    }
    return service;
  }

  testWidgets('says nothing is declared when there is no declared election',
      (tester) async {
    final service = await pump(tester, null);

    expect(find.text(resultsNotDeclaredText), findsOneWidget);
    expect(service.resultsFor, isNull);
  });

  testWidgets('loads the results of the latest declared election',
      (tester) async {
    final service = await pump(
        tester,
        const ElectionSummary(
          id: 21,
          title: 'EC 2026',
          phase: 'Declared',
          ecPeriodId: 2,
          voterCount: 7,
          eligibleVoterCount: 10,
        ));

    expect(service.resultsFor, 21);
  });

  testWidgets('FR-016: lists every published tracking code', (tester) async {
    final service = await pump(
      tester,
      const ElectionSummary(
        id: 21,
        title: 'EC 2026',
        phase: 'Declared',
        ecPeriodId: 2,
        voterCount: 3,
        eligibleVoterCount: 10,
      ),
      trackingCodes: const ['CODE-001', 'CODE-002', 'CODE-003'],
    );

    expect(service.trackingCodesFor, 21);
    expect(find.text('3 of 3 codes'), findsOneWidget);
    expect(find.text('CODE-001'), findsOneWidget);
    expect(find.text('CODE-002'), findsOneWidget);
    expect(find.text('CODE-003'), findsOneWidget);
  });
}
