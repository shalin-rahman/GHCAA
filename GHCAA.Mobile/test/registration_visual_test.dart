import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:golden_toolkit/golden_toolkit.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:ghcaa_mobile/screens/auth/register_screen.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';

Widget wrapInApp(Widget child) => ProviderScope(
      child: MaterialApp(
        debugShowCheckedModeBanner: false,
        theme: AppTheme.midnightTheme,
        home: child,
      ),
    );

void main() {
  group('Registration Wizard Visual Freeze', () {
    testGoldens('Register Step 1: Identity & Contact', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const RegisterScreen()));
      await screenMatchesGolden(tester, 'registration_step_1');
    });

    testGoldens('Register Step 2: Academic & Career', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const RegisterScreen()));
      
      // Advance to Step 2
      final nextButton = find.text('CONTINUE');
      await tester.tap(nextButton);
      await tester.pumpAndSettle();
      
      await screenMatchesGolden(tester, 'registration_step_2');
    });

    testGoldens('Register Step 3: Preferences & Registry', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const RegisterScreen()));
      
      // Advance to Step 2
      await tester.tap(find.text('CONTINUE'));
      await tester.pumpAndSettle();
      
      // Advance to Step 3
      await tester.tap(find.text('CONTINUE'));
      await tester.pumpAndSettle();
      
      await screenMatchesGolden(tester, 'registration_step_3');
    });
  });
}
