import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'dart:developer' as developer;
import '../../core/api/api_client.dart';
import '../../core/storage/storage_service.dart';
import '../../core/router/app_router.dart';

final pushNotificationServiceProvider = Provider<PushNotificationService>((ref) => PushNotificationService(ref));

class PushNotificationService {
  final Ref _ref;

  PushNotificationService(this._ref);

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

      // 2. Get Token, then register it and keep it registered across rotations.
      String? token = await messaging.getToken();
      developer.log('Registration Token: $token');
      if (token != null) await registerDeviceToken(token);
      messaging.onTokenRefresh.listen(registerDeviceToken);

      // 3. Listeners
      FirebaseMessaging.onMessage.listen((RemoteMessage message) {
        developer.log('Received foreground message: ${message.notification?.title}');
      });

      FirebaseMessaging.onMessageOpenedApp.listen(handleNotificationTap);
    } catch (e) {
      developer.log('Push Notification initialization bypassed: $e');
    }
  }

  // 82.41: the device token used to only be logged, so no targeted push (as opposed to the
  // separate SignalR broadcast/in-app channel) could ever reach a specific member's device.
  // Skips the call while logged out — sending it then would just be a 401 the interceptor
  // has to unwind; initialize() re-runs on every rebuild, so it registers again shortly
  // after login instead.
  Future<void> registerDeviceToken(String token) async {
    try {
      final authToken = await _ref.read(storageServiceProvider).getToken();
      if (authToken == null) return;

      await _ref.read(dioProvider).post('/notifications/device-token', data: {
        'token': token,
        'platform': defaultTargetPlatform.name,
      });
    } catch (e) {
      developer.log('PushNotificationService: device token registration failed: $e');
    }
  }

  /// Pulls the target route out of the message payload. The backend decides where a tap
  /// should land (e.g. a specific event or notification) by putting a go_router path in
  /// `data.route` — the client doesn't maintain its own type-to-route mapping.
  static String? resolveRouteFromMessageData(Map<String, dynamic> data) {
    final route = data['route'];
    if (route is String && route.isNotEmpty) return route;
    return null;
  }

  void handleNotificationTap(RemoteMessage message) {
    developer.log('App opened via notification: ${message.data}');
    final route = resolveRouteFromMessageData(message.data);
    if (route == null) return;
    try {
      _ref.read(routerProvider).go(route);
    } catch (e) {
      developer.log('PushNotificationService: failed to navigate to $route: $e');
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
