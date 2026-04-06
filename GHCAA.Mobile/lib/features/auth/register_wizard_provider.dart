import 'package:flutter_riverpod/flutter_riverpod.dart';

// ── Typed model ─────────────────────────────────────────────────────────────
class RegisterModel {
  final String fullName;
  final String email;
  final String mobileNo;
  final String nid;
  final String passingYear;
  final String presentAddress;
  final String permanentAddress;
  final String fatherName;
  final String motherName;
  final String emergencyContactName;
  final String emergencyContactRelation;
  final String emergencyContactPhone;
  final String? tShirtSize;
  final List<Map<String, dynamic>> academicHistory;
  final String? profileImagePath;
  final String? nidPhotoPath;
  final String? paymentProofPath;
  final String membershipType;
  final String bloodGroup;
  final String? dateOfBirth;
  final String? degree;
  final String? subject;
  final String? designation;
  final bool hasAcceptedTerms;
  final bool hasAcceptedGdpr;
  final bool notifyEventCreation;
  final bool notifyParticipationApproval;
  final bool notifyRegistrationUpdate;
  final bool notifyRelevantUpdates;
  final int paymentMethodId;
  final String transactionId;
  final String institutionName;

  const RegisterModel({
    this.fullName = '',
    this.email = '',
    this.mobileNo = '',
    this.nid = '',
    this.passingYear = '',
    this.presentAddress = '',
    this.permanentAddress = '',
    this.fatherName = '',
    this.motherName = '',
    this.emergencyContactName = '',
    this.emergencyContactRelation = '',
    this.emergencyContactPhone = '',
    this.tShirtSize = 'M',
    this.academicHistory = const [],
    this.profileImagePath,
    this.nidPhotoPath,
    this.paymentProofPath,
    this.membershipType = 'General',
    this.bloodGroup = 'APositive',
    this.dateOfBirth,
    this.degree = 'HSC',
    this.subject,
    this.designation = '',
    this.hasAcceptedTerms = false,
    this.hasAcceptedGdpr = false,
    this.notifyEventCreation = true,
    this.notifyParticipationApproval = true,
    this.notifyRegistrationUpdate = true,
    this.notifyRelevantUpdates = true,
    this.paymentMethodId = 0,
    this.transactionId = '',
    this.institutionName = 'Govt. Haraganga College',
  });

