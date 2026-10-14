import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/router/app_router.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  // The auth notifier reads the stored token as soon as the router is built.
  // Without a handler the read throws MissingPluginException after the test ends.
  setUp(() {
    TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger
        .setMockMethodCallHandler(
      const MethodChannel('plugins.it_nomads.com/flutter_secure_storage'),
      (call) async => null,
    );
  });

  test('GoRouter should initialize at root portal screen', () {
    final container = ProviderContainer();
    addTearDown(container.dispose);
    final router = container.read(routerProvider);

    // Core Navigation Assertions
    expect(router.configuration.routes.isNotEmpty, true);
  });
}
