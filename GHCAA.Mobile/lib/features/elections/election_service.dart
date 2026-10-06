import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';
import '../../core/constants/app_constants.dart';
import '../../core/utils/app_utils.dart';

final electionServiceProvider =
    Provider<ElectionService>((ref) => ElectionService(ref.read(dioProvider)));

const electionPhaseLabels = <String, String>{
  'Announced': 'Announced',
  'Nomination': 'Nominations open',
  'Scrutiny': 'Under scrutiny',
  'Withdrawal': 'Withdrawal open',
  'CandidateList': 'Candidate list',
  'Campaign': 'Campaign',
  'Polling': 'Voting open',
  'Counting': 'Counting',
  'Declared': 'Declared',
  'Archived': 'Archived',
};

// Matches the backend ElectionPhase enum order (GHCAA.Domain.Enums), used to
// work out the next phase for the admin "advance phase" action.
const electionPhaseOrder = <String>[
  'Announced',
  'Nomination',
  'Scrutiny',
  'Withdrawal',
  'CandidateList',
  'Campaign',
  'Polling',
  'Counting',
  'Declared',
  'Archived',
];

String? nextElectionPhase(String phase) {
  final index = electionPhaseOrder.indexOf(phase);
  if (index == -1 || index >= electionPhaseOrder.length - 1) return null;
  return electionPhaseOrder[index + 1];
}

// 37.13t. Shown when polling is asked for and the server says too few officials can approve.
const electionTooFewApproversWarning =
    'Fewer than two officials can approve on this election. Once polling opens nobody can be appointed, so if an official has to be removed in an emergency there may be nobody left to approve it until the result is declared.';

// The warning to add when moving [election] to [next], or null.
String? pollingApproverWarning(AdminElection election, String next) =>
    next == 'Polling' && election.tooFewApprovers
        ? electionTooFewApproversWarning
        : null;

const nominationStatusLabels = <String, String>{
  'Submitted': 'Submitted',
  'UnderScrutiny': 'Under scrutiny',
  'Accepted': 'Accepted',
  'Rejected': 'Rejected',
  'Withdrawn': 'Withdrawn',
};

String electionPhaseLabel(String phase) => electionPhaseLabels[phase] ?? phase;
String nominationStatusLabel(String status) =>
    nominationStatusLabels[status] ?? status;

class ElectionSummary {
  final int id;
  final String title;
  final String phase;
  final int ecPeriodId;
  final int voterCount;
  final int eligibleVoterCount;
  final DateTime? announcedOn;
  final DateTime? pollingOpensOn;
  final DateTime? pollingClosesOn;
  // Spec 023 FR-001. Voters compare it with the one the commission announced.
  final String? ballotKeyFingerprint;

  const ElectionSummary({
    required this.id,
    required this.title,
    required this.phase,
    required this.ecPeriodId,
    required this.voterCount,
    required this.eligibleVoterCount,
    this.ballotKeyFingerprint,
    this.announcedOn,
    this.pollingOpensOn,
    this.pollingClosesOn,
  });

  factory ElectionSummary.fromJson(Map<String, dynamic> json) =>
      ElectionSummary(
        id: json['id'] as int,
        title: json['title'] as String? ?? '',
        phase: json['phase']?.toString() ?? 'Announced',
        ecPeriodId: json['ecPeriodId'] as int? ?? 0,
        voterCount: json['voterCount'] as int? ?? 0,
        eligibleVoterCount: json['eligibleVoterCount'] as int? ?? 0,
        ballotKeyFingerprint: json['ballotKeyFingerprint'] as String?,
        announcedOn: AppUtils.parseDate(json['announcedOn'] as String?),
        pollingOpensOn: AppUtils.parseDate(json['pollingOpensOn'] as String?),
        pollingClosesOn: AppUtils.parseDate(json['pollingClosesOn'] as String?),
      );
}

class Nomination {
  final int id;
  final int electionSeatId;
  final int candidateMemberId;
  final String status;
  final String statement;
  final String? photoPath;

  const Nomination({
    required this.id,
    required this.electionSeatId,
    required this.candidateMemberId,
    required this.status,
    required this.statement,
    this.photoPath,
  });

  factory Nomination.fromJson(Map<String, dynamic> json) => Nomination(
        id: json['id'] as int,
        electionSeatId: json['electionSeatId'] as int,
        candidateMemberId: json['candidateMemberId'] as int,
        status: json['status']?.toString() ?? 'Submitted',
        statement: json['statement'] as String? ?? '',
        photoPath: json['photoPath'] as String?,
      );
}

