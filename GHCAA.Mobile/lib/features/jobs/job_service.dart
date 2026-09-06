import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

final jobServiceProvider = Provider<JobService>((ref) {
  return JobService(ref.read(dioProvider));
});

class JobService {
  final Dio _dio;
  JobService(this._dio);

  Future<List<dynamic>> getAllJobs() async {
    try {
      final response = await _dio.get('/jobs');
      return response.data as List<dynamic>;
    } catch (e) {
      debugPrint('JobService.getAllJobs failed: $e');
      rethrow;
    }
  }

  Future<bool> postJob(Map<String, dynamic> data) async {
    try {
      final response = await _dio.post('/jobs', data: data);
      return response.statusCode == 200 || response.statusCode == 201;
    } catch (e) {
      debugPrint('JobService.postJob failed: $e');
      return false;
    }
  }

  Future<bool> deleteJob(int id) async {
    try {
      final response = await _dio.delete('/jobs/$id');
      return response.statusCode == 200;
    } catch (e) {
      debugPrint('JobService.deleteJob failed: $e');
      return false;
    }
  }

  // ---- Admin approval workflow ----

  Future<List<dynamic>> getPendingJobs() async {
    try {
      final response = await _dio.get('/jobs/admin/pending');
      return response.data as List<dynamic>;
    } catch (e) {
      debugPrint('JobService.getPendingJobs failed: $e');
      rethrow;
    }
  }

  Future<bool> resolveJobApproval(int id, bool approve, {String? reason, bool notifyMember = true}) async {
    try {
      final endpoint = approve ? 'approve' : 'reject';
      final response = await _dio.post(
        '/jobs/admin/$id/$endpoint',
        queryParameters: approve ? {'notifyMember': notifyMember} : null,
        data: approve ? null : {'reason': reason, 'notifyMember': notifyMember},
      );
      return response.statusCode == 200;
    } catch (e) {
      debugPrint('JobService.resolveJobApproval failed: $e');
      return false;
    }
  }
}
