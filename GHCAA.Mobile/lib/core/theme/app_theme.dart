import 'package:flutter/material.dart';
import '../config/org_config.dart';

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

  static final ThemeData darkTheme = buildTheme(OrgConfig.ghcaaDefaults.branding);

  /// Legacy alias — the "Midnight Gold" branding name used in tests and docs.
  static ThemeData get midnightTheme => darkTheme;

  /// Parses a "#RRGGBB" (or "#AARRGGBB") hex string into a [Color].
  /// Falls back to [fallback] on any parse failure so a malformed tenant
  /// config value never crashes theme construction.
  static Color _colorFromHex(String hex, Color fallback) {
    try {
      var value = hex.replaceFirst('#', '');
      if (value.length == 6) value = 'FF$value';
      return Color(int.parse(value, radix: 16));
    } catch (_) {
      return fallback;
    }
  }

  /// Builds the app [ThemeData] from tenant [OrgBranding], so white-labeled
  /// orgs get their configured primary/accent colors applied to
  /// theme-driven surfaces (buttons, app bar, inputs, color scheme).
  static ThemeData buildTheme(OrgBranding branding) {
    final primary = _colorFromHex(branding.primaryColor, royalGold);
    final secondary = _colorFromHex(branding.accentColor, brightGold);

    return ThemeData(
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
      appBarTheme: AppBarTheme(
        backgroundColor: Colors.transparent,
        elevation: 0,
        centerTitle: true,
        titleTextStyle: TextStyle(
          fontFamily: 'Outfit',
          fontSize: 18,
          fontWeight: FontWeight.w800,
          color: secondary,
          letterSpacing: 1.2,
        ),
        iconTheme: IconThemeData(color: primary),
      ),
      colorScheme: ColorScheme.dark(
        primary: primary,
        onPrimary: obsidianBlack,
        secondary: secondary,
        surface: deepCharcoal,
        onSurface: textMain,
        surfaceContainerHighest: const Color(0xFF161616),
        outline: glassBorder,
      ),
      textTheme: TextTheme(
        displayLarge: const TextStyle(
          fontFamily: 'Outfit',
          fontWeight: FontWeight.w800,
          color: textMain,
          letterSpacing: -0.02,
          fontSize: 32,
        ),
        headlineMedium: TextStyle(
          fontFamily: 'Outfit',
          fontWeight: FontWeight.w700,
          color: secondary,
          fontSize: 24,
        ),
        titleLarge: const TextStyle(
          fontFamily: 'Outfit',
          fontWeight: FontWeight.w600,
          color: textMain,
          fontSize: 18,
        ),
        bodyLarge: const TextStyle(
          fontFamily: 'Outfit',
          color: textMain,
          fontSize: 16,
        ),
        bodyMedium: const TextStyle(
          fontFamily: 'Outfit',
          color: textMuted,
          fontSize: 14,
        ),
        labelLarge: TextStyle(
          fontFamily: 'Outfit',
          fontWeight: FontWeight.w800,
          color: primary,
          fontSize: 12,
          letterSpacing: 1.2,
        ),
      ),
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          backgroundColor: primary,
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
          shadowColor: primary.withValues(alpha: 0.5),
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
          borderSide: BorderSide(color: primary, width: 2),
        ),
        errorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(radiusM),
          borderSide: const BorderSide(color: Colors.redAccent),
        ),
      ),
    );
  }
}
