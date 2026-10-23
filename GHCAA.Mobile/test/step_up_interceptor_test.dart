import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/api/step_up_interceptor.dart';

/// FR-39 (37.1w). The vote endpoint answers 403 STEP_UP_REQUIRED until the
/// request carries the re-issued token. Records every path it sees.
class _StepUpAdapter implements HttpClientAdapter {
  _StepUpAdapter(
      {this.verifyStatus = 200, this.sendStatus = 200, this.enrolled = false, this.statusStatus = 200, this.noEmail = false});

  final int verifyStatus;
  final int sendStatus;
  final bool enrolled;
  final bool noEmail;
  final int statusStatus;
  final List<String> paths = [];
  final List<String?> authHeaders = [];
  Object? verifyBody;

  static const _json = {
    Headers.contentTypeHeader: [Headers.jsonContentType],
  };

  @override
  void close({bool force = false}) {}

  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<Uint8List>? requestStream,
    Future<void>? cancelFuture,
  ) async {
    paths.add(options.path);
    authHeaders.add(options.headers['Authorization'] as String?);
    if (options.path == StepUpInterceptor.authenticatorPath) {
      return statusStatus == 200
          ? ResponseBody.fromString('{"enrolled":$enrolled,"pending":false,"recoveryCodesLeft":10,"setupNeedsPassword":$noEmail}', 200, headers: _json)
          : ResponseBody.fromString('{"detail":"down"}', statusStatus, headers: _json);
    }
    if (options.path == StepUpInterceptor.requestPath) {
      if (sendStatus != 200) {
        return ResponseBody.fromString('{"detail":"The code could not be emailed."}', sendStatus, headers: _json);
      }
      return ResponseBody.fromString('{"sentTo":"sha*****@example.com"}', 200, headers: _json);
    }
    if (options.path == StepUpInterceptor.verifyPath) {
      verifyBody = options.data;
      return verifyStatus == 200
          ? ResponseBody.fromString('{"token":"stepped-up"}', 200, headers: _json)
          : ResponseBody.fromString('{"detail":"bad code"}', verifyStatus, headers: _json);
    }
    if (options.headers['Authorization'] == 'Bearer stepped-up') {
      return ResponseBody.fromString('{"ok":true}', 200, headers: _json);
    }
    return ResponseBody.fromString('{"code":"STEP_UP_REQUIRED"}', 403, headers: _json);
  }
}

