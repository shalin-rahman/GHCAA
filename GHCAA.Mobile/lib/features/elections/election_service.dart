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

class ElectionService {
  final Dio _dio;
  ElectionService(this._dio);

  Future<ElectionSummary> create({
    required String title,
    required int ecPeriodId,
    required DateTime nominationOpensOn,
    required DateTime nominationClosesOn,
    required DateTime pollingOpensOn,
    required DateTime pollingClosesOn,
    required int createdBy,
  }) async {
    final response = await _dio.post('/elections', data: {
      'title': title,
      'ecPeriodId': ecPeriodId,
      'nominationOpensOn':
          AppUtils.toWire(nominationOpensOn, includeTime: true),
      'nominationClosesOn':
          AppUtils.toWire(nominationClosesOn, includeTime: true),
      'pollingOpensOn': AppUtils.toWire(pollingOpensOn, includeTime: true),
      'pollingClosesOn': AppUtils.toWire(pollingClosesOn, includeTime: true),
      'createdBy': createdBy,
    });
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
  try {
    final service = ref.read(electionServiceProvider);
    final response = await service._dio.get('/elections/current');
    return response.data == null
        ? null
        : ElectionSummary.fromJson(response.data as Map<String, dynamic>);
  } catch (_) {
    return null;
  }
});
