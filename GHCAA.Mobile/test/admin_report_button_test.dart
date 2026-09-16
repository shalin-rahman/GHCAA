import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:ghcaa_mobile/core/api/api_client.dart';
import 'package:ghcaa_mobile/core/bootstrap/error_handlers.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:shared_preferences/shared_preferences.dart';

// 12.5: on an unhandled error, the fallback UI's "SEND REPORT TO ADMINISTRATOR"
// button posts an error report to the admin /contact inbox. This exercises
// that button in isolation, without going through the global ErrorWidget.builder.
class _ContactAdapter implements HttpClientAdapter {
  final List<RequestOptions> requests = [];
  bool fail = false;

  @override
  void close({bool force = false}) {}

  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<Uint8List>? requestStream,
    Future<void>? cancelFuture,
  ) async {
    requests.add(options);
    if (fail) {
      return ResponseBody.fromString('{}', 500, headers: {
        Headers.contentTypeHeader: ['application/json'],
      });
    }
    return ResponseBody.fromString(jsonEncode({'Message': 'ok'}), 200, headers: {
      Headers.contentTypeHeader: ['application/json'],
    });
  }
}

void main() {
  late ProviderContainer container;
  late _ContactAdapter adapter;

  setUp(() {
    dotenv.testLoad(fileInput: 'BASE_API_URL=http://localhost:5087/api');
    SharedPreferences.setMockInitialValues({});
    TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger
        .setMockMethodCallHandler(
      const MethodChannel('plugins.it_nomads.com/flutter_secure_storage'),
      (call) async {
        if (call.method == 'read') return null; // no session token stored
        return null;
      },
    );
    // Fixed profile so the button doesn't trigger its own background /profile
    // fetch through the shared adapter and skew the request count.
    container = ProviderContainer(overrides: [
      userProfileProvider.overrideWith((ref) async => {'fullName': 'Alum Test', 'email': 'alum@test.com'}),
    ]);
    adapter = _ContactAdapter();
    container.read(dioProvider).httpClientAdapter = adapter;
  });

  tearDown(() => container.dispose());

  Future<void> pumpButton(WidgetTester tester) async {
    await tester.pumpWidget(
      UncontrolledProviderScope(
        container: container,
        child: const MaterialApp(
          home: Scaffold(
            body: AdminReportButton(exception: 'boom'),
          ),
        ),
      ),
    );
  }

  testWidgets('posts an error report to /contact and shows sent state', (tester) async {
    await pumpButton(tester);

    expect(find.text('SEND REPORT TO ADMINISTRATOR'), findsOneWidget);

    await tester.tap(find.byType(OutlinedButton));
    await tester.pumpAndSettle();

    expect(adapter.requests, hasLength(1));
    expect(adapter.requests.single.path, contains('/contact'));
    final body = adapter.requests.single.data as Map;
    expect(body['subject'], 'Mobile app error report');
    expect(body['message'], contains('boom'));
    expect(find.text('REPORT SENT'), findsOneWidget);
  });

  testWidgets('shows a retry state when the request fails', (tester) async {
    adapter.fail = true;
    await pumpButton(tester);

    await tester.tap(find.byType(OutlinedButton));

    // RetryInterceptor (8.6) retries a 500 up to 3 times with backoff
    // (~3.5s total), so one pumpAndSettle() call settles on "no new frames"
    // before the retries finish. Poll instead.
    for (var i = 0; i < 30 && find.text('FAILED — TAP TO RETRY').evaluate().isEmpty; i++) {
      await tester.pump(const Duration(milliseconds: 200));
    }

    expect(find.text('FAILED — TAP TO RETRY'), findsOneWidget);
  });
}
