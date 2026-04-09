import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:ghcaa_mobile/main.dart' as app;

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  group('Financial Portal E2E (Mobile)', () {
    testWidgets('Member should view payment history and dues', (tester) async {
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

      // 3. Verify Ledger Entries from Seed Data
      expect(find.text('Life Membership'), findsOneWidget);
      expect(find.text('5000.0'), findsOneWidget);
      expect(find.text('Annual Reunion 2026'), findsOneWidget);

      // 4. Verify Total Dues (if logic present)
      // expect(find.text('Outstanding: 0.0'), findsOneWidget);
    });
  });
}
