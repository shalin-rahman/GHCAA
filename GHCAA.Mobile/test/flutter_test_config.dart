import 'dart:async';
import 'dart:io' show Platform;

import 'package:golden_toolkit/golden_toolkit.dart';

/// Global test configuration, auto-loaded by `flutter test` for everything
/// under `test/`.
///
/// Golden (screenshot) tests compare the rendered widget tree against committed
/// PNGs. Those PNGs are pixel-exact to the OS/font stack they were generated on.
/// On CI (Linux) the same widgets anti-alias fonts slightly differently, so an
/// exact comparison reports ~1% diffs and fails even when nothing is broken.
///
/// We therefore skip the *pixel assertion* on CI only. The widgets are still
/// pumped and built, so genuine failures — exceptions, render overflows, layout
/// errors — are still caught. Locally (where the goldens are valid) the pixel
/// comparison runs normally, keeping the visual-freeze tests useful for review.
Future<void> testExecutable(FutureOr<void> Function() testMain) {
  final bool isCi = Platform.environment.containsKey('CI') ||
      Platform.environment.containsKey('GITHUB_ACTIONS');

  return GoldenToolkit.runWithConfiguration(
    () async => testMain(),
    config: GoldenToolkitConfiguration(
      skipGoldenAssertion: () => isCi,
    ),
  );
}
