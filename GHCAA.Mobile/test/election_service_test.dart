import 'dart:convert';
import 'dart:typed_data';
import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/features/elections/election_service.dart';

class _ElectionAdapter implements HttpClientAdapter {
  String? method;
  String? path;
  Object? body;

  @override
  void close({bool force = false}) {}

  @override
  Future<ResponseBody> fetch(RequestOptions options,
      Stream<Uint8List>? requestStream, Future<void>? cancelFuture) async {
    method = options.method;
    path = options.path;
    body = options.data;
    if (options.path == '/elections/12') {
      return _json({
        'id': 12,
        'title': 'EC 2026',
        'phase': 'Polling',
        'ecPeriodId': 2,
        'voterCount': 0,
        'eligibleVoterCount': 10
      });
    }
    if (options.path == '/elections/12/nominations') {
      return _json([
        {
          'id': 4,
          'electionSeatId': 3,
          'candidateMemberId': 8,
          'status': 'Accepted',
          'statement': 'For service'
        }
      ]);
    }
    if (options.path == '/elections/12/vote') return _json({}, status: 200);
    throw StateError('Unhandled ${options.method} ${options.path}');
  }

  ResponseBody _json(Object value, {int status = 200}) =>
      ResponseBody.fromString(
        jsonEncode(value),
        status,
        headers: {
          Headers.contentTypeHeader: ['application/json']
        },
      );
}

void main() {
  test('loads an election summary and nominations from the engine routes',
      () async {
    final adapter = _ElectionAdapter();
    final service = ElectionService(Dio()..httpClientAdapter = adapter);

    final election = await service.get(12);
    final nominations = await service.getNominations(12);

    expect(election.title, 'EC 2026');
    expect(nominations.single.candidateMemberId, 8);
    expect(adapter.path, '/elections/12/nominations');
  });

  test('posts the selected nomination as a vote', () async {
    final adapter = _ElectionAdapter();
    final service = ElectionService(Dio()..httpClientAdapter = adapter);

    expect(await service.vote(12, electionSeatId: 3, nominationId: 4), isTrue);
    expect(adapter.method, 'POST');
    expect(adapter.path, '/elections/12/vote');
    expect((adapter.body as Map)['nominationId'], 4);
  });
}