class AdminElection {
  final int id;
  final String title;
  final String? description;
  final String phase;
  final DateTime? announcedOn;
  final DateTime? nominationOpensOn;
  final DateTime? nominationClosesOn;
  final DateTime? pollingOpensOn;
  final DateTime? pollingClosesOn;
  final DateTime? declaredOn;
  final bool isActive;
  final int eligibleVoterCount;
  // Spec 023 FR-001. Set once the returning officer's public key is stored.
  final String? ballotKeyFingerprint;
  // Spec 023 (37.12e). ElectionPermission flag names the caller holds on this election.
  final List<String> myPermissions;
  // True once a live appointment to a persona that takes over from the admin exists.
  final bool adminHandedOver;
  // 37.13t. Fewer live officials with Approve than an emergency revocation needs.
  final bool tooFewApprovers;

  const AdminElection({
    required this.id,
    required this.title,
    this.description,
    required this.phase,
    this.announcedOn,
    this.nominationOpensOn,
    this.nominationClosesOn,
    this.pollingOpensOn,
    this.pollingClosesOn,
    this.declaredOn,
    required this.isActive,
    required this.eligibleVoterCount,
    this.ballotKeyFingerprint,
    this.myPermissions = const [],
    this.adminHandedOver = false,
    this.tooFewApprovers = false,
  });

  factory AdminElection.fromJson(Map<String, dynamic> json) => AdminElection(
        id: json['id'] as int,
        title: json['title'] as String? ?? '',
        description: json['description'] as String?,
        phase: json['phase']?.toString() ?? 'Announced',
        announcedOn: AppUtils.parseDate(json['announcedOn'] as String?),
        nominationOpensOn:
            AppUtils.parseDate(json['nominationOpensOn'] as String?),
        nominationClosesOn:
            AppUtils.parseDate(json['nominationClosesOn'] as String?),
        pollingOpensOn: AppUtils.parseDate(json['pollingOpensOn'] as String?),
        pollingClosesOn: AppUtils.parseDate(json['pollingClosesOn'] as String?),
        declaredOn: AppUtils.parseDate(json['declaredOn'] as String?),
        isActive: json['isActive'] as bool? ?? false,
        eligibleVoterCount: json['eligibleVoterCount'] as int? ?? 0,
        ballotKeyFingerprint: json['ballotKeyFingerprint'] as String?,
        myPermissions: (json['myPermissions'] as List<dynamic>?)
                ?.map((e) => e.toString())
                .toList() ??
            const [],
        adminHandedOver: json['adminHandedOver'] as bool? ?? false,
        tooFewApprovers: json['tooFewApprovers'] as bool? ?? false,
      );
}

// One row of GET /me/election-appointments. Revoked rows never come back.
class ElectionAppointment {
  final int id;
  final String electionTitle;
  final String personaName;
  final String declarationText;
  final DateTime? appointedAt;
  final DateTime? acceptedAt;
  final bool isLive;
  final bool isReturningOfficer;
  // Filled on the admin list for one election (37.13y); the "mine" list leaves them empty.
  final String displayName;
  final String? email;
  final DateTime? revokedAt;

  ElectionAppointment({
    required this.id,
    required this.electionTitle,
    required this.personaName,
    required this.declarationText,
    this.appointedAt,
    this.acceptedAt,
    this.isLive = false,
    this.isReturningOfficer = false,
    this.displayName = '',
    this.email,
    this.revokedAt,
  });

  bool get isPending => acceptedAt == null;

  factory ElectionAppointment.fromJson(Map<String, dynamic> json) =>
      ElectionAppointment(
        id: json['id'] as int,
        electionTitle: json['electionTitle'] as String? ?? '',
        personaName: json['personaName'] as String? ?? '',
        declarationText: json['declarationText'] as String? ?? '',
        appointedAt: AppUtils.parseDate(json['appointedAt'] as String?),
        acceptedAt: AppUtils.parseDate(json['acceptedAt'] as String?),
        isLive: json['isLive'] as bool? ?? false,
        isReturningOfficer: json['isReturningOfficer'] as bool? ?? false,
        displayName: json['displayName'] as String? ?? '',
        email: json['email'] as String?,
        revokedAt: AppUtils.parseDate(json['revokedAt'] as String?),
      );
}

// A persona that can be appointed, from GET /election-personas.
class ElectionPersonaOption {
  final int id;
  final String name;

  const ElectionPersonaOption({required this.id, required this.name});

  factory ElectionPersonaOption.fromJson(Map<String, dynamic> json) =>
      ElectionPersonaOption(
          id: json['id'] as int, name: json['name'] as String? ?? '');
}

// The open site-wide unlock of frozen election rules (37.13z).
class ElectionRulesUnlock {
  final int id;
  final String openedBy;
  final String reason;
  final DateTime? openedAt;
  final DateTime? expiresAt;

