import 'package:dio/dio.dart';
import '../../core/utils/upload_file_naming.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';
import '../../core/services/device_info_service.dart';
import '../../core/storage/storage_service.dart';
import '../../core/real_time/notification_hub_service.dart';
import '../../core/router/app_router.dart';
import '../../core/models/auth_models.dart';
import '../../core/models/member_profile.dart';


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
      } catch (e) {
        debugPrint('AuthService.login: device info unavailable: $e');
      }

      final response = await _dio.post('/auth/login', data: {
        'username': identifier, 
        'password': password,
        if (device != null) ...device.toJson(),
      });


      if (response.statusCode == 200) {
        final parsed = LoginResponse.fromJson(Map<String, dynamic>.from(response.data));

        // SECURITY: Clear ALL previous session data before saving new credentials.
        // This prevents role/profile leakage when switching between admin and member accounts.
        await _storage.clearAll();

        await _storage.saveToken(parsed.token);
        if (parsed.refreshToken != null) await _storage.saveRefreshToken(parsed.refreshToken!);
        await _storage.saveRole(parsed.role);
        
        // 82.40: biometric re-login used to store the raw password here. It now just flips
        // this flag; loginWithStoredToken() below re-authenticates from the refresh token
        // that saveRefreshToken already persisted above, so no password is ever written.
        await _storage.setBiometricEnabled(enableBiometric);

        // roleProvider/userProfileProvider are non-autoDispose and cache across
        // navigations, so a stale value from the previous session survives clearAll()
        // above until something re-reads them. Invalidate so the new user's login
        // always refetches instead of showing whoever was logged in before.
        _ref.invalidate(roleProvider);
        _ref.invalidate(userProfileProvider);

        return null; // Success
      }
    } catch (e) {
      debugPrint('AuthService.login failed: $e');
      if (e is DioException) {
        if (e.response?.statusCode == 401) {
          return "Invalid username or password.";
        }
        if (e.type == DioExceptionType.connectionTimeout || e.type == DioExceptionType.receiveTimeout) {
          return "Server connection timed out.";
        }
      }
      return "Something went wrong. Try again.";
    }
    return "Login failed. Please check your credentials.";
  }

  /// 82.40: biometric "fast login" re-authenticates through the already-stored refresh
  /// token (the same device-bound, server-rotatable token /auth/refresh-mobile issues on
  /// every normal login) instead of resubmitting a saved plaintext password.
  Future<String?> loginWithStoredToken() async {
    try {
      final refreshToken = await _storage.getRefreshToken();
      if (refreshToken == null) {
        return "No saved session found. Please log in with your password.";
      }

      final response = await _dio.post('/auth/refresh-mobile', data: {'refreshToken': refreshToken});
      if (response.statusCode == 200) {
        final parsed = LoginResponse.fromJson(Map<String, dynamic>.from(response.data));

        await _storage.saveToken(parsed.token);
        if (parsed.refreshToken != null) await _storage.saveRefreshToken(parsed.refreshToken!);

        _ref.invalidate(roleProvider);
        _ref.invalidate(userProfileProvider);
        return null; // Success
      }
    } catch (e) {
      debugPrint('AuthService.loginWithStoredToken failed: $e');
      if (e is DioException && e.response?.statusCode == 401) {
        // Refresh token was revoked or expired server-side — clear it so the biometric
        // option disappears until the member logs in with a password again.
        await _storage.clearAll();
        return "Your saved session has expired. Please log in with your password.";
      }
      return "Something went wrong. Try again.";
    }
    return "Session refresh failed. Please log in with your password.";
  }

  Future<List<Map<String, dynamic>>> getSocialProviders() async {
    try {
      final response = await _dio.get('/auth/providers');
      if (response.statusCode == 200) {
        return List<Map<String, dynamic>>.from(response.data);
      }
    } catch (e) {
      debugPrint('AuthService.getSocialProviders failed: $e');
    }
    return [];
  }

  Future<String?> googleLogin(String idToken) async {
    return _socialLogin('/auth/google', {'idToken': idToken});
  }

  Future<String?> facebookLogin(String accessToken) async {
    return _socialLogin('/auth/facebook', {'accessToken': accessToken});
  }

  Future<String?> _socialLogin(String path, Map<String, dynamic> data) async {
    try {
      final response = await _dio.post(path, data: data);
      if (response.statusCode == 200) {
        final parsed = LoginResponse.fromJson(Map<String, dynamic>.from(response.data));

        await _storage.clearAll();
        await _storage.saveToken(parsed.token);
        if (parsed.refreshToken != null) await _storage.saveRefreshToken(parsed.refreshToken!);
        await _storage.saveRole(parsed.role);
        
        return null; // Success
      }
    } catch (e) {
      debugPrint('AuthService._socialLogin failed ($path): $e');
      if (e is DioException) {
        return e.response?.data?['message'] ?? e.response?.data?['error'] ?? "Authentication failed.";
      }
      return "Something went wrong.";
    }
    return "Authentication failed.";
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
        formData.files.add(MapEntry('photo', await MultipartFile.fromFile(photoPath, filename: UploadFileNaming.forType('photo', photoPath))));
      }
      if (nidPath != null && nidPath.isNotEmpty) {
        formData.files.add(MapEntry('certificate', await MultipartFile.fromFile(nidPath, filename: UploadFileNaming.forType('certificate', nidPath))));
      }
      if (paymentPath != null && paymentPath.isNotEmpty) {
        formData.files.add(MapEntry('paymentProof', await MultipartFile.fromFile(paymentPath, filename: UploadFileNaming.forType('paymentproof', paymentPath))));
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
      debugPrint('AuthService.register failed: $e');
      if (e is DioException) {
        return e.response?.data?['message'] ?? e.response?.data?['error'] ?? "Data mismatch or connection error.";
      }
      return "Something went wrong.";
    }
  }

  Future<bool> forgotPassword(String identifier) async {
    try {
      final response = await _dio.post('/auth/forgot-password', data: {'identifier': identifier});
      return response.statusCode == 200;
    } catch (e) {
      debugPrint('AuthService.forgotPassword failed: $e');
      return false;
    }
  }

  Future<void> logout() async {
    // 29E.3: tear down the SignalR NotificationHub (socket + 4 broadcast controllers) on
    // logout. Invalidating the provider fires its ref.onDispose → dispose(); otherwise the
    // authenticated hub would keep streaming for the previous user until app kill.
    _ref.invalidate(notificationHubServiceProvider);
    // 82.38: same reasoning applies to role/profile — both are non-autoDispose and were
    // being left cached, so a second user on a shared device could see the first user's
    // role/profile (including admin-only UI) until something else triggered a refetch.
    _ref.invalidate(roleProvider);
    _ref.invalidate(userProfileProvider);
    await _storage.clearAll();
    // Invalidating alone isn't enough: the new stream doesn't emit until its
    // create callback actually runs, and callers navigate away right after
    // this returns. Without awaiting the first value here, the router still
    // reads the pre-logout cached token at that point and bounces the user
    // straight back to the dashboard instead of the login screen.
    _ref.invalidate(authStateProvider);
    await _ref.read(authStateProvider.future);
  }

  Future<String?> getRole() async {
    return _storage.getRole();
  }

  Future<bool> updateProfile(Map<String, dynamic> data) async {
    try {
      final response = await _dio.put('/profile', data: data);
      return response.statusCode == 200;
    } catch (e) {
      debugPrint('AuthService.updateProfile failed: $e');
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
/// Adheres to the 100% completion definition: mandatory fields, photo, 1+ academic, 1+ professional.
double calculateProfileCompleteness(Map<String, dynamic>? profile) {
  if (profile == null) return 0;
  
  int totalWeight = 0;
  int filledWeight = 0;

  // 1. Mandatory Personal Fields (Weight: 1 each)
  final personalFields = ['fullName', 'email', 'mobileNo', 'dateOfBirth', 'gender', 'presentAddress', 'bloodGroup', 'nid'];
  for (var f in personalFields) {
    totalWeight++;
    if (profile[f] != null && profile[f].toString().isNotEmpty) filledWeight++;
  }

  // 2. Profile Photo (Weight: 2)
  totalWeight += 2;
  if (profile['photoPath'] != null && profile['photoPath'].toString().isNotEmpty) filledWeight += 2;

  // 3. Academic History (At least one entry) (Weight: 2)
  totalWeight += 2;
  final academic = profile['academicHistory'];
  if (academic is List && academic.isNotEmpty) {
    // Check if at least one entry is reasonably complete
    final first = academic[0];
    if (first['institutionName'] != null && first['degree'] != null) filledWeight += 2;
  }

  // 4. Professional History (At least one entry) (Weight: 2)
  totalWeight += 2;
  final professional = profile['professionalHistory'];
  if (professional is List && professional.isNotEmpty) {
    final first = professional[0];
    if (first['organizationName'] != null && first['designation'] != null) filledWeight += 2;
  }

  return filledWeight / totalWeight;
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
      // Parse-only, for the throw: profile_screen.dart and profile_edit_screen.dart
      // still read the map directly (profile_edit_screen edits too many dynamic
      // admin/list fields to be worth a rigid typed rewrite), so this call's only
      // job is to fail loudly here if a required field drifted, before the raw map
      // gets cached and handed to them.
      MemberProfile.fromJson(profile);
      await storage.saveProfile(profile);
      return profile;
    }
    return await storage.getProfile();
  } catch (e) {
    debugPrint('userProfileProvider failed: $e');
    return await storage.getProfile();
  }
});

/// Admin / SuperAdmin checks for mobile UI (case-insensitive; tolerant of API casing).
extension UserRoleExt on String? {
  bool get isStaffAdminRole {
    final r = this?.trim().toLowerCase();
    return r == 'superadmin' || r == 'admin';
  }
}
