import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:ghcaa_mobile/features/auth/register_wizard_provider.dart';

void main() {
  group('Haragangian 3-Step Wizard Unit Tests', () {
    test('Step 1: Identity Gateway should validate mandatory fields', () {
      final container = ProviderContainer();
      final wizard = container.read(registerWizardProvider.notifier);
      
      // Initially, the model should be empty
      expect(wizard.state.model.fullName, '');
      expect(wizard.state.currentStep, 0);

      // Updating Identity Gateway
      wizard.updateModel(fullName: 'Shalin Rahman', mobileNo: '01712345678');
      expect(wizard.state.model.fullName, 'Shalin Rahman');
      expect(wizard.state.model.mobileNo, '01712345678');
    });

    test('Step 2: Alumni Success should require academic record persistence', () {
      final container = ProviderContainer();
      final wizard = container.read(registerWizardProvider.notifier);
      
      // Move to Step 2
      wizard.nextPage();
      expect(wizard.state.currentStep, 1);

      // Validate initial Academic history is empty
      expect(wizard.state.model.academicHistory, isEmpty);

      // Add a Govt. Haraganga College record
      wizard.updateModel(academicHistory: [
        {'institutionName': 'Govt. Haraganga College', 'degree': 'HSC', 'passingYear': '2015'}
      ]);
      expect(wizard.state.model.academicHistory.length, 1);
      expect(wizard.state.model.academicHistory[0]['degree'], 'HSC');
    });

    test('Step 3: Gateway Verification should finalize step transition', () {
      final container = ProviderContainer();
      final wizard = container.read(registerWizardProvider.notifier);
      
      // Step through all 3 stages
      expect(wizard.state.currentStep, 0); // Step 1
      wizard.nextPage();
      expect(wizard.state.currentStep, 1); // Step 2
      wizard.nextPage();
      expect(wizard.state.currentStep, 2); // Step 3: Gateway Verification
      
      // Final confirmation
      expect(wizard.state.isLastStep, isTrue);
    });
  });
}
