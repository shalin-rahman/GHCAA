import 'dart:io';
import 'package:flutter/material.dart';

class UploadSurface extends StatelessWidget {
  final File? file;
  final String label;
  final VoidCallback onPick;
  final VoidCallback? onRemove;
  final bool enabled;
  final bool uploading;
  final String? errorText;

  const UploadSurface({
    super.key,
    required this.file,
    required this.label,
    required this.onPick,
    this.onRemove,
    this.enabled = true,
    this.uploading = false,
    this.errorText,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final hasFile = file != null;
    return InkWell(
      onTap: enabled && !uploading ? onPick : null,
      borderRadius: BorderRadius.circular(16),
      child: Container(
        constraints: const BoxConstraints(minHeight: 112),
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: theme.colorScheme.surfaceContainerHighest.withValues(alpha: 0.35),
          borderRadius: BorderRadius.circular(16),
          border: Border.all(
            color: errorText != null
                ? theme.colorScheme.error
                : theme.colorScheme.outline.withValues(alpha: 0.35),
          ),
        ),
        child: Row(
          children: [
            Icon(
              hasFile ? Icons.check_circle_outline : Icons.upload_file,
              color: errorText != null
                  ? theme.colorScheme.error
                  : theme.colorScheme.secondary,
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Text(
                hasFile ? file!.path.split(Platform.pathSeparator).last : label,
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
              ),
            ),
            if (uploading)
              const SizedBox(
                width: 20,
                height: 20,
                child: CircularProgressIndicator(strokeWidth: 2),
              )
            else if (hasFile && onRemove != null)
              IconButton(
                onPressed: enabled ? onRemove : null,
                icon: const Icon(Icons.close),
                tooltip: 'Remove file',
              ),
          ],
        ),
      ),
    );
  }
}
