import '../../core/utils/app_utils.dart';

class ScholarshipFund {
  final int id;
  final String name;
  final String description;
  final String? namedAfter;
  final double targetAmount;
  final bool isActive;

  const ScholarshipFund({
    required this.id,
    required this.name,
    required this.description,
    this.namedAfter,
    required this.targetAmount,
    required this.isActive,
  });

  factory ScholarshipFund.fromJson(Map<String, dynamic> json) =>
      ScholarshipFund(
        id: json['id'] as int,
        name: json['name'] as String? ?? '',
        description: json['description'] as String? ?? '',
        namedAfter: json['namedAfter'] as String?,
        targetAmount: (json['targetAmount'] as num?)?.toDouble() ?? 0,
        isActive: json['isActive'] as bool? ?? false,
      );
}

class ScholarshipCall {
  final int id;
  final int scholarshipFundId;
  final String academicYear;
  final DateTime? opensOn;
  final DateTime? closesOn;
  final int slotCount;
  final double awardAmount;
  final String eligibilityCriteria;
  final bool isActive;

  const ScholarshipCall({
    required this.id,
    required this.scholarshipFundId,
    required this.academicYear,
    this.opensOn,
    this.closesOn,
    required this.slotCount,
    required this.awardAmount,
    required this.eligibilityCriteria,
    required this.isActive,
  });

  factory ScholarshipCall.fromJson(Map<String, dynamic> json) =>
      ScholarshipCall(
        id: json['id'] as int,
        scholarshipFundId: json['scholarshipFundId'] as int,
        academicYear: json['academicYear'] as String? ?? '',
        opensOn: AppUtils.parseDate(json['opensOn'] as String?),
        closesOn: AppUtils.parseDate(json['closesOn'] as String?),
        slotCount: json['slotCount'] as int? ?? 0,
        awardAmount: (json['awardAmount'] as num?)?.toDouble() ?? 0,
        eligibilityCriteria: json['eligibilityCriteria'] as String? ?? '',
        isActive: json['isActive'] as bool? ?? false,
      );
}

class CreateScholarshipApplication {
  final String applicantName;
  final String applicantEmail;
  final String applicantPhone;
  final String institutionName;
  final String className;
  final String guardianName;
  final double householdIncome;
  final String needStatement;
  final String meritStatement;

  const CreateScholarshipApplication({
    required this.applicantName,
    required this.applicantEmail,
    required this.applicantPhone,
    required this.institutionName,
    required this.className,
    required this.guardianName,
    required this.householdIncome,
    required this.needStatement,
    required this.meritStatement,
  });

  Map<String, dynamic> toJson() => {
        'applicantName': applicantName,
        'applicantEmail': applicantEmail,
        'applicantPhone': applicantPhone,
        'institutionName': institutionName,
        'class': className,
        'guardianName': guardianName,
        'householdIncome': householdIncome,
        'needStatement': needStatement,
        'meritStatement': meritStatement,
      };
}

class ScholarshipApplication {
  final int id;
  final int scholarshipCallId;
  final String applicantName;
  final String applicantEmail;
  final String applicantPhone;
  final String institutionName;
  final String className;
  final String guardianName;
  final double householdIncome;
  final String needStatement;
  final String meritStatement;
  final String status;
  final DateTime? submittedAt;
  final String referenceCode;

  const ScholarshipApplication({
    required this.id,
    required this.scholarshipCallId,
    required this.applicantName,
    required this.applicantEmail,
    required this.applicantPhone,
    required this.institutionName,
    required this.className,
    required this.guardianName,
    required this.householdIncome,
    required this.needStatement,
    required this.meritStatement,
    required this.status,
    this.submittedAt,
    required this.referenceCode,
  });

  factory ScholarshipApplication.fromJson(Map<String, dynamic> json) =>
      ScholarshipApplication(
        id: json['id'] as int,
        scholarshipCallId: json['scholarshipCallId'] as int,
        applicantName: json['applicantName'] as String? ?? '',
        applicantEmail: json['applicantEmail'] as String? ?? '',
        applicantPhone: json['applicantPhone'] as String? ?? '',
        institutionName: json['institutionName'] as String? ?? '',
        className: json['class'] as String? ?? '',
        guardianName: json['guardianName'] as String? ?? '',
        householdIncome: (json['householdIncome'] as num?)?.toDouble() ?? 0,
        needStatement: json['needStatement'] as String? ?? '',
        meritStatement: json['meritStatement'] as String? ?? '',
        status: json['status']?.toString() ?? '',
        submittedAt: AppUtils.parseDate(json['submittedAt'] as String?),
        referenceCode: json['referenceCode'] as String? ?? '',
      );
}

class ScholarshipStatus {
  final String referenceCode;
  final String status;
  final String academicYear;
  final DateTime? submittedAt;
  final double? awardAmount;
  final String? disbursementStatus;

  const ScholarshipStatus({
    required this.referenceCode,
    required this.status,
    required this.academicYear,
    this.submittedAt,
    this.awardAmount,
    this.disbursementStatus,
  });

  factory ScholarshipStatus.fromJson(Map<String, dynamic> json) =>
      ScholarshipStatus(
        referenceCode: json['referenceCode'] as String? ?? '',
        status: json['status']?.toString() ?? '',
        academicYear: json['academicYear'] as String? ?? '',
        submittedAt: AppUtils.parseDate(json['submittedAt'] as String?),
        awardAmount: (json['awardAmount'] as num?)?.toDouble(),
        disbursementStatus: json['disbursementStatus']?.toString(),
      );
}
