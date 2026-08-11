import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:ghcaa_mobile/main.dart' as app;

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  group('Member E2E Journey (Mobile)', () {
    testWidgets('Login -> Dashboard -> Logout full flow', (tester) async {
      // 1. Launch App
      app.main();
      await tester.pumpAndSettle();

      // 2. Perform Login (AppHomeScreen: TextField + LOGIN button)
      final usernameField = find.byType(TextField).first;
      final passwordField = find.byType(TextField).last;

      await tester.enterText(usernameField, 'demo_user');
      await tester.enterText(passwordField, 'DemoPass123!');
      await tester.tap(find.text('LOGIN'));
      
      // Wait for navigation and animations
      await tester.pumpAndSettle(const Duration(seconds: 5));

      // 3. Verify Dashboard (HashGen --apply seeds Member 9998 "Demo User")
      expect(find.text('DEMO USER'), findsOneWidget);
      expect(find.text('AUTHORIZED ACCESS'), findsOneWidget);

      // 4. Navigate to Digital ID
      final idCardAction = find.text('Digital ID');
      await tester.tap(idCardAction);
      await tester.pumpAndSettle();
      
      expect(find.text('Member Credentials'), findsOneWidget);

      // 5. Logout
      final logoutButton = find.byIcon(Icons.power_settings_new);
      await tester.tap(logoutButton);
      await tester.pumpAndSettle();
      
      // Confirm dialog logout
      await tester.tap(find.text('LOGOUT'));
      await tester.pumpAndSettle();

      // 6. Verify back at login
      expect(find.text('Haragangian Portal'), findsOneWidget);
    });
  });
}
