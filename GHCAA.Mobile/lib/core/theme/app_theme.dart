import 'package:flutter/material.dart';

class AppTheme {
  // Core Brand Palette - "Obsidian & Gold Leaf"
  static const Color obsidianBlack = Color(0xFF000000);
  static const Color deepCharcoal = Color(0xFF0C0C0C);
  static const Color royalGold = Color(0xFFC5A059);
  static const Color brightGold = Color(0xFFE5C15E);
  static const Color darkGold = Color(0xFF8E6D2A);
  
  static const Color textMain = Color(0xFFF5F5F5);
  static const Color textMuted = Color(0xFF888888);
  
  static const Color glassBorder = Color(0x14FFFFFF); // rgba(255, 255, 255, 0.08)
  static const Color shadowColor = Color(0x99000000); // rgba(0, 0, 0, 0.6)

  // Legacy Aliases for backward compatibility
  static const Color midnightBase = obsidianBlack;
  static const Color midnightSurface = deepCharcoal;
  static const Color daylightBase = Color(0xFFF5F5F5); 
  static const Color daylightSurface = Color(0xFFFFFFFF);
  
  static const Color adminMidnightBase = obsidianBlack; 
  static const Color adminMidnightSurface = deepCharcoal;
  static const Color adminDaylightSurface = Color(0xFFFFFFFF);

  static const Color textPrimaryDark = textMain;
  static const Color textSecondaryDark = textMuted;
  static const Color textSecondaryLight = Color(0xFF6C757D);

  static const Color royalGoldVibrant = brightGold;

  static final ThemeData darkTheme = ThemeData(
    useMaterial3: true,
    brightness: Brightness.dark,
    scaffoldBackgroundColor: obsidianBlack,
    colorScheme: const ColorScheme.dark(
      primary: royalGold,
      onPrimary: obsidianBlack,
      secondary: brightGold,
      surface: deepCharcoal,
      onSurface: textMain,
      outline: glassBorder,
    ),
    textTheme: const TextTheme(
      displayLarge: TextStyle(
        fontFamily: 'Outfit',
        fontWeight: FontWeight.w800,
        color: textMain,
        letterSpacing: -0.02,
      ),
      bodyLarge: TextStyle(
        fontFamily: 'Outfit',
        color: textMain,
        fontSize: 16,
      ),
      bodyMedium: TextStyle(
        fontFamily: 'Outfit',
        color: textMuted,
        fontSize: 14,
      ),
    ),
    elevatedButtonTheme: ElevatedButtonThemeData(
      style: ElevatedButton.styleFrom(
        backgroundColor: royalGold,
        foregroundColor: obsidianBlack,
        textStyle: const TextStyle(
          fontFamily: 'Outfit',
          fontWeight: FontWeight.w800,
          letterSpacing: 1.0,
        ),
        padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 24),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(8),
        ),
        elevation: 8,
        shadowColor: royalGold.withOpacity(0.4),
      ),
    ),
    inputDecorationTheme: InputDecorationTheme(
      filled: true,
      fillColor: obsidianBlack,
      labelStyle: const TextStyle(color: textMuted, fontFamily: 'Outfit'),
      hintStyle: const TextStyle(color: Colors.white24, fontFamily: 'Outfit'),
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(8),
        borderSide: const BorderSide(color: Color(0xFF1A1A1A)),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(8),
        borderSide: const BorderSide(color: royalGold, width: 2),
      ),
    ),
  );

  /// Legacy alias — the "Midnight Gold" branding name used in tests and docs.
  static ThemeData get midnightTheme => darkTheme;
}
