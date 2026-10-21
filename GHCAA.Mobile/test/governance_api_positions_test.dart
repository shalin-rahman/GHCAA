import 'dart:convert';
import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/api/governance_api.dart';

class _PositionsAdapter implements HttpClientAdapter {
  String? method;
  String? path;

  @override
  void close({bool force = false}) {}

  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<Uint8List>? requestStream,
    Future<void>? cancelFuture,
  ) async {
    method = options.method;
    path = options.path;
    return ResponseBody.fromString(
      jsonEncode([
        {'id': 1, 'key': 'President', 'name': 'President', 'seatCount': 1},
        {'id': 16, 'key': 'Patron', 'name': 'Patron', 'seatCount': 2},
      ]),
      200,
      headers: {
        Headers.contentTypeHeader: ['application/json'],
      },
    );
  }
}

void main() {
  // 95.2: the admin assign picker lists whatever positions the API holds, admin-added ones included.
  test('FR-34: getPositions reads the public positions list', () async {
    final adapter = _PositionsAdapter();
    final api = GovernanceApi(Dio()..httpClientAdapter = adapter);

    final list = await api.getPositions();

    expect(adapter.method, 'GET');
    expect(adapter.path, '/governance/positions');
    expect(list.map((p) => p['name']), ['President', 'Patron']);
  });
}
