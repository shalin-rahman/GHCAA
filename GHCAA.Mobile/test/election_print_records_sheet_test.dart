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
import 'package:ghcaa_mobile/screens/admin/election_print_records_sheet.dart';

class _FakeElectionService extends ElectionService {
  _FakeElectionService(this.rows, {this.fail = false}) : super(Dio());

  final List<ElectionFormPrintRow> rows;
  final bool fail;
  final calls = <String>[];

  @override
  Future<List<ElectionFormPrintRow>> getPrintRecords(int id) async {
    calls.add('prints $id');
    if (fail) throw DioException(requestOptions: RequestOptions(path: '/'));
    return rows;
  }
}

const _election = AdminElection(
  id: 12,
  title: 'EC 2026',
  phase: ElectionPhases.counting,
  isActive: true,
  eligibleVoterCount: 10,
  myPermissions: ['Count', 'ManageSetup'],
);

final _rows = [
  ElectionFormPrintRow(
      id: 'a1',
      formCode: 'ER-10',
      printedAt: DateTime.utc(2026, 11, 20, 9),
      printedByRole: 'Returning Officer',
      phaseAtPrint: ElectionPhases.counting),
  ElectionFormPrintRow(
      id: 'b2',
      formCode: 'ER-12',
      printedAt: DateTime.utc(2026, 11, 19, 9),
      printedByRole: 'Returning Officer',
      phaseAtPrint: ElectionPhases.polling,
      revokedAt: DateTime.utc(2026, 11, 21, 10),
      revokeReason: 'Misprint'),
];

// 37.16i. FR-39: officials see the print records on mobile, read only.
void main() {
  setUpAll(() => dotenv.testLoad(fileInput: 'PORTAL_TITLE=GHCAA'));

  Future<_FakeElectionService> pump(WidgetTester tester, _FakeElectionService service) async {
    tester.view.physicalSize = const Size(390, 2000);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.reset);
    await tester.pumpWidget(ProviderScope(
      overrides: [
        electionServiceProvider.overrideWith((ref) => service),
        orgDateFormatProvider.overrideWith((ref) => OrgConfig.offlineDefaults.dateFormat),
      ],
      child: MaterialApp(
        theme: AppTheme.midnightTheme,
        home: const Scaffold(body: ElectionPrintRecordsSheet(election: _election)),
      ),
    ));
    for (var i = 0; i < 5; i++) {
      await tester.pump(const Duration(milliseconds: 100));
    }
    return service;
  }

  testWidgets('FR-39 (37.16i): shows the list and no revoke action', (tester) async {
    final service = await pump(tester, _FakeElectionService(_rows));

    expect(service.calls, ['prints 12']);
    expect(find.byKey(const Key('print-a1')), findsOneWidget);
    expect(find.byKey(const Key('print-b2')), findsOneWidget);
    expect(find.text('ER-10'), findsOneWidget);
    expect(find.textContaining('Misprint'), findsOneWidget);
    expect(find.text('Phase Voting open'), findsOneWidget);
    expect(find.text(ElectionConstants.printRecordsWebOnly), findsOneWidget);
    expect(find.byType(TextField), findsNothing);
    expect(find.byType(ElevatedButton), findsNothing);
    expect(find.byType(TextButton), findsNothing);
  });

  testWidgets('FR-39 (37.16i): says so when nothing was printed', (tester) async {
    await pump(tester, _FakeElectionService(const []));
    expect(find.text(ElectionConstants.printRecordsEmpty), findsOneWidget);
  });

  testWidgets('FR-39 (37.16i): a failed load says so instead of showing an empty list', (tester) async {
    await pump(tester, _FakeElectionService(const [], fail: true));
    expect(find.text(ElectionConstants.printRecordsLoadFailed), findsOneWidget);
    expect(find.text(ElectionConstants.printRecordsEmpty), findsNothing);
  });
}
