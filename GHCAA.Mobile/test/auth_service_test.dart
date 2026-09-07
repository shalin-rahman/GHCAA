import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter/services.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/api/api_client.dart';
import 'package:ghcaa_mobile/core/storage/storage_service.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:shared_preferences/shared_preferences.dart';

// 82.38: canned /auth/login and /profile responses keyed by username/token,
// so login/logout can be exercised against roleProvider and userProfileProvider
// without a real backend.
class _FakeLoginAdapter implements HttpClientAdapter {
  @override
  void close({bool force = false}) {}

  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<Uint8List>? requestStream,
    Future<void>? cancelFuture,
  ) async {
    if (options.path.contains('/auth/login')) {
      final body = options.data as Map;
      final isUserA = body['username'] == 'userA';
      return ResponseBody.fromString(
        jsonEncode({
          'token': isUserA ? 'token-A' : 'token-B',
          'refreshToken': isUserA ? 'refresh-A' : 'refresh-B',
          'role': isUserA ? 'Admin' : 'Member',
        }),
        200,
        headers: {
          Headers.contentTypeHeader: ['application/json'],
        },
      );
    }

    if (options.path.contains('/profile')) {
      final isUserA = options.headers['Authorization'] == 'Bearer token-A';
      return ResponseBody.fromString(
        jsonEncode({'fullName': isUserA ? 'User A' : 'User B'}),
        200,
        headers: {
          Headers.contentTypeHeader: ['application/json'],
        },
      );
    }

    if (options.path.contains('/auth/refresh-mobile')) {
      final submitted = (options.data as Map)['refreshToken'];
      if (submitted != 'valid-refresh-token') {
        return ResponseBody.fromString(
          jsonEncode({'detail': 'Invalid or expired refresh token.'}),
          401,
          headers: {
            Headers.contentTypeHeader: ['application/json'],
          },
        );
      }
      return ResponseBody.fromString(
        jsonEncode({'token': 'refreshed-token', 'refreshToken': 'rotated-refresh-token'}),
        200,
        headers: {
          Headers.contentTypeHeader: ['application/json'],
        },
      );
    }

    throw UnimplementedError('Unhandled path in _FakeLoginAdapter: ${options.path}');
  }
}

void main() {
  late ProviderContainer container;

  setUp(() {
    dotenv.testLoad(fileInput: 'BASE_API_URL=http://localhost:5087/api');
    SharedPreferences.setMockInitialValues({});

    // StorageService reads/writes tokens through flutter_secure_storage, which
    // has no implementation under `flutter test`. Back it with a plain map so
    // saveToken/getToken/removeToken behave like the real plugin instead of
    // throwing MissingPluginException.
    final secureStore = <String, String>{};
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
    container.read(dioProvider).httpClientAdapter = _FakeLoginAdapter();
  });

  tearDown(() => container.dispose());

  test(
    '82.38: switching users never leaks the previous user\'s cached role/profile',
    () async {
      final authService = container.read(authServiceProvider);

      await authService.login('userA', 'pw');
      expect(await container.read(roleProvider.future), 'Admin');
      expect(
        (await container.read(userProfileProvider.future))?['fullName'],
        'User A',
      );

      await authService.logout();
      await authService.login('userB', 'pw');

      expect(await container.read(roleProvider.future), 'Member');
      expect(
        (await container.read(userProfileProvider.future))?['fullName'],
        'User B',
      );
    },
  );

  group('82.40: biometric re-login via loginWithStoredToken', () {
    test('fails cleanly when no refresh token has been saved', () async {
      final authService = container.read(authServiceProvider);
      final error = await authService.loginWithStoredToken();
      expect(error, isNotNull);
    });

    test('rotates the access/refresh tokens on a valid stored refresh token', () async {
      final storage = container.read(storageServiceProvider);
      await storage.saveRefreshToken('valid-refresh-token');

      final authService = container.read(authServiceProvider);
      final error = await authService.loginWithStoredToken();

      expect(error, isNull);
      expect(await storage.getToken(), 'refreshed-token');
      expect(await storage.getRefreshToken(), 'rotated-refresh-token');
    });

    test('clears the stored session when the refresh token was revoked/expired', () async {
      final storage = container.read(storageServiceProvider);
      await storage.saveToken('stale-access-token');
      await storage.saveRefreshToken('revoked-refresh-token');

      final authService = container.read(authServiceProvider);
      final error = await authService.loginWithStoredToken();

      expect(error, isNotNull);
      expect(await storage.getToken(), isNull);
      expect(await storage.getRefreshToken(), isNull);
    });
  });
}
