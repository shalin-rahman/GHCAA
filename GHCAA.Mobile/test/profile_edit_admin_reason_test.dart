import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:ghcaa_mobile/features/admin/admin_service.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:ghcaa_mobile/features/files/file_service.dart';
import 'package:ghcaa_mobile/features/lookups/dropdown_service.dart';
import 'package:ghcaa_mobile/screens/member/profile_edit_screen.dart';

import 'helpers/fake_services.dart';

const _reason = 'The first academic record must be from the institution.';

class _EmptyDropdownService implements DropdownService {
  @override
  Future<List<Map<String, String>>> getOptions(String group) async => [];
  @override
  Future<List<Map<String, String>>> getSubjectOptions(String? degree) async =>
      [];
}

class _AdminAuthService extends FakeAuthService {
  @override
  Future<String?> getRole() async => 'Admin';
}

class _RefusingAdminService extends FakeAdminService {
  @override
  Future<bool> updateMember(int id, Map<String, dynamic> data) async {
    final options = RequestOptions(path: '/admin/members/$id');
    throw DioException(
      requestOptions: options,
      type: DioExceptionType.badResponse,
      response: Response(
          requestOptions: options, statusCode: 400, data: {'detail': _reason}),
    );
  }
}

void main() {
  testWidgets('admin edit shows the server reason for a refused update',
      (tester) async {
    tester.view.physicalSize = const Size(1080, 2400);
    tester.view.devicePixelRatio = 3.0;
    addTearDown(tester.view.reset);

    final router = GoRouter(routes: [
      GoRoute(
        path: '/',
        builder: (context, state) => const ProfileEditScreen(initialData: {
          'id': 42,
          'fullName': 'Edited Member',
          'mobileNo': '01700000000',
          'email': 'member@example.com',
          'nid': '1234567890',
        }),
      ),
    ]);

    await tester.pumpWidget(ProviderScope(
      overrides: [
        userProfileProvider.overrideWith((ref) => {'id': 1, 'fullName': 'Admin'}),
        authServiceProvider.overrideWith((ref) => _AdminAuthService()),
        adminServiceProvider.overrideWith((ref) => _RefusingAdminService()),
        dropdownDataProvider.overrideWith((ref) => _EmptyDropdownService()),
        fileServiceProvider.overrideWith((ref) => FakeFileService()),
      ],
      child: MaterialApp.router(routerConfig: router),
    ));
    await tester.pump();
    await tester.pump();

    final save = find.text('SAVE CHANGES');
    await tester.ensureVisible(save);
    await tester.tap(save);
    await tester.pump();
    await tester.pump(const Duration(milliseconds: 100));

    expect(find.text(_reason), findsOneWidget);
    expect(find.text('Registry Error: Protocol rejected.'), findsNothing);
  });
}
