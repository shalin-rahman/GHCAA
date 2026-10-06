import 'package:flutter/material.dart';
import '../theme/app_theme.dart';

/// 37.1w: asks for the emailed step-up code. Styled like [showConfirmDialog].
/// Returns the code, or `null` if the user cancels or dismisses it.
Future<String?> showStepUpDialog(BuildContext context, {String? sentTo}) async {
  final controller = TextEditingController();
  final result = await showDialog<String>(
    context: context,
    barrierDismissible: false,
    builder: (context) => AlertDialog(
      backgroundColor: AppTheme.midnightSurface,
      title: const Text('Verify it is you', style: TextStyle(color: Colors.white, fontWeight: FontWeight.w900)),
      content: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            sentTo == null || sentTo.isEmpty
                ? 'We sent a code to your email. Enter it to continue.'
                : 'We sent a code to $sentTo. Enter it to continue.',
            style: const TextStyle(color: Colors.white70, height: 1.4),
          ),
          const SizedBox(height: 12),
          TextField(
            key: const Key('stepUpCodeField'),
            controller: controller,
            autofocus: true,
            keyboardType: TextInputType.number,
            style: const TextStyle(color: Colors.white),
            decoration: const InputDecoration(labelText: 'Verification code'),
          ),
        ],
      ),
      actions: [
        TextButton(onPressed: () => Navigator.pop(context), child: const Text('CANCEL')),
        ElevatedButton(
          onPressed: () => Navigator.pop(context, controller.text),
          style: ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold),
          child: const Text('VERIFY'),
        ),
      ],
    ),
  );
  controller.dispose();
  return result;
}
