import 'package:flutter/material.dart';
import '../api/step_up_interceptor.dart';
import '../theme/app_theme.dart';

/// 37.1w: asks for the step-up code. Styled like [showConfirmDialog].
/// With [useApp] it asks for the authenticator app code, or a recovery code
/// (95.6), and says nothing about email. With [canSwitch] it also offers an
/// emailed code, and returns [StepUpInterceptor.emailInstead] when picked.
/// Returns the code, or `null` if the user cancels or dismisses it.
Future<String?> showStepUpDialog(BuildContext context,
    {String? sentTo, bool useApp = false, bool canSwitch = false}) {
  final String intro;
  if (useApp) {
    intro = 'Enter the 6-digit code from your authenticator app. Lost your phone? Enter a recovery code.';
  } else if (sentTo == null || sentTo.isEmpty) {
    intro = 'We sent a code to your email. Enter it to continue.';
  } else {
    intro = 'We sent a code to $sentTo. Enter it to continue.';
  }
  return showDialog<String>(
    context: context,
    barrierDismissible: false,
    builder: (context) => _StepUpDialog(intro: intro, useApp: useApp, canSwitch: canSwitch),
  );
}

// Owns the controller so it is disposed after the closing animation, not while the field still draws.
class _StepUpDialog extends StatefulWidget {
  const _StepUpDialog({required this.intro, required this.useApp, required this.canSwitch});

  final String intro;
  final bool useApp;
  final bool canSwitch;

  @override
  State<_StepUpDialog> createState() => _StepUpDialogState();
}

class _StepUpDialogState extends State<_StepUpDialog> {
  final _controller = TextEditingController();

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      backgroundColor: AppTheme.midnightSurface,
      title: const Text('Verify it is you', style: TextStyle(color: Colors.white, fontWeight: FontWeight.w900)),
      content: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(widget.intro, style: const TextStyle(color: Colors.white70, height: 1.4)),
          const SizedBox(height: 12),
          TextField(
            key: const Key('stepUpCodeField'),
            controller: _controller,
            autofocus: true,
            // Recovery codes have letters, so the app mode keeps the full keyboard.
            keyboardType: widget.useApp ? TextInputType.text : TextInputType.number,
            style: const TextStyle(color: Colors.white),
            decoration: InputDecoration(labelText: widget.useApp ? 'App or recovery code' : 'Verification code'),
          ),
          if (widget.useApp && widget.canSwitch)
            TextButton(
              key: const Key('stepUpEmailInstead'),
              onPressed: () => Navigator.pop(context, StepUpInterceptor.emailInstead),
              child: const Text('Email me a code instead'),
            ),
        ],
      ),
      actions: [
        TextButton(onPressed: () => Navigator.pop(context), child: const Text('CANCEL')),
        ElevatedButton(
          onPressed: () => Navigator.pop(context, _controller.text),
          style: ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold),
          child: const Text('VERIFY'),
        ),
      ],
    );
  }
}
