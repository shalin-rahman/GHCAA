import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

final notificationServiceProvider = Provider<NotificationService>((ref) => NotificationService(ref.read(dioProvider)));

class NotificationService {
  final Dio _dio;
  NotificationService(this._dio);

  Future<List<dynamic>> getMyNotifications() async {
    try {
      final response = await _dio.get('/Notification');
      return response.data as List<dynamic>;
    } catch (e) {
      return [];
    }
  }

  Future<bool> markAsRead(int id) async {
    try {
      final response = await _dio.post('/Notification/$id/read');
      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }
}
