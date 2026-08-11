import 'dart:async';
import 'dart:io' show Platform;

import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:golden_toolkit/golden_toolkit.dart';
import 'package:shared_preferences/shared_preferences.dart';

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

  // Ensure the test binding exists before registering mock channel handlers.
  TestWidgetsFlutterBinding.ensureInitialized();

  // Native-only plugins have no implementation under `flutter test`, so screens
  // that call them throw MissingPluginException before they can render. Mock
  // them centrally here (once per test file) so every visual-freeze test can
  // pump these screens instead of each file re-declaring the same handlers.

  // shared_preferences (`getAll` on load) — start from an empty store.
  SharedPreferences.setMockInitialValues(<String, Object>{});

  // screen_protector — sensitive screens (Digital ID, Payment/Financial portal)
  // call preventScreenshotOn()/allowScreenshotOn() in initState; no-op them.
  TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger
      .setMockMethodCallHandler(
    const MethodChannel('screen_protector'),
    (methodCall) async => null,
  );

  return GoldenToolkit.runWithConfiguration(
    () async => testMain(),
    config: GoldenToolkitConfiguration(
      skipGoldenAssertion: () => isCi,
    ),
  );
}
