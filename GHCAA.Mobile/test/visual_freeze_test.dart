import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:golden_toolkit/golden_toolkit.dart';
import 'package:ghcaa_mobile/screens/member/digital_id_screen.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

void main() {
  group('Digital ID Visual Freeze', () {
    testGoldens('Identity: Member Digital ID Card should match baseline', (tester) async {
      final builder = GoldenBuilder.grid(columns: 1, widthToHeightRatio: 0.6)
        ..addScenario(
          'Portrait View',
          const ProviderScope(
            child: MaterialApp(
              debugShowCheckedModeBanner: false,
              home: DigitalIDScreen(),
            ),
          ),
        );

      await tester.pumpWidgetBuilder(builder.build());
      await screenMatchesGolden(tester, 'digital_id_card_portrait');
    });
  });
}
