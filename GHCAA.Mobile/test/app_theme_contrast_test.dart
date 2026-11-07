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

  group('84.91 color scheme labels', () {
    ThemeData themeWith(String primary, String accent) => AppTheme.buildTheme(
        OrgBranding.fromJson({'primaryColor': primary, 'accentColor': accent}));

    test('dark primary and accent get white labels', () {
      final scheme = themeWith('#1a237e', '#2f6f4f').colorScheme;

      expect(scheme.onPrimary, Colors.white);
      expect(scheme.onSecondary, Colors.white);
    });

    test('light primary and accent keep the dark label', () {
      final scheme = themeWith('#c5a059', '#f4d36f').colorScheme;

      expect(scheme.onPrimary, AppTheme.obsidianBlack);
      expect(scheme.onSecondary, AppTheme.obsidianBlack);
    });

    test('labels read on every configured color', () {
      for (final c in ['#121212', '#2f6f4f', '#7b1fa2', '#c62828', '#c5a059', '#ffffff']) {
        final scheme = themeWith(c, c).colorScheme;
        expect(AppTheme.contrastRatio(scheme.onPrimary, scheme.primary),
            greaterThanOrEqualTo(AppTheme.minTextContrast), reason: c);
        expect(AppTheme.contrastRatio(scheme.onSecondary, scheme.secondary),
            greaterThanOrEqualTo(AppTheme.minTextContrast), reason: c);
      }
    });
  });

  group('84.91 hex parsing', () {
    const fallback = Color(0xFF010203);

    test('accepts 3, 6 and 8 digits, with or without # and spaces', () {
      expect(AppTheme.parseHexColor('#abc', fallback), const Color(0xFFAABBCC));
      expect(AppTheme.parseHexColor('abc', fallback), const Color(0xFFAABBCC));
      expect(AppTheme.parseHexColor('#C5A059', fallback), const Color(0xFFC5A059));
      expect(AppTheme.parseHexColor('  #c5a059  ', fallback), const Color(0xFFC5A059));
      expect(AppTheme.parseHexColor('#80c5a059', fallback), const Color(0x80C5A059));
    });

    test('anything else returns the fallback', () {
      for (final bad in [null, '', '#', '#ab', '#abcd', '#abcde', '#abcdefa', '#abcdefabc', '#ggg', 'red', '#12 345']) {
        expect(AppTheme.parseHexColor(bad, fallback), fallback, reason: '$bad');
        expect(AppTheme.tryParseHexColor(bad), isNull, reason: '$bad');
      }
    });

    test('a 3-digit accent builds the same theme as its 6-digit form', () {
      expect(_accentFill('#fff'), _accentFill('#ffffff'));
    });
  });
}

Color _accentFill(String accent) => _buttonFill(_themeWithAccent(accent));
