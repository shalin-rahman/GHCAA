import 'dart:convert';
import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/features/admin/admin_service.dart';
import 'package:ghcaa_mobile/screens/admin/fee_config_screen.dart';

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

  // 8.15: the dialog used to send no date, so the API stored 0001-01-01.
  test('FR-42: a new fee sends the picked effective date as yyyy-MM-dd', () {
    final payload = buildFeePayload(null, 500, DateTime(2027, 1, 1));

    expect(payload['effectiveDate'], '2027-01-01');
    expect(payload['id'], 0);
    expect(payload['category'], 0);
    expect(payload['membershipType'], 2);
  });

  test('FR-42: an edit keeps the row id, category, type and given date', () {
    final existing = {'id': 9, 'category': 1, 'membershipType': 4, 'effectiveDate': '2025-07-01T00:00:00.000'};

    final payload = buildFeePayload(existing, 750, DateTime.parse(existing['effectiveDate'] as String));

    expect(payload['id'], 9);
    expect(payload['category'], 1);
    expect(payload['membershipType'], 4);
    expect(payload['amount'], 750);
    expect(payload['effectiveDate'], '2025-07-01');
  });
}
