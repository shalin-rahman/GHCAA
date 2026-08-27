import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

class ForumCategory {
  final int id;
  final String name;
  final String? description;
  final int sortOrder;
  final int topicCount;
  final int postCount;

  ForumCategory({
    required this.id,
    required this.name,
    this.description,
    required this.sortOrder,
    required this.topicCount,
    required this.postCount,
  });

  factory ForumCategory.fromJson(Map<String, dynamic> json) {
    return ForumCategory(
      id: json['id'] ?? 0,
      name: json['name'] ?? '',
      description: json['description'],
      sortOrder: json['sortOrder'] ?? 0,
      topicCount: json['topicCount'] ?? 0,
      postCount: json['postCount'] ?? 0,
    );
  }
}

class ForumTopic {
  final int id;
  final int categoryId;
  final String title;
  final String content;
  final int authorId;
  final String authorName;
  final String? authorPhotoUrl;
  final DateTime createdAt;
  final DateTime? lastUpdatedAt;
  final int viewCount;
  final bool isPinned;
  final bool isLocked;
  final int replyCount;

  ForumTopic({
    required this.id,
    required this.categoryId,
    required this.title,
    required this.content,
    required this.authorId,
    required this.authorName,
    this.authorPhotoUrl,
    required this.createdAt,
    this.lastUpdatedAt,
    required this.viewCount,
    required this.isPinned,
    required this.isLocked,
    required this.replyCount,
  });

  factory ForumTopic.fromJson(Map<String, dynamic> json) {
    return ForumTopic(
      id: json['id'] ?? 0,
      categoryId: json['categoryId'] ?? 0,
      title: json['title'] ?? '',
      content: json['content'] ?? '',
      authorId: json['authorId'] ?? 0,
      authorName: json['authorName'] ?? '',
      authorPhotoUrl: json['authorPhotoUrl'],
      createdAt: json['createdAt'] != null ? DateTime.parse(json['createdAt']) : DateTime.now(),
      lastUpdatedAt: json['lastUpdatedAt'] != null ? DateTime.parse(json['lastUpdatedAt']) : null,
      viewCount: json['viewCount'] ?? 0,
      isPinned: json['isPinned'] ?? false,
      isLocked: json['isLocked'] ?? false,
      replyCount: json['replyCount'] ?? 0,
    );
  }
}

class ForumPost {
  final int id;
  final int topicId;
  final String content;
  final int authorId;
  final String authorName;
  final String? authorPhotoUrl;
  final DateTime createdAt;
  final DateTime? updatedAt;
  final int? parentPostId;

  ForumPost({
    required this.id,
    required this.topicId,
    required this.content,
    required this.authorId,
    required this.authorName,
    this.authorPhotoUrl,
    required this.createdAt,
    this.updatedAt,
    this.parentPostId,
  });

  factory ForumPost.fromJson(Map<String, dynamic> json) {
    return ForumPost(
      id: json['id'] ?? 0,
      topicId: json['topicId'] ?? 0,
      content: json['content'] ?? '',
      authorId: json['authorId'] ?? 0,
      authorName: json['authorName'] ?? '',
      authorPhotoUrl: json['authorPhotoUrl'],
      createdAt: json['createdAt'] != null ? DateTime.parse(json['createdAt']) : DateTime.now(),
      updatedAt: json['updatedAt'] != null ? DateTime.parse(json['updatedAt']) : null,
      parentPostId: json['parentPostId'],
    );
  }
}

class ForumService {
  final Dio _dio;

  ForumService(this._dio);

  Future<List<ForumCategory>> getCategories() async {
    try {
      final response = await _dio.get('/forum/categories');
      if (response.statusCode == 200) {
        return (response.data as List).map((c) => ForumCategory.fromJson(c)).toList();
      }
    } catch (e) {
      debugPrint('ForumService.getCategories failed: $e');
      rethrow;
    }
    return [];
  }

  Future<List<ForumTopic>> getTopics(int categoryId, {int page = 1, int pageSize = 20}) async {
    try {
      final response = await _dio.get(
        '/forum/categories/$categoryId/topics',
        queryParameters: {'page': page, 'pageSize': pageSize},
      );
      if (response.statusCode == 200) {
        return (response.data as List).map((t) => ForumTopic.fromJson(t)).toList();
      }
    } catch (e) {
      debugPrint('ForumService.getTopics failed: $e');
      rethrow;
    }
    return [];
  }

  Future<ForumTopic?> getTopic(int topicId) async {
    try {
      final response = await _dio.get('/forum/topics/$topicId');
      if (response.statusCode == 200) {
        return ForumTopic.fromJson(response.data);
      }
    } catch (e) {
      debugPrint('ForumService.getTopic failed: $e');
    }
    return null;
  }

  Future<List<ForumPost>> getPosts(int topicId, {int page = 1, int pageSize = 100}) async {
    try {
      final response = await _dio.get(
        '/forum/topics/$topicId/posts',
        queryParameters: {'page': page, 'pageSize': pageSize},
      );
      if (response.statusCode == 200) {
        return (response.data as List).map((p) => ForumPost.fromJson(p)).toList();
      }
    } catch (e) {
      debugPrint('ForumService.getPosts failed: $e');
      rethrow;
    }
    return [];
  }

  Future<ForumTopic?> createTopic(int categoryId, String title, String content) async {
    try {
      final response = await _dio.post(
        '/forum/topics',
        data: {
          'categoryId': categoryId,
          'title': title,
          'content': content,
        },
      );
      if (response.statusCode == 201 || response.statusCode == 200) {
        return ForumTopic.fromJson(response.data);
      }
    } catch (e) {
      debugPrint('ForumService.createTopic failed: $e');
    }
    return null;
  }

  Future<ForumPost?> createPost(int topicId, String content, {int? parentPostId}) async {
    try {
      final response = await _dio.post(
        '/forum/topics/$topicId/posts',
        data: {
          'topicId': topicId,
          'content': content,
          'parentPostId': parentPostId,
        },
      );
      if (response.statusCode == 200 || response.statusCode == 201) {
        return ForumPost.fromJson(response.data);
      }
    } catch (e) {
      debugPrint('ForumService.createPost failed: $e');
    }
    return null;
  }

  Future<bool> deleteTopic(int topicId) async {
    try {
      final response = await _dio.delete('/forum/topics/$topicId');
      return response.statusCode == 204 || response.statusCode == 200;
    } catch (e) {
      debugPrint('ForumService.deleteTopic failed: $e');
      return false;
    }
  }

  Future<bool> deletePost(int postId) async {
    try {
      final response = await _dio.delete('/forum/posts/$postId');
      return response.statusCode == 204 || response.statusCode == 200;
    } catch (e) {
      debugPrint('ForumService.deletePost failed: $e');
      return false;
    }
  }
}

final forumServiceProvider = Provider<ForumService>((ref) {
  return ForumService(ref.read(dioProvider));
});

final forumCategoriesProvider = FutureProvider.autoDispose<List<ForumCategory>>((ref) async {
  return ref.read(forumServiceProvider).getCategories();
});

final forumTopicsProvider = FutureProvider.family.autoDispose<List<ForumTopic>, int>((ref, categoryId) async {
  return ref.read(forumServiceProvider).getTopics(categoryId);
});

final topicDetailProvider = FutureProvider.family.autoDispose<ForumTopic?, int>((ref, topicId) async {
  return ref.read(forumServiceProvider).getTopic(topicId);
});

final topicPostsProvider = FutureProvider.family.autoDispose<List<ForumPost>, int>((ref, topicId) async {
  return ref.read(forumServiceProvider).getPosts(topicId);
});