  const ElectionRulesUnlock({
    required this.id,
    required this.openedBy,
    required this.reason,
    this.openedAt,
    this.expiresAt,
  });

  factory ElectionRulesUnlock.fromJson(Map<String, dynamic> json) =>
      ElectionRulesUnlock(
        id: json['id'] as int,
        openedBy: json['openedBy'] as String? ?? '',
        reason: json['reason'] as String? ?? '',
        openedAt: AppUtils.parseDate(json['openedAt'] as String?),
        expiresAt: AppUtils.parseDate(json['expiresAt'] as String?),
      );

  // Whole minutes left, rounded up, so a nearly-expired unlock reads 1 rather than 0.
  int minutesLeft(DateTime now) {
    if (expiresAt == null) return 0;
    final seconds = expiresAt!.difference(now).inSeconds;
    return seconds <= 0 ? 0 : (seconds + 59) ~/ 60;
  }
}

// Same checks as the API, so the common mistakes are caught before the step-up code is asked for.
String? rulesUnlockProblem(String reason, int? minutes) {
  final length = reason.trim().length;
  if (length < ElectionConstants.unlockReasonMin) {
    return 'Give a reason of at least ${ElectionConstants.unlockReasonMin} characters.';
  }
  if (length > ElectionConstants.unlockReasonMax) {
    return 'The reason can be at most ${ElectionConstants.unlockReasonMax} characters.';
  }
  if (minutes == null || minutes < 1 || minutes > ElectionConstants.unlockMaxMinutes) {
    return 'An unlock lasts 1 to ${ElectionConstants.unlockMaxMinutes} minutes.';
  }
  return null;
}

// The appoint body, or the problem to show. A picked member wins over the typed name and email.
({Map<String, dynamic>? body, String? problem}) buildAppointRequest({
  int? personaId,
  int? memberId,
  String displayName = '',
  String email = '',
  String phone = '',
  bool isReturningOfficer = false,
}) {
  if (personaId == null) return (body: null, problem: 'Choose a role.');
  final name = displayName.trim();
  final mail = email.trim();
  if (memberId == null && (name.isEmpty || mail.isEmpty)) {
    return (body: null, problem: 'Choose a member, or give a name and email.');
  }
  return (
    body: {
      'personaId': personaId,
      'memberId': memberId,
      'displayName': memberId == null ? name : null,
      'email': memberId == null ? mail : null,
      'phone': memberId == null && phone.trim().isNotEmpty ? phone.trim() : null,
      'isReturningOfficer': isReturningOfficer,
    },
    problem: null,
  );
}

class ElectionService {
  final Dio _dio;
  ElectionService(this._dio);

  // Creates through the admin console endpoint (`api/admin/elections`) —
  // there is no bare `POST /elections` route on the backend. The server
  // derives the creating officer from the auth claim, so no identity field
  // goes in the body.
  Future<AdminElection> create({
    required String title,
    required int ecPeriodId,
    required DateTime nominationOpensOn,
    required DateTime nominationClosesOn,
    required DateTime pollingOpensOn,
    required DateTime pollingClosesOn,
    String? description,
  }) async {
    final response = await _dio.post('/admin/elections', data: {
      'title': title,
      'ecPeriodId': ecPeriodId,
      'nominationOpensOn':
          AppUtils.toWire(nominationOpensOn, includeTime: true),
      'nominationClosesOn':
          AppUtils.toWire(nominationClosesOn, includeTime: true),
      'pollingOpensOn': AppUtils.toWire(pollingOpensOn, includeTime: true),
      'pollingClosesOn': AppUtils.toWire(pollingClosesOn, includeTime: true),
      if (description != null && description.isNotEmpty)
        'description': description,
    });
    return AdminElection.fromJson(response.data as Map<String, dynamic>);
  }

