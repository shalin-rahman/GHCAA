// FR-39
// The step-up dialog offers an emailed code to an app user only when asked to.
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/api/step_up_interceptor.dart';
import 'package:ghcaa_mobile/core/widgets/step_up_dialog.dart';

class _Result {
  String? value;
}

Future<_Result> _open(WidgetTester tester, {required bool canSwitch}) async {
  final result = _Result();
  await tester.pumpWidget(MaterialApp(
    home: Builder(
      builder: (context) => ElevatedButton(
        onPressed: () async {
          result.value = await showStepUpDialog(context, useApp: true, canSwitch: canSwitch);
        },
        child: const Text('open'),
      ),
    ),
  ));
  await tester.tap(find.text('open'));
  await tester.pumpAndSettle();
  return result;
}

void main() {
  testWidgets('FR-39: an app user can pick an emailed code instead', (tester) async {
    final result = await _open(tester, canSwitch: true);

    await tester.tap(find.byKey(const Key('stepUpEmailInstead')));
    await tester.pumpAndSettle();

    expect(result.value, StepUpInterceptor.emailInstead);
  });

  testWidgets('FR-39: no email option when the user cannot switch', (tester) async {
    await _open(tester, canSwitch: false);

    expect(find.byKey(const Key('stepUpEmailInstead')), findsNothing);
  });
}
