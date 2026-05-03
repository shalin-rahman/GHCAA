import 'dart:ui';
import 'package:flutter/material.dart';
import '../theme/app_theme.dart';

class GlassContainer extends StatelessWidget {
  final Widget child;
  final double blurRadius;
  final double opacity;
  final EdgeInsetsGeometry padding;

  const GlassContainer({
    super.key,
    required this.child,
    this.blurRadius = 10.0,
    this.opacity = 0.15,
    this.padding = const EdgeInsets.all(AppTheme.spaceL),
  });

  @override
  Widget build(BuildContext context) {
    return ClipRRect(
      borderRadius: BorderRadius.circular(AppTheme.radiusXL),
      child: BackdropFilter(
        filter: ImageFilter.blur(sigmaX: blurRadius, sigmaY: blurRadius),
        child: Container(
          padding: padding,
          decoration: BoxDecoration(
            color: Colors.white.withValues(alpha: opacity),
            borderRadius: BorderRadius.circular(AppTheme.radiusXL),
            border: Border.all(
              color: AppTheme.royalGold.withValues(alpha: 0.3),
              width: 1.5,
            ),
          ),
          child: DefaultTextStyle(
            style: const TextStyle(color: AppTheme.textPrimaryDark, fontFamily: 'Outfit'),
            child: child,
          ),
        ),
      ),
    );
  }
}
