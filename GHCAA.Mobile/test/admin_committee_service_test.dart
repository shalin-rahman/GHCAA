import 'dart:convert';
import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/features/admin/admin_service.dart';

class _CommitteeAdapter implements HttpClientAdapter {
  String? method;
  String? path;
  Object? body;
  int status = 200;

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
    body = options.data;
    return ResponseBody.fromString(
      jsonEncode(status == 200 ? {'message': 'Committee term ended'} : {'detail': 'Pick a reason the term ended.'}),
      status,
      headers: {
        Headers.contentTypeHeader: ['application/json'],
      },
    );
  }
}

void main() {
  // 95.3: the old DELETE ended a term without a reason. The API now only takes this POST.
  test('FR-34: endCommitteeTerm posts the reason, note and notify flag', () async {
    final adapter = _CommitteeAdapter();
    final service = AdminService(Dio()..httpClientAdapter = adapter);

    final ok = await service.endCommitteeTerm(9, reason: 'Other', note: 'Moved abroad', notifyMember: true);

    expect(ok, isTrue);
    expect(adapter.method, 'POST');
    expect(adapter.path, '/admin/governance/members/9/end-term');
    expect(adapter.body, {'reason': 'Other', 'note': 'Moved abroad', 'notifyMember': true});
  });

  test('FR-34: endCommitteeTerm returns false when the API refuses', () async {
    final adapter = _CommitteeAdapter()..status = 409;
    final service = AdminService(Dio()..httpClientAdapter = adapter);

    final ok = await service.endCommitteeTerm(9, reason: 'Resigned');

    expect(ok, isFalse);
  });
}
