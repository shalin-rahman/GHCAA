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
  final List<Map<String, dynamic>> academicHistory;
  final String? profileImagePath;

  const RegisterModel({
    this.fullName = '',
    this.email = '',
    this.mobileNo = '',
    this.nid = '',
    this.passingYear = '',
    this.presentAddress = '',
    this.permanentAddress = '',
    this.academicHistory = const [],
    this.profileImagePath,
  });

  RegisterModel copyWith({
    String? fullName,
    String? email,
    String? mobileNo,
    String? nid,
    String? passingYear,
    String? presentAddress,
    String? permanentAddress,
    List<Map<String, dynamic>>? academicHistory,
    String? profileImagePath,
  }) {
    return RegisterModel(
      fullName: fullName ?? this.fullName,
      email: email ?? this.email,
      mobileNo: mobileNo ?? this.mobileNo,
      nid: nid ?? this.nid,
      passingYear: passingYear ?? this.passingYear,
      presentAddress: presentAddress ?? this.presentAddress,
      permanentAddress: permanentAddress ?? this.permanentAddress,
      academicHistory: academicHistory ?? this.academicHistory,
      profileImagePath: profileImagePath ?? this.profileImagePath,
    );
  }

  /// Converts to the payload expected by the backend registration endpoint.
  Map<String, dynamic> toJson() => {
        'fullName': fullName,
        'email': email,
        'mobileNo': mobileNo,
        'nid': nid,
        'passingYear': passingYear,
        'presentAddress': presentAddress,
        'permanentAddress': permanentAddress,
        'academicHistory': academicHistory,
      };
}

// ── State ─────────────────────────────────────────────────────────────────
class RegisterState {
  /// 0-indexed, matching PageController index and test contract.
  final int currentStep;
  final RegisterModel model;

  const RegisterState({
    this.currentStep = 0,
    this.model = const RegisterModel(),
  });

  /// True when user is on the final confirmation step (step 3, index 2).
  bool get isLastStep => currentStep == 2;

  /// Legacy 1-indexed getter for any UI code still using [step].
  int get step => currentStep + 1;

  /// Backwards-compatible flat-map view of the model for the register_screen UI.
  /// New code should prefer accessing [model] directly.
  Map<String, dynamic> get data => {
        'FullName': model.fullName,
        'Email': model.email,
        'MobileNo': model.mobileNo,
        'NID': model.nid,
        'PassingYear': model.passingYear.isEmpty ? null : int.tryParse(model.passingYear),
        'PresentAddress': model.presentAddress,
        'PermanentAddress': model.permanentAddress,
        'Degree': null, // stored separately via updateData
        'Subject': null,
        'Designation': null,
        'BloodGroup': 'APositive',
        'MembershipType': 'General',
        'HasAcceptedTerms': false,
      };

  RegisterState copyWith({int? currentStep, RegisterModel? model}) {
    return RegisterState(
      currentStep: currentStep ?? this.currentStep,
      model: model ?? this.model,
    );
  }
}

// ── Notifier ─────────────────────────────────────────────────────────────
class RegisterWizardNotifier extends StateNotifier<RegisterState> {
  static const int _totalSteps = 3;

  RegisterWizardNotifier() : super(const RegisterState());

  // -- Navigation -----------------------------------------------------------
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

  /// Legacy aliases preserved for register_screen UI compatibility.
  void nextStep() => nextPage();
  void prevStep() => prevPage();

  // -- Model mutation -------------------------------------------------------
  void updateModel({
    String? fullName,
    String? email,
    String? mobileNo,
    String? nid,
    String? passingYear,
    String? presentAddress,
    String? permanentAddress,
    List<Map<String, dynamic>>? academicHistory,
    String? profileImagePath,
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
        academicHistory: academicHistory,
        profileImagePath: profileImagePath,
      ),
    );
  }

  /// Legacy key-value setter kept for backwards compatibility.
  void updateData(String key, dynamic value) {
    updateModel(
      fullName: key == 'fullName' ? value as String : null,
      email: key == 'email' ? value as String : null,
      mobileNo: key == 'mobileNo' ? value as String : null,
      nid: key == 'nid' ? value as String : null,
      passingYear: key == 'passingYear' ? value as String : null,
      presentAddress: key == 'presentAddress' ? value as String : null,
      permanentAddress: key == 'permanentAddress' ? value as String : null,
    );
  }

  void reset() => state = const RegisterState();
}

// ── Provider ──────────────────────────────────────────────────────────────
final registerWizardProvider =
    StateNotifierProvider<RegisterWizardNotifier, RegisterState>(
  (ref) => RegisterWizardNotifier(),
);
