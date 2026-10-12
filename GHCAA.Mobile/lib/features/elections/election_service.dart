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

// Same wording as ELECTION_APPROVAL_ACTION_LABELS on the web.
const electionApprovalActionLabels = <String, String>{
  'Publish': 'Publish the election',
  'OpenPolling': 'Open polling',
  'ReplaceBallotKey': 'Replace the ballot key',
  'ClosePolling': 'Close polling',
  'Declare': 'Declare results',
  'Archive': 'Archive',
  'Count': 'Count the ballots',
  'EmergencyRevoke': 'Remove an official in an emergency',
};

String electionPhaseLabel(String phase) => electionPhaseLabels[phase] ?? phase;
String electionApprovalActionLabel(String action) =>
    electionApprovalActionLabels[action] ?? action;
String nominationStatusLabel(String status) =>
    nominationStatusLabels[status] ?? status;

// How a tie for the last place is settled. [wire] is the API name.
enum ElectionTieRule {
  drawingLots('DrawingLots', 'Drawing lots',
      'A tie is settled by drawing lots in front of the candidates.'),
  runOff('RunOff', 'Run-off poll',
      'A tie goes to a second poll between the tied candidates.'),
  chairCastingVote('ChairCastingVote', "Returning officer's casting vote",
      "The returning officer's casting vote settles a tie.");

  final String wire;
  final String label;
  final String description;
  const ElectionTieRule(this.wire, this.label, this.description);
}

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
  // FR-047 (37.13d). A test run: hidden from members and the public, declares nobody.
  final bool isTest;

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
    this.isTest = false,
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
        isTest: json['isTest'] as bool? ?? false,
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

// One candidate on the paper from GET /elections/{id}/ballot.
class BallotCandidate {
  final int nominationId;
  final String name;
  final String? photoUrl;
  final String statement;

  const BallotCandidate({
    required this.nominationId,
    required this.name,
    this.photoUrl,
    this.statement = '',
  });

  factory BallotCandidate.fromJson(Map<String, dynamic> json) =>
      BallotCandidate(
        nominationId: json['nominationId'] as int,
        name: json['name'] as String? ?? '',
        photoUrl: json['photoUrl'] as String?,
        statement: json['statement'] as String? ?? '',
      );
}

class BallotSeat {
  final int seatId;
  final String positionTitle;
  final int places;
  final List<BallotCandidate> candidates;

  const BallotSeat({
    required this.seatId,
    required this.positionTitle,
    required this.places,
    required this.candidates,
  });

  factory BallotSeat.fromJson(Map<String, dynamic> json) => BallotSeat(
        seatId: json['seatId'] as int,
        positionTitle: json['positionTitle'] as String? ?? '',
        places: json['places'] as int? ?? 1,
        candidates: (json['candidates'] as List<dynamic>? ?? const [])
            .map((c) => BallotCandidate.fromJson(c as Map<String, dynamic>))
            .toList(),
      );
}

class Ballot {
  final int electionId;
  final String title;
  final String phase;
  final bool hasVoted;
  final String? ballotKeyFingerprint;
  final List<BallotSeat> seats;
  // Null until the voter roll is frozen. It only warns early; the server checks the roll again on cast.
  final bool? isEligible;
  final String? ineligibilityReason;

  const Ballot({
    required this.electionId,
    required this.title,
    required this.phase,
    required this.hasVoted,
    this.ballotKeyFingerprint,
    required this.seats,
    this.isEligible,
    this.ineligibilityReason,
  });

  factory Ballot.fromJson(Map<String, dynamic> json) => Ballot(
        electionId: json['electionId'] as int,
        title: json['title'] as String? ?? '',
        phase: json['phase']?.toString() ?? 'Announced',
        hasVoted: json['hasVoted'] as bool? ?? false,
        ballotKeyFingerprint: json['ballotKeyFingerprint'] as String?,
        seats: (json['seats'] as List<dynamic>? ?? const [])
            .map((s) => BallotSeat.fromJson(s as Map<String, dynamic>))
            .toList(),
        isEligible: json['isEligible'] as bool?,
        ineligibilityReason: json['ineligibilityReason'] as String?,
      );
}

