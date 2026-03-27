import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:sentry_flutter/sentry_flutter.dart';
import 'core/config/app_config.dart';
import 'core/theme/app_theme.dart';
import 'core/router/app_router.dart';
import 'features/notifications/push_notification_service.dart';

void main() async {
  // 1. Ensure Flutter binding is valid
  WidgetsFlutterBinding.ensureInitialized();
  
  // 2. Load Environment Config
  await dotenv.load(fileName: ".env");

  // 3. Initialize Sentry Observability (Industry Standard)
  await SentryFlutter.init(
    (options) {
      options.dsn = dotenv.env['SENTRY_DSN'] ?? 'https://example@sentry.io/project';
      options.tracesSampleRate = 1.0; // Captures all performance traces for development
      options.environment = AppConfig.environment;
    },
    appRunner: () async {
      // 4. Initialize Cloud Infrastructure (Firebase)
      try {
        await Firebase.initializeApp();
        FirebaseMessaging.onBackgroundMessage(PushNotificationService.firebaseMessagingBackgroundHandler);
      } catch (e) {
        debugPrint('Cloud Notification Service Initialization Failed: $e');
        // This fails if google-services.json is missing, but app continues for local testing
      }
      
      runApp(
        const ProviderScope(
          child: HaragangianApp(),
        ),
      );
    },
  );
}

class HaragangianApp extends ConsumerWidget {
  const HaragangianApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final router = ref.watch(routerProvider);
    
    // 5. Initialize Push Notifications on startup
    ref.read(pushNotificationServiceProvider).initialize();
    
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
