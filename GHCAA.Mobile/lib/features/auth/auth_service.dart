import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';
import '../../core/storage/storage_service.dart';

final authServiceProvider = Provider<AuthService>((ref) {
  return AuthService(ref.read(dioProvider), ref.read(storageServiceProvider));
});

class AuthService {
  final Dio _dio;
  final StorageService _storage;

  AuthService(this._dio, this._storage);

  Future<bool> login(String identifier, String password) async {
    try {
      final response = await _dio.post('/auth/login', data: {
        'identifier': identifier, 
        'password': password,
      });

      if (response.statusCode == 200) {
        final data = response.data;
        final token = data['token']; 
        final role = data['role'] ?? 'Member';

        await _storage.saveToken(token);
        await _storage.saveRole(role);

        return true;
      }
    } catch (e) {
      print('Login Error: $e');
    }
    return false;
  }

  Future<bool> register(Map<String, dynamic> data) async {
    try {
      final response = await _dio.post('/auth/register', data: data);
      return response.statusCode == 200 || response.statusCode == 201;
    } catch (e) {
      print('Registration Error: $e');
      return false;
    }
  }

  Future<bool> forgotPassword(String identifier) async {
    try {
      final response = await _dio.post('/auth/forgot-password', data: {'identifier': identifier});
      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }

  Future<void> logout() async {
    await _storage.clearAll();
  }

  Future<String?> getRole() async {
    return _storage.getRole();
  }
}

final userProfileProvider = FutureProvider.autoDispose<Map<String, dynamic>?>((ref) async {
  try {
    final dio = ref.read(dioProvider);
    final response = await dio.get('/members/profile');
    if (response.statusCode == 200 && response.data != null) {
      return Map<String, dynamic>.from(response.data);
    }
    return null;
  } catch (_) {
    return null;
  }
});
