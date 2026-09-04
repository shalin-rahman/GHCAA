// Shared setup/pump helpers for the golden (screenshot) test files.
//
// These were the same few lines copy-pasted at the top of every visual-freeze
// test's setUpAll/testGoldens blocks. Keep additions here limited to things
// every caller does identically — a mock or pump sequence a single file needs
// for its own reasons belongs in that file.
import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';

/// Two explicit frames instead of `pumpAndSettle`.
///
/// `pumpAndSettle` loops until nothing is scheduled, which never finishes (or
/// throws after a rendering overflow) on the animated/loading widgets these
/// screens show while their fake providers resolve. One frame plus a short
/// delay is enough for the widget tree and the fake Futures to settle.
Future<void> pumpAndSettleShort(WidgetTester tester) async {
  await tester.pump();
  await tester.pump(const Duration(milliseconds: 50));
}

/// Mocks the `local_auth` platform channel so screens that check biometric
/// availability (login, digital ID, dashboard) don't throw
/// `MissingPluginException` under `flutter test`.
void mockLocalAuthChannel() {
  TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger.setMockMethodCallHandler(
    const MethodChannel('plugins.flutter.io/local_auth'),
    (methodCall) async {
      if (methodCall.method == 'getAvailableBiometrics') return <String>[];
      if (methodCall.method == 'isDeviceSupported') return false;
      return null;
    },
  );
}
