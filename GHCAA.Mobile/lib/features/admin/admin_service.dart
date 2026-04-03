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
      final response = await _dio.get('/admin/members', queryParameters: {
        'statusFilter': 'pending',
        'page': 1,
        'pageSize': 100,
      });
      return response.data['items'] ?? [];
    } catch (e) {
      return [];
    }
  }

  Future<List<dynamic>> getContactMessages() async {
    try {
      final response = await _dio.get('/admin/contact-messages');
      return response.data as List<dynamic>;
    } catch (e) {
      return [];
    }
  }

  Future<bool> markMessageAsRead(int messageId) async {
    try {
      final response = await _dio.post('/admin/contact-messages/$messageId/read');
      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }

  Future<bool> resolveApproval(int memberId, bool approve, {String? reason}) async {
    try {
      final endpoint = approve ? 'approve' : 'reject';
      final response = await _dio.post('/admin/members/$memberId/$endpoint', data: {
        'approvedByAdminId': 1, // Placeholder: map to current session
        'rejectedByAdminId': 1,
        'reason': reason ?? (approve ? 'Approved' : 'Rejected'),
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

  // Governance logic
  Future<List<dynamic>> getECPeriods() async {
    try {
      final response = await _dio.get('/admin/governance/periods');
      return response.data as List<dynamic>;
    } catch (_) { return []; }
  }

  Future<bool> createECPeriod(Map<String, dynamic> data) async {
    try {
      final response = await _dio.post('/admin/governance/periods', data: data);
      return response.statusCode == 201 || response.statusCode == 200;
    } catch (_) { return false; }
  }

  Future<bool> updateECPeriod(int id, Map<String, dynamic> data) async {
    try {
      final response = await _dio.put('/admin/governance/periods/$id', data: data);
      return response.statusCode == 200;
    } catch (_) { return false; }
  }

  // Financial Ledger
  Future<List<dynamic>> getLedgerRecords({String? search, int page = 1}) async {
    try {
      final response = await _dio.get('/ledger', queryParameters: {
        'search': search,
        'page': page,
        'pageSize': 200, // Batch fetch for admin view
      });
      return response.data['items'] ?? [];
    } catch (_) { return []; }
  }

  Future<Map<String, dynamic>> getLedgerSummary(int year) async {
    try {
      final response = await _dio.get('/ledger/summary', queryParameters: {'year': year});
      return response.data as Map<String, dynamic>;
    } catch (_) { return {'totalRevenue': 0, 'totalExpenses': 0, 'netPosition': 0}; }
  }

  Future<bool> updateMember(int id, Map<String, dynamic> data) async {
    try {
      final response = await _dio.put('/admin/members/$id', data: data);
      return response.statusCode == 200;
    } catch (_) { return false; }
  }
}
