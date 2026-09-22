import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../core/theme/app_theme.dart';
import '../core/widgets/empty_state_widget.dart';
import '../core/widgets/glass_container.dart';
import '../core/widgets/loading_panel.dart';
import '../core/utils/app_utils.dart';
import '../features/credentials/credential_verification_service.dart';

final credentialVerificationProvider =
    FutureProvider.autoDispose.family<CredentialVerification, String>(
  (ref, shortCode) =>
      ref.read(credentialVerificationServiceProvider).verify(shortCode),
);

class CredentialVerificationScreen extends ConsumerStatefulWidget {
  final String? initialCode;

  const CredentialVerificationScreen({super.key, this.initialCode});

  @override
  ConsumerState<CredentialVerificationScreen> createState() =>
      _CredentialVerificationScreenState();
}

class _CredentialVerificationScreenState
    extends ConsumerState<CredentialVerificationScreen> {
  late final TextEditingController _codeController;
  String? _submittedCode;

  @override
  void initState() {
    super.initState();
    _codeController = TextEditingController(text: widget.initialCode ?? '');
    if (_isValidCode(_codeController.text)) {
      _submittedCode = _codeController.text.trim().toUpperCase();
    }
  }

  @override
  void dispose() {
    _codeController.dispose();
    super.dispose();
  }

  bool _isValidCode(String value) =>
      RegExp(r'^[A-HJ-NP-Z2-9]{10}$', caseSensitive: false)
          .hasMatch(value.trim());

  void _submit() {
    final code = _codeController.text.trim().toUpperCase();
    if (!_isValidCode(code)) {
      setState(() => _submittedCode = null);
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Enter a valid credential code.')),
      );
      return;
    }
    setState(() => _submittedCode = code);
  }

  @override
  Widget build(BuildContext context) {
    final verification = _submittedCode == null
        ? null
        : ref.watch(credentialVerificationProvider(_submittedCode!));

    return Scaffold(
      appBar: AppBar(title: const Text('Verify credential')),
      body: ListView(
        padding: const EdgeInsets.all(AppTheme.spaceL),
        children: [
          GlassContainer(
            child: Padding(
              padding: const EdgeInsets.all(AppTheme.spaceL),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Text(
                    'CREDENTIAL VERIFICATION',
                    style: Theme.of(context).textTheme.labelMedium,
                  ),
                  const SizedBox(height: AppTheme.spaceS),
                  Text(
                    'Check a membership credential',
                    style: Theme.of(context).textTheme.headlineSmall,
                  ),
                  const SizedBox(height: AppTheme.spaceL),
                  TextField(
                    controller: _codeController,
                    maxLength: 10,
                    textCapitalization: TextCapitalization.characters,
                    decoration: const InputDecoration(
                      labelText: 'Short code',
                      hintText: 'Ten-character code',
                    ),
                  ),
                  const SizedBox(height: AppTheme.spaceS),
                  FilledButton(
                    onPressed: verification?.isLoading == true ? null : _submit,
                    child: const Text('Verify'),
                  ),
                ],
              ),
            ),
          ),
          if (verification != null) ...[
            const SizedBox(height: AppTheme.spaceL),
            verification.when(
              loading: () => const LoadingPanel(message: 'Checking credential...'),
              error: (_, __) => const EmptyStateWidget(
                'Credential not found.',
                icon: Icons.verified_outlined,
              ),
              data: (credential) => GlassContainer(
                child: ListTile(
                  leading: Icon(
                    credential.valid
                        ? Icons.verified
                        : Icons.gpp_bad_outlined,
                    color: credential.valid
                        ? Theme.of(context).colorScheme.secondary
                        : Theme.of(context).colorScheme.error,
                  ),
                  title: Text(
                    credential.valid
                        ? 'Credential valid'
                        : 'Credential revoked',
                  ),
                  subtitle: Text([
                    if (credential.memberName != null) credential.memberName!,
                    if (credential.membershipType != null)
                      credential.membershipType!,
                    credential.status,
                    if (credential.issuedOn != null)
                      'Issued ${AppUtils.formatDate(credential.issuedOn!)}',
                  ].join('\n')),
                ),
              ),
            ),
          ],
        ],
      ),
    );
  }
}
