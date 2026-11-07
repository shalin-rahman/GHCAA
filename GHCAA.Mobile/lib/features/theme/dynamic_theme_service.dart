import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';
import '../../core/theme/app_theme.dart';

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

  // A bad color from the admin form must not drop the whole theme, so each
  // value falls back: background to the gold the theme screen already uses,
  // text to whichever of black or white reads on that background, and the
  // optional sidebar color to none.
  factory SpecialTheme.fromJson(Map<String, dynamic> json) {
    final background =
        AppTheme.parseHexColor(json['backgroundColor']?.toString(), AppTheme.royalGold);
    return SpecialTheme(
      id: json['id'],
      title: json['title'],
      backgroundColor: background,
      textColor: AppTheme.parseHexColor(
          json['textColor']?.toString(), AppTheme.readableTextOn(background)),
      sidebarColor: AppTheme.tryParseHexColor(json['sidebarColor']?.toString()),
      announcement: json['announcementText'] ?? '',
      animation: json['animationStyle'] ?? 'Fade',
      imageUrl: json['imageUrl'],
      gradientEnabled: json['enableGradientFading'] ?? true,
    );
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
    } catch (e) {
      debugPrint('ThemeService.getActiveTheme failed: $e');
    }
    return null;
  }
}
