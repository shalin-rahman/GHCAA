import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:golden_toolkit/golden_toolkit.dart';
import 'package:ghcaa_mobile/screens/member/dashboard_screen.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';

void main() {
  group('Dashboard Visual Freeze', () {
    testGoldens('Dashboard: Member Home should match baseline', (tester) async {
       // Mocking user profile for consistent dashboard layout
      final overrides = [
        userProfileProvider.overrideWith((ref) => {
          'fullName': 'Shalin Rahman',
          'membershipType': 'Life Member',
          'category': 'Active',
          'batch': '2010',
          'currentDesignation': 'Senior Software Engineer',
          'membershipId': 'L-4422'
        }),
      ];

      final builder = GoldenBuilder.grid(columns: 1, widthToHeightRatio: 0.5)
        ..addScenario(
          'Standard Dashboard',
          ProviderScope(
            overrides: overrides,
            child: const MaterialApp(
              debugShowCheckedModeBanner: false,
              home: DashboardScreen(),
            ),
          ),
        );

      await tester.pumpWidgetBuilder(builder.build());
      await screenMatchesGolden(tester, 'member_dashboard_standard');
    });
  });
}
