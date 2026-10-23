import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:ghcaa_mobile/features/elections/ballot_key.dart';
import 'package:ghcaa_mobile/features/elections/election_service.dart';
import 'package:ghcaa_mobile/features/theme/dynamic_theme_service.dart';
import 'package:ghcaa_mobile/screens/admin/election_management_screen.dart';

class _FakeElectionService extends ElectionService {
  _FakeElectionService() : super(Dio());

  final calls = <String>[];
  bool waiting = false;

  @override
  Future<bool> publish(int id) async {
    calls.add('publish $id');
    return waiting;
  }

  @override
  Future<bool> close(int id) async {
    calls.add('close $id');
    return waiting;
  }

  @override
  Future<void> removeCandidate(int electionId, int candidateId) async {
    calls.add('remove $electionId $candidateId');
  }

  @override
  Future<bool> setBallotKey(int id, String publicKey) async {
    calls.add('ballot-key $id $publicKey');
    return waiting;
  }

  @override
  Future<({List<dynamic>? results, ElectionApproval? pending})> runCount(
      int id, String privateKey) async {
    calls.add('count $id $privateKey');
    if (!waiting) return (results: <dynamic>[{}, {}], pending: null);
    return (
      results: null,
      pending: ElectionApproval(
          id: 3,
          electionId: id,
          action: 'Count',
          requestedBy: 'Officer',
          requestedAt: DateTime.utc(2026, 10, 23),
          expiresAt: DateTime.utc(2026, 10, 24)),
    );
  }
}

// Stands in for the isolate and the platform file dialogs.
class _FakeKeyTools extends BallotKeyTools {
  final saved = <String, String>{};
  bool saveCancelled = false;
  String? picked;

  @override
  Future<BallotKeyPair> generate() async =>
      const BallotKeyPair(publicKey: 'SPKI', privateKey: 'PKCS8');

  @override
  Future<bool> save(String fileName, String pem) async {
    if (saveCancelled) return false;
    saved[fileName] = pem;
    return true;
  }

  @override
  Future<String?> pick() async => picked;
}

const _allPermissions = [
  'ChangePhase',
  'ManageSetup',
  'ManageVoterRoll',
  'Count',
  'Declare',
  'SetBallotKey',
  'ViewDashboard',
  'AppointOfficials',
];

AdminElection _election(String phase,
        {List<String> permissions = _allPermissions, String? fingerprint = 'ab12'}) =>
    AdminElection(
      id: 12,
      title: 'EC 2026',
      phase: phase,
      isActive: true,
      eligibleVoterCount: 40,
      ballotKeyFingerprint: fingerprint,
      myPermissions: permissions,
      candidates: const [
        AdminElectionCandidate(id: 7, name: 'Rina', positionTitle: 'President'),
      ],
    );

