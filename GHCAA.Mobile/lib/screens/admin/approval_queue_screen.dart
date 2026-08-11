import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../features/admin/admin_service.dart';
import 'package:go_router/go_router.dart';
import '../../core/widgets/glass_container.dart';

import '../../features/auth/auth_service.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../core/widgets/logo_spinner.dart';

final pendingApprovalsProvider = FutureProvider<List<dynamic>>((ref) async => ref.read(adminServiceProvider).getPendingApprovals());

final memberApprovalSearchQueryProvider = StateProvider.autoDispose<String>((ref) => "");

class ApprovalQueueScreen extends ConsumerWidget {
  const ApprovalQueueScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final pendingAsync = ref.watch(pendingApprovalsProvider);
    final searchQuery = ref.watch(memberApprovalSearchQueryProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      isAdmin: true,
      title: 'Approvals',
      breadcrumb: 'ADMIN > APPROVALS',
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
            child: TextField(
              decoration: InputDecoration(
                hintText: 'Search pending queue...',
                prefixIcon: const Icon(Icons.search, size: 20, color: AppTheme.royalGold),
                suffixIcon: searchQuery.isNotEmpty
                  ? IconButton(
                      icon: const Icon(Icons.close_rounded, size: 20, color: Colors.white54),
                      onPressed: () => ref.read(memberApprovalSearchQueryProvider.notifier).state = "",
                    )
                  : null,
                filled: true,
                fillColor: isDark ? Colors.black.withValues(alpha: 0.2) : Colors.white,
                contentPadding: const EdgeInsets.symmetric(horizontal: 20, vertical: 14),
                border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(16),
                    borderSide: BorderSide(color: AppTheme.royalGold.withValues(alpha: 0.1))),
              ),
              onChanged: (v) => ref.read(memberApprovalSearchQueryProvider.notifier).state = v.toLowerCase(),
            ),
          ),
          Expanded(
            child: pendingAsync.when(
              data: (pending) {
                final filtered = pending.where((m) {
                  final name = (m['fullName'] ?? '').toString().toLowerCase();
                  final id = m['id'].toString();
                  return name.contains(searchQuery) || id.contains(searchQuery);
                }).toList();

                return Column(
                  children: [
                    if (filtered.isNotEmpty)
                      Padding(
                        padding: const EdgeInsets.only(left: 24, bottom: 8),
                        child: Align(
                          alignment: Alignment.centerLeft,
                          child: Text(
                            'Showing ${filtered.length} of ${pending.length} pending delegates',
                            style: TextStyle(fontSize: 10, color: AppTheme.royalGold.withValues(alpha: 0.7), fontWeight: FontWeight.bold),
                          ),
                        ),
                      ),
                    Expanded(
                      child: filtered.isEmpty 
                        ? EmptyStateWidget(searchQuery.isEmpty ? 'No pending approvals found.' : 'No items match your search.', icon: Icons.task_alt)
                        : ListView.builder(
                            padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 4),
                            itemCount: filtered.length,
                            itemBuilder: (context, index) {
                              final member = filtered[index];
                              return Padding(
                                padding: const EdgeInsets.only(bottom: 12.0),
                                child: GlassContainer(
                                  padding: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 12.0),
                                  child: InkWell(
                                    onTap: () => context.push('/directory/${member['id']}'),
                                    child: Column(
                                      children: [
                                        Row(
                                          children: [
                                            Container(
                                              width: 38,
                                              height: 48,
                                              decoration: BoxDecoration(color: AppTheme.royalGold.withValues(alpha: 0.1), borderRadius: BorderRadius.circular(8)),
                                              child: Center(child: Text(member['fullName']?[0] ?? '?', style: const TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.bold))),
                                            ),
                                            const SizedBox(width: 16),
                                            Expanded(
                                              child: Column(
                                                crossAxisAlignment: CrossAxisAlignment.start,
                                                children: [
                                                  Text(member['fullName'] ?? 'Anonymous Delegate',
                                                      style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 13, color: Colors.white, letterSpacing: -0.2)),
                                                  const SizedBox(height: 2),
                                                  Text('SYSTEM ID: ${member['id']}'.toUpperCase(), style: const TextStyle(color: AppTheme.royalGold, fontSize: 8, fontWeight: FontWeight.bold, letterSpacing: 1.2)),
                                                ],
                                              ),
                                            ),
                                            Row(
                                              mainAxisSize: MainAxisSize.min,
                                              children: [
                                                IconButton(
                                                  icon: const Icon(Icons.how_to_reg_rounded, color: Colors.greenAccent, size: 24), 
                                                  onPressed: () => _handleApproval(ref, member['id'], true),
                                                  padding: EdgeInsets.zero,
                                                  constraints: const BoxConstraints(),
                                                ),
                                                const SizedBox(width: 20),
                                                IconButton(
                                                  icon: const Icon(Icons.remove_circle_outline_rounded, color: Colors.redAccent, size: 24), 
                                                  onPressed: () => _handleApproval(ref, member['id'], false),
                                                  padding: EdgeInsets.zero,
                                                  constraints: const BoxConstraints(),
                                                ),
                                              ],
                                            ),
                                          ],
                                        ),
                                        const SizedBox(height: 12),
                                        const Divider(color: Colors.white10, height: 1),
                                        const SizedBox(height: 8),
                                        Row(
                                          mainAxisAlignment: MainAxisAlignment.end,
                                          children: [
                                            Flexible(
                                              child: Text(
                                                'DEPT: ${member['department'] ?? 'GENERAL'} | BATCH ${member['batch'] ?? 'N/A'}'.toUpperCase(),
                                                textAlign: TextAlign.right,
                                                style: const TextStyle(color: AppTheme.textSecondaryDark, fontSize: 9, fontWeight: FontWeight.w900, letterSpacing: 0.5),
                                              ),
                                            ),
                                            const SizedBox(width: 8),
                                            const Icon(Icons.school_outlined, size: 12, color: AppTheme.royalGold),
                                          ],
                                        ),
                                      ],
                                    ),
                                  ),
                                ),
                              );
                            },
                          ),
                    ),
                  ],
                );
              },
              loading: () => const Center(child: LogoSpinner(size: 120)),
              error: (e, s) => Center(child: Text('Error loading queue: $e')),
            ),
          ),
        ],
      ),
    );
  }

  Future<void> _handleApproval(WidgetRef ref, int id, bool approve) async {
    final adminProfile = ref.read(userProfileProvider).value;
    final adminId = adminProfile?['id'] ?? 1; // Fallback to 1 if not yet loaded (SuperAdmin expected)
    
    final success = await ref.read(adminServiceProvider).resolveApproval(id, approve, adminId: adminId);
    if (success) {
      ref.invalidate(pendingApprovalsProvider);
    }
  }
}
