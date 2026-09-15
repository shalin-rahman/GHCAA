import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:shared_preferences/shared_preferences.dart';

final storageServiceProvider = Provider<StorageService>((ref) => StorageService());

class StorageService {
  static const _jwtKey = 'jwt_token';
  static const _legacyJwtPrefsKey = 'jwt_token';
  // 82.40: these two used to hold the member's raw username/password for biometric "fast
  // login". Nothing writes to them any more (biometric re-login now reuses the refresh
  // token below, the same device-bound, server-revocable credential the app already keeps),
  // but the keys stay named here so purgeLegacyBiometricCredentials() can still find and
  // delete them on a device that has an older build's plaintext password sitting in storage.
  static const _credUserKey = 'cred_user';
  static const _credPassKey = 'cred_pass';
  static const _biometricEnabledKey = 'biometric_enabled';
  static const _refreshTokenKey = 'refresh_token';
  static const _legacyRefreshTokenPrefsKey = 'refresh_token';

  final FlutterSecureStorage _secure = const FlutterSecureStorage();
  String? _cachedToken;
  String? _cachedRefreshToken;

  Future<void> saveToken(String token) async {
    _cachedToken = token;
    if (kIsWeb) {
      final prefs = await SharedPreferences.getInstance();
      await prefs.setString(_legacyJwtPrefsKey, token);
      return;
    }
    await _secure.write(key: _jwtKey, value: token);
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_legacyJwtPrefsKey);
  }

  Future<String?> getToken() async {
    if (_cachedToken != null) return _cachedToken;
    if (kIsWeb) {
      final prefs = await SharedPreferences.getInstance();
      _cachedToken = prefs.getString(_legacyJwtPrefsKey);
      return _cachedToken;
    }
    var token = await _secure.read(key: _jwtKey);
    if (token != null && token.isNotEmpty) {
      _cachedToken = token;
      return token;
    }
    final prefs = await SharedPreferences.getInstance();
    final legacy = prefs.getString(_legacyJwtPrefsKey);
    if (legacy != null && legacy.isNotEmpty) {
      _cachedToken = legacy;
      await _secure.write(key: _jwtKey, value: legacy);
      await prefs.remove(_legacyJwtPrefsKey);
      return legacy;
    }
    return null;
  }

  Future<void> removeToken() async {
    _cachedToken = null;
    if (kIsWeb) {
      final prefs = await SharedPreferences.getInstance();
      await prefs.remove(_legacyJwtPrefsKey);
      return;
    }
    await _safeDelete(_jwtKey);
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_legacyJwtPrefsKey);
  }

  Future<void> saveRefreshToken(String token) async {
    _cachedRefreshToken = token;
    if (kIsWeb) {
      final prefs = await SharedPreferences.getInstance();
      await prefs.setString(_legacyRefreshTokenPrefsKey, token);
      return;
    }
    await _secure.write(key: _refreshTokenKey, value: token);
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_legacyRefreshTokenPrefsKey);
  }

  Future<String?> getRefreshToken() async {
    if (_cachedRefreshToken != null) return _cachedRefreshToken;
    if (kIsWeb) {
      final prefs = await SharedPreferences.getInstance();
      _cachedRefreshToken = prefs.getString(_legacyRefreshTokenPrefsKey);
      return _cachedRefreshToken;
    }
    final token = await _secure.read(key: _refreshTokenKey);
    if (token != null && token.isNotEmpty) {
      _cachedRefreshToken = token;
      return token;
    }
    return null;
  }

  Future<void> removeRefreshToken() async {
    _cachedRefreshToken = null;
    if (kIsWeb) {
      final prefs = await SharedPreferences.getInstance();
      await prefs.remove(_legacyRefreshTokenPrefsKey);
      return;
    }
    await _safeDelete(_refreshTokenKey);
  }

  // flutter_secure_storage's Windows backend keeps its values in one DPAPI-encrypted
  // file. If that file is still locked by another process (e.g. this same exe from a
  // just-finished prior run) a delete throws PathAccessException, which otherwise
  // aborts logout() before it reaches context.go('/login') and strands the user on
  // the dashboard. A failed delete just leaves a stale entry to be overwritten on the
  // next saveToken/saveRefreshToken, so it's safe to swallow here.
  Future<void> _safeDelete(String key) async {
    try {
      await _secure.delete(key: key);
    } catch (e) {
      debugPrint('StorageService: failed to delete secure key "$key": $e');
    }
  }

  Future<void> saveRole(String role) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('user_role', role);
  }

  Future<String?> getRole() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString('user_role');
  }

  Future<void> saveDashboardLayout(bool isCompact) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool('dashboard_is_compact', isCompact);
  }

  Future<bool> getDashboardLayout() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getBool('dashboard_is_compact') ?? false;
  }

  Future<void> saveProfile(Map<String, dynamic> profile) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('user_profile_cache', jsonEncode(profile));
  }

  Future<Map<String, dynamic>?> getProfile() async {
    final prefs = await SharedPreferences.getInstance();
    final profileStr = prefs.getString('user_profile_cache');
    if (profileStr != null) {
      return jsonDecode(profileStr) as Map<String, dynamic>;
    }
    return null;
  }

  Future<void> clearAll() async {
    await removeToken();
    await removeRefreshToken();
    await purgeLegacyBiometricCredentials();
    final prefs = await SharedPreferences.getInstance();
    await prefs.clear(); // also drops the biometric_enabled flag
  }

  Future<void> setBiometricEnabled(bool enabled) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool(_biometricEnabledKey, enabled);
  }

  Future<bool> isBiometricEnabled() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getBool(_biometricEnabledKey) ?? false;
  }

  /// Scrubs the plaintext username/password an older build may have written for biometric
  /// fast login. Never writes to these keys — only deletes.
  Future<void> purgeLegacyBiometricCredentials() async {
    if (kIsWeb) return;
    await _safeDelete(_credUserKey);
    await _safeDelete(_credPassKey);
  }
}
