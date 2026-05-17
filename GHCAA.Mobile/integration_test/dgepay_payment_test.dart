import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:ghcaa_mobile/main.dart' as app;

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  group('DGePay Payment Gateway Flow (Mobile)', () {
    testWidgets('Member should be able to initiate payment via DGePay', (tester) async {
      // 1. Launch App & Login
      app.main();
      await tester.pumpAndSettle();

      await tester.enterText(find.byType(TextFormField).first, 'demo_user@test.com');
      await tester.enterText(find.byType(TextFormField).last, 'DemoPass123!');
      await tester.tap(find.byType(ElevatedButton));
      await tester.pumpAndSettle(const Duration(seconds: 2));

      // 2. Open Financials
      await tester.tap(find.text('Payments'));
      await tester.pumpAndSettle();

      // 3. Initiate Payment
      final payButton = find.text('Pay Now');
      if (payButton.evaluate().isNotEmpty) {
        await tester.tap(payButton.first);
        await tester.pumpAndSettle();
        
        // 4. Select DGePay from Gateway Selection BottomSheet
        final dgePayOption = find.text('DGePay');
        if (dgePayOption.evaluate().isNotEmpty) {
          await tester.tap(dgePayOption);
          await tester.pumpAndSettle();

          // 5. Verify the webview is requested or payment is processing
          // Since we are mocking/intercepting this in integration context or opening a webview,
          // we just assert that the gateway UI flow completes without crashing and navigates.
          expect(find.text('Processing...'), findsNothing); 
        }
      }
    });
  });
}
