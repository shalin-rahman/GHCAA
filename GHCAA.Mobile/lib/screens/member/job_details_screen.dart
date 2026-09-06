import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/api/api_client.dart';
import '../../core/widgets/logo_spinner.dart';
import '../../features/auth/auth_service.dart';
import '../../core/widgets/confirm_dialog.dart';

final jobDetailsProvider = FutureProvider.family<Map<String, dynamic>?, int>((ref, jobId) async {
  try {
    final dio = ref.read(dioProvider);
    final response = await dio.get('/jobs/$jobId');
    return response.data as Map<String, dynamic>;
  } catch (e) {
    return null;
  }
});

class JobDetailsScreen extends ConsumerWidget {
  final int jobId;

  const JobDetailsScreen({super.key, required this.jobId});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final detailsAsync = ref.watch(jobDetailsProvider(jobId));
    final roleAsync = ref.watch(roleProvider);
    final isAdmin = roleAsync.value?.isStaffAdminRole ?? false;

    return AppScaffold(
      title: 'Job Details',
      breadcrumb: 'Portal > Jobs',
      actions: isAdmin ? [
        IconButton(
          icon: const Icon(Icons.edit, color: AppTheme.royalGold, size: 20),
          onPressed: () => detailsAsync.whenData((job) {
            if (job != null) _showEditDialog(context, ref, job);
          }),
        ),
        IconButton(
          icon: const Icon(Icons.delete_outline, color: Colors.redAccent, size: 20),
          onPressed: () => _confirmDelete(context, ref),
        ),
      ] : null,
      child: detailsAsync.when(
        data: (job) {
          if (job == null) return const Center(child: Text('Job posting not found.', style: TextStyle(color: Colors.red)));

          return SingleChildScrollView(
            padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 20),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                GlassContainer(
                   child: Column(
                     crossAxisAlignment: CrossAxisAlignment.start,
                     children: [
                        Text((job['title'] ?? 'Job Details').toUpperCase(), style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w900, letterSpacing: 1.2)),
                        const SizedBox(height: 8),
                        Text('COMPANY: ${job['company'] ?? 'TBA'}', style: Theme.of(context).textTheme.labelLarge),
                        const Divider(height: 48),
                        Text(job['description'] ?? 'No details available.', style: Theme.of(context).textTheme.bodyLarge?.copyWith(height: 1.6, color: Colors.white.withValues(alpha: 0.9))),
                        const SizedBox(height: 24),
                        _buildStatRow(context, 'Location', job['location'] ?? 'Remote/Hybrid'),
                        _buildStatRow(context, 'Employment Type', job['jobType'] ?? 'Full-Time'),
                        _buildStatRow(context, 'Salary', job['salaryRange'] ?? 'Negotiable'),
                        _buildStatRow(context, 'Experience', job['experienceRequired'] ?? 'Open'),
                        if (job['applicationLink'] != null) const Divider(height: 48),
                        if (job['applicationLink'] != null)
                          Center(
                            child: ElevatedButton.icon(
                              onPressed: () async {
                                final url = Uri.tryParse(job['applicationLink'].toString());
                                if (url != null && await canLaunchUrl(url)) {
                                  await launchUrl(url, mode: LaunchMode.externalApplication);
                                }
                              },
                              icon: const Icon(Icons.open_in_browser, size: 20),
                              label: const Text('APPLY'),
                            ),
                          ),
                      ],
                    )
                ),
              ],
            ),
          );
        },
        loading: () => const Center(child: LogoSpinner(size: 120)),
        error: (e, s) => Center(child: Text('Sync Error: $e', style: const TextStyle(color: Colors.red))),
      ),
    );
  }

  Widget _buildStatRow(BuildContext context, String label, String val) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12.0),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
           Text(label.toUpperCase(), style: Theme.of(context).textTheme.labelSmall?.copyWith(color: AppTheme.royalGold.withValues(alpha: 0.6), height: 1)),
           const SizedBox(width: 16),
           Expanded(
             child: Text(val, textAlign: TextAlign.end, style: Theme.of(context).textTheme.bodyMedium?.copyWith(color: Colors.white, fontWeight: FontWeight.bold), overflow: TextOverflow.ellipsis),
           ),
        ],
      )
    );
  }

  void _showEditDialog(BuildContext context, WidgetRef ref, Map<String, dynamic> job) {
    final titleCtrl = TextEditingController(text: job['title']);
    final descCtrl = TextEditingController(text: job['description']);
    bool saving = false;

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: AppTheme.deepCharcoal,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(24))),
      builder: (ctx) => StatefulBuilder(
        builder: (ctx, setStateModal) => Padding(
          padding: EdgeInsets.only(left: 24, right: 24, top: 24, bottom: MediaQuery.of(ctx).viewInsets.bottom + 32),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Text('EDIT JOB', style: Theme.of(context).textTheme.headlineMedium?.copyWith(fontSize: 16)),
              const SizedBox(height: 24),
              TextField(
                controller: titleCtrl,
                decoration: const InputDecoration(labelText: 'Job Title'),
              ),
              const SizedBox(height: 16),
              TextField(
                controller: descCtrl,
                maxLines: 4,
                decoration: const InputDecoration(labelText: 'Description'),
              ),
              const SizedBox(height: 32),
              ElevatedButton(
                onPressed: saving ? null : () async {
                  setStateModal(() => saving = true);
                  try {
                    final dio = ref.read(dioProvider);
                    await dio.put('/jobs/$jobId', data: {'title': titleCtrl.text, 'description': descCtrl.text});
                    ref.invalidate(jobDetailsProvider(jobId));
                    if (ctx.mounted) Navigator.pop(ctx);
                    if (context.mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Job posting updated.')));
                  } catch (e) {
                    if (context.mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Update failed: $e'), backgroundColor: Colors.redAccent));
                  } finally {
                    if (ctx.mounted) setStateModal(() => saving = false);
                  }
                },
                child: saving
                    ? SizedBox(height: 20, width: 20, child: LogoSpinner.small())
                    : const Text('SAVE CHANGES'),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Future<void> _confirmDelete(BuildContext context, WidgetRef ref) async {
    final confirm = await showConfirmDialog(
      context,
      title: 'Delete Job',
      message: 'Are you sure you want to delete this job posting? This action cannot be undone.',
      confirmLabel: 'Delete',
      destructive: true,
    );
    if (!confirm || !context.mounted) return;

    try {
      final dio = ref.read(dioProvider);
      await dio.delete('/jobs/$jobId');
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Job posting removed.')));
        context.pop();
      }
    } catch (e) {
      if (context.mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Delete failed: $e'), backgroundColor: Colors.redAccent));
    }
  }
}
