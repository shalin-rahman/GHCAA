import 'package:flutter/material.dart';
import '../theme/app_theme.dart';

class EmptyStateWidget extends StatelessWidget {
  final String message;
  final IconData icon;
  final String? actionLabel;
  final VoidCallback? onAction;

  /// 30.31: mirrors web `.empty-state.compact` — set true for inline/embedded empty states
  /// (e.g. inside a card or list section) where the default full-size treatment is too large.
  final bool compact;

  const EmptyStateWidget(
    this.message, {
    super.key,
    this.icon = Icons.inbox_outlined,
    this.actionLabel,
    this.onAction,
    this.compact = false,
  });

  @override
  Widget build(BuildContext context) {
    final iconSize = compact ? AppTheme.emptyStateCompactIconSize : 64.0;
    final gap = compact ? AppTheme.emptyStateCompactGap : AppTheme.spaceL;
    final padding = compact ? AppTheme.emptyStateCompactPadding : 40.0;

    return Center(
      child: Padding(
        padding: EdgeInsets.all(padding),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(icon, size: iconSize, color: AppTheme.royalGold.withValues(alpha: 0.3)),
            SizedBox(height: gap),
            Text(
              message,
              textAlign: TextAlign.center,
              style: TextStyle(
                color: AppTheme.textSecondaryDark,
                fontSize: compact ? 12 : 14,
                fontWeight: FontWeight.w600,
              ),
            ),
            if (actionLabel != null && onAction != null) ...[
              SizedBox(height: gap),
              ElevatedButton.icon(
                onPressed: onAction,
                icon: const Icon(Icons.refresh, size: 18),
                label: Text(actionLabel!, style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w800, letterSpacing: 1)),
              ),
            ],
          ],
        ),
      ),
    );
  }
}
