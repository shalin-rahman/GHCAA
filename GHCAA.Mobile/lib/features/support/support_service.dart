import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

final supportServiceProvider = Provider<SupportService>((ref) => SupportService(ref.read(dioProvider)));

class SupportService {
  final Dio _dio;
  SupportService(this._dio);

  Future<bool> checkSystemHealth() async {
    try {
      final response = await _dio.get('/health');
      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }

  Future<bool> contactSupport(String message) async {
    try {
      final response = await _dio.post('/contact', data: {'message': message});
      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }
}

final familyServiceProvider = Provider<FamilyService>((ref) => FamilyService(ref.read(dioProvider)));

class FamilyService {
  final Dio _dio;
  FamilyService(this._dio);

  Future<List<dynamic>> getFamilyLinks() async {
    try {
      final response = await _dio.get('/familylink');
      return response.data as List<dynamic>;
    } catch (e) {
      return [];
    }
  }

  Future<bool> addFamilyMember(Map<String, dynamic> data) async {
    try {
      final response = await _dio.post('/familylink', data: data);
      return response.statusCode == 200 || response.statusCode == 201;
    } catch (e) {
      return false;
    }
  }
}