  RegisterModel copyWith({
    String? fullName,
    String? email,
    String? mobileNo,
    String? nid,
    String? passingYear,
    String? presentAddress,
    String? permanentAddress,
    String? fatherName,
    String? motherName,
    String? emergencyContactName,
    String? emergencyContactRelation,
    String? emergencyContactPhone,
    String? tShirtSize,
    List<Map<String, dynamic>>? academicHistory,
    String? profileImagePath,
    String? nidPhotoPath,
    String? paymentProofPath,
    String? membershipType,
    String? bloodGroup,
    String? dateOfBirth,
    String? degree,
    String? subject,
    String? designation,
    bool? hasAcceptedTerms,
    bool? hasAcceptedGdpr,
    bool? notifyEventCreation,
    bool? notifyParticipationApproval,
    bool? notifyRegistrationUpdate,
    bool? notifyRelevantUpdates,
    int? paymentMethodId,
    String? transactionId,
    String? institutionName,
  }) {
    return RegisterModel(
      fullName: fullName ?? this.fullName,
      email: email ?? this.email,
      mobileNo: mobileNo ?? this.mobileNo,
      nid: nid ?? this.nid,
      passingYear: passingYear ?? this.passingYear,
      presentAddress: presentAddress ?? this.presentAddress,
      permanentAddress: permanentAddress ?? this.permanentAddress,
      fatherName: fatherName ?? this.fatherName,
      motherName: motherName ?? this.motherName,
      emergencyContactName: emergencyContactName ?? this.emergencyContactName,
      emergencyContactRelation: emergencyContactRelation ?? this.emergencyContactRelation,
      emergencyContactPhone: emergencyContactPhone ?? this.emergencyContactPhone,
      tShirtSize: tShirtSize ?? this.tShirtSize,
      academicHistory: academicHistory ?? this.academicHistory,
      profileImagePath: profileImagePath ?? this.profileImagePath,
      nidPhotoPath: nidPhotoPath ?? this.nidPhotoPath,
      paymentProofPath: paymentProofPath ?? this.paymentProofPath,
      membershipType: membershipType ?? this.membershipType,
      bloodGroup: bloodGroup ?? this.bloodGroup,
      dateOfBirth: dateOfBirth ?? this.dateOfBirth,
      degree: degree ?? this.degree,
      subject: subject ?? this.subject,
      designation: designation ?? this.designation,
      hasAcceptedTerms: hasAcceptedTerms ?? this.hasAcceptedTerms,
      hasAcceptedGdpr: hasAcceptedGdpr ?? this.hasAcceptedGdpr,
      notifyEventCreation: notifyEventCreation ?? this.notifyEventCreation,
      notifyParticipationApproval: notifyParticipationApproval ?? this.notifyParticipationApproval,
      notifyRegistrationUpdate: notifyRegistrationUpdate ?? this.notifyRegistrationUpdate,
      notifyRelevantUpdates: notifyRelevantUpdates ?? this.notifyRelevantUpdates,
      paymentMethodId: paymentMethodId ?? this.paymentMethodId,
      transactionId: transactionId ?? this.transactionId,
      institutionName: institutionName ?? this.institutionName,
    );
  }

  Map<String, dynamic> toJson() => {
        'fullName': fullName,
        'email': email,
        'mobileNo': mobileNo,
        'nid': nid,
        'fatherName': fatherName,
        'motherName': motherName,
        'emergencyContactName': emergencyContactName,
        'emergencyContactRelation': emergencyContactRelation,
        'emergencyContactPhone': emergencyContactPhone,
        'tShirtSize': tShirtSize,
        'passingYear': int.tryParse(passingYear),
        'presentAddress': presentAddress,
        'permanentAddress': permanentAddress,
        'academicHistory': academicHistory.isNotEmpty 
            ? academicHistory 
            : [
                {
                  'InstitutionName': institutionName,
                  'Degree': degree,
                  'Subject': subject,
                  'PassingYear': int.tryParse(passingYear),
                  'IsGHC': true,
                }
              ],
        'membershipType': membershipType,
        'bloodGroup': bloodGroup,
        'dateOfBirth': dateOfBirth,
        'degree': degree,
        'subject': subject,
        'designation': designation,
        'profileImagePath': profileImagePath,
        'nidPhotoPath': nidPhotoPath,
        'paymentProofPath': paymentProofPath,
        'notifyEventCreation': notifyEventCreation,
        'notifyParticipationApproval': notifyParticipationApproval,
        'notifyRegistrationUpdate': notifyRegistrationUpdate,
        'notifyRelevantUpdates': notifyRelevantUpdates,
        'hasAcceptedTerms': hasAcceptedTerms,
        'hasAcceptedGdpr': hasAcceptedGdpr,
        'paymentMethodId': paymentMethodId,
        'transactionId': transactionId,
      };
}

class RegisterState {
  final int currentStep;
  final RegisterModel model;

  const RegisterState({
    this.currentStep = 0,
    this.model = const RegisterModel(),
  });

  bool get isLastStep => currentStep == 2;
  int get step => currentStep + 1;

