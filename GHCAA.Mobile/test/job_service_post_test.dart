import 'dart:convert';
import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/features/jobs/job_service.dart';

// Answers every request with one status and JSON body.
class _FixedAdapter implements HttpClientAdapter {
  _FixedAdapter(this.status, this.body);

  final int status;
  final Object body;

  @override
  void close({bool force = false}) {}

  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<Uint8List>? requestStream,
    Future<void>? cancelFuture,
  ) async {
    return ResponseBody.fromString(jsonEncode(body), status,
        headers: {
          Headers.contentTypeHeader: [Headers.jsonContentType],
        });
  }
}

void main() {
  group('84.56: JobService.postJob', () {
    test('returns null on success', () async {
      final adapter = _FixedAdapter(201, {'id': 1});
      final service = JobService(Dio()..httpClientAdapter = adapter);

      expect(await service.postJob({'title': 't'}), isNull);
    });

    test('returns the first validation message from a 400', () async {
      final adapter = _FixedAdapter(400, {
        'title': 'One or more validation errors occurred.',
        'errors': {
          'ApplicationLink': ['The ApplicationLink field is not a valid URL.'],
        },
      });
      final service = JobService(Dio()..httpClientAdapter = adapter);

      expect(await service.postJob({'title': 't'}),
          'The ApplicationLink field is not a valid URL.');
    });

    test('falls back to the fixed text when the body has no message', () async {
      final adapter = _FixedAdapter(500, {'status': 500});
      final service = JobService(Dio()..httpClientAdapter = adapter);

      expect(await service.postJob({'title': 't'}), JobService.postJobFailed);
    });
  });
}
