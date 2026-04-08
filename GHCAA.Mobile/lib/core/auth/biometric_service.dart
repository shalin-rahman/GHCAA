import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:flutter/services.dart';
import 'package:local_auth/local_auth.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

final biometricServiceProvider = Provider<BiometricService>((ref) => BiometricService());

class BiometricService {
  final LocalAuthentication auth = LocalAuthentication();

  Future<bool> isDeviceCapable() async {
    // local_auth plugin does not support Flutter Web — return false gracefully.
    if (kIsWeb) return false;
    try {
      final bool canAuthenticateWithBiometrics = await auth.canCheckBiometrics;
      final bool canAuthenticate = canAuthenticateWithBiometrics || await auth.isDeviceSupported();
      return canAuthenticate;
    } on PlatformException catch (_) {
      return false;
    }
  }

  Future<bool> authenticate() async {
    if (kIsWeb) return false;
    try {
      final bool didAuthenticate = await auth.authenticate(
        localizedReason: 'Please authenticate to access sensitive membership data.',
        options: const AuthenticationOptions(
          biometricOnly: true,
          stickyAuth: true,
        ),
      );
      return didAuthenticate;
    } on PlatformException {
      return false;
    }
  }
}
