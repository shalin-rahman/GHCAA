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
          // Transparent Material ancestor so ListTile/InkWell children have a
          // surface to paint ink + background on. Without it, a ListTile sees
          // the DecoratedBox background above it and throws the debug assertion
          // "ListTile background color or ink splashes may be invisible" during
          // pump (fails golden tests on CI). Transparency paints nothing, so
          // the glass look is unchanged.
          child: Material(
            type: MaterialType.transparency,
            child: DefaultTextStyle(
              style: const TextStyle(color: AppTheme.textPrimaryDark, fontFamily: 'Outfit'),
              child: child,
            ),
          ),
        ),
      ),
    );
  }
}
