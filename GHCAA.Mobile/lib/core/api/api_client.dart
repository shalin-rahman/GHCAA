import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../config/app_config.dart';

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

  dio.interceptors.add(
    InterceptorsWrapper(
      onRequest: (options, handler) async {
        final prefs = await SharedPreferences.getInstance();
        final token = prefs.getString('jwt_token');
        if (token != null) {
          options.headers['Authorization'] = 'Bearer $token';
        }
        return handler.next(options);
      },
      onError: (DioException e, handler) {
        String message = 'The GHCAA portal encountered a connection hiccup.';
        
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
