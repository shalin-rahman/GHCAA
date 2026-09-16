import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:ghcaa_mobile/core/storage/storage_service.dart';
import 'package:ghcaa_mobile/main.dart' as app;

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  group('Financial Portal E2E (Mobile)', () {
    testWidgets('Member should view payment history from seeded ledger', (tester) async {
      // A token saved by an earlier run of this same built exe persists in
      // Windows secure storage across runs and redirects straight to the
      // dashboard, pulling the login fields out from under the test.
      await StorageService().clearAll();

      // app.main() installs a global ErrorWidget.builder override as part of
      // normal bootstrap. The test binding checks it's unchanged the moment
      // this test body returns, before any addTearDown callback runs, so
      // restore it inline at the end instead.
      final originalErrorWidgetBuilder = ErrorWidget.builder;

      // 1. Launch App & Login
      app.main();
      await tester.pump();
      for (var i = 0; i < 50 && find.byType(TextField).evaluate().isEmpty; i++) {
        await tester.pump(const Duration(milliseconds: 200));
      }

      await tester.enterText(find.byType(TextField).first, 'demo_user');
      await tester.enterText(find.byType(TextField).last, 'DemoPass123!');
      // AppHomeScreen shows "LOGIN" both as a section header and as the
      // button label — scope the tap to the button itself.
      await tester.pump(const Duration(milliseconds: 300));
      await tester.tap(find.widgetWithText(ElevatedButton, 'LOGIN'));
      await tester.pumpAndSettle(const Duration(seconds: 5));

      // 2. Open Financials from the dashboard grid
      // The dashboard grid extends past the visible window height, so
      // 'Payments' can render below the viewport (hit-test warning: offset
      // outside the render tree bounds) unless the containing scrollable
      // brings it into view first.
      await tester.ensureVisible(find.text('Payments'));
      await tester.pump();
      await tester.tap(find.text('Payments'));
      await tester.pump();
      // ledgerProvider/duesProvider/savedMethodsProvider are real async
      // fetches that don't schedule a frame while pending, so a bare
      // pumpAndSettle can return before the ledger row actually renders.
      // Poll instead.
      for (var i = 0; i < 50 && find.text('MembershipFee').evaluate().isEmpty; i++) {
        await tester.pump(const Duration(milliseconds: 200));
      }
      await tester.pumpAndSettle();

      // 3. Verify the payment row HashGen --apply seeds for the demo member
      // (GHCAA.Domain.Enums.FinancialCategory.MembershipFee, Amount 5000 —
      // see HashGen/Program.cs). financial_service.dart maps the ledger
      // description from the raw financialCategory enum name, not a
      // free-text field, so "MembershipFee" is what actually renders, and
      // formatCurrency renders the amount with two decimals and thousands
      // separator regardless of which currency symbol the org is configured
      // with — match on that part rather than the symbol.
      expect(find.text('MembershipFee'), findsOneWidget);
      expect(find.textContaining('5,000.00'), findsOneWidget);

      ErrorWidget.builder = originalErrorWidgetBuilder;
    });
  });
}
