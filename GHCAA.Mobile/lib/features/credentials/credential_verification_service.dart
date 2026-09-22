import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/api/api_client.dart';
import 'credential_verification_models.dart';

export 'credential_verification_models.dart';

final credentialVerificationServiceProvider =
    Provider<CredentialVerificationService>(
  (ref) => CredentialVerificationService(ref.read(dioProvider)),
);

class CredentialVerificationService {
  final Dio _dio;

  CredentialVerificationService(this._dio);

  Future<CredentialVerification> verify(String shortCode) async {
    final normalizedCode = shortCode.trim().toUpperCase();
    final response = await _dio.get(
      '/verify/${Uri.encodeComponent(normalizedCode)}',
    );
    return CredentialVerification.fromJson(
      response.data as Map<String, dynamic>,
    );
  }
}
