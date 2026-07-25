import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:ghcaa_mobile/core/config/app_config.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/core/services/biometric_service.dart';
import 'package:ghcaa_mobile/screens/app_home_screen.dart';

// Fake BiometricService that never triggers platform channels.
class _FakeBiometricService extends BiometricService {
  @override
  Future<bool> isBiometricsAvailable() async => false;
}

void main() {
  setUp(() async {
    dotenv.testLoad(fileInput: 'PORTAL_TITLE=Haragangian\nORG_TAGLINE=Sharing Heritage, Aligning Lives, Integrating Networks');
    SharedPreferences.setMockInitialValues({});
  });

  group('GHCAA UI Consistency Tests', () {
    testWidgets('AppHomeScreen should present Midnight Gold branding elements',
        (WidgetTester tester) async {
      await tester.pumpWidget(
        ProviderScope(
          overrides: [
            biometricServiceProvider.overrideWithValue(_FakeBiometricService()),
          ],
          child: MaterialApp(
            theme: AppTheme.midnightTheme,
            home: const AppHomeScreen(),
          ),
        ),
      );
      await tester.pumpAndSettle();

      // Assert Presence of Branded Title
      expect(find.text(AppConfig.portalTitle), findsOneWidget);

      // Assert Presence of Mission Motto
      expect(find.text(AppConfig.organizationTagline), findsOneWidget);

      // Assert Interaction Touch-points (section label + submit button + register link)
      expect(find.text('LOGIN'), findsNWidgets(2));
      expect(find.text('REGISTER HERE'), findsOneWidget);
    });

    testWidgets('ElevatedButtons should follow the royalGold design system',
        (WidgetTester tester) async {
      await tester.pumpWidget(MaterialApp(
        theme: AppTheme.midnightTheme,
        home: Scaffold(
          body: Center(
              child:
                  ElevatedButton(onPressed: () {}, child: const Text('Test'))),
        ),
      ));

      final finder = find.byType(ElevatedButton);
      final ElevatedButton button = tester.widget(finder);

      final color = button.style?.backgroundColor?.resolve({}) ??
          Theme.of(tester.element(finder))
              .elevatedButtonTheme
              .style
              ?.backgroundColor
              ?.resolve({});

      // The theme is tenant-driven (buildTheme(branding)); the design-system
      // gold for interactive surfaces is the accent = colorScheme.secondary.
      expect(color, AppTheme.midnightTheme.colorScheme.secondary);
    });
  });
}
