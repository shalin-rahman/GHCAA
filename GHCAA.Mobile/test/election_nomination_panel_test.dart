import 'dart:async';
import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:ghcaa_mobile/features/elections/election_service.dart';
import 'package:ghcaa_mobile/features/networking/networking_service.dart';
import 'package:ghcaa_mobile/features/theme/dynamic_theme_service.dart';
import 'package:ghcaa_mobile/screens/member/election_nomination_panel.dart';

class _FakeElectionService extends ElectionService {
  _FakeElectionService(this.nominations) : super(Dio());

  List<Nomination> nominations;
  Map<String, dynamic>? filed;
  int? withdrawn;
  bool refuse = false;

  @override
  Future<List<ElectionSeatView>> getSeats(int id) async => const [
        ElectionSeatView(seatId: 10, positionTitle: 'President', places: 1),
        ElectionSeatView(seatId: 20, positionTitle: 'Members', places: 2),
      ];

  @override
  Future<List<Nomination>> getNominations(int id) async => nominations;

  @override
  Future<Nomination> nominate(int id, Map<String, dynamic> data) async {
    if (refuse) {
      final options = RequestOptions(path: '/elections/$id/nominations');
      throw DioException(
          requestOptions: options,
          response: Response(
              requestOptions: options,
              statusCode: 400,
              data: {'detail': 'The seconder is not on the voter roll.'}));
    }
    filed = data;
    return const Nomination(id: 99, electionSeatId: 10, candidateMemberId: 3, status: 'Submitted', statement: '');
  }

  @override
  Future<bool> withdrawNomination(int nominationId) async {
    withdrawn = nominationId;
    nominations = [
      for (final n in nominations)
        n.id == nominationId
            ? Nomination(id: n.id, electionSeatId: n.electionSeatId, candidateMemberId: n.candidateMemberId, status: 'Withdrawn', statement: '')
            : n
    ];
    return true;
  }
}

class _FakeNetworkingService extends NetworkingService {
  _FakeNetworkingService() : super(Dio());

  final queries = <String?>[];
  final replies = <String, Completer<Map<String, dynamic>>>{};

  @override
  Future<Map<String, dynamic>> searchAlumni({
    String? query,
    String? batch,
    String? department,
    String? membershipType,
    String? category,
    String? cursor,
    int pageSize = 20,
  }) {
    queries.add(query);
    final held = replies[query];
    if (held != null) return held.future;
    return Future.value(_page(query == 'Dev' ? 4 : 3, query == 'Dev' ? 'Dev Roy' : 'Chitra Das'));
  }
}

Map<String, dynamic> _page(int id, String name) => {
      'items': [
        {'id': id, 'fullName': name, 'membershipNumber': 'M-$id'},
      ],
    };

Nomination _nomination(int id, int candidate, String status, {int seat = 10}) =>
    Nomination(id: id, electionSeatId: seat, candidateMemberId: candidate, status: status, statement: '');