  Map<String, dynamic> get data => {
        'FullName': model.fullName,
        'Email': model.email,
        'MobileNo': model.mobileNo,
        'NID': model.nid,
        'FatherName': model.fatherName,
        'MotherName': model.motherName,
        'EmergencyContactName': model.emergencyContactName,
        'EmergencyContactRelation': model.emergencyContactRelation,
        'EmergencyContactPhone': model.emergencyContactPhone,
        'TShirtSize': model.tShirtSize,
        'PassingYear': model.passingYear.isEmpty ? null : int.tryParse(model.passingYear),
        'PresentAddress': model.presentAddress,
        'PermanentAddress': model.permanentAddress,
        'Degree': model.degree,
        'Subject': model.subject,
        'Designation': model.designation,
        'BloodGroup': model.bloodGroup,
        'MembershipType': model.membershipType,
        'HasAcceptedTerms': model.hasAcceptedTerms,
        'HasAcceptedGdpr': model.hasAcceptedGdpr,
        'DateOfBirth': model.dateOfBirth,
        'ProfileImagePath': model.profileImagePath,
        'NidPhotoPath': model.nidPhotoPath,
        'PaymentProofPath': model.paymentProofPath,
        'NotifyEventCreation': model.notifyEventCreation,
        'NotifyParticipationApproval': model.notifyParticipationApproval,
        'NotifyRegistrationUpdate': model.notifyRegistrationUpdate,
        'NotifyRelevantUpdates': model.notifyRelevantUpdates,
        'PaymentMethodId': model.paymentMethodId,
        'TransactionId': model.transactionId,
        'InstitutionName': model.institutionName,
        'AcademicHistory': model.academicHistory.isNotEmpty 
            ? model.academicHistory 
            : [
                {
                  'InstitutionName': model.institutionName,
                  'Degree': model.degree,
                  'Subject': model.subject,
                  'PassingYear': model.passingYear.isEmpty ? null : int.tryParse(model.passingYear),
                  'IsGHC': true,
                }
              ],
      };

  RegisterState copyWith({int? currentStep, RegisterModel? model}) {
    return RegisterState(
      currentStep: currentStep ?? this.currentStep,
      model: model ?? this.model,
    );
  }
}

class RegisterWizardNotifier extends StateNotifier<RegisterState> {
  static const int _totalSteps = 3;

  RegisterWizardNotifier() : super(const RegisterState());

  void nextPage() {
    if (state.currentStep < _totalSteps - 1) {
      state = state.copyWith(currentStep: state.currentStep + 1);
    }
  }

  void prevPage() {
    if (state.currentStep > 0) {
      state = state.copyWith(currentStep: state.currentStep - 1);
    }
  }

  void nextStep() => nextPage();
  void prevStep() => prevPage();

  void updateModel({
    String? fullName,
    String? email,
    String? mobileNo,
    String? nid,
    String? passingYear,
    String? presentAddress,
    String? permanentAddress,
    String? fatherName,
    String? motherName,
    String? emergencyContactName,
    String? emergencyContactRelation,
    String? emergencyContactPhone,
    String? tShirtSize,
    List<Map<String, dynamic>>? academicHistory,
    String? profileImagePath,
    String? nidPhotoPath,
    String? paymentProofPath,
    String? membershipType,
    String? bloodGroup,
    String? dateOfBirth,
    String? degree,
    String? subject,
    String? designation,
    bool? hasAcceptedTerms,
    bool? hasAcceptedGdpr,
    bool? notifyEventCreation,
    bool? notifyParticipationApproval,
    bool? notifyRegistrationUpdate,
    bool? notifyRelevantUpdates,
    int? paymentMethodId,
    String? transactionId,
    String? institutionName,
  }) {
    state = state.copyWith(
      model: state.model.copyWith(
        fullName: fullName,
        email: email,
        mobileNo: mobileNo,
        nid: nid,
        passingYear: passingYear,
        presentAddress: presentAddress,
        permanentAddress: permanentAddress,
        fatherName: fatherName,
        motherName: motherName,
        emergencyContactName: emergencyContactName,
        emergencyContactRelation: emergencyContactRelation,
        emergencyContactPhone: emergencyContactPhone,
        tShirtSize: tShirtSize,
        academicHistory: academicHistory,
        profileImagePath: profileImagePath,
        nidPhotoPath: nidPhotoPath,
        paymentProofPath: paymentProofPath,
        membershipType: membershipType,
        bloodGroup: bloodGroup,
        dateOfBirth: dateOfBirth,
        degree: degree,
        subject: subject,
        designation: designation,
        hasAcceptedTerms: hasAcceptedTerms,
        hasAcceptedGdpr: hasAcceptedGdpr,
        notifyEventCreation: notifyEventCreation,
        notifyParticipationApproval: notifyParticipationApproval,
        notifyRegistrationUpdate: notifyRegistrationUpdate,
        notifyRelevantUpdates: notifyRelevantUpdates,
        paymentMethodId: paymentMethodId,
        transactionId: transactionId,
        institutionName: institutionName,
      ),
    );
  }

