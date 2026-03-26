import 'package:dio/dio.dart';
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
      return [];
    }
  }

  Future<bool> postJob(Map<String, dynamic> data) async {
    try {
      final response = await _dio.post('/jobs', data: data);
      return response.statusCode == 200 || response.statusCode == 201;
    } catch (e) {
      return false;
    }
  }
}
