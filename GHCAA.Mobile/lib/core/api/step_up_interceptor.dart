import 'package:dio/dio.dart';

/// 37.1w: the API answers a step-up-gated action (casting a vote, for one) with
/// 403 and code STEP_UP_REQUIRED. This asks for a code, verifies it, stores the
/// re-issued token and retries the original request once.
/// 95.6: a user with an authenticator app gets asked for the app code first and
/// no email goes out, unless they ask for an emailed code instead. The API takes either.
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
  static const authenticatorPath = '/auth/authenticator';
  static const _retriedKey = '_stepUpRetried';
  // The prompt returns this when an app user asks for an emailed code instead.
  static const emailInstead = '\u0000email-instead';

  final Dio _dio;
  // Gets the masked address the code went to (null when the API did not say),
  // whether to ask for the authenticator app code instead of the email one, and
  // whether the user may switch to an emailed code (false when no email is on file).
  final Future<String?> Function(String? sentTo, bool useApp, bool canSwitch) promptForCode;
  final Future<void> Function(String token) saveToken;

  bool _isStepUpChallenge(DioException err) {
    if (err.response?.statusCode != 403) return false;
    final data = err.response?.data;
    if (data is! Map) return false;
    return (data['code'] ?? data['Code']) == errorCode;
  }

  // A failed status check returns null, which falls back to email.
  Future<Map?> _authenticatorStatus() async {
    try {
      final res = await _dio.get(authenticatorPath, options: Options(extra: {_retriedKey: true}));
      final body = res.data;
      return body is Map ? body : null;
    } catch (_) {
      return null;
    }
  }

  // Returns the masked address the code went to, when the API says.
  Future<String?> _sendEmailCode() async {
    final sent = await _dio.post(requestPath);
    final body = sent.data;
    return body is Map ? (body['sentTo'] ?? body['SentTo']) as String? : null;
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) async {
    final options = err.requestOptions;
    if (!_isStepUpChallenge(err) ||
        options.extra[_retriedKey] == true ||
        options.path.contains('/auth/step-up/')) {
      return handler.next(err);
    }

    final status = await _authenticatorStatus();
    var useApp = status != null && (status['enrolled'] ?? status['Enrolled']) == true;
    final canSwitch = useApp && (status['setupNeedsPassword'] ?? status['SetupNeedsPassword']) != true;
    String? sentTo;
    if (!useApp) {
      try {
        sentTo = await _sendEmailCode();
      } on DioException catch (sendErr) {
        // Pass on the send failure, not the 403. Its detail says the email did not go out.
        return handler.next(sendErr);
      }
    }

    try {
      var code = await promptForCode(sentTo, useApp, canSwitch);
      if (canSwitch && code == emailInstead) {
        useApp = false;
        try {
          sentTo = await _sendEmailCode();
        } on DioException catch (sendErr) {
          return handler.next(sendErr);
        }
        code = await promptForCode(sentTo, false, false);
      }
      if (code == null || code.trim().isEmpty) return handler.next(err);

      final verify = await _dio.post(verifyPath, data: {
        'code': code.trim(),
        'method': useApp ? 'app' : 'email',
      });
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
