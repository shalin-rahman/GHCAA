class AcademicConstants {
  static const List<String> certificates = [
    'HSC',
    'Bachelor (Pass)',
    'Bachelor (Honours)',
    'Masters',
    'PGD',
    'PhD',
    'Medicine',
    'Engineering',
    'Law'
  ];

  static const List<String> hscSubjects = [
    'Science',
    'Arts & Humanities',
    'Business Studies'
  ];

  static const List<String> generalSubjects = [
    'Bengali', 'English', 'History', 'Islamic History & Culture',
    'Philosophy', 'Islamic Studies', 'Library Science', 'Economics',
    'Political Science', 'Sociology', 'Social Work', 'Anthropology',
    'Public Administration', 'Physics', 'Chemistry', 'Mathematics',
    'Statistics', 'Botany', 'Zoology', 'Geography & Environment',
    'Psychology', 'Soil Science', 'Accounting', 'Management',
    'Marketing', 'Finance & Banking', 'Fine Arts', 'Physical Education',
    'Business Administration', 'Computer', 'Civil', 'Mechanical',
    'Electrical', 'Medical', 'Dentestry', 'Engineering', 'Law',
    'Pharma', 'Agriculture', 'Textile', 'Lather', 'Education'
  ];
}

// 82.42: group names for DropdownService.getOptions(group) — these used to be repeated as
// separate string literals in register_screen.dart, profile_edit_screen.dart and
// dropdown_service.dart's own switch, which is exactly the drift risk that let the API-side
// group name and the client's fallback switch case fall out of sync in the first place.
class LookupGroups {
  static const String membershipStatus = 'MembershipStatus';
  static const String userStatus = 'UserStatus'; // profile_edit_screen's admin-only status field uses this alias
  static const String memberCategory = 'MemberCategory';
  static const String gender = 'Gender';
  static const String bloodGroup = 'BloodGroup';
  static const String jobCategory = 'JobCategory';
  static const String passingYear = 'PassingYear';
}

class MembershipConstants {
  // 35.5 (closes 28.21): the registration tier list was deleted rather than completed with Guest.
  // Membership tiers are admin-assigned only, so no registration screen offers them; admin screens
  // read the tiers from the API via DropdownService.getOptions('MembershipType').

  static const List<Map<String, String>> bloodGroupOptions = [
    {'value': 'APositive', 'label': 'A+'},
    {'value': 'ANegative', 'label': 'A-'},
    {'value': 'BPositive', 'label': 'B+'},
    {'value': 'BNegative', 'label': 'B-'},
    {'value': 'OPositive', 'label': 'O+'},
    {'value': 'ONegative', 'label': 'O-'},
    {'value': 'ABPositive', 'label': 'AB+'},
    {'value': 'ABNegative', 'label': 'AB-'},
  ];
}
