import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

class SpecialTheme {
  final int id;
  final String title;
  final Color backgroundColor;
  final Color textColor;
  final Color? sidebarColor;
  final String announcement;
  final String animation;
  final String? imageUrl;
  final bool gradientEnabled;

  SpecialTheme({
    required this.id,
    required this.title,
    required this.backgroundColor,
    required this.textColor,
    this.sidebarColor,
    required this.announcement,
    required this.animation,
    this.imageUrl,
    this.gradientEnabled = true,
  });

  factory SpecialTheme.fromJson(Map<String, dynamic> json) {
    return SpecialTheme(
      id: json['id'],
      title: json['title'],
      backgroundColor: _parseHex(json['backgroundColor']),
      textColor: _parseHex(json['textColor']),
      sidebarColor: json['sidebarColor'] != null && json['sidebarColor'].toString().isNotEmpty ? _parseHex(json['sidebarColor']) : null,
      announcement: json['announcementText'] ?? '',
      animation: json['animationStyle'] ?? 'Fade',
      imageUrl: json['imageUrl'],
      gradientEnabled: json['enableGradientFading'] ?? true,
    );
  }

  static Color _parseHex(String hex) {
    hex = hex.replaceFirst('#', '');
    if (hex.length == 6) hex = 'FF$hex';
    return Color(int.parse(hex, radix: 16));
  }
}

final themeServiceProvider = Provider<ThemeService>((ref) => ThemeService(ref.read(dioProvider)));

final activeSpecialThemeProvider = FutureProvider<SpecialTheme?>((ref) async {
  return ref.read(themeServiceProvider).getActiveTheme();
});

final globalAppBarVisibilityProvider = StateProvider<bool>((ref) => true);

class ThemeService {
  final Dio _dio;
  ThemeService(this._dio);

  Future<SpecialTheme?> getActiveTheme() async {
    try {
      final response = await _dio.get('/Theme/active');
      if (response.statusCode == 200 && response.data != null) {
        return SpecialTheme.fromJson(response.data);
      }
    } catch (_) {
      // Background failure is acceptable in production
    }
    return null;
  }
}
