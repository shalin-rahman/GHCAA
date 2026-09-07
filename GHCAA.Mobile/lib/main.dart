import 'dart:ui';
import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:sentry_flutter/sentry_flutter.dart';
import 'core/config/app_config.dart';
import 'core/theme/app_theme.dart';
import 'core/router/app_router.dart';
import 'core/session/session_manager.dart';
import 'features/notifications/push_notification_service.dart'; // Keep this import
import 'core/widgets/no_internet_banner.dart';
import 'core/services/app_localizations.dart';
import 'core/services/org_config_service.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

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
  
  // Installed before SentryFlutter.init below. sentry_flutter's error integrations chain
  // to whatever handler is already installed at init time instead of replacing it, so
  // installing after init would silently drop Sentry's crash classification and filtering.
  PlatformDispatcher.instance.onError = (error, stack) {
    debugPrint('Uncaught platform error: $error');
    return false; // let Sentry's chained default handler (installed by init, below) also run
  };

  // Reports framework errors automatically instead of relying on the user tapping
  // "DIAGNOSE & REPORT". Also installed before init, for the same chaining reason as above.
  final defaultFlutterOnError = FlutterError.onError;
  FlutterError.onError = (FlutterErrorDetails details) {
    debugPrint('Uncaught Flutter framework error: ${details.exceptionAsString()}');
    defaultFlutterOnError?.call(details);
  };

  await dotenv.load(fileName: ".env");

  final dsn = dotenv.env['SENTRY_DSN'];

  if (dsn != null && dsn.isNotEmpty && dsn != 'https://example@sentry.io/project') {
    await SentryFlutter.init(
      (options) {
        options.dsn = dsn;
        options.tracesSampleRate = 1.0; // trace every transaction; fine for our volume
        options.environment = AppConfig.environment;
      },
      appRunner: () => _initAndRunApp(),
    );
  } else {
    // No DSN configured (e.g. local dev) — run without Sentry.
    debugPrint('Sentry Observability Offline: No valid DSN provided.');
    _initAndRunApp();
  }
}

Future<void> _initAndRunApp() async {
  // 29E.5: kIsWeb is the real compile-time web flag. bool.fromEnvironment('dart.library.js_util')
  // is NOT set by the toolchain, so it was always false — meaning Firebase init would still run
  // (and crash) on web builds. kIsWeb (from foundation, re-exported by material) is correct.
  if (!kIsWeb) {
    try {
      await Firebase.initializeApp();
      try {
        FirebaseMessaging.onBackgroundMessage(PushNotificationService.firebaseMessagingBackgroundHandler);
      } catch (e) {
        debugPrint('Background message handler registration failed: $e');
      }
    } catch (e) {
      debugPrint('Cloud Notification Service Initialization Failed: $e');
    }
  } else {
    debugPrint('Native Cloud Services Skipped for Web Platform.');
  }
  
  runApp(
    const ProviderScope(
      child: AlumniApp(),
    ),
  );
}

class AlumniApp extends ConsumerStatefulWidget {
  const AlumniApp({super.key});

  @override
  ConsumerState<AlumniApp> createState() => _AlumniAppState();
}

class _AlumniAppState extends ConsumerState<AlumniApp> with WidgetsBindingObserver {
  
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    // 82.39: SessionManager (sessionProvider) is the single inactivity-timeout owner now.
    // Resuming from background just asks it to re-check immediately, since its own
    // Timer.periodic can miss ticks that should have fired while the app was suspended.
    if (state == AppLifecycleState.resumed) {
      ref.read(sessionProvider.notifier).checkNow();
    }
  }

  @override
  Widget build(BuildContext context) {
    final router = ref.watch(routerProvider);
    final locale = ref.watch(languageProvider);
    final branding = ref.watch(orgBrandingProvider);
    final theme = AppTheme.buildTheme(branding);

    // Firebase.apps can be empty if init above failed; guard before touching push setup.
    try {
      if (Firebase.apps.isNotEmpty) {
        ref.read(pushNotificationServiceProvider).initialize();
      }
    } catch (e) {
      debugPrint('Push notification service init failed: $e');
    }
    
    return Listener(
      behavior: HitTestBehavior.translucent,
      onPointerDown: (_) => ref.read(sessionProvider.notifier).userActivityDetected(),
      child: MaterialApp.router(
        title: AppConfig.appName,
        theme: theme,
        darkTheme: theme,
        themeMode: ThemeMode.dark,
        locale: locale,
        localizationsDelegates: const [
          GlobalMaterialLocalizations.delegate,
          GlobalWidgetsLocalizations.delegate,
          GlobalCupertinoLocalizations.delegate,
        ],
        supportedLocales: const [
          Locale('en', ''),
          Locale('bn', ''),
        ],
        routerConfig: router,
        debugShowCheckedModeBanner: false,
        builder: (context, child) => ConnectivityAwareWrapper(
          child: child ?? const SizedBox.shrink(),
        ),
      ),
    );
  }
}
