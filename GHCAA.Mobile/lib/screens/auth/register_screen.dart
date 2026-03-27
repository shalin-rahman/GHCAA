import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/constants/app_constants.dart';
import '../../core/constants/registration_constants.dart';
import '../../features/auth/register_wizard_provider.dart';
import '../../features/auth/auth_service.dart';

class RegisterScreen extends ConsumerStatefulWidget {
  const RegisterScreen({super.key});

  @override
  ConsumerState<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends ConsumerState<RegisterScreen> {
  bool _isLoading = false;
  final _formKey = GlobalKey<FormState>();

  @override
  Widget build(BuildContext context) {
    final registerState = ref.watch(registerWizardProvider);

    return AppScaffold(
      title: 'Alumni Enrollment',
      breadcrumb: 'Executive Onboarding > Membership Enrollment',
      leading: IconButton(
        icon: const Icon(Icons.arrow_back),
        onPressed: () {
          if (registerState.step > 1) {
            ref.read(registerWizardProvider.notifier).prevStep();
          } else {
            context.go('/');
          }
        },
      ),
      child: Padding(
        padding: const EdgeInsets.all(AppConstants.paddingLarge),
        child: Form(
          key: _formKey,
          child: SingleChildScrollView(
            child: Column(
              children: [
                LinearProgressIndicator(
                  value: registerState.step / 3,
                  backgroundColor: Theme.of(context).brightness == Brightness.dark ? Colors.white10 : Colors.black12,
                  color: AppTheme.royalGold,
                ),
                const SizedBox(height: AppConstants.paddingLarge),
                GlassContainer(
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Text(
                        'Step ${registerState.step} of 3',
                        style: Theme.of(context).textTheme.displayLarge?.copyWith(fontSize: 20, color: AppTheme.royalGold),
                      ),
                      const SizedBox(height: AppConstants.paddingMedium),
                      _buildStepContent(context, registerState),
                      const SizedBox(height: AppConstants.paddingExtraLarge),
                      SizedBox(
                        width: double.infinity,
                        child: _isLoading 
                          ? const Center(child: CircularProgressIndicator(color: AppTheme.royalGold))
                          : ElevatedButton(
                              onPressed: () async {
                                if (_formKey.currentState!.validate()) {
                                  if (registerState.step < 3) {
                                    ref.read(registerWizardProvider.notifier).nextStep();
                                  } else {
                                    _submitRegistration(registerState);
                                  }
                                }
                              },
                              child: Text(registerState.step == 3 ? 'Submit Registry' : 'Continue'),
                            ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Future<void> _submitRegistration(RegisterState registerState) async {
    setState(() => _isLoading = true);
    try {
      final error = await ref.read(authServiceProvider).register(registerState.data);
      if (!mounted) return;
      
      if (error == null) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Registry filed successfully! Please check your email for verification.'))
        );
        context.go('/login');
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Registration failed: $error'), backgroundColor: Colors.redAccent)
        );
      }
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Backend Synchonization Error: $e'), backgroundColor: Colors.redAccent)
      );
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  Widget _buildStepContent(BuildContext context, RegisterState state) {
    switch (state.step) {
      case 1:
        return _buildIdentityStep(context, state);
      case 2:
        return _buildCareerStep(context, state);
      case 3:
        return _buildVerificationStep(context, state);
      default:
        return const SizedBox.shrink();
    }
  }

  Widget _buildIdentityStep(BuildContext context, RegisterState state) {
    return Column(
      children: [
        const Text('Bio & Identity', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
        const SizedBox(height: AppConstants.paddingLarge),
        _buildTextField(
          label: 'Full Name',
          initialValue: state.data['FullName'],
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('FullName', v),
        ),
        const SizedBox(height: AppConstants.paddingMedium),
        _buildTextField(
          label: 'NID Number',
          initialValue: state.data['NID'],
          keyboardType: TextInputType.number,
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('NID', v),
        ),
        const SizedBox(height: AppConstants.paddingMedium),
        _buildTextField(
          label: 'Mobile Number',
          initialValue: state.data['MobileNo'],
          keyboardType: TextInputType.phone,
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('MobileNo', v),
        ),
        const SizedBox(height: AppConstants.paddingMedium),
        _buildTextField(
          label: 'Email Address',
          initialValue: state.data['Email'],
          keyboardType: TextInputType.emailAddress,
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('Email', v),
        ),
        const SizedBox(height: AppConstants.paddingMedium),
        DropdownButtonFormField<String>(
          initialValue: state.data['BloodGroup'] ?? 'APositive',
          decoration: const InputDecoration(labelText: 'Blood Group'),
          items: MembershipConstants.bloodGroupOptions.map((opt) {
            return DropdownMenuItem(value: opt['value'], child: Text(opt['label']!));
          }).toList(),
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('BloodGroup', v),
        ),
      ],
    );
  }

  Widget _buildCareerStep(BuildContext context, RegisterState state) {
    final degree = state.data['Degree'] ?? 'HSC';
    
    return Column(
      children: [
        const Text('Alumni Success', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
        const SizedBox(height: AppConstants.paddingLarge),
        DropdownButtonFormField<String>(
          initialValue: degree,
          decoration: const InputDecoration(labelText: 'Degree (from GHC)'),
          items: AcademicConstants.certificates.map((opt) {
            return DropdownMenuItem(value: opt, child: Text(opt));
          }).toList(),
          onChanged: (v) {
            ref.read(registerWizardProvider.notifier).updateData('Degree', v);
            ref.read(registerWizardProvider.notifier).updateData('Subject', null);
          },
        ),
        const SizedBox(height: AppConstants.paddingMedium),
        DropdownButtonFormField<String>(
          initialValue: state.data['Subject'],
          decoration: const InputDecoration(labelText: 'Focus / Subject'),
          items: (degree == 'HSC' ? AcademicConstants.hscSubjects : AcademicConstants.generalSubjects).map((opt) {
            return DropdownMenuItem(value: opt, child: Text(opt));
          }).toList(),
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('Subject', v),
          validator: (v) => v == null ? 'Please select a focus' : null,
        ),
        const SizedBox(height: AppConstants.paddingMedium),
        DropdownButtonFormField<int>(
          initialValue: state.data['PassingYear'],
          decoration: const InputDecoration(labelText: 'Passing Year'),
          items: AcademicConstants.getAcademicYears().map((opt) {
            return DropdownMenuItem(value: opt, child: Text(opt.toString()));
          }).toList(),
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('PassingYear', v),
          validator: (v) => v == null ? 'Please select passing year' : null,
        ),
        const SizedBox(height: AppConstants.paddingMedium),
        _buildTextField(
          label: 'Current Designation',
          initialValue: state.data['Designation'],
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('Designation', v),
        ),
        const SizedBox(height: AppConstants.paddingMedium),
        DropdownButtonFormField<String>(
          initialValue: state.data['MembershipType'] ?? 'General',
          decoration: const InputDecoration(labelText: 'Membership Tier'),
          items: MembershipConstants.typeOptions.map((opt) {
            return DropdownMenuItem(value: opt['value'], child: Text(opt['label']!));
          }).toList(),
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('MembershipType', v),
        ),
      ],
    );
  }

  Widget _buildVerificationStep(BuildContext context, RegisterState state) {
    return Column(
      children: [
        const Text('Gateway Verification', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
        const SizedBox(height: AppConstants.paddingLarge),
        const Icon(Icons.verified_user_outlined, size: 80, color: AppTheme.royalGold),
        const SizedBox(height: AppConstants.paddingLarge),
        const Text(
          'By submitting, you agree to the Haragangian Alumni Association terms and conditions.',
          textAlign: TextAlign.center,
          style: TextStyle(fontSize: 14, color: Colors.grey),
        ),
        const SizedBox(height: AppConstants.paddingMedium),
        CheckboxListTile(
          title: const Text('I accept the Privacy Policy'),
          value: state.data['HasAcceptedTerms'] ?? false,
          activeColor: AppTheme.royalGold,
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('HasAcceptedTerms', v),
        ),
      ],
    );
  }

  Widget _buildTextField({
    required String label,
    required String? initialValue,
    required Function(String) onChanged,
    TextInputType keyboardType = TextInputType.text,
  }) {
    return TextFormField(
      initialValue: initialValue,
      decoration: InputDecoration(labelText: label, border: const OutlineInputBorder()),
      keyboardType: keyboardType,
      onChanged: onChanged,
      validator: (v) => v == null || v.isEmpty ? 'This field is required' : null,
    );
  }
}
