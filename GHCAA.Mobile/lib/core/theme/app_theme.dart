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
 
  // Premium Gradients
  static const Gradient goldGradient = LinearGradient(
    colors: [royalGold, brightGold, royalGold],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );

  static const Gradient darkSurfaceGradient = LinearGradient(
    colors: [deepCharcoal, obsidianBlack],
    begin: Alignment.topCenter,
    end: Alignment.bottomCenter,
  );

  static final ThemeData darkTheme = ThemeData(
    useMaterial3: true,
    brightness: Brightness.dark,
    scaffoldBackgroundColor: obsidianBlack,
    dividerTheme: const DividerThemeData(color: glassBorder, thickness: 1),
    cardTheme: CardThemeData(
      color: deepCharcoal,
      elevation: 0,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(16),
        side: const BorderSide(color: glassBorder, width: 1),
      ),
      margin: const EdgeInsets.symmetric(vertical: 8, horizontal: 16),
    ),
    appBarTheme: const AppBarTheme(
      backgroundColor: Colors.transparent,
      elevation: 0,
      centerTitle: true,
      titleTextStyle: TextStyle(
        fontFamily: 'Outfit',
        fontSize: 18,
        fontWeight: FontWeight.w800,
        color: brightGold,
        letterSpacing: 1.2,
      ),
      iconTheme: IconThemeData(color: royalGold),
    ),
    colorScheme: const ColorScheme.dark(
      primary: royalGold,
      onPrimary: obsidianBlack,
      secondary: brightGold,
      surface: deepCharcoal,
      onSurface: textMain,
      surfaceContainerHighest: Color(0xFF161616),
      outline: glassBorder,
    ),
    textTheme: const TextTheme(
      displayLarge: TextStyle(
        fontFamily: 'Outfit',
        fontWeight: FontWeight.w800,
        color: textMain,
        letterSpacing: -0.02,
        fontSize: 32,
      ),
      headlineMedium: TextStyle(
        fontFamily: 'Outfit',
        fontWeight: FontWeight.w700,
        color: brightGold,
        fontSize: 24,
      ),
      titleLarge: TextStyle(
        fontFamily: 'Outfit',
        fontWeight: FontWeight.w600,
        color: textMain,
        fontSize: 18,
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
      labelLarge: TextStyle(
        fontFamily: 'Outfit',
        fontWeight: FontWeight.w800,
        color: royalGold,
        fontSize: 12,
        letterSpacing: 1.2,
      ),
    ),
    elevatedButtonTheme: ElevatedButtonThemeData(
      style: ElevatedButton.styleFrom(
        backgroundColor: royalGold,
        foregroundColor: obsidianBlack,
        textStyle: const TextStyle(
          fontFamily: 'Outfit',
          fontWeight: FontWeight.w800,
          letterSpacing: 1.2,
        ),
        padding: const EdgeInsets.symmetric(vertical: 18, horizontal: 32),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(12),
        ),
        elevation: 12,
        shadowColor: royalGold.withValues(alpha: 0.5),
      ),
    ),
    inputDecorationTheme: InputDecorationTheme(
      filled: true,
      fillColor: deepCharcoal,
      labelStyle: const TextStyle(color: textMuted, fontFamily: 'Outfit'),
      hintStyle: const TextStyle(color: Colors.white24, fontFamily: 'Outfit'),
      contentPadding: const EdgeInsets.all(20),
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: glassBorder),
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: glassBorder),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: royalGold, width: 2),
      ),
      errorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: Colors.redAccent),
      ),
    ),
  );

  /// Legacy alias — the "Midnight Gold" branding name used in tests and docs.
  static ThemeData get midnightTheme => darkTheme;
}
