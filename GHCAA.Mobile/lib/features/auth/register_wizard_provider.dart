import 'package:flutter_riverpod/flutter_riverpod.dart';

class RegisterState {
  final int step;
  final Map<String, dynamic> data;

  RegisterState({this.step = 1, this.data = const {}});

  RegisterState copyWith({int? step, Map<String, dynamic>? data}) {
    return RegisterState(
      step: step ?? this.step,
      data: data ?? this.data,
    );
  }
}

class RegisterWizardNotifier extends StateNotifier<RegisterState> {
  RegisterWizardNotifier() : super(RegisterState());

  void nextStep() {
    if (state.step < 5) {
      state = state.copyWith(step: state.step + 1);
    }
  }

  void prevStep() {
    if (state.step > 1) {
      state = state.copyWith(step: state.step - 1);
    }
  }

  void updateData(String key, dynamic value) {
    final newData = Map<String, dynamic>.from(state.data);
    newData[key] = value;
    state = state.copyWith(data: newData);
  }
}

final registerWizardProvider = StateNotifierProvider<RegisterWizardNotifier, RegisterState>((ref) {
  return RegisterWizardNotifier();
});
