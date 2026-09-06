import 'package:flutter/material.dart';
import '../theme/app_theme.dart';

/// Shared reject-with-reason prompt used by all admin approval queues
/// (member applications, gallery albums/photos, job postings) so every
/// reject action captures a reason before hitting the API, mirroring the
/// web admin's reject-with-reason modal.
///
/// Returns the trimmed reason string, or `null` if the admin cancelled.
Future<String?> showRejectReasonDialog(BuildContext context, {String title = 'REJECT SUBMISSION'}) async {
  final result = await showRejectReasonWithNotifyDialog(context, title: title, showNotifyToggle: false);
  return result?.reason;
}

/// Same prompt as [showRejectReasonDialog], with an optional "notify member"
/// checkbox (82.52) for callers that gate the API's own notify flag on it.
/// Callers that don't pass `showNotifyToggle: true` get the same dialog as
/// before, just wrapped in the record so both variants share one widget.
Future<({String reason, bool notify})?> showRejectReasonWithNotifyDialog(
  BuildContext context, {
  String title = 'REJECT SUBMISSION',
  bool showNotifyToggle = true,
}) async {
  final reasonCtrl = TextEditingController();
  bool notify = true;
  final result = await showDialog<bool>(
    context: context,
    builder: (context) => StatefulBuilder(
      builder: (context, setState) => AlertDialog(
        backgroundColor: AppTheme.deepCharcoal,
        surfaceTintColor: Colors.transparent,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20), side: const BorderSide(color: Colors.redAccent, width: 0.5)),
        title: Text(title, style: const TextStyle(color: Colors.redAccent, fontWeight: FontWeight.w900, fontSize: 14, letterSpacing: 1)),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text('Please provide a reason for rejection. This will be visible to the submitter.', style: TextStyle(color: Colors.white70, height: 1.4, fontSize: 12)),
            const SizedBox(height: AppTheme.spaceM),
            TextField(
              controller: reasonCtrl,
              autofocus: true,
              maxLines: 3,
              decoration: const InputDecoration(labelText: 'Reason', prefixIcon: Icon(Icons.report_gmailerrorred_rounded)),
            ),
            if (showNotifyToggle)
              CheckboxListTile(
                value: notify,
                onChanged: (v) => setState(() => notify = v ?? true),
                title: const Text('Notify the member', style: TextStyle(color: Colors.white70, fontSize: 13)),
                controlAffinity: ListTileControlAffinity.leading,
                contentPadding: EdgeInsets.zero,
                dense: true,
              ),
          ],
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context, false), child: const Text('CANCEL')),
          ElevatedButton(
            onPressed: () => Navigator.pop(context, true),
            style: ElevatedButton.styleFrom(backgroundColor: Colors.redAccent),
            child: const Text('REJECT'),
          ),
        ],
      ),
    ),
  );

  if (result != true) return null;
  final reason = reasonCtrl.text.trim();
  return (reason: reason.isEmpty ? 'No reason provided' : reason, notify: notify);
}
