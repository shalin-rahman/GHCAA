/// Typed shape for GET /profile. Required fields below mirror what GHCAA.Domain's
/// Member/AcademicRecord/ProfessionalRecord mark [Required] (non-nullable columns) —
/// they are never actually missing from a healthy response, so a missing one here
/// means the API contract drifted, and this should throw rather than default.
///
/// `raw` keeps the full untouched map: profile_edit_screen.dart edits dozens of
/// admin/dynamic fields (membershipType, category, notifyEventCreation, etc.) that
/// aren't worth modeling one by one, so it keeps working off the map. Parsing the
/// response through this class at the fetch boundary is what makes a schema drift
/// fail loudly, even though most callers still read the map.
class AcademicRecord {
  final String institutionName;
  final String degree;
  final String subject;
  final int passingYear;
  final bool isGHC;
  final Map<String, dynamic> raw;

  AcademicRecord({
    required this.institutionName,
    required this.degree,
    required this.subject,
    required this.passingYear,
    this.isGHC = false,
    required this.raw,
  });

  factory AcademicRecord.fromJson(Map<String, dynamic> json) {
    return AcademicRecord(
      institutionName: json['institutionName'] as String,
      degree: json['degree'] as String,
      subject: json['subject'] as String,
      passingYear: json['passingYear'] as int,
      isGHC: json['isGHC'] as bool? ?? false,
      raw: json,
    );
  }
}

class ProfessionalRecord {
  final String organizationName;
  final String designation;
  final String? sector;
  final String? location;
  final bool isCurrent;
  final Map<String, dynamic> raw;

  ProfessionalRecord({
    required this.organizationName,
    required this.designation,
    this.sector,
    this.location,
    this.isCurrent = false,
    required this.raw,
  });

  factory ProfessionalRecord.fromJson(Map<String, dynamic> json) {
    return ProfessionalRecord(
      organizationName: json['organizationName'] as String,
      designation: json['designation'] as String,
      sector: json['sector'] as String?,
      location: json['location'] as String?,
      isCurrent: json['isCurrent'] as bool? ?? false,
      raw: json,
    );
  }
}

class MemberProfile {
  final int id;
  final String fullName;
  final String email;
  final String mobileNo;
  final List<AcademicRecord> academicHistory;
  final List<ProfessionalRecord> professionalHistory;
  final Map<String, dynamic> raw;

  MemberProfile({
    required this.id,
    required this.fullName,
    required this.email,
    required this.mobileNo,
    required this.academicHistory,
    required this.professionalHistory,
    required this.raw,
  });

  factory MemberProfile.fromJson(Map<String, dynamic> json) {
    final academicJson = json['academicHistory'] as List? ?? const [];
    final professionalJson = json['professionalHistory'] as List? ?? const [];
    return MemberProfile(
      id: json['id'] as int,
      fullName: json['fullName'] as String,
      email: json['email'] as String,
      mobileNo: json['mobileNo'] as String,
      academicHistory: academicJson
          .map((e) => AcademicRecord.fromJson(Map<String, dynamic>.from(e as Map)))
          .toList(),
      professionalHistory: professionalJson
          .map((e) => ProfessionalRecord.fromJson(Map<String, dynamic>.from(e as Map)))
          .toList(),
      raw: json,
    );
  }
}
