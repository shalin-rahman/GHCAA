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
  /// with ProblemDetails (82.4), so `detail` comes first. A model-validation 400 has no
  /// detail and a generic title, so the first entry of its `errors` map comes before the
  /// title (84.56). `message` and `error` cover bodies on the older ad-hoc shape. Web
  /// `serverErrorMessage` in http.util.ts reads the same order.
  static String? serverMessage(dynamic data) {
    if (data is! Map) return null;
    final detail = _text(data['detail']);
    if (detail != null) return detail;
    // A map of field to messages, or a plain list; web reads both through Object.values.
    final errors = data['errors'];
    final entries = errors is Map ? errors.values : (errors is List ? errors : const []);
    for (final entry in entries) {
      final first = entry is List && entry.isNotEmpty ? _text(entry.first) : _text(entry);
      if (first != null) return first;
    }
    for (final key in const ['title', 'message', 'error']) {
      final value = _text(data[key]);
      if (value != null) return value;
    }
    return null;
  }

  static String? _text(dynamic value) {
    if (value == null || value is Map || value is List) return null;
    final s = value.toString();
    return s.isEmpty ? null : s;
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
