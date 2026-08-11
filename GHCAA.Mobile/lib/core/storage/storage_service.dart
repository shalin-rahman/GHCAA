import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:shared_preferences/shared_preferences.dart';

final storageServiceProvider = Provider<StorageService>((ref) => StorageService());

class StorageService {
  static const _jwtKey = 'jwt_token';
  static const _legacyJwtPrefsKey = 'jwt_token';
  static const _credUserKey = 'cred_user';
  static const _credPassKey = 'cred_pass';
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
    await _secure.delete(key: _jwtKey);
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
    await _secure.delete(key: _refreshTokenKey);
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
    await clearCredentials();
    final prefs = await SharedPreferences.getInstance();
    await prefs.clear();
  }

  Future<void> saveCredentials(String username, String password) async {
    if (kIsWeb) return; // Do not store passwords on web implicitly
    await _secure.write(key: _credUserKey, value: username);
    await _secure.write(key: _credPassKey, value: password);
  }

  Future<Map<String, String>?> getCredentials() async {
    if (kIsWeb) return null;
    final user = await _secure.read(key: _credUserKey);
    final pass = await _secure.read(key: _credPassKey);
    if (user != null && pass != null) return {'username': user, 'password': pass};
    return null;
  }

  Future<void> clearCredentials() async {
    if (kIsWeb) return;
    await _secure.delete(key: _credUserKey);
    await _secure.delete(key: _credPassKey);
  }
}
