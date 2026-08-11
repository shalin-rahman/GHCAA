import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

final networkingServiceProvider = Provider<NetworkingService>((ref) {
  return NetworkingService(ref.read(dioProvider));
});

class NetworkingService {
  final Dio _dio;
  NetworkingService(this._dio);

  Future<Map<String, dynamic>> searchAlumni({
    String? query, 
    String? batch, 
    String? department, 
    String? membershipType,
    String? category,
    int pageNumber = 1, 
    int pageSize = 20
  }) async {
    try {
      final params = {
        if (query?.isNotEmpty ?? false) 'query': query,
        if (batch?.isNotEmpty ?? false) 'passingYear': batch,
        if (department?.isNotEmpty ?? false) 'subject': department,
        if (membershipType?.isNotEmpty ?? false) 'membershipType': membershipType,
        if (category?.isNotEmpty ?? false) 'category': category,
        'page': pageNumber.toString(),
        'pageSize': pageSize.toString(),
      };
      final response = await _dio.get('/networking/search', queryParameters: params);
      
      if (response.data is Map) {
        return response.data as Map<String, dynamic>;
      }
      return { 'items': (response.data as List<dynamic>?) ?? [], 'totalItems': 0 };
    } catch (e) {
      return { 'items': [], 'totalItems': 0 };
    }
  }

  Future<Map<String, dynamic>?> getProfile() async {
    try {
      final response = await _dio.get('/profile');
      return response.data as Map<String, dynamic>;
    } catch (e) {
      return null;
    }
  }

  Future<List<dynamic>> getECPeriods() async {
    try {
      final response = await _dio.get('/networking/periods');
      final data = response.data;
      if (data is Map && data.containsKey('items')) {
        return data['items'] as List<dynamic>;
      }
      return (data as List<dynamic>?) ?? [];
    } catch (e) {
      debugPrint('NetworkingService.getECPeriods failed: $e');
      rethrow;
    }
  }

  Future<List<dynamic>> getExecutiveCommittee({int? periodId}) async {
    try {
      final response = await _dio.get('/networking/committee', queryParameters: periodId != null ? {'periodId': periodId} : {});
      return response.data as List<dynamic>;
    } catch (e) {
      debugPrint('NetworkingService.getExecutiveCommittee failed: $e');
      rethrow;
    }
  }

  Future<bool> updateProfile(Map<String, dynamic> data) async {
    try {
      final response = await _dio.put('/profile/update', data: data);
      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }
}
