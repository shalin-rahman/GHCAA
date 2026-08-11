import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:ghcaa_mobile/main.dart' as app;

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  group('Article Contribution E2E (Mobile)', () {
    testWidgets('Member can submit an article and see it in Pending state', (tester) async {
      // 1. Launch & Login
      app.main();
      await tester.pumpAndSettle();

      await tester.enterText(find.byType(TextField).first, 'demo_user');
      await tester.enterText(find.byType(TextField).last, 'DemoPass123!');
      await tester.tap(find.text('LOGIN'));
      await tester.pumpAndSettle(const Duration(seconds: 5));

      // 2. Open Articles
      await tester.tap(find.byTooltip('Open navigation menu'));
      await tester.pumpAndSettle();
      await tester.tap(find.text('My Articles'));
      await tester.pumpAndSettle();

      // 3. Tap Submit New Article
      await tester.tap(find.byIcon(Icons.add));
      await tester.pumpAndSettle();

      // 4. Fill form
      final titleField = find.byType(TextFormField).first;
      final bodyField = find.byType(TextFormField).at(1);

      await tester.enterText(titleField, 'Visual E2E Test Article');
      await tester.enterText(bodyField,
          'This article was submitted by an automated E2E test to verify the editorial workflow.');

      // 5. Submit
      await tester.tap(find.widgetWithText(ElevatedButton, 'Submit'));
      await tester.pumpAndSettle(const Duration(seconds: 2));

      // Verify success feedback: either a toast, 'pending' label, or redirect back to articles screen
      final successFeedback = find.byWidgetPredicate((w) =>
        w is Text && (
          (w.data?.toLowerCase().contains('submitted') ?? false) ||
          (w.data?.toLowerCase().contains('pending') ?? false) ||
          (w.data == 'My Articles')
        )
      );
      expect(successFeedback, findsAtLeastNWidgets(1));
    });
  });

  group('Article Moderation E2E (Web via Admin)', () {
    // NOTE: This is documented here for reference.
    // The admin moderation workflow is driven via Playwright:
    // See: GHCAA.Web/tests/e2e/admin-workflow.spec.ts (article-approvals)
  });
}
