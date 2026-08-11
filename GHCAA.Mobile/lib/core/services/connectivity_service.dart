import 'package:connectivity_plus/connectivity_plus.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

/// Emits [true] when the device has network access, [false] otherwise.
final connectivityStreamProvider = StreamProvider<bool>((ref) {
  return Connectivity()
      .onConnectivityChanged
      .map((results) => results.any((r) => r != ConnectivityResult.none));
});

/// Provides the current connectivity status synchronously (defaults to true
/// until the first event arrives to avoid false "offline" flicker on startup).
final isOnlineProvider = Provider<bool>((ref) {
  final async = ref.watch(connectivityStreamProvider);
  return async.when(
    data: (online) => online,
    loading: () => true,
    error: (_, __) => true,
  );
});
