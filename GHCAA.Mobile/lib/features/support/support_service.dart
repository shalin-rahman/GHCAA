import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
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
      debugPrint('SupportService.checkSystemHealth failed: $e');
      return false;
    }
  }

  Future<bool> contactSupport(String message) async {
    try {
      final response = await _dio.post('/contact', data: {'message': message});
      return response.statusCode == 200;
    } catch (e) {
      debugPrint('SupportService.contactSupport failed: $e');
      return false;
    }
  }
}

final familyServiceProvider = Provider<FamilyService>((ref) => FamilyService(ref.read(dioProvider)));

class FamilyService {
  final Dio _dio;
  FamilyService(this._dio);

  // NOTE: there are 3 distinct classes named `FamilyService` in this codebase (this one, plus
  // features/family/family_service.dart and features/networking/family_service.dart) — the
  // "(support_service.dart)" suffix disambiguates which one logged, since the class name alone
  // does not.
  Future<List<dynamic>> getFamilyLinks() async {
    try {
      final response = await _dio.get('/familylink');
      return response.data as List<dynamic>;
    } catch (e) {
      debugPrint('FamilyService.getFamilyLinks failed (support_service.dart): $e');
      rethrow;
    }
  }

  Future<bool> addFamilyMember(Map<String, dynamic> data) async {
    try {
      final response = await _dio.post('/familylink', data: data);
      return response.statusCode == 200 || response.statusCode == 201;
    } catch (e) {
      debugPrint('FamilyService.addFamilyMember failed (support_service.dart): $e');
      return false;
    }
  }
}
