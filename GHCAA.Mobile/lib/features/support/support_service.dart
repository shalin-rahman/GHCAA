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
