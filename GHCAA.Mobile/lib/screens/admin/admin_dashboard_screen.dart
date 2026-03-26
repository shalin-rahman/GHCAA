import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_tile.dart';
import '../../core/constants/app_constants.dart';
import '../../features/admin/admin_service.dart';

final adminAnalyticsProvider = FutureProvider<Map<String, dynamic>>((ref) async {
  return ref.read(adminServiceProvider).getGlobalAnalytics();
});

class AdminDashboardScreen extends ConsumerWidget {
  const AdminDashboardScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final analyticsAsync = ref.watch(adminAnalyticsProvider);

    return AppScaffold(
      isAdmin: true,
      title: 'Admin Command Center',
      actions: [
        IconButton(icon: const Icon(Icons.logout), onPressed: () => context.go('/')),
      ],
      bottomNavigationBar: _buildBottomNav(context),
      child: SingleChildScrollView(
        padding: const EdgeInsets.all(AppConstants.paddingLarge),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Global Analytics', style: Theme.of(context).textTheme.displayLarge?.copyWith(fontSize: 24)),
            const SizedBox(height: AppConstants.paddingMedium),
            analyticsAsync.when(
              data: (analytics) => Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  StatTile(title: 'Total Members', value: '${analytics['totalMembers'] ?? 0}'),
                  StatTile(title: 'Pending Approv.', value: '${analytics['pendingApprovals'] ?? 0}', highlight: true),
                ],
              ),
              loading: () => const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
              error: (e, s) => Center(child: Text('Error: $e')),
            ),
            const SizedBox(height: AppConstants.paddingExtraLarge),
            Text('Management Suites', style: Theme.of(context).textTheme.displayLarge?.copyWith(fontSize: 20)),
            const SizedBox(height: AppConstants.paddingMedium),
            GlassTile(icon: Icons.gavel, title: 'Membership Governance', subtitle: 'Approve/Reject enrollment requests', onTap: () => context.go('/admin/approvals')),
            GlassTile(icon: Icons.groups, title: 'Global Directory', subtitle: 'Manage all alumni records', onTap: () => context.go('/directory')),
            GlassTile(icon: Icons.receipt_long, title: 'Financial Ledger', subtitle: 'Track all contributions & dues', onTap: () => context.go('/admin/ledger')),
            GlassTile(icon: Icons.payments, title: 'Fee Structure', subtitle: 'Configure membership & admission costs', onTap: () => context.go('/admin/fees')),
            GlassTile(icon: Icons.support_agent, title: 'Support Desk', subtitle: 'Respond to member queries', onTap: () => context.go('/admin/messages')),
            GlassTile(icon: Icons.campaign, title: 'Communication Hub', subtitle: 'Push notifications & bulk SMS', onTap: () => context.go('/admin/communication')),
            GlassTile(icon: Icons.collections, title: 'Media CMS', subtitle: 'Manage gallery and news feed', onTap: () => context.go('/admin/cms')),
            GlassTile(icon: Icons.admin_panel_settings, title: 'Role Permissions', subtitle: 'Manage system access levels', onTap: () => context.go('/admin/themes')),
          ],
        ),
      ),
    );
  }

  Widget _buildBottomNav(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return BottomNavigationBar(
      backgroundColor: isDark ? AppTheme.adminMidnightSurface : AppTheme.adminDaylightSurface,
      selectedItemColor: AppTheme.royalGold,
      unselectedItemColor: isDark ? AppTheme.textSecondaryDark : AppTheme.textSecondaryLight,
      currentIndex: 0,
      onTap: (index) {
        if (index == 1) context.go('/admin/approvals');
        if (index == 2) context.go('/admin/audit');
      },
      type: BottomNavigationBarType.fixed,
      items: const [
        BottomNavigationBarItem(icon: Icon(Icons.admin_panel_settings), label: 'Console'),
        BottomNavigationBarItem(icon: Icon(Icons.pending_actions), label: 'Approvals'),
        BottomNavigationBarItem(icon: Icon(Icons.analytics), label: 'Audit'),
        BottomNavigationBarItem(icon: Icon(Icons.settings), label: 'Settings'),
      ],
    );
  }
}
