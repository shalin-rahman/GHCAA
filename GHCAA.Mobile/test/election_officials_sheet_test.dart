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
import 'package:ghcaa_mobile/screens/admin/election_officials_sheet.dart';

class _FakeElectionService extends ElectionService {
  _FakeElectionService() : super(Dio());

  @override
  Future<List<ElectionAppointment>> getAppointments(int electionId) async =>
      const [];

  @override
  Future<List<ElectionPersonaOption>> getPersonas() async => const [];
}

class _FakeNetworkingService extends NetworkingService {
  _FakeNetworkingService() : super(Dio());

  final queries = <String?>[];

  @override
  Future<Map<String, dynamic>> searchAlumni({
    String? query,
    String? batch,
    String? department,
    String? membershipType,
    String? category,
    String? cursor,
    int pageSize = 20,
  }) async {
    queries.add(query);
    return {
      'items': [
        {'id': 41, 'fullName': 'Ann Rahman', 'membershipNumber': 'GHC-041'},
      ],
    };
  }
}

void main() {
  setUpAll(() => dotenv.testLoad(fileInput: 'PORTAL_TITLE=GHCAA'));

  // FR-37.12d. A commissioner is not an admin, so the search must not use the admin member list.
  testWidgets('member search uses the member directory', (tester) async {
    tester.view.physicalSize = const Size(390, 1400);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.reset);
    final networking = _FakeNetworkingService();
    await tester.pumpWidget(ProviderScope(
      overrides: [
        electionServiceProvider.overrideWith((ref) => _FakeElectionService()),
        networkingServiceProvider.overrideWith((ref) => networking),
        isSuperAdminProvider.overrideWith((ref) async => false),
        activeSpecialThemeProvider.overrideWith((ref) async => null),
      ],
      child: MaterialApp(
        theme: AppTheme.midnightTheme,
        home: const Scaffold(
          body: ElectionOfficialsSheet(
            election: AdminElection(
                id: 5,
                title: 'EC 2026',
                phase: 'Nomination',
                isActive: true,
                eligibleVoterCount: 0),
          ),
        ),
      ),
    ));
    await tester.pump();

    await tester.enterText(find.byType(TextField).first, ' Ann ');
    await tester.pump(const Duration(milliseconds: 400));
    await tester.pump();

    expect(networking.queries, ['Ann']);
    expect(find.text('Ann Rahman'), findsOneWidget);
  });
}
