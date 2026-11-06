import 'dart:io';

import 'package:flutter_test/flutter_test.dart';

// Shown dates go through AppUtils.formatDate or displayAppDate so they follow dd-MM-yyyy or
// the org's setting. Ten screens once cut ISO strings with split('T')[0] and showed
// yyyy-MM-dd, so this test fails if that or a direct DateFormat comes back.
void main() {
  test('screens format dates only through the shared helpers', () {
    final rawSplit = RegExp(r'''split\(\s*['"]T['"]\s*\)\s*\[\s*0\s*\]''');
    // substring(0, 10) is the same cut of an ISO string.
    final rawCut = RegExp(r'substring\(\s*0\s*,\s*10\s*\)');
    final rawFormat = RegExp(r'\bDateFormat\(');
    final found = <String>[];

    for (final entity in Directory('lib').listSync(recursive: true)) {
      if (entity is! File || !entity.path.endsWith('.dart')) continue;
      final path = entity.path.replaceAll('\\', '/');
      if (path.endsWith('core/utils/app_utils.dart')) continue;
      final lines = entity.readAsLinesSync();
      for (var i = 0; i < lines.length; i++) {
        if (rawSplit.hasMatch(lines[i]) || rawCut.hasMatch(lines[i]) || rawFormat.hasMatch(lines[i])) {
          found.add('$path:${i + 1}');
        }
      }
    }

    expect(found, isEmpty);
  });
}
