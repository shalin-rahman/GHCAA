import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/features/admin/roles_service.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:ghcaa_mobile/features/theme/dynamic_theme_service.dart';
import 'package:ghcaa_mobile/screens/admin/permissions_matrix_screen.dart';

import 'helpers/fake_services.dart';

class _RolesService extends FakeRolesService {
  _RolesService(this.users);

  final List<dynamic> users;
  final unlocked = <int>[];
  String? unlockError;

  @override
  Future<List<dynamic>> getUsers() async => users;

  @override
  Future<String?> unlockUser(int userId) async {
    unlocked.add(userId);
    return unlockError;
  }
}

// FR-39
void main() {
  setUpAll(() => dotenv.testLoad(fileInput: 'PORTAL_TITLE=GHCAA'));

  final future = DateTime.now().add(const Duration(minutes: 10)).toUtc().toIso8601String();
  final past = DateTime.now().subtract(const Duration(minutes: 1)).toUtc().toIso8601String();

  Future<void> pump(WidgetTester tester, _RolesService service) async {
    tester.view.physicalSize = const Size(390, 1400);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.reset);
    await tester.pumpWidget(ProviderScope(
      overrides: [
        rolesServiceProvider.overrideWith((ref) => service),
        roleProvider.overrideWith((ref) => Future.value('SuperAdmin')),
        isAdminProvider.overrideWith((ref) async => true),
        isElectionStaffProvider.overrideWith((ref) async => true),
        activeSpecialThemeProvider.overrideWith((ref) async => null),
      ],
      child: MaterialApp.router(
        theme: AppTheme.midnightTheme,
        routerConfig: GoRouter(routes: [
          GoRoute(path: '/', builder: (_, __) => const PermissionsMatrixScreen()),
        ]),
      ),
    ));
    for (var i = 0; i < 20; i++) {
      await tester.pump(const Duration(milliseconds: 50));
    }
  }

  testWidgets('FR-39: unlock shows for a locked user and calls the service',
      (tester) async {
    final service = _RolesService([
      {'id': 7, 'username': 'ops', 'roles': <String>['Admin'], 'isActive': true, 'lockedUntil': future},
    ]);
    await pump(tester, service);

    await tester.tap(find.byKey(const ValueKey('unlock-user-7')));
    await tester.pump(const Duration(milliseconds: 300));
    await tester.tap(find.text('UNLOCK'));
    await tester.pump(const Duration(milliseconds: 300));

    expect(service.unlocked, [7]);
    expect(find.text('Account unlocked.'), findsOneWidget);
  });

  testWidgets('FR-39: unlock stays hidden when no lock is running',
      (tester) async {
    final service = _RolesService([
      {'id': 7, 'username': 'ops', 'roles': <String>['Admin'], 'isActive': true, 'lockedUntil': past},
      {'id': 8, 'username': 'clerk', 'roles': <String>['Admin'], 'isActive': true},
    ]);
    await pump(tester, service);

    expect(find.text('ops'), findsOneWidget);
    expect(find.byKey(const ValueKey('unlock-user-7')), findsNothing);
    expect(find.byKey(const ValueKey('unlock-user-8')), findsNothing);
  });

  testWidgets('FR-39: a failed unlock shows the server reason', (tester) async {
    final service = _RolesService([
      {'id': 7, 'username': 'ops', 'roles': <String>['Admin'], 'isActive': true, 'lockedUntil': future},
    ])..unlockError = 'User not found.';
    await pump(tester, service);

    await tester.tap(find.byKey(const ValueKey('unlock-user-7')));
    await tester.pump(const Duration(milliseconds: 300));
    await tester.tap(find.text('UNLOCK'));
    await tester.pump(const Duration(milliseconds: 300));

    expect(find.text('User not found.'), findsOneWidget);
  });
}
