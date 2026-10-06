import 'dart:convert';
import 'dart:typed_data';
import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/features/elections/election_service.dart';

class _ElectionAdapter implements HttpClientAdapter {
  String? method;
  String? path;
  Object? body;
  bool unlockOpen = false;

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
    // 37.13y: officials of one election.
    if (options.path == '/elections/12/appointments') {
      final appointment = {
        'id': 3,
        'electionTitle': 'EC 2026',
        'personaName': 'Polling Officer',
        'displayName': 'Rina',
        'email': 'rina@example.org',
        'isLive': true,
      };
      return options.method == 'GET'
          ? _json([
              appointment,
              {...appointment, 'id': 4, 'isLive': false, 'revokedAt': '2026-10-02T10:00:00Z'}
            ])
          : _json({...appointment, 'id': 5, 'isLive': false});
    }
    if (options.path == '/elections/appointments/3/revoke') {
      return ResponseBody.fromString('', 204);
    }
    if (options.path == '/elections/appointments/3/emergency-revoke') {
      return _json({'id': 40, 'action': 'EmergencyRevoke'}, status: 202);
    }
    if (options.path == '/election-personas') {
      return _json([
        {'id': 2, 'name': 'Polling Officer'}
      ]);
    }
    // 37.13z: the site-wide rules unlock.
    final unlock = {
      'id': 1,
      'openedBy': 'shalin',
      'reason': 'Wrong ballot order after freeze',
      'openedAt': '2026-10-05T10:00:00Z',
      'expiresAt': '2026-10-05T10:30:00Z',
    };
    if (options.path == '/admin/elections/rules-unlock') {
      if (options.method == 'POST') return _json(unlock);
      return unlockOpen ? _json(unlock) : ResponseBody.fromString('', 204);
    }
    if (options.path == '/admin/elections/rules-unlock/close') {
      return ResponseBody.fromString('', 204);
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

  // 37.13t: opening polling with too few approvers carries a warning.
  test('warns only when moving to polling with too few approvers', () {
    final few = AdminElection.fromJson(
        {'id': 1, 'title': 'EC', 'phase': 'Campaign', 'tooFewApprovers': true});
    final enough = AdminElection.fromJson({'id': 2, 'title': 'EC', 'phase': 'Campaign'});

    expect(pollingApproverWarning(few, 'Polling'), electionTooFewApproversWarning);
    expect(pollingApproverWarning(few, 'Counting'), isNull);
    expect(pollingApproverWarning(enough, 'Polling'), isNull);
  });

  // FR-39 (spec 023, 37.12f): true tells the screen the step is waiting.
  test('reports a 202 phase or declare reply as waiting', () async {
    final service =
        ElectionService(Dio()..httpClientAdapter = _ElectionAdapter());

    expect(await service.setPhase(12, 'Counting'), isFalse);
    expect(await service.declare(12), isTrue);
  });

  // 37.13y: the admin list of officials carries who holds each one.
  test('loads appointments with name, email and revoked time', () async {
    final service =
        ElectionService(Dio()..httpClientAdapter = _ElectionAdapter());

    final list = await service.getAppointments(12);

    expect(list.map((a) => a.id), [3, 4]);
    expect(list.first.displayName, 'Rina');
    expect(list.first.email, 'rina@example.org');
    expect(list.first.revokedAt, isNull);
    expect(list.last.revokedAt, isNotNull);
  });

  test('a "mine" appointment without the admin fields still parses', () {
    final a = ElectionAppointment.fromJson({
      'id': 1,
      'electionTitle': 'EC',
      'personaName': 'Polling Officer',
    });

    expect(a.displayName, '');
    expect(a.email, isNull);
    expect(a.revokedAt, isNull);
  });

  test('appoints, revokes and asks for an emergency revoke on the right routes',
      () async {
    final adapter = _ElectionAdapter();
    final service = ElectionService(Dio()..httpClientAdapter = adapter);

    final created = await service.appoint(12, {'personaId': 2, 'memberId': 11});
    expect(created.id, 5);
    expect(adapter.method, 'POST');
    expect((adapter.body as Map)['memberId'], 11);

    await service.revokeAppointment(3, null);
    expect(adapter.path, '/elections/appointments/3/revoke');
    expect((adapter.body as Map)['reason'], isNull);

    await service.emergencyRevoke(3, 'Conflict of interest');
    expect(adapter.path, '/elections/appointments/3/emergency-revoke');
    expect((adapter.body as Map)['reason'], 'Conflict of interest');
  });

  test('loads the personas that can be appointed', () async {
    final service =
        ElectionService(Dio()..httpClientAdapter = _ElectionAdapter());

    final personas = await service.getPersonas();

    expect(personas.single.id, 2);
    expect(personas.single.name, 'Polling Officer');
  });

  group('buildAppointRequest', () {
    test('needs a role first', () {
      expect(buildAppointRequest(memberId: 11).problem, 'Choose a role.');
    });

    test('needs a member, or both a name and an email', () {
      expect(buildAppointRequest(personaId: 2, displayName: 'Tanvir').problem,
          'Choose a member, or give a name and email.');
      expect(buildAppointRequest(personaId: 2, email: 'a@b.org').problem,
          'Choose a member, or give a name and email.');
      expect(buildAppointRequest(personaId: 2, displayName: '  ', email: '  ').problem,
          'Choose a member, or give a name and email.');
    });

    test('a picked member drops the typed name, email and phone', () {
      final r = buildAppointRequest(
          personaId: 2, memberId: 11, displayName: 'left over', email: 'x@y.org', phone: '017');
      expect(r.problem, isNull);
      expect(r.body, {
        'personaId': 2,
        'memberId': 11,
        'displayName': null,
        'email': null,
        'phone': null,
        'isReturningOfficer': false,
      });
    });

    test('trims an outside appointee and leaves an empty phone out', () {
      final r = buildAppointRequest(
          personaId: 2,
          displayName: ' Tanvir ',
          email: ' tanvir@example.org ',
          phone: '  ',
          isReturningOfficer: true);
      expect(r.body, {
        'personaId': 2,
        'memberId': null,
        'displayName': 'Tanvir',
        'email': 'tanvir@example.org',
        'phone': null,
        'isReturningOfficer': true,
      });
    });
  });

  // 37.13z: the same checks as the API and the web admin.
  test('checks the unlock reason length and the minutes', () {
    const good = 'Wrong ballot order after freeze';
    expect(rulesUnlockProblem('too short', 30), contains('at least 20'));
    expect(rulesUnlockProblem('x' * 1001, 30), contains('at most 1000'));
    expect(rulesUnlockProblem(good, null), contains('1 to 60'));
    expect(rulesUnlockProblem(good, 0), contains('1 to 60'));
    expect(rulesUnlockProblem(good, 61), contains('1 to 60'));
    expect(rulesUnlockProblem(good, 1), isNull);
    expect(rulesUnlockProblem(good, 60), isNull);
    expect(rulesUnlockProblem('   ${'x' * 19}   ', 30), contains('at least 20'));
  });

  test('rounds the minutes left up and never below zero', () {
    final unlock = ElectionRulesUnlock(
        id: 1,
        openedBy: 'shalin',
        reason: 'r',
        expiresAt: DateTime.utc(2026, 10, 5, 10, 30));
    final opened = DateTime.utc(2026, 10, 5, 10);

    expect(unlock.minutesLeft(opened), 30);
    expect(unlock.minutesLeft(opened.add(const Duration(minutes: 29, seconds: 1))), 1);
    expect(unlock.minutesLeft(opened.add(const Duration(minutes: 31))), 0);
    expect(
        const ElectionRulesUnlock(id: 1, openedBy: '', reason: '').minutesLeft(opened), 0);
  });

  test('reads no unlock from a 204 and the open one from a 200', () async {
    final adapter = _ElectionAdapter();
    final service = ElectionService(Dio()..httpClientAdapter = adapter);

    expect(await service.getRulesUnlock(), isNull);

    adapter.unlockOpen = true;
    final open = await service.getRulesUnlock();
    expect(open?.openedBy, 'shalin');
    expect(open?.expiresAt, isNotNull);
  });

  test('opens and closes the unlock on the right routes', () async {
    final adapter = _ElectionAdapter();
    final service = ElectionService(Dio()..httpClientAdapter = adapter);

    final opened = await service.openRulesUnlock('Wrong ballot order after freeze', 15);
    expect(opened.id, 1);
    expect(adapter.body, {'reason': 'Wrong ballot order after freeze', 'minutes': 15});

    await service.closeRulesUnlock();
    expect(adapter.method, 'POST');
    expect(adapter.path, '/admin/elections/rules-unlock/close');
  });
}
