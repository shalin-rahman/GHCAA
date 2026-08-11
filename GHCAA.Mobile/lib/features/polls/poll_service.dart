import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

final pollServiceProvider = Provider<PollService>((ref) {
  return PollService(ref.read(dioProvider));
});

class PollOption {
  final int id;
  final String text;
  final int voteCount;
  final double percentage;

  PollOption({
    required this.id,
    required this.text,
    required this.voteCount,
    required this.percentage,
  });

  factory PollOption.fromJson(Map<String, dynamic> json) {
    return PollOption(
      id: json['id'],
      text: json['text'] ?? '',
      voteCount: json['voteCount'] ?? 0,
      percentage: (json['percentage'] ?? 0.0).toDouble(),
    );
  }
}

class Poll {
  final int id;
  final String title;
  final String? description;
  final bool allowMultipleChoice;
  final bool isActive;
  final DateTime createdAt;
  final DateTime? expiryDate;
  final List<PollOption> options;
  final int totalVotes;
  final bool hasVoted;
  List<int> selectedOptionIds; // Mutable for UI state

  Poll({
    required this.id,
    required this.title,
    this.description,
    required this.allowMultipleChoice,
    required this.isActive,
    required this.createdAt,
    this.expiryDate,
    required this.options,
    required this.totalVotes,
    required this.hasVoted,
    required this.selectedOptionIds,
  });

  factory Poll.fromJson(Map<String, dynamic> json) {
    return Poll(
      id: json['id'],
      title: json['title'] ?? '',
      description: json['description'],
      allowMultipleChoice: json['allowMultipleChoice'] ?? false,
      isActive: json['isActive'] ?? false,
      createdAt: DateTime.parse(json['createdAt']),
      expiryDate: json['expiryDate'] != null ? DateTime.parse(json['expiryDate']) : null,
      options: (json['options'] as List?)?.map((o) => PollOption.fromJson(o)).toList() ?? [],
      totalVotes: json['totalVotes'] ?? 0,
      hasVoted: json['hasVoted'] ?? false,
      selectedOptionIds: List<int>.from(json['selectedOptionIds'] ?? []),
    );
  }
}

class PollService {
  final Dio _dio;

  PollService(this._dio);

  Future<List<Poll>> getActivePolls() async {
    try {
      final response = await _dio.get('/polls/active');
      if (response.statusCode == 200) {
        return (response.data as List).map((p) => Poll.fromJson(p)).toList();
      }
    } catch (e) {
      debugPrint('PollService.getActivePolls failed: $e');
      rethrow;
    }
    return [];
  }

  Future<bool> vote(int pollId, List<int> optionIds) async {
    try {
      final response = await _dio.post('/polls/$pollId/vote', data: {'optionIds': optionIds});
      return response.statusCode == 200;
    } catch (_) {
      return false;
    }
  }
}

final activePollsProvider = FutureProvider.autoDispose<List<Poll>>((ref) async {
  return ref.read(pollServiceProvider).getActivePolls();
});
