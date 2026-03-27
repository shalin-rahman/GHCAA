import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../features/admin/admin_service.dart';
import '../../features/auth/auth_service.dart';

final adminAnalyticsProvider =
    FutureProvider.autoDispose<Map<String, dynamic>>((ref) async {
  return ref.read(adminServiceProvider).getGlobalAnalytics();
});

class DashboardScreen extends ConsumerWidget {
  const DashboardScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final profileAsync = ref.watch(userProfileProvider);
    final roleAsync = ref.watch(roleProvider);

    return AppScaffold(
      showAppBar: false,
      child: profileAsync.when(
        data: (profile) {
          final fullName = profile?['fullName'] ?? 'Distinguished Alumnus';
          final firstName = fullName.split(' ').first;
          final isAdmin =
              roleAsync.value == 'Admin' || roleAsync.value == 'SuperAdmin';

          return CustomScrollView(
            slivers: [
              SliverToBoxAdapter(
                child: Padding(
                  padding: const EdgeInsets.symmetric(
                      horizontal: 24.0, vertical: 32.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Row(
                            children: [
                              IconButton(
                                  icon: const Icon(Icons.menu,
                                      color: AppTheme.royalGold),
                                  onPressed: () =>
                                      Scaffold.of(context).openDrawer()),
                              const SizedBox(width: 8),
                              const CircleAvatar(
                                radius: 20,
                                backgroundColor: Colors.black45,
                                child: Icon(Icons.person,
                                    color: AppTheme.royalGold),
                              ),
                            ],
                          ),
                          Container(
                            decoration: BoxDecoration(
                                color: Colors.redAccent.withValues(alpha: 0.1),
                                borderRadius: BorderRadius.circular(12)),
                            child: IconButton(
                              icon: const Icon(Icons.power_settings_new,
                                  color: Colors.redAccent, size: 22),
                              tooltip: 'Sign Out Session',
                              onPressed: () async {
                                HapticFeedback.heavyImpact();
                                final confirmed = await showDialog<bool>(
                                  context: context,
                                  builder: (ctx) => AlertDialog(
                                    backgroundColor: AppTheme.midnightSurface,
                                    title: const Text('End Session',
                                        style: TextStyle(color: Colors.white)),
                                    content: const Text(
                                        'Are you sure you want to securely exit the Haragangian Platform?',
                                        style: TextStyle(
                                            color: AppTheme.textSecondaryDark)),
                                    actions: [
                                      TextButton(
                                          onPressed: () =>
                                              Navigator.pop(ctx, false),
                                          child: const Text('CANCEL')),
                                      TextButton(
                                          onPressed: () =>
                                              Navigator.pop(ctx, true),
                                          child: const Text('LOGOUT',
                                              style: TextStyle(
                                                  color: Colors.redAccent,
                                                  fontWeight:
                                                      FontWeight.bold))),
                                    ],
                                  ),
                                );
                                if (confirmed == true && context.mounted) {
                                  // Capture router before async gap
                                  final router = GoRouter.of(context);
                                  await ref.read(authServiceProvider).logout();
                                  router.go('/login');
                                }
                              },
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 32),
                      const Text(
                        'WELCOME BACK,',
                        style: TextStyle(
                            color: AppTheme.royalGold,
                            fontSize: 10,
                            fontWeight: FontWeight.w900,
                            letterSpacing: 2.0),
                      ),
                      const SizedBox(height: 8),
                      Text(
                        firstName.toUpperCase(),
                        style: const TextStyle(
                            color: Colors.white,
                            fontSize: 32,
                            fontWeight: FontWeight.w900,
                            letterSpacing: 1.0),
                      ),

                      // ----- ADMIN EXCLUSIVE OVERRIDE -----
                      if (isAdmin) ...[
                        const SizedBox(height: 48),
                        const Text('SYSTEM HEALTH VERIFICATION',
                            style: TextStyle(
                                color: Colors.redAccent,
                                fontSize: 10,
                                fontWeight: FontWeight.w900,
                                letterSpacing: 1.5)),
                        const SizedBox(height: 16),
                        _buildAdminAnalyticsCluster(ref),
                        const SizedBox(height: 32),
                        const Text('ADMINISTRATION CONSOLE',
                            style: TextStyle(
                                color: AppTheme.textSecondaryDark,
                                fontSize: 10,
                                fontWeight: FontWeight.w900,
                                letterSpacing: 1.5)),
                        const SizedBox(height: 16),
                        _buildAdminGrid(context),
                        const SizedBox(height: 32),
                        const Divider(color: Colors.white10),
                      ],
                      // ------------------------------------

                      SizedBox(height: isAdmin ? 32 : 48),
                      const Text(
                        'EXECUTIVE ACTIONS',
                        style: TextStyle(
                            color: AppTheme.textSecondaryDark,
                            fontSize: 10,
                            fontWeight: FontWeight.w900,
                            letterSpacing: 1.5),
                      ),
                      const SizedBox(height: 16),
                      _buildMemberPremiumGrid(context),
                      const SizedBox(height: 32),
                      _buildPremiumBanner(context, isAdmin),
                      const SizedBox(height: 40),
                    ],
                  ),
                ),
              ),
            ],
          );
        },
        loading: () => const Center(
            child: CircularProgressIndicator(color: AppTheme.royalGold)),
        error: (err, stack) => Center(
            child: Text('Initialization Error: $err',
                style: const TextStyle(color: Colors.red))),
      ),
    );
  }

  Widget _buildAdminAnalyticsCluster(WidgetRef ref) {
    final analyticsAsync = ref.watch(adminAnalyticsProvider);
    return analyticsAsync.when(
      data: (analytics) => Row(
        children: [
          _buildAdminStatCard(
              'TOTAL ALUMNI',
              '${analytics['totalMembers'] ?? 0}',
              Icons.people_outline,
              Colors.blueAccent),
          const SizedBox(width: 12),
          _buildAdminStatCard(
              'ACTION PENDING',
              '${analytics['pendingApprovals'] ?? 0}',
              Icons.gavel,
              Colors.orangeAccent),
        ],
      ),
      loading: () => const LinearProgressIndicator(color: Colors.redAccent),
      error: (_, __) => const SizedBox(),
    );
  }

  Widget _buildAdminStatCard(
      String label, String value, IconData icon, Color accent) {
    return Expanded(
      child: GlassContainer(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Icon(icon, size: 20, color: accent),
            const SizedBox(height: 12),
            Text(value,
                style: const TextStyle(
                    fontSize: 24,
                    fontWeight: FontWeight.w900,
                    color: Colors.white)),
            Text(label,
                style: const TextStyle(
                    fontSize: 9,
                    fontWeight: FontWeight.bold,
                    color: AppTheme.textSecondaryDark)),
          ],
        ),
      ),
    );
  }

  Widget _buildAdminGrid(BuildContext context) {
    return GridView.count(
      crossAxisCount: 2,
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      mainAxisSpacing: 16,
      crossAxisSpacing: 16,
      childAspectRatio: 1.25,
      children: [
        _buildActionCard(context, Icons.gavel, 'Approvals',
            'Member verifications', '/admin/approvals',
            isRed: true),
        _buildActionCard(context, Icons.analytics_outlined, 'Audit Trail',
            'Global trace logs', '/admin/audit',
            isRed: true),
        _buildActionCard(context, Icons.receipt_long_outlined, 'Global Ledger',
            'Financial insights', '/admin/ledger',
            isRed: true),
        _buildActionCard(context, Icons.settings_display_outlined, 'UI Themes',
            'Platform config', '/admin/themes',
            isRed: true),
        _buildActionCard(context, Icons.edit_attributes_outlined, 'Fee config',
            'Registration dues', '/admin/fees',
            isRed: true),
      ],
    );
  }

  Widget _buildMemberPremiumGrid(BuildContext context) {
    return GridView.count(
      crossAxisCount: 2,
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      mainAxisSpacing: 16,
      crossAxisSpacing: 16,
      childAspectRatio: 1.1,
      children: [
        _buildActionCard(context, Icons.account_circle_outlined,
            'Identity Engine', 'Update profile', '/profile'),
        _buildActionCard(context, Icons.groups_outlined, 'Alumni Registry',
            'Infinite network', '/directory'),
        _buildActionCard(context, Icons.account_balance_wallet_outlined,
            'Fiscal Portal', 'Dues & payments', '/financials'),
        _buildActionCard(context, Icons.work_outline, 'Career Link',
            'Job opportunities', '/jobs'),
        _buildActionCard(context, Icons.event_note_outlined, 'Timeline',
            'Upcoming events', '/events'),
        _buildActionCard(context, Icons.my_library_books_outlined,
            'The Journal', 'Magazines & News', '/news'), // Fixed to /news
      ],
    );
  }

  Widget _buildActionCard(BuildContext context, IconData icon, String title,
      String subtitle, String route,
      {bool isRed = false}) {
    return GestureDetector(
      onTap: () {
        HapticFeedback.lightImpact();
        context.go(route);
      },
      child: GlassContainer(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Container(
              padding: const EdgeInsets.all(8),
              decoration: BoxDecoration(
                color: isRed
                    ? Colors.redAccent.withValues(alpha: 0.1)
                    : AppTheme.royalGold.withValues(alpha: 0.1),
                borderRadius: BorderRadius.circular(8),
              ),
              child: Icon(icon,
                  color: isRed ? Colors.redAccent : AppTheme.royalGold,
                  size: 24),
            ),
            const Spacer(),
            Text(
              title,
              style: const TextStyle(
                  color: Colors.white,
                  fontSize: 13,
                  fontWeight: FontWeight.bold,
                  letterSpacing: 0.5),
            ),
            const SizedBox(height: 4),
            Text(
              subtitle,
              style: TextStyle(
                  color: AppTheme.textSecondaryDark.withValues(alpha: 0.8),
                  fontSize: 9,
                  fontWeight: FontWeight.normal),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildPremiumBanner(BuildContext context, bool isAdmin) {
    return GlassContainer(
      padding: EdgeInsets.zero,
      child: Container(
        padding: const EdgeInsets.all(24),
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(16),
          border: Border.all(
              color: isAdmin
                  ? Colors.redAccent.withValues(alpha: 0.3)
                  : AppTheme.royalGold.withValues(alpha: 0.3)),
          gradient: LinearGradient(
            colors: [
              isAdmin
                  ? Colors.redAccent.withValues(alpha: 0.15)
                  : AppTheme.royalGold.withValues(alpha: 0.1),
              Colors.transparent
            ],
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
          ),
        ),
        child: Row(
          children: [
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('PLATFORM AUTHORITY',
                      style: TextStyle(
                          color:
                              isAdmin ? Colors.redAccent : AppTheme.royalGold,
                          fontSize: 10,
                          fontWeight: FontWeight.bold,
                          letterSpacing: 1.5)),
                  const SizedBox(height: 8),
                  Text(isAdmin ? 'System Administrator' : 'Verified Executive',
                      style: const TextStyle(
                          color: Colors.white,
                          fontSize: 16,
                          fontWeight: FontWeight.w900)),
                ],
              ),
            ),
            Icon(Icons.verified,
                color: isAdmin
                    ? Colors.redAccent
                    : AppTheme.royalGold.withValues(alpha: 0.8),
                size: 40),
          ],
        ),
      ),
    );
  }
}
