import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
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
      title: 'Global Career Hub',
      child: AsyncValueWidget<List<dynamic>>(
        value: jobsAsync,
        loadingMessage: 'Synchronizing global careers...',
        onRetry: () => ref.invalidate(jobsListProvider),
        data: (jobs) => RefreshIndicator(
          color: AppTheme.royalGold,
          onRefresh: () async => ref.invalidate(jobsListProvider),
          child: jobs.isEmpty 
            ? const Center(child: Text('No active career opportunities found.'))
            : ListView.builder(
                padding: const EdgeInsets.all(20),
                itemCount: jobs.length,
                itemBuilder: (context, index) {
                  final job = jobs[index];
                  return Padding(
                    padding: const EdgeInsets.only(bottom: 16.0),
                    child: GlassContainer(
                      padding: const EdgeInsets.all(20.0),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: [
                          Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              Text(job['companyName'] ?? 'Recruiter', style: const TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.bold, fontSize: 11)),
                              Text(job['createdAt'] ?? '', style: const TextStyle(fontSize: 10, color: AppTheme.textSecondaryDark)),
                            ],
                          ),
                          const SizedBox(height: 12),
                          Text(job['title'] ?? 'Job Opportunity', style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 18, color: Colors.white)),
                          const SizedBox(height: 8),
                          Text(job['description'] ?? 'Opportunity details not available.', style: const TextStyle(height: 1.5, fontSize: 13, color: AppTheme.textSecondaryDark)),
                          const SizedBox(height: 24),
                          Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              Row(
                                children: [
                                  const Icon(Icons.location_on, size: 14, color: AppTheme.royalGold),
                                  const SizedBox(width: 4),
                                  Text(job['location'] ?? 'Remote', style: const TextStyle(fontSize: 12, color: Colors.white70)),
                                  const SizedBox(width: 16),
                                  const Icon(Icons.work, size: 14, color: AppTheme.royalGold),
                                  const SizedBox(width: 4),
                                  Text(job['type'] ?? 'Full-time', style: const TextStyle(fontSize: 12, color: Colors.white70)),
                                ],
                              ),
                              ElevatedButton(
                                onPressed: () {
                                  ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Forwarding application profile...')));
                                },
                                style: ElevatedButton.styleFrom(
                                  backgroundColor: AppTheme.royalGold.withValues(alpha: 0.1),
                                  foregroundColor: AppTheme.royalGold,
                                  side: const BorderSide(color: AppTheme.royalGold),
                                  padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 8),
                                ),
                                child: const Text('APPLY'),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),
                  );
                },
              ),
        ),
      ),
    );
  }
}
