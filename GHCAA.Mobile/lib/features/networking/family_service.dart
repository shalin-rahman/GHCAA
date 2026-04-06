import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

final familyServiceProvider = Provider<FamilyService>((ref) {
  return FamilyService(ref.read(dioProvider));
});

class FamilyService {
  final Dio _dio;
  FamilyService(this._dio);

  Future<List<dynamic>> getMyFamily() async {
    try {
      final response = await _dio.get('/family-links/my-family');
      return response.data as List<dynamic>;
    } catch (e) {
      return [];
    }
  }

  Future<List<dynamic>> getSentRequests() async {
    try {
      final response = await _dio.get('/family-links/sent');
      return response.data as List<dynamic>;
    } catch (e) {
      return [];
    }
  }

  Future<List<dynamic>> getReceivedRequests() async {
    try {
      final response = await _dio.get('/family-links/received');
      return response.data as List<dynamic>;
    } catch (e) {
      return [];
    }
  }

  Future<bool> sendRequest(String membershipNo, int relationshipType, {String? note}) async {
    try {
      final response = await _dio.post('/family-links/send', data: {
        'targetMembershipNumber': membershipNo,
        'relationship': relationshipType,
        'note': note,
      });
      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }

  Future<bool> respondToRequest(int requestId, bool approve) async {
    try {
      final response = await _dio.post('/family-links/respond', data: {
        'requestId': requestId,
        'approve': approve,
      });
      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }

  Future<bool> cancelRequest(int requestId) async {
    try {
      final response = await _dio.post('/family-links/$requestId/cancel');
      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }

  Future<bool> removeLink(int requestId) async {
    try {
      final response = await _dio.delete('/family-links/remove/$requestId');
      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }
}
