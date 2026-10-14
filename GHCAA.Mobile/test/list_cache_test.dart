import 'dart:convert';
import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/api/api_client.dart';
import 'package:ghcaa_mobile/core/api/session_caches.dart';
import 'package:ghcaa_mobile/core/constants/app_constants.dart';
import 'package:ghcaa_mobile/core/utils/ttl_cache.dart';
import 'package:ghcaa_mobile/core/widgets/custom_network_image.dart';
import 'package:ghcaa_mobile/features/events/events_service.dart';
import 'package:ghcaa_mobile/features/jobs/job_service.dart';
import 'package:ghcaa_mobile/features/lookups/lookup_service.dart';

class _CountingAdapter implements HttpClientAdapter {
  final requests = <String>[];
  final queries = <Map<String, dynamic>>[];

  int count(String method, String path) =>
      requests.where((r) => r == '$method $path').length;

  @override
  void close({bool force = false}) {}

  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<Uint8List>? requestStream,
    Future<void>? cancelFuture,
  ) async {
    requests.add('${options.method} ${options.path}');
    queries.add(options.queryParameters);
    final isPaged = options.method == 'GET' &&
        (options.path == '/events' || options.path == '/jobs');
    final isList =
        options.method == 'GET' && options.path.startsWith('/lookups/');
    final Object body = options.path == '/lookups'
        ? {'Sector': []}
        : isPaged
            ? {
                'items': [
                  {'id': options.queryParameters['page']}
                ],
                'page': options.queryParameters['page'],
                'totalPages': 3,
              }
            : isList
                ? [
                    {'id': 1}
                  ]
                : {};
    return ResponseBody.fromString(
      jsonEncode(body),
      200,
      headers: {
        Headers.contentTypeHeader: ['application/json'],
      },
    );
  }
}

