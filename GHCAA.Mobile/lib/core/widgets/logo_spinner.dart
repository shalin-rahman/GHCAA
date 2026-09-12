import 'dart:ui' as ui;
import 'dart:math' as math;
import 'package:flutter/material.dart';
import 'org_logo.dart';

/// Shared "logo spinner" loader: the GHCAA seal held still at the center while
/// two counter-rotating gold arcs orbit around it. Mirrors the web app's
/// hero-banner loader (GHCAA.Web `common/logo-spinner`) so the loading
/// experience is visually consistent across web and mobile.
class LogoSpinner extends StatefulWidget {
  /// Diameter of the logo seal; the ring scales proportionally.
  final double size;
  /// Shows the outward ripple pulse behind the ring.
  final bool ripple;
  /// Optional caption shown under the spinner.
  final String? label;

  const LogoSpinner({
    super.key,
    this.size = 160,
    this.ripple = true,
    this.label,
  });

  @override
  State<LogoSpinner> createState() => _LogoSpinnerState();

  /// Convenience factory for small inline usages (buttons, list rows).
  static Widget small({double size = 22}) => LogoSpinner(size: size, ripple: false);
}

class _LogoSpinnerState extends State<LogoSpinner> with TickerProviderStateMixin {
  late final AnimationController _forward;
  late final AnimationController _reverse;
  late final AnimationController? _rippleController;

  @override
  void initState() {
    super.initState();
    _forward = AnimationController(vsync: this, duration: const Duration(milliseconds: 2400))..repeat();
    _reverse = AnimationController(vsync: this, duration: const Duration(milliseconds: 3600))..repeat(reverse: false);
    _rippleController = widget.ripple
        ? (AnimationController(vsync: this, duration: const Duration(milliseconds: 2400))..repeat())
        : null;
  }

  @override
  void dispose() {
    _forward.dispose();
    _reverse.dispose();
    _rippleController?.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final size = widget.size;
    final accent = Theme.of(context).colorScheme.secondary;
    final textColor = Theme.of(context).colorScheme.onSurface;
    final ringInset = size * 0.0875;
    final reverseRingInset = size * 0.1375;

    final spinner = SizedBox(
      width: size + reverseRingInset * 2,
      height: size + reverseRingInset * 2,
      child: Stack(
        alignment: Alignment.center,
        children: [
          if (_rippleController case final ripple?) ...[
            _RipplePulse(controller: ripple, baseSize: size, delay: 0),
            _RipplePulse(controller: ripple, baseSize: size, delay: 0.5),
          ],
          RotationTransition(
            turns: _forward,
            child: CustomPaint(
              size: Size(size + ringInset * 2, size + ringInset * 2),
              painter: _ArcPainter(
                color: accent,
                secondaryColor: accent.withValues(alpha: 0.3),
                strokeWidth: 3,
                startTurn: 0.0,
              ),
            ),
          ),
          RotationTransition(
            turns: _reverse,
            child: CustomPaint(
              size: Size(size + reverseRingInset * 2, size + reverseRingInset * 2),
              painter: _ArcPainter(
                color: accent,
                secondaryColor: accent.withValues(alpha: 0.2),
                strokeWidth: 3,
                startTurn: 0.5,
              ),
            ),
          ),
          Container(
            width: size,
            height: size,
            padding: const EdgeInsets.all(15),
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              boxShadow: [
                BoxShadow(color: accent.withValues(alpha: 0.2), blurRadius: 20),
              ],
            ),
            child: Stack(
              fit: StackFit.expand,
              children: [
                Transform.translate(
                  offset: const Offset(0, 5),
                  child: ColorFiltered(
                    colorFilter: ColorFilter.mode(Colors.black.withValues(alpha: 0.8), BlendMode.srcATop),
                    child: ImageFiltered(
                      imageFilter: ui.ImageFilter.blur(sigmaX: 5.0, sigmaY: 5.0),
                      child: const OrgLogo(fit: BoxFit.contain),
                    ),
                  ),
                ),
                const OrgLogo(fit: BoxFit.contain),
              ],
            ),
          ),
        ],
      ),
    );

    if (widget.label == null) return spinner;

    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        spinner,
        const SizedBox(height: 12),
        Text(
          widget.label!,
          textAlign: TextAlign.center,
          style: TextStyle(fontSize: 13, color: textColor.withValues(alpha: 0.8)),
        ),
      ],
    );
  }
}

class _ArcPainter extends CustomPainter {
  final Color color;
  final Color secondaryColor;
  final double strokeWidth;
  final double startTurn;

  _ArcPainter({
    required this.color,
    required this.secondaryColor,
    required this.strokeWidth,
    required this.startTurn,
  });

  @override
  void paint(Canvas canvas, Size size) {
    final rect = Rect.fromLTWH(strokeWidth / 2, strokeWidth / 2, size.width - strokeWidth, size.height - strokeWidth);
    final startAngle = startTurn * 2 * math.pi;

    final primaryPaint = Paint()
      ..color = color
      ..style = PaintingStyle.stroke
      ..strokeWidth = strokeWidth
      ..strokeCap = StrokeCap.round;
    canvas.drawArc(rect, startAngle - math.pi / 2, math.pi * 0.85, false, primaryPaint);

    final secondaryPaint = Paint()
      ..color = secondaryColor
      ..style = PaintingStyle.stroke
      ..strokeWidth = strokeWidth
      ..strokeCap = StrokeCap.round;
    canvas.drawArc(rect, startAngle + math.pi * 0.35 - math.pi / 2, math.pi * 0.5, false, secondaryPaint);
  }

  @override
  bool shouldRepaint(covariant _ArcPainter oldDelegate) => false;
}

class _RipplePulse extends StatelessWidget {
  final AnimationController controller;
  final double baseSize;
  final double delay;

  const _RipplePulse({required this.controller, required this.baseSize, required this.delay});

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: controller,
      builder: (context, _) {
        final accent = Theme.of(context).colorScheme.secondary;
        var t = (controller.value + delay) % 1.0;
        final size = baseSize * (0.756 + t * (1.375 - 0.756));
        final opacity = (0.8 * (1 - t)).clamp(0.0, 0.8);
        return Container(
          width: size,
          height: size,
          decoration: BoxDecoration(
            shape: BoxShape.circle,
            border: Border.all(color: accent.withValues(alpha: opacity * 0.5), width: 2),
          ),
        );
      },
    );
  }
}
