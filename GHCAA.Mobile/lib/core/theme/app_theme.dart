import 'package:flutter/material.dart';

class AppTheme {
  // Core Brand Palette - "Obsidian & Gold Leaf"
  static const Color obsidianBlack = Color(0xFF0B0E14);
  static const Color deepCharcoal = Color(0xFF161B22);
  static const Color royalGold = Color(0xFFD4AF37);
  static const Color brightGold = Color(0xFFFFD700);
  static const Color darkGold = Color(0xFF996515);
  
  static const Color textMain = Color(0xFFFFFFFF);
  static const Color textMuted = Color(0xFFA0AEC0);
  
  static const Color glassBorder = Color(0x33D4AF37); // Royal Gold with low opacity
  static const Color shadowColor = Color(0x66000000); 


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
 
  // Design Tokens - 8pt Grid System
  static const double spaceXS = 4.0;
  static const double spaceS = 8.0;
  static const double spaceM = 16.0;
  static const double spaceL = 24.0;
  static const double spaceXL = 32.0;
  static const double spaceXXL = 48.0;
  static const double spaceHUGE = 64.0;

  // Border Radius Tokens
  static const double radiusXS = 4.0;
  static const double radiusS = 8.0;
  static const double radiusM = 12.0;
  static const double radiusL = 16.0;
  static const double radiusXL = 24.0;

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
        borderRadius: BorderRadius.circular(radiusL),
        side: const BorderSide(color: glassBorder, width: 1),
      ),
      margin: const EdgeInsets.symmetric(vertical: spaceS, horizontal: spaceM),
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
        padding: const EdgeInsets.symmetric(vertical: spaceM + 2, horizontal: spaceXL),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(radiusM),
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
      contentPadding: const EdgeInsets.all(spaceM),
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(radiusM),
        borderSide: const BorderSide(color: glassBorder),
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(radiusM),
        borderSide: const BorderSide(color: glassBorder),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(radiusM),
        borderSide: const BorderSide(color: royalGold, width: 2),
      ),
      errorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(radiusM),
        borderSide: const BorderSide(color: Colors.redAccent),
      ),
    ),
  );

  /// Legacy alias — the "Midnight Gold" branding name used in tests and docs.
  static ThemeData get midnightTheme => darkTheme;
}
