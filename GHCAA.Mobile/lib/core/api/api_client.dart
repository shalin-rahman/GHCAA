import 'dart:async';

import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../config/app_config.dart';
import '../storage/storage_service.dart';

bool _isRefreshing = false;
final List<Completer<String?>> _refreshWaiters = [];

/// Posts to /auth/refresh-mobile with a plain, non-intercepted Dio instance
/// (to avoid recursive interceptor triggering), persists the rotated tokens,
/// and returns the new access token. If a refresh is already in-flight, this
/// call queues behind it instead of firing a duplicate request.
Future<String?> _refreshAccessToken(StorageService storage, String refreshToken) async {
  if (_isRefreshing) {
    final waiter = Completer<String?>();
    _refreshWaiters.add(waiter);
    return waiter.future;
  }

  _isRefreshing = true;
  try {
    final plainDio = Dio(BaseOptions(baseUrl: AppConfig.apiBaseUrl));
    final response = await plainDio.post(
      '/auth/refresh-mobile',
      data: {'refreshToken': refreshToken},
    );

    final newToken = response.data['token'] as String?;
    final newRefreshToken = response.data['refreshToken'] as String?;
    if (newToken == null) throw Exception('Refresh response missing token.');

    await storage.saveToken(newToken);
    if (newRefreshToken != null) await storage.saveRefreshToken(newRefreshToken);

    for (final waiter in _refreshWaiters) {
      waiter.complete(newToken);
    }
    _refreshWaiters.clear();
    return newToken;
  } catch (e) {
    for (final waiter in _refreshWaiters) {
      waiter.complete(null);
    }
    _refreshWaiters.clear();
    rethrow;
  } finally {
    _isRefreshing = false;
  }
}

final dioProvider = Provider<Dio>((ref) {
  final dio = Dio(
    BaseOptions(
      baseUrl: AppConfig.apiBaseUrl,
      connectTimeout: const Duration(seconds: 15),
      receiveTimeout: const Duration(seconds: 15),
      headers: {
        'Content-Type': 'application/json',
      },
      // Rely on Dio's default validateStatus (200-299) to ensure 4xx errors throw correctly and trigger catch blocks
    ),
  );

  final storage = ref.read(storageServiceProvider);

  dio.interceptors.add(
    InterceptorsWrapper(
      onRequest: (options, handler) async {
        final token = await storage.getToken();
        if (token != null) {
          options.headers['Authorization'] = 'Bearer $token';
        }
        return handler.next(options);
      },
      onError: (DioException e, handler) async {
        final requestPath = e.requestOptions.path;
        final isAuthEndpoint = requestPath.contains('/auth/login') ||
            requestPath.contains('/auth/refresh-mobile') ||
            requestPath.contains('/auth/google') ||
            requestPath.contains('/auth/facebook');

        if (e.response?.statusCode == 401 && !isAuthEndpoint) {
          final refreshToken = await storage.getRefreshToken();
          if (refreshToken != null) {
            try {
              final newToken = await _refreshAccessToken(storage, refreshToken);
              if (newToken != null) {
                final retryOptions = e.requestOptions;
                retryOptions.headers['Authorization'] = 'Bearer $newToken';
                final plainDio = Dio(BaseOptions(baseUrl: AppConfig.apiBaseUrl));
                final retryResponse = await plainDio.fetch(retryOptions);
                return handler.resolve(retryResponse);
              }
            } catch (_) {
              // Fall through to session-expired handling below.
            }
          }
          await storage.clearAll();
        }

        String message = 'The GHCAA portal encountered a connection hiccup.';

        // 82.4: the API now returns one error shape (RFC 7807 ProblemDetails), so 'detail'
        // (falling back to 'title') is where a server-supplied message lives. 'message' is kept
        // as a fallback for any response still on the old ad-hoc shape.
        if (e.response?.data is Map) {
          final data = e.response?.data as Map;
          final serverMessage = data['detail'] ?? data['title'] ?? data['message'];
          if (serverMessage != null) {
            message = serverMessage.toString();
          }
        } else if (e.response?.data is String && (e.response?.data as String).isNotEmpty) {
          message = e.response?.data;
        } else {
          // Otherwise map common Dio/HTTP failures to a friendly message.
          if (e.type == DioExceptionType.connectionTimeout ||
              e.type == DioExceptionType.receiveTimeout) {
            message = 'The server is taking too long to respond. Please check your internet.';
          } else if (e.type == DioExceptionType.connectionError) {
            message = 'Unable to reach the GHCAA API Engine. Is it running?';
          } else if (e.response?.statusCode == 401) {
            message = 'Your session has expired. Please sign in again for security.';
          } else if (e.response?.statusCode == 403) {
            message = 'You do not have the required permissions for this action.';
          } else if (e.response?.statusCode == 404) {
            message = 'The requested information was not found on the server.';
          } else if (e.response?.statusCode == 500) {
            message = 'Our servers are experiencing a temporary issue. We are on it!';
          }
        }

        // Create a new exception with the friendly message
        final friendlyException = DioException(
          requestOptions: e.requestOptions,
          response: e.response,
          type: e.type,
          error: message,
          message: message,
        );

        return handler.next(friendlyException);
      },
    ),
  );

  return dio;
});
