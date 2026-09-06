import 'package:flutter/material.dart';
import '../theme/app_theme.dart';

/// Shared confirm/cancel prompt (82.48) so screens stop hand-rolling the same
/// AlertDialog for delete and other yes/no actions. Mirrors the styling those
/// screens already used: [AppTheme.midnightSurface] background, plain
/// CANCEL/confirm TextButton footer.
///
/// Returns `true` if the user confirmed, `false` otherwise (cancel, dismiss,
/// or back button — never `null`, so callers don't need an `== true` check).
Future<bool> showConfirmDialog(
  BuildContext context, {
  required String title,
  String? message,
  String confirmLabel = 'Confirm',
  bool destructive = false,
}) async {
  final confirmColor = destructive ? Colors.redAccent : AppTheme.royalGold;
  final result = await showDialog<bool>(
    context: context,
    builder: (context) => AlertDialog(
      backgroundColor: AppTheme.midnightSurface,
      title: Text(title, style: TextStyle(color: destructive ? Colors.redAccent : Colors.white, fontWeight: FontWeight.w900)),
      content: message == null ? null : Text(message, style: const TextStyle(color: Colors.white70, height: 1.4)),
      actions: [
        TextButton(onPressed: () => Navigator.pop(context, false), child: const Text('CANCEL')),
        ElevatedButton(
          onPressed: () => Navigator.pop(context, true),
          style: ElevatedButton.styleFrom(backgroundColor: confirmColor),
          child: Text(confirmLabel.toUpperCase()),
        ),
      ],
    ),
  );
  return result ?? false;
}
