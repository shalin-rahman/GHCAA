import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/features/theme/dynamic_theme_service.dart';

Map<String, dynamic> _json({Object? bg, Object? text, Object? sidebar}) => {
      'id': 1,
      'title': 'Victory Day',
      'backgroundColor': bg,
      'textColor': text,
      'sidebarColor': sidebar,
    };

void main() {
  group('84.91 SpecialTheme colors', () {
    test('reads 3, 6 and 8 digit colors', () {
      final t = SpecialTheme.fromJson(_json(bg: '#060', text: ' #ffffff ', sidebar: '#80123456'));

      expect(t.backgroundColor, const Color(0xFF006600));
      expect(t.textColor, const Color(0xFFFFFFFF));
      expect(t.sidebarColor, const Color(0x80123456));
    });

    test('a bad background falls back to gold with a readable text default', () {
      final t = SpecialTheme.fromJson(_json(bg: 'green', text: null));

      expect(t.backgroundColor, AppTheme.royalGold);
      expect(t.textColor, AppTheme.readableTextOn(AppTheme.royalGold));
    });

    test('a bad text color reads on a dark background', () {
      final t = SpecialTheme.fromJson(_json(bg: '#000080', text: '#12'));

      expect(t.textColor, Colors.white);
    });

    test('an empty or bad sidebar color becomes none', () {
      expect(SpecialTheme.fromJson(_json(bg: '#000', text: '#fff', sidebar: '')).sidebarColor, isNull);
      expect(SpecialTheme.fromJson(_json(bg: '#000', text: '#fff', sidebar: 'xyz')).sidebarColor, isNull);
      expect(SpecialTheme.fromJson(_json(bg: '#000', text: '#fff')).sidebarColor, isNull);
    });
  });
}
