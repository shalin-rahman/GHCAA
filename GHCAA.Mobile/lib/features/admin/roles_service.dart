import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

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
    } catch (_) {
      return [];
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
    } catch (_) {
      return false;
    }
  }

  // --- Roles ---
  Future<List<dynamic>> getRoles() async {
    try {
      final r = await _dio.get('/roles');
      return r.data as List<dynamic>;
    } catch (_) {
      return [];
    }
  }

  Future<bool> createRole(String roleName) async {
    try {
      final r = await _dio.post('/roles', data: '"$roleName"');
      return r.statusCode == 200;
    } catch (_) {
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
    } catch (_) {
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
    } catch (_) {
      return false;
    }
  }
}
