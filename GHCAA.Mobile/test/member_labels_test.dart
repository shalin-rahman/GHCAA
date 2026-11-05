import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/config/org_config.dart';
import 'package:ghcaa_mobile/core/constants/registration_constants.dart';
import 'package:ghcaa_mobile/core/services/org_config_service.dart';
import 'package:ghcaa_mobile/features/lookups/dropdown_service.dart';
import 'package:ghcaa_mobile/features/lookups/lookup_service.dart';
import 'package:ghcaa_mobile/features/lookups/member_labels.dart';

class _GroupLookupService extends LookupService {
  _GroupLookupService([this.groups = const {}]) : super(Dio());

  final Map<String, List<dynamic>> groups;
  final List<String> asked = [];

  @override
  Future<List<dynamic>> getByGroup(String group) async {
    asked.add(group);
    return groups[group] ?? [];
  }
}

final _built = OrgConfig.offlineDefaults.locales['en']!;

// A server pack that renames one type, blanks one category and leaves the rest out.
LocalePack _partialPack() => LocalePack.fromJson({
      'membershipTypeLabels': {'Founding': 'Founder'},
      'memberCategoryLabels': {'Mentor': '  '},
    });

ProviderContainer _container({LocalePack? pack, _GroupLookupService? lookups}) {
  final container = ProviderContainer(overrides: [
    localePackProvider.overrideWithValue(pack ?? _partialPack()),
    lookupServiceProvider.overrideWithValue(lookups ?? _GroupLookupService()),
  ]);
  addTearDown(container.dispose);
  return container;
}

void main() {
  group('FR-07 (84.77): member type and category labels', () {
    final labels = MemberLabels.fromPack(_partialPack());

    test('a served label wins and a missing key keeps the build-time label', () {
      expect(labels.membershipType('Founding'), 'Founder');
      expect(labels.membershipType('General'), _built.membershipTypeLabels['General']);
    });

    test('a blank served label falls back to the build-time label', () {
      expect(labels.memberCategory('Mentor'), _built.memberCategoryLabels['Mentor']);
    });

    test('the Active category reads Active', () {
      expect(labels.memberCategory('Active'), 'Active');
    });

    test('an ordinal works as int or digit string', () {
      expect(labels.membershipType(0), 'Founder');
      expect(labels.membershipType('0'), 'Founder');
      expect(labels.memberCategory(6), 'Active');
    });

    test('names match without regard to case', () {
      expect(MemberLabels.toKey('lifelongpatron', MemberEnumKeys.memberCategories), 'LifelongPatron');
    });

    test('null, negative, out-of-range and unknown values give the default label', () {
      final general = labels.membershipType('General');
      final none = labels.memberCategory('None');
      for (final bad in <Object?>[null, -1, 7, '99', 'Platinum', '']) {
        expect(labels.membershipType(bad), general, reason: '$bad');
      }
      expect(labels.memberCategory(11), none);
      expect(MemberLabels.toKey(-1, MemberEnumKeys.memberCategories), isNull);
    });

    test('options keep the enum key as value and list every key in server order', () {
      expect(labels.membershipTypeOptions.map((o) => o['value']), MemberEnumKeys.membershipTypes);
      expect(labels.memberCategoryOptions, hasLength(11));
      expect(labels.memberCategoryOptions.first, {'value': 'None', 'label': labels.memberCategory('None')});
    });

    test('the provider follows the locale pack', () {
      final container = _container();
      expect(container.read(memberLabelsProvider).membershipType('Founding'), 'Founder');
    });
  });

  group('FR-01 (84.77): dropdown lists come from the server', () {
    test('a served group is returned as value and label strings', () async {
      final lookups = _GroupLookupService({
        LookupGroups.professionalSector: [
          {'value': 'IT', 'label': 'Information Technology'},
        ],
      });
      final options =
          await _container(lookups: lookups).read(dropdownDataProvider).getOptions(LookupGroups.professionalSector);
      expect(options, [
        {'value': 'IT', 'label': 'Information Technology'},
      ]);
    });

    test('an empty Degree, AcademicSubject or ProfessionalSector group has no client copy', () async {
      final service = _container().read(dropdownDataProvider);
      for (final group in [LookupGroups.degree, LookupGroups.academicSubject, LookupGroups.professionalSector]) {
        expect(await service.getOptions(group), isEmpty, reason: group);
      }
    });

    test('empty type and category groups fall back to the merged labels', () async {
      final service = _container().read(dropdownDataProvider);
      final types = await service.getOptions(LookupGroups.membershipType);
      final categories = await service.getOptions(LookupGroups.memberCategory);
      expect(types.first, {'value': 'Founding', 'label': 'Founder'});
      expect(categories, hasLength(11));
    });

    test('HSC lists its streams without asking the server', () async {
      final lookups = _GroupLookupService();
      final options =
          await _container(lookups: lookups).read(dropdownDataProvider).getSubjectOptions(AcademicConstants.hscDegree);
      expect(options.map((o) => o['value']), AcademicConstants.hscStreams);
      expect(lookups.asked, isEmpty);
    });

    test('any other degree asks for AcademicSubject', () async {
      final lookups = _GroupLookupService({
        LookupGroups.academicSubject: [
          {'value': 'Physics', 'label': 'Physics'},
        ],
      });
      final options = await _container(lookups: lookups).read(dropdownDataProvider).getSubjectOptions('BSc');
      expect(options.single['value'], 'Physics');
      expect(lookups.asked, [LookupGroups.academicSubject]);
    });
  });
}
