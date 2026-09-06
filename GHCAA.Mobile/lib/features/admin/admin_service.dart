import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
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
        'statusFilter': 'Applied',
        'page': 1,
        'pageSize': 100,
      });
      return response.data['items'] ?? [];
    } catch (e) {
      debugPrint('AdminService.getPendingApprovals failed: $e');
      rethrow;
    }
  }

  Future<List<dynamic>> getContactMessages() async {
    try {
      final response = await _dio.get('/admin/contact-messages');
      return response.data as List<dynamic>;
    } catch (e) {
      debugPrint('AdminService.getContactMessages failed: $e');
      rethrow;
    }
  }

  Future<bool> markMessageAsRead(int messageId) async {
    try {
      final response = await _dio.post('/admin/contact-messages/$messageId/read');
      return response.statusCode == 200;
    } catch (e) {
      debugPrint('AdminService.markMessageAsRead failed: $e');
      return false;
    }
  }

  Future<bool> resolveApproval(int memberId, bool approve, {required int adminId, String? reason}) async {
    try {
      final endpoint = approve ? 'approve' : 'reject';
      // 29F.1: The acting admin is resolved from the JWT server-side; the client no longer sends an
      // admin id (the previous approvedByAdminId/rejectedByAdminId fields were ignored by the API).
      final response = await _dio.post('/admin/members/$memberId/$endpoint', data: {
        'reason': reason ?? (approve ? 'Approved' : 'Rejected'),
      });
      return response.statusCode == 200;
    } catch (e) {
      debugPrint('AdminService.resolveApproval failed: $e');
      return false;
    }
  }

  Future<Map<String, dynamic>> getGlobalAnalytics() async {
    try {
      final response = await _dio.get('/admin/analytics');
      return response.data as Map<String, dynamic>;
    } catch (e) {
      debugPrint('AdminService.getGlobalAnalytics failed: $e');
      return {'totalMembers': 0, 'pendingApprovals': 0, 'totalEvents': 0};
    }
  }

  // Governance logic
  Future<List<dynamic>> getECPeriods() async {
    try {
      final response = await _dio.get('/admin/governance/periods');
      return response.data as List<dynamic>;
    } catch (e) {
      debugPrint('AdminService.getECPeriods failed: $e');
      rethrow;
    }
  }

  Future<bool> createECPeriod(Map<String, dynamic> data) async {
    try {
      final response = await _dio.post('/admin/governance/periods', data: data);
      return response.statusCode == 201 || response.statusCode == 200;
    } catch (e) {
      debugPrint('AdminService.createECPeriod failed: $e');
      return false;
    }
  }

  Future<bool> updateECPeriod(int id, Map<String, dynamic> data) async {
    try {
      final response = await _dio.put('/admin/governance/periods/$id', data: data);
      return response.statusCode == 200;
    } catch (e) {
      debugPrint('AdminService.updateECPeriod failed: $e');
      return false;
    }
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
    } catch (e) {
      debugPrint('AdminService.getLedgerRecords failed: $e');
      rethrow;
    }
  }

  Future<Map<String, dynamic>> getLedgerSummary(int year) async {
    try {
      final response = await _dio.get('/ledger/summary', queryParameters: {'year': year});
      return response.data as Map<String, dynamic>;
    } catch (e) {
      debugPrint('AdminService.getLedgerSummary failed: $e');
      return {'totalRevenue': 0, 'totalExpenses': 0, 'netPosition': 0};
    }
  }

  Future<bool> updateMember(int id, Map<String, dynamic> data) async {
    try {
      final response = await _dio.put('/admin/members/$id', data: data);
      return response.statusCode == 200;
    } catch (e) {
      debugPrint('AdminService.updateMember failed: $e');
      return false;
    }
  }

  // Financial Fee Configuration
  Future<List<dynamic>> getFeeConfigs() async {
    try {
      final response = await _dio.get('/financials/fees/config');
      return response.data as List<dynamic>;
    } catch (e) {
      debugPrint('AdminService.getFeeConfigs failed: $e');
      rethrow;
    }
  }

  Future<bool> addFeeConfig(Map<String, dynamic> data) async {
    try {
      final response = await _dio.post('/financials/fees/config', data: data);
      return response.statusCode == 200 || response.statusCode == 201;
    } catch (e) {
      debugPrint('AdminService.addFeeConfig failed: $e');
      return false;
    }
  }

  Future<bool> updateFeeConfig(Map<String, dynamic> data) async {
    try {
      final response = await _dio.put('/financials/fees/config', data: data);
      return response.statusCode == 200;
    } catch (e) {
      debugPrint('AdminService.updateFeeConfig failed: $e');
      return false;
    }
  }

  // Committee Member Management
  Future<List<dynamic>> getCommitteeMembers(int periodId) async {
    try {
      final response = await _dio.get('/admin/governance/periods/$periodId/members');
      return response.data as List<dynamic>;
    } catch (e) {
      debugPrint('AdminService.getCommitteeMembers failed: $e');
      rethrow;
    }
  }

  Future<bool> assignMemberToCommittee(int periodId, Map<String, dynamic> data) async {
    try {
      final response = await _dio.post('/admin/governance/periods/$periodId/members', data: data);
      return response.statusCode == 200;
    } catch (e) {
      debugPrint('AdminService.assignMemberToCommittee failed: $e');
      return false;
    }
  }

  Future<bool> removeMemberFromCommittee(int ecMemberId, {bool notifyMember = false}) async {
    try {
      final response = await _dio.delete('/admin/governance/members/$ecMemberId', queryParameters: {'notifyMember': notifyMember});
      return response.statusCode == 200;
    } catch (e) {
      debugPrint('AdminService.removeMemberFromCommittee failed: $e');
      return false;
    }
  }
}
