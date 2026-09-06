import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

class AdminActionCircle extends StatelessWidget {
  final IconData icon;
  final Color color;
  final VoidCallback onTap;
  final String? tooltip;
  final bool disabled;

  const AdminActionCircle({
    super.key,
    required this.icon,
    required this.color,
    required this.onTap,
    this.tooltip,
    this.disabled = false,
  });

  @override
  Widget build(BuildContext context) {
    return Tooltip(
      message: tooltip ?? '',
      child: GestureDetector(
        onTap: disabled ? null : () {
          HapticFeedback.mediumImpact();
          onTap();
        },
        child: Container(
          padding: const EdgeInsets.all(10),
          decoration: BoxDecoration(
            color: Colors.black54,
            shape: BoxShape.circle,
            border: Border.all(color: (disabled ? Colors.white24 : color).withValues(alpha: 0.3)),
          ),
          child: Icon(icon, color: disabled ? Colors.white24 : color, size: 18),
        ),
      ),
    );
  }
}
