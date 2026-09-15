import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:ghcaa_mobile/core/storage/storage_service.dart';
import 'package:ghcaa_mobile/main.dart' as app;

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  group('Member E2E Journey (Mobile)', () {
    testWidgets('Login -> Dashboard -> Logout full flow', (tester) async {
      // A token saved by an earlier run of this same built exe persists in
      // Windows secure storage across runs. Left in place, the router's
      // auth-state stream picks it up a couple seconds in and redirects
      // straight to the dashboard, pulling the login fields out from under
      // the test.
      await StorageService().clearAll();

      // main() installs a custom ErrorWidget.builder for the production
      // crash screen. IntegrationTestWidgetsFlutterBinding snapshots
      // ErrorWidget.builder before this closure runs and checks it's
      // unchanged the moment the closure returns — before any addTearDown
      // callback gets a turn, since those run at the outer test-package
      // level, after this whole body (and that check) has finished. So the
      // restore has to happen as the last line of the test body itself.
      final originalErrorWidgetBuilder = ErrorWidget.builder;

      // 1. Launch App
      app.main();
      await tester.pumpAndSettle();

      // 2. Perform Login (AppHomeScreen: TextField + LOGIN button)
      final usernameField = find.byType(TextField).first;
      final passwordField = find.byType(TextField).last;

      await tester.enterText(usernameField, 'demo_user');
      await tester.enterText(passwordField, 'DemoPass123!');
      // AppHomeScreen shows "LOGIN" both as a section header and as the
      // button label — scope the tap to the button itself.
      await tester.tap(find.widgetWithText(ElevatedButton, 'LOGIN'));
      
      // Wait for navigation and animations
      await tester.pumpAndSettle(const Duration(seconds: 5));

      // 3. Verify Dashboard (HashGen --apply seeds Member 9998 "Demo User")
      expect(find.text('DEMO USER'), findsOneWidget);
      expect(find.text('AUTHORIZED ACCESS'), findsOneWidget);

      // 4. Navigate to Digital ID
      final idCardAction = find.text('Digital ID');
      await tester.tap(idCardAction);
      await tester.pumpAndSettle();
      
      // The screen title is branding-driven ("Member Credentials" only shows
      // when org config has no appName set), so assert on the card content
      // instead, which is stable regardless of branding config.
      expect(find.text('PORTABLE PDF'), findsOneWidget);

      // 5. Logout (the logout button lives on the dashboard app bar, not on
      // Digital ID, so go back via the bottom-nav "Home" tab first)
      await tester.tap(find.text('Home'));
      await tester.pumpAndSettle();

      final logoutButton = find.byIcon(Icons.power_settings_new);
      await tester.tap(logoutButton);
      await tester.pumpAndSettle();
      
      // Confirm dialog logout. logout() clears secure storage then awaits
      // the router's auth stream re-emitting before returning. That await
      // doesn't itself mark any widget dirty, so pumpAndSettle can decide
      // the tree is idle and return before the redirect actually lands —
      // even with an explicit duration passed to it, since the duration
      // only paces pumps while a frame is already scheduled. Poll instead:
      // keep pumping in real time until the login screen shows up or the
      // budget below runs out.
      await tester.tap(find.text('LOGOUT'));
      await tester.pump();
      for (var i = 0; i < 50 && find.text('GHCAA AUTHENTICATION').evaluate().isEmpty; i++) {
        await tester.pump(const Duration(milliseconds: 200));
      }

      // 6. Verify back at login. The router sends a logged-out user to
      // LoginScreen, not the AppHomeScreen used at launch, and its heading
      // is a static string ("GHCAA AUTHENTICATION"), unlike the
      // branding-driven text around it.
      expect(find.text('GHCAA AUTHENTICATION'), findsOneWidget);

      ErrorWidget.builder = originalErrorWidgetBuilder;
    });
  });
}
