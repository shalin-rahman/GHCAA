import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/config/org_config.dart';
import '../../core/services/org_config_service.dart';

// Enum order matches the server, so a list index is the ordinal the API may send.
class MemberEnumKeys {
  static const List<String> membershipTypes = [
    'Founding', 'Executive', 'General', 'Associate', 'Honorary', 'Advisory', 'Guest',
  ];
  static const List<String> memberCategories = [
    'None', 'LifelongPatron', 'Sponsor', 'Advisor', 'Mentor', 'Recruiter',
    'Active', 'Volunteer', 'Contributor', 'Guest', 'Student',
  ];
  static const String defaultMembershipType = 'General';
  static const String defaultMemberCategory = 'None';
}

/// Membership type and member category labels from OrgConfig. A key the server pack leaves out
/// keeps the build-time label, so a partial pack never shows a raw key.
class MemberLabels {
  final Map<String, String> membershipTypes;
  final Map<String, String> memberCategories;

  const MemberLabels._(this.membershipTypes, this.memberCategories);

  factory MemberLabels.fromPack(LocalePack pack) {
    final fallback = OrgConfig.offlineDefaults.locales['en']!;
    return MemberLabels._(
      _merge(MemberEnumKeys.membershipTypes, fallback.membershipTypeLabels, pack.membershipTypeLabels),
      _merge(MemberEnumKeys.memberCategories, fallback.memberCategoryLabels, pack.memberCategoryLabels),
    );
  }

  static Map<String, String> _merge(List<String> keys, Map<String, String> base, Map<String, String> served) {
    return {
      for (final key in keys)
        key: _nonBlank(served[key]) ?? _nonBlank(base[key]) ?? key,
    };
  }

  static String? _nonBlank(String? s) => (s == null || s.trim().isEmpty) ? null : s;

  /// Turns an ordinal (int or digit string) or a name into the enum key, or null when it matches nothing.
  static String? toKey(Object? value, List<String> keys) {
    if (value == null) return null;
    final text = value.toString().trim();
    final ordinal = value is int ? value : int.tryParse(text);
    if (ordinal != null) {
      return ordinal >= 0 && ordinal < keys.length ? keys[ordinal] : null;
    }
    for (final key in keys) {
      if (key.toLowerCase() == text.toLowerCase()) return key;
    }
    return null;
  }

  String membershipType(Object? value) {
    final key = toKey(value, MemberEnumKeys.membershipTypes) ?? MemberEnumKeys.defaultMembershipType;
    return membershipTypes[key]!;
  }

  String memberCategory(Object? value) {
    final key = toKey(value, MemberEnumKeys.memberCategories) ?? MemberEnumKeys.defaultMemberCategory;
    return memberCategories[key]!;
  }

  List<Map<String, String>> get membershipTypeOptions => _options(membershipTypes);
  List<Map<String, String>> get memberCategoryOptions => _options(memberCategories);

  static List<Map<String, String>> _options(Map<String, String> labels) =>
      labels.entries.map((e) => {'value': e.key, 'label': e.value}).toList();
}

final memberLabelsProvider = Provider<MemberLabels>((ref) {
  return MemberLabels.fromPack(ref.watch(localePackProvider));
});
