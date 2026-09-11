import 'package:flutter/material.dart';
import 'logo_spinner.dart';

class LoadingPanel extends StatelessWidget {
  final String? message;
  final double minHeight;

  const LoadingPanel({
    super.key,
    this.message,
    this.minHeight = 220,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return ConstrainedBox(
      constraints: BoxConstraints(minHeight: minHeight),
      child: Center(
        child: Container(
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 28),
          decoration: BoxDecoration(
            color: theme.colorScheme.surface.withValues(alpha: 0.72),
            borderRadius: BorderRadius.circular(20),
            border: Border.all(
              color: theme.colorScheme.outline.withValues(alpha: 0.22),
            ),
          ),
          child: LogoSpinner(
            size: 88,
            label: message,
          ),
        ),
      ),
    );
  }
}
