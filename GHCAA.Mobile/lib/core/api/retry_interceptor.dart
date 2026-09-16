import 'dart:math';

import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';

/// Retries transient failures — timeouts, connection errors, and 5xx
/// responses — with exponential backoff. A 4xx (bad request, auth failure,
/// not found, etc.) means the request itself was wrong, so it is never
/// retried; retrying it would just repeat the same failure.
class RetryInterceptor extends Interceptor {
  RetryInterceptor({
    required Dio dio,
    this.maxRetries = 3,
    this.baseDelay = const Duration(milliseconds: 500),
  }) : _dio = dio;

  final Dio _dio;
  final int maxRetries;
  final Duration baseDelay;
  final Random _random = Random();

  static const _retryCountKey = '_retryCount';

  bool _isRetryable(DioException err) {
    switch (err.type) {
      case DioExceptionType.connectionTimeout:
      case DioExceptionType.sendTimeout:
      case DioExceptionType.receiveTimeout:
      case DioExceptionType.connectionError:
        return true;
      default:
        final status = err.response?.statusCode;
        return status != null && status >= 500 && status < 600;
    }
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) async {
    final options = err.requestOptions;
    final attempt = (options.extra[_retryCountKey] as int?) ?? 0;

    if (!_isRetryable(err) || attempt >= maxRetries) {
      return handler.next(err);
    }

    options.extra[_retryCountKey] = attempt + 1;

    final exponential = baseDelay * pow(2, attempt);
    final jitter = Duration(milliseconds: _random.nextInt(150));

    debugPrint(
      '[RetryInterceptor] attempt ${attempt + 1}/$maxRetries for '
      '${options.method} ${options.path} after ${(exponential + jitter).inMilliseconds}ms '
      '(${err.type}${err.response?.statusCode != null ? ', ${err.response?.statusCode}' : ''})',
    );

    await Future.delayed(exponential + jitter);

    try {
      final response = await _dio.fetch(options);
      return handler.resolve(response);
    } on DioException catch (e) {
      // _dio.fetch runs back through this same interceptor, so retries beyond
      // this point are already exhausted by the time an exception surfaces here.
      return handler.next(e);
    } catch (e) {
      return handler.next(err);
    }
  }
}