// TODO 84.16. Admin steps on the phone, gated the way the web gates them.
void main() {
  setUpAll(() => dotenv.testLoad(fileInput: 'PORTAL_TITLE=GHCAA'));

  Future<void> settle(WidgetTester tester) async {
    for (var i = 0; i < 5; i++) {
      await tester.pump(const Duration(milliseconds: 100));
    }
  }

  Future<_FakeElectionService> pump(WidgetTester tester, AdminElection election,
      {BallotKeyTools tools = const BallotKeyTools()}) async {
    tester.view.physicalSize = const Size(390, 2400);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.reset);
    final service = _FakeElectionService();
    await tester.pumpWidget(ProviderScope(
      overrides: [
        adminElectionsProvider.overrideWith((ref) async => [election]),
        electionServiceProvider.overrideWith((ref) => service),
        ballotKeyToolsProvider.overrideWith((ref) => tools),
        roleProvider.overrideWith((ref) => Future.value('Admin')),
        isSuperAdminProvider.overrideWith((ref) async => false),
        activeSpecialThemeProvider.overrideWith((ref) async => null),
      ],
      child: MaterialApp.router(
        theme: AppTheme.midnightTheme,
        routerConfig: GoRouter(routes: [
          GoRoute(path: '/', builder: (_, __) => const ElectionManagementScreen()),
        ]),
      ),
    ));
    await settle(tester);
    return service;
  }

  Finder key(String k) => find.byKey(Key(k));

  testWidgets('an announced election offers publish and roll edits, not close or count',
      (tester) async {
    await pump(tester, _election('Announced'));

    expect(key('action-publish'), findsOneWidget);
    expect(key('action-freeze'), findsOneWidget);
    expect(key('action-advance'), findsNothing);
    expect(key('action-close'), findsNothing);
    expect(key('action-count'), findsNothing);
    expect(key('action-declare'), findsNothing);
  });

  testWidgets('FR-029 (spec 018): publishing asks first, then calls the admin route', (tester) async {
    final service = await pump(tester, _election('Announced'));

    await tester.tap(key('action-publish'));
    await settle(tester);
    await tester.tap(find.text('CONFIRM'));
    await settle(tester);

    expect(service.calls, ['publish 12']);
  });

  testWidgets('FR-030 (spec 018): a two-person close says it waits for approval', (tester) async {
    final service = await pump(tester, _election('Polling'));
    service.waiting = true;

    expect(key('action-advance'), findsNothing);
    await tester.tap(key('action-close'));
    await settle(tester);
    await tester.tap(find.text('CONFIRM'));
    await settle(tester);

    expect(service.calls, ['close 12']);
    expect(find.textContaining('A second official must approve'), findsOneWidget);
  });

  testWidgets('counting offers count and declare, and no phase advance', (tester) async {
    await pump(tester, _election('Counting'));

    expect(key('action-count'), findsOneWidget);
    expect(key('action-declare'), findsOneWidget);
    expect(key('action-advance'), findsNothing);
    expect(key('action-freeze'), findsNothing);
  });

  testWidgets('FR-39 (spec 023, 84.44): with no key yet the button offers to make one',
      (tester) async {
    await pump(tester, _election('Campaign', fingerprint: null));
    expect(find.text('Make officer key'), findsOneWidget);
    expect(find.text('No returning officer key yet. Voting cannot open without one.'),
        findsOneWidget);
  });

  testWidgets('FR-39 (spec 023, 84.44): an existing key offers replace', (tester) async {
    await pump(tester, _election('Announced'));
    expect(find.text('Replace officer key'), findsOneWidget);
  });

  testWidgets('FR-39 (spec 023, 84.44): no key button from polling on', (tester) async {
    await pump(tester, _election('Polling'));
    expect(key('action-ballot-key'), findsNothing);
  });

  testWidgets('FR-39 (spec 023, 84.44): no key button without SetBallotKey', (tester) async {
    await pump(tester, _election('Announced', permissions: const ['ChangePhase']));
    expect(key('action-ballot-key'), findsNothing);
  });

  testWidgets('FR-39 (spec 023, 84.44): making a key saves the PEM file, then sends only the public key',
      (tester) async {
    final tools = _FakeKeyTools();
    final service = await pump(tester, _election('Campaign', fingerprint: null), tools: tools);

    await tester.tap(key('action-ballot-key'));
    await settle(tester);
    expect(find.text('Make returning officer key'), findsOneWidget);
    await tester.tap(find.text('MAKE KEY'));
    await settle(tester);

    expect(tools.saved.keys.single, 'ec-2026-returning-officer-key.pem');
    expect(tools.saved.values.single, ballotKeyPem('PKCS8'));
    expect(service.calls, ['ballot-key 12 SPKI']);
    expect(find.textContaining('Key stored.'), findsOneWidget);
  });

  testWidgets('FR-39 (spec 023, 84.44): a cancelled save sends nothing', (tester) async {
    final tools = _FakeKeyTools()..saveCancelled = true;
    final service = await pump(tester, _election('Campaign', fingerprint: null), tools: tools);

    await tester.tap(key('action-ballot-key'));
    await settle(tester);
    await tester.tap(find.text('MAKE KEY'));
    await settle(tester);

    expect(service.calls, isEmpty);
    expect(find.text('Key file not saved. Nothing was sent.'), findsOneWidget);
  });

  testWidgets('FR-39 (spec 023, 84.44): a replace held for approval says so', (tester) async {
    final tools = _FakeKeyTools();
    final service = await pump(tester, _election('Campaign'), tools: tools);
    service.waiting = true;

    await tester.tap(key('action-ballot-key'));
    await settle(tester);
    expect(find.text('Replace returning officer key'), findsOneWidget);
    await tester.tap(find.text('MAKE KEY'));
    await settle(tester);

    expect(find.textContaining('A second person must approve the new key'), findsOneWidget);
  });

  testWidgets('FR-024 (spec 018, 84.44): counting reads the picked key file and reports the rows',
      (tester) async {
    final tools = _FakeKeyTools()..picked = ballotKeyPem('PKCS8').replaceAll('\n', '\r\n');
    final service = await pump(tester, _election('Counting'), tools: tools);

    await tester.tap(key('action-count'));
    await settle(tester);
    await tester.tap(find.text('CHOOSE KEY FILE'));
    await settle(tester);

    expect(service.calls, ['count 12 PKCS8']);
    expect(find.text('Count done. 2 result rows stored.'), findsOneWidget);
  });

  testWidgets('FR-024 (spec 018, 84.44): a held count tells the officer to count again', (tester) async {
    final tools = _FakeKeyTools()..picked = ballotKeyPem('PKCS8');
    final service = await pump(tester, _election('Counting'), tools: tools);
    service.waiting = true;

    await tester.tap(key('action-count'));
    await settle(tester);
    await tester.tap(find.text('CHOOSE KEY FILE'));
    await settle(tester);

    expect(find.textContaining('Then count again with the same key file.'), findsOneWidget);
  });

  testWidgets('FR-024 (spec 018, 84.44): no file picked means no count call', (tester) async {
    final tools = _FakeKeyTools();
    final service = await pump(tester, _election('Counting'), tools: tools);

    await tester.tap(key('action-count'));
    await settle(tester);
    await tester.tap(find.text('CHOOSE KEY FILE'));
    await settle(tester);

    expect(service.calls, isEmpty);
  });

  testWidgets('a declared election with a key offers key destroyed', (tester) async {
    await pump(tester, _election('Declared'));
    expect(key('action-key-destroyed'), findsOneWidget);
  });

  testWidgets('a declared election with no key offers no key destroyed', (tester) async {
    await pump(tester, _election('Declared', fingerprint: null));
    expect(key('action-key-destroyed'), findsNothing);
  });

  testWidgets('a viewer with no step permissions sees no step buttons', (tester) async {
    await pump(tester, _election('Announced', permissions: const ['ViewDashboard']));

    expect(key('action-publish'), findsNothing);
    expect(key('action-freeze'), findsNothing);
    expect(key('candidate-7'), findsOneWidget);
    expect(key('candidate-7-remove'), findsNothing);
    expect(key('action-approvals'), findsOneWidget);
  });

  testWidgets('FR-032 (spec 018): removing a candidate asks first, then calls the delete route',
      (tester) async {
    final service = await pump(tester, _election('Nomination'));

    await tester.tap(key('candidate-7-remove'));
    await settle(tester);
    await tester.tap(find.text('CONFIRM'));
    await settle(tester);

    expect(service.calls, ['remove 12 7']);
  });
}
