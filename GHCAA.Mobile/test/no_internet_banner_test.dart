import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/services/connectivity_service.dart';
import 'package:ghcaa_mobile/core/widgets/no_internet_banner.dart';

void main() {
  Future<void> pumpBanner(WidgetTester tester, bool online) async {
    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          isOnlineProvider.overrideWithValue(online),
        ],
        child: const MaterialApp(
          home: Scaffold(body: NoInternetBanner()),
        ),
      ),
    );
    await tester.pumpAndSettle();
  }

  testWidgets('is hidden (transparent, off-screen) while online', (tester) async {
    await pumpBanner(tester, true);

    final opacity = tester.widget<AnimatedOpacity>(find.byType(AnimatedOpacity));
    final slide = tester.widget<AnimatedSlide>(find.byType(AnimatedSlide));

    expect(opacity.opacity, 0.0);
    expect(slide.offset, const Offset(0, -1));
  });

  testWidgets('shows the offline message when connectivity drops', (tester) async {
    await pumpBanner(tester, false);

    final opacity = tester.widget<AnimatedOpacity>(find.byType(AnimatedOpacity));
    final slide = tester.widget<AnimatedSlide>(find.byType(AnimatedSlide));

    expect(opacity.opacity, 1.0);
    expect(slide.offset, Offset.zero);
    expect(find.textContaining('No Internet Connection'), findsOneWidget);
  });

  testWidgets('reacts to connectivity changes after mount', (tester) async {
    // isOnlineProvider derives from connectivityStreamProvider's AsyncValue,
    // so drive it through a controller we can push events on directly.
    final controller = StreamController<bool>();
    addTearDown(controller.close);

    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          connectivityStreamProvider.overrideWith((ref) => controller.stream),
        ],
        child: const MaterialApp(
          home: Scaffold(body: NoInternetBanner()),
        ),
      ),
    );
    await tester.pump();

    // Before any event, isOnlineProvider defaults to true (loading state).
    var opacity = tester.widget<AnimatedOpacity>(find.byType(AnimatedOpacity));
    expect(opacity.opacity, 0.0);

    controller.add(false);
    await tester.pumpAndSettle();
    opacity = tester.widget<AnimatedOpacity>(find.byType(AnimatedOpacity));
    expect(opacity.opacity, 1.0);

    controller.add(true);
    await tester.pumpAndSettle();
    opacity = tester.widget<AnimatedOpacity>(find.byType(AnimatedOpacity));
    expect(opacity.opacity, 0.0);
  });
}
