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

      // 2. Perform Login
      final usernameField = find.byType(TextFormField).first;
      final passwordField = find.byType(TextFormField).last;
      final loginButton = find.byType(ElevatedButton);

      await tester.enterText(usernameField, 'demo_user@test.com');
      await tester.enterText(passwordField, 'DemoPass123!');
      await tester.tap(loginButton);
      
      // Wait for navigation and animations
      await tester.pumpAndSettle(const Duration(seconds: 2));

      // 3. Verify Dashboard
      expect(find.text('SHALIN RAHMAN'), findsOneWidget);
      expect(find.text('AUTHORIZED ACCESS'), findsOneWidget);

      // 4. Navigate to Digital ID
      final idCardAction = find.text('Digital ID');
      await tester.tap(idCardAction);
      await tester.pumpAndSettle();
      
      expect(find.text('Digital ID Card'), findsOneWidget);

      // 5. Logout
      final logoutButton = find.byIcon(Icons.power_settings_new);
      await tester.tap(logoutButton);
      await tester.pumpAndSettle();
      
      // Confirm dialog logout
      await tester.tap(find.text('LOGOUT'));
      await tester.pumpAndSettle();

      // 6. Verify back at login
      expect(find.text('Haragangian Alumni'), findsOneWidget);
    });
  });
}
