import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/api/api_client.dart';
import 'scholarship_models.dart';

export 'scholarship_models.dart';

final scholarshipServiceProvider = Provider<ScholarshipService>(
  (ref) => ScholarshipService(ref.read(dioProvider)),
);

class ScholarshipService {
  final Dio _dio;

  ScholarshipService(this._dio);

  Future<List<ScholarshipFund>> getPublicFunds() async {
    final response = await _dio.get('/scholarships/public/funds');
    return (response.data as List)
        .map((item) => ScholarshipFund.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<List<ScholarshipCall>> getPublicCalls() async {
    final response = await _dio.get('/scholarships/public/calls');
    return (response.data as List)
        .map((item) => ScholarshipCall.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<ScholarshipApplication> submitApplication(
    int callId,
    CreateScholarshipApplication application,
  ) async {
    final response = await _dio.post(
      '/scholarships/calls/$callId/applications',
      data: application.toJson(),
    );
    return ScholarshipApplication.fromJson(
      response.data as Map<String, dynamic>,
    );
  }

  Future<ScholarshipStatus> getStatus(
    String referenceCode,
    String email,
  ) async {
    final response = await _dio.get(
      '/scholarships/status/${Uri.encodeComponent(referenceCode)}',
      queryParameters: {'email': email},
    );
    return ScholarshipStatus.fromJson(response.data as Map<String, dynamic>);
  }
}
