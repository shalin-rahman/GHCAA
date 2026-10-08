import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';

/// Standardized API exception wrapper for Flutter mobile client with diagnostic logging.
class ApiException implements Exception {
  final String message;
  final int? statusCode;
  final dynamic data;

  ApiException(this.message, {this.statusCode, this.data}) {
    debugPrint('[ApiException] Status: $statusCode | Message: $message | Data: $data');
  }

  /// The message the server put in an error body, or null if there is none. The API answers
  /// with ProblemDetails (82.4), so `detail` then `title` come first; `message` and `error`
  /// cover bodies still on the older ad-hoc shape. Every mobile reader goes through this so
  /// they agree on the order (84.43).
  static String? serverMessage(dynamic data) {
    if (data is! Map) return null;
    for (final key in const ['detail', 'title', 'message', 'error']) {
      final value = data[key];
      if (value != null && value.toString().isNotEmpty) return value.toString();
    }
    return null;
  }

  factory ApiException.fromDioException(DioException error) {
    final response = error.response;
    final statusCode = response?.statusCode;
    String message = 'An unexpected network error occurred.';

    final serverMessage = ApiException.serverMessage(response?.data);
    if (serverMessage != null) {
      message = serverMessage;
    } else if (response?.statusMessage != null && response!.statusMessage!.isNotEmpty) {
      message = response.statusMessage!;
    } else if (error.type == DioExceptionType.connectionTimeout ||
        error.type == DioExceptionType.receiveTimeout ||
        error.type == DioExceptionType.sendTimeout) {
      message = 'Connection timed out. Please check your internet connection.';
    }

    final exception = ApiException(message, statusCode: statusCode, data: response?.data);
    debugPrint('[ApiException.fromDioException] Logged exception: $exception | Request: ${error.requestOptions.path}');
    return exception;
  }

  @override
  String toString() => 'ApiException: $message (status: $statusCode)';
}
