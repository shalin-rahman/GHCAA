import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/config/org_config.dart';
import 'package:ghcaa_mobile/core/constants/app_constants.dart';
import 'package:ghcaa_mobile/core/services/org_config_service.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/features/elections/election_service.dart';
import 'package:ghcaa_mobile/screens/admin/election_form_records_sheet.dart';

class _FakeElectionService extends ElectionService {
  _FakeElectionService() : super(Dio());

  final calls = <String>[];
  ElectionFormDetails? savedDetails;
  List<RecountFigure>? savedFigures;
  List<ElectionRecount> recounts = const [];
  List<ScrutinyCheck> checks = const [ScrutinyCheck(item: 'MembershipVerified', isChecked: true)];

  @override
  Future<AdminElection> updateFormDetails(int id, ElectionFormDetails details) async {
    calls.add('details $id');
    savedDetails = details;
    return _election(formDetails: details);
  }

  @override
  Future<List<ElectionRecount>> getRecounts(int id) async {
    calls.add('recounts $id');
    return recounts;
  }

  @override
  Future<void> decideRecount(int id, int recountId, bool allowed, String? reason) async {
    calls.add('decide $recountId $allowed $reason');
  }

  @override
  Future<ElectionResults?> getResults(int id) async => const ElectionResults(
        electionId: 12,
        title: 'EC 2026',
        phase: 'Declared',
        eligibleVoters: 10,
        ballotsCast: 9,
        spoiledBallots: 0,
        seats: [
          SeatResult(seatId: 3, positionTitle: 'President', places: 1, abstentions: 0, candidates: [
            SeatResultCandidate(nominationId: 4, name: 'Rina', voteCount: 5, isElected: true, isTie: false),
            SeatResultCandidate(nominationId: 5, name: 'Tanvir', voteCount: 4, isElected: false, isTie: false),
          ]),
          SeatResult(seatId: 7, positionTitle: 'Secretary', places: 1, abstentions: 0, candidates: [
            SeatResultCandidate(nominationId: 9, name: 'Other', voteCount: 9, isElected: true, isTie: false),
          ]),
        ],
      );

  @override
  Future<void> recordRecountFigures(int id, int recountId, List<RecountFigure> figures) async {
    calls.add('figures $recountId');
    savedFigures = figures;
  }

  String nominationStatus = 'Accepted';

  @override
  Future<List<Nomination>> getAdminNominations(int id) async {
    calls.add('nominations $id');
    return [
      Nomination(id: 4, electionSeatId: 3, candidateMemberId: 8, status: nominationStatus, statement: ''),
    ];
  }

  @override
  Future<void> scrutinise(int nominationId, bool accepted, {String? reason}) async {
    calls.add('scrutinise $nominationId $accepted');
    nominationStatus = accepted ? 'Accepted' : 'Rejected';
  }

  @override
  Future<List<ScrutinyCheck>> getScrutinyChecks(int id, int nominationId) async {
    calls.add('checks $nominationId');
    return checks;
  }

  @override
  Future<List<ScrutinyCheck>> setScrutinyCheck(int id, int nominationId, String item, bool isChecked) async {
    calls.add('tick $nominationId $item $isChecked');
    checks = [...checks.where((c) => c.item != item), ScrutinyCheck(item: item, isChecked: isChecked)];
    return checks;
  }
}

AdminElection _election({
  String phase = 'Nomination',
  List<String> permissions = const ['ManageSetup', 'ViewDashboard', 'DecideAppeals', 'DecideNominations'],
  ElectionFormDetails? formDetails,
}) =>
    AdminElection(
      id: 12,
      title: 'EC 2026',
      phase: phase,
      isActive: true,
      eligibleVoterCount: 10,
      myPermissions: permissions,
      positions: const [ElectionPosition(id: 3, title: 'President')],
      formDetails: formDetails,
    );

const _pending = ElectionRecount(
    id: 6, electionSeatId: 3, positionTitle: 'President', applicantName: 'Rina', grounds: 'Close margin');

