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

  // 12.5: routes an unhandled-error report to the admin inbox through the same
  // /contact endpoint the support screen uses. No rotating log file exists yet
  // (12.3), so this only carries the immediate error context, not a log attachment.
  Future<bool> sendErrorReport({
    required String fullName,
    required String email,
    required String errorSummary,
    required String stackSummary,
    required String platformInfo,
  }) async {
    try {
      final response = await _dio.post('/contact', data: {
        'fullName': fullName,
        'email': email,
        'subject': 'Mobile app error report',
        'message': 'Automatic error report from the mobile app.\n\n'
            'Error: $errorSummary\n\n'
            'Platform: $platformInfo\n\n'
            'Stack (top frames):\n$stackSummary',
      });
      return response.statusCode == 200;
    } catch (e) {
      debugPrint('SupportService.sendErrorReport failed: $e');
      return false;
    }
  }
}
