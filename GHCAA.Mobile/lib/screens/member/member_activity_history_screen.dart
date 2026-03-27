import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../features/activity/activity_service.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../core/widgets/glass_tile.dart';

final activityHistoryTrackerProvider = FutureProvider.autoDispose<List<dynamic>>((ref) => ref.read(activityServiceProvider).getMyActivity());

class MemberActivityHistoryScreen extends ConsumerWidget {
  const MemberActivityHistoryScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final activityAsync = ref.watch(activityHistoryTrackerProvider);

    return AppScaffold(
      title: 'Activity Record',
      breadcrumb: 'Identity Registry > Action History',
      child: AsyncValueWidget<List<dynamic>>(
        value: activityAsync,
        loadingMessage: 'Synchronizing activity details...',
        onRetry: () => ref.invalidate(activityHistoryTrackerProvider),
        data: (logs) => logs.isEmpty 
          ? const Center(child: Text('No activity records synchronized.', style: TextStyle(color: AppTheme.textSecondaryDark)))
          : RefreshIndicator(
              color: AppTheme.royalGold,
              onRefresh: () async {
                HapticFeedback.mediumImpact();
                ref.invalidate(activityHistoryTrackerProvider);
              },
              child: ListView.builder(
                padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 24),
                itemCount: logs.length,
                itemBuilder: (context, index) {
                  final log = logs[index];
                  return Padding(
                    padding: const EdgeInsets.only(bottom: 12.0),
                    child: GlassTile(
                      icon: _getIconForAction(log['action']),
                      title: (log['action'] ?? 'SYSTEM EVENT').toUpperCase(),
                      subtitle: '${log['details']}\n${log['createdAt'] ?? ''}',
                      onTap: () {
                        HapticFeedback.lightImpact();
                      },
                    ),
                  );
                },
              ),
            ),
      ),
    );
  }

  IconData _getIconForAction(String? action) {
    if (action == null) return Icons.history;
    final a = action.toLowerCase();
    if (a.contains('login')) return Icons.login;
    if (a.contains('update')) return Icons.edit_note;
    if (a.contains('password')) return Icons.lock_reset;
    if (a.contains('apply')) return Icons.assignment_outlined;
    return Icons.history;
  }
}
