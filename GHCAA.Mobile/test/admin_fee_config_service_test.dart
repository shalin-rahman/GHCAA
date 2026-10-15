import 'dart:convert';
import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/features/admin/admin_service.dart';

class _FeeConfigAdapter implements HttpClientAdapter {
  int status = 200;

  @override
  void close({bool force = false}) {}

  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<Uint8List>? requestStream,
    Future<void>? cancelFuture,
  ) async {
    return ResponseBody.fromString(
      jsonEncode(status == 200
          ? {'id': 1}
          : {'detail': 'A Annual fee for General effective 2026-01-01 already exists.'}),
      status,
      headers: {
        Headers.contentTypeHeader: ['application/json'],
      },
    );
  }
}

void main() {
  // 62.56g: these used to swallow the error and return false, so the dialog stayed open with no
  // reason. They now rethrow and the screen shows the server's detail.
  test('FR-42: addFeeConfig rethrows a 400 with the server detail', () async {
    final adapter = _FeeConfigAdapter()..status = 400;
    final service = AdminService(Dio()..httpClientAdapter = adapter);

    final call = service.addFeeConfig({'category': 'Annual'});

    await expectLater(
      call,
      throwsA(isA<DioException>()
          .having((e) => e.response?.statusCode, 'status', 400)
          .having((e) => e.response?.data['detail'], 'detail', contains('already exists'))),
    );
  });

  test('FR-42: updateFeeConfig rethrows a 400 with the server detail', () async {
    final adapter = _FeeConfigAdapter()..status = 400;
    final service = AdminService(Dio()..httpClientAdapter = adapter);

    await expectLater(service.updateFeeConfig({'id': 1}), throwsA(isA<DioException>()));
  });

  test('FR-42: addFeeConfig returns true on success', () async {
    final service = AdminService(Dio()..httpClientAdapter = _FeeConfigAdapter());

    expect(await service.addFeeConfig({'category': 'Annual'}), isTrue);
  });
}
