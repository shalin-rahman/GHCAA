import 'dart:async';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../features/auth/auth_service.dart';

final sessionProvider = StateNotifierProvider<SessionManager, DateTime>((ref) => SessionManager(ref));

// 82.39: this used to be one of two independent inactivity-timeout mechanisms (this one at 15
// minutes, main.dart's own _checkInactivity at 10 minutes), both clearing the same storage on
// their own schedule, so the effective timeout was silently whichever fired first. SessionManager
// is now the single owner: main.dart just forwards activity pings and app-resume checks to it.
class SessionManager extends StateNotifier<DateTime> {
    Timer? _timer;
    final Ref _ref;

  // Inactivity limit: 15 minutes.
  static const Duration inactivityLimit = Duration(minutes: 15);

  SessionManager(this._ref) : super(DateTime.now()) {
    _startTimer();
  }

  void userActivityDetected() {
    state = DateTime.now();
  }

  void _startTimer() {
    _timer?.cancel();
    _timer = Timer.periodic(const Duration(minutes: 1), (timer) => checkNow());
  }

  /// Runs the same inactivity check the periodic timer runs. Call this directly on app
  /// resume — a backgrounded app's Timer.periodic can miss ticks it should have fired while
  /// suspended, so the periodic timer alone can't be trusted to catch expiry on its own.
  void checkNow() {
    final inactiveDuration = DateTime.now().difference(state);
    if (inactiveDuration >= inactivityLimit) {
      _handleSessionExpiry();
    }
  }

  Future<void> _handleSessionExpiry() async {
    _timer?.cancel();
    // Goes through AuthService.logout() (not just storage.clearAll()) so the SignalR hub and
    // cached role/profile providers are torn down the same way an explicit logout tears them
    // down — an inactivity-triggered logout shouldn't leave those behind.
    await _ref.read(authServiceProvider).logout();
  }

  @override
  void dispose() {
    _timer?.cancel();
    super.dispose();
  }
}
