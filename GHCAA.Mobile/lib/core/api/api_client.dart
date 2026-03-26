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
      // Keep memory usage low by not storing entire response bodies in case of multi-MB logs
      validateStatus: (status) => status != null && status < 500,
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
        String message = 'System encountered an unexpected error.';
        if (e.type == DioExceptionType.connectionTimeout || e.type == DioExceptionType.receiveTimeout) {
          message = 'Network timeout. Check your connectivity.';
        } else if (e.response?.statusCode == 401) {
          message = 'Session Expired. Please re-authenticate.';
        } else if (e.response?.statusCode == 403) {
          message = 'Access Denied: High-privilege access required.';
        }
        print('API ERROR: $message | ${e.message}');
        return handler.next(e);
      },
    ),
  );

  return dio;
});