// Same words as the web ballot, so both clients explain abstaining the same way.
const ballotAbstainOption = 'Abstain on this seat';
const ballotAbstainHelp =
    'Your ballot still counts, but no candidate on this seat gets your vote.';
const ballotUndecidedHint =
    'Pick a candidate or abstain on every seat to continue.';
const ballotNotOnRoll = 'You are not on the voter roll for this election.';

// A seat is decided once it has an entry: picks, or an empty list for abstain. A seat the
// member never touched has no entry, so it is never taken as an abstention.
bool allSeatsDecided(List<BallotSeat> seats, Map<int, List<int>> picks) =>
    seats.isNotEmpty && seats.every((s) => picks.containsKey(s.seatId));

// The picks after a tap on [nominationId]. A second tap removes it. With one place the pick
// replaces the old one; with more places a pick over the limit is ignored.
List<int> togglePick(List<int> current, int nominationId, int places) {
  if (current.contains(nominationId)) {
    return current.where((id) => id != nominationId).toList();
  }
  if (places <= 1) return [nominationId];
  if (current.length >= places) return current;
  return [...current, nominationId];
}

// The whole paper for the vote call. The server needs every listed seat, so an abstained seat
// goes in as an empty list. The screen only calls this once [allSeatsDecided] is true. Picks
// beyond the places are cut off.
Map<int, List<int>> buildBallotChoices(
    List<BallotSeat> seats, Map<int, List<int>> picks) {
  return {
    for (final seat in seats)
      seat.seatId: [
        for (final id in (picks[seat.seatId] ?? const <int>[]))
          if (seat.candidates.any((c) => c.nominationId == id)) id
      ].take(seat.places).toList(),
  };
}

class SeatResultCandidate {
  final int nominationId;
  final String name;
  final int voteCount;
  final bool isElected;
  final bool isTie;

  const SeatResultCandidate({
    required this.nominationId,
    required this.name,
    required this.voteCount,
    required this.isElected,
    required this.isTie,
  });

  factory SeatResultCandidate.fromJson(Map<String, dynamic> json) =>
      SeatResultCandidate(
        nominationId: json['nominationId'] as int,
        name: json['name'] as String? ?? '',
        voteCount: json['voteCount'] as int? ?? 0,
        isElected: json['isElected'] as bool? ?? false,
        isTie: json['isTie'] as bool? ?? false,
      );
}

class SeatResult {
  final int seatId;
  final String positionTitle;
  final int places;
  final int abstentions;
  final List<SeatResultCandidate> candidates;

  const SeatResult({
    required this.seatId,
    required this.positionTitle,
    required this.places,
    required this.abstentions,
    required this.candidates,
  });

  factory SeatResult.fromJson(Map<String, dynamic> json) => SeatResult(
        seatId: json['seatId'] as int,
        positionTitle: json['positionTitle'] as String? ?? '',
        places: json['places'] as int? ?? 1,
        abstentions: json['abstentions'] as int? ?? 0,
        candidates: (json['candidates'] as List<dynamic>? ?? const [])
            .map((c) => SeatResultCandidate.fromJson(c as Map<String, dynamic>))
            .toList(),
      );
}

class ElectionResults {
  final int electionId;
  final String title;
  final String phase;
  final DateTime? declaredOn;
  final int eligibleVoters;
  final int ballotsCast;
  final int spoiledBallots;
  final List<SeatResult> seats;

  const ElectionResults({
    required this.electionId,
    required this.title,
    required this.phase,
    this.declaredOn,
    required this.eligibleVoters,
    required this.ballotsCast,
    required this.spoiledBallots,
    required this.seats,
  });

