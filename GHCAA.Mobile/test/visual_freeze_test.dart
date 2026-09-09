@Tags(['golden'])
library;

import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:golden_toolkit/golden_toolkit.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:go_router/go_router.dart';
import 'package:ghcaa_mobile/screens/member/digital_id_screen.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:ghcaa_mobile/core/storage/storage_service.dart';
import 'package:ghcaa_mobile/core/services/biometric_service.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';

import 'helpers/fake_services.dart';
import 'helpers/golden_test_utils.dart';

Widget _wrapInApp(Widget child) {
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
      userProfileProvider.overrideWith((ref) => {
        'fullName': 'Test Member',
        'membershipType': 'Life Member',
        'category': 'Active',
        'batch': '2010',
        'membershipId': 'L-9999',
      }),
      roleProvider.overrideWith((ref) => Future.value('Member')),
      storageServiceProvider.overrideWith((ref) => FakeStorageService()),
      authServiceProvider.overrideWith((ref) => FakeAuthService()),
      biometricServiceProvider.overrideWith((ref) => FakeBiometricService()),
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

  group('Digital ID Visual Freeze', () {
    testGoldens('Identity: Member Digital ID Card should match baseline', (tester) async {
      await tester.pumpWidgetBuilder(
        _wrapInApp(const DigitalIDScreen()),
        surfaceSize: const Size(390, 844),
      );
      await pumpAndSettleShort(tester);
      await screenMatchesGolden(tester, 'digital_id_card_portrait');
    });
  });
}
