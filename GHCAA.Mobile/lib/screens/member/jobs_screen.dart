import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../features/jobs/job_service.dart';

final jobsListProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(jobServiceProvider).getAllJobs();
});

class JobsScreen extends ConsumerWidget {
  const JobsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final jobsAsync = ref.watch(jobsListProvider);

    return AppScaffold(
      title: 'Haragangian Careers',
      breadcrumb: 'Member Portal > Career Hub',
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () {
          HapticFeedback.mediumImpact();
          // Implementation for posting a job
        },
        backgroundColor: AppTheme.royalGold,
        icon: const Icon(Icons.add, color: Colors.black, size: 20),
        label: const Text('POST OPPORTUNITY', style: TextStyle(color: Colors.black, fontSize: 11, fontWeight: FontWeight.w900, letterSpacing: 1)),
      ),
      child: AsyncValueWidget<List<dynamic>>(
        value: jobsAsync,
        loadingMessage: 'Synchronizing global careers...',
        onRetry: () => ref.invalidate(jobsListProvider),
        data: (jobs) => RefreshIndicator(
          color: AppTheme.royalGold,
          onRefresh: () async {
            HapticFeedback.mediumImpact();
            ref.invalidate(jobsListProvider);
          },
          child: jobs.isEmpty 
            ? const Center(child: Text('No active career opportunities found in the registry.', style: TextStyle(color: AppTheme.textSecondaryDark)))
            : ListView.builder(
                padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                itemCount: jobs.length,
                itemBuilder: (context, index) {
                  final job = jobs[index];
                  return Padding(
                    padding: const EdgeInsets.only(bottom: 16.0),
                    child: GlassContainer(
                      padding: const EdgeInsets.all(20.0),
                      child: InkWell(
                        onTap: () {
                          HapticFeedback.lightImpact();
                          context.push('/jobs/${job['id']}');
                        },
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.stretch,
                          children: [
                            Row(
                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                              children: [
                                Container(
                                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                                  decoration: BoxDecoration(color: AppTheme.royalGold.withValues(alpha: 0.1), borderRadius: BorderRadius.circular(4)),
                                  child: Text(_getJobCategoryLabel(job['jobCategory']), style: const TextStyle(color: AppTheme.royalGold, fontSize: 8, fontWeight: FontWeight.w900, letterSpacing: 1.2)),
                                ),
                                Text(
                                  job['createdAt']?.toString().split('T')[0] ?? '', 
                                  style: const TextStyle(fontSize: 10, color: AppTheme.textSecondaryDark, fontWeight: FontWeight.bold)
                                ),
                              ],
                            ),
                            const SizedBox(height: 16),
                            Text(
                              job['title'] ?? 'Job Opportunity', 
                              style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 18, color: Colors.white, height: 1.2, letterSpacing: -0.2)
                            ),
                            const SizedBox(height: 6),
                            Text(
                              '${job['companyName'] ?? 'RECRUITER PENDING'} • ${job['location'] ?? 'REMOTE'}', 
                              style: const TextStyle(fontSize: 11, color: AppTheme.royalGold, fontWeight: FontWeight.bold, letterSpacing: 0.5)
                            ),
                            const SizedBox(height: 14),
                            Text(
                              job['description'] ?? 'Opportunity details not available.', 
                              style: const TextStyle(height: 1.6, fontSize: 13, color: AppTheme.textSecondaryDark),
                              maxLines: 2,
                              overflow: TextOverflow.ellipsis,
                            ),
                            const SizedBox(height: 24),
                            const Divider(color: Colors.white10, height: 1),
                            const SizedBox(height: 16),
                            Row(
                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                              children: [
                                Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Row(
                                      children: [
                                        Container(width: 18, height: 18, decoration: const BoxDecoration(color: AppTheme.royalGold, shape: BoxShape.circle), child: Center(child: Text(job['postedByMemberName']?[0] ?? 'A', style: const TextStyle(color: Colors.black, fontSize: 10, fontWeight: FontWeight.bold)))),
                                        const SizedBox(width: 8),
                                        Text('Posted by ${job['postedByMemberName'] ?? 'Alumni'}', style: const TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: Colors.white70)),
                                      ],
                                    ),
                                    if (job['applicationDeadline'] != null) ...[
                                      const SizedBox(height: 6),
                                      Text('DEADLINE: ${job['applicationDeadline'].toString().split('T')[0]}', style: const TextStyle(fontSize: 9, color: Colors.redAccent, fontWeight: FontWeight.w900, letterSpacing: 1)),
                                    ],
                                  ],
                                ),
                                const Text('DETAILS ↗', style: TextStyle(fontSize: 9, fontWeight: FontWeight.w900, color: AppTheme.royalGold, letterSpacing: 1.5)),
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
      ),
    );
  }

  String _getJobCategoryLabel(dynamic category) {
    if (category == null) return 'FULL-TIME';
    // Mapping identical to web constants
    switch (category.toString()) {
      case '0':
      case 'FullTime': return 'FULL-TIME';
      case '1':
      case 'PartTime': return 'PART-TIME';
      case '2':
      case 'Freelance': return 'FREELANCE';
      case '3':
      case 'Mentorship': return 'MENTORSHIP';
      default: return category.toString().toUpperCase();
    }
  }
}
