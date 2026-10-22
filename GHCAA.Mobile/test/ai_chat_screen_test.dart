// FR-42
// The assistant's name comes from org branding, not a hard-coded org name.
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:ghcaa_mobile/core/config/org_config.dart';
import 'package:ghcaa_mobile/core/services/org_config_service.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/features/assistant/assistant_service.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:ghcaa_mobile/features/theme/dynamic_theme_service.dart';
import 'package:ghcaa_mobile/screens/member/ai_chat_screen.dart';

class _FakeAssistantService implements AssistantService {
  @override
  Future<String?> ask(String question) async => 'Hello';
}

Future<void> _pump(WidgetTester tester, Map<String, dynamic> branding) async {
  await tester.pumpWidget(ProviderScope(
    overrides: [
      orgBrandingProvider.overrideWithValue(OrgBranding.fromJson(branding)),
      assistantServiceProvider.overrideWith((ref) => _FakeAssistantService()),
      roleProvider.overrideWith((ref) => Future.value('Member')),
      activeSpecialThemeProvider.overrideWith((ref) async => null),
    ],
    child: MaterialApp.router(
      theme: AppTheme.midnightTheme,
      routerConfig: GoRouter(routes: [
        GoRoute(path: '/', builder: (_, __) => const AIChatScreen()),
      ]),
    ),
  ));
  for (var i = 0; i < 5; i++) {
    await tester.pump(const Duration(milliseconds: 100));
  }
}

void main() {
  testWidgets('labels the assistant with the member nickname', (tester) async {
    await _pump(tester, {'shortName': 'EAA', 'memberNickname': 'Eagle'});

    expect(find.text('EAGLE AI'), findsOneWidget);
    expect(find.text('EAA AI Assistant'), findsWidgets);
    expect(find.textContaining('HARAGANGIAN'), findsNothing);
  });

  testWidgets('falls back to the short name when there is no nickname',
      (tester) async {
    await _pump(tester, {'shortName': 'EAA', 'memberNickname': ''});

    expect(find.text('EAA AI'), findsOneWidget);
  });

  testWidgets('reads plainly when the profile has no names', (tester) async {
    await _pump(tester, {'shortName': '', 'memberNickname': ''});

    expect(find.text('AI'), findsOneWidget);
    expect(find.text('AI Assistant'), findsWidgets);
  });
}
