import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/constants/app_constants.dart';
import '../../features/support/support_service.dart';

class SupportScreen extends ConsumerStatefulWidget {
  const SupportScreen({super.key});

  @override
  ConsumerState<SupportScreen> createState() => _SupportScreenState();
}

class _SupportScreenState extends ConsumerState<SupportScreen> {
  final _msgController = TextEditingController();
  bool _sending = false;

  @override
  Widget build(BuildContext context) {
    return AppScaffold(
      title: 'Contact GHCAA',
      child: SingleChildScrollView(
        padding: const EdgeInsets.all(AppConstants.paddingLarge),
        child: Column(
          children: [
            const Icon(Icons.support_agent, size: 60, color: AppTheme.royalGold),
            const SizedBox(height: 16),
            const Text('Need assistance or have feedback? Send us a message below.', textAlign: TextAlign.center),
            const SizedBox(height: 32),
            GlassContainer(
              child: TextField(
                controller: _msgController,
                maxLines: 5,
                decoration: const InputDecoration(
                  hintText: 'Describe your issue...',
                  border: InputBorder.none,
                ),
              ),
            ),
            const SizedBox(height: 24),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                onPressed: _sending ? null : () async {
                  setState(() => _sending = true);
                  final success = await ref.read(supportServiceProvider).contactSupport(_msgController.text);
                  setState(() => _sending = false);
                  if (success && mounted) {
                    ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Message sent! We will get back to you.')));
                    _msgController.clear();
                  }
                },
                child: _sending ? const CircularProgressIndicator() : const Text('Send Message'),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
