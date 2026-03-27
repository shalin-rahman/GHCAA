import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
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
      title: 'Technical Support',
      breadcrumb: 'System Hub > Support Registry',
      child: SingleChildScrollView(
        padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 40),
        child: Column(
          children: [
            Container(
              padding: const EdgeInsets.all(24),
              decoration: BoxDecoration(
                color: AppTheme.royalGold.withValues(alpha: 0.05),
                shape: BoxShape.circle,
                border: Border.all(color: AppTheme.royalGold.withValues(alpha: 0.2)),
              ),
              child: const Icon(Icons.support_agent_outlined, size: 64, color: AppTheme.royalGold),
            ),
            const SizedBox(height: 24),
            const Text(
              'ASSOCIATION HELP DESK',
              style: TextStyle(fontSize: 10, fontWeight: FontWeight.w900, color: AppTheme.royalGold, letterSpacing: 2)
            ),
            const SizedBox(height: 12),
            const Text(
              'Need assistance or have platform feedback? Our executive support team is ready to synchronize with you.', 
              textAlign: TextAlign.center,
              style: TextStyle(color: AppTheme.textSecondaryDark, fontSize: 13, height: 1.5)
            ),
            const SizedBox(height: 48),
            const Align(
              alignment: Alignment.centerLeft,
              child: Padding(
                padding: EdgeInsets.only(left: 4, bottom: 8),
                child: Text('MESSAGE DETAILS', style: TextStyle(fontSize: 9, fontWeight: FontWeight.w900, color: AppTheme.royalGold, letterSpacing: 1.5)),
              ),
            ),
            GlassContainer(
              padding: const EdgeInsets.all(16),
              child: TextField(
                controller: _msgController,
                maxLines: 6,
                style: const TextStyle(color: Colors.white, fontSize: 14),
                decoration: const InputDecoration(
                  hintText: 'Describe your technical issue or alumni request...',
                  hintStyle: TextStyle(color: AppTheme.textSecondaryDark, fontSize: 13),
                  border: InputBorder.none,
                ),
              ),
            ),
            const SizedBox(height: 40),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed: _sending ? null : () async {
                  HapticFeedback.lightImpact();
                  setState(() => _sending = true);
                  try {
                    final success = await ref.read(supportServiceProvider).contactSupport(_msgController.text);
                    if (!context.mounted) return;
                    setState(() => _sending = false);
                    if (success) {
                      HapticFeedback.mediumImpact();
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(content: Text('Support ticket synchronized successfully.')),
                      );
                      _msgController.clear();
                    }
                  } catch (e) {
                    if (context.mounted) setState(() => _sending = false);
                  }
                },
                icon: _sending ? const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.black)) : const Icon(Icons.send_rounded, size: 18),
                label: Text(_sending ? 'SYNCHRONIZING...' : 'SEND SUPPORT TICKET', style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 11, letterSpacing: 1)),
                style: ElevatedButton.styleFrom(padding: const EdgeInsets.symmetric(vertical: 18)),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
