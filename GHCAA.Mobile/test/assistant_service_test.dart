// FR-42
// TODO 84.11. The API binds AssistantQueryDto.Query, so the body must carry `query`.
import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/features/assistant/assistant_service.dart';

void main() {
  test('posts the question under query and reads the answer', () async {
    late RequestOptions sent;
    final dio = Dio()..httpClientAdapter = _Adapter((options) {
      sent = options;
      return {'answer': 'Hi there', 'foundMembers': null};
    });

    final answer = await AssistantService(dio).ask('Who is in batch 1990?');

    expect(sent.method, 'POST');
    expect(sent.path, '/assistant/ask');
    expect(sent.data, {'query': 'Who is in batch 1990?'});
    expect(answer, 'Hi there');
  });
}

class _Adapter implements HttpClientAdapter {
  final Object Function(RequestOptions) handler;
  _Adapter(this.handler);

  @override
  Future<ResponseBody> fetch(RequestOptions options, Stream<List<int>>? requestStream, Future<void>? cancelFuture) async {
    return ResponseBody.fromString(
      jsonEncode(handler(options)),
      200,
      headers: {'content-type': ['application/json']},
    );
  }

  @override
  void close({bool force = false}) {}
}