  void updateData(String key, dynamic value) {
    switch (key) {
      case 'FullName':
        updateModel(fullName: value as String);
        break;
      case 'Email':
        updateModel(email: value as String);
        break;
      case 'MobileNo':
        updateModel(mobileNo: value as String);
        break;
      case 'NID':
        updateModel(nid: value as String);
        break;
      case 'PassingYear':
        updateModel(passingYear: value?.toString());
        break;
      case 'PresentAddress':
        updateModel(presentAddress: value as String);
        break;
      case 'PermanentAddress':
        updateModel(permanentAddress: value as String);
        break;
      case 'FatherName':
        updateModel(fatherName: value as String);
        break;
      case 'MotherName':
        updateModel(motherName: value as String);
        break;
      case 'EmergencyContactName':
        updateModel(emergencyContactName: value as String);
        break;
      case 'EmergencyContactRelation':
        updateModel(emergencyContactRelation: value as String);
        break;
      case 'EmergencyContactPhone':
        updateModel(emergencyContactPhone: value as String);
        break;
      case 'TShirtSize':
        updateModel(tShirtSize: value as String);
        break;
      case 'Degree':
        updateModel(degree: value as String);
        break;
      case 'Subject':
        updateModel(subject: value as String);
        break;
      case 'Designation':
        updateModel(designation: value as String);
        break;
      case 'BloodGroup':
        updateModel(bloodGroup: value as String);
        break;
      case 'MembershipType':
        updateModel(membershipType: value as String);
        break;
      case 'HasAcceptedTerms':
        updateModel(hasAcceptedTerms: value as bool);
        break;
      case 'HasAcceptedGdpr':
        updateModel(hasAcceptedGdpr: value as bool);
        break;
      case 'DateOfBirth':
        updateModel(dateOfBirth: value as String);
        break;
      case 'ProfileImagePath':
        updateModel(profileImagePath: value as String?);
        break;
      case 'NidPhotoPath':
        updateModel(nidPhotoPath: value as String?);
        break;
      case 'PaymentProofPath':
        updateModel(paymentProofPath: value as String?);
        break;
      case 'NotifyEventCreation':
        updateModel(notifyEventCreation: value as bool);
        break;
      case 'NotifyParticipationApproval':
        updateModel(notifyParticipationApproval: value as bool);
        break;
      case 'NotifyRegistrationUpdate':
        updateModel(notifyRegistrationUpdate: value as bool);
        break;
      case 'NotifyRelevantUpdates':
        updateModel(notifyRelevantUpdates: value as bool);
        break;
      case 'PaymentMethodId':
        updateModel(paymentMethodId: value as int);
        break;
      case 'TransactionId':
        updateModel(transactionId: value as String);
        break;
      case 'InstitutionName':
        updateModel(institutionName: value as String);
        break;
    }
  }

  void reset() => state = const RegisterState();
}

// ── Provider ──────────────────────────────────────────────────────────────
final registerWizardProvider =
    StateNotifierProvider<RegisterWizardNotifier, RegisterState>(
  (ref) => RegisterWizardNotifier(),
);