void main() {
  late Dio dio;
  late _StepUpAdapter adapter;
  String? savedToken;
  String? promptedSentTo;
  bool? promptedForApp;
  List<bool> promptedCanSwitch = [];
  int prompts = 0;

  // With switchToEmail the first prompt asks for an emailed code, and [code] answers the second.
  Dio build(
      {String? code,
      int verifyStatus = 200,
      int sendStatus = 200,
      bool enrolled = false,
      int statusStatus = 200,
      bool noEmail = false,
      bool switchToEmail = false}) {
    savedToken = null;
    prompts = 0;
    promptedForApp = null;
    promptedCanSwitch = [];
    adapter = _StepUpAdapter(
        verifyStatus: verifyStatus,
        sendStatus: sendStatus,
        enrolled: enrolled,
        statusStatus: statusStatus,
        noEmail: noEmail);
    final d = Dio()..httpClientAdapter = adapter;
    d.interceptors.add(StepUpInterceptor(
      dio: d,
      promptForCode: (sentTo, useApp, canSwitch) async {
        prompts++;
        promptedSentTo = sentTo;
        promptedForApp = useApp;
        promptedCanSwitch.add(canSwitch);
        if (switchToEmail && prompts == 1) return StepUpInterceptor.emailInstead;
        return code;
      },
      saveToken: (t) async => savedToken = t,
    ));
    return d;
  }

  test('FR-39: STEP_UP_REQUIRED asks for a code, verifies, saves the token and retries once', () async {
    dio = build(code: '123456');

    final res = await dio.post('/elections/1/vote');

    expect(res.statusCode, 200);
    expect(prompts, 1);
    expect(savedToken, 'stepped-up');
    expect(adapter.paths, [
      '/elections/1/vote',
      StepUpInterceptor.authenticatorPath,
      StepUpInterceptor.requestPath,
      StepUpInterceptor.verifyPath,
      '/elections/1/vote',
    ]);
    expect(adapter.authHeaders.last, 'Bearer stepped-up');
    expect(adapter.verifyBody, {'code': '123456', 'method': 'email'});
    expect(promptedForApp, isFalse);
  });

  test('FR-39: a user with an authenticator app is asked for the app code and gets no email', () async {
    dio = build(code: 'ABCDE-12345', enrolled: true);

    final res = await dio.post('/elections/1/vote');

    expect(res.statusCode, 200);
    expect(promptedForApp, isTrue);
    expect(promptedSentTo, isNull);
    expect(adapter.paths, isNot(contains(StepUpInterceptor.requestPath)));
    expect(adapter.verifyBody, {'code': 'ABCDE-12345', 'method': 'app'});
    expect(promptedCanSwitch, [true]);
  });

  test('FR-39: an app user can ask for an emailed code instead', () async {
    dio = build(code: '123456', enrolled: true, switchToEmail: true);

    final res = await dio.post('/elections/1/vote');

    expect(res.statusCode, 200);
    expect(prompts, 2);
    expect(promptedCanSwitch, [true, false]);
    expect(promptedForApp, isFalse);
    expect(promptedSentTo, 'sha*****@example.com');
    expect(adapter.paths, contains(StepUpInterceptor.requestPath));
    expect(adapter.verifyBody, {'code': '123456', 'method': 'email'});
  });

  test('FR-39: an app user with no email on file is not offered an emailed code', () async {
    dio = build(code: '654321', enrolled: true, noEmail: true);

    await dio.post('/elections/1/vote');

    expect(promptedCanSwitch, [false]);
    expect(adapter.verifyBody, {'code': '654321', 'method': 'app'});
  });

  test('FR-39: a failed authenticator status check falls back to the email code', () async {
    dio = build(code: '123456', statusStatus: 500);

    final res = await dio.post('/elections/1/vote');

    expect(res.statusCode, 200);
    expect(promptedForApp, isFalse);
    expect(adapter.paths, contains(StepUpInterceptor.requestPath));
  });

  test('the prompt is told the masked address the code went to', () async {
    dio = build(code: '123456');

    await dio.post('/elections/1/vote');

    expect(promptedSentTo, 'sha*****@example.com');
  });

  test('FR-39: cancelling the prompt returns the original 403', () async {
    dio = build(code: null);

    final err = await dio.post('/elections/1/vote').then<DioException?>((_) => null, onError: (e) => e as DioException);

    expect(err?.response?.statusCode, 403);
    expect(savedToken, isNull);
    expect(adapter.paths,
        ['/elections/1/vote', StepUpInterceptor.authenticatorPath, StepUpInterceptor.requestPath]);
  });

  test('FR-39: a rejected code returns the original 403 without retrying', () async {
    dio = build(code: '000000', verifyStatus: 400);

    final err = await dio.post('/elections/1/vote').then<DioException?>((_) => null, onError: (e) => e as DioException);

    expect(err?.response?.statusCode, 403);
    expect(savedToken, isNull);
    expect(adapter.paths.where((p) => p == '/elections/1/vote').length, 1);
  });

  test('a code that could not be emailed returns the send error, not the 403', () async {
    dio = build(code: '123456', sendStatus: 503);

    final err = await dio.post('/elections/1/vote').then<DioException?>((_) => null, onError: (e) => e as DioException);

    expect(err?.response?.statusCode, 503);
    expect((err?.response?.data as Map)['detail'], 'The code could not be emailed.');
    expect(prompts, 0);
  });

  test('a plain 403 without the step-up code passes straight through', () async {
    dio = Dio()..httpClientAdapter = _Plain403Adapter();
    dio.interceptors.add(StepUpInterceptor(
      dio: dio,
      promptForCode: (_, __, ___) async {
        prompts++;
        return '1';
      },
      saveToken: (_) async {},
    ));
    prompts = 0;

    final err = await dio.get('/admin/x').then<DioException?>((_) => null, onError: (e) => e as DioException);

    expect(err?.response?.statusCode, 403);
    expect(prompts, 0);
  });
}

class _Plain403Adapter implements HttpClientAdapter {
  @override
  void close({bool force = false}) {}

  @override
  Future<ResponseBody> fetch(RequestOptions options, Stream<Uint8List>? s, Future<void>? c) async =>
      ResponseBody.fromString('{"detail":"forbidden"}', 403, headers: {
        Headers.contentTypeHeader: [Headers.jsonContentType],
      });
}
