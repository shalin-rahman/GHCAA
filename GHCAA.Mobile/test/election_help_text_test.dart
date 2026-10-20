import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/constants/app_constants.dart';
import 'package:ghcaa_mobile/features/elections/election_service.dart';
import 'package:ghcaa_mobile/screens/admin/election_rules_unlock_sheet.dart';

class _NoUnlockService extends ElectionService {
  _NoUnlockService() : super(Dio());

  @override
  Future<ElectionRulesUnlock?> getRulesUnlock() async => null;
}

void main() {
  testWidgets('FR-039: the unlock form says what an unlock does and why it is short',
      (tester) async {
    await tester.pumpWidget(ProviderScope(
      overrides: [electionServiceProvider.overrideWith((ref) => _NoUnlockService())],
      child: const MaterialApp(home: Scaffold(body: ElectionRulesUnlockSheet())),
    ));
    await tester.tap(find.text('Check rules unlock'));
    await tester.pumpAndSettle();

    expect(find.text(ElectionConstants.unlockWhatHint), findsOneWidget);
    expect(find.text(ElectionConstants.unlockReasonHint), findsOneWidget);
    expect(find.text(ElectionConstants.unlockMinutesHint), findsOneWidget);
    expect(ElectionConstants.unlockMinutesHint, contains('30 minutes'));
    expect(ElectionConstants.unlockMinutesHint, contains('most is 60'));
  });
}
