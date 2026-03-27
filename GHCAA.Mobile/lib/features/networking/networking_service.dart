import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

final networkingServiceProvider = Provider<NetworkingService>((ref) {
  return NetworkingService(ref.read(dioProvider));
});

class NetworkingService {
  final Dio _dio;
  NetworkingService(this._dio);

  Future<List<dynamic>> searchAlumni({String query = '', String? batch, String? department, int pageNumber = 1, int pageSize = 20}) async {
    try {
      final response = await _dio.get('/networking/search', queryParameters: {
        if (query.isNotEmpty) 'query': query,
        if (batch != null) 'passingYear': batch,
        if (department != null) 'category': department,
        'pageNumber': pageNumber,
        'pageSize': pageSize,
      });
      
      if (response.data is Map && response.data.containsKey('items')) {
        return response.data['items'] as List<dynamic>;
      }
      return (response.data as List<dynamic>?) ?? [];
    } catch (e) {
      return [];
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
      return [];
    }
  }

  Future<List<dynamic>> getExecutiveCommittee({int? periodId}) async {
    try {
      final response = await _dio.get('/networking/committee', queryParameters: periodId != null ? {'periodId': periodId} : {});
      return response.data as List<dynamic>;
    } catch (e) {
      return [];
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
