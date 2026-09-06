import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';
import '../../core/constants/registration_constants.dart';
import 'lookup_service.dart';

final dropdownDataProvider = Provider<DropdownService>((ref) {
  return DropdownService(ref);
});

class DropdownService {
  final Ref _ref;
  DropdownService(this._ref);

  static const List<String> defaultSectors = [
    'Ready-made Garments (RMG)', 'Textiles & Spinning', 'Pharmaceuticals',
    'Banking & Financial Services', 'Information Technology (IT) & Software',
    'Telecommunications', 'Agriculture & Crop Production', 'Real Estate & Housing',
    'Healthcare & Medical Services', 'Education & Research', 'Public Administration & Defense'
  ];

  static const List<String> defaultSubjects = [
    'Science', 'Arts & Humanities', 'Business Studies', 'Bengali', 'English',
    'History', 'Economics', 'Political Science', 'Physics', 'Chemistry',
    'Mathematics', 'Accounting', 'Management', 'Marketing', 'Finance & Banking'
  ];

  static const List<String> defaultDegrees = [
    'HSC', 'Bachelor (Pass)', 'Bachelor (Honours)', 'Masters', 'PhD'
  ];

  static const List<String> defaultBloodGroups = [
    'Unknown', 'A+', 'A-', 'B+', 'B-', 'O+', 'O-', 'AB+', 'AB-'
  ];

  static const List<String> defaultMembershipTypes = [
    'Founding', 'Executive', 'General', 'Associate', 'Honorary', 'Advisory', 'Guest'
  ];

  static const List<String> defaultRelationshipTypes = [
    'Spouse', 'Parent', 'Child', 'Sibling', 'Other'
  ];

  static const List<String> defaultHSCSubjects = ['Science', 'Arts & Humanities', 'Business Studies'];
  static const List<String> defaultGeneralSubjects = [
    'Bengali', 'English', 'History', 'Philosophy', 'Economics', 'Political Science', 
    'Physics', 'Chemistry', 'Mathematics', 'Accounting', 'Management', 'Marketing', 
    'Finance & Banking', 'Computer', 'Civil', 'Mechanical', 'Electrical', 'Medical', 'Law'
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

  Future<List<Map<String, String>>> getOptions(String group) async {
    final apiLookups = await _ref.read(lookupsByGroupProvider(group).future);
    if (apiLookups.isNotEmpty) {
      return apiLookups.map((e) => {
        'value': e['value'].toString(),
        'label': e['label'].toString(),
      }).toList();
    }

    // Fallbacks
    switch (group) {
      case 'ProfessionalSector':
        return defaultSectors.map((s) => {'value': s, 'label': s}).toList();
      case 'Subject':
        return defaultSubjects.map((s) => {'value': s, 'label': s}).toList();
      case 'HSCSubject':
        return defaultHSCSubjects.map((s) => {'value': s, 'label': s}).toList();
      case 'GeneralSubject':
        return defaultGeneralSubjects.map((s) => {'value': s, 'label': s}).toList();
      case 'Degree':
        return defaultDegrees.map((s) => {'value': s, 'label': s}).toList();
      case LookupGroups.bloodGroup:
        return bloodGroupMap.entries.map((e) => {'value': e.key, 'label': e.value}).toList();
      case 'MembershipType':
        return defaultMembershipTypes.map((s) => {'value': s, 'label': s}).toList();
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
      case LookupGroups.memberCategory:
        return [
          {'value': 'None', 'label': 'None'},
          {'value': 'LifelongPatron', 'label': 'Lifelong Patron'},
          {'value': 'Sponsor', 'label': 'Sponsor'},
          {'value': 'Advisor', 'label': 'Advisor'},
          {'value': 'Mentor', 'label': 'Mentor'},
          {'value': 'Recruiter', 'label': 'Recruiter'},
          {'value': 'Active', 'label': 'Active Member'},
          {'value': 'Volunteer', 'label': 'Volunteer'},
          {'value': 'Contributor', 'label': 'Contributor'},
          {'value': 'Guest', 'label': 'Guest'},
          {'value': 'Student', 'label': 'Student'},
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
        final currentYear = DateTime.now().year;
        return List.generate(currentYear - 1950 + 1, (i) => (currentYear - i).toString())
            .map((y) => {'value': y, 'label': y}).toList();
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