  factory ElectionResults.fromJson(Map<String, dynamic> json) =>
      ElectionResults(
        electionId: json['electionId'] as int,
        title: json['title'] as String? ?? '',
        phase: json['phase']?.toString() ?? 'Declared',
        declaredOn: AppUtils.parseDate(json['declaredOn'] as String?),
        eligibleVoters: json['eligibleVoters'] as int? ?? 0,
        ballotsCast: json['ballotsCast'] as int? ?? 0,
        spoiledBallots: json['spoiledBallots'] as int? ?? 0,
        seats: (json['seats'] as List<dynamic>? ?? const [])
            .map((s) => SeatResult.fromJson(s as Map<String, dynamic>))
            .toList(),
      );
}

// Codes that contain [query], ignoring case and spaces at the ends. An empty query keeps all.
List<String> filterTrackingCodes(List<String> codes, String query) {
  final q = query.trim().toLowerCase();
  if (q.isEmpty) return codes;
  return codes.where((c) => c.toLowerCase().contains(q)).toList();
}

// Spec 023 FR-033. One accepted official on the public board. No contact details.
class BoardMember {
  final String personaName;
  final String displayName;

  const BoardMember({required this.personaName, required this.displayName});

  factory BoardMember.fromJson(Map<String, dynamic> json) => BoardMember(
        personaName: json['personaName'] as String? ?? '',
        displayName: json['displayName'] as String? ?? '',
      );
}

class BoardGroup {
  final String groupName;
  final List<BoardMember> members;

  const BoardGroup({required this.groupName, required this.members});

  factory BoardGroup.fromJson(Map<String, dynamic> json) => BoardGroup(
        groupName: json['groupName'] as String? ?? '',
        members: (json['members'] as List? ?? const [])
            .map((m) => BoardMember.fromJson(m as Map<String, dynamic>))
            .toList(),
      );
}

class ElectionBoard {
  final bool isTest;
  final List<BoardGroup> groups;
  // The server sends no turnout while polling is open.
  final int? ballotsCast;
  final int? rollSize;

  const ElectionBoard({
    required this.isTest,
    required this.groups,
    this.ballotsCast,
    this.rollSize,
  });

  factory ElectionBoard.fromJson(Map<String, dynamic> json) {
    final turnout = json['turnout'] as Map<String, dynamic>?;
    return ElectionBoard(
      isTest: json['isTest'] as bool? ?? false,
      groups: (json['groups'] as List? ?? const [])
          .map((g) => BoardGroup.fromJson(g as Map<String, dynamic>))
          .toList(),
      ballotsCast: turnout?['ballotsCast'] as int?,
      rollSize: turnout?['rollSize'] as int?,
    );
  }
}

// A two-person step waiting for a second official (37.13h).
class ElectionApproval {
  final int id;
  final int electionId;
  final String action;
  final String requestedBy;
  final DateTime requestedAt;
  final DateTime expiresAt;
  final DateTime? approvedAt;
  // True when the caller asked for it. The server flags it, so the screen
  // never offers Approve on the caller's own request.
  final bool requestedByMe;

  const ElectionApproval({
    required this.id,
    required this.electionId,
    required this.action,
    required this.requestedBy,
    required this.requestedAt,
    required this.expiresAt,
    this.approvedAt,
    this.requestedByMe = false,
  });

