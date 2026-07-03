import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:ghcaa_mobile/main.dart' as app;

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  group('Member Journey E2E', () {
    testWidgets('Full Member Lifecycle: Login to Profile Verification', (tester) async {
      // 1. Boot Application
      app.main();
      await tester.pumpAndSettle();

      // 2. Login Phase (AppHomeScreen)
      await tester.enterText(find.byType(TextField).first, 'demo_user');
      await tester.enterText(find.byType(TextField).last, 'DemoPass123!');
      await tester.tap(find.text('LOGIN'));

      // Allow time for API call and transitions
      await tester.pumpAndSettle(const Duration(seconds: 5));

      // 3. Dashboard Verification (HashGen --apply: Member 9998)
      expect(find.text('DEMO USER'), findsOneWidget);
      expect(find.text('AUTHORIZED ACCESS'), findsOneWidget);

      // 4. Directory Navigation
      await tester.tap(find.text('Alumni Directory'));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      expect(find.text('Member Directory'), findsOneWidget);

      // 5. Return to Dashboard and open Profile via drawer
      await tester.pageBack();
      await tester.pumpAndSettle();

      await tester.tap(find.byIcon(Icons.menu_rounded));
      await tester.pumpAndSettle();
      await tester.tap(find.text('My Profile'));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      expect(find.text('My Profile'), findsOneWidget);

      // 6. Logout Flow (return to dashboard first)
      await tester.tap(find.text('Home'));
      await tester.pumpAndSettle();

      await tester.tap(find.byIcon(Icons.power_settings_new));
      await tester.pumpAndSettle();

      await tester.tap(find.text('LOGOUT'));
      await tester.pumpAndSettle();

      // Verify back at portal home
      expect(find.text('Haragangian Portal'), findsOneWidget);
    });
  });
}
