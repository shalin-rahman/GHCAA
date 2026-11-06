class AcademicConstants {
  static const int earliestPassingYear = 1950;
  static const int passingYearFutureYears = 6;
  // Bangladesh has no daylight saving, so a fixed offset gives the association's year.
  static const Duration associationUtcOffset = Duration(hours: 6);

  static const String hscDegree = 'HSC';

  // HSC has streams, not subjects. The server's AcademicGroup enum says "Humanities", so these stay
  // here as web keeps them in HSC_SUBJECT_GROUPS.
  static const List<String> hscStreams = ['Science', 'Arts & Humanities', 'Business Studies'];
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
  static const String degree = 'Degree';
  static const String academicSubject = 'AcademicSubject';
  static const String professionalSector = 'ProfessionalSector';
  static const String tShirtSize = 'TShirtSize';
  static const String membershipType = 'MembershipType';
  static const String discussionCategory = 'DiscussionCategory';
}

// Same rule as Constants.Nid on the API.
class MemberRules {
  static final RegExp nidPattern = RegExp(r'^(\d{10}|\d{13}|\d{17})$');
  static const String nidMessage = 'Enter 10, 13 or 17 digits.';
}

class MembershipConstants {
  // 35.5 (closes 28.21): the registration tier list was deleted rather than completed with Guest.
  // Membership tiers are admin-assigned only, so no registration screen offers them; admin screens
  // read the tiers through DropdownService.getOptions(LookupGroups.membershipType), which falls back to the OrgConfig labels.

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
