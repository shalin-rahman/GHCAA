import 'dart:convert';
import 'dart:typed_data';
import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/constants/app_constants.dart';
import 'package:ghcaa_mobile/features/elections/election_service.dart';

const _election = {
  'id': 12,
  'title': 'EC 2026',
  'phase': 'Announced',
  'isActive': true,
  'eligibleVoterCount': 0,
};

const _approval = {
  'id': 3,
  'electionId': 12,
  'action': 'Count',
  'requestedBy': 'Officer',
  'requestedAt': '2026-10-23T09:00:00Z',
  'expiresAt': '2026-10-24T09:00:00Z',
};

// Records every request. Paths listed in [stored] answer 202, as the server
// does when a second official has to approve the step.
class _Adapter implements HttpClientAdapter {
  final requests = <String>[];
  final bodies = <Object?>[];
  final stored = <String>{};

  @override
  void close({bool force = false}) {}

  @override
  Future<ResponseBody> fetch(RequestOptions options,
      Stream<Uint8List>? requestStream, Future<void>? cancelFuture) async {
    requests.add('${options.method} ${options.path}');
    bodies.add(options.data);
    if (stored.contains(options.path)) {
      // A held count answers with the approval it stored, as the API does.
      return options.path.endsWith('/count')
          ? _json(_approval, 202)
          : ResponseBody.fromString('', 202);
    }
    switch (options.path) {
      case '/admin/elections':
        return _json(options.method == 'POST' ? _election : [_election]);
      case '/elections/12/voter-roll/freeze':
        return _json({'count': 40});
      case '/elections/12/count':
        return _json([]);
      default:
        return ResponseBody.fromString('', 200);
    }
  }

  ResponseBody _json(Object value, [int status = 200]) =>
      ResponseBody.fromString(jsonEncode(value), status, headers: {
        Headers.contentTypeHeader: ['application/json']
      });
}

// TODO 84.16. The admin steps the web runs, called from the phone.
void main() {
  late _Adapter adapter;
  late ElectionService service;

  setUp(() {
    adapter = _Adapter();
    service = ElectionService(Dio()..httpClientAdapter = adapter);
  });

  test('FR-029, FR-030 (spec 018): publish and close hit the admin routes and run at once on 200', () async {
    expect(await service.publish(12), isFalse);
    expect(await service.close(12), isFalse);
    expect(adapter.requests,
        ['POST /admin/elections/12/publish', 'POST /admin/elections/12/close']);
  });

  test('FR-029, FR-030 (spec 018): publish and close report a 202 as waiting for a second official', () async {
    adapter.stored.addAll(['/admin/elections/12/publish', '/admin/elections/12/close']);
    expect(await service.publish(12), isTrue);
    expect(await service.close(12), isTrue);
  });

  test('FR-032 (spec 018): remove candidate sends a DELETE for that candidate', () async {
    await service.removeCandidate(12, 7);
    expect(adapter.requests.single, 'DELETE /admin/elections/12/candidates/7');
  });

  test('FR-021 (spec 018): scrutiny decision posts accepted and reason', () async {
    await service.scrutinise(4, false, reason: 'Dues unpaid');
    expect(adapter.requests.single, 'POST /elections/nominations/4/scrutiny');
    expect(adapter.bodies.single, {'accepted': false, 'reason': 'Dues unpaid'});
  });

  test('FR-39 (spec 023, 84.44): the ballot key goes up as publicKey, and a 202 waits', () async {
    expect(await service.setBallotKey(12, 'SPKI'), isFalse);
    adapter.stored.add('/admin/elections/12/ballot-key');
    expect(await service.setBallotKey(12, 'SPKI'), isTrue);
    expect(adapter.requests.first, 'POST /admin/elections/12/ballot-key');
    expect(adapter.bodies.first, {'publicKey': 'SPKI'});
  });

  test('FR-024 (spec 018, 84.44): a count with the key file returns the results on 200', () async {
    final run = await service.runCount(12, 'PKCS8');
    expect(adapter.requests.single, 'POST /elections/12/count');
    expect(adapter.bodies.single, {'privateKey': 'PKCS8'});
    expect(run.results, isEmpty);
    expect(run.pending, isNull);
  });

  test('FR-024 (spec 018, 84.44): a held count returns the approval on 202', () async {
    adapter.stored.add('/elections/12/count');
    final run = await service.runCount(12, 'PKCS8');
    expect(run.results, isNull);
    expect(run.pending!.id, 3);
    expect(run.pending!.action, 'Count');
  });

  test('key destroyed and readiness attest post to the election routes', () async {
    await service.keyDestroyed(12);
    await service.attestReadiness(12, 'mock-election');
    expect(adapter.requests, [
      'POST /elections/12/key-destroyed',
      'POST /elections/12/readiness/attest',
    ]);
    expect(adapter.bodies.last, {'condition': 'mock-election'});
  });

  test('FR-027 (spec 018): an admin election lists its candidates, and none when the field is absent', () {
    final withList = AdminElection.fromJson({
      ..._election,
      'candidates': [
        {'id': 7, 'name': 'Rina', 'positionTitle': 'President'}
      ],
    });
    expect(withList.candidates.single.id, 7);
    expect(withList.candidates.single.positionTitle, 'President');
    expect(AdminElection.fromJson(_election).candidates, isEmpty);
  });

  test('FR-015, FR-018, FR-024, FR-025 (spec 018): walks an election from draft to declared and archived', () async {
    final e = await service.create(
      title: 'EC 2026',
      ecPeriodId: 1,
      nominationOpensOn: DateTime(2026, 11, 1),
      nominationClosesOn: DateTime(2026, 11, 5),
      pollingOpensOn: DateTime(2026, 11, 20),
      pollingClosesOn: DateTime(2026, 11, 20, 17),
    );
    await service.publish(e.id);
    expect(await service.freezeVoterRoll(e.id), 40);
    await service.scrutinise(4, true);
    for (final phase in [
      ElectionPhases.scrutiny,
      ElectionPhases.withdrawal,
      ElectionPhases.candidateList,
      ElectionPhases.campaign,
    ]) {
      await service.setPhase(e.id, phase);
    }
    await service.attestReadiness(e.id, 'mock-election');
    await service.setPhase(e.id, ElectionPhases.polling);
    await service.close(e.id);
    expect(await service.count(e.id), isEmpty);
    expect(await service.declare(e.id), isFalse);
    await service.keyDestroyed(e.id);
    await service.setPhase(e.id, ElectionPhases.archived);

    expect(adapter.requests, [
      'POST /admin/elections',
      'POST /admin/elections/12/publish',
      'POST /elections/12/voter-roll/freeze',
      'POST /elections/nominations/4/scrutiny',
      'POST /elections/12/phase',
      'POST /elections/12/phase',
      'POST /elections/12/phase',
      'POST /elections/12/phase',
      'POST /elections/12/readiness/attest',
      'POST /elections/12/phase',
      'POST /admin/elections/12/close',
      'POST /elections/12/count',
      'POST /elections/12/declare',
      'POST /elections/12/key-destroyed',
      'POST /elections/12/phase',
    ]);
    // FR-37.1a. The phase goes as a quoted JSON string. A bare one fails to parse on the API.
    expect(adapter.bodies[9], jsonEncode(ElectionPhases.polling));
    expect(adapter.bodies.last, jsonEncode(ElectionPhases.archived));
  });
}
