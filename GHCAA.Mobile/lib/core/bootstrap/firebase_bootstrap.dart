import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/foundation.dart' show kIsWeb, debugPrint;
import '../../features/notifications/push_notification_service.dart';

/// Initialises Firebase and registers the FCM background-message handler.
///
/// No-ops on web builds — Firebase native SDKs crash on web when called with
/// a native initialisation path. [kIsWeb] is the compile-time web flag
/// (29E.5: `bool.fromEnvironment('dart.library.js_util')` is NOT set by the
/// toolchain and must not be used here).
Future<void> initFirebase() async {
  if (kIsWeb) {
    debugPrint('Native Cloud Services Skipped for Web Platform.');
    return;
  }

  try {
    await Firebase.initializeApp();
    try {
      FirebaseMessaging.onBackgroundMessage(
          PushNotificationService.firebaseMessagingBackgroundHandler);
    } catch (e) {
      debugPrint('Background message handler registration failed: $e');
    }
  } catch (e) {
    debugPrint('Cloud Notification Service Initialization Failed: $e');
  }
}
