import 'dart:io';

import 'package:flutter_test/flutter_test.dart';

void main() {
  // 7.16: guards against someone editing the release build steps in
  // mobile_deployment.yml and dropping the obfuscation flags without
  // noticing — there's no other gate that would catch that.
  test('release build steps in mobile_deployment.yml pass --obfuscate and --split-debug-info', () {
    final workflow = File('../.github/workflows/mobile_deployment.yml');
    expect(workflow.existsSync(), isTrue, reason: 'expected to find the mobile deployment workflow');

    final contents = workflow.readAsStringSync();
    final releaseBuildLines = contents
        .split('\n')
        .where((line) => line.contains('flutter build') && line.contains('--release'))
        .toList();

    expect(releaseBuildLines, isNotEmpty, reason: 'expected at least one release build command');
    for (final line in releaseBuildLines) {
      expect(line, contains('--obfuscate'), reason: 'release build missing --obfuscate: $line');
      expect(line, contains('--split-debug-info='), reason: 'release build missing --split-debug-info: $line');
    }
  });
}
