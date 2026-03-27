import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';
import '../../core/services/device_info_service.dart';
import '../../core/storage/storage_service.dart';


final authServiceProvider = Provider<AuthService>((ref) {
  return AuthService(ref.read(dioProvider), ref.read(storageServiceProvider), ref);
});


class AuthService {
  final Dio _dio;
  final StorageService _storage;
  final Ref _ref;

  AuthService(this._dio, this._storage, this._ref);


  Future<String?> login(String identifier, String password) async {
    try {
      DeviceInfo? device;
      try {
        device = await _ref.read(deviceInfoProvider.future);
      } catch (_) {}

      final response = await _dio.post('/auth/login', data: {
        'username': identifier, 
        'password': password,
        if (device != null) ...device.toJson(),
      });


      if (response.statusCode == 200) {
        final data = response.data;
        final token = data['token']; 
        final role = data['role'] ?? 'Member';

        await _storage.saveToken(token);
        await _storage.saveRole(role);

        return null; // Success
      }
    } catch (e) {
      if (e is DioException) {
        if (e.response?.statusCode == 401) {
          return "Invalid username or password.";
        }
        if (e.type == DioExceptionType.connectionTimeout || e.type == DioExceptionType.receiveTimeout) {
          return "Server connection timed out.";
        }
      }
      return "An unexpected error occurred. Please try again.";
    }
    return "Login failed. Please check your credentials.";
  }

  Future<String?> register(Map<String, dynamic> data) async {
    try {
      final response = await _dio.post('/auth/register', data: data);
      if (response.statusCode == 200 || response.statusCode == 201) {
        return null; // Success
      }
    } catch (e) {
      if (e is DioException) {
        if (e.response?.statusCode == 400) {
          final msg = e.response?.data?['message'] ?? e.response?.data?['error'] ?? "Individual details already registered or data mismatch.";
          return msg;
        }
        if (e.type == DioExceptionType.connectionTimeout || e.type == DioExceptionType.receiveTimeout) {
          return "Connection timed out. Please try again.";
        }
      }
      return "An unexpected error occurred during submission.";
    }
    return "Submission failed. Please check your data.";
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

final roleProvider = FutureProvider.autoDispose<String?>((ref) async {
  return ref.read(authServiceProvider).getRole();
});

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
