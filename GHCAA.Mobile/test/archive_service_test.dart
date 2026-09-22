import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/features/archive/archive_service.dart';

void main() {
  test('loads public archive collections with search', () async {
    final dio = Dio()..httpClientAdapter = _Adapter((options) {
      expect(options.path, '/archive/public');
      expect(options.queryParameters['search'], 'stories');
      return [
        {'id': 1, 'title': 'Stories', 'description': 'Memories', 'decade': 1970}
      ];
    });

    final result = await ArchiveService(dio).getPublicCollections(search: ' stories ');

    expect(result.single.title, 'Stories');
    expect(result.single.decade, 1970);
  });
}

class _Adapter implements HttpClientAdapter {
  final List<dynamic> Function(RequestOptions) handler;
  _Adapter(this.handler);

  @override
  Future<ResponseBody> fetch(RequestOptions options, Stream<List<int>>? requestStream, Future<void>? cancelFuture) async {
    final data = handler(options);
    return ResponseBody.fromString(
      jsonEncode(data),
      200,
      headers: {'content-type': ['application/json']},
    );
  }

  @override
  void close({bool force = false}) {}
}
