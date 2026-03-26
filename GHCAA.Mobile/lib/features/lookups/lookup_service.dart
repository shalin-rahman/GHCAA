import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

final lookupServiceProvider = Provider<LookupService>((ref) => LookupService(ref.read(dioProvider)));

class LookupService {
  final Dio _dio;
  LookupService(this._dio);

  Future<Map<String, List<dynamic>>> getAllLookups() async {
    try {
      final response = await _dio.get('/lookups');
      return Map<String, List<dynamic>>.from(response.data);
    } catch (e) {
      return {};
    }
  }

  Future<List<dynamic>> getByGroup(String group) async {
    try {
      final response = await _dio.get('/lookups/$group');
      return response.data as List<dynamic>;
    } catch (e) {
      return [];
    }
  }
}
