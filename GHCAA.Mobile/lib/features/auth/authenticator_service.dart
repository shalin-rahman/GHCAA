import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';
import '../../core/api/step_up_interceptor.dart';

final authenticatorServiceProvider = Provider<AuthenticatorService>((ref) {
  return AuthenticatorService(ref.read(dioProvider));
});

class AuthenticatorStatus {
  const AuthenticatorStatus(
      {required this.enrolled, required this.pending, required this.recoveryCodesLeft, this.setupNeedsPassword = false});

  factory AuthenticatorStatus.fromJson(Map<String, dynamic> json) => AuthenticatorStatus(
        enrolled: json['enrolled'] == true,
        pending: json['pending'] == true,
        recoveryCodesLeft: (json['recoveryCodesLeft'] as num?)?.toInt() ?? 0,
        setupNeedsPassword: json['setupNeedsPassword'] == true,
      );

  final bool enrolled;
  final bool pending;
  final int recoveryCodesLeft;
  // True when there is no email on file, so setup asks for the password instead of a step-up.
  final bool setupNeedsPassword;
}

class AuthenticatorSetup {
  const AuthenticatorSetup({required this.secret, required this.otpAuthUri});

  factory AuthenticatorSetup.fromJson(Map<String, dynamic> json) => AuthenticatorSetup(
        secret: json['secret'] as String? ?? '',
        otpAuthUri: json['otpAuthUri'] as String? ?? '',
      );

  final String secret;
  // The QR is drawn on the phone from this, so the API's PNG data URI is not needed here.
  final String otpAuthUri;
}

/// 95.6: authenticator app setup for step-up. Setup, new codes and removal are
/// step-up gated themselves, so the step-up interceptor asks for a code first.
class AuthenticatorService {
  AuthenticatorService(this._dio);

  final Dio _dio;
  static const _base = StepUpInterceptor.authenticatorPath;

  Future<AuthenticatorStatus> status() async {
    final res = await _dio.get(_base);
    return AuthenticatorStatus.fromJson(Map<String, dynamic>.from(res.data as Map));
  }

  Future<AuthenticatorSetup> setup() async {
    final res = await _dio.post('$_base/setup');
    return AuthenticatorSetup.fromJson(Map<String, dynamic>.from(res.data as Map));
  }

  // For a user with no email, who cannot pass an email step-up. The API refuses it for anyone else.
  Future<AuthenticatorSetup> setupWithPassword(String password) async {
    final res = await _dio.post('$_base/setup-with-password', data: {'password': password});
    return AuthenticatorSetup.fromJson(Map<String, dynamic>.from(res.data as Map));
  }

  // Returns the recovery codes. They are shown once and never again.
  Future<List<String>> confirm(String code) async {
    final res = await _dio.post('$_base/confirm', data: {'code': code});
    return _codes(res.data);
  }

  Future<List<String>> regenerateRecoveryCodes() async {
    final res = await _dio.post('$_base/recovery-codes');
    return _codes(res.data);
  }

  Future<void> remove() => _dio.delete(_base);

  static List<String> _codes(dynamic body) =>
      body is Map ? List<String>.from((body['codes'] as List?) ?? const []) : const [];
}
