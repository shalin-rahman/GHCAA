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


  Future<String?> login(String identifier, String password, {bool enableBiometric = false}) async {
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

        // SECURITY: Clear ALL previous session data before saving new credentials.
        // This prevents role/profile leakage when switching between admin and member accounts.
        await _storage.clearAll();

        await _storage.saveToken(token);
        await _storage.saveRole(role);
        
        if (enableBiometric) {
          await _storage.saveCredentials(identifier, password); // For Biometric Fast Login
        }
        // Note: do NOT call clearCredentials here — we just called clearAll() above.
        // Credentials for biometric are only saved if the user opted in above.

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
      final String? photoPath = data['ProfileImagePath'];
      final String? nidPath = data['NidPhotoPath'];
      final String? paymentPath = data['PaymentProofPath'];
      final academicHistory = data['AcademicHistory'] as List?;

      data.remove('ProfileImagePath');
      data.remove('NidPhotoPath');
      data.remove('PaymentProofPath');
      data.remove('AcademicHistory');

      final formData = FormData.fromMap(data);

      if (photoPath != null && photoPath.isNotEmpty) {
        formData.files.add(MapEntry('photo', await MultipartFile.fromFile(photoPath, filename: 'profile_photo.jpg')));
      }
      if (nidPath != null && nidPath.isNotEmpty) {
        formData.files.add(MapEntry('certificate', await MultipartFile.fromFile(nidPath, filename: 'academic_proof.jpg')));
      }
      if (paymentPath != null && paymentPath.isNotEmpty) {
        formData.files.add(MapEntry('paymentProof', await MultipartFile.fromFile(paymentPath, filename: 'payment_receipt.jpg')));
      }

      if (academicHistory != null) {
        for (int i = 0; i < academicHistory.length; i++) {
          final record = academicHistory[i] as Map<String, dynamic>;
          record.forEach((key, value) {
            if (value != null) {
              formData.fields.add(MapEntry('AcademicHistory[$i].$key', value.toString()));
            }
          });
        }
      }

      final response = await _dio.post('/auth/register', data: formData);
      return (response.statusCode == 200 || response.statusCode == 201) ? null : "Submission failed.";
    } catch (e) {
      if (e is DioException) {
        return e.response?.data?['message'] ?? e.response?.data?['error'] ?? "Data mismatch or connection error.";
      }
      return "An unexpected error occurred.";
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

  Future<bool> updateProfile(Map<String, dynamic> data) async {
    try {
      final response = await _dio.put('/profile', data: data);
      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }
}

// Non-autoDispose so role is cached in Riverpod across navigations.
// Invalidate explicitly after login/logout via ref.invalidate(roleProvider).
final roleProvider = FutureProvider<String?>((ref) async {
  return ref.read(authServiceProvider).getRole();
});

/// Canonical list of profile fields for completeness calculation.
/// Uses exact keys returned by the backend /profile endpoint.
/// Update this one list to affect all completeness indicators across the app.
const profileCompletenessFields = [
  'fullName',
  'email',
  'mobileNo',      // API key — NOT 'phoneNumber'
  'dateOfBirth',
  'gender',
  'membershipNumber',
  'photoPath',
  'passingYear',   // API key — NOT 'batch'
  'subject',       // API key — NOT 'department'
  'presentAddress',
  'bloodGroup',
];

/// Returns a value between 0.0 and 1.0 representing profile completeness.
double calculateProfileCompleteness(Map<String, dynamic>? profile) {
  if (profile == null) return 0;
  final filled = profileCompletenessFields
      .where((f) => profile[f] != null && profile[f].toString().isNotEmpty)
      .length;
  return filled / profileCompletenessFields.length;
}

// Non-autoDispose so profile is cached in Riverpod across navigations.
// Invalidate explicitly after login/logout via ref.invalidate(userProfileProvider).
final userProfileProvider = FutureProvider<Map<String, dynamic>?>((ref) async {
  final storage = ref.read(storageServiceProvider);
  try {
    final dio = ref.read(dioProvider);
    final response = await dio.get('/profile');
    if (response.statusCode == 200 && response.data != null) {
      final profile = Map<String, dynamic>.from(response.data);
      await storage.saveProfile(profile);
      return profile;
    }
    return await storage.getProfile();
  } catch (_) {
    return await storage.getProfile();
  }
});

class ActivityNotifier extends StateNotifier<DateTime> {
  ActivityNotifier() : super(DateTime.now());
  void update() => state = DateTime.now();
}

final lastActivityProvider = StateNotifierProvider<ActivityNotifier, DateTime>((ref) => ActivityNotifier());

/// Admin / SuperAdmin checks for mobile UI (case-insensitive; tolerant of API casing).
extension UserRoleExt on String? {
  bool get isStaffAdminRole {
    final r = this?.trim().toLowerCase();
    return r == 'superadmin' || r == 'admin';
  }
}