  factory ElectionApproval.fromJson(Map<String, dynamic> json) =>
      ElectionApproval(
        id: json['id'] as int,
        electionId: json['electionId'] as int,
        action: json['action'] as String? ?? '',
        requestedBy: json['requestedBy'] as String? ?? '',
        requestedAt: DateTime.parse(json['requestedAt'] as String),
        expiresAt: DateTime.parse(json['expiresAt'] as String),
        approvedAt: json['approvedAt'] == null
            ? null
            : DateTime.parse(json['approvedAt'] as String),
        requestedByMe: json['requestedByMe'] as bool? ?? false,
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
    bool isTest = false,
    ElectionTieRule tieRule = ElectionTieRule.drawingLots,
    DateTime? scrutinyOn,
    DateTime? withdrawalClosesOn,
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
      'isTest': isTest,
      'tieRule': tieRule.wire,
      if (scrutinyOn != null) 'scrutinyOn': AppUtils.toWire(scrutinyOn),
      if (withdrawalClosesOn != null)
        'withdrawalClosesOn': AppUtils.toWire(withdrawalClosesOn),
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

  // Anonymous route. The newest declared or archived election, null on a 404.
  // /elections/current has no election once one is declared, so results use this.
  Future<ElectionSummary?> getLatestDeclared() async {
    try {
      final response = await _dio.get('/elections/latest-declared');
      return ElectionSummary.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) return null;
      rethrow;
    }
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

  // Anonymous route. Null when the server answers 404.
  Future<ElectionBoard?> getBoard(int id) async {
    try {
      final response = await _dio.get('/elections/$id/board');
      return ElectionBoard.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) return null;
      rethrow;
    }
  }

  Future<Ballot> getBallot(int id) async {
    final response = await _dio.get('/elections/$id/ballot');
    return Ballot.fromJson(response.data as Map<String, dynamic>);
  }

  // Null while the result is not declared (the server answers 404).
  Future<ElectionResults?> getResults(int id) async {
    try {
      final response = await _dio.get('/elections/$id/results');
      return ElectionResults.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) return null;
      rethrow;
    }
  }

  // Each inner list is one ballot's choices for the seat. Null until Declared, or when the
  // organisation does not publish them (404).
  Future<Map<int, List<List<int>>>?> getOpenedBallots(int id) async {
    try {
      final response = await _dio.get('/elections/$id/opened-ballots');
      final seats = (response.data as Map<String, dynamic>)['seats'] as List<dynamic>;
      return {
        for (final s in seats.cast<Map<String, dynamic>>())
          s['electionSeatId'] as int: [
            for (final b in s['ballots'] as List<dynamic>)
              (b as List<dynamic>).cast<int>()
          ]
      };
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) return null;
      rethrow;
    }
  }

  // Null when the chain is whole, otherwise the first broken entry number.
  Future<int?> firstBrokenAuditEntry(int id) async {
    final response = await _dio.get('/elections/$id/audit/verify');
    return (response.data as Map<String, dynamic>)['firstBrokenSeq'] as int?;
  }

  // Null until the count has run (404).
  Future<List<String>?> getTrackingCodes(int id) async {
    try {
      final response = await _dio.get('/elections/$id/tracking-codes');
      return (response.data as List<dynamic>).map((c) => c.toString()).toList();
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) return null;
      rethrow;
    }
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
  // 37.12q. Throws the frozen roll away. The server refuses after nominations
  // open, inside the cutoff window, or once a nomination exists.
  Future<int> unfreezeVoterRoll(int id, String reason) async => (await _dio
          .post('/elections/$id/voter-roll/unfreeze', data: {'reason': reason}))
      .data['removed'] as int;
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

  // 37.13h. The list needs step-up; StepUpInterceptor asks for the code.
  Future<List<ElectionApproval>> getApprovals(int electionId) async {
    final response = await _dio.get('/elections/$electionId/approvals');
    return (response.data as List)
        .map((item) => ElectionApproval.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<void> approve(int approvalId) =>
      _dio.post('/elections/approvals/$approvalId/approve', data: {});

  Future<void> reject(int approvalId, String? reason) => _dio
      .post('/elections/approvals/$approvalId/reject', data: {'reason': reason});

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

final latestDeclaredElectionProvider =
    FutureProvider.autoDispose<ElectionSummary?>((ref) async {
  final service = ref.read(electionServiceProvider);
  try {
    return await service.getLatestDeclared();
  } on DioException catch (e) {
    service.logFailure('getLatestDeclared', e);
    rethrow;
  }
});
