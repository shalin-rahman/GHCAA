import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/glass_tile.dart';
import '../../core/widgets/logo_spinner.dart';
import '../../features/admin/admin_service.dart';

final adminAnalyticsProvider = FutureProvider<Map<String, dynamic>>((ref) async {
  return ref.read(adminServiceProvider).getGlobalAnalytics();
});

class AdminDashboardScreen extends ConsumerStatefulWidget {
  const AdminDashboardScreen({super.key});

  @override
  ConsumerState<AdminDashboardScreen> createState() => _AdminDashboardState();
}

class _AdminDashboardState extends ConsumerState<AdminDashboardScreen> {
  int _currentIndex = 0;

  @override
  Widget build(BuildContext context) {
    final analyticsAsync = ref.watch(adminAnalyticsProvider);

    return AppScaffold(
      isAdmin: true,
      title: 'Dashboard',
      actions: [
        IconButton(icon: const Icon(Icons.notifications_outlined), onPressed: () {}),
        IconButton(icon: const Icon(Icons.logout), onPressed: () => context.go('/login')),
      ],
      bottomNavigationBar: _buildBottomNav(context),
      child: SingleChildScrollView(
        padding: const EdgeInsets.all(20.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text('OVERVIEW', 
              style: TextStyle(letterSpacing: 1.5, fontSize: 10, fontWeight: FontWeight.bold, color: AppTheme.royalGold)),
            const SizedBox(height: 16),
            analyticsAsync.when(
              data: (analytics) => Row(
                children: [
                  _buildStatCard('MEMBERS', '${analytics['totalMembers'] ?? 0}', Icons.people, Colors.blueAccent),
                  const SizedBox(width: 12),
                  _buildStatCard('APPROVALS', '${analytics['pendingApprovals'] ?? 0}', Icons.priority_high, Colors.orangeAccent),
                ],
              ),
              loading: () => const Center(child: LogoSpinner(size: 120)),
              error: (e, s) => Center(child: Text('Sync Error: $e')),
            ),
            const SizedBox(height: 32),
            const Text('ADMINISTRATIVE MODULES', 
              style: TextStyle(letterSpacing: 1.5, fontSize: 10, fontWeight: FontWeight.bold, color: AppTheme.royalGold)),
            const SizedBox(height: 16),
            GridView.count(
              crossAxisCount: 2,
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              mainAxisSpacing: 12,
              crossAxisSpacing: 12,
              childAspectRatio: 1.4,
              children: [
                _buildQuickAction(context, Icons.gavel, 'APPROVALS', '/admin/approvals'),
                _buildQuickAction(context, Icons.history_edu, 'AUDIT LOGS', '/admin/audit'),
                _buildQuickAction(context, Icons.receipt_long, 'FINANCIAL LEDGER', '/admin/ledger'),
                _buildQuickAction(context, Icons.campaign, 'COMMUNICATIONS', '/admin/messages'),
                _buildQuickAction(context, Icons.business, 'MEMBER DIRECTORY', '/directory'),
                _buildQuickAction(context, Icons.rate_review_outlined, 'ARTICLE REVIEW', '/admin/articles'),
                _buildQuickAction(context, Icons.photo_library_outlined, 'ALBUM REVIEW', '/admin/gallery-approvals'),
                _buildQuickAction(context, Icons.work_outline_rounded, 'JOB REVIEW', '/admin/job-approvals'),
                _buildQuickAction(context, Icons.qr_code_scanner, 'GATEKEEPER', '/admin/gatekeeper'),
              ],
            ),
            const SizedBox(height: 32),
            const Text('MANAGEMENT SYSTEM', 
              style: TextStyle(letterSpacing: 1.5, fontSize: 10, fontWeight: FontWeight.bold, color: AppTheme.royalGold)),
            const SizedBox(height: 12),
            GlassTile(icon: Icons.groups_outlined, title: 'EXECUTIVE COMMITTEE', subtitle: 'Manage Institutional Periods', onTap: () => context.go('/admin/governance')),
            GlassTile(icon: Icons.collections, title: 'CONTENT MANAGEMENT', subtitle: 'Manage news & gallery assets', onTap: () => context.go('/admin/cms')),
            GlassTile(icon: Icons.support_agent, title: 'CONTACT MESSAGES', subtitle: 'Global support queue', onTap: () => context.go('/admin/messages')),
          ],
        ),
      ),
    );
  }

  Widget _buildStatCard(String label, String value, IconData icon, Color accent) {
    return Expanded(
      child: GlassContainer(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Icon(icon, size: 20, color: accent),
            const SizedBox(height: 12),
            Text(value, style: const TextStyle(fontSize: 24, fontWeight: FontWeight.w900, color: Colors.white)),
            Text(label, style: const TextStyle(fontSize: 9, fontWeight: FontWeight.bold, color: AppTheme.textSecondaryDark)),
          ],
        ),
      ),
    );
  }

  Widget _buildQuickAction(BuildContext context, IconData icon, String label, String route) {
    return GestureDetector(
      onTap: () => context.go(route),
      child: GlassContainer(
        padding: const EdgeInsets.symmetric(vertical: 16),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(icon, size: 28, color: AppTheme.royalGold),
            const SizedBox(height: 8),
            Text(label, style: const TextStyle(fontSize: 12, fontWeight: FontWeight.bold, color: Colors.white)),
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
      currentIndex: _currentIndex,
      onTap: (index) {
        setState(() => _currentIndex = index);
        if (index == 1) context.go('/admin/approvals');
        if (index == 2) context.go('/admin/audit');
      },
      type: BottomNavigationBarType.fixed,
      items: const [
        BottomNavigationBarItem(icon: Icon(Icons.dashboard_outlined), label: 'Dashboard'),
        BottomNavigationBarItem(icon: Icon(Icons.task_alt), label: 'Approvals'),
        BottomNavigationBarItem(icon: Icon(Icons.analytics_outlined), label: 'Audit'),
        BottomNavigationBarItem(icon: Icon(Icons.account_tree_outlined), label: 'Network'),
      ],
    );
  }
}
