import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/config/app_config.dart';
import 'package:ghcaa_mobile/features/support/support_service.dart';

class _RecordingAdapter implements HttpClientAdapter {
  Uri? lastUri;

  @override
  void close({bool force = false}) {}

  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<Uint8List>? requestStream,
    Future<void>? cancelFuture,
  ) async {
    lastUri = options.uri;
    return ResponseBody.fromString('{}', 200, headers: {
      Headers.contentTypeHeader: ['application/json'],
    });
  }
}

void main() {
  setUpAll(() => dotenv.testLoad(fileInput: 'BASE_API_URL=http://localhost:5087/api'));

  // NFR-R4: the call used to go to /api/health, which never existed, so the support
  // screen always said the system was down.
  test('NFR-R4: checkSystemHealth calls /health at the site root, not under /api', () async {
    final adapter = _RecordingAdapter();
    final dio = Dio(BaseOptions(baseUrl: AppConfig.apiBaseUrl))..httpClientAdapter = adapter;

    final healthy = await SupportService(dio).checkSystemHealth();

    expect(healthy, isTrue);
    expect(adapter.lastUri.toString(), AppConfig.healthUrl);
    expect(adapter.lastUri!.path, '/health');
  });
}
