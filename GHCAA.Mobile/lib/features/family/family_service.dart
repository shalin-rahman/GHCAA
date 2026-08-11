import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

final familyListProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(familyServiceProvider).getFamilyMembers();
});

final familyServiceProvider = Provider<FamilyService>((ref) => FamilyService(ref.read(dioProvider)));

class FamilyService {
  final Dio _dio;
  FamilyService(this._dio);

  Future<List<dynamic>> getFamilyMembers() async {
    try {
      final response = await _dio.get('/members/family');
      return response.data as List<dynamic>;
    } catch (e) {
      debugPrint('FamilyService.getFamilyMembers failed: $e');
      rethrow;
    }
  }

  Future<bool> addFamilyMember(Map<String, dynamic> data) async {
    try {
      final response = await _dio.post('/members/family', data: data);
      return response.statusCode == 200 || response.statusCode == 201;
    } catch (e) {
      return false;
    }
  }
}
