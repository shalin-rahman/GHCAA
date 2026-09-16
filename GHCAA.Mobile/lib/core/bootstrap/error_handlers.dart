import 'dart:io';
import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:sentry_flutter/sentry_flutter.dart';
import '../theme/app_theme.dart';
import '../../features/auth/auth_service.dart';
import '../../features/support/support_service.dart';

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
            const SizedBox(height: 12),
            AdminReportButton(exception: details.exception, stack: details.stack),
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

/// Sends the crashing error to the in-app Administrator inbox via the same
/// /contact pathway the support screen uses (12.5). Separate from the
/// "DIAGNOSE & REPORT" button above, which only reaches Sentry.
class AdminReportButton extends ConsumerStatefulWidget {
  const AdminReportButton({super.key, required this.exception, this.stack});

  final Object exception;
  final StackTrace? stack;

  @override
  ConsumerState<AdminReportButton> createState() => _AdminReportButtonState();
}

enum _ReportState { idle, sending, sent, failed }

class _AdminReportButtonState extends ConsumerState<AdminReportButton> {
  _ReportState _state = _ReportState.idle;

  String _stackSummary() {
    final lines = (widget.stack?.toString() ?? '').split('\n');
    return lines.take(5).join('\n');
  }

  Future<void> _send() async {
    setState(() => _state = _ReportState.sending);

    // Best-effort: use the cached member profile if one is already loaded.
    // The error widget can render before or without an authenticated session,
    // so this falls back to a generic sender rather than failing outright.
    final profile = ref.read(userProfileProvider).valueOrNull;
    final fullName = profile?['fullName'] as String? ?? 'Mobile App User';
    final email = profile?['email'] as String? ?? 'mobile-error-report@ghcaa.local';

    final success = await ref.read(supportServiceProvider).sendErrorReport(
          fullName: fullName,
          email: email,
          errorSummary: widget.exception.toString(),
          stackSummary: _stackSummary(),
          platformInfo: '${Platform.operatingSystem} ${Platform.operatingSystemVersion}',
        );

    if (!mounted) return;
    setState(() => _state = success ? _ReportState.sent : _ReportState.failed);
  }

  @override
  Widget build(BuildContext context) {
    final label = switch (_state) {
      _ReportState.idle => 'SEND REPORT TO ADMINISTRATOR',
      _ReportState.sending => 'SENDING...',
      _ReportState.sent => 'REPORT SENT',
      _ReportState.failed => 'FAILED — TAP TO RETRY',
    };

    return OutlinedButton(
      onPressed: _state == _ReportState.sending || _state == _ReportState.sent ? null : _send,
      style: OutlinedButton.styleFrom(side: const BorderSide(color: AppTheme.royalGold)),
      child: Text(label, style: const TextStyle(color: AppTheme.royalGold)),
    );
  }
}
