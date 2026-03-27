import 'dart:async';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';

final sessionProvider = StateNotifierProvider<SessionManager, DateTime>((ref) => SessionManager(ref));

class SessionManager extends StateNotifier<DateTime> {
    Timer? _timer;
  
  // Set inactivity limit to 15 minutes by default
  static const Duration inactivityLimit = Duration(minutes: 15);

  SessionManager(Ref ref) : super(DateTime.now()) {
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
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove('jwt_token');
    
    // We cannot access context directly here, so we reset state or use a global router approach
    // We'll use the 'ref' to trigger a logout state or coordinate with AuthState
    // For now, we'll rely on the Router listening to changes or redirecting on next build
  }

  @override
  void dispose() {
    _timer?.cancel();
    super.dispose();
  }
}
