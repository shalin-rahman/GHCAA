import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:ghcaa_mobile/core/config/org_config.dart';
import 'package:ghcaa_mobile/core/constants/registration_constants.dart';
import 'package:ghcaa_mobile/core/services/org_config_service.dart';
import 'package:ghcaa_mobile/core/storage/storage_service.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/core/widgets/app_dropdown_field.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:ghcaa_mobile/features/lookups/lookup_service.dart';
import 'package:ghcaa_mobile/features/lookups/member_labels.dart';
import 'package:ghcaa_mobile/features/networking/networking_service.dart';
import 'package:ghcaa_mobile/screens/member/directory_screen.dart';

import 'helpers/fake_services.dart';

class _RecordingLookupService extends LookupService {
  _RecordingLookupService() : super(Dio());

  final List<String> asked = [];

  @override
  Future<List<dynamic>> getByGroup(String group) async {
    asked.add(group);
    return [];
  }
}

class _OneMemberNetworking extends FakeNetworkingService {
  @override
  Future<Map<String, dynamic>> searchAlumni({String? query, String? batch, String? department, String? membershipType, String? category, String? cursor, int pageSize = 20}) async => {
        'items': [
          {'id': 1, 'fullName': 'Test Member', 'memberCategory': 6},
        ],
        'totalItems': 1,
      };
}

void main() {
  group('FR-05 (84.77): directory filters', () {
    late _RecordingLookupService lookups;

    Future<void> pumpDirectory(WidgetTester tester) async {
      tester.view.physicalSize = const Size(400, 900);
      tester.view.devicePixelRatio = 1;
      addTearDown(tester.view.reset);
      lookups = _RecordingLookupService();
      await tester.pumpWidget(ProviderScope(
        overrides: [
          lookupServiceProvider.overrideWithValue(lookups),
          localePackProvider.overrideWithValue(LocalePack.fromJson({
            'memberCategoryLabels': {'Active': 'Active'},
          })),
          networkingServiceProvider.overrideWith((ref) => _OneMemberNetworking()),
          storageServiceProvider.overrideWith((ref) => FakeStorageService()),
          authServiceProvider.overrideWith((ref) => FakeAuthService()),
          roleProvider.overrideWith((ref) => Future.value('Member')),
        ],
        child: MaterialApp.router(
          theme: AppTheme.midnightTheme,
          routerConfig: GoRouter(routes: [GoRoute(path: '/', builder: (context, state) => const DirectoryScreen())]),
        ),
      ));
      await tester.pump();
      await tester.pump();
    }

    testWidgets('DEPT asks for AcademicSubject, not the old Subject group', (tester) async {
      await pumpDirectory(tester);
      expect(lookups.asked, contains(LookupGroups.academicSubject));
      expect(lookups.asked, isNot(contains('Subject')));
    });

    testWidgets('CATEGORY lists all eleven categories with the enum key as value', (tester) async {
      await pumpDirectory(tester);
      final category = tester
          .widgetList<AppDropdownField<String?>>(find.byType(AppDropdownField<String?>))
          .singleWhere((d) => d.hintText == 'CATEGORY');
      final items = category.items.skip(1).toList();
      expect(items.map((i) => i.value), MemberEnumKeys.memberCategories);
      expect((items[6].child as Text).data, 'Active');
    });

    testWidgets('a member card shows the category label for an ordinal', (tester) async {
      await pumpDirectory(tester);
      expect(find.text('ACTIVE'), findsOneWidget);
    });
  });
}
