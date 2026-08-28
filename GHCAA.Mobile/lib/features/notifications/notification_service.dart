import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

final notificationServiceProvider = Provider<NotificationService>((ref) => NotificationService(ref.read(dioProvider)));

class NotificationService {
  final Dio _dio;
  NotificationService(this._dio);

  Future<List<dynamic>> getMyNotifications() async {
    try {
      final response = await _dio.get('/notifications');
      return response.data as List<dynamic>;
    } catch (e) {
      debugPrint('NotificationService.getMyNotifications failed: $e');
      rethrow;
    }
  }

  Future<bool> markAsRead(int id) async {
    try {
      final response = await _dio.post('/notifications/$id/read');
      return response.statusCode == 200;
    } catch (e) {
      debugPrint('NotificationService.markAsRead failed: $e');
      return false;
    }
  }
}
