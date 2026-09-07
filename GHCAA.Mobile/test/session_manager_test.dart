import 'package:dio/dio.dart';
import 'package:flutter/services.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/api/api_client.dart';
import 'package:ghcaa_mobile/core/session/session_manager.dart';
import 'package:ghcaa_mobile/core/storage/storage_service.dart';
import 'package:shared_preferences/shared_preferences.dart';

// 82.39: SessionManager (sessionProvider) used to be one of two independent inactivity-timeout
// mechanisms (this one at 15 minutes, main.dart's own _checkInactivity at 10). These tests pin
// SessionManager as the single owner: activity resets its clock, and once inactivityLimit has
// elapsed it clears the stored session the same way an explicit logout does.
class _NoopAdapter implements HttpClientAdapter {
  @override
  void close({bool force = false}) {}

  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<Uint8List>? requestStream,
    Future<void>? cancelFuture,
  ) async {
    throw UnimplementedError('No network calls expected in session_manager_test: ${options.path}');
  }
}

void main() {
  late ProviderContainer container;

  setUp(() {
    dotenv.testLoad(fileInput: 'BASE_API_URL=http://localhost:5087/api');
    SharedPreferences.setMockInitialValues({'jwt_token': 'stale-legacy-value'});

    final secureStore = <String, String>{'jwt_token': 'test-token'};
    TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger
        .setMockMethodCallHandler(
      const MethodChannel('plugins.it_nomads.com/flutter_secure_storage'),
      (call) async {
        final key = call.arguments is Map ? call.arguments['key'] as String? : null;
        switch (call.method) {
          case 'write':
            secureStore[key!] = call.arguments['value'] as String;
            return null;
          case 'read':
            return secureStore[key];
          case 'delete':
            secureStore.remove(key);
            return null;
          case 'readAll':
            return secureStore;
          case 'deleteAll':
            secureStore.clear();
            return null;
          default:
            return null;
        }
      },
    );

    container = ProviderContainer();
    container.read(dioProvider).httpClientAdapter = _NoopAdapter();
  });

  tearDown(() => container.dispose());

  test('userActivityDetected keeps checkNow() from expiring the session', () async {
    final manager = container.read(sessionProvider.notifier);
    manager.userActivityDetected();

    manager.checkNow();

    expect(await container.read(storageServiceProvider).getToken(), isNotNull);
  });

  test('checkNow() clears the stored session once inactivityLimit has elapsed', () async {
    final manager = container.read(sessionProvider.notifier);

    // Simulate the last recorded activity being older than the inactivity limit, the
    // same condition the periodic timer or an app-resume check would observe.
    container.read(sessionProvider.notifier).state =
        DateTime.now().subtract(SessionManager.inactivityLimit + const Duration(seconds: 1));

    manager.checkNow();
    // _handleSessionExpiry awaits AuthService.logout(), which is fire-and-forgotten from
    // checkNow(); give the microtask queue a turn to let it finish.
    await Future<void>.delayed(Duration.zero);

    expect(await container.read(storageServiceProvider).getToken(), isNull);
  });
}
