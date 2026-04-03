import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:ghcaa_mobile/core/config/app_config.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/screens/app_home_screen.dart';

void main() {
  setUp(() async {
    // Initialize mock env for tests
    dotenv.testLoad(fileInput: 'PORTAL_TITLE=Haragangian\nORG_TAGLINE=Sharing Heritage, Aligning Lives, Integrating Networks');
  });

  group('GHCAA UI Consistency Tests', () {
    testWidgets('AppHomeScreen should present Midnight Gold branding elements',
        (WidgetTester tester) async {
      await tester.pumpWidget(MaterialApp(
        theme: AppTheme.midnightTheme,
        home: const AppHomeScreen(),
      ));
      await tester.pumpAndSettle();

      // Assert Presence of Branded Title
      expect(find.text(AppConfig.portalTitle), findsOneWidget);
      
      // Assert Presence of Mission Motto
      expect(find.text(AppConfig.organizationTagline), findsOneWidget);
      
      // Assert Interaction Touch-points
      expect(find.text('Register as Member'), findsOneWidget);
      expect(find.text('Member / Admin Login'), findsOneWidget);
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

      // We check the button's style (might be null if using theme)
      // and fallback to checking the theme's elevatedButtonTheme
      final color = button.style?.backgroundColor?.resolve({}) ??
          Theme.of(tester.element(finder))
              .elevatedButtonTheme
              .style
              ?.backgroundColor
              ?.resolve({});

      expect(color, AppTheme.royalGold);
    });
  });
}
