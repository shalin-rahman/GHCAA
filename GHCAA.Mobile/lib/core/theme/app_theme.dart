import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

class AppTheme {
  // Ultra-Premium Core
  static const Color royalGold = Color(0xFFD4AF37);
  static const Color royalGoldVibrant = Color(0xFFFFD700);
  static const Color royalGoldMuted = Color(0xFFB3922D);

  // Midnight Palette (Deeper & High Contrast)
  static const Color midnightBase = Color(0xFF020617); 
  static const Color midnightSurface = Color(0xFF0F172A);
  static const Color textPrimaryDark = Color(0xFFF8FAFC);
  static const Color textSecondaryDark = Color(0xFF94A3B8);

  // Daylight Palette
  static const Color daylightBase = Color(0xFFF1F5F9);
  static const Color daylightSurface = Color(0xFFFFFFFF);
  static const Color textPrimaryLight = Color(0xFF0F172A);
  static const Color textSecondaryLight = Color(0xFF475569);

  // Admin Context Palette (Imperial Purple & Rich Gold)
  static const Color adminMidnightBase = Color(0xFF0F071A);
  static const Color adminMidnightSurface = Color(0xFF1E0E33);
  static const Color adminDaylightBase = Color(0xFFFAF5FF);
  static const Color adminDaylightSurface = Color(0xFFFDFBFF);


  static ThemeData get darkTheme {
    return ThemeData.dark().copyWith(
      useMaterial3: true,
      scaffoldBackgroundColor: midnightBase,
      primaryColor: royalGold,
      colorScheme: const ColorScheme.dark(
        primary: royalGold,
        secondary: royalGoldVibrant,
        surface: midnightSurface,
        onSurface: textPrimaryDark,
      ),
      textTheme: GoogleFonts.outfitTextTheme(ThemeData.dark().textTheme).copyWith(
        displayLarge: GoogleFonts.outfit(fontSize: 32, fontWeight: FontWeight.bold, color: royalGold),
        displayMedium: GoogleFonts.outfit(fontSize: 24, fontWeight: FontWeight.w600, color: textPrimaryDark),
        bodyLarge: GoogleFonts.outfit(fontSize: 16, color: textPrimaryDark),
        bodyMedium: GoogleFonts.outfit(fontSize: 14, color: textSecondaryDark),
      ),
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          backgroundColor: royalGold,
          foregroundColor: Colors.black,
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 16),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
          elevation: 8,
          textStyle: const TextStyle(fontWeight: FontWeight.bold),
        ),
      ),
      listTileTheme: const ListTileThemeData(
        textColor: textPrimaryDark,
        iconColor: textSecondaryDark,
        subtitleTextStyle: TextStyle(color: textSecondaryDark, fontSize: 13),
        titleTextStyle: TextStyle(color: textPrimaryDark, fontWeight: FontWeight.w600, fontSize: 15),
      ),
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: Colors.white.withOpacity(0.05),
        hintStyle: const TextStyle(color: textSecondaryDark),
        labelStyle: const TextStyle(color: textSecondaryDark),
        border: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: BorderSide(color: Colors.white12)),
        enabledBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: const BorderSide(color: Colors.white12)),
        focusedBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: const BorderSide(color: royalGold)),
      ),
      chipTheme: ChipThemeData(
        backgroundColor: Colors.white.withOpacity(0.08),
        labelStyle: const TextStyle(color: textPrimaryDark, fontSize: 12),
      ),
      dividerColor: Colors.white12,

    );
  }

  static ThemeData get lightTheme {
    return ThemeData.light().copyWith(
      useMaterial3: true,
      scaffoldBackgroundColor: daylightBase,
      primaryColor: royalGold,
      colorScheme: const ColorScheme.light(
        primary: royalGold,
        secondary: royalGoldMuted,
        surface: daylightSurface,
        onSurface: textPrimaryLight,
      ),
      textTheme: GoogleFonts.outfitTextTheme(ThemeData.light().textTheme).copyWith(
        displayLarge: GoogleFonts.outfit(fontSize: 32, fontWeight: FontWeight.bold, color: royalGold),
        displayMedium: GoogleFonts.outfit(fontSize: 24, fontWeight: FontWeight.w600, color: textPrimaryLight),
        bodyLarge: GoogleFonts.outfit(fontSize: 16, color: textPrimaryLight),
        bodyMedium: GoogleFonts.outfit(fontSize: 14, color: textSecondaryLight),
      ),
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          backgroundColor: royalGold,
          foregroundColor: Colors.white,
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 16),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
          elevation: 4,
          textStyle: const TextStyle(fontWeight: FontWeight.bold),
        ),
      ),
    );
  }
}
