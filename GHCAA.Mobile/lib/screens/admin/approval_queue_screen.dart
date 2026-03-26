import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_tile.dart';
import '../../core/constants/app_constants.dart';
import '../../features/admin/admin_service.dart';

final pendingApprovalsProvider = FutureProvider<List<dynamic>>((ref) async => ref.read(adminServiceProvider).getPendingApprovals());

class ApprovalQueueScreen extends ConsumerWidget {
  const ApprovalQueueScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final pendingAsync = ref.watch(pendingApprovalsProvider);

    return AppScaffold(
      isAdmin: true,
      title: 'Member Approval Queue',
      child: pendingAsync.when(
        data: (pending) => pending.isEmpty 
          ? const Center(child: Text('No pending applications.'))
          : ListView.builder(
              padding: const EdgeInsets.all(AppConstants.paddingLarge),
              itemCount: pending.length,
              itemBuilder: (context, index) {
                final member = pending[index];
                return Padding(
                  padding: const EdgeInsets.only(bottom: 12.0),
                  child: GlassTile(
                    icon: Icons.person_add,
                    title: member['fullName'] ?? 'Anonymous Delegate',
                    subtitle: '${member['batch'] ?? 'Batch'} • ${member['department'] ?? 'Department'}',
                    trailing: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        IconButton(icon: const Icon(Icons.check_circle, color: Colors.green), onPressed: () => _handleApproval(ref, member['id'], true)),
                        IconButton(icon: const Icon(Icons.cancel, color: Colors.red), onPressed: () => _handleApproval(ref, member['id'], false)),
                      ],
                    ),
                    onTap: () {},
                  ),
                );
              },
            ),
        loading: () => const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
        error: (e, s) => Center(child: Text('Error loading queue: $e')),
      ),
    );
  }

  Future<void> _handleApproval(WidgetRef ref, int id, bool approve) async {
    final success = await ref.read(adminServiceProvider).resolveApproval(id, approve);
    if (success) {
      ref.invalidate(pendingApprovalsProvider);
    }
  }
}
