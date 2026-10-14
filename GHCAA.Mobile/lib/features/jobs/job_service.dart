import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';
import '../../core/constants/app_constants.dart';
import '../../core/models/paged_list.dart';
import '../../core/utils/ttl_cache.dart';

final jobServiceProvider = Provider<JobService>((ref) {
  return JobService(ref.read(dioProvider));
});

class JobService {
  final Dio _dio;
  final _jobs = TtlCache<String, PagedList>(CacheConstants.listCacheTtl);
  static const _firstPageKey = 'first';

  JobService(this._dio);

  /// For callers that change jobs through dio directly, and for session changes.
  void clearCache() => _jobs.clear();

  /// Only the first page is cached; it is the one a return to the screen shows.
  Future<PagedList> getJobsPage(int page) async {
    if (page == 1) {
      final cached = _jobs.get(_firstPageKey);
      if (cached != null) return cached;
    }
    try {
      final response = await _dio.get('/jobs',
          queryParameters: {'page': page, 'pageSize': PagingConstants.listPageSize});
      final jobs = PagedList.fromJson(response.data as Map<String, dynamic>);
      if (page == 1) _jobs.set(_firstPageKey, jobs);
      return jobs;
    } catch (e) {
      debugPrint('JobService.getJobsPage failed: $e');
      rethrow;
    }
  }

  Future<bool> postJob(Map<String, dynamic> data) async {
    try {
      final response = await _dio.post('/jobs', data: data);
      _jobs.clear();
      return response.statusCode == 200 || response.statusCode == 201;
    } catch (e) {
      debugPrint('JobService.postJob failed: $e');
      return false;
    }
  }

  Future<bool> deleteJob(int id) async {
    try {
      final response = await _dio.delete('/jobs/$id');
      _jobs.clear();
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
      _jobs.clear();
      return response.statusCode == 200;
    } catch (e) {
      debugPrint('JobService.resolveJobApproval failed: $e');
      return false;
    }
  }
}