  Future<List<AdminElection>> listAdmin() async {
    final response = await _dio.get('/admin/elections');
    return (response.data as List)
        .map((item) => AdminElection.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<ElectionSummary?> getCurrent() async {
    final response = await _dio.get('/elections/current');
    if (response.statusCode == 204 || response.data == null) return null;
    return ElectionSummary.fromJson(response.data as Map<String, dynamic>);
  }

  Future<ElectionSummary> get(int id) async {
    final response = await _dio.get('/elections/$id');
    return ElectionSummary.fromJson(response.data as Map<String, dynamic>);
  }

  Future<List<Nomination>> getNominations(int id) async {
    final response = await _dio.get('/elections/$id/nominations');
    return (response.data as List)
        .map((item) => Nomination.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<Nomination> nominate(int id, Map<String, dynamic> data) async {
    final response = await _dio.post('/elections/$id/nominations', data: data);
    return Nomination.fromJson(response.data as Map<String, dynamic>);
  }

  // Sends the whole ballot in one call. [choices] maps each contested seat to
  // its picks; an empty list casts that seat blank. Returns the tracking code.
  Future<String> castBallot(int id, Map<int, List<int>> choices) async {
    final response = await _dio.post('/elections/$id/vote', data: {
      'seats': [
        for (final entry in choices.entries)
          {'electionSeatId': entry.key, 'nominationIds': entry.value},
      ],
    });
    return (response.data as Map<String, dynamic>)['trackingCode'] as String;
  }

  Future<bool> withdrawNomination(int nominationId) async {
    final response =
        await _dio.post('/elections/nominations/$nominationId/withdraw');
    return response.statusCode == 200;
  }

  // Spec 023 (37.12f). True when the server stored the step for a second
  // person (202) instead of running it. Approving happens on the web admin.
  Future<bool> setPhase(int id, String phase) async =>
      (await _dio.post('/elections/$id/phase', data: phase)).statusCode ==
      202;
  Future<int> freezeVoterRoll(int id) async =>
      (await _dio.post('/elections/$id/voter-roll/freeze')).data['count']
          as int;
  // The first count needs the returning officer's key file, which only the
  // web admin can read. After that this returns the stored results. With no
  // key the server never counts or asks a second person to approve (37.13h).
  Future<List<dynamic>> count(int id) async =>
      (await _dio.post('/elections/$id/count')).data as List<dynamic>;
  // True when the declaration waits for a second person, as with setPhase.
  Future<bool> declare(int id) async =>
      (await _dio.post('/elections/$id/declare')).statusCode == 202;

  Future<List<int>> downloadOfficialDocument(int id, String formCode) async =>
      (await _dio.get<List<int>>(
        '/elections/$id/documents/$formCode',
        options: Options(responseType: ResponseType.bytes),
      ))
          .data ??
      <int>[];

  Future<List<ElectionAppointment>> getMyAppointments() async {
    final response = await _dio.get('/me/election-appointments');
    return (response.data as List)
        .map((item) =>
            ElectionAppointment.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  // Needs step-up; StepUpInterceptor asks for the code and retries.
  Future<void> acceptAppointment(int id) => _dio.post(
      '/me/election-appointments/$id/accept',
      data: {'agreeToDeclaration': true});

  Future<void> declineAppointment(int id, String? reason) => _dio.post(
      '/me/election-appointments/$id/decline',
      data: {'reason': reason});

  // 37.13y. Officials of one election, live and revoked.
  Future<List<ElectionAppointment>> getAppointments(int electionId) async {
    final response = await _dio.get('/elections/$electionId/appointments');
    return (response.data as List)
        .map((item) =>
            ElectionAppointment.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<ElectionAppointment> appoint(
      int electionId, Map<String, dynamic> body) async {
    final response =
        await _dio.post('/elections/$electionId/appointments', data: body);
    return ElectionAppointment.fromJson(response.data as Map<String, dynamic>);
  }

  Future<void> revokeAppointment(int id, String? reason) =>
      _dio.post('/elections/appointments/$id/revoke', data: {'reason': reason});

  // Always stored for a second person (202). Approving happens on the web admin.
  Future<void> emergencyRevoke(int id, String reason) => _dio.post(
      '/elections/appointments/$id/emergency-revoke',
      data: {'reason': reason});

  Future<List<ElectionPersonaOption>> getPersonas() async {
    final response = await _dio.get('/election-personas');
    return (response.data as List)
        .map((item) =>
            ElectionPersonaOption.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  // 37.13z. Every call needs step-up. A 204 means nothing is open.
  Future<ElectionRulesUnlock?> getRulesUnlock() async {
    final response = await _dio.get('/admin/elections/rules-unlock');
    if (response.statusCode == 204 || response.data is! Map) return null;
    return ElectionRulesUnlock.fromJson(response.data as Map<String, dynamic>);
  }

  Future<ElectionRulesUnlock> openRulesUnlock(String reason, int minutes) async {
    final response = await _dio.post('/admin/elections/rules-unlock',
        data: {'reason': reason, 'minutes': minutes});
    return ElectionRulesUnlock.fromJson(response.data as Map<String, dynamic>);
  }

  Future<void> closeRulesUnlock() =>
      _dio.post('/admin/elections/rules-unlock/close', data: {});

  void logFailure(String operation, Object error) =>
      debugPrint('ElectionService.$operation failed: $error');
}

final currentElectionProvider =
    FutureProvider.autoDispose<ElectionSummary?>((ref) async {
  final service = ref.read(electionServiceProvider);
  try {
    return await service.getCurrent();
  } on DioException catch (e) {
    service.logFailure('getCurrent', e);
    rethrow;
  }
});
