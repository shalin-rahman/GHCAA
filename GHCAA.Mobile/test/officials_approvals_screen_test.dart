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
import 'package:ghcaa_mobile/screens/officials/officials_approvals_screen.dart';
import 'package:ghcaa_mobile/screens/officials/officials_dashboard_screen.dart';

class _FakeElectionService extends ElectionService {
  _FakeElectionService() : super(Dio());

  final approved = <int>[];
  final rejected = <int>[];

  @override
  Future<List<ElectionApproval>> getApprovals(int electionId) async => [
        ElectionApproval(
          id: 30,
          electionId: electionId,
          action: 'Declare',
          requestedBy: 'Rina',
          requestedAt: DateTime.utc(2026, 10, 9),
          expiresAt: DateTime.utc(2026, 10, 11),
          requestedByMe: true,
        ),
        ElectionApproval(
          id: 31,
          electionId: electionId,
          action: 'Count',
          requestedBy: 'Tanvir',
          requestedAt: DateTime.utc(2026, 10, 9),
          expiresAt: DateTime.utc(2026, 10, 11),
        ),
      ];

  @override
  Future<void> approve(int approvalId) async => approved.add(approvalId);

  @override
  Future<void> reject(int approvalId, String? reason) async =>
      rejected.add(approvalId);
}

void main() {
  late _FakeElectionService service;

  setUpAll(() => dotenv.testLoad(fileInput: 'PORTAL_TITLE=GHCAA'));

  Future<void> pump(WidgetTester tester,
      {List<String> permissions = const ['Approve']}) async {
    tester.view.physicalSize = const Size(390, 1400);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.reset);
    service = _FakeElectionService();
    await tester.pumpWidget(ProviderScope(
      overrides: [
        electionServiceProvider.overrideWith((ref) => service),
        officialsElectionsProvider.overrideWith((ref) async => [
              AdminElection(
                id: 1,
                title: 'EC 2026',
                phase: 'Counting',
                isActive: true,
                eligibleVoterCount: 40,
                myPermissions: permissions,
              ),
            ]),
        roleProvider.overrideWith((ref) => Future.value('ElectionOfficial')),
        activeSpecialThemeProvider.overrideWith((ref) async => null),
      ],
      child: MaterialApp.router(
        theme: AppTheme.midnightTheme,
        routerConfig: GoRouter(routes: [
          GoRoute(
              path: '/',
              builder: (_, __) => const OfficialsApprovalsScreen(electionId: 1)),
        ]),
      ),
    ));
    for (var i = 0; i < 5; i++) {
      await tester.pump(const Duration(milliseconds: 100));
    }
  }

  testWidgets('hides approve and reject on the caller\'s own request',
      (tester) async {
    await pump(tester);

    expect(find.text('You asked for this, so a second official must decide.'),
        findsOneWidget);
    expect(find.text('Declare results'), findsOneWidget);
    expect(find.text('Count the ballots'), findsOneWidget);
    // One Approve button, for the other official's request.
    expect(find.widgetWithText(FilledButton, 'Approve'), findsOneWidget);
    expect(find.widgetWithText(OutlinedButton, 'Reject'), findsOneWidget);
  });

  testWidgets('hides approve and reject without the Approve permission',
      (tester) async {
    await pump(tester, permissions: ['ViewDashboard']);

    expect(find.widgetWithText(FilledButton, 'Approve'), findsNothing);
    expect(find.widgetWithText(OutlinedButton, 'Reject'), findsNothing);
  });

  testWidgets('approve sends the other official\'s request id', (tester) async {
    await pump(tester);

    await tester.tap(find.widgetWithText(FilledButton, 'Approve'));
    await tester.pump();

    expect(service.approved, [31]);
  });

  testWidgets('reject sends the other official\'s request id', (tester) async {
    await pump(tester);

    await tester.tap(find.widgetWithText(OutlinedButton, 'Reject'));
    await tester.pump();

    expect(service.rejected, [31]);
  });
}
