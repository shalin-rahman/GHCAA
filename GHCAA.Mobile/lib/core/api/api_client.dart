import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../config/app_config.dart';
import '../storage/storage_service.dart';

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
      onError: (DioException e, handler) {
        String message = 'The GHCAA portal encountered a connection hiccup.';
        
        // 1. Check for Backend Error Message (High Priority)
        if (e.response?.data is Map && e.response?.data['message'] != null) {
          message = e.response?.data['message'];
        } else if (e.response?.data is String && (e.response?.data as String).isNotEmpty) {
          message = e.response?.data;
        } else {
          // 2. Fallback to World-Class Status Messages
          if (e.type == DioExceptionType.connectionTimeout || 
              e.type == DioExceptionType.receiveTimeout) {
            message = 'The server is taking too long to respond. Please check your internet.';
          } else if (e.type == DioExceptionType.connectionError) {
            message = 'Unable to reach the GHCAA API Engine. Is it running?';
          } else if (e.response?.statusCode == 401) {
            message = 'Your session has expired. Please sign in again for security.';
            storage.clearAll();
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
