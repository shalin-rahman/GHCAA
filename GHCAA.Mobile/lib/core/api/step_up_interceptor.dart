import 'package:dio/dio.dart';

/// 37.1w: the API answers a step-up-gated action (casting a vote, for one) with
/// 403 and code STEP_UP_REQUIRED. This asks for the emailed code, verifies it,
/// stores the re-issued token and retries the original request once.
/// Cancelling the prompt, or a failed verify, hands back the original error.
class StepUpInterceptor extends Interceptor {
  StepUpInterceptor({
    required Dio dio,
    required this.promptForCode,
    required this.saveToken,
  }) : _dio = dio;

  static const errorCode = 'STEP_UP_REQUIRED';
  static const requestPath = '/auth/step-up/request';
  static const verifyPath = '/auth/step-up/verify';
  static const _retriedKey = '_stepUpRetried';

  final Dio _dio;
  final Future<String?> Function() promptForCode;
  final Future<void> Function(String token) saveToken;

  bool _isStepUpChallenge(DioException err) {
    if (err.response?.statusCode != 403) return false;
    final data = err.response?.data;
    if (data is! Map) return false;
    return (data['code'] ?? data['Code']) == errorCode;
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) async {
    final options = err.requestOptions;
    if (!_isStepUpChallenge(err) ||
        options.extra[_retriedKey] == true ||
        options.path.contains('/auth/step-up/')) {
      return handler.next(err);
    }

    try {
      await _dio.post(requestPath);
      final code = await promptForCode();
      if (code == null || code.trim().isEmpty) return handler.next(err);

      final verify = await _dio.post(verifyPath, data: {'code': code.trim()});
      final body = verify.data;
      final token = body is Map ? (body['token'] ?? body['Token']) as String? : null;
      if (token == null) return handler.next(err);
      await saveToken(token);

      options.headers['Authorization'] = 'Bearer $token';
      options.extra[_retriedKey] = true;
      return handler.resolve(await _dio.fetch(options));
    } on DioException {
      return handler.next(err);
    }
  }
}
