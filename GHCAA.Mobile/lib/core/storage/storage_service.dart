import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:shared_preferences/shared_preferences.dart';

final storageServiceProvider = Provider<StorageService>((ref) => StorageService());

class StorageService {
  static const _jwtKey = 'jwt_token';
  static const _legacyJwtPrefsKey = 'jwt_token';

  final FlutterSecureStorage _secure = const FlutterSecureStorage();

  Future<void> saveToken(String token) async {
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
    if (kIsWeb) {
      final prefs = await SharedPreferences.getInstance();
      return prefs.getString(_legacyJwtPrefsKey);
    }
    var token = await _secure.read(key: _jwtKey);
    if (token != null && token.isNotEmpty) return token;
    final prefs = await SharedPreferences.getInstance();
    final legacy = prefs.getString(_legacyJwtPrefsKey);
    if (legacy != null && legacy.isNotEmpty) {
      await _secure.write(key: _jwtKey, value: legacy);
      await prefs.remove(_legacyJwtPrefsKey);
      return legacy;
    }
    return null;
  }

  Future<void> removeToken() async {
    if (kIsWeb) {
      final prefs = await SharedPreferences.getInstance();
      await prefs.remove(_legacyJwtPrefsKey);
      return;
    }
    await _secure.delete(key: _jwtKey);
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_legacyJwtPrefsKey);
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
    final prefs = await SharedPreferences.getInstance();
    await prefs.clear();
  }
}
