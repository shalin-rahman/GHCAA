import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

final notificationServiceProvider = Provider<NotificationService>((ref) => NotificationService(ref.read(dioProvider)));

class CommunicationLog {
  final int id;
  final String channel;
  final String subject;
  final String body;
  final DateTime sentDate;
  final String status;
  final String deliveryScope;

  CommunicationLog.fromJson(Map<String, dynamic> json)
      : id = json['id'] as int,
        channel = json['channel'] as String? ?? 'Email',
        subject = json['subject'] as String? ?? '',
        body = json['body'] as String? ?? '',
        sentDate = DateTime.parse(json['sentDate'] as String),
        status = json['status'] as String? ?? 'Unavailable',
        deliveryScope = json['deliveryScope'] as String? ?? 'Targeted';
}

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

  Future<List<CommunicationLog>> getMyCommunications({int page = 1, int pageSize = 25}) async {
    try {
      final response = await _dio.get('/communications/me', queryParameters: {'page': page, 'pageSize': pageSize});
      final items = (response.data['items'] as List<dynamic>? ?? const []);
      return items.map((item) => CommunicationLog.fromJson(item as Map<String, dynamic>)).toList();
    } catch (e) {
      debugPrint('NotificationService.getMyCommunications failed: $e');
      rethrow;
    }
  }
}
