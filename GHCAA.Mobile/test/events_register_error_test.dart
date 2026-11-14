import 'dart:convert';
import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/features/events/events_service.dart';

/// Answers every request with the given status and JSON body.
class _FixedAdapter implements HttpClientAdapter {
  _FixedAdapter(this.status, this.body);
  final int status;
  final Object body;

  @override
  void close({bool force = false}) {}

  @override
  Future<ResponseBody> fetch(RequestOptions options, Stream<Uint8List>? requestStream, Future<void>? cancelFuture) async =>
      ResponseBody.fromString(jsonEncode(body), status, headers: {
        Headers.contentTypeHeader: ['application/problem+json'],
      });
}

void main() {
  // 84.102: a refused registration comes back as 409 with the reason in detail.
  group('EventsService.registerForEvent', () {
    test('keeps the server reason when the API refuses', () async {
      final service = EventsService(Dio()
        ..httpClientAdapter = _FixedAdapter(409, {'status': 409, 'detail': 'You are already registered for this event.'}));

      final ok = await service.registerForEvent(1);

      expect(ok, isFalse);
      expect(service.lastRegisterError, 'You are already registered for this event.');
    });

    test('clears the reason on the next success', () async {
      final service = EventsService(Dio()..httpClientAdapter = _FixedAdapter(409, {'detail': 'This event is for members only.'}));
      await service.registerForEvent(1);
      final dio = Dio()..httpClientAdapter = _FixedAdapter(200, {'id': 5});
      final next = EventsService(dio);
      next.lastRegisterError = 'old';

      expect(await next.registerForEvent(1), isTrue);
      expect(next.lastRegisterError, isNull);
      expect(service.lastRegisterError, 'This event is for members only.');
    });

    test('leaves the reason empty when the reply has none', () async {
      final service = EventsService(Dio()..httpClientAdapter = _FixedAdapter(500, 'oops'));

      expect(await service.registerForEvent(1), isFalse);
      expect(service.lastRegisterError, isNull);
    });
  });
}
