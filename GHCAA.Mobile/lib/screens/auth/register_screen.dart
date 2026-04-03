import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/constants/app_constants.dart';
import '../../features/auth/register_wizard_provider.dart';
import '../../features/auth/auth_service.dart';
import '../../features/lookups/dropdown_service.dart';
import '../../features/files/file_service.dart';

class RegisterScreen extends ConsumerStatefulWidget {
  const RegisterScreen({super.key});

  @override
  ConsumerState<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends ConsumerState<RegisterScreen> {
  bool _isLoading = false;
  final _formKey = GlobalKey<FormState>();

  static const _stepLabels = ['Identity & Contact', 'Academic & Media', 'Preferences & Submit'];

  @override
  Widget build(BuildContext context) {
    final registerState = ref.watch(registerWizardProvider);
    final currentStep = registerState.currentStep; // 0-indexed

    return AppScaffold(
      title: 'Alumni Enrollment',
      breadcrumb: 'Executive Onboarding > Membership Enrollment',
      leading: IconButton(
        icon: const Icon(Icons.arrow_back),
        onPressed: () {
          if (currentStep > 0) {
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
                // Step indicator
                Row(
                  children: List.generate(3, (i) {
                    final isActive = i <= currentStep;
                    return Expanded(
                      child: Container(
                        margin: const EdgeInsets.symmetric(horizontal: 3),
                        height: 4,
                        decoration: BoxDecoration(
                          color: isActive ? AppTheme.royalGold : Colors.white10,
                          borderRadius: BorderRadius.circular(2),
                        ),
                      ),
                    );
                  }),
                ),
                const SizedBox(height: 8),
                Text(
                  'Step ${currentStep + 1} of 3 — ${_stepLabels[currentStep]}',
                  style: const TextStyle(color: AppTheme.royalGold, fontSize: 11, fontWeight: FontWeight.bold, letterSpacing: 0.5),
                ),
                const SizedBox(height: AppConstants.paddingLarge),
                GlassContainer(
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      _buildStepContent(context, registerState),
                      const SizedBox(height: AppConstants.paddingExtraLarge),
                      SizedBox(
                        width: double.infinity,
                        child: _isLoading
                          ? const Center(child: CircularProgressIndicator(color: AppTheme.royalGold))
                          : ElevatedButton(
                              onPressed: () async {
                                if (_formKey.currentState!.validate()) {
                                  if (!registerState.isLastStep) {
                                    ref.read(registerWizardProvider.notifier).nextStep();
                                  } else {
                                    await _submitRegistration(registerState);
                                  }
                                }
                              },
                              child: Text(
                                registerState.isLastStep ? 'Submit Application' : 'Continue',
                                style: const TextStyle(fontWeight: FontWeight.w900, letterSpacing: 1),
                              ),
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

  // ─── Step Router ─────────────────────────────────────────────────────────
  Widget _buildStepContent(BuildContext context, RegisterState state) {
    switch (state.currentStep) {
      case 0: return _buildStep1Identity(context, state);
      case 1: return _buildStep2AcademicMedia(context, state);
      case 2: return _buildStep3PreferencesVerification(context, state);
      default: return const SizedBox.shrink();
    }
  }

  // ─── Step 1: Identity & Contact ───────────────────────────────────────────
  Widget _buildStep1Identity(BuildContext context, RegisterState state) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        _sectionTitle('Bio & Identity'),
        _buildTextField(
          label: 'Full Name *',
          initialValue: state.data['FullName'],
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('FullName', v),
        ),
        _gap(),
        _buildTextField(
          label: 'NID Number *',
          initialValue: state.data['NID'],
          keyboardType: TextInputType.number,
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('NID', v),
        ),
        _gap(),
        _buildTextField(
          label: 'Mobile Number *',
          initialValue: state.data['MobileNo'],
          keyboardType: TextInputType.phone,
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('MobileNo', v),
          validator: (v) {
            if (v == null || v.isEmpty) return 'Mobile number is required';
            if (!RegExp(r'^01[3-9]\d{8}$').hasMatch(v)) return 'Enter a valid BD mobile number (e.g. 017XXXXXXXX)';
            return null;
          },
        ),
        _gap(),
        _buildTextField(
          label: 'Email Address *',
          initialValue: state.data['Email'],
          keyboardType: TextInputType.emailAddress,
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('Email', v),
          validator: (v) {
            if (v == null || v.isEmpty) return 'Email is required';
            if (!RegExp(r'^[\w.-]+@[\w.-]+\.\w+$').hasMatch(v)) return 'Enter a valid email address';
            return null;
          },
        ),
        _gap(),
        TextFormField(
          readOnly: true,
          decoration: const InputDecoration(
            labelText: 'Date of Birth *',
            border: OutlineInputBorder(),
            hintText: 'Tap to select',
            prefixIcon: Icon(Icons.calendar_today_outlined),
          ),
          controller: TextEditingController(
            text: state.data['DateOfBirth'] != null
                ? state.data['DateOfBirth'].toString().split('T')[0]
                : '',
          ),
          onTap: () async {
            final date = await showDatePicker(
              context: context,
              initialDate: DateTime.now().subtract(const Duration(days: 365 * 25)),
              firstDate: DateTime(1940),
              lastDate: DateTime.now().subtract(const Duration(days: 365 * 16)),
              builder: (ctx, child) => Theme(
                data: Theme.of(ctx).copyWith(
                  colorScheme: const ColorScheme.dark(primary: AppTheme.royalGold),
                ),
                child: child!,
              ),
            );
            if (date != null) {
              ref.read(registerWizardProvider.notifier).updateData('DateOfBirth', date.toIso8601String());
            }
          },
          validator: (_) => state.data['DateOfBirth'] == null ? 'Date of birth is required' : null,
        ),
        _gap(),
        _buildAsyncDropdown(
          label: 'Blood Group *',
          group: 'BloodGroup',
          value: state.data['BloodGroup'],
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('BloodGroup', v),
        ),
        _gap(),
        _buildTextField(
          label: 'Present Address *',
          initialValue: state.data['PresentAddress'],
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('PresentAddress', v),
        ),
        _gap(),
        _buildTextField(
          label: 'Permanent Address *',
          initialValue: state.data['PermanentAddress'],
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('PermanentAddress', v),
        ),
      ],
    );
  }

  // ─── Step 2: Academic & Media ─────────────────────────────────────────────
  Widget _buildStep2AcademicMedia(BuildContext context, RegisterState state) {
    final degree = state.data['Degree'] ?? 'HSC';
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        _sectionTitle('Academic Career'),
        _buildAsyncDropdown(
          label: 'Highest Degree from GHC *',
          group: 'Degree',
          value: degree,
          onChanged: (v) {
            ref.read(registerWizardProvider.notifier).updateData('Degree', v);
            ref.read(registerWizardProvider.notifier).updateData('Subject', null);
          },
        ),
        _gap(),
        _buildAsyncDropdown(
          label: 'Focus / Subject *',
          group: degree == 'HSC' ? 'HSCSubject' : 'GeneralSubject',
          value: state.data['Subject'],
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('Subject', v),
        ),
        _gap(),
        _buildAsyncDropdown(
          label: 'Passing Year *',
          group: 'PassingYear',
          value: state.data['PassingYear']?.toString(),
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('PassingYear', int.tryParse(v ?? '')),
        ),
        _gap(),
        _buildTextField(
          label: 'Current Designation',
          initialValue: state.data['Designation'],
          required: false,
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('Designation', v),
        ),
        _gap(),
        _buildAsyncDropdown(
          label: 'Membership Category *',
          group: 'MembershipType',
          value: state.data['MembershipType'] ?? 'General',
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('MembershipType', v),
        ),
        const SizedBox(height: AppConstants.paddingLarge),
        _sectionTitle('Identity Documents'),
        _buildPickerField(
          label: 'Profile Photo',
          icon: Icons.person_outline,
          value: state.data['ProfileImagePath'],
          onPicked: (path) => ref.read(registerWizardProvider.notifier).updateData('ProfileImagePath', path),
        ),
        _gap(),
        _buildPickerField(
          label: 'NID Scan / Photo',
          icon: Icons.badge_outlined,
          value: state.data['NidPhotoPath'],
          onPicked: (path) => ref.read(registerWizardProvider.notifier).updateData('NidPhotoPath', path),
        ),
      ],
    );
  }

  // ─── Step 3: Preferences & Verification ──────────────────────────────────
  Widget _buildStep3PreferencesVerification(BuildContext context, RegisterState state) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        _sectionTitle('Notification Preferences'),
        const Text(
          'Choose which updates you want to receive. All are enabled by default — you can change these later from your profile.',
          style: TextStyle(fontSize: 12, color: Colors.white54),
        ),
        const SizedBox(height: AppConstants.paddingMedium),
        FutureBuilder<List<Map<String, String>>>(
          future: ref.read(dropdownDataProvider).getOptions('NotificationType'),
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const Center(child: CircularProgressIndicator(color: AppTheme.royalGold));
            }
            // Fallback to hardcoded defaults if API fails or returns empty
            final preferences = snapshot.data?.isNotEmpty == true
                ? snapshot.data!
                : [
                    {'value': 'NotifyEventCreation', 'label': 'New Event Announcements', 'description': 'Get notified when new events are published'},
                    {'value': 'NotifyParticipationApproval', 'label': 'Participation Approvals', 'description': 'Updates on your event registration status'},
                    {'value': 'NotifyRegistrationUpdate', 'label': 'Registration Updates', 'description': 'Status changes on your membership application'},
                    {'value': 'NotifyRelevantUpdates', 'label': 'General Announcements', 'description': 'Alumni news and community updates'},
                  ];

            return Column(
              children: preferences.map((p) {
                final key = p['value']!;
                final currentVal = state.data[key] ?? true;
                return SwitchListTile(
                  contentPadding: EdgeInsets.zero,
                  title: Text(p['label']!, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13)),
                  subtitle: Text(p['description'] ?? '', style: const TextStyle(fontSize: 11, color: Colors.white54)),
                  value: currentVal is bool ? currentVal : true,
                  activeThumbColor: AppTheme.royalGold,
                  onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData(key, v),
                );
              }).toList(),
            );
          },
        ),
        const Divider(color: Colors.white10, height: 40),
        _sectionTitle('Terms & Submission'),
        const Icon(Icons.verified_user_outlined, size: 48, color: AppTheme.royalGold),
        const SizedBox(height: 12),
        const Text(
          'By submitting, you confirm that all information provided is accurate and you agree to the Haragangian Alumni Association terms and privacy policy.',
          textAlign: TextAlign.center,
          style: TextStyle(fontSize: 12, color: Colors.white54),
        ),
        const SizedBox(height: 8),
        CheckboxListTile(
          contentPadding: EdgeInsets.zero,
          title: const Text('I accept the Terms & Privacy Policy', style: TextStyle(fontSize: 13, fontWeight: FontWeight.bold)),
          value: state.data['HasAcceptedTerms'] ?? false,
          activeColor: AppTheme.royalGold,
          controlAffinity: ListTileControlAffinity.leading,
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('HasAcceptedTerms', v),
        ),
        // Inline validation hint
        if (state.data['HasAcceptedTerms'] != true)
          const Padding(
            padding: EdgeInsets.only(left: 8, top: 4),
            child: Text('You must accept the terms to submit', style: TextStyle(color: Colors.redAccent, fontSize: 11)),
          ),
      ],
    );
  }

  // ─── Helpers ─────────────────────────────────────────────────────────────
  Widget _sectionTitle(String title) => Padding(
    padding: const EdgeInsets.only(bottom: AppConstants.paddingMedium),
    child: Text(title, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16, color: Colors.white)),
  );

  Widget _gap() => const SizedBox(height: AppConstants.paddingMedium);

  Widget _buildPickerField({
    required String label,
    required IconData icon,
    String? value,
    required Function(String?) onPicked,
  }) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(label.toUpperCase(), style: const TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.bold)),
        const SizedBox(height: 6),
        InkWell(
          onTap: () async {
            final file = await ref.read(fileServiceProvider).pickImage();
            if (file != null) {
              onPicked(file.path);
            }
          },
          borderRadius: BorderRadius.circular(8),
          child: Container(
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              border: Border.all(color: value != null ? AppTheme.royalGold.withValues(alpha: 0.5) : Colors.white10),
              borderRadius: BorderRadius.circular(8),
            ),
            child: Row(
              children: [
                Icon(icon, color: value != null ? AppTheme.royalGold : Colors.white30, size: 20),
                const SizedBox(width: 12),
                Expanded(
                  child: Text(
                    value != null ? value.split('/').last : 'Tap to select file...',
                    style: TextStyle(color: value != null ? Colors.white : Colors.white30, fontSize: 13),
                    overflow: TextOverflow.ellipsis,
                  ),
                ),
                if (value != null) const Icon(Icons.check_circle, color: Colors.greenAccent, size: 16),
              ],
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildAsyncDropdown({
    required String label,
    required String group,
    String? value,
    required void Function(String?) onChanged,
  }) {
    return FutureBuilder<List<Map<String, String>>>(
      future: ref.read(dropdownDataProvider).getOptions(group),
      builder: (context, snapshot) {
        final options = snapshot.data ?? [];
        final validValue = options.any((e) => e['value'] == value) ? value : null;
        return DropdownButtonFormField<String>(
          initialValue: validValue,
          decoration: InputDecoration(labelText: label, border: const OutlineInputBorder()),
          dropdownColor: AppTheme.midnightSurface,
          items: options.map((o) => DropdownMenuItem(value: o['value'], child: Text(o['label']!))).toList(),
          onChanged: onChanged,
          validator: (v) => v == null ? 'Please select $label' : null,
        );
      },
    );
  }

  Widget _buildTextField({
    required String label,
    required String? initialValue,
    required Function(String) onChanged,
    TextInputType keyboardType = TextInputType.text,
    bool required = true,
    String? Function(String?)? validator,
  }) {
    return TextFormField(
      initialValue: initialValue,
      decoration: InputDecoration(labelText: label, border: const OutlineInputBorder()),
      keyboardType: keyboardType,
      onChanged: onChanged,
      validator: validator ?? (required ? (v) => (v == null || v.isEmpty) ? 'This field is required' : null : null),
    );
  }

  Future<void> _submitRegistration(RegisterState registerState) async {
    // Enforce terms acceptance before advancing
    if (registerState.data['HasAcceptedTerms'] != true) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please accept the Terms & Privacy Policy to continue.'), backgroundColor: Colors.redAccent),
      );
      return;
    }

    setState(() => _isLoading = true);
    try {
      final error = await ref.read(authServiceProvider).register(registerState.model.toJson());
      if (!mounted) return;

      if (error == null) {
        ref.read(registerWizardProvider.notifier).reset();
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Application submitted! Check your email for the OTP verification link.')),
        );
        context.go('/login');
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Submission failed: $error'), backgroundColor: Colors.redAccent),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Connection error: $e'), backgroundColor: Colors.redAccent),
        );
      }
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }
}
