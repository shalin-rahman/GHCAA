import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/screens/app_home_screen.dart';

void main() {
  group('GHCAA UI Consistency Tests', () {
    testWidgets('AppHomeScreen should present Midnight Gold branding elements', (WidgetTester tester) async {
      await tester.pumpWidget(MaterialApp(
        theme: AppTheme.midnightTheme,
        home: const AppHomeScreen(),
      ));

      // Assert Presence of Branded Title
      expect(find.text('GHCAA PORTAL'), findsOneWidget);
      
      // Assert Presence of Mission Motto
      expect(find.textContaining('Sharing Heritage'), findsOneWidget);
      
      // Assert Interaction Touch-points
      expect(find.text('Register as Member'), findsOneWidget);
      expect(find.text('Member / Admin Login'), findsOneWidget);
    });

    testWidgets('ElevatedButtons should follow the royalGold design system', (WidgetTester tester) async {
      await tester.pumpWidget(MaterialApp(
        theme: AppTheme.midnightTheme,
        home: Scaffold(
          body: Center(child: ElevatedButton(onPressed: () {}, child: const Text('Test'))),
        ),
      ));

      final ElevatedButton button = tester.widget(find.byType(ElevatedButton));
      final color = button.style?.backgroundColor?.resolve({});
      expect(color, AppTheme.royalGold);
    });
  });
}
