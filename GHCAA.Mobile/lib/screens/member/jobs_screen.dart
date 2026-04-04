import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../features/jobs/job_service.dart';
import '../../features/auth/auth_service.dart';

final jobSearchQueryProvider = StateProvider.autoDispose<String>((ref) => "");

final jobsListProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(jobServiceProvider).getAllJobs();
});

class JobsScreen extends ConsumerStatefulWidget {
  const JobsScreen({super.key});

  @override
  ConsumerState<JobsScreen> createState() => _JobsScreenState();
}

class _JobsScreenState extends ConsumerState<JobsScreen> {
  final TextEditingController _searchController = TextEditingController();

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final jobsAsync = ref.watch(jobsListProvider);
    final searchQuery = ref.watch(jobSearchQueryProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final selfProfile = ref.watch(userProfileProvider).value;
    final myMemberId = selfProfile?['id'];

    return AppScaffold(
      title: 'Opportunities Hub',
      breadcrumb: 'PORTAL > JOB HUB',
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () {
          HapticFeedback.mediumImpact();
          _showJobFormDialog(context, ref);
        },
        backgroundColor: AppTheme.royalGold,
        icon: const Icon(Icons.add, color: Colors.black, size: 20),
        label: const Text('SHARE OPPORTUNITY', style: TextStyle(color: Colors.black, fontSize: 11, fontWeight: FontWeight.w900, letterSpacing: 1)),
      ),
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Search career hub...',
                prefixIcon: const Icon(Icons.search, size: 20, color: AppTheme.royalGold),
                suffixIcon: _searchController.text.isNotEmpty 
                  ? IconButton(
                      icon: const Icon(Icons.close_rounded, size: 20, color: Colors.white54),
                      onPressed: () {
                        _searchController.clear();
                        ref.read(jobSearchQueryProvider.notifier).state = "";
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
              onChanged: (v) => ref.read(jobSearchQueryProvider.notifier).state = v.toLowerCase(),
            ),
          ),
          Expanded(
            child: AsyncValueWidget<List<dynamic>>(
              value: jobsAsync,
              loadingMessage: 'Loading jobs...',
              onRetry: () => ref.invalidate(jobsListProvider),
              data: (jobs) {
                final filtered = jobs.where((job) {
                  final title = job['title']?.toString().toLowerCase() ?? '';
                  final company = job['companyName']?.toString().toLowerCase() ?? '';
                  return title.contains(searchQuery) || company.contains(searchQuery);
                }).toList();

                return RefreshIndicator(
                  color: AppTheme.royalGold,
                  onRefresh: () async {
                    HapticFeedback.mediumImpact();
                    ref.invalidate(jobsListProvider);
                  },
                  child: Column(
                    children: [
                      if (filtered.isNotEmpty)
                        Padding(
                          padding: const EdgeInsets.only(left: 24, bottom: 8),
                          child: Align(
                            alignment: Alignment.centerLeft,
                            child: Text(
                              'Showing ${filtered.length} of ${jobs.length} listed opportunities',
                              style: TextStyle(fontSize: 10, color: AppTheme.royalGold.withValues(alpha: 0.7), fontWeight: FontWeight.bold),
                            ),
                          ),
                        ),
                      Expanded(
                        child: filtered.isEmpty 
                          ? Center(child: Text(searchQuery.isEmpty ? 'No active opportunities found.' : 'No opportunities match your search.', style: const TextStyle(color: AppTheme.textSecondaryDark)))
                          : ListView.builder(
                              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                              itemCount: filtered.length,
                              itemBuilder: (context, index) {
                                final job = filtered[index];
                                final isMine = myMemberId != null && job['memberId'] == myMemberId;

                                return Padding(
                                  padding: const EdgeInsets.only(bottom: 16.0),
                                  child: GlassContainer(
                                    padding: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 12.0),
                                    child: InkWell(
                                      onTap: () {
                                        HapticFeedback.lightImpact();
                                        context.push('/jobs/${job['id']}');
                                      },
                                      child: Column(
                                        crossAxisAlignment: CrossAxisAlignment.start,
                                        children: [
                                          Row(
                                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                            children: [
                                              Expanded(
                                                child: Text(job['title'] ?? 'Position Title Missing',
                                                    style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 13, color: Colors.white, letterSpacing: -0.2)),
                                              ),
                                              if (isMine)
                                                IconButton(
                                                  icon: const Icon(Icons.delete_outline, color: Colors.redAccent, size: 18),
                                                  onPressed: () async {
                                                     final confirm = await showDialog<bool>(
                                                       context: context,
                                                       builder: (ctx) => AlertDialog(
                                                         backgroundColor: AppTheme.midnightSurface,
                                                         title: const Text('Delete Post?', style: TextStyle(color: Colors.white, fontSize: 14)),
                                                         actions: [
                                                           TextButton(onPressed: () => Navigator.pop(ctx, false), child: const Text('CANCEL')),
                                                           TextButton(onPressed: () => Navigator.pop(ctx, true), child: const Text('DELETE', style: TextStyle(color: Colors.redAccent))),
                                                         ],
                                                       ),
                                                     );
                                                     if (confirm == true) {
                                                       await ref.read(jobServiceProvider).deleteJob(job['id']);
                                                       ref.invalidate(jobsListProvider);
                                                     }
                                                  },
                                                )
                                              else
                                                const Icon(Icons.arrow_forward_ios_rounded, size: 10, color: AppTheme.royalGold),
                                            ],
                                          ),
                                          const SizedBox(height: 1),
                                          Text(job['companyName'] ?? 'Organization Undisclosed',
                                              style: const TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.bold, letterSpacing: 0.5)),
                                          const SizedBox(height: 10),
                                          const Divider(color: Colors.white10, height: 1),
                                          const SizedBox(height: 10),
                                          Row(
                                            children: [
                                              _buildMiniBadge(Icons.location_on_outlined, job['location'] ?? 'Remote'),
                                              const SizedBox(width: 12),
                                              _buildMiniBadge(Icons.work_outline, job['jobType'] ?? 'Full-time'),
                                              const Spacer(),
                                              Text('Deadline: ${job['deadlineDate']?.toString().split('T')[0] ?? 'TBD'}',
                                                  style: const TextStyle(color: Colors.white38, fontSize: 8.5, fontWeight: FontWeight.bold)),
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
                  ),
                );
              },
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildMiniBadge(IconData icon, String label) {
    return Row(
      children: [
        Icon(icon, size: 11, color: AppTheme.royalGold.withValues(alpha: 0.7)),
        const SizedBox(width: 4),
        Text(label.toUpperCase(), style: const TextStyle(color: AppTheme.textSecondaryDark, fontSize: 8.5, fontWeight: FontWeight.bold, letterSpacing: 0.2)),
      ],
    );
  }

  void _showJobFormDialog(BuildContext context, WidgetRef ref) {
    final titleCtrl = TextEditingController();
    final companyCtrl = TextEditingController();
    final locCtrl = TextEditingController();
    final linkCtrl = TextEditingController();
    final descCtrl = TextEditingController();

    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: AppTheme.midnightSurface,
        title: const Text('SHARE OPPORTUNITY', style: TextStyle(color: AppTheme.royalGold, fontSize: 14, fontWeight: FontWeight.w900, letterSpacing: 1.5)),
        content: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              _buildField('Job Title', titleCtrl),
              _buildField('Organization', companyCtrl),
              _buildField('Location', locCtrl),
              _buildField('External Link', linkCtrl),
              _buildField('Short Description', descCtrl, maxLines: 3),
            ],
          ),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('CANCEL', style: TextStyle(color: Colors.white54))),
          ElevatedButton(
            onPressed: () async {
              try {
                final payload = {
                  'title': titleCtrl.text,
                  'companyName': companyCtrl.text,
                  'location': locCtrl.text,
                  'externalUrl': linkCtrl.text,
                  'description': descCtrl.text,
                  'postedAt': DateTime.now().toIso8601String(),
                  'isActive': true,
                };
                final success = await ref.read(jobServiceProvider).postJob(payload);
                if (success) {
                  ref.invalidate(jobsListProvider);
                  if (ctx.mounted) Navigator.pop(ctx);
                }
              } catch (_) {}
            },
            child: const Text('POST HUB', style: TextStyle(color: Colors.black, fontWeight: FontWeight.bold)),
          ),
        ],
      ),
    );
  }

  Widget _buildField(String label, TextEditingController ctrl, {int maxLines = 1}) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12.0),
      child: TextField(
        controller: ctrl,
        maxLines: maxLines,
        style: const TextStyle(color: Colors.white, fontSize: 13),
        decoration: InputDecoration(
          labelText: label,
          labelStyle: const TextStyle(color: Colors.white38, fontSize: 11),
          enabledBorder: const UnderlineInputBorder(borderSide: BorderSide(color: Colors.white10)),
        ),
      ),
    );
  }
}
