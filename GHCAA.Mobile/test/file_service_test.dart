import 'dart:convert';
import 'dart:io';
import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/features/files/file_service.dart';
import 'package:path_provider_platform_interface/path_provider_platform_interface.dart';

// 82.33: financial_portal_screen's receipt download and member_details_screen's
// certificate/payment-proof viewer both used to hand an [Authorize]'d URL straight to
// an unauthenticated request (launchUrl / Image.network). These tests exercise the
// authenticated-fetch helper both screens now go through instead.
class _FakeSecureFileAdapter implements HttpClientAdapter {
  @override
  void close({bool force = false}) {}

  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<Uint8List>? requestStream,
    Future<void>? cancelFuture,
  ) async {
    final hasAuthHeader = options.headers['Authorization'] == 'Bearer test-token';

    if (options.path.contains('/financials/receipt/') || options.path.contains('/secure-files/')) {
      if (!hasAuthHeader) {
        return ResponseBody.fromString('Unauthorized', 401);
      }
      if (options.path.contains('missing')) {
        return ResponseBody.fromString('Not found', 404);
      }
      return ResponseBody.fromBytes(utf8.encode('pdf-bytes'), 200);
    }

    throw UnimplementedError('Unhandled path in _FakeSecureFileAdapter: ${options.path}');
  }
}

class _FakeTempDirPathProvider extends PathProviderPlatform {
  final Directory tempDir;
  _FakeTempDirPathProvider(this.tempDir);

  @override
  Future<String?> getTemporaryPath() async => tempDir.path;
}

void main() {
  late Dio dio;
  late FileService fileService;
  late Directory tempDir;

  setUp(() async {
    dio = Dio(BaseOptions(headers: {'Authorization': 'Bearer test-token'}));
    dio.httpClientAdapter = _FakeSecureFileAdapter();
    fileService = FileService(dio);

    tempDir = await Directory.systemTemp.createTemp('ghcaa_file_service_test');
    PathProviderPlatform.instance = _FakeTempDirPathProvider(tempDir);
  });

  tearDown(() async {
    if (await tempDir.exists()) await tempDir.delete(recursive: true);
  });

  group('fetchAuthenticatedBytes', () {
    test('returns bytes when the request carries the auth header', () async {
      final bytes = await fileService.fetchAuthenticatedBytes('/financials/receipt/5');
      expect(bytes, isNotNull);
      expect(utf8.decode(bytes!), 'pdf-bytes');
    });

    test('returns null instead of throwing when the resource is missing', () async {
      final bytes = await fileService.fetchAuthenticatedBytes('/secure-files/missing/file.jpg');
      expect(bytes, isNull);
    });

    test('returns null instead of throwing when the request is unauthenticated', () async {
      final unauthDio = Dio()..httpClientAdapter = _FakeSecureFileAdapter();
      final unauthService = FileService(unauthDio);
      final bytes = await unauthService.fetchAuthenticatedBytes('/financials/receipt/5');
      expect(bytes, isNull);
    });
  });

  group('downloadToTempFile', () {
    test('writes the fetched bytes to a temp file and returns it', () async {
      final file = await fileService.downloadToTempFile('/financials/receipt/5', fileName: 'receipt_5.pdf');
      expect(file, isNotNull);
      expect(await file!.exists(), isTrue);
      expect(await file.readAsString(), 'pdf-bytes');
    });

    test('returns null without touching disk when the fetch fails', () async {
      final file = await fileService.downloadToTempFile('/secure-files/missing/file.jpg', fileName: 'missing.jpg');
      expect(file, isNull);
    });
  });
}
