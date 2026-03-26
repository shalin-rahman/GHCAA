import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

final networkingServiceProvider = Provider<NetworkingService>((ref) {
  return NetworkingService(ref.read(dioProvider));
});

class NetworkingService {
  final Dio _dio;
  NetworkingService(this._dio);

  Future<List<dynamic>> searchAlumni({String query = '', String? batch, String? department}) async {
    try {
      final response = await _dio.get('/networking/search', queryParameters: {
        'query': query,
        'batch': batch,
        'department': department,
      });
      return response.data as List<dynamic>;
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

  Future<bool> updateProfile(Map<String, dynamic> data) async {
    try {
      final response = await _dio.put('/profile/update', data: data);
      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }
}
