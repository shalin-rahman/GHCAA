import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/router/app_router.dart';
import 'package:go_router/go_router.dart';

/// Every route in the tree, including the ones nested under the [ShellRoute].
Iterable<GoRoute> _allRoutes(List<RouteBase> routes) sync* {
  for (final route in routes) {
    if (route is GoRoute) yield route;
    yield* _allRoutes(route.routes);
  }
}

void main() {
  late ProviderContainer container;
  late List<GoRoute> routes;

  setUp(() {
    container = ProviderContainer();
    addTearDown(container.dispose);
    routes = _allRoutes(container.read(routerProvider).configuration.routes).toList();
  });

  test('every registered route has a unique path and a name', () {
    final paths = routes.map((r) => r.path).toList();
    final names = routes.map((r) => r.name).toList();

    expect(paths.toSet().length, paths.length, reason: 'duplicate route path registered');
    expect(names, isNot(contains(null)), reason: 'every route is navigated to by name');
    expect(names.toSet().length, names.length, reason: 'duplicate route name registered');
  });

  test('the member routes are registered', () {
    expect(routes.map((r) => r.path), containsAll(<String>[
      '/',
      '/login',
      '/register',
      '/dashboard',
      '/directory',
      '/directory/:id',
      '/digital_id',
      '/profile',
      '/profile/edit',
      '/events',
      '/events/:id',
      '/jobs',
      '/jobs/:id',
      '/financials',
      '/financials/payment',
      '/news',
      '/news/:id',
      '/gallery',
      '/committee',
      '/notifications',
      '/activity',
      '/family',
      '/support',
      '/articles',
      '/submit_article',
      '/magazine',
      '/about',
      '/mentorship',
      '/professionals',
      '/polls',
      '/chats',
      '/chat/:id',
      '/assistant',
    ]));
  });

  test('the forum routes are registered', () {
    expect(routes.map((r) => r.path), containsAll(<String>[
      '/forum',
      '/forum/topics/:id',
      '/forum/topic/:id',
    ]));
  });

  test('the admin routes are registered', () {
    expect(routes.map((r) => r.path), containsAll(<String>[
      '/admin_dashboard',
      '/admin/approvals',
      '/admin/audit',
      '/admin/ledger',
      '/admin/themes',
      '/admin/fees',
      '/admin/messages',
      '/admin/gatekeeper',
      '/admin/governance',
      '/admin/articles',
      '/admin/permissions',
      '/admin/cms',
    ]));
  });
}
