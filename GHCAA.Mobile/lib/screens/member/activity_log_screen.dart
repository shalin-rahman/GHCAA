import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/constants/app_constants.dart';
import '../../features/activity/activity_service.dart';

final activityTrackerProvider = FutureProvider<List<dynamic>>((ref) => ref.read(activityServiceProvider).getMyActivity());

class ActivityLogScreen extends ConsumerWidget {
  const ActivityLogScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final activityAsync = ref.watch(activityTrackerProvider);

    return AppScaffold(
      title: 'Action History',
      child: activityAsync.when(
        data: (logs) => logs.isEmpty 
          ? const Center(child: Text('No activity recorded yet.'))
          : ListView.builder(
              padding: const EdgeInsets.all(AppConstants.paddingLarge),
              itemCount: logs.length,
              itemBuilder: (context, index) {
                final log = logs[index];
                return Padding(
                  padding: const EdgeInsets.only(bottom: 12.0),
                  child: GlassContainer(
                    child: ListTile(
                      leading: const Icon(Icons.history, color: AppTheme.royalGold),
                      title: Text(log['action'] ?? 'System Event', style: const TextStyle(fontWeight: FontWeight.bold)),
                      subtitle: Text(log['details'] ?? ''),
                      trailing: Text(log['createdAt'] ?? '', style: const TextStyle(fontSize: 10)),
                    ),
                  ),
                );
              },
            ),
        loading: () => const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
        error: (e, s) => Center(child: Text('Error: $e')),
      ),
    );
  }
}
