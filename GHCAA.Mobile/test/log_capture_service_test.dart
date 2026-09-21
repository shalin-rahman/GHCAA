import 'dart:io';

import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/logging/log_capture_service.dart';
import 'package:path_provider_platform_interface/path_provider_platform_interface.dart';

// 12.3: rotating in-app log capture backing the 12.4 error-report/share-logs
// flow. Follows file_service_test.dart's path_provider fake pattern.
class _FakeDocsDirPathProvider extends PathProviderPlatform {
  final Directory docsDir;
  _FakeDocsDirPathProvider(this.docsDir);

  @override
  Future<String?> getApplicationDocumentsPath() async => docsDir.path;
}

void main() {
  late Directory docsDir;
  late LogCaptureService service;

  setUp(() async {
    docsDir = await Directory.systemTemp.createTemp('ghcaa_log_capture_test');
    PathProviderPlatform.instance = _FakeDocsDirPathProvider(docsDir);
    service = LogCaptureService();
  });

  tearDown(() async {
    if (await docsDir.exists()) await docsDir.delete(recursive: true);
  });

  test('tail() reports no entries before anything is logged', () async {
    expect(await service.tail(), '(no log entries)');
  });

  test('logApiError writes a line containing the method, path, and message', () async {
    await service.logApiError(method: 'GET', path: '/events', statusCode: 500, message: 'boom');

    final tail = await service.tail();
    expect(tail, contains('GET /events'));
    expect(tail, contains('500'));
    expect(tail, contains('boom'));
  });

  test('logEvent appends alongside logApiError entries', () async {
    await service.logApiError(method: 'GET', path: '/events', statusCode: 500, message: 'boom');
    await service.logEvent('user logged in');

    final tail = await service.tail();
    expect(tail, contains('boom'));
    expect(tail, contains('user logged in'));
  });

  test('tail() returns only the last maxChars characters', () async {
    for (var i = 0; i < 50; i++) {
      await service.logEvent('event number $i');
    }

    final tail = await service.tail(maxChars: 100);
    expect(tail.length, lessThanOrEqualTo(100));
    expect(tail, contains('event number 49'));
  });

  test('rotates the log file once it exceeds the size cap', () async {
    final longMessage = 'x' * 2000;
    for (var i = 0; i < 200; i++) {
      await service.logEvent('ENTRY$i:$longMessage');
    }

    final file = await service.logFileForSharing();
    expect(file, isNotNull);
    final length = await file!.length();
    expect(length, lessThan(300 * 1024));
    // The earliest entries should have been dropped by rotation.
    final content = await file.readAsString();
    expect(content, isNot(contains('ENTRY0:')));
    expect(content, contains('ENTRY199:$longMessage'));
  });

  test('logFileForSharing returns null when nothing has been logged', () async {
    expect(await service.logFileForSharing(), isNull);
  });
}
