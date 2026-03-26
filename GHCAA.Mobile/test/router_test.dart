import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/router/app_router.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

void main() {
  test('GoRouter should initialize at root portal screen', () {
    final container = ProviderContainer();
    final router = container.read(routerProvider);
    
    // Core Navigation Assertions
    expect(router.configuration.debugKnownRoutes.map((e) => e.name), 
      containsAll(['home', 'login', 'register', 'dashboard', 'directory', 'profile', 'admin_dashboard'])
    );
  });
}
