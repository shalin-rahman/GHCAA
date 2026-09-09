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

  factory ApiException.fromDioException(DioException error) {
    final response = error.response;
    final statusCode = response?.statusCode;
    String message = 'An unexpected network error occurred.';

    if (response?.data is Map<String, dynamic>) {
      final map = response!.data as Map<String, dynamic>;
      message = map['message'] ?? map['title'] ?? map['detail'] ?? message;
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
