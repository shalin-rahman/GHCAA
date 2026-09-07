import 'package:dio/dio.dart';
import 'package:flutter/services.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/api/api_client.dart';
import 'package:ghcaa_mobile/core/storage/storage_service.dart';
import 'package:ghcaa_mobile/features/notifications/push_notification_service.dart';
import 'package:shared_preferences/shared_preferences.dart';

// 82.41: the FCM device token used to only be logged, never sent to the backend, so no
// targeted push could reach a specific member's device. These tests cover the two pieces
// that don't require the Firebase plugin itself: the device-token registration call
// (skipped while logged out, posted while logged in) and the tap-to-navigate route
// resolution (server names the destination via data.route).
class _DeviceTokenAdapter implements HttpClientAdapter {
  final List<RequestOptions> requests = [];

  @override
  void close({bool force = false}) {}

  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<Uint8List>? requestStream,
    Future<void>? cancelFuture,
  ) async {
    requests.add(options);
    return ResponseBody.fromString('{}', 200, headers: {
      Headers.contentTypeHeader: ['application/json'],
    });
  }
}

void main() {
  group('resolveRouteFromMessageData', () {
    test('returns the route named in the payload', () {
      expect(
        PushNotificationService.resolveRouteFromMessageData({'route': '/dashboard'}),
        '/dashboard',
      );
    });

    test('returns null when no route is present', () {
      expect(PushNotificationService.resolveRouteFromMessageData({'title': 'Hello'}), isNull);
    });

    test('returns null for a blank route', () {
      expect(PushNotificationService.resolveRouteFromMessageData({'route': ''}), isNull);
    });
  });

  group('registerDeviceToken', () {
    late ProviderContainer container;
    late _DeviceTokenAdapter adapter;

    setUp(() {
      dotenv.testLoad(fileInput: 'BASE_API_URL=http://localhost:5087/api');
      SharedPreferences.setMockInitialValues({});
      TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger
          .setMockMethodCallHandler(
        const MethodChannel('plugins.it_nomads.com/flutter_secure_storage'),
        (call) async {
          if (call.method == 'read') return null; // no session token stored by default
          return null;
        },
      );

      container = ProviderContainer();
      adapter = _DeviceTokenAdapter();
      container.read(dioProvider).httpClientAdapter = adapter;
    });

    tearDown(() => container.dispose());

    test('skips the request while logged out', () async {
      final service = container.read(pushNotificationServiceProvider);
      await service.registerDeviceToken('device-token-123');
      expect(adapter.requests, isEmpty);
    });

    test('posts the token once a session token is stored', () async {
      await container.read(storageServiceProvider).saveToken('session-token');

      final service = container.read(pushNotificationServiceProvider);
      await service.registerDeviceToken('device-token-123');

      expect(adapter.requests, hasLength(1));
      expect(adapter.requests.single.path, contains('/notifications/device-token'));
      expect(adapter.requests.single.data['token'], 'device-token-123');
    });
  });
}
