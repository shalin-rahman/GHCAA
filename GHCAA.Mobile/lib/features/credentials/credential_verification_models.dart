import '../../core/utils/app_utils.dart';

class CredentialVerification {
  final bool valid;
  final String? memberName;
  final String? membershipType;
  final DateTime? issuedOn;
  final String status;

  const CredentialVerification({
    required this.valid,
    this.memberName,
    this.membershipType,
    this.issuedOn,
    required this.status,
  });

  factory CredentialVerification.fromJson(Map<String, dynamic> json) {
    return CredentialVerification(
      valid: json['valid'] as bool? ?? false,
      memberName: json['memberName'] as String?,
      membershipType: json['membershipType'] as String?,
      issuedOn: AppUtils.parseDate(json['issuedOn'] as String?),
      status: json['status']?.toString() ?? '',
    );
  }
}
