import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:sentry_flutter/sentry_flutter.dart';
import 'core/config/app_config.dart';
import 'core/theme/app_theme.dart';
import 'core/router/app_router.dart';
import 'features/notifications/push_notification_service.dart'; // Keep this import

void main() async {
  // 1. Ensure Flutter binding is valid
  WidgetsFlutterBinding.ensureInitialized();
  
  // 2. Load Environment Config
  await dotenv.load(fileName: ".env");

  final dsn = dotenv.env['SENTRY_DSN'];
  
  if (dsn != null && dsn.isNotEmpty && dsn != 'https://example@sentry.io/project') {
    // 3. Initialize Sentry Observability (Industry Standard)
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
  const bool isWeb = bool.fromEnvironment('dart.library.js_util'); // Simple web check
  
  if (!isWeb) {
    try {
      await Firebase.initializeApp();
      try {
        FirebaseMessaging.onBackgroundMessage(PushNotificationService.firebaseMessagingBackgroundHandler);
      } catch (_) {}
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

class HaragangianApp extends ConsumerWidget {
  const HaragangianApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final router = ref.watch(routerProvider);
    
    // 5. Initialize Push Notifications on startup (Defensive check)
    try {
      if (Firebase.apps.isNotEmpty) {
        ref.read(pushNotificationServiceProvider).initialize();
      }
    } catch (_) {}
    
    return MaterialApp.router(
      title: AppConfig.appName,
      theme: AppTheme.darkTheme,
      darkTheme: AppTheme.darkTheme,
      themeMode: ThemeMode.dark,
      routerConfig: router,
      debugShowCheckedModeBanner: false,
    );
  }
}
