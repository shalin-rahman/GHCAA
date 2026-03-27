import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
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
      error: (e, s) => Center(
        child: Padding(
          padding: const EdgeInsets.all(32.0),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.error_outline, size: 48, color: Colors.redAccent),
              const SizedBox(height: 16),
              const Text('Something went wrong', style: TextStyle(fontWeight: FontWeight.bold, color: Colors.white)),
              const SizedBox(height: 8),
              Text(e.toString(), textAlign: TextAlign.center, style: const TextStyle(fontSize: 12, color: Colors.grey)),
              if (onRetry != null) ...[
                const SizedBox(height: 24),
                ElevatedButton(onPressed: onRetry!, child: const Text('Connect to API Engine')),
              ],
            ],
          ),
        ),
      ),
    );
  }
}
