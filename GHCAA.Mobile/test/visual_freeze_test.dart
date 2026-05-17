import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:golden_toolkit/golden_toolkit.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:go_router/go_router.dart';
import 'package:ghcaa_mobile/screens/member/digital_id_screen.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:ghcaa_mobile/core/storage/storage_service.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/core/services/biometric_service.dart';
import 'package:local_auth/local_auth.dart';

// --- Minimal Fakes ---
class _FakeStorageService implements StorageService {
  @override Future<void> saveToken(String token) async {}
  @override Future<String?> getToken() async => 'mock-token';
  @override Future<void> removeToken() async {}
  @override Future<void> saveRole(String role) async {}
  @override Future<String?> getRole() async => 'Member';
  @override Future<void> saveDashboardLayout(bool isCompact) async {}
  @override Future<bool> getDashboardLayout() async => false;
  @override Future<void> saveProfile(Map<String, dynamic> profile) async {}
  @override Future<Map<String, dynamic>?> getProfile() async => null;
  @override Future<void> clearAll() async {}
  @override Future<void> saveCredentials(String username, String password) async {}
  @override Future<Map<String, String>?> getCredentials() async => null;
  @override Future<void> clearCredentials() async {}
}

class _FakeAuthService implements AuthService {
  @override Future<String?> login(String identifier, String password, {bool enableBiometric = false}) async => null;
  @override Future<void> logout() async {}
  @override Future<String?> register(Map<String, dynamic> data) async => null;
  @override Future<bool> forgotPassword(String identifier) async => true;
  @override Future<String?> getRole() async => 'Member';
  @override Future<bool> updateProfile(Map<String, dynamic> data) async => true;
  @override Future<List<Map<String, dynamic>>> getSocialProviders() async => [];
  @override Future<String?> googleLogin(String idToken) async => null;
  @override Future<String?> facebookLogin(String accessToken) async => null;
}

class _FakeBiometricService implements BiometricService {
  @override Future<bool> isBiometricsAvailable() async => false;
  @override Future<List<BiometricType>> getAvailableBiometrics() async => [];
  @override Future<bool> authenticate({required String reason}) async => true;
}

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
      storageServiceProvider.overrideWith((ref) => _FakeStorageService()),
      authServiceProvider.overrideWith((ref) => _FakeAuthService()),
      biometricServiceProvider.overrideWith((ref) => _FakeBiometricService()),
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

    TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger.setMockMethodCallHandler(
      const MethodChannel('plugins.flutter.io/local_auth'),
      (methodCall) async {
        if (methodCall.method == 'getAvailableBiometrics') return <String>[];
        if (methodCall.method == 'isDeviceSupported') return false;
        return null;
      },
    );
  });

  group('Digital ID Visual Freeze', () {
    testGoldens('Identity: Member Digital ID Card should match baseline', (tester) async {
      await tester.pumpWidgetBuilder(
        _wrapInApp(const DigitalIDScreen()),
        surfaceSize: const Size(390, 844),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 50));
      await screenMatchesGolden(tester, 'digital_id_card_portrait');
    });
  });
}
