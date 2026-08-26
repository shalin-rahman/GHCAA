import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_tile.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../core/widgets/reject_reason_dialog.dart';
import '../../features/jobs/job_service.dart';
import 'package:flutter/services.dart';

final pendingJobsProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async => ref.read(jobServiceProvider).getPendingJobs());
final jobApprovalSearchQueryProvider = StateProvider.autoDispose<String>((ref) => "");

/// Sibling screen mirroring ArticleApprovalScreen's queue pattern for job
/// postings awaiting admin review.
class JobApprovalScreen extends ConsumerStatefulWidget {
  const JobApprovalScreen({super.key});

  @override
  ConsumerState<JobApprovalScreen> createState() => _JobApprovalScreenState();
}

class _JobApprovalScreenState extends ConsumerState<JobApprovalScreen> {
  final TextEditingController _searchController = TextEditingController();

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final jobsAsync = ref.watch(pendingJobsProvider);
    final searchQuery = ref.watch(jobApprovalSearchQueryProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      isAdmin: true,
      title: 'Job Postings',
      breadcrumb: 'ADMIN > JOB REVIEW',
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Search pending job postings...',
                prefixIcon: const Icon(Icons.search, size: 20, color: AppTheme.royalGold),
                suffixIcon: _searchController.text.isNotEmpty
                  ? IconButton(
                      icon: const Icon(Icons.close_rounded, size: 20, color: Colors.white54),
                      onPressed: () {
                        _searchController.clear();
                        ref.read(jobApprovalSearchQueryProvider.notifier).state = "";
                      },
                    )
                  : null,
                filled: true,
                fillColor: isDark ? Colors.black.withValues(alpha: 0.2) : Colors.white,
                contentPadding: const EdgeInsets.symmetric(horizontal: 20, vertical: 14),
                border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(16),
                    borderSide: BorderSide(color: AppTheme.royalGold.withValues(alpha: 0.1))),
              ),
              onChanged: (v) => ref.read(jobApprovalSearchQueryProvider.notifier).state = v.toLowerCase(),
            ),
          ),
          Expanded(
            child: AsyncValueWidget<List<dynamic>>(
              value: jobsAsync,
              loadingMessage: 'Loading pending job postings...',
              onRetry: () => ref.invalidate(pendingJobsProvider),
              data: (jobs) {
                final filtered = jobs.where((job) {
                  final title = (job['title'] ?? '').toString().toLowerCase();
                  final memberId = (job['postedByMemberId'] ?? '').toString();
                  return title.contains(searchQuery) || memberId.contains(searchQuery);
                }).toList();

                return RefreshIndicator(
                  color: AppTheme.royalGold,
                  onRefresh: () async {
                    HapticFeedback.mediumImpact();
                    ref.invalidate(pendingJobsProvider);
                  },
                  child: Column(
                    children: [
                      if (filtered.isNotEmpty)
                        Padding(
                          padding: const EdgeInsets.only(left: 24, bottom: 8),
                          child: Align(
                            alignment: Alignment.centerLeft,
                            child: Text(
                              'Showing ${filtered.length} of ${jobs.length} pending reviews',
                              style: TextStyle(fontSize: 10, color: AppTheme.royalGold.withValues(alpha: 0.7), fontWeight: FontWeight.bold),
                            ),
                          ),
                        ),
                      Expanded(
                        child: filtered.isEmpty
                          ? EmptyStateWidget(searchQuery.isEmpty ? 'No pending job postings found.' : 'No items match your search.', icon: Icons.work_outline_rounded)
                          : ListView.builder(
                              padding: const EdgeInsets.all(20.0),
                              itemCount: filtered.length,
                              itemBuilder: (context, index) {
                                final job = filtered[index];
                                return Padding(
                                  padding: const EdgeInsets.only(bottom: 12.0),
                                  child: GlassTile(
                                    icon: Icons.work_outline_rounded,
                                    title: job['title'] ?? 'Untitled Job',
                                    subtitle: 'By Member #${job['postedByMemberId'] ?? '—'} | ${job['company'] ?? 'GHCAA'}',
                                    trailing: Row(
                                      mainAxisSize: MainAxisSize.min,
                                      children: [
                                        IconButton(
                                          icon: const Icon(Icons.check_circle_outline, color: Colors.greenAccent, size: 24),
                                          onPressed: () => _handleResolve(context, ref, job['id'], true),
                                        ),
                                        IconButton(
                                          icon: const Icon(Icons.cancel_outlined, color: Colors.redAccent, size: 24),
                                          onPressed: () => _handleResolve(context, ref, job['id'], false),
                                        ),
                                      ],
                                    ),
                                    onTap: () {},
                                  ),
                                );
                              },
                            ),
                      ),
                    ],
                  ),
                );
              },
            ),
          ),
        ],
      ),
    );
  }

  Future<void> _handleResolve(BuildContext context, WidgetRef ref, int id, bool approve) async {
    String? reason;
    if (!approve) {
      reason = await showRejectReasonDialog(context, title: 'REJECT JOB POSTING');
      if (reason == null) return;
    }
    HapticFeedback.heavyImpact();
    final success = await ref.read(jobServiceProvider).resolveJobApproval(id, approve, reason: reason);
    if (success && context.mounted) {
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(approve ? 'Job approved.' : 'Job rejected.')));
      ref.invalidate(pendingJobsProvider);
    }
  }
}
