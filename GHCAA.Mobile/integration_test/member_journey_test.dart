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

      // 2. Login Phase
      final loginIdField = find.byType(TextField).at(0); // Quick Login ID
      final passwordField = find.byType(TextField).at(1); // PasswordField uses TextField internally
      final loginButton = find.text('LOGIN');

      await tester.enterText(loginIdField, '9000001');
      await tester.enterText(passwordField, 'DemoPass123!');
      await tester.tap(loginButton);
      
      // Allow time for API call and transitions
      await tester.pumpAndSettle(const Duration(seconds: 3));

      // 3. Dashboard Verification
      // Check for presence of key dashboard elements
      expect(find.text('TEST MEMBER ONE (COMPLETE)'), findsOneWidget);
      expect(find.text('GHC-9000001'), findsOneWidget);
      expect(find.text('AUTHORIZED ACCESS'), findsOneWidget);
      
      // 4. Directory Navigation & Search
      final directoryCard = find.text('Directory');
      await tester.tap(directoryCard);
      await tester.pumpAndSettle();
      
      expect(find.text('ALUMNI DIRECTORY'), findsOneWidget);
      
      // Perform a search
      final searchField = find.byIcon(Icons.search);
      await tester.tap(searchField);
      await tester.enterText(find.byType(TextField), 'Professor');
      await tester.pumpAndSettle(const Duration(milliseconds: 500));
      
      // 5. Profile Verification
      // Navigate back to Dashboard (using bottom nav if present, or pop)
      if (find.byIcon(Icons.home).evaluate().isNotEmpty) {
        await tester.tap(find.byIcon(Icons.home));
      } else {
        await tester.pageBack();
      }
      await tester.pumpAndSettle();
      
      // Go to Profile
      final profileAction = find.byIcon(Icons.person_outline);
      await tester.tap(profileAction);
      await tester.pumpAndSettle();
      
      expect(find.text('MEMBER PROFILE'), findsOneWidget);
      expect(find.text('tester1@example.com'), findsOneWidget);
      
      // 6. Logout Flow
      final logoutBtn = find.byIcon(Icons.power_settings_new);
      await tester.tap(logoutBtn);
      await tester.pumpAndSettle();
      
      await tester.tap(find.text('LOGOUT'));
      await tester.pumpAndSettle();
      
      // Verify back at Welcome screen
      expect(find.text('QUICK LOGIN'), findsOneWidget);
    });
  });
}
