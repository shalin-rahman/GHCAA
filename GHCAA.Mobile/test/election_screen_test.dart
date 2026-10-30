import 'dart:convert';
import 'dart:typed_data';
import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/core/storage/storage_service.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:ghcaa_mobile/features/elections/election_service.dart';
import 'package:ghcaa_mobile/features/theme/dynamic_theme_service.dart';
import 'package:ghcaa_mobile/screens/member/election_screen.dart';

class _FakeElectionService extends ElectionService {
  _FakeElectionService() : super(Dio());

  Map<int, List<int>>? cast;
  bool hasVoted = false;

  @override
  Future<Ballot> getBallot(int id) async => Ballot(
        electionId: 12,
        title: 'EC 2026',
        phase: 'Polling',
        hasVoted: hasVoted,
        seats: const [
          BallotSeat(
              seatId: 3,
              positionTitle: 'Committee',
              places: 2,
              candidates: [
                BallotCandidate(nominationId: 10, name: 'Rina'),
                BallotCandidate(nominationId: 11, name: 'Tanvir'),
                BallotCandidate(nominationId: 12, name: 'Maya'),
              ]),
        ],
      );

  @override
  Future<ElectionBoard?> getBoard(int id) async => const ElectionBoard(
        isTest: false,
        groups: [
          BoardGroup(groupName: 'Commission', members: [
            BoardMember(
                personaName: 'Chief Election Commissioner',
                displayName: 'Rahim Uddin'),
          ]),
        ],
      );

  @override
  Future<CastBallotResult> castBallot(
      int id, Map<int, List<int>> choices) async {
    cast = choices;
    return (trackingCode: 'ABCD-EFGH', receiptPdf: _pdf);
  }
}

class _ReceiptStorage extends StorageService {
  _ReceiptStorage({this.failSave = false});

  final bool failSave;

  @override
  Future<void> saveBallotReceipt(int electionId, String pdfBase64) async {
    if (failSave) throw StateError('Storage unavailable');
    await super.saveBallotReceipt(electionId, pdfBase64);
  }
}

final _pdf = Uint8List.fromList(utf8.encode('%PDF-1.7'));

void main() {
  late _FakeElectionService service;

  setUpAll(() => dotenv.testLoad(fileInput: 'PORTAL_TITLE=GHCAA'));

  Future<void> pump(WidgetTester tester,
      {bool hasVoted = false,
      bool failReceiptSave = false,
      Map<String, Object> prefs = const {}}) async {
    tester.view.physicalSize = const Size(390, 1400);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.reset);
    SharedPreferences.setMockInitialValues(prefs);
    service = _FakeElectionService()..hasVoted = hasVoted;
    await tester.pumpWidget(ProviderScope(
      overrides: [
        electionServiceProvider.overrideWith((ref) => service),
        storageServiceProvider
            .overrideWith((ref) => _ReceiptStorage(failSave: failReceiptSave)),
        currentElectionProvider
            .overrideWith((ref) async => const ElectionSummary(
                  id: 12,
                  title: 'EC 2026',
                  phase: 'Polling',
                  ecPeriodId: 2,
                  voterCount: 0,
                  eligibleVoterCount: 10,
                )),
        roleProvider.overrideWith((ref) => Future.value('Member')),
        activeSpecialThemeProvider.overrideWith((ref) async => null),
      ],
      child: MaterialApp.router(
        theme: AppTheme.midnightTheme,
        routerConfig: GoRouter(routes: [
          GoRoute(path: '/', builder: (_, __) => const ElectionScreen()),
          GoRoute(
              path: '/election/documents',
              builder: (_, __) => const Text('Documents route')),
        ]),
      ),
    ));
    for (var i = 0; i < 5; i++) {
      await tester.pump(const Duration(milliseconds: 100));
    }
  }

  testWidgets('a third pick on a two-place seat is blocked', (tester) async {
    await pump(tester);

    expect(find.textContaining('Pick up to 2'), findsOneWidget);
    await tester.tap(find.text('Rina'));
    await tester.pump();
    await tester.tap(find.text('Tanvir'));
    await tester.pump();
    await tester.tap(find.text('Maya'));
    await tester.pump();

    expect(find.byIcon(Icons.check_box), findsNWidgets(2));
    expect(find.textContaining('2 picked'), findsOneWidget);
  });

  // FR-33
  testWidgets('the officials list shows names and roles at phone width',
      (tester) async {
    await pump(tester);

    expect(find.text('Who runs this election'), findsOneWidget);
    expect(find.text('Commission'), findsOneWidget);
    expect(
        find.text('Rahim Uddin, Chief Election Commissioner'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });

  // FR-25. 37.14s
  testWidgets('the ballot links to the election rules and forms',
      (tester) async {
    await pump(tester);

    await tester.ensureVisible(find.text('Rules and forms'));
    await tester.tap(find.text('Rules and forms'));
    for (var i = 0; i < 3; i++) {
      await tester.pump(const Duration(milliseconds: 100));
    }

    expect(find.text('Documents route'), findsOneWidget);
  });

  testWidgets('abstain clears the picks on the seat', (tester) async {
    await pump(tester);

    await tester.tap(find.text('Rina'));
    await tester.pump();
    await tester.tap(find.text('Abstain on this seat'));
    await tester.pump();

    expect(find.byIcon(Icons.check_box), findsNothing);
    expect(find.textContaining('0 picked'), findsOneWidget);
  });

  // 37.14o
  testWidgets('a cast keeps the server receipt and offers it for download',
      (tester) async {
    await pump(tester);

    await tester.tap(find.text('Rina'));
    await tester.pump();
    await tester.tap(find.text('Abstain on this seat'));
    await tester.pump();
    await tester.ensureVisible(find.text('Review ballot'));
    await tester.tap(find.text('Review ballot'));
    await tester.pump();
    await tester.ensureVisible(find.text('Confirm and cast ballot'));
    await tester.tap(find.text('Confirm and cast ballot'));
    for (var i = 0; i < 5; i++) {
      await tester.pump(const Duration(milliseconds: 100));
    }

    expect(find.text('Download receipt'), findsOneWidget);
    final prefs = await SharedPreferences.getInstance();
    expect(prefs.getString('ballot_receipt_12'), base64Encode(_pdf));
  });

  // 37.14o
  testWidgets('a member who already voted can download the stored receipt',
      (tester) async {
    await pump(tester,
        hasVoted: true, prefs: {'ballot_receipt_12': base64Encode(_pdf)});

    expect(
        find.text('You have already voted in this election.'), findsOneWidget);
    expect(find.text('Download receipt'), findsOneWidget);
  });

  testWidgets('warns when the receipt cannot be kept on the device',
      (tester) async {
    await pump(tester, failReceiptSave: true);

    await tester.tap(find.text('Rina'));
    await tester.pump();
    await tester.tap(find.text('Abstain on this seat'));
    await tester.pump();
    await tester.ensureVisible(find.text('Review ballot'));
    await tester.tap(find.text('Review ballot'));
    await tester.pump();
    await tester.ensureVisible(find.text('Confirm and cast ballot'));
    await tester.tap(find.text('Confirm and cast ballot'));
    for (var i = 0; i < 5; i++) {
      await tester.pump(const Duration(milliseconds: 100));
    }

    expect(find.text('Download receipt'), findsOneWidget);
    expect(
        find.textContaining('It was not saved on this device'), findsOneWidget);
  });

  testWidgets('no download button without a stored receipt', (tester) async {
    await pump(tester, hasVoted: true);

    expect(find.text('Download receipt'), findsNothing);
  });
}
