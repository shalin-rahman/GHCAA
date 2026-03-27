import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'dart:developer' as developer;

final pushNotificationServiceProvider = Provider<PushNotificationService>((ref) => PushNotificationService());

class PushNotificationService {
  FirebaseMessaging messaging = FirebaseMessaging.instance;

  Future<void> initialize() async {
    // 1. Request Permission
    NotificationSettings settings = await messaging.requestPermission(
      alert: true,
      badge: true,
      sound: true,
    );

    if (settings.authorizationStatus == AuthorizationStatus.authorized) {
      developer.log('User granted notification permission');
    }

    // 2. Get Token (Send this to your backend api/notifications/token)
    String? token = await messaging.getToken();
    developer.log('Registration Token: $token');

    // 3. Handle Foreground Messages
    FirebaseMessaging.onMessage.listen((RemoteMessage message) {
      developer.log('Received foreground message: ${message.notification?.title}');
      // You can show a local snackbar or alert here
    });

    // 4. Handle Interaction when app is in background but opened via notification
    FirebaseMessaging.onMessageOpenedApp.listen((RemoteMessage message) {
      developer.log('App opened via notification: ${message.data}');
      // Navigation logic can be added here
    });
  }

  // Mandatory background handler (static/top-level)
  static Future<void> firebaseMessagingBackgroundHandler(RemoteMessage message) async {
    await Firebase.initializeApp();
    developer.log('Handling background message: ${message.messageId}');
  }
}
