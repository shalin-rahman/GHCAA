import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:dio/dio.dart';
import 'skeleton_loader.dart';

class AsyncValueWidget<T> extends StatelessWidget {
  final AsyncValue<T> value;
  final Widget Function(T) data;
  final String? loadingMessage;
  final VoidCallback? onRetry;
  final Widget? loadingWidget;

  const AsyncValueWidget({
    super.key,
    required this.value,
    required this.data,
    this.loadingMessage,
    this.onRetry,
    this.loadingWidget,
  });

  @override
  Widget build(BuildContext context) {
    return value.when(
      data: data,
      loading: () => loadingWidget ?? SkeletonLoader.listStub(),
      error: (e, s) {
        String displayError = 'Unknown Synchronization Error';
        if (e is DioException) {
          displayError = e.message ?? e.toString();
        } else {
          displayError = e.toString();
        }

        return Center(
          child: Padding(
            padding: const EdgeInsets.all(40.0),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                Icon(
                  Icons.wifi_off_rounded, 
                  size: 64, 
                  color: Theme.of(context).colorScheme.secondary.withValues(alpha: 0.3)
                ),
                const SizedBox(height: 24),
                Text(
                  'SYNC INTERRUPTED', 
                  style: TextStyle(
                    letterSpacing: 1.5,
                    fontWeight: FontWeight.w900, 
                    color: Theme.of(context).colorScheme.onSurface,
                    fontSize: 14
                  )
                ),
                const SizedBox(height: 12),
                Text(
                  displayError, 
                  textAlign: TextAlign.center, 
                  style: TextStyle(
                    fontSize: 12, 
                    color: Theme.of(context).colorScheme.onSurface.withValues(alpha: 0.65),
                    height: 1.5
                  )
                ),
                if (onRetry != null) ...[
                  const SizedBox(height: 32),
                  SizedBox(
                    width: 200,
                    child: ElevatedButton.icon(
                      onPressed: onRetry!, 
                      icon: const Icon(Icons.refresh_rounded, size: 18),
                      label: const Text('RETRY CONNECTION'),
                    ),
                  ),
                ],
              ],
            ),
          ),
        );
      },
    );
  }
}
