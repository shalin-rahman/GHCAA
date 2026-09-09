import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:firebase_core/firebase_core.dart';
import 'package:sentry_flutter/sentry_flutter.dart';
import 'core/config/app_config.dart';
import 'core/theme/app_theme.dart';
import 'core/router/app_router.dart';
import 'core/session/session_manager.dart';
import 'features/notifications/push_notification_service.dart';
import 'core/widgets/no_internet_banner.dart';
import 'core/services/app_localizations.dart';
import 'core/services/org_config_service.dart';
import 'core/bootstrap/error_handlers.dart';
import 'core/bootstrap/firebase_bootstrap.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  // Error handlers must be installed before SentryFlutter.init so Sentry's
  // integrations chain onto them rather than replacing them.
  setupErrorHandlers();

  await dotenv.load(fileName: ".env");

  final dsn = dotenv.env['SENTRY_DSN'];

  if (dsn != null && dsn.isNotEmpty && dsn != 'https://example@sentry.io/project') {
    await SentryFlutter.init(
      (options) {
        options.dsn = dsn;
        options.tracesSampleRate = 1.0;
        options.environment = AppConfig.environment;
      },
      appRunner: () => _initAndRunApp(),
    );
  } else {
    debugPrint('Sentry Observability Offline: No valid DSN provided.');
    _initAndRunApp();
  }
}

Future<void> _initAndRunApp() async {
  await initFirebase();
  runApp(const ProviderScope(child: AlumniApp()));
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
    // 82.39: SessionManager is the single inactivity-timeout owner.
    // Re-check immediately on resume since Timer.periodic can miss ticks
    // that fired while the app was suspended.
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

    // Firebase.apps is empty if init failed; guard before touching push setup.
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
