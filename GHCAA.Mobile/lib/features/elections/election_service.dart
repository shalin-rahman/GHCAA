import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';
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

  const ElectionSummary({
    required this.id,
    required this.title,
    required this.phase,
    required this.ecPeriodId,
    required this.voterCount,
    required this.eligibleVoterCount,
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
        pollingClosesOn:
            AppUtils.parseDate(json['pollingClosesOn'] as String?),
        declaredOn: AppUtils.parseDate(json['declaredOn'] as String?),
        isActive: json['isActive'] as bool? ?? false,
        eligibleVoterCount: json['eligibleVoterCount'] as int? ?? 0,
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

  Future<bool> vote(int id,
      {required int electionSeatId,
      required int nominationId,
      String? serialNumber}) async {
    final response = await _dio.post('/elections/$id/vote', data: {
      'electionSeatId': electionSeatId,
      'nominationId': nominationId,
      if (serialNumber != null) 'serialNumber': serialNumber,
    });
    return response.statusCode == 200;
  }

  Future<bool> withdrawNomination(int nominationId) async {
    final response =
        await _dio.post('/elections/nominations/$nominationId/withdraw');
    return response.statusCode == 200;
  }

  Future<void> setPhase(int id, String phase) =>
      _dio.post('/elections/$id/phase', data: phase);
  Future<int> freezeVoterRoll(int id) async =>
      (await _dio.post('/elections/$id/voter-roll/freeze')).data['count']
          as int;
  Future<List<dynamic>> count(int id) async =>
      (await _dio.post('/elections/$id/count')).data as List<dynamic>;
  Future<bool> declare(int id) async =>
      (await _dio.post('/elections/$id/declare')).statusCode == 200;

  Future<List<int>> downloadOfficialDocument(int id, String formCode) async =>
      (await _dio.get<List<int>>(
        '/elections/$id/documents/$formCode',
        options: Options(responseType: ResponseType.bytes),
      )).data ?? <int>[];

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
