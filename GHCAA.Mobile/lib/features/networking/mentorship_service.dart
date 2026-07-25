import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

final mentorshipServiceProvider = Provider<MentorshipService>((ref) {
  return MentorshipService(ref.read(dioProvider));
});

class MentorshipService {
  final Dio _dio;
  MentorshipService(this._dio);

  Future<bool> sendRequest(int mentorId, String? message, String? domain) async {
    try {
      final r = await _dio.post('/mentorship', data: {
        'mentorId': mentorId,
        'message': message,
        'domain': domain,
      });
      return r.statusCode == 200;
    } catch (_) {
      return false;
    }
  }

  Future<List<dynamic>> getSentRequests() async {
    try {
      final r = await _dio.get('/mentorship/sent');
      return r.data as List<dynamic>;
    } catch (e) {
      debugPrint('MentorshipService.getSentRequests failed: $e');
      rethrow;
    }
  }

  Future<List<dynamic>> getReceivedRequests() async {
    try {
      final r = await _dio.get('/mentorship/received');
      return r.data as List<dynamic>;
    } catch (e) {
      debugPrint('MentorshipService.getReceivedRequests failed: $e');
      rethrow;
    }
  }

  Future<bool> respondToRequest(int requestId, bool accept, String? note) async {
    try {
      final r = await _dio.post('/mentorship/$requestId/respond', data: {
        'accept': accept,
        'note': note,
      });
      return r.statusCode == 200;
    } catch (_) {
      return false;
    }
  }

  Future<bool> markComplete(int requestId) async {
    try {
      final r = await _dio.post('/mentorship/$requestId/complete');
      return r.statusCode == 200;
    } catch (_) {
      return false;
    }
  }
}
