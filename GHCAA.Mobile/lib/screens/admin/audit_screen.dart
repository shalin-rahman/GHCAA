import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_tile.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../core/constants/app_constants.dart';
import '../../features/activity/activity_service.dart';

final globalAuditProvider = FutureProvider.autoDispose<List<dynamic>>((ref) => ref.read(activityServiceProvider).getGlobalActivity());

class AdminAuditScreen extends ConsumerWidget {
  const AdminAuditScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final auditAsync = ref.watch(globalAuditProvider);

    return AppScaffold(
      isAdmin: true,
      title: 'Global Audit Trail',
      child: AsyncValueWidget<List<dynamic>>(
        value: auditAsync,
        loadingMessage: 'Synchronizing system audit logs...',
        onRetry: () => ref.invalidate(globalAuditProvider),
        data: (logs) => logs.isEmpty
          ? const Center(child: Text('No system activity found.'))
          : ListView.builder(
              padding: const EdgeInsets.all(AppConstants.paddingLarge),
              itemCount: logs.length,
              itemBuilder: (context, index) {
                final log = logs[index];
                return Padding(
                  padding: const EdgeInsets.only(bottom: 12.0),
                  child: GlassTile(
                    icon: _getIconForAction(log['action']),
                    title: log['action'] ?? 'System Event',
                    subtitle: 'By Member #${log['memberId']} | ${log['createdAt'] ?? 'Date Unknown'}',
                    onTap: () => _showDetails(context, log),
                    trailing: const Icon(Icons.info_outline, size: 16, color: AppTheme.royalGold),
                  ),
                );
              },
            ),
      ),
    );
  }

  IconData _getIconForAction(String? action) {
    if (action == null) return Icons.history_edu;
    final a = action.toLowerCase();
    if (a.contains('login')) return Icons.login;
    if (a.contains('approve')) return Icons.verified_user_sharp;
    if (a.contains('update')) return Icons.edit_note;
    if (a.contains('delete')) return Icons.delete_sweep;
    if (a.contains('export')) return Icons.file_download;
    return Icons.history_edu;
  }

  void _showDetails(BuildContext context, dynamic log) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: AppTheme.midnightSurface,
        title: Text(log['action']?.toUpperCase() ?? 'LOG DATA', style: const TextStyle(color: AppTheme.royalGold, fontSize: 16)),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _detailRow('Details', log['details']),
            _detailRow('Timestamp', log['createdAt']),
            _detailRow('Member ID', log['memberId']?.toString()),
            _detailRow('IP Address', log['ipAddress']),
          ],
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('CLOSE'))
        ],
      ),
    );
  }

  Widget _detailRow(String label, String? value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4.0),
      child: Text('$label: ${value ?? 'N/A'}', style: const TextStyle(fontSize: 12, color: Colors.white70)),
    );
  }
}
