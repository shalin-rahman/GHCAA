import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';
import '../../core/api/api_exception.dart';

final rolesServiceProvider = Provider<RolesService>((ref) {
  return RolesService(ref.read(dioProvider));
});

class RolesService {
  final Dio _dio;
  RolesService(this._dio);

  // --- Users (Admin Accounts) ---
  Future<List<dynamic>> getUsers() async {
    try {
      final r = await _dio.get('/roles/users');
      return r.data as List<dynamic>;
    } catch (e) {
      debugPrint('RolesService.getUsers failed: $e');
      rethrow;
    }
  }

  Future<bool> createAdmin(String username, String password, String role) async {
    try {
      final r = await _dio.post('/roles/users', data: {
        'username': username,
        'password': password,
        'role': role,
      });
      return r.statusCode == 200;
    } catch (e) {
      debugPrint('RolesService.createAdmin failed: $e');
      return false;
    }
  }

  // FR-39: ends the login lock and the authenticator lock early. Null on success,
  // otherwise the server's reason. The step-up prompt is handled by the interceptor.
  Future<String?> unlockUser(int userId) async {
    try {
      await _dio.post('/roles/users/$userId/unlock');
      return null;
    } on DioException catch (e) {
      debugPrint('RolesService.unlockUser failed: $e');
      if (e.response?.statusCode == 404) return 'User not found.';
      return ApiException.serverMessage(e.response?.data) ?? 'Failed to unlock the account.';
    }
  }

  // --- Roles ---
  Future<List<dynamic>> getRoles() async {
    try {
      final r = await _dio.get('/roles');
      return r.data as List<dynamic>;
    } catch (e) {
      debugPrint('RolesService.getRoles failed: $e');
      rethrow;
    }
  }

  Future<bool> createRole(String roleName) async {
    try {
      final r = await _dio.post('/roles', data: '"$roleName"');
      return r.statusCode == 200;
    } catch (e) {
      debugPrint('RolesService.createRole failed: $e');
      return false;
    }
  }

  Future<bool> assignRole(int userId, String roleName) async {
    try {
      final r = await _dio.post('/roles/assign', queryParameters: {
        'userId': userId,
        'roleName': roleName,
      });
      return r.statusCode == 200;
    } catch (e) {
      debugPrint('RolesService.assignRole failed: $e');
      return false;
    }
  }

  Future<bool> removeRole(int userId, String roleName) async {
    try {
      final r = await _dio.post('/roles/remove', queryParameters: {
        'userId': userId,
        'roleName': roleName,
      });
      return r.statusCode == 200;
    } catch (e) {
      debugPrint('RolesService.removeRole failed: $e');
      return false;
    }
  }
}
