import 'dart:convert';
import 'dart:typed_data';
import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/config/org_config.dart';
import 'package:ghcaa_mobile/features/elections/ballot_receipt.dart';
import 'package:ghcaa_mobile/features/elections/election_service.dart';

const _seat = {
  'seatId': 3,
  'positionTitle': 'President',
  'places': 1,
  'candidates': [
    {
      'nominationId': 4,
      'name': 'Rina',
      'photoUrl': '/uploads/r.png',
      'statement': 'For service'
    },
    {'nominationId': 5, 'name': 'Tanvir', 'statement': ''}
  ]
};

class _ElectionAdapter implements HttpClientAdapter {
  String? method;
  String? path;
  Object? body;
  bool unlockOpen = false;
  bool noDeclared = false;

  @override
  void close({bool force = false}) {}

  @override
  Future<ResponseBody> fetch(RequestOptions options,
      Stream<Uint8List>? requestStream, Future<void>? cancelFuture) async {
    method = options.method;
    path = options.path;
    body = options.data;
    if (options.path == '/elections/latest-declared') {
      if (noDeclared) return _json({'title': 'Not found'}, status: 404);
      return _json({
        'id': 12,
        'title': 'EC 2026',
        'phase': 'Declared',
        'ecPeriodId': 2,
        'voterCount': 7,
        'eligibleVoterCount': 10
      });
    }
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
    if (options.path == '/admin/elections/12/nominations') {
      return _json([
        {
          'id': 4,
          'electionSeatId': 3,
          'candidateMemberId': 8,
          'status': 'Accepted',
          'statement': 'For service',
          'consentAt': '2026-10-20T09:30:00'
        }
      ]);
    }
    if (options.path == '/elections/12/ballot') {
      return _json({
        'electionId': 12,
        'title': 'EC 2026',
        'phase': 'Polling',
        'hasVoted': true,
        'ballotKeyFingerprint': 'ab12',
        'seats': [_seat],
        'isEligible': false,
        'ineligibilityReason': 'Membership dues are overdue.'
      });
    }
    if (options.path == '/elections/12/results') {
      return _json({
        'electionId': 12,
        'title': 'EC 2026',
        'phase': 'Declared',
        'declaredOn': '2026-10-08T10:00:00Z',
        'eligibleVoters': 10,
        'ballotsCast': 7,
        'spoiledBallots': 1,
        'seats': [
          {
            'seatId': 3,
            'positionTitle': 'President',
            'places': 1,
            'abstentions': 2,
            'candidates': [
              {
                'nominationId': 4,
                'name': 'Rina',
                'voteCount': 4,
                'isElected': true,
                'isTie': false
              },
              {
                'nominationId': 5,
                'name': 'Tanvir',
                'voteCount': 1,
                'isElected': false,
                'isTie': false
              }
            ]
          }
        ]
      });
    }
    if (options.path == '/elections/12/opened-ballots') {
      return _json({
        'electionId': 12,
        'seats': [
          {
            'electionSeatId': 3,
            'ballots': [
              [4],
              [5, 6]
            ]
          }
        ]
      });
    }
    if (options.path == '/elections/13/opened-ballots') {
      return _json({'title': 'Not found'}, status: 404);
    }
    if (options.path == '/elections/12/audit/verify') {
      return _json({'ok': false, 'entries': 5, 'firstBrokenSeq': 3});
    }
    if (options.path == '/elections/12/audit') {
      return _json([
        {
          'seq': 1,
          'at': '2026-10-01T09:00:00Z',
          'actorUserId': 4,
          'action': 'PhaseChanged',
          'detailJson': '{"to":"Nomination"}',
          'hash': 'ab12'
        },
        {
          'seq': 2,
          'at': '2026-10-02T09:00:00Z',
          'action': 'BallotCast',
          'detailJson': '{}',
          'hash': 'cd34'
        }
      ]);
    }
    if (options.path == '/elections/12/tracking-codes') {
      return _json(['AAAA-1111', 'BBBB-2222']);
    }
    if (options.path == '/elections/13/results' ||
        options.path == '/elections/13/tracking-codes') {
      return _json({'title': 'Not found'}, status: 404);
    }
    if (options.path == '/elections/14/results') {
      return _json({'title': 'Server error'}, status: 500);
    }
    if (options.path == '/admin/elections' && options.method == 'POST') {
      return _json({'id': 40, 'title': 'EC 2027'});
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
    // 37.13h: two-person approvals.
    if (options.path == '/elections/12/approvals') {
      return _json([
        {
          'id': 30,
          'electionId': 12,
          'action': 'Declare',
          'requestedBy': 'Rina',
          'requestedAt': '2026-10-09T10:00:00Z',
          'expiresAt': '2026-10-10T10:00:00Z',
          'requestedByMe': true,
        },
        {
          'id': 31,
          'electionId': 12,
          'action': 'Count',
          'requestedBy': 'Tanvir',
          'requestedAt': '2026-10-09T11:00:00Z',
          'expiresAt': '2026-10-10T11:00:00Z',
        }
      ]);
    }
    if (options.path == '/elections/approvals/31/approve' ||
        options.path == '/elections/approvals/31/reject') {
      return ResponseBody.fromString('', 204);
    }
    if (options.path == '/elections/12/board') {
      return _json({
        'isTest': false,
        'groups': [],
        'turnout': {'ballotsCast': 7, 'rollSize': 10},
      });
    }
    if (options.path == '/elections/13/board') {
      return _json({'isTest': false, 'groups': [], 'turnout': null});
    }
    // 37.13z: the site-wide rules unlock.
    final unlock = {
      'id': 1,
      'openedBy': 'shalin',
      'reason': 'Wrong ballot order after freeze',
      'openedAt': '2026-10-05T10:00:00Z',
      'expiresAt': '2026-10-05T10:30:00Z',
    };
    if (options.path == '/elections/9/voter-roll/unfreeze') {
      return _json({'removed': 12});
    }
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

  test('the officials nomination list reads the admin route and its consent time as UTC', () async {
    final adapter = _ElectionAdapter();
    final service = ElectionService(Dio()..httpClientAdapter = adapter);

    final nominations = await service.getAdminNominations(12);

    expect(adapter.path, '/admin/elections/12/nominations');
    expect(nominations.single.consentAt!.toUtc(), DateTime.utc(2026, 10, 20, 9, 30));
  });

  test('latest declared election reads from its own route', () async {
    final adapter = _ElectionAdapter();
    final service = ElectionService(Dio()..httpClientAdapter = adapter);

    final election = await service.getLatestDeclared();

    expect(election!.id, 12);
    expect(election.phase, 'Declared');
    expect(adapter.path, '/elections/latest-declared');
  });

  test('latest declared election is null on a 404', () async {
    final adapter = _ElectionAdapter()..noDeclared = true;
    final service = ElectionService(Dio()..httpClientAdapter = adapter);

    expect(await service.getLatestDeclared(), isNull);
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

  test('cast body carries every pick for a seat', () async {
    final adapter = _ElectionAdapter();
    final service = ElectionService(Dio()..httpClientAdapter = adapter);

    await service.castBallot(12, {
      3: [4, 5],
    });

    expect((adapter.body as Map)['seats'], [
      {
        'electionSeatId': 3,
        'nominationIds': [4, 5]
      },
    ]);
  });

  test('create sends the tie rule and the optional dates', () async {
    final adapter = _ElectionAdapter();
    final service = ElectionService(Dio()..httpClientAdapter = adapter);
    final d = DateTime(2026, 11, 1);

    await service.create(
      title: 'EC 2027',
      ecPeriodId: 2,
      nominationOpensOn: d,
      nominationClosesOn: d,
      pollingOpensOn: d,
      pollingClosesOn: d,
      tieRule: ElectionTieRule.runOff,
      scrutinyOn: DateTime(2026, 11, 3),
      withdrawalClosesOn: DateTime(2026, 11, 5),
    );

    final body = adapter.body as Map;
    expect(adapter.path, '/admin/elections');
    expect(body['tieRule'], 'RunOff');
    expect(body['scrutinyOn'], '2026-11-03');
    expect(body['withdrawalClosesOn'], '2026-11-05');
  });

  test('create defaults to drawing lots and omits unset dates', () async {
    final adapter = _ElectionAdapter();
    final service = ElectionService(Dio()..httpClientAdapter = adapter);
    final d = DateTime(2026, 11, 1);

    await service.create(
      title: 'EC 2027',
      ecPeriodId: 2,
      nominationOpensOn: d,
      nominationClosesOn: d,
      pollingOpensOn: d,
      pollingClosesOn: d,
    );

    final body = adapter.body as Map;
    expect(body['tieRule'], 'DrawingLots');
    expect(body.containsKey('scrutinyOn'), isFalse);
    expect(body.containsKey('withdrawalClosesOn'), isFalse);
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
  test('names an approval action and falls back to the wire name', () {
    expect(electionApprovalActionLabel('Declare'), 'Declare results');
    expect(electionApprovalActionLabel('SomethingNew'), 'SomethingNew');
  });

  test('labels every action the server can ask approval for', () {
    for (final action in ElectionSettings.allApprovalActions) {
      expect(electionApprovalActionLabels.containsKey(action), isTrue);
    }
  });

  test('warns only when moving to polling with too few approvers', () {
    final few = AdminElection.fromJson(
        {'id': 1, 'title': 'EC', 'phase': 'Campaign', 'tooFewApprovers': true});
    final enough = AdminElection.fromJson({'id': 2, 'title': 'EC', 'phase': 'Campaign'});

    expect(pollingApproverWarning(few, 'Polling'), electionTooFewApproversWarning);
    expect(pollingApproverWarning(few, 'Counting'), isNull);
    expect(pollingApproverWarning(enough, 'Polling'), isNull);
  });

  // FR-047 (37.13d).
  test('reads the test-run flag and defaults it to false', () {
    expect(AdminElection.fromJson({'id': 1, 'title': 'EC', 'isTest': true}).isTest, isTrue);
    expect(AdminElection.fromJson({'id': 2, 'title': 'EC'}).isTest, isFalse);
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

  test('loads approvals and flags the ones the caller asked for', () async {
    final service =
        ElectionService(Dio()..httpClientAdapter = _ElectionAdapter());

    final approvals = await service.getApprovals(12);

    expect(approvals.map((a) => a.id), [30, 31]);
    expect(approvals.first.requestedByMe, isTrue);
    expect(approvals.last.requestedByMe, isFalse);
    expect(approvals.last.approvedAt, isNull);
  });

  test('approves and rejects on the approval routes', () async {
    final adapter = _ElectionAdapter();
    final service = ElectionService(Dio()..httpClientAdapter = adapter);

    await service.approve(31);
    expect(adapter.method, 'POST');
    expect(adapter.path, '/elections/approvals/31/approve');

    await service.reject(31, 'Wrong date');
    expect(adapter.path, '/elections/approvals/31/reject');
    expect((adapter.body as Map)['reason'], 'Wrong date');
  });

  test('reads turnout from the board, and leaves it empty when none is sent',
      () async {
    final service =
        ElectionService(Dio()..httpClientAdapter = _ElectionAdapter());

    final withTurnout = await service.getBoard(12);
    final polling = await service.getBoard(13);

    expect(withTurnout!.ballotsCast, 7);
    expect(withTurnout.rollSize, 10);
    expect(polling!.ballotsCast, isNull);
    expect(polling.rollSize, isNull);
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

  test('unfreezes the voter roll with the reason', () async {
    final adapter = _ElectionAdapter();
    final service = ElectionService(Dio()..httpClientAdapter = adapter);

    final removed = await service.unfreezeVoterRoll(9, 'Wrong roll');

    expect(removed, 12);
    expect(adapter.method, 'POST');
    expect(adapter.path, '/elections/9/voter-roll/unfreeze');
    expect(adapter.body, {'reason': 'Wrong roll'});
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
  test('loads the ballot with seats, candidates and the voted flag', () async {
    final service =
        ElectionService(Dio()..httpClientAdapter = _ElectionAdapter());

    final ballot = await service.getBallot(12);

    expect(ballot.hasVoted, isTrue);
    expect(ballot.phase, 'Polling');
    expect(ballot.ballotKeyFingerprint, 'ab12');
    expect(ballot.seats.single.positionTitle, 'President');
    expect(ballot.seats.single.candidates.first.photoUrl, '/uploads/r.png');
    expect(ballot.seats.single.candidates.last.photoUrl, isNull);
    expect(ballot.isEligible, isFalse);
    expect(ballot.ineligibilityReason, 'Membership dues are overdue.');
  });

  test('eligibility is left open when the server does not send it', () {
    final ballot = Ballot.fromJson({
      'electionId': 1,
      'title': 'EC',
      'phase': 'Polling',
      'hasVoted': false
    });

    expect(ballot.isEligible, isNull);
    expect(ballot.ineligibilityReason, isNull);
  });

  test('loads the declared results', () async {
    final service =
        ElectionService(Dio()..httpClientAdapter = _ElectionAdapter());

    final r = (await service.getResults(12))!;

    expect(r.eligibleVoters, 10);
    expect(r.ballotsCast, 7);
    expect(r.spoiledBallots, 1);
    expect(r.declaredOn, isNotNull);
    expect(r.seats.single.abstentions, 2);
    expect(r.seats.single.candidates.first.isElected, isTrue);
    expect(r.seats.single.candidates.last.voteCount, 1);
  });

  test(
      'results and tracking codes read as null on a 404 and rethrow other errors',
      () async {
    final service =
        ElectionService(Dio()..httpClientAdapter = _ElectionAdapter());

    expect(await service.getResults(13), isNull);
    expect(await service.getTrackingCodes(13), isNull);
    expect(() => service.getResults(14), throwsA(isA<DioException>()));
  });

  test('opened ballots read per seat, null on a 404', () async {
    final service =
        ElectionService(Dio()..httpClientAdapter = _ElectionAdapter());

    final opened = (await service.getOpenedBallots(12))!;

    expect(opened[3], [
      [4],
      [5, 6]
    ]);
    expect(await service.getOpenedBallots(13), isNull);
  });

  test('audit verify returns the first broken entry', () async {
    final service =
        ElectionService(Dio()..httpClientAdapter = _ElectionAdapter());

    expect(await service.firstBrokenAuditEntry(12), 3);
  });

  test('FR-49: audit verify reads the whole check', () async {
    final service =
        ElectionService(Dio()..httpClientAdapter = _ElectionAdapter());

    final check = await service.verifyAudit(12);

    expect(check.ok, isFalse);
    expect(check.entries, 5);
    expect(check.firstBrokenSeq, 3);
  });

  test('FR-49: getAudit reads the entries in order, actor optional', () async {
    final service =
        ElectionService(Dio()..httpClientAdapter = _ElectionAdapter());

    final entries = await service.getAudit(12);

    expect(entries.map((e) => e.seq), [1, 2]);
    expect(entries.first.actorUserId, 4);
    expect(entries.first.at, DateTime.utc(2026, 10, 1, 9));
    expect(entries.last.actorUserId, isNull);
    expect(entries.last.hash, 'cd34');
  });

  test('loads the tracking codes and filters them', () async {
    final service =
        ElectionService(Dio()..httpClientAdapter = _ElectionAdapter());

    final codes = (await service.getTrackingCodes(12))!;

    expect(codes, ['AAAA-1111', 'BBBB-2222']);
    expect(filterTrackingCodes(codes, ' bbbb '), ['BBBB-2222']);
    expect(filterTrackingCodes(codes, ''), codes);
    expect(filterTrackingCodes(codes, 'zzz'), isEmpty);
  });

  group('ballot paper', () {
    final seats = [
      BallotSeat.fromJson(_seat),
      const BallotSeat(
          seatId: 6,
          positionTitle: 'Members',
          places: 2,
          candidates: [
            BallotCandidate(nominationId: 10, name: 'A'),
            BallotCandidate(nominationId: 11, name: 'B'),
            BallotCandidate(nominationId: 12, name: 'C'),
          ]),
    ];

    test('one place swaps the pick, a second tap clears it', () {
      expect(togglePick([], 4, 1), [4]);
      expect(togglePick([4], 5, 1), [5]);
      expect(togglePick([4], 4, 1), isEmpty);
    });

    test('several places stop at the limit', () {
      expect(togglePick([10], 11, 2), [10, 11]);
      expect(togglePick([10, 11], 12, 2), [10, 11]);
      expect(togglePick([10, 11], 10, 2), [11]);
    });

    // FR-39
    test('review waits until every seat has a pick or an abstain', () {
      expect(allSeatsDecided(seats, {}), isFalse);
      expect(
          allSeatsDecided(seats, {
            6: [10]
          }),
          isFalse);
      expect(
          allSeatsDecided(seats, {
            3: [],
            6: [10]
          }),
          isTrue);
      expect(allSeatsDecided(const [], {}), isFalse);
    });

    test('every seat is sent, an unpicked one as an empty list', () {
      final choices = buildBallotChoices(seats, {
        6: [10, 11]
      });

      expect(choices, {
        3: <int>[],
        6: [10, 11],
      });
    });

    test('picks over the places or not on the seat are dropped', () {
      final choices = buildBallotChoices(seats, {
        3: [4, 5],
        6: [10, 99, 11, 12],
      });

      expect(choices[3], [4]);
      expect(choices[6], [10, 11]);
    });

    test('the receipt lines show names, or Abstained', () {
      final lines = ballotReceiptLines(seats, {
        3: [],
        6: [10, 12],
      });

      expect(lines[0], (position: 'President', choice: ballotAbstainedLabel));
      expect(lines[1], (position: 'Members', choice: 'A, C'));
      expect(
          ballotReceiptLines(seats, {}).every((l) => l.choice == 'Abstained'),
          isTrue);
    });

    test('the receipt PDF builds', () async {
      final bytes = await buildBallotReceiptPdf(
        electionTitle: 'EC 2026',
        dateText: '09-10-2026',
        trackingCode: 'ABCD-EFGH',
        lines: ballotReceiptLines(seats, {}),
      );

      expect(String.fromCharCodes(bytes.take(4)), '%PDF');
    });
  });
}
