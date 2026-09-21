import 'dart:convert';
import 'dart:io';

import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:ghcaa_mobile/core/api/api_client.dart';
import 'package:ghcaa_mobile/core/bootstrap/error_handlers.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:path_provider_platform_interface/path_provider_platform_interface.dart';
import 'package:shared_preferences/shared_preferences.dart';

// 12.3: _send() now reads the rotating log tail before posting the report, so
// path_provider needs a fake here too (same pattern as log_capture_service_test.dart)
// or the platform channel call never resolves within the test's pump cycles.
class _FakeDocsDirPathProvider extends PathProviderPlatform {
  final Directory docsDir;
  _FakeDocsDirPathProvider(this.docsDir);

  @override
  Future<String?> getApplicationDocumentsPath() async => docsDir.path;
}

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
  late Directory docsDir;

  setUp(() async {
    dotenv.testLoad(fileInput: 'BASE_API_URL=http://localhost:5087/api');
    SharedPreferences.setMockInitialValues({});
    docsDir = await Directory.systemTemp.createTemp('ghcaa_admin_report_test');
    PathProviderPlatform.instance = _FakeDocsDirPathProvider(docsDir);
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

  tearDown(() async {
    container.dispose();
    if (await docsDir.exists()) await docsDir.delete(recursive: true);
  });

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

    // _send() now awaits LogCaptureService's real dart:io File calls (12.3),
    // which need runAsync() to complete — testWidgets otherwise runs in a
    // FakeAsync zone that never resolves genuine OS-level I/O. pumpAndSettle()
    // can also decide "settled" before that real I/O posts its next frame, so
    // poll for the end state instead of trusting a single pumpAndSettle().
    await tester.runAsync(() async {
      await tester.tap(find.byType(OutlinedButton));
      // pump(duration) inside runAsync doesn't itself block real time — the
      // fake clock it would normally advance is paused for this closure — so
      // the real wait has to come from Future.delayed, with a bare pump() after
      // each one just to sync the widget tree with the completed future.
      for (var i = 0; i < 30 && find.text('REPORT SENT').evaluate().isEmpty; i++) {
        await Future<void>.delayed(const Duration(milliseconds: 50));
        await tester.pump();
      }
    });

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

    await tester.runAsync(() async {
      await tester.tap(find.byType(OutlinedButton));

      // RetryInterceptor (8.6) retries a 500 up to 3 times with backoff
      // (~3.5s total), so one pumpAndSettle() call settles on "no new frames"
      // before the retries finish. Poll instead, with a long enough real-time
      // budget to cover all three backoff waits (see the comment on the first
      // test's identical loop for why the wait must come from Future.delayed).
      for (var i = 0; i < 60 && find.text('FAILED — TAP TO RETRY').evaluate().isEmpty; i++) {
        await Future<void>.delayed(const Duration(milliseconds: 150));
        await tester.pump();
      }
    });

    expect(find.text('FAILED — TAP TO RETRY'), findsOneWidget);
  });
}
