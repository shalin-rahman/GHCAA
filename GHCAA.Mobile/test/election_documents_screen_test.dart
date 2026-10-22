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
import 'package:ghcaa_mobile/screens/member/election_documents_screen.dart';

class _FakeElectionService extends ElectionService {
  _FakeElectionService() : super(Dio());

  List<ElectionContent> pages = const [
    ElectionContent(
        key: 'regulations',
        kind: 'Page',
        group: 'Rules',
        title: 'Election Regulations',
        summary: 'The rules',
        displayOrder: 1),
    ElectionContent(
        key: 'conduct',
        kind: 'Page',
        group: 'Rules',
        title: 'Code of Conduct',
        displayOrder: 2),
  ];

  @override
  Future<List<ElectionContent>> getContent(String kind) async =>
      kind == ElectionContentKinds.page
          ? pages
          : const [
              ElectionContent(
                  key: 'ER-01',
                  kind: 'Form',
                  title: 'Notice of Election',
                  displayOrder: 1),
            ];

  @override
  Future<ElectionContent> getContentEntry(String kind, String key) async =>
      const ElectionContent(
          key: 'regulations',
          kind: 'Page',
          title: 'Election Regulations',
          body: '# Regulations\n\n- Every member may vote once.',
          displayOrder: 1);
}

// FR-25. 37.14s
void main() {
  late _FakeElectionService service;

  setUpAll(() => dotenv.testLoad(fileInput: 'PORTAL_TITLE=GHCAA'));

  Future<void> pump(WidgetTester tester, {bool admin = false}) async {
    tester.view.physicalSize = const Size(390, 1400);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.reset);
    await tester.pumpWidget(ProviderScope(
      overrides: [
        electionServiceProvider.overrideWith((ref) => service),
        roleProvider.overrideWith((ref) => Future.value(admin ? 'Admin' : 'Member')),
        isAdminProvider.overrideWith((ref) async => admin),
        isElectionStaffProvider.overrideWith((ref) async => admin),
        activeSpecialThemeProvider.overrideWith((ref) async => null),
      ],
      child: MaterialApp.router(
        theme: AppTheme.midnightTheme,
        routerConfig: GoRouter(routes: [
          GoRoute(path: '/', builder: (_, __) => const ElectionDocumentsScreen()),
          GoRoute(
              path: '/election/documents/:key',
              builder: (_, state) =>
                  ElectionPageScreen(pageKey: state.pathParameters['key']!)),
        ]),
      ),
    ));
    for (var i = 0; i < 5; i++) {
      await tester.pump(const Duration(milliseconds: 100));
    }
  }

  setUp(() => service = _FakeElectionService());

  testWidgets('lists the pages under their group at phone width',
      (tester) async {
    await pump(tester);

    expect(find.text('Rules and guides'), findsOneWidget);
    expect(find.text('Rules'), findsOneWidget);
    expect(find.text('Election Regulations'), findsOneWidget);
    expect(find.text('The rules'), findsOneWidget);
    expect(find.text('Code of Conduct'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });

  Future<void> openForms(WidgetTester tester) async {
    await tester.tap(find.text('Forms'));
    for (var i = 0; i < 5; i++) {
      await tester.pump(const Duration(milliseconds: 100));
    }
  }

  testWidgets('the forms tab lists every form with its code', (tester) async {
    await pump(tester, admin: true);
    await openForms(tester);

    expect(find.text('Notice of Election'), findsOneWidget);
    expect(find.text('ER-01'), findsOneWidget);
    expect(find.byIcon(Icons.print_outlined), findsOneWidget);
  });

  testWidgets('a member sees the forms but cannot print them', (tester) async {
    await pump(tester);
    await openForms(tester);

    expect(find.text('ER-01'), findsOneWidget);
    expect(find.byIcon(Icons.print_outlined), findsNothing);
  });

  testWidgets('opening a page shows its text', (tester) async {
    await pump(tester);

    await tester.tap(find.text('Election Regulations'));
    for (var i = 0; i < 5; i++) {
      await tester.pump(const Duration(milliseconds: 100));
    }

    expect(find.text('Regulations'), findsOneWidget);
    expect(find.text('•  Every member may vote once.'), findsOneWidget);
  });

  testWidgets('no pages shows the empty message', (tester) async {
    service.pages = const [];
    await pump(tester);

    expect(find.text('No election pages are published yet.'), findsOneWidget);
  });
}
