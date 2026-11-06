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
  final bool allowComments;
  final bool commentsLocked;
  final int commentCount;
  final String category;
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
    this.allowComments = true,
    this.commentsLocked = false,
    this.commentCount = 0,
    this.category = 'Other',
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
      allowComments: json['allowComments'] ?? true,
      commentsLocked: json['commentsLocked'] ?? false,
      commentCount: json['commentCount'] ?? 0,
      category: json['category'] ?? 'Other',
    );
  }
}

class PollComment {
  final int id;
  final int? parentId;
  final int authorId;
  final String authorName;
  final String content;
  final DateTime postedAt;
  final bool removed;
  final List<PollComment> replies;

  const PollComment({
    required this.id,
    this.parentId,
    required this.authorId,
    required this.authorName,
    required this.content,
    required this.postedAt,
    this.removed = false,
    this.replies = const [],
  });

  factory PollComment.fromJson(Map<String, dynamic> json) => PollComment(
        id: json['id'],
        parentId: json['parentId'],
        authorId: json['authorId'] ?? 0,
        authorName: json['authorName'] ?? '',
        content: json['content'] ?? '',
        postedAt: DateTime.parse(json['postedAt']),
        removed: json['removed'] ?? false,
        replies: (json['replies'] as List?)?.map((r) => PollComment.fromJson(r)).toList() ?? const [],
      );

  PollComment copyWith({String? content, bool? removed, List<PollComment>? replies}) => PollComment(
        id: id,
        parentId: parentId,
        authorId: authorId,
        authorName: authorName,
        content: content ?? this.content,
        postedAt: postedAt,
        removed: removed ?? this.removed,
        replies: replies ?? this.replies,
      );
}

class PollCommentPage {
  final List<PollComment> comments;
  // Top-level comments in the whole thread, used for paging.
  final int totalCount;
  final String? closedReason;

  const PollCommentPage({required this.comments, required this.totalCount, this.closedReason});

  factory PollCommentPage.fromJson(Map<String, dynamic> json) => PollCommentPage(
        comments: (json['comments'] as List?)?.map((c) => PollComment.fromJson(c)).toList() ?? const [],
        totalCount: json['totalCount'] ?? 0,
        closedReason: json['closedReason'],
      );
}

class PollService {
  final Dio _dio;

  PollService(this._dio);

  static const int commentPageSize = 20;
  // Same limit as Constants.Discussion.MaxCommentLength on the API.
  static const int maxCommentLength = 2000;

  Future<List<Poll>> getActivePolls({String? category}) async {
    try {
      final response = await _dio.get('/polls/active',
          queryParameters: category == null ? null : {'category': category});
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
    } catch (e) {
      debugPrint('PollService.vote failed: $e');
      return false;
    }
  }

  // Comment calls let DioException through so the screen can show the server's reason,
  // such as a locked thread.
  Future<PollCommentPage> getComments(int pollId, int page) async {
    final response = await _dio.get('/polls/$pollId/comments',
        queryParameters: {'page': page, 'pageSize': commentPageSize});
    return PollCommentPage.fromJson(response.data);
  }

  Future<PollComment> postComment(int pollId, String content, {int? parentId}) async {
    final response = await _dio.post('/polls/$pollId/comments', data: {'content': content, 'parentId': parentId});
    return PollComment.fromJson(response.data);
  }

  Future<void> removeComment(int pollId, int commentId) async {
    await _dio.delete('/polls/$pollId/comments/$commentId', data: {'reason': null});
  }
}

// Null means every category.
final pollCategoryFilterProvider = StateProvider.autoDispose<String?>((ref) => null);

final activePollsProvider = FutureProvider.autoDispose<List<Poll>>((ref) async {
  final category = ref.watch(pollCategoryFilterProvider);
  return ref.read(pollServiceProvider).getActivePolls(category: category);
});
