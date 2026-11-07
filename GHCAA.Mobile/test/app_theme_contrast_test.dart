import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/config/org_config.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';

Color _buttonLabel(ThemeData theme) =>
    theme.elevatedButtonTheme.style!.foregroundColor!.resolve({})!;

Color _buttonFill(ThemeData theme) =>
    theme.elevatedButtonTheme.style!.backgroundColor!.resolve({})!;

ThemeData _themeWithAccent(String accent) => AppTheme.buildTheme(
    OrgBranding.fromJson({'primaryColor': '#121212', 'accentColor': accent}));

void main() {
  group('84.89 button label contrast', () {
    test('offline fallback accent gets a readable label', () {
      final theme = AppTheme.buildTheme(OrgConfig.offlineDefaults.branding);

      expect(
        AppTheme.contrastRatio(_buttonLabel(theme), _buttonFill(theme)),
        greaterThanOrEqualTo(AppTheme.minTextContrast),
      );
    });

    test('a light accent keeps the dark label', () {
      final theme = _themeWithAccent('#c5a059');

      expect(_buttonLabel(theme), AppTheme.obsidianBlack);
    });

    test('configured accents get a readable label without changing the accent', () {
      for (final accent in ['#2f6f4f', '#1a237e', '#7b1fa2', '#c62828', '#f4d36f', '#ffffff']) {
        final theme = _themeWithAccent(accent);
        final fill = _buttonFill(theme);

        expect(fill.toARGB32() & 0xFFFFFF,
            int.parse(accent.substring(1), radix: 16),
            reason: accent);
        expect(
          AppTheme.contrastRatio(_buttonLabel(theme), fill),
          greaterThanOrEqualTo(AppTheme.minTextContrast),
          reason: accent,
        );
      }
    });

    test('a malformed accent falls back and still reads', () {
      final theme = _themeWithAccent('not-a-color');

      expect(_buttonFill(theme), AppTheme.defaultProfileAccent);
      expect(
        AppTheme.contrastRatio(_buttonLabel(theme), _buttonFill(theme)),
        greaterThanOrEqualTo(AppTheme.minTextContrast),
      );
    });
  });
}
