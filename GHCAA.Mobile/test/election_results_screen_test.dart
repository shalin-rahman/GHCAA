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
  _FakeElectionService() : super(Dio());

  int? resultsFor;

  @override
  Future<ElectionResults?> getResults(int id) async {
    resultsFor = id;
    return null;
  }

  @override
  Future<List<String>?> getTrackingCodes(int id) async => null;
}

void main() {
  setUpAll(() => dotenv.testLoad(fileInput: 'PORTAL_TITLE=GHCAA'));

  Future<_FakeElectionService> pump(
      WidgetTester tester, ElectionSummary? latest) async {
    tester.view.physicalSize = const Size(390, 1400);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.reset);
    final service = _FakeElectionService();
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
}
