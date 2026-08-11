import 'dart:async';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../storage/storage_service.dart';

final sessionProvider = StateNotifierProvider<SessionManager, DateTime>((ref) => SessionManager(ref));

class SessionManager extends StateNotifier<DateTime> {
    Timer? _timer;
    final Ref _ref;

  // Set inactivity limit to 15 minutes by default
  static const Duration inactivityLimit = Duration(minutes: 15);

  SessionManager(this._ref) : super(DateTime.now()) {
    _startTimer();
  }

  void userActivityDetected() {
    state = DateTime.now();
  }

  void _startTimer() {
    _timer?.cancel();
    _timer = Timer.periodic(const Duration(minutes: 1), (timer) {
      final inactiveDuration = DateTime.now().difference(state);
      if (inactiveDuration >= inactivityLimit) {
        _handleSessionExpiry();
      }
    });
  }

  Future<void> _handleSessionExpiry() async {
    _timer?.cancel();
    // 29A.2: The real auth token lives in secure storage (StorageService), NOT the stale
    // SharedPreferences 'jwt_token' key the old code removed. Clear it the same way logout does
    // so authStateProvider's poll sees a null token and the router redirects to /login.
    await _ref.read(storageServiceProvider).clearAll();
  }

  @override
  void dispose() {
    _timer?.cancel();
    super.dispose();
  }
}
