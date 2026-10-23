// FR-39
// The authenticator app screen walks through setup, shows recovery codes once,
// and guards new codes and removal behind a confirm.
import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:ghcaa_mobile/core/config/org_config.dart';
import 'package:ghcaa_mobile/core/services/org_config_service.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:ghcaa_mobile/features/auth/authenticator_service.dart';
import 'package:ghcaa_mobile/features/theme/dynamic_theme_service.dart';
import 'package:ghcaa_mobile/screens/member/authenticator_screen.dart';

class _FakeAuthenticatorService implements AuthenticatorService {
  _FakeAuthenticatorService({this.enrolled = false, this.confirmFails = false, this.noEmail = false});

  bool enrolled;
  final bool confirmFails;
  final bool noEmail;
  String? setupPassword;
  int stepUpSetups = 0;
  int codesLeft = 10;
  String? confirmedWith;
  int regenerated = 0;
  int removed = 0;

  @override
  Future<AuthenticatorStatus> status() async =>
      AuthenticatorStatus(
          enrolled: enrolled, pending: false, recoveryCodesLeft: enrolled ? codesLeft : 0, setupNeedsPassword: noEmail);

  @override
  Future<AuthenticatorSetup> setup() async {
    stepUpSetups++;
    return const AuthenticatorSetup(
        secret: 'JBSWY3DPEHPK3PXP', otpAuthUri: 'otpauth://totp/Test:a?secret=JBSWY3DPEHPK3PXP');
  }

  @override
  Future<AuthenticatorSetup> setupWithPassword(String password) async {
    setupPassword = password;
    return const AuthenticatorSetup(secret: 'PWSECRET', otpAuthUri: 'otpauth://totp/Test:a?secret=PWSECRET');
  }

  @override
  Future<List<String>> confirm(String code) async {
    confirmedWith = code;
    if (confirmFails) {
      final req = RequestOptions(path: '/auth/authenticator/confirm');
      throw DioException(
          requestOptions: req,
          response: Response(requestOptions: req, statusCode: 400, data: {'detail': 'Code did not match.'}));
    }
    enrolled = true;
    return ['AAAAA-11111', 'BBBBB-22222'];
  }

  @override
  Future<List<String>> regenerateRecoveryCodes() async {
    regenerated++;
    return ['CCCCC-33333'];
  }

  @override
  Future<void> remove() async {
    removed++;
    enrolled = false;
  }
}

Future<void> _pump(WidgetTester tester, _FakeAuthenticatorService fake) async {
  await tester.binding.setSurfaceSize(const Size(400, 1000));
  addTearDown(() => tester.binding.setSurfaceSize(null));
  await tester.pumpWidget(ProviderScope(
    overrides: [
      orgBrandingProvider.overrideWithValue(OrgBranding.fromJson(const {'shortName': 'EAA'})),
      authenticatorServiceProvider.overrideWith((ref) => fake),
      roleProvider.overrideWith((ref) => Future.value('Member')),
      activeSpecialThemeProvider.overrideWith((ref) async => null),
    ],
    child: MaterialApp.router(
      theme: AppTheme.midnightTheme,
      routerConfig: GoRouter(routes: [
        GoRoute(path: '/', builder: (_, __) => const AuthenticatorScreen()),
      ]),
    ),
  ));
  await _settle(tester);
}

// pumpAndSettle never settles with the scaffold's animations, so pump a few frames.
Future<void> _settle(WidgetTester tester) async {
  for (var i = 0; i < 5; i++) {
    await tester.pump(const Duration(milliseconds: 100));
  }
}

Future<void> _tap(WidgetTester tester, Finder finder) async {
  await tester.ensureVisible(finder);
  await tester.tap(finder);
  await _settle(tester);
}

void main() {
  testWidgets('FR-39: setup shows the key, turns the app on and shows recovery codes once', (tester) async {
    final fake = _FakeAuthenticatorService();
    await _pump(tester, fake);

    expect(find.byKey(const Key('authPassword')), findsNothing);
    await _tap(tester, find.byKey(const Key('authSetupButton')));
    expect(find.text('JBSWY3DPEHPK3PXP'), findsOneWidget);
    expect(fake.setupPassword, isNull);

    await tester.enterText(find.byKey(const Key('authConfirmCode')), '123456');
    await _tap(tester, find.byKey(const Key('authConfirmButton')));

    expect(fake.confirmedWith, '123456');
    expect(find.text('AAAAA-11111'), findsOneWidget);

    await _tap(tester, find.byKey(const Key('authCodesDone')));
    expect(find.text('AAAAA-11111'), findsNothing);
    expect(find.text('Recovery codes left: 10'), findsOneWidget);
  });

  // 95.6: no email means no email step-up, so setup asks for the password.
  testWidgets('FR-39: with no email on file, setup asks for the password', (tester) async {
    final fake = _FakeAuthenticatorService(noEmail: true);
    await _pump(tester, fake);

    await _tap(tester, find.byKey(const Key('authSetupButton')));
    expect(fake.setupPassword, isNull);
    expect(find.text('Enter your password.'), findsOneWidget);

    await tester.enterText(find.byKey(const Key('authPassword')), 'secret-pw');
    await _tap(tester, find.byKey(const Key('authSetupButton')));

    expect(fake.setupPassword, 'secret-pw');
    expect(fake.stepUpSetups, 0);
    expect(find.text('PWSECRET'), findsOneWidget);
  });

  testWidgets('FR-39: a short code is not sent', (tester) async {
    final fake = _FakeAuthenticatorService();
    await _pump(tester, fake);

    await _tap(tester, find.byKey(const Key('authSetupButton')));
    await tester.enterText(find.byKey(const Key('authConfirmCode')), '123');
    await _tap(tester, find.byKey(const Key('authConfirmButton')));

    expect(fake.confirmedWith, isNull);
    expect(find.text('Enter the 6-digit code from the app.'), findsOneWidget);
  });

  testWidgets('FR-39: a wrong code shows the server message and stays on setup', (tester) async {
    final fake = _FakeAuthenticatorService(confirmFails: true);
    await _pump(tester, fake);

    await _tap(tester, find.byKey(const Key('authSetupButton')));
    await tester.enterText(find.byKey(const Key('authConfirmCode')), '654321');
    await _tap(tester, find.byKey(const Key('authConfirmButton')));

    expect(find.text('Code did not match.'), findsOneWidget);
    expect(find.byKey(const Key('authConfirmCode')), findsOneWidget);
  });

  testWidgets('FR-39: removing the app needs a confirm', (tester) async {
    final fake = _FakeAuthenticatorService(enrolled: true);
    await _pump(tester, fake);

    await _tap(tester, find.byKey(const Key('authRemove')));
    await _tap(tester, find.text('CANCEL'));
    expect(fake.removed, 0);

    await _tap(tester, find.byKey(const Key('authRemove')));
    await _tap(tester, find.text('REMOVE APP'));
    expect(fake.removed, 1);
    expect(find.byKey(const Key('authSetupButton')), findsOneWidget);
  });

  testWidgets('FR-39: new recovery codes are shown after a confirm', (tester) async {
    final fake = _FakeAuthenticatorService(enrolled: true);
    await _pump(tester, fake);

    await _tap(tester, find.byKey(const Key('authNewCodes')));
    await _tap(tester, find.text('MAKE NEW CODES'));

    expect(fake.regenerated, 1);
    expect(find.text('CCCCC-33333'), findsOneWidget);
  });
}
