import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

final adminServiceProvider = Provider<AdminService>((ref) {
  return AdminService(ref.read(dioProvider));
});

class AdminService {
  final Dio _dio;
  AdminService(this._dio);

  Future<List<dynamic>> getPendingApprovals() async {
    try {
      final response = await _dio.get('/admin/members/pending');
      return response.data as List<dynamic>;
    } catch (e) {
      return [];
    }
  }

  Future<bool> resolveApproval(int memberId, bool approve, {String? reason}) async {
    try {
      final response = await _dio.post('/admin/members/resolve', data: {
        'memberId': memberId,
        'isApproved': approve,
        'rejectionReason': reason,
      });
      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }

  Future<Map<String, dynamic>> getGlobalAnalytics() async {
    try {
      final response = await _dio.get('/admin/analytics');
      return response.data as Map<String, dynamic>;
    } catch (e) {
      return {'totalMembers': 0, 'pendingApprovals': 0, 'totalEvents': 0};
    }
  }
}
