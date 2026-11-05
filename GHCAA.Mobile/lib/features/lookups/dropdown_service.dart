import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';
import '../../core/constants/registration_constants.dart';
import 'lookup_service.dart';
import 'member_labels.dart';

final dropdownDataProvider = Provider<DropdownService>((ref) {
  return DropdownService(ref);
});

class DropdownService {
  final Ref _ref;
  DropdownService(this._ref);

  static const List<String> defaultBloodGroups = [
    'Unknown', 'A+', 'A-', 'B+', 'B-', 'O+', 'O-', 'AB+', 'AB-'
  ];

  static const List<String> defaultRelationshipTypes = [
    'Spouse', 'Parent', 'Child', 'Sibling', 'Other'
  ];

  static const List<Map<String, String>> defaultArticleCategories = [
    {'value': 'Event', 'label': 'Event Highlights'},
    {'value': 'Magazine', 'label': 'E-Magazine Article'},
    {'value': 'Regular', 'label': 'Regular Portal Update'},
  ];

  static const Map<String, String> bloodGroupMap = {
    'Unknown': 'Not Specified',
    'APositive': 'A+', 'ANegative': 'A-', 'BPositive': 'B+', 'BNegative': 'B-',
    'OPositive': 'O+', 'ONegative': 'O-', 'ABPositive': 'AB+', 'ABNegative': 'AB-'
  };

  static const Map<String, String> jobCategoryMap = {
    'IT': 'IT & Software Development',
    'Finance': 'Finance & Banking',
    'Engineering': 'Engineering & Construction',
    'Marketing': 'Marketing & Sales',
    'Education': 'Education & Research',
    'Health': 'Healthcare & Pharma',
    'PublicSector': 'Govt. & Public Sector',
    'Mentorship': 'Mentorship & Career Guidance',
    'Other': 'Other Opportunities'
  };

  /// HSC lists its streams from the client; every other degree reads the AcademicSubject group.
  Future<List<Map<String, String>>> getSubjectOptions(String? degree) async {
    if (degree == AcademicConstants.hscDegree) {
      return AcademicConstants.hscStreams.map((s) => {'value': s, 'label': s}).toList();
    }
    return getOptions(LookupGroups.academicSubject);
  }

  Future<List<Map<String, String>>> getOptions(String group) async {
    final apiLookups = await _ref.read(lookupsByGroupProvider(group).future);
    if (apiLookups.isNotEmpty) {
      return apiLookups.map((e) => {
        'value': e['value'].toString(),
        'label': e['label'].toString(),
      }).toList();
    }

    // Degree, AcademicSubject and ProfessionalSector have no client copy: the server owns them.
    switch (group) {
      case LookupGroups.bloodGroup:
        return bloodGroupMap.entries.map((e) => {'value': e.key, 'label': e.value}).toList();
      case LookupGroups.membershipType:
        return _ref.read(memberLabelsProvider).membershipTypeOptions;
      case LookupGroups.memberCategory:
        return _ref.read(memberLabelsProvider).memberCategoryOptions;
      case 'RelationshipType':
        return defaultRelationshipTypes.map((s) => {'value': s, 'label': s}).toList();
      case 'ArticleCategory':
        return defaultArticleCategories;
      case LookupGroups.jobCategory:
        return jobCategoryMap.entries.map((e) => {'value': e.key, 'label': e.value}).toList();
      case LookupGroups.gender:
        return [
          {'value': 'None', 'label': 'Not Specified'},
          {'value': 'Male', 'label': 'Male'},
          {'value': 'Female', 'label': 'Female'},
          {'value': 'Other', 'label': 'Other'},
        ];
      case LookupGroups.userStatus:
      case LookupGroups.membershipStatus:
        return [
          {'value': 'Applied', 'label': 'Pending Approval'},
          {'value': 'Active', 'label': 'Active Member'},
          {'value': 'InactivePayment', 'label': 'Inactive (Unpaid)'},
          {'value': 'InactiveResigned', 'label': 'Resigned'},
          {'value': 'Terminated', 'label': 'Terminated'},
        ];
      case LookupGroups.passingYear:
        return passingYearFallback().map((y) => {'value': y, 'label': y}).toList();
      case 'PaymentMethod':
        try {
          final dio = _ref.read(dioProvider);
          final response = await dio.get('/payment-config/active');
          if (response.statusCode == 200 && response.data is List) {
            return (response.data as List).map((e) => {
              'value': e['id'].toString(),
              'label': e['displayName'].toString(),
              'instructions': e['instructions']?.toString() ?? '',
              'requiresReceipt': e['requiresReceipt'].toString(),
              'requiresReference': e['requiresReference'].toString(),
            }).toList();
          }
        } catch (e) {
          debugPrint('DropdownService.getOptions failed: $e');
        }
        return [];
      default:
        return [];
    }
  }
}
