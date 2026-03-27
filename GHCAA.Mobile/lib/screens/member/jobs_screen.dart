import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/glass_container.dart';
import '../../features/jobs/job_service.dart';

final jobsListProvider = FutureProvider<List<dynamic>>((ref) async {
  return ref.read(jobServiceProvider).getAllJobs();
});

class JobsScreen extends ConsumerWidget {
  const JobsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final jobsAsync = ref.watch(jobsListProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return Scaffold(
      appBar: AppBar(title: const Text('Job Hub'), backgroundColor: Colors.transparent, elevation: 0),
      body: Container(
        decoration: BoxDecoration(
          gradient: RadialGradient(
            center: const Alignment(0, -1.0),
            radius: 1.5,
            colors: isDark 
              ? [AppTheme.midnightSurface, AppTheme.midnightBase]
              : [AppTheme.daylightSurface, AppTheme.daylightBase],
          ),
        ),
        child: SafeArea(
          child: jobsAsync.when(
            data: (jobs) => jobs.isEmpty 
              ? const Center(child: Text('No job postings found.'))
              : ListView.builder(
                  padding: const EdgeInsets.all(16),
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
                                Text(job['companyName'] ?? 'Company', style: Theme.of(context).textTheme.bodySmall?.copyWith(color: AppTheme.royalGold, fontWeight: FontWeight.bold)),
                                Text(job['createdAt'] ?? '', style: const TextStyle(fontSize: 10)),
                              ],
                            ),
                            const SizedBox(height: 12),
                            Text(
                              job['title'] ?? 'Job Opportunity',
                              style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold, fontSize: 18),
                            ),
                            const SizedBox(height: 8),
                            Text(
                              job['description'] ?? 'No description available.',
                              style: const TextStyle(height: 1.4, fontSize: 13),
                            ),
                            const SizedBox(height: 20),
                            Row(
                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                              children: [
                                Row(
                                  children: [
                                    const Icon(Icons.location_on, size: 14, color: AppTheme.royalGold),
                                    const SizedBox(width: 4),
                                    Text(job['location'] ?? 'N/A', style: Theme.of(context).textTheme.bodySmall),
                                    const SizedBox(width: 16),
                                    const Icon(Icons.work, size: 14, color: AppTheme.royalGold),
                                    const SizedBox(width: 4),
                                    Text(job['type'] ?? 'Full-time', style: Theme.of(context).textTheme.bodySmall),
                                  ],
                                ),
                                ElevatedButton(
                                  onPressed: () {},
                                  style: ElevatedButton.styleFrom(
                                    backgroundColor: AppTheme.royalGold.withValues(alpha: 0.1),
                                    foregroundColor: AppTheme.royalGold,
                                    side: const BorderSide(color: AppTheme.royalGold),
                                    padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 8),
                                  ),
                                  child: const Text('Apply'),
                                ),
                              ],
                            ),
                          ],
                        ),
                      ),
                    );
                  },
                ),
            loading: () => const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
            error: (e, s) => Center(child: Text('Error loading jobs: $e')),
          ),
        ),
      ),
    );
  }
}