void main() {
  group('TtlCache', () {
    test('returns a stored value inside the window', () {
      var now = DateTime(2026, 1, 1);
      final cache =
          TtlCache<String, int>(const Duration(minutes: 5), now: () => now);
      cache.set('a', 1);
      now = now.add(const Duration(minutes: 4, seconds: 59));
      expect(cache.get('a'), 1);
    });

    test('drops a value once the window has passed', () {
      var now = DateTime(2026, 1, 1);
      final cache =
          TtlCache<String, int>(const Duration(minutes: 5), now: () => now);
      cache.set('a', 1);
      now = now.add(const Duration(minutes: 5));
      expect(cache.get('a'), isNull);
    });

    test('returns null for a missing key and after clear', () {
      final cache = TtlCache<String, int>(CacheConstants.listCacheTtl);
      expect(cache.get('missing'), isNull);
      cache.set('a', 1);
      cache.clear();
      expect(cache.get('a'), isNull);
    });
  });

  group('EventsService cache', () {
    test('a second read inside the window sends no request', () async {
      final adapter = _CountingAdapter();
      final service = EventsService(Dio()..httpClientAdapter = adapter);

      await service.getUpcomingEventsPage(1);
      await service.getUpcomingEventsPage(1);

      expect(adapter.count('GET', '/events'), 1);
    });

    test('create, register and delete clear the cache', () async {
      final adapter = _CountingAdapter();
      final service = EventsService(Dio()..httpClientAdapter = adapter);

      await service.getUpcomingEventsPage(1);
      await service.createEvent({'title': 'x'});
      await service.getUpcomingEventsPage(1);
      await service.registerForEvent(1);
      await service.getUpcomingEventsPage(1);
      await service.deleteEvent(1);
      await service.getUpcomingEventsPage(1);

      expect(adapter.count('GET', '/events'), 4);
    });

    test('clearCache forces the next read to fetch again', () async {
      final adapter = _CountingAdapter();
      final service = EventsService(Dio()..httpClientAdapter = adapter);

      await service.getUpcomingEventsPage(1);
      service.clearCache();
      await service.getUpcomingEventsPage(1);

      expect(adapter.count('GET', '/events'), 2);
    });

    test('NFR-P1: sends the page and page size, and caches page 1 only',
        () async {
      final adapter = _CountingAdapter();
      final service = EventsService(Dio()..httpClientAdapter = adapter);

      final first = await service.getUpcomingEventsPage(1);
      await service.getUpcomingEventsPage(2);
      await service.getUpcomingEventsPage(2);
      await service.getUpcomingEventsPage(1);

      expect(adapter.count('GET', '/events'), 3);
      expect(adapter.queries[1],
          {'page': 2, 'pageSize': PagingConstants.listPageSize});
      expect(first.hasMore, isTrue);
    });

    test('the returned list cannot change the cache', () async {
      final service =
          EventsService(Dio()..httpClientAdapter = _CountingAdapter());

      final first = (await service.getUpcomingEventsPage(1)).items;
      expect(() => first.add({'id': 2}), throwsUnsupportedError);
      final second = (await service.getUpcomingEventsPage(1)).items;
      expect(() => second.clear(), throwsUnsupportedError);
      expect(second.length, 1);
    });
  });

  group('JobService cache', () {
    test('a second read inside the window sends no request', () async {
      final adapter = _CountingAdapter();
      final service = JobService(Dio()..httpClientAdapter = adapter);

      await service.getJobsPage(1);
      await service.getJobsPage(1);

      expect(adapter.count('GET', '/jobs'), 1);
    });

    test('post, delete and approval clear the cache', () async {
      final adapter = _CountingAdapter();
      final service = JobService(Dio()..httpClientAdapter = adapter);

      await service.getJobsPage(1);
      await service.postJob({'title': 'x'});
      await service.getJobsPage(1);
      await service.deleteJob(1);
      await service.getJobsPage(1);
      await service.resolveJobApproval(1, true);
      await service.getJobsPage(1);

      expect(adapter.count('GET', '/jobs'), 4);
    });

    test('clearCache forces the next read to fetch again', () async {
      final adapter = _CountingAdapter();
      final service = JobService(Dio()..httpClientAdapter = adapter);

      await service.getJobsPage(1);
      service.clearCache();
      await service.getJobsPage(1);

      expect(adapter.count('GET', '/jobs'), 2);
    });

    test('NFR-P1: sends the page and page size, and caches page 1 only',
        () async {
      final adapter = _CountingAdapter();
      final service = JobService(Dio()..httpClientAdapter = adapter);

      await service.getJobsPage(1);
      await service.getJobsPage(2);
      await service.getJobsPage(2);
      await service.getJobsPage(1);

      expect(adapter.count('GET', '/jobs'), 3);
      expect(adapter.queries[1],
          {'page': 2, 'pageSize': PagingConstants.listPageSize});
    });

    test('the returned list cannot change the cache', () async {
      final service = JobService(Dio()..httpClientAdapter = _CountingAdapter());

      final first = (await service.getJobsPage(1)).items;
      expect(() => first.removeAt(0), throwsUnsupportedError);
      expect((await service.getJobsPage(1)).items.length, 1);
    });
  });

  group('LookupService cache', () {
    test('a group is fetched once, and groups are cached separately', () async {
      final adapter = _CountingAdapter();
      final service = LookupService(Dio()..httpClientAdapter = adapter);

      await service.getByGroup('Sector');
      await service.getByGroup('Sector');
      await service.getByGroup('PassingYear');

      expect(adapter.count('GET', '/lookups/Sector'), 1);
      expect(adapter.count('GET', '/lookups/PassingYear'), 1);
    });

    test('the full lookup map is fetched once', () async {
      final adapter = _CountingAdapter();
      final service = LookupService(Dio()..httpClientAdapter = adapter);

      await service.getAllLookups();
      await service.getAllLookups();

      expect(adapter.count('GET', '/lookups'), 1);
    });

    test('clearCache empties both the group and the full-map caches', () async {
      final adapter = _CountingAdapter();
      final service = LookupService(Dio()..httpClientAdapter = adapter);

      await service.getByGroup('Sector');
      await service.getAllLookups();
      service.clearCache();
      await service.getByGroup('Sector');
      await service.getAllLookups();

      expect(adapter.count('GET', '/lookups/Sector'), 2);
      expect(adapter.count('GET', '/lookups'), 2);
    });

    test('returned lists and maps cannot change the cache', () async {
      final service =
          LookupService(Dio()..httpClientAdapter = _CountingAdapter());

      final group = await service.getByGroup('Sector');
      expect(() => group.add({'id': 2}), throwsUnsupportedError);
      final all = await service.getAllLookups();
      expect(() => all['New'] = [], throwsUnsupportedError);
      expect(() => all['Sector']!.add(1), throwsUnsupportedError);
    });
  });

  group('session change', () {
    test('invalidateSessionCaches gives the next user fresh services',
        () async {
      final adapter = _CountingAdapter();
      final container = ProviderContainer(overrides: [
        dioProvider.overrideWithValue(Dio()..httpClientAdapter = adapter),
      ]);
      addTearDown(container.dispose);
      final invalidate = Provider<void Function()>(
          (ref) => () => invalidateSessionCaches(ref));

      await container.read(eventsServiceProvider).getUpcomingEventsPage(1);
      await container.read(jobServiceProvider).getJobsPage(1);
      await container.read(lookupServiceProvider).getByGroup('Sector');
      container.read(invalidate)();
      await container.read(eventsServiceProvider).getUpcomingEventsPage(1);
      await container.read(jobServiceProvider).getJobsPage(1);
      await container.read(lookupServiceProvider).getByGroup('Sector');

      expect(adapter.count('GET', '/events'), 2);
      expect(adapter.count('GET', '/jobs'), 2);
      expect(adapter.count('GET', '/lookups/Sector'), 2);
    });
  });

  group('decodePixels', () {
    Future<int?> decode(WidgetTester tester, double? size, double ratio) async {
      int? result;
      tester.view.devicePixelRatio = ratio;
      addTearDown(tester.view.resetDevicePixelRatio);
      await tester.pumpWidget(MaterialApp(
        home: Builder(builder: (context) {
          result = decodePixels(context, size);
          return const SizedBox();
        }),
      ));
      return result;
    }

    testWidgets('multiplies the logical size by the pixel ratio',
        (tester) async {
      expect(await decode(tester, 50, 2.0), 100);
    });

    testWidgets('caps the pixel ratio', (tester) async {
      expect(await decode(tester, 50, 4.0),
          (50 * ImageConstants.maxDecodePixelRatio).ceil());
    });

    testWidgets('returns null for unknown or unbounded sizes', (tester) async {
      expect(await decode(tester, null, 2.0), isNull);
      expect(await decode(tester, double.infinity, 2.0), isNull);
    });
  });
}
