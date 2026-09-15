import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:ghcaa_mobile/core/storage/storage_service.dart';
import 'package:ghcaa_mobile/main.dart' as app;

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  group('Member Journey E2E', () {
    testWidgets('Full Member Lifecycle: Login to Profile Verification', (tester) async {
      // AppScaffold hides its drawer entirely at width >= 600 (tablet
      // layout). The Windows test window defaults wider than that, so
      // without this the hamburger tap is a silent no-op and there's no
      // drawer content to find at all.
      tester.view.physicalSize = const Size(480, 900);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      // A token saved by an earlier run of this same built exe persists in
      // Windows secure storage across runs and redirects straight to the
      // dashboard, pulling the login fields out from under the test.
      await StorageService().clearAll();

      // app.main() installs a global ErrorWidget.builder override
      // (see setupErrorHandlers()) as part of normal bootstrap. The test
      // binding asserts ErrorWidget.builder is unchanged when the test body
      // returns, before any addTearDown callback runs, so restore it inline
      // at the end of this test rather than via addTearDown.
      final originalErrorWidgetBuilder = ErrorWidget.builder;

      // 1. Boot Application
      app.main();
      await tester.pump();
      for (var i = 0; i < 50 && find.byType(TextField).evaluate().isEmpty; i++) {
        await tester.pump(const Duration(milliseconds: 200));
      }

      // 2. Login Phase (AppHomeScreen)
      await tester.enterText(find.byType(TextField).first, 'demo_user');
      await tester.enterText(find.byType(TextField).last, 'DemoPass123!');
      // Entering text can shift the layout (focus scroll / keyboard inset)
      // after the tap offset would otherwise be computed, so pump one frame
      // first. A bare pumpAndSettle() here hangs until its internal timeout,
      // since this screen has a continuously running background animation
      // that never stops scheduling frames.
      await tester.pump(const Duration(milliseconds: 300));
      // AppHomeScreen shows "LOGIN" both as a section header and as the
      // button label — scope the tap to the button itself.
      await tester.tap(find.widgetWithText(ElevatedButton, 'LOGIN'));

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
      // AppScaffold deliberately omits a back button on '/directory' (it's a
      // bottom-nav tab, not a pushed detail screen), so pageBack() has
      // nothing to find here. Use the same bottom-nav 'Home' tap the app
      // itself relies on for this route.
      await tester.tap(find.text('Home'));
      await tester.pumpAndSettle();

      await tester.tap(find.byIcon(Icons.menu_rounded));
      await tester.pump();
      // The drawer's menu section waits on roleProvider, a real async fetch
      // that doesn't itself schedule a frame while pending, so pumpAndSettle
      // can return before "My Profile" is actually rendered. Poll instead.
      for (var i = 0; i < 50 && find.text('My Profile').evaluate().isEmpty; i++) {
        await tester.pump(const Duration(milliseconds: 200));
      }
      // The drawer is still sliding in when its text first appears in the
      // tree, so its on-screen position isn't final yet — settle the slide
      // animation before computing a tap offset, or the tap can land off the
      // left edge of the viewport.
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
      await tester.pump();
      for (var i = 0; i < 50 && find.text('GHCAA AUTHENTICATION').evaluate().isEmpty; i++) {
        await tester.pump(const Duration(milliseconds: 200));
      }

      // Verify back at login. Router sends a logged-out user to LoginScreen,
      // whose heading is the static 'GHCAA AUTHENTICATION', not branding text.
      expect(find.text('GHCAA AUTHENTICATION'), findsOneWidget);

      ErrorWidget.builder = originalErrorWidgetBuilder;
    });
  });
}
