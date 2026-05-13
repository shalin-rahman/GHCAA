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
import '../../core/utils/app_utils.dart';

class RegisterScreen extends ConsumerStatefulWidget {
  const RegisterScreen({super.key});

  @override
  ConsumerState<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends ConsumerState<RegisterScreen> {
  bool _isLoading = false;
  final _formKey = GlobalKey<FormState>();

  static const _stepLabels = [
    'Identity & Reachability',
    'Background & Milestones',
    'Registry Filing & Subscription'
  ];

  @override
  Widget build(BuildContext context) {
    final registerState = ref.watch(registerWizardProvider);
    final currentStep = registerState.currentStep; // 0-indexed

    return AppScaffold(
      title: 'Member Registry',
      breadcrumb: 'PORTAL > REGISTRATION',
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
        padding: const EdgeInsets.all(AppTheme.spaceL),
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
                        margin: const EdgeInsets.symmetric(horizontal: AppTheme.spaceXS),
                        height: AppTheme.spaceXS,
                        decoration: BoxDecoration(
                          color: isActive ? AppTheme.royalGold : Colors.white10,
                          borderRadius: BorderRadius.circular(AppTheme.radiusXS / 2),
                        ),
                      ),
                    );
                  }),
                ),
                const SizedBox(height: AppTheme.spaceS),
                Text(
                  'Step ${currentStep + 1} of 3 — ${_stepLabels[currentStep]}',
                  style: const TextStyle(
                    color: AppTheme.royalGold,
                    fontSize: 11,
                    fontWeight: FontWeight.bold,
                    letterSpacing: 0.5,
                  ),
                ),
                const SizedBox(height: AppTheme.spaceL),
                GlassContainer(
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      _buildStepContent(context, registerState),
                      const SizedBox(height: AppTheme.spaceXL),
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
                                registerState.isLastStep ? 'Finalize Registry' : 'Continue Assessment →',
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
      case 1: return _buildStep2Background(context, state);
      case 2: return _buildStep3Registry(context, state);
      default: return const SizedBox.shrink();
    }
  }

  // ─── Step 1: Identity & Reachability ─────────────────────────────────────
  Widget _buildStep1Identity(BuildContext context, RegisterState state) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        _sectionTitle('Identity & Reachability'),
        _buildTextField(
          label: 'Full Legal Name (SSC/HSC Record) *',
          initialValue: state.data['FullName'],
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('FullName', v),
        ),
        _gap(),
        _buildTextField(
          label: "Father's Name *",
          initialValue: state.data['FatherName'],
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('FatherName', v),
        ),
        _gap(),
        _buildTextField(
          label: "Mother's Name *",
          initialValue: state.data['MotherName'],
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('MotherName', v),
        ),
        _gap(),
        _buildTextField(
          label: 'National ID (NID) *',
          initialValue: state.data['NID'],
          keyboardType: TextInputType.number,
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('NID', v),
        ),
        _gap(),
        _buildTextField(
          label: 'Verified Mobile *',
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
          label: 'Primary Email (Login) *',
          initialValue: state.data['Email'],
          keyboardType: TextInputType.emailAddress,
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('Email', v),
          validator: (v) {
            if (v == null || v.isEmpty) return 'Email is required';
            if (!RegExp(r'^[\w.-]+@[\w.-]+\.\w+$').hasMatch(v)) return 'Enter a valid email address';
            return null;
          },
        ),
        _buildDateField(
          label: 'Date Of Birth (Registry Record) *',
          value: state.data['DateOfBirth'],
          onPicked: (v) => ref.read(registerWizardProvider.notifier).updateData('DateOfBirth', v),
        ),
        const Divider(color: Colors.white10),
        _buildAsyncDropdown(
          label: 'Blood Group *',
          group: 'BloodGroup',
          value: state.data['BloodGroup'],
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('BloodGroup', v),
        ),
        _gap(),
        _buildAsyncDropdown(
          label: 'T-Shirt Size *',
          group: 'TShirtSize',
          value: state.data['TShirtSize'],
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('TShirtSize', v),
        ),
        _gap(),
        _buildTextField(
          label: 'Present Resident Address *',
          initialValue: state.data['PresentAddress'],
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('PresentAddress', v),
        ),
        _gap(),
        _buildTextField(
          label: 'Permanent Family Residence *',
          initialValue: state.data['PermanentAddress'],
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('PermanentAddress', v),
        ),
      ],
    );
  }

  // ─── Step 2: Background & Milestones ─────────────────────────────────────
  Widget _buildStep2Background(BuildContext context, RegisterState state) {
    final degree = state.data['Degree'] ?? 'HSC';
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        _sectionTitle('Academic Records'),
        _buildTextField(
          label: 'Educational Institution *',
          initialValue: state.data['InstitutionName'],
          hintText: 'Default: Govt. Haraganga College',
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('InstitutionName', v),
        ),
        _gap(),
        _buildAsyncDropdown(
          label: 'Degree Conferred *',
          group: 'Degree',
          value: degree,
          onChanged: (v) {
            ref.read(registerWizardProvider.notifier).updateData('Degree', v);
            ref.read(registerWizardProvider.notifier).updateData('Subject', null);
          },
        ),
        _gap(),
        _buildAsyncDropdown(
          label: 'Major Cluster / Subject *',
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
        const Divider(color: Colors.white10),
        _buildTextField(
          label: 'Current Profession / Designation',
          initialValue: state.data['Designation'],
          required: false,
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('Designation', v),
        ),
        _gap(),
        _sectionTitle('Emergency Protocol Personnel'),
        _buildTextField(
          label: 'Emergency Contact Person Legal Name *',
          initialValue: state.data['EmergencyContactName'],
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('EmergencyContactName', v),
        ),
        _gap(),
        _buildTextField(
          label: 'Consanguinity / Relationship *',
          initialValue: state.data['EmergencyContactRelation'],
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('EmergencyContactRelation', v),
        ),
        _gap(),
        _buildTextField(
          label: 'Direct Phone Channel *',
          initialValue: state.data['EmergencyContactPhone'],
          keyboardType: TextInputType.phone,
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('EmergencyContactPhone', v),
        ),
        const SizedBox(height: AppConstants.paddingExtraLarge),
        _sectionTitle('Registry Filing & Subscription'),
        _buildPickerField(
          label: 'Formal Profile Photo *',
          icon: Icons.person_outline,
          value: state.data['ProfileImagePath'],
          onPicked: (path) => ref.read(registerWizardProvider.notifier).updateData('ProfileImagePath', path),
        ),
        _gap(),
        _buildPickerField(
          label: 'Academic Certificate Proof',
          icon: Icons.badge_outlined,
          value: state.data['NidPhotoPath'],
          onPicked: (path) => ref.read(registerWizardProvider.notifier).updateData('NidPhotoPath', path),
        ),
      ],
    );
  }

  // ─── Step 3: Registry Filing & Subscription ──────────────────────────────
  Widget _buildStep3Registry(BuildContext context, RegisterState state) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        _sectionTitle('Membership & Subscription'),
        _buildAsyncDropdown(
          label: 'Membership Category *',
          group: 'MembershipType',
          value: state.data['MembershipType'] ?? 'General',
          onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('MembershipType', v),
        ),
        _gap(),
        _sectionTitle('Registry Filing Fee'),
        const Text(
          'Select your preferred channel for the one-time registration filing fee. Instructions will appear below your choice.',
          style: TextStyle(fontSize: 11, color: Colors.white54),
        ),
        const SizedBox(height: AppTheme.spaceM),
        FutureBuilder<List<Map<String, String>>>(
          future: ref.read(dropdownDataProvider).getOptions('PaymentMethod'),
          builder: (context, snapshot) {
            final options = snapshot.data ?? [];
            final selectedValue = state.data['PaymentMethodId']?.toString();
            final selectedMethod = options.firstWhere((o) => o['value'] == selectedValue, orElse: () => {});

            return Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                DropdownButtonFormField<String>(
                  initialValue: options.any((o) => o['value'] == selectedValue) ? selectedValue : null,
                  decoration: const InputDecoration(
                    labelText: 'Payment Method *',
                    border: OutlineInputBorder(),
                  ),
                  dropdownColor: AppTheme.midnightSurface,
                  items: options.map((o) => DropdownMenuItem(value: o['value'], child: Text(o['label']!))).toList(),
                  onChanged: (v) {
                    ref.read(registerWizardProvider.notifier).updateData('PaymentMethodId', int.tryParse(v ?? '0'));
                  },
                  validator: (v) => (v == null || v == '0') ? 'Please select a payment method' : null,
                ),
                if (selectedMethod.isNotEmpty) ...[
                  const SizedBox(height: AppTheme.spaceS),
                  Container(
                    padding: const EdgeInsets.all(AppTheme.spaceM),
                    decoration: BoxDecoration(
                      color: AppTheme.royalGold.withValues(alpha: 0.05),
                      borderRadius: BorderRadius.circular(AppTheme.radiusS),
                      border: Border.all(color: AppTheme.royalGold.withValues(alpha: 0.2)),
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Row(
                          children: [
                            Icon(Icons.info_outline, color: AppTheme.royalGold, size: 16),
                            SizedBox(width: AppTheme.spaceS),
                            Text('Payment Instructions', style: TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.bold, fontSize: 12)),
                          ],
                        ),
                        const SizedBox(height: AppTheme.spaceS),
                        Text(
                          selectedMethod['instructions'] ?? 'Follow standard procedure.',
                          style: const TextStyle(fontSize: 12, color: Colors.white70, height: 1.4),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 16),
                  if (selectedMethod['requiresReference'] == 'true') ...[
                    _buildTextField(
                      label: 'Transaction ID / Reference *',
                      initialValue: state.data['TransactionId'],
                      onChanged: (v) => ref.read(registerWizardProvider.notifier).updateData('TransactionId', v),
                    ),
                    _gap(),
                  ],
                  if (selectedMethod['requiresReceipt'] == 'true') ...[
                    _buildPickerField(
                      label: 'Payment Receipt / Screenshot *',
                      icon: Icons.receipt_long_outlined,
                      value: state.data['PaymentProofPath'],
                      onPicked: (path) => ref.read(registerWizardProvider.notifier).updateData('PaymentProofPath', path),
                    ),
                    _gap(),
                  ],
                ],
              ],
            );
          },
        ),
        _gap(),
        _sectionTitle('Notification Protocols'),
        const Text(
          'Choose which updates you want to receipt into your digital channel. All are active by default.',
          style: TextStyle(fontSize: 12, color: Colors.white54),
        ),
        const SizedBox(height: AppConstants.paddingMedium),
        FutureBuilder<List<Map<String, String>>>(
          future: ref.read(dropdownDataProvider).getOptions('NotificationType'),
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const Center(child: CircularProgressIndicator(color: AppTheme.royalGold));
            }
            final preferences = snapshot.data?.isNotEmpty == true
                ? snapshot.data!
                : [
                    {'value': 'NotifyEventCreation', 'label': 'Event Announcements', 'description': 'When new events are published'},
                    {'value': 'NotifyParticipationApproval', 'label': 'Participation Approvals', 'description': 'Updates on event attendance status'},
                    {'value': 'NotifyRegistrationUpdate', 'label': 'Registry Updates', 'description': 'Status changes on membership application'},
                    {'value': 'NotifyRelevantUpdates', 'label': 'Official Bulletins', 'description': 'News and relevant alumni updates'},
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
        const Divider(color: Colors.white10, height: AppTheme.spaceXXL),
        _sectionTitle('Constitution & Consent'),
        const Center(child: Icon(Icons.gavel_rounded, size: 48, color: AppTheme.royalGold)),
        const SizedBox(height: AppTheme.spaceS),
        _buildConsentCheckbox(
          'I have read and understood the constitution and the Broadened Terms and Conditions of Registration, and I irrevocably agree to be bound by them.',
          state.data['HasAcceptedTerms'] ?? false,
          (v) => ref.read(registerWizardProvider.notifier).updateData('HasAcceptedTerms', v),
        ),
        _buildConsentCheckbox(
          "I explicitly consent to the Association's Data Privacy & GDPR protocols for processing my personal information as described in the directory and privacy sections.",
          state.data['HasAcceptedGdpr'] ?? false,
          (v) => ref.read(registerWizardProvider.notifier).updateData('HasAcceptedGdpr', v),
        ),
        _buildConsentCheckbox(
          'I solemnly affirm that the data provided is accurate. I pledge to uphold the GHCAA Constitution and maintain association decorum.',
          state.data['HasAffirmed'] ?? false,
          (v) => ref.read(registerWizardProvider.notifier).updateData('HasAffirmed', v),
        ),
      ],
    );
  }

  Widget _buildConsentCheckbox(String label, bool value, Function(bool?) onChanged) {
    return Padding(
      padding: const EdgeInsets.only(bottom: AppTheme.spaceS),
      child: CheckboxListTile(
        contentPadding: EdgeInsets.zero,
        title: Text(label, style: const TextStyle(fontSize: 11, color: Colors.white70, height: 1.4)),
        value: value,
        activeColor: AppTheme.royalGold,
        controlAffinity: ListTileControlAffinity.leading,
        onChanged: onChanged,
      ),
    );
  }

  // ─── Helpers ─────────────────────────────────────────────────────────────
  Widget _sectionTitle(String title) => Padding(
    padding: const EdgeInsets.only(bottom: AppConstants.paddingMedium),
    child: Text(title, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16, color: Colors.white)),
  );

  Widget _gap() => const Divider(color: Colors.white10);

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
        const SizedBox(height: AppTheme.spaceXS),
        InkWell(
          onTap: () async {
            final file = await ref.read(fileServiceProvider).pickImage();
            if (file != null) {
              onPicked(file.path);
            }
          },
          borderRadius: BorderRadius.circular(AppTheme.radiusS),
          child: Container(
            padding: const EdgeInsets.all(AppTheme.spaceM),
            decoration: BoxDecoration(
              border: Border.all(color: value != null ? AppTheme.royalGold.withValues(alpha: 0.5) : Colors.white10),
              borderRadius: BorderRadius.circular(AppTheme.radiusS),
            ),
            child: Row(
              children: [
                Icon(icon, color: value != null ? AppTheme.royalGold : Colors.white30, size: 20),
                const SizedBox(width: AppTheme.spaceM),
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
    String? hintText,
    TextInputType keyboardType = TextInputType.text,
    bool required = true,
    String? Function(String?)? validator,
  }) {
    return TextFormField(
      initialValue: initialValue,
      decoration: InputDecoration(
        labelText: label, 
        hintText: hintText,
        border: InputBorder.none,
      ),
      keyboardType: keyboardType,
      onChanged: onChanged,
      validator: validator ?? (required ? (v) => (v == null || v.isEmpty) ? 'This field is required' : null : null),
    );
  }

  Widget _buildDateField({
    required String label,
    String? value,
    required Function(String) onPicked,
  }) {
    return InkWell(
      onTap: () async {
        final date = await showDatePicker(
          context: context,
          initialDate: value != null ? AppUtils.parseDate(value) ?? DateTime.now().subtract(const Duration(days: 365 * 25)) : DateTime.now().subtract(const Duration(days: 365 * 25)),
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
          onPicked(AppUtils.formatDate(date));
        }
      },
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: AppTheme.spaceM, vertical: AppTheme.spaceM),
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(AppTheme.radiusXS),
          border: Border.all(color: Colors.white24),
        ),
        child: Row(
          children: [
            const Icon(Icons.calendar_today_outlined, size: 18, color: AppTheme.royalGold),
            const SizedBox(width: AppTheme.spaceM),
            Expanded(
              child: Text(
                value != null ? AppUtils.formatDate(value) : label,
                style: TextStyle(color: value != null ? Colors.white : Colors.white54, fontSize: 13),
                overflow: TextOverflow.ellipsis,
              ),
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _submitRegistration(RegisterState registerState) async {
    // Enforce terms acceptance before advancing
    if (registerState.data['HasAcceptedTerms'] != true || registerState.data['HasAcceptedGdpr'] != true) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please accept all terms and privacy policies to continue.'), backgroundColor: Colors.redAccent),
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
          const SnackBar(content: Text('Application submitted! Check your email for verification.')),
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
