import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:ghcaa_mobile/core/storage/storage_service.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:ghcaa_mobile/features/elections/election_service.dart';
import 'package:ghcaa_mobile/features/theme/dynamic_theme_service.dart';
import 'package:ghcaa_mobile/screens/officials/my_appointments_screen.dart';

class _FakeElectionService extends ElectionService {
  _FakeElectionService() : super(Dio());

  final accepted = <int>[];
  final declined = <int, String?>{};

  @override
  Future<void> acceptAppointment(int id) async => accepted.add(id);

  @override
  Future<void> declineAppointment(int id, String? reason) async =>
      declined[id] = reason;
}

class _FakeAuthService extends AuthService {
  _FakeAuthService(Ref ref) : super(Dio(), StorageService(), ref);

  // Static, because a failed accept never builds the provider and so no instance exists.
  static int reloads = 0;

  @override
  Future<bool> reloadSession() async {
    reloads++;
    return true;
  }
}

class _FailingElectionService extends _FakeElectionService {
  @override
  Future<void> acceptAppointment(int id) async =>
      throw DioException(requestOptions: RequestOptions(path: '/x'));
}

void main() {
  late _FakeElectionService service;

  setUpAll(() => dotenv.testLoad(fileInput: 'PORTAL_TITLE=GHCAA'));

  Future<void> pump(WidgetTester tester, {bool acceptFails = false}) async {
    tester.view.physicalSize = const Size(390, 1400);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.reset);
    _FakeAuthService.reloads = 0;
    service = acceptFails ? _FailingElectionService() : _FakeElectionService();
    await tester.pumpWidget(ProviderScope(
      overrides: [
        electionServiceProvider.overrideWith((ref) => service),
        authServiceProvider.overrideWith((ref) => _FakeAuthService(ref)),
        myAppointmentsProvider.overrideWith((ref) async => [
              ElectionAppointment(
                id: 7,
                electionTitle: 'EC 2026',
                personaName: 'Polling Officer',
                declarationText: 'I will act fairly.',
              ),
            ]),
        roleProvider.overrideWith((ref) => Future.value('Member')),
        activeSpecialThemeProvider.overrideWith((ref) async => null),
      ],
      child: MaterialApp.router(
        theme: AppTheme.midnightTheme,
        routerConfig: GoRouter(routes: [
          GoRoute(path: '/', builder: (_, __) => const MyAppointmentsScreen()),
        ]),
      ),
    ));
    for (var i = 0; i < 5; i++) {
      await tester.pump(const Duration(milliseconds: 100));
    }
  }

  testWidgets('accept stays disabled until the declaration is ticked',
      (tester) async {
    await pump(tester);

    final accept = find.widgetWithText(FilledButton, 'Accept');
    expect(tester.widget<FilledButton>(accept).onPressed, isNull);

    await tester.tap(find.byKey(const Key('agree-7')));
    await tester.pump();
    await tester.tap(accept);
    await tester.pump();

    expect(service.accepted, [7]);
  });

  // FR-37.12d. The new role must reach the officials screens without a fresh login.
  testWidgets('reloads the session after an accept', (tester) async {
    await pump(tester);
    await tester.tap(find.byKey(const Key('agree-7')));
    await tester.pump();
    await tester.tap(find.widgetWithText(FilledButton, 'Accept'));
    await tester.pump();
    expect(_FakeAuthService.reloads, 1);
  });

  testWidgets('does not reload the session when the accept fails',
      (tester) async {
    await pump(tester, acceptFails: true);
    await tester.tap(find.byKey(const Key('agree-7')));
    await tester.pump();
    await tester.tap(find.widgetWithText(FilledButton, 'Accept'));
    await tester.pump();
    expect(_FakeAuthService.reloads, 0);
  });

  testWidgets('decline sends the trimmed reason', (tester) async {
    await pump(tester);

    await tester.enterText(find.byType(TextField), '  away that week  ');
    await tester.tap(find.widgetWithText(OutlinedButton, 'Decline'));
    await tester.pump();

    expect(service.declined, {7: 'away that week'});
  });
}