// FR-37.1b. Members file nominations and candidates withdraw them from the app.
void main() {
  setUpAll(() => dotenv.testLoad(fileInput: 'PORTAL_TITLE=GHCAA'));

  late _FakeElectionService elections;
  late _FakeNetworkingService networking;

  Future<void> pump(WidgetTester tester, String phase, [List<Nomination> nominations = const []]) async {
    tester.view.physicalSize = const Size(390, 2400);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.reset);
    elections = _FakeElectionService(nominations);
    networking = _FakeNetworkingService();
    await tester.pumpWidget(ProviderScope(
      overrides: [
        electionServiceProvider.overrideWith((ref) => elections),
        networkingServiceProvider.overrideWith((ref) => networking),
        userProfileProvider.overrideWith((ref) async => {'id': 7}),
        activeSpecialThemeProvider.overrideWith((ref) async => null),
      ],
      child: MaterialApp(
        theme: AppTheme.midnightTheme,
        home: Scaffold(
          body: SingleChildScrollView(child: ElectionNominationPanel(electionId: 5, phase: phase)),
        ),
      ),
    ));
    await tester.pump();
    await tester.pump();
  }

  Future<void> pickMember(WidgetTester tester, int fieldIndex, String query, String name) async {
    await tester.enterText(find.byType(TextField).at(fieldIndex), query);
    await tester.pump(const Duration(milliseconds: 400));
    await tester.pump();
    await tester.tap(find.text(name));
    await tester.pump();
  }

  ElevatedButton fileButton(WidgetTester tester) =>
      tester.widget<ElevatedButton>(find.widgetWithText(ElevatedButton, 'FILE NOMINATION'));

  testWidgets('files a nomination with the signed-in member as proposer, then resets', (tester) async {
    await pump(tester, 'Nomination');

    await tester.tap(find.byType(DropdownButtonFormField<int>));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Members (2 places)').last);
    await tester.pumpAndSettle();
    await pickMember(tester, 0, 'Chi', 'Chitra Das');
    expect(fileButton(tester).onPressed, isNull);
    await pickMember(tester, 0, 'Dev', 'Dev Roy');
    await tester.enterText(find.byKey(const ValueKey('nomination-statement')), '  I will serve.  ');
    await tester.pump();

    await tester.tap(find.text('FILE NOMINATION'));
    await tester.pump();
    await tester.pump();

    expect(elections.filed, {
      'electionSeatId': 20,
      'candidateMemberId': 3,
      'proposerMemberId': 7,
      'seconderMemberId': 4,
      'statement': 'I will serve.',
    });
    expect(find.textContaining('Nomination filed'), findsOneWidget);
    expect(find.text('Candidate: Chitra Das'), findsNothing);
    expect(fileButton(tester).onPressed, isNull);
  });

  testWidgets('keeps the form and shows the reason when the server rejects it', (tester) async {
    await pump(tester, 'Nomination');
    elections.refuse = true;

    await tester.tap(find.byType(DropdownButtonFormField<int>));
    await tester.pumpAndSettle();
    await tester.tap(find.text('President').last);
    await tester.pumpAndSettle();
    await pickMember(tester, 0, 'Chi', 'Chitra Das');
    await pickMember(tester, 0, 'Dev', 'Dev Roy');
    await tester.enterText(find.byKey(const ValueKey('nomination-statement')), 'Statement');
    await tester.pump();
    await tester.tap(find.text('FILE NOMINATION'));
    await tester.pump();
    await tester.pump();

    expect(find.text('The seconder is not on the voter roll.'), findsOneWidget);
    expect(find.text('Candidate: Chitra Das'), findsOneWidget);
  });

  testWidgets('skips a search under two characters', (tester) async {
    await pump(tester, 'Nomination');

    await tester.enterText(find.byType(TextField).first, 'C');
    await tester.pump(const Duration(milliseconds: 400));

    expect(networking.queries, isEmpty);
  });

  testWidgets('drops a reply for a query the member has already changed', (tester) async {
    await pump(tester, 'Nomination');
    final slow = Completer<Map<String, dynamic>>();
    networking.replies['Chi'] = slow;

    await tester.enterText(find.byType(TextField).first, 'Chi');
    await tester.pump(const Duration(milliseconds: 400));
    await tester.enterText(find.byType(TextField).first, 'Chx');
    slow.complete(_page(3, 'Chitra Das'));
    await tester.pump();

    expect(find.text('Chitra Das'), findsNothing);
  });

  testWidgets('shows only the signed-in member\'s nominations, named by seat', (tester) async {
    await pump(tester, 'Nomination', [_nomination(1, 7, 'Submitted', seat: 20), _nomination(2, 8, 'Submitted')]);

    expect(find.byKey(const ValueKey('nomination-1')), findsOneWidget);
    expect(find.byKey(const ValueKey('nomination-2')), findsNothing);
    expect(find.descendant(of: find.byKey(const ValueKey('nomination-1')), matching: find.text('Members')), findsOneWidget);
    expect(find.text('WITHDRAW'), findsNothing);
  });

  testWidgets('hides the form and offers withdraw on accepted nominations in the withdrawal phase', (tester) async {
    await pump(tester, 'Withdrawal', [_nomination(1, 7, 'Accepted'), _nomination(2, 7, 'Rejected', seat: 20)]);

    expect(find.text('FILE NOMINATION'), findsNothing);
    expect(find.descendant(of: find.byKey(const ValueKey('nomination-1')), matching: find.text('WITHDRAW')), findsOneWidget);
    expect(find.descendant(of: find.byKey(const ValueKey('nomination-2')), matching: find.text('WITHDRAW')), findsNothing);
  });

  testWidgets('withdraws after the member confirms', (tester) async {
    await pump(tester, 'Withdrawal', [_nomination(1, 7, 'Accepted')]);

    await tester.tap(find.text('WITHDRAW'));
    await tester.pumpAndSettle();
    await tester.tap(find.widgetWithText(ElevatedButton, 'WITHDRAW'));
    await tester.pumpAndSettle();

    expect(elections.withdrawn, 1);
    expect(find.text('WITHDRAW'), findsNothing);
  });

  testWidgets('does not withdraw when the member cancels', (tester) async {
    await pump(tester, 'Withdrawal', [_nomination(1, 7, 'Accepted')]);

    await tester.tap(find.text('WITHDRAW'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('CANCEL'));
    await tester.pumpAndSettle();

    expect(elections.withdrawn, isNull);
  });
}
