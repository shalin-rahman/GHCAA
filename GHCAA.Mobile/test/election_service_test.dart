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
    if (options.path == '/elections/12/vote') {
      return _json({'trackingCode': 'ABCD-EFGH'});
    }
    // The phase change runs at once. Declare waits for a second person (spec 023, 37.12f).
    if (options.path == '/elections/12/phase') {
      return ResponseBody.fromString('', 200);
    }
    if (options.path == '/elections/12/declare') {
      return _json({'id': 30, 'action': 'Declare'}, status: 202);
    }
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

  test('posts the whole ballot and returns the tracking code', () async {
    final adapter = _ElectionAdapter();
    final service = ElectionService(Dio()..httpClientAdapter = adapter);

    final code = await service.castBallot(12, {
      3: [4],
      5: [],
    });

    expect(code, 'ABCD-EFGH');
    expect(adapter.method, 'POST');
    expect(adapter.path, '/elections/12/vote');
    expect((adapter.body as Map)['seats'], [
      {
        'electionSeatId': 3,
        'nominationIds': [4]
      },
      {'electionSeatId': 5, 'nominationIds': []},
    ]);
  });

  // FR-39: the admin card shows whether the returning officer key is set.
  test('reads the returning officer key fingerprint', () {
    final withKey = AdminElection.fromJson(
        {'id': 1, 'title': 'EC', 'ballotKeyFingerprint': 'ab12'});
    final withoutKey = AdminElection.fromJson({'id': 2, 'title': 'EC'});

    expect(withKey.ballotKeyFingerprint, 'ab12');
    expect(withoutKey.ballotKeyFingerprint, isNull);
  });

  // FR-39: the admin card learns what the caller may do on this election.
  test('reads my permissions and the handover flag', () {
    final e = AdminElection.fromJson({
      'id': 1,
      'title': 'EC',
      'myPermissions': ['Count', 'Declare'],
      'adminHandedOver': true,
    });
    final bare = AdminElection.fromJson({'id': 2, 'title': 'EC'});

    expect(e.myPermissions, ['Count', 'Declare']);
    expect(e.adminHandedOver, isTrue);
    expect(bare.myPermissions, isEmpty);
    expect(bare.adminHandedOver, isFalse);
  });

  // FR-39 (spec 023, 37.12f): true tells the screen the step is waiting.
  test('reports a 202 phase or declare reply as waiting', () async {
    final service =
        ElectionService(Dio()..httpClientAdapter = _ElectionAdapter());

    expect(await service.setPhase(12, 'Counting'), isFalse);
    expect(await service.declare(12), isTrue);
  });
}
