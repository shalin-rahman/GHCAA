import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

final activityServiceProvider = Provider<ActivityService>((ref) => ActivityService(ref.read(dioProvider)));

class ActivityService {
  final Dio _dio;
  ActivityService(this._dio);

  /// Fetches activity logs for the current authenticated user.
  Future<List<dynamic>> getMyActivity() async {
    try {
      final response = await _dio.get('/activity/me');
      return response.data as List<dynamic>;
    } catch (e) {
      debugPrint('ActivityService.getMyActivity failed: $e');
      rethrow;
    }
  }

  /// Fetches global activity logs (Admin only).
  Future<List<dynamic>> getGlobalActivity() async {
    try {
      final response = await _dio.get('/activity/admin/global');
      return response.data as List<dynamic>;
    } catch (e) {
      debugPrint('ActivityService.getGlobalActivity failed: $e');
      rethrow;
    }
  }

  /// Fetches activity logs for a specific member (Admin only).
  Future<List<dynamic>> getMemberActivity(int memberId) async {
    try {
      final response = await _dio.get('/activity/admin/$memberId');
      return response.data as List<dynamic>;
    } catch (e) {
      debugPrint('ActivityService.getMemberActivity failed: $e');
      rethrow;
    }
  }
}
