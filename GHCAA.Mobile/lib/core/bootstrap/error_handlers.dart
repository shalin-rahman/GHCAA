import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:sentry_flutter/sentry_flutter.dart';
import '../theme/app_theme.dart';

/// Installs the three Flutter/platform error handlers.
///
/// Must be called **before** [SentryFlutter.init] so Sentry's integrations
/// chain onto the handlers already installed here rather than replacing them.
void setupErrorHandlers() {
  // Fallback UI shown when a widget throws during build/layout/paint.
  ErrorWidget.builder = (details) => Directionality(
    textDirection: TextDirection.ltr,
    child: Material(
      child: Container(
        padding: const EdgeInsets.all(32),
        decoration: const BoxDecoration(
          gradient: RadialGradient(
            center: Alignment(0, -0.6),
            radius: 1.5,
            colors: [AppTheme.midnightSurface, AppTheme.midnightBase],
          ),
        ),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.error_outline_rounded, color: AppTheme.royalGold, size: 60),
            const SizedBox(height: 24),
            const Text(
              'UNEXPECTED SYSTEM OVERLOAD',
              style: TextStyle(color: Colors.white, fontWeight: FontWeight.w900, fontSize: 18, letterSpacing: 2),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 12),
            const Text(
              'The registry is currently experiencing a visual synchronization error. Our engineers have been notified.',
              style: TextStyle(color: AppTheme.textSecondaryDark, fontSize: 12, height: 1.5),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 48),
            ElevatedButton(
              onPressed: () => Sentry.captureException(details.exception, stackTrace: details.stack),
              style: ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold),
              child: const Text('DIAGNOSE & REPORT', style: TextStyle(color: Colors.black)),
            ),
          ],
        ),
      ),
    ),
  );

  // Installed before SentryFlutter.init. Sentry's error integrations chain onto
  // whatever handler is already installed at init time, so installing after init
  // would silently drop Sentry's crash classification and filtering.
  PlatformDispatcher.instance.onError = (error, stack) {
    debugPrint('Uncaught platform error: $error');
    return false; // let Sentry's chained default handler also run
  };

  // Reports framework errors automatically instead of relying on the user
  // tapping "DIAGNOSE & REPORT". Also installed before init for the same reason.
  final defaultFlutterOnError = FlutterError.onError;
  FlutterError.onError = (FlutterErrorDetails details) {
    debugPrint('Uncaught Flutter framework error: ${details.exceptionAsString()}');
    defaultFlutterOnError?.call(details);
  };
}
