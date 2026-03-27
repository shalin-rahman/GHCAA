import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/constants/app_constants.dart';
import '../../features/auth/register_wizard_provider.dart';
import '../../features/auth/auth_service.dart';

class RegisterScreen extends ConsumerWidget {
  const RegisterScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final registerState = ref.watch(registerWizardProvider);



    return AppScaffold(
      title: 'Member Registration',
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
        child: SingleChildScrollView(
          child: Column(
            children: [
              LinearProgressIndicator(
                value: registerState.step / 5,
                backgroundColor: Theme.of(context).brightness == Brightness.dark ? Colors.white10 : Colors.black12,
                color: AppTheme.royalGold,
              ),
              const SizedBox(height: AppConstants.paddingLarge),
              GlassContainer(
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Text(
                      'Step ${registerState.step} of 5',
                      style: Theme.of(context).textTheme.displayLarge?.copyWith(fontSize: 20, color: AppTheme.royalGold),
                    ),
                    const SizedBox(height: AppConstants.paddingMedium),
                    _buildStepContent(context, registerState.step),
                    const SizedBox(height: AppConstants.paddingExtraLarge),
                    SizedBox(
                      width: double.infinity,
                      child: ElevatedButton(
                        onPressed: () async {
                          if (registerState.step < 5) {
                            ref.read(registerWizardProvider.notifier).nextStep();
                          } else {
                            final success = await ref.read(authServiceProvider).register(registerState.data);
                            if (success && context.mounted) {
                              ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Submitted for approval!')));
                              context.go('/');
                            } else if (context.mounted) {
                              ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Submission failed.')));
                            }
                          }
                        },
                        child: Text(registerState.step == 5 ? 'Finish' : 'Continue'),
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildStepContent(BuildContext context, int step) {
    final Map<int, String> titles = {
      1: 'Personal Information',
      2: 'Contact Details',
      3: 'Academic History',
      4: 'Professional Experience',
      5: 'Verification Documents',
    };

    return Column(
      children: [
        Text(titles[step] ?? '', style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
        const SizedBox(height: AppConstants.paddingLarge),
        if (step < 5) ...[
          TextFormField(decoration: const InputDecoration(labelText: 'Field 1', border: OutlineInputBorder())),
          const SizedBox(height: AppConstants.paddingMedium),
          TextFormField(decoration: const InputDecoration(labelText: 'Field 2', border: OutlineInputBorder())),
        ] else ...[
          const Icon(Icons.upload_file, size: 64, color: AppTheme.royalGold),
          const SizedBox(height: AppConstants.paddingMedium),
          const Text('Upload copy of your Degree or NID'),
        ],
      ],
    );
  }
}