// TODO 37.14m. ER-11 and ER-28 form records sheet.
void main() {
  setUpAll(() => dotenv.testLoad(fileInput: 'PORTAL_TITLE=GHCAA'));

  Future<void> settle(WidgetTester tester) async {
    for (var i = 0; i < 5; i++) {
      await tester.pump(const Duration(milliseconds: 100));
    }
  }

  Future<_FakeElectionService> pump(WidgetTester tester, AdminElection election,
      {List<ElectionRecount> recounts = const [], String nominationStatus = 'Accepted'}) async {
    tester.view.physicalSize = const Size(390, 4000);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.reset);
    final service = _FakeElectionService()
      ..recounts = recounts
      ..nominationStatus = nominationStatus;
    await tester.pumpWidget(ProviderScope(
      overrides: [
        electionServiceProvider.overrideWith((ref) => service),
        orgDateFormatProvider.overrideWith((ref) => OrgConfig.offlineDefaults.dateFormat),
      ],
      child: MaterialApp(
        theme: AppTheme.midnightTheme,
        home: Scaffold(body: ElectionFormRecordsSheet(election: election)),
      ),
    ));
    await settle(tester);
    return service;
  }

  testWidgets('ER-28: sections follow the caller permissions', (tester) async {
    final service = await pump(tester, _election(permissions: const ['ViewDashboard']));

    expect(service.calls, ['recounts 12']);
    expect(find.byKey(const Key('form-save')), findsNothing);
    expect(find.byKey(const Key('recount-record')), findsNothing);
    expect(find.text(ElectionConstants.recountsEmpty), findsOneWidget);
  });

  testWidgets('ER-28: saving sends the trimmed text and a cleared date as null', (tester) async {
    final service = await pump(
        tester, _election(formDetails: ElectionFormDetails(finalListOn: DateTime(2026, 11, 2), venue: 'Hall')));

    await tester.tap(find.byKey(const Key('form-finalListOn-clear')));
    await tester.enterText(find.byKey(const Key('form-venue')), '  Main hall ');
    await tester.enterText(find.byKey(const Key('form-noticeReference')), '   ');
    await tester.tap(find.byKey(const Key('form-save')));
    await settle(tester);

    expect(service.calls, contains('details 12'));
    expect(service.savedDetails!.finalListOn, isNull);
    expect(service.savedDetails!.venue, 'Main hall');
    expect(service.savedDetails!.noticeReference, isNull);
    expect(find.text(ElectionConstants.formDetailsSaved), findsOneWidget);
  });

  testWidgets('ER-11: refusing needs a reason and allowing does not', (tester) async {
    final service = await pump(tester, _election(), recounts: const [_pending]);

    await tester.tap(find.byKey(const Key('recount-6-allow')));
    await settle(tester);
    expect(service.calls, contains('decide 6 true null'));
    expect(service.calls.last, 'recounts 12');
    ScaffoldMessenger.of(tester.element(find.byType(Scaffold))).removeCurrentSnackBar();
    await tester.pump();

    await tester.tap(find.byKey(const Key('recount-6-refuse')));
    await settle(tester);
    expect(find.text(ElectionConstants.recountRefuseReasonRequired), findsOneWidget);
    expect(service.calls.where((c) => c.startsWith('decide 6 false')), isEmpty);

    await tester.enterText(find.byKey(const Key('recount-6-reason')), ' Margin too wide ');
    await tester.tap(find.byKey(const Key('recount-6-refuse')));
    await settle(tester);
    expect(service.calls, contains('decide 6 false Margin too wide'));
  });

  testWidgets('ER-11: recording stays off until seat, applicant, grounds and time are given',
      (tester) async {
    await pump(tester, _election());

    ElevatedButton record() => tester.widget<ElevatedButton>(find.byKey(const Key('recount-record')));
    expect(record().onPressed, isNull);

    await tester.enterText(find.byKey(const Key('recount-applicant')), 'Rina');
    await tester.enterText(find.byKey(const Key('recount-grounds')), 'Close margin');
    await tester.pump();
    expect(record().onPressed, isNull);
  });

  testWidgets('ER-11: figures prefill from the recount seat and save whole numbers only',
      (tester) async {
    const allowed = ElectionRecount(
        id: 6,
        electionSeatId: 3,
        positionTitle: 'President',
        applicantName: 'Rina',
        grounds: 'Close margin',
        allowed: true);
    final service = await pump(
        tester, _election(phase: 'Declared', permissions: const ['ViewDashboard', 'Count']),
        recounts: const [allowed]);

    await tester.tap(find.byKey(const Key('recount-6-figures')));
    await settle(tester);

    expect(find.byKey(const Key('figure-4')), findsOneWidget);
    expect(find.byKey(const Key('figure-5')), findsOneWidget);
    expect(find.byKey(const Key('figure-9')), findsNothing);

    TextButton save() => tester.widget<TextButton>(find.byKey(const Key('recount-6-save-figures')));
    await tester.enterText(find.byKey(const Key('figure-5')), '-1');
    await tester.pump();
    expect(save().onPressed, isNull);

    await tester.enterText(find.byKey(const Key('figure-5')), '6');
    await tester.pump();
    await tester.tap(find.byKey(const Key('recount-6-save-figures')));
    await settle(tester);

    expect(service.calls, contains('figures 6'));
    expect(service.savedFigures!.map((f) => '${f.nominationId}:${f.voteCount}'), ['4:5', '5:6']);
  });

  testWidgets('ER-11: a recount seat with no declared result closes the figures and says why',
      (tester) async {
    const noResult = ElectionRecount(
        id: 6, electionSeatId: 99, positionTitle: 'Treasurer', applicantName: 'Rina', grounds: 'G', allowed: true);
    await pump(tester, _election(phase: 'Declared', permissions: const ['ViewDashboard', 'Count']),
        recounts: const [noResult]);

    await tester.tap(find.byKey(const Key('recount-6-figures')));
    await settle(tester);

    expect(find.text(ElectionConstants.recountNoResult), findsOneWidget);
    expect(find.byKey(const Key('recount-6-save-figures')), findsNothing);
    expect(find.byKey(const Key('recount-6-figures')), findsOneWidget);
  });

  test('ER-11: figures need a declared result, an allowed recount with none yet, and Count', () {
    const allowed = ElectionRecount(
        id: 6, electionSeatId: 3, positionTitle: 'P', applicantName: 'A', grounds: 'G', allowed: true);
    final declared = _election(phase: 'Declared', permissions: const ['Count']);

    expect(canEnterRecountFigures(declared, allowed), isTrue);
    expect(canEnterRecountFigures(_election(phase: 'Counting', permissions: const ['Count']), allowed),
        isFalse);
    expect(canEnterRecountFigures(_election(phase: 'Declared', permissions: const []), allowed), isFalse);
    expect(canEnterRecountFigures(declared, _pending), isFalse);
    expect(
        canEnterRecountFigures(
            declared,
            const ElectionRecount(
                id: 6,
                electionSeatId: 3,
                positionTitle: 'P',
                applicantName: 'A',
                grounds: 'G',
                allowed: true,
                figures: [RecountFigure(nominationId: 4, voteCount: 5)])),
        isFalse);
  });

  testWidgets('ER-28: a nomination with no consent says so, and scrutiny ticks save in scrutiny',
      (tester) async {
    final service = await pump(tester, _election(phase: 'Scrutiny'));

    expect(find.text(ElectionConstants.consentMissing), findsOneWidget);

    await tester.tap(find.byKey(const Key('nomination-4-scrutiny')));
    await settle(tester);
    expect(service.calls, contains('checks 4'));
    expect(
        tester.widget<CheckboxListTile>(find.byKey(const Key('scrutiny-MembershipVerified'))).value, isTrue);

    await tester.tap(find.byKey(const Key('scrutiny-VotingMember')));
    await settle(tester);
    expect(service.calls.last, 'tick 4 VotingMember true');
    expect(tester.widget<CheckboxListTile>(find.byKey(const Key('scrutiny-VotingMember'))).value, isTrue);
  });

  testWidgets('ER-28: scrutiny ticks are read-only outside the scrutiny phase', (tester) async {
    await pump(tester, _election(phase: 'Polling'));

    await tester.tap(find.byKey(const Key('nomination-4-scrutiny')));
    await settle(tester);

    expect(
        tester.widget<CheckboxListTile>(find.byKey(const Key('scrutiny-VotingMember'))).onChanged, isNull);
  });

  // 84.16. The scrutiny decision itself, not just the ticks.
  testWidgets('FR-021 (spec 018): accepting a nomination under scrutiny records it and reloads the list', (tester) async {
    final service = await pump(tester, _election(phase: 'Scrutiny'), nominationStatus: 'UnderScrutiny');

    await tester.tap(find.byKey(const Key('nomination-4-accept')));
    await settle(tester);

    expect(service.calls, contains('scrutinise 4 true'));
    expect(service.calls.lastIndexOf('nominations 12'),
        greaterThan(service.calls.indexOf('scrutinise 4 true')));
    expect(find.text('Status: Accepted'), findsOneWidget);
    expect(find.byKey(const Key('nomination-4-accept')), findsNothing);
  });

  testWidgets('FR-021 (spec 018): a decided nomination gets no accept or reject', (tester) async {
    await pump(tester, _election(phase: 'Scrutiny'));
    expect(find.text('Status: Accepted'), findsOneWidget);
    expect(find.byKey(const Key('nomination-4-reject')), findsNothing);
  });

  testWidgets('FR-021 (spec 018): a caller who cannot decide nominations gets no accept or reject', (tester) async {
    await pump(tester, _election(phase: 'Scrutiny', permissions: const ['ManageSetup']),
        nominationStatus: 'Submitted');
    expect(find.text('Status: Submitted'), findsOneWidget);
    expect(find.byKey(const Key('nomination-4-reject')), findsNothing);
  });
}
