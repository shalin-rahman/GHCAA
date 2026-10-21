import 'dart:convert';
import 'dart:typed_data';
import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/utils/app_utils.dart';
import 'package:ghcaa_mobile/features/elections/election_service.dart';

const _recount = {
  'id': 6,
  'electionSeatId': 3,
  'positionTitle': 'President',
  'applicantName': 'Rina',
  'applicantMemberId': 8,
  'grounds': 'Close margin',
  'explanation': null,
  'receivedAt': '2026-10-20T09:30:00',
  'allowed': null,
  'decisionReason': null,
  'figures': [],
};

class _Adapter implements HttpClientAdapter {
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
    switch (options.path) {
      case '/admin/elections/12/form-details':
        return _json({
          'id': 12,
          'title': 'EC 2026',
          'phase': 'Nomination',
          'isActive': true,
          'eligibleVoterCount': 10,
          'positions': [
            {'id': 3, 'title': 'President'}
          ],
          'formDetails': {
            'finalListOn': '2026-11-02',
            'silenceStartsAt': '2026-11-09T18:00:00',
            'venue': 'Main hall',
          },
        });
      case '/admin/elections/12/nominations/4/scrutiny-checks':
        return _json([
          {'item': 'MembershipVerified', 'isChecked': true},
        ]);
      case '/admin/elections/12/recounts':
        return _json(options.method == 'POST' ? _recount : [_recount]);
      default:
        return ResponseBody.fromString('', 204);
    }
  }

  ResponseBody _json(Object value) => ResponseBody.fromString(jsonEncode(value), 200,
      headers: {
        Headers.contentTypeHeader: ['application/json']
      });
}

// TODO 37.14m. ER-11 and ER-28 form records on the mobile client.
void main() {
  late _Adapter adapter;
  late ElectionService service;

  setUp(() {
    adapter = _Adapter();
    service = ElectionService(Dio()..httpClientAdapter = adapter);
  });

  test('ER-28: form details send days as dates and times as UTC, nulls included', () async {
    final silence = DateTime(2026, 11, 9, 18);
    final updated = await service.updateFormDetails(
        12,
        ElectionFormDetails(
            finalListOn: DateTime(2026, 11, 2), silenceStartsAt: silence, venue: 'Main hall'));

    expect(adapter.method, 'PUT');
    final body = adapter.body as Map;
    expect(body['finalListOn'], '2026-11-02');
    expect(body['silenceStartsAt'], silence.toUtc().toIso8601String());
    expect(body.containsKey('campaignOpensOn'), isTrue);
    expect(body['campaignOpensOn'], isNull);
    expect(body['recountWindowEndsOn'], isNull);

    expect(updated.positions.single.title, 'President');
    expect(updated.formDetails!.venue, 'Main hall');
    expect(updated.formDetails!.finalListOn, DateTime(2026, 11, 2));
    expect(updated.formDetails!.silenceStartsAt, AppUtils.parseUtc('2026-11-09T18:00:00'));
  });

  test('ER-28: an election without positions or form details still parses', () {
    final e = AdminElection.fromJson(
        {'id': 1, 'title': 'T', 'phase': 'Draft', 'isActive': true, 'eligibleVoterCount': 0});
    expect(e.positions, isEmpty);
    expect(e.formDetails, isNull);
  });

  test('ER-28: consent time goes to the nomination as UTC', () async {
    final at = DateTime(2026, 10, 20, 14, 5);
    await service.recordConsent(12, 4, at);

    expect(adapter.path, '/admin/elections/12/nominations/4/consent');
    expect((adapter.body as Map)['consentAt'], at.toUtc().toIso8601String());
  });

  test('ER-28: scrutiny ticks load and save on the nomination route', () async {
    final loaded = await service.getScrutinyChecks(12, 4);
    expect(adapter.method, 'GET');
    expect(loaded.single.item, 'MembershipVerified');
    expect(loaded.single.isChecked, isTrue);

    await service.setScrutinyCheck(12, 4, 'VotingMember', false);
    expect(adapter.method, 'PUT');
    expect(adapter.body, {'item': 'VotingMember', 'isChecked': false});
  });

  test('ER-11: recounts list, record with a UTC received time, and parse a pending one', () async {
    final list = await service.getRecounts(12);
    expect(list.single.allowed, isNull);
    expect(list.single.receivedAt, AppUtils.parseUtc('2026-10-20T09:30:00'));

    final received = DateTime(2026, 10, 20, 15, 30);
    final created = await service.recordRecount(12,
        electionSeatId: 3, applicantName: 'Rina', grounds: 'Close margin', receivedAt: received);
    expect(adapter.method, 'POST');
    final body = adapter.body as Map;
    expect(body['receivedAt'], received.toUtc().toIso8601String());
    expect(body['applicantMemberId'], isNull);
    expect(body['explanation'], isNull);
    expect(created.id, 6);
  });

  test('ER-11: decision and figures go to their own recount routes', () async {
    await service.decideRecount(12, 6, false, 'Margin over the threshold');
    expect(adapter.path, '/admin/elections/12/recounts/6/decision');
    expect(adapter.body, {'allowed': false, 'reason': 'Margin over the threshold'});

    await service.recordRecountFigures(12, 6, const [
      RecountFigure(nominationId: 4, voteCount: 41),
      RecountFigure(nominationId: 5, voteCount: 0),
    ]);
    expect(adapter.path, '/admin/elections/12/recounts/6/figures');
    expect(adapter.body, {
      'figures': [
        {'nominationId': 4, 'voteCount': 41},
        {'nominationId': 5, 'voteCount': 0},
      ]
    });
  });
}
