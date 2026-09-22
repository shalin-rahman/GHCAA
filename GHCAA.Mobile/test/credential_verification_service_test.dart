import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/features/credentials/credential_verification_service.dart';

void main() {
  test('verifies a trimmed credential code', () async {
    final dio = Dio()..httpClientAdapter = _Adapter((options) {
      expect(options.path, '/verify/ABCD234567');
      return {
        'valid': true,
        'memberName': 'A Member',
        'membershipType': 'Life',
        'issuedOn': '2026-09-22T00:00:00Z',
        'status': 'Valid',
      };
    });

    final result =
        await CredentialVerificationService(dio).verify(' abcd234567 ');

    expect(result.valid, isTrue);
    expect(result.memberName, 'A Member');
    expect(result.status, 'Valid');
  });
}

class _Adapter implements HttpClientAdapter {
  final Map<String, dynamic> Function(RequestOptions) handler;

  _Adapter(this.handler);

  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<List<int>>? requestStream,
    Future<void>? cancelFuture,
  ) async {
    return ResponseBody.fromString(
      jsonEncode(handler(options)),
      200,
      headers: {'content-type': ['application/json']},
    );
  }

  @override
  void close({bool force = false}) {}
}
