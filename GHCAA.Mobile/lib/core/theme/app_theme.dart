import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

class AppTheme {
  // --- Core Brand Tokens ---
  static const Color royalGold = Color(0xFFD4AF37);
  static const Color royalGoldVibrant = Color(0xFFFFD700);
  static const Color royalGoldMuted = Color(0xFFB3922D);

  // --- Midnight Palette ---
  static const Color midnightBase = Color(0xFF020617);
  static const Color midnightSurface = Color(0xFF0F172A);
  static const Color textPrimaryDark = Color(0xFFF8FAFC);
  static const Color textSecondaryDark = Color(0xFF94A3B8);

  // --- Admin Palette ---
  static const Color adminMidnightBase = Color(0xFF0F071A);
  static const Color adminMidnightSurface = Color(0xFF1E0E33);
  static const Color adminDaylightBase = Color(0xFFFAF5FF);
  static const Color adminDaylightSurface = Color(0xFFFDFBFF);

  // --- Shared Text Style Defaults ---
  // Used in any widget that renders on our dark background.
  static const TextStyle bodyOnDark = TextStyle(color: textPrimaryDark);
  static const TextStyle subtitleOnDark = TextStyle(color: textSecondaryDark, fontSize: 13);
  static const TextStyle labelGold = TextStyle(color: royalGold, fontWeight: FontWeight.bold, letterSpacing: 1.1);

  // -----------------------------------------------------------------------
  //  DARK THEME  (Primary — the entire GHCAA design is dark)
  // -----------------------------------------------------------------------
  static ThemeData get darkTheme {
    final base = ThemeData.dark(useMaterial3: true);
    return base.copyWith(
      scaffoldBackgroundColor: midnightBase,
      primaryColor: royalGold,
      colorScheme: const ColorScheme.dark(
        primary: royalGold,
        secondary: royalGoldVibrant,
        surface: midnightSurface,
        onSurface: textPrimaryDark,
        onPrimary: Colors.black,
        onSecondary: Colors.black,
        error: Color(0xFFFF6B6B),
        onError: Colors.white,
      ),
      // All text defaults to white on dark canvas
      textTheme: GoogleFonts.outfitTextTheme(base.textTheme).apply(
        bodyColor: textPrimaryDark,
        displayColor: textPrimaryDark,
      ).copyWith(
        displayLarge: GoogleFonts.outfit(fontSize: 32, fontWeight: FontWeight.bold, color: royalGold),
        displayMedium: GoogleFonts.outfit(fontSize: 24, fontWeight: FontWeight.w600, color: textPrimaryDark),
        bodyLarge: GoogleFonts.outfit(fontSize: 16, color: textPrimaryDark),
        bodyMedium: GoogleFonts.outfit(fontSize: 14, color: textSecondaryDark),
        labelSmall: GoogleFonts.outfit(fontSize: 10, color: textSecondaryDark, letterSpacing: 1.2),
      ),
      // ListTile — used across all screens
      listTileTheme: const ListTileThemeData(
        textColor: textPrimaryDark,
        iconColor: textSecondaryDark,
        titleTextStyle: TextStyle(color: textPrimaryDark, fontWeight: FontWeight.w600, fontSize: 15),
        subtitleTextStyle: TextStyle(color: textSecondaryDark, fontSize: 13),
        contentPadding: EdgeInsets.symmetric(horizontal: 16, vertical: 4),
      ),
      // TextField / FormField
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: Colors.white.withOpacity(0.06),
        hintStyle: const TextStyle(color: textSecondaryDark),
        labelStyle: const TextStyle(color: textSecondaryDark),
        prefixIconColor: royalGold,
        suffixIconColor: textSecondaryDark,
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: Colors.white12),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: Colors.white12),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: royalGold, width: 1.5),
        ),
        errorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: Color(0xFFFF6B6B)),
        ),
      ),
      // Buttons
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          backgroundColor: royalGold,
          foregroundColor: Colors.black,
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 16),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
          elevation: 8,
          textStyle: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15),
        ),
      ),
      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          foregroundColor: royalGold,
          side: const BorderSide(color: royalGold),
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 16),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
          textStyle: const TextStyle(fontWeight: FontWeight.w600, fontSize: 15),
        ),
      ),
      textButtonTheme: TextButtonThemeData(
        style: TextButton.styleFrom(foregroundColor: royalGold),
      ),
      // Switch / Toggle
      switchTheme: SwitchThemeData(
        thumbColor: WidgetStateProperty.resolveWith((s) => s.contains(WidgetState.selected) ? royalGold : textSecondaryDark),
        trackColor: WidgetStateProperty.resolveWith((s) => s.contains(WidgetState.selected) ? royalGold.withOpacity(0.3) : Colors.white12),
      ),
      // Chips
      chipTheme: ChipThemeData(
        backgroundColor: Colors.white.withOpacity(0.08),
        labelStyle: const TextStyle(color: textPrimaryDark, fontSize: 12),
        side: const BorderSide(color: Colors.white12),
      ),
      // Snackbar
      snackBarTheme: SnackBarThemeData(
        backgroundColor: midnightSurface,
        contentTextStyle: const TextStyle(color: textPrimaryDark),
        behavior: SnackBarBehavior.floating,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(12),
          side: const BorderSide(color: Colors.white12),
        ),
      ),
      // Misc
      dividerColor: Colors.white12,
      iconTheme: const IconThemeData(color: textSecondaryDark),
      progressIndicatorTheme: const ProgressIndicatorThemeData(color: royalGold),
    );
  }

  // -----------------------------------------------------------------------
  //  LIGHT THEME  (mirror dark but with light surfaces — kept consistent)
  // -----------------------------------------------------------------------
  static ThemeData get lightTheme => darkTheme; // Mirror dark; GHCAA is always Midnight Gold
  
  // Alias for midnightTheme (used in widget tests)
  static ThemeData get midnightTheme => darkTheme;
}
