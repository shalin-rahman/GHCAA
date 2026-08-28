import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

final roleServiceProvider = Provider<RoleService>((ref) => RoleService(ref.read(dioProvider)));

class RoleService {
  final Dio _dio;
  RoleService(this._dio);

  Future<List<dynamic>> getAllRoles() async {
    try {
      final response = await _dio.get('/Roles');
      return response.data as List<dynamic>;
    } catch (e) {
      debugPrint('RoleService.getAllRoles failed: $e');
      rethrow;
    }
  }

  Future<bool> assignRole(int memberId, String roleName) async {
    try {
      final response = await _dio.post('/Roles/assign', data: {'memberId': memberId, 'roleName': roleName});
      return response.statusCode == 200;
    } catch (e) {
      debugPrint('RoleService.assignRole failed: $e');
      return false;
    }
  }
}
