import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'dart:developer' as developer;

final pushNotificationServiceProvider = Provider<PushNotificationService>((ref) => PushNotificationService());

class PushNotificationService {
  Future<void> initialize() async {
    try {
      FirebaseMessaging messaging = FirebaseMessaging.instance;

      // 1. Request Permission
      NotificationSettings settings = await messaging.requestPermission(
        alert: true,
        badge: true,
        sound: true,
      );

      if (settings.authorizationStatus == AuthorizationStatus.authorized) {
        developer.log('User granted notification permission');
      }

      // 2. Get Token
      String? token = await messaging.getToken();
      developer.log('Registration Token: $token');

      // 3. Listeners
      FirebaseMessaging.onMessage.listen((RemoteMessage message) {
        developer.log('Received foreground message: ${message.notification?.title}');
      });

      FirebaseMessaging.onMessageOpenedApp.listen((RemoteMessage message) {
        developer.log('App opened via notification: ${message.data}');
      });
    } catch (e) {
      developer.log('Push Notification initialization bypassed: $e');
    }
  }

  static Future<void> firebaseMessagingBackgroundHandler(RemoteMessage message) async {
    try {
      if (Firebase.apps.isEmpty) {
        await Firebase.initializeApp();
      }
      developer.log('Handling background message: ${message.messageId}');
    } catch (e) {
      developer.log('PushNotificationService.firebaseMessagingBackgroundHandler failed: $e');
    }
  }
}
