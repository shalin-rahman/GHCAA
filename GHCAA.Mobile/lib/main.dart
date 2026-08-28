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
import 'features/auth/auth_service.dart';
import 'features/notifications/push_notification_service.dart'; // Keep this import
import 'core/widgets/no_internet_banner.dart';
import 'core/services/app_localizations.dart';
import 'core/services/org_config_service.dart';

void main() async {
  // 1. Ensure Flutter binding is valid
  WidgetsFlutterBinding.ensureInitialized();

  // 1b. Global Error UI (World-Class Redirection)
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
  
  // 2b. Catch background/untracked errors. Installed BEFORE SentryFlutter.init below —
  // sentry_flutter's OnErrorIntegration/FlutterErrorIntegration CHAIN to whatever handler is
  // already installed at init time rather than replacing it, so installing after init would
  // silently detach Sentry's own crash classification (unhandled vs handled), silent-error
  // filtering, and context collection, on top of firing once per frame for a persistent error.
  PlatformDispatcher.instance.onError = (error, stack) {
    debugPrint('Uncaught platform error: $error');
    return false; // let Sentry's chained default handler (installed by init, below) also run
  };

  // 2c. Catch framework errors (build/layout/paint) automatically instead of relying
  // on the user tapping "DIAGNOSE & REPORT" on the ErrorWidget fallback screen. Also installed
  // before init, for the same chaining reason.
  final defaultFlutterOnError = FlutterError.onError;
  FlutterError.onError = (FlutterErrorDetails details) {
    debugPrint('Uncaught Flutter framework error: ${details.exceptionAsString()}');
    defaultFlutterOnError?.call(details);
  };

  // 3. Load Environment Config
  await dotenv.load(fileName: ".env");

  final dsn = dotenv.env['SENTRY_DSN'];

  if (dsn != null && dsn.isNotEmpty && dsn != 'https://example@sentry.io/project') {
    // 4. Initialize Sentry Observability (Industry Standard)
    await SentryFlutter.init(
      (options) {
        options.dsn = dsn;
        options.tracesSampleRate = 1.0; // Captures all performance traces for development
        options.environment = AppConfig.environment;
      },
      appRunner: () => _initAndRunApp(),
    );
  } else {
    // Graceful fallback for local development or missing config
    debugPrint('Sentry Observability Offline: No valid DSN provided.');
    _initAndRunApp();
  }
}

Future<void> _initAndRunApp() async {
  // 4. Initialize Cloud Infrastructure (Firebase)
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
      child: HaragangianApp(),
    ),
  );
}

class HaragangianApp extends ConsumerStatefulWidget {
  const HaragangianApp({super.key});

  @override
  ConsumerState<HaragangianApp> createState() => _HaragangianAppState();
}

class _HaragangianAppState extends ConsumerState<HaragangianApp> with WidgetsBindingObserver {
  
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
    if (state == AppLifecycleState.resumed) {
      _checkInactivity();
    }
  }

  void _checkInactivity() async {
    final lastActivity = ref.read(lastActivityProvider);
    const limit = Duration(minutes: 10);
    if (DateTime.now().difference(lastActivity) > limit) {
      final auth = ref.read(authServiceProvider);
      // Ensure we only logout if already authenticated
      final role = await auth.getRole();
      if (role != null) {
        await auth.logout();
      }
    } else {
      ref.read(lastActivityProvider.notifier).update();
    }
  }

  @override
  Widget build(BuildContext context) {
    final router = ref.watch(routerProvider);
    final locale = ref.watch(languageProvider);
    final branding = ref.watch(orgBrandingProvider);
    final theme = AppTheme.buildTheme(branding);

    // 5. Initialize Push Notifications on startup (Defensive check)
    try {
      if (Firebase.apps.isNotEmpty) {
        ref.read(pushNotificationServiceProvider).initialize();
      }
    } catch (e) {
      debugPrint('Push notification service init failed: $e');
    }
    
    return Listener(
      behavior: HitTestBehavior.translucent,
      onPointerDown: (_) => ref.read(lastActivityProvider.notifier).update(),
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
