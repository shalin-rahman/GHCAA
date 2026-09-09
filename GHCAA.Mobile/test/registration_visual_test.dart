@Tags(['golden'])
library;

import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:golden_toolkit/golden_toolkit.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:go_router/go_router.dart';

import 'package:ghcaa_mobile/screens/auth/register_screen.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:ghcaa_mobile/features/auth/register_wizard_provider.dart';
import 'package:ghcaa_mobile/features/lookups/dropdown_service.dart';
import 'package:ghcaa_mobile/features/files/file_service.dart';
import 'package:ghcaa_mobile/core/storage/storage_service.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/core/services/biometric_service.dart';

import 'helpers/fake_services.dart';
import 'helpers/golden_test_utils.dart';

// This wizard needs a dropdown fixture with exactly one option (unlike the
// other visual-freeze files, which use zero or two) — kept local rather than
// merged into helpers/fake_services.dart since it covers a different case.
class _FakeDropdownService implements DropdownService {
  @override Future<List<Map<String, String>>> getOptions(String group) async => [
    {'value': '1', 'label': 'Option A', 'instructions': 'Instruction text'},
  ];
}

/// Build the wrapped app with all required provider overrides.
/// [step] (0-indexed) pre-seeds the wizard at a specific step.
Widget _wrapInApp(Widget child, {int step = 0}) {
  final router = GoRouter(
    initialLocation: '/',
    routes: [
      GoRoute(path: '/', builder: (context, state) => child),
      GoRoute(path: '/login', builder: (context, state) => const SizedBox()),
      GoRoute(path: '/dashboard', builder: (context, state) => const SizedBox()),
    ],
  );

  return ProviderScope(
    overrides: [
      storageServiceProvider.overrideWith((ref) => FakeStorageService()),
      authServiceProvider.overrideWith((ref) => FakeAuthService()),
      dropdownDataProvider.overrideWith((ref) => _FakeDropdownService()),
      fileServiceProvider.overrideWith((ref) => FakeFileService()),
      biometricServiceProvider.overrideWith((ref) => FakeBiometricService()),
      // Pre-seed wizard at the desired step — bypasses form validation
      registerWizardProvider.overrideWith(
        (ref) => RegisterWizardNotifier()..setStep(step),
      ),
    ],
    child: MaterialApp.router(
      routerConfig: router,
      debugShowCheckedModeBanner: false,
      theme: AppTheme.midnightTheme,
    ),
  );
}

void main() {
  setUpAll(() async {
    await loadAppFonts();
    dotenv.testLoad(fileInput: 'PORTAL_TITLE=GHCAA\nPORTAL_SUBTITLE=ALUMNI');
    mockLocalAuthChannel();
  });

  group('Registration Wizard Visual Freeze', () {
    // Same pump/golden-compare shape for every wizard step, only the seeded
    // step index and golden file name change.
    const steps = [
      (label: 'Register Step 1: Identity & Contact', step: 0, golden: 'registration_step_1'),
      (label: 'Register Step 2: Academic & Career', step: 1, golden: 'registration_step_2'),
      (label: 'Register Step 3: Preferences & Registry', step: 2, golden: 'registration_step_3'),
    ];

    for (final s in steps) {
      testGoldens(s.label, (tester) async {
        await tester.pumpWidgetBuilder(
          _wrapInApp(const RegisterScreen(), step: s.step),
          surfaceSize: const Size(390, 844),
        );
        await pumpAndSettleShort(tester);
        await screenMatchesGolden(tester, s.golden);
      });
    }
  });
}
