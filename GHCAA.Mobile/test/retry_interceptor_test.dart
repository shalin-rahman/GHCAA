import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/api/retry_interceptor.dart';

/// Adapter that fails [failuresBeforeSuccess] times before returning 200,
/// each failure shaped by [failureMode]. Counts every call it sees so tests
/// can assert exactly how many attempts the interceptor made.
class _ScriptedAdapter implements HttpClientAdapter {
  _ScriptedAdapter({required this.failuresBeforeSuccess, required this.failureMode});

  final int failuresBeforeSuccess;
  final String failureMode; // '503' | '400' | 'timeout' | 'connectionError'
  int callCount = 0;

  @override
  void close({bool force = false}) {}

  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<Uint8List>? requestStream,
    Future<void>? cancelFuture,
  ) async {
    callCount++;
    if (callCount <= failuresBeforeSuccess) {
      switch (failureMode) {
        case 'timeout':
          throw DioException(
            requestOptions: options,
            type: DioExceptionType.connectionTimeout,
          );
        case 'connectionError':
          throw DioException(
            requestOptions: options,
            type: DioExceptionType.connectionError,
          );
        case '400':
          return ResponseBody.fromString('{"detail":"bad request"}', 400,
              headers: {Headers.contentTypeHeader: ['application/json']});
        case '503':
        default:
          return ResponseBody.fromString('{"detail":"unavailable"}', 503,
              headers: {Headers.contentTypeHeader: ['application/json']});
      }
    }
    return ResponseBody.fromString('{"ok":true}', 200,
        headers: {Headers.contentTypeHeader: ['application/json']});
  }
}

Dio _buildDio(_ScriptedAdapter adapter, {int maxRetries = 3}) {
  final dio = Dio(BaseOptions(baseUrl: 'https://api.test'));
  dio.httpClientAdapter = adapter;
  dio.interceptors.add(RetryInterceptor(
    dio: dio,
    maxRetries: maxRetries,
    baseDelay: const Duration(milliseconds: 1),
  ));
  return dio;
}

void main() {
  group('RetryInterceptor', () {
    test('retries on a 5xx response and succeeds once the server recovers', () async {
      final adapter = _ScriptedAdapter(failuresBeforeSuccess: 2, failureMode: '503');
      final dio = _buildDio(adapter);

      final response = await dio.get('/ping');

      expect(response.statusCode, 200);
      expect(adapter.callCount, 3); // 2 failures + 1 success
    });

    test('retries on connection timeout', () async {
      final adapter = _ScriptedAdapter(failuresBeforeSuccess: 1, failureMode: 'timeout');
      final dio = _buildDio(adapter);

      final response = await dio.get('/ping');

      expect(response.statusCode, 200);
      expect(adapter.callCount, 2);
    });

    test('retries on a connection error', () async {
      final adapter = _ScriptedAdapter(failuresBeforeSuccess: 1, failureMode: 'connectionError');
      final dio = _buildDio(adapter);

      final response = await dio.get('/ping');

      expect(response.statusCode, 200);
      expect(adapter.callCount, 2);
    });

    test('does not retry a 4xx client error', () async {
      final adapter = _ScriptedAdapter(failuresBeforeSuccess: 5, failureMode: '400');
      final dio = _buildDio(adapter);

      await expectLater(dio.get('/ping'), throwsA(isA<DioException>()));
      expect(adapter.callCount, 1);
    });

    test('gives up after maxRetries and surfaces the failure', () async {
      final adapter = _ScriptedAdapter(failuresBeforeSuccess: 100, failureMode: '503');
      final dio = _buildDio(adapter, maxRetries: 3);

      await expectLater(dio.get('/ping'), throwsA(isA<DioException>()));
      expect(adapter.callCount, 4); // initial attempt + 3 retries, all failing
    });
  });
}
