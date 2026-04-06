import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../features/admin/admin_service.dart';
import '../../features/auth/auth_service.dart';
import '../../core/config/app_config.dart';
import '../../core/storage/storage_service.dart';

// Persistent Layout State
final dashboardLayoutProvider = StateNotifierProvider<DashboardLayoutNotifier, bool>((ref) {
  final storage = ref.read(storageServiceProvider);
  return DashboardLayoutNotifier(storage);
});

class DashboardLayoutNotifier extends StateNotifier<bool> {
  final StorageService _storage;
  DashboardLayoutNotifier(this._storage) : super(false) {
    _load();
  }

  Future<void> _load() async {
    state = await _storage.getDashboardLayout();
  }

  Future<void> toggle() async {
    state = !state;
    await _storage.saveDashboardLayout(state);
  }
}

final adminAnalyticsProvider =
    FutureProvider.autoDispose<Map<String, dynamic>>((ref) async {
  return ref.read(adminServiceProvider).getGlobalAnalytics();
});

class DashboardScreen extends ConsumerWidget {
  const DashboardScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final profileAsync = ref.watch(userProfileProvider);
    final isCompact = ref.watch(dashboardLayoutProvider);

    return AppScaffold(
      showAppBar: false,
      child: profileAsync.when(
        data: (profile) {
          final roleAsync = ref.watch(roleProvider);
          final isAdmin = roleAsync.value?.isStaffAdminRole ?? false;
          final fullName = profile?['fullName'] ?? 'Distinguished Alumnus';
          
          final photoUrl = AppConfig.resolveImageUrl(profile?['photoPath']);

          return CustomScrollView(
            slivers: [
              SliverToBoxAdapter(
                child: Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 20.0, vertical: 24.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // Top Row with App Logo and Hamburger
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Builder(builder: (ctx) => GestureDetector(
                            onTap: () {
                              HapticFeedback.selectionClick();
                              Scaffold.of(ctx).openDrawer();
                            },
                            child: Row(
                              children: [
                                Image.asset('assets/logo.png', height: 40),
                                const SizedBox(width: 8),
                                const Icon(Icons.menu_rounded, color: AppTheme.royalGold, size: 28),
                              ],
                            ),
                          )),
                          Row(
                            children: [
                              IconButton(
                                  icon: Icon(isCompact ? Icons.grid_view_rounded : Icons.view_compact_rounded, 
                                    color: AppTheme.royalGold.withValues(alpha: 0.5), size: 20),
                                  tooltip: 'Toggle Layout Density',
                                  onPressed: () {
                                    HapticFeedback.mediumImpact();
                                    ref.read(dashboardLayoutProvider.notifier).toggle();
                                  },
                                ),
                                Container(
                                  margin: const EdgeInsets.only(left: 8),
                                  decoration: BoxDecoration(
                                      color: Colors.redAccent.withValues(alpha: 0.1),
                                      borderRadius: BorderRadius.circular(12),
                                      border: Border.all(color: Colors.redAccent.withValues(alpha: 0.2)),
                                  ),
                                child: IconButton(
                                  icon: const Icon(Icons.power_settings_new, color: Colors.redAccent, size: 18),
                                  onPressed: () => _handleLogout(context, ref),
                                ),
                              ),
                            ],
                          ),
                        ],
                      ),
                      const SizedBox(height: 24),
                      
                      // Majestic Profile Banner (More compact version)
                      _buildMajesticBanner(fullName, profile, photoUrl),
                      const SizedBox(height: 16),
                      _buildCompletenessCheck(context, profile),
                      const SizedBox(height: 24),

                      // ----- ADMIN SECTION -----
                      if (isAdmin) ...[
                        _buildCategoryHeader('ADMINISTRATIVE CONTROLS', Colors.redAccent),
                        _buildAdminAnalyticsCluster(ref),
                        const SizedBox(height: 16),
                        _buildAdminGrid(context, isCompact),
                        const SizedBox(height: 24),
                        const Divider(color: Colors.white10),
                        const SizedBox(height: 24),
                      ],
                      // ------------------------------------

                      _buildCategoryHeader('DASHBOARD NAVIGATION', AppTheme.royalGold),
                        _buildResponsiveGrid(context, isCompact, [
                        _buildActionCard(context, Icons.account_circle_outlined, 'Digital ID', 'Profile', '/digital_id', isCompact: isCompact),
                        _buildActionCard(context, Icons.groups_outlined, 'Member Directory', 'Directory', '/directory', isCompact: isCompact),
                        _buildActionCard(context, Icons.account_balance_wallet_outlined, 'Payments', 'Dues', '/financials', isCompact: isCompact, accentColor: Colors.tealAccent),
                        _buildActionCard(context, Icons.work_outline, 'Jobs', 'Listings', '/jobs', isCompact: isCompact),
                        _buildActionCard(context, Icons.event_available_outlined, 'Events', 'Announcements', '/events', isCompact: isCompact),
                        _buildActionCard(context, Icons.newspaper_outlined, 'News', 'Feed', '/news', isCompact: isCompact, accentColor: Colors.purpleAccent),
                        _buildActionCard(context, Icons.photo_library_outlined, 'Gallery', 'Photos', '/gallery', isCompact: isCompact, accentColor: Colors.purpleAccent),
                        _buildActionCard(context, Icons.corporate_fare_outlined, 'EC Committee', 'Members', '/committee', isCompact: isCompact),
                        _buildActionCard(context, Icons.history_outlined, 'Activity Log', 'Logs', '/activity', isCompact: isCompact),
                        _buildActionCard(context, Icons.notifications_active_outlined, 'Notifications', 'Alerts', '/notifications', isCompact: isCompact, accentColor: Colors.orangeAccent),
                        _buildActionCard(context, Icons.support_agent_outlined, 'Support', 'Helpdesk', '/support', isCompact: isCompact),
                        _buildActionCard(context, Icons.info_outline, 'About GHCAA', 'Info', '/about', isCompact: isCompact),
                      ]),
                      
                      const SizedBox(height: 40),
                      _buildPremiumBadge(isAdmin),
                      const SizedBox(height: 60),
                    ],
                  ),
                ),
              ),
            ],
          );
        },
        loading: () => const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
        error: (err, stack) => Center(child: Text('Initialization Error: $err', style: const TextStyle(color: Colors.red))),
      ),
    );
  }

  Widget _buildCompletenessCheck(BuildContext context, Map<String, dynamic>? profile) {
    if (profile == null) return const SizedBox();
    final double completeness = _calculateCompleteness(profile);
    if (completeness >= 0.95) return const SizedBox();

    return GestureDetector(
      onTap: () {
        HapticFeedback.lightImpact();
        context.push('/profile');
      },
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
        decoration: BoxDecoration(
          color: Colors.amberAccent.withValues(alpha: 0.1),
          borderRadius: BorderRadius.circular(16),
          border: Border.all(color: Colors.amberAccent.withValues(alpha: 0.2)),
        ),
        child: Row(
          children: [
             const Icon(Icons.tips_and_updates_outlined, color: Colors.amberAccent, size: 20),
             const SizedBox(width: 12),
             Expanded(
               child: Column(
                 crossAxisAlignment: CrossAxisAlignment.start,
                 children: [
                   const Text('PROFILE INCOMPLETE', style: TextStyle(color: Colors.amberAccent, fontSize: 8, fontWeight: FontWeight.w900, letterSpacing: 1)),
                   const SizedBox(height: 2),
                   Text('Your profile is ${(completeness * 100).toInt()}% complete. Please update your information for full access.', 
                     style: const TextStyle(color: Colors.white70, fontSize: 10, fontWeight: FontWeight.w500)),
                 ],
               ),
             ),
             const Icon(Icons.arrow_forward_ios_rounded, size: 12, color: Colors.amberAccent),
          ],
        ),
      ),
    );
  }

  double _calculateCompleteness(Map<String, dynamic>? profile) {
    return calculateProfileCompleteness(profile);
  }

  Widget _buildMajesticBanner(String fullName, Map<String, dynamic>? profile, String? photoUrl) {
    final membershipType = profile?['membershipType']?.toString() ?? '';
    final membershipCategory = profile?['category']?.toString() ?? '';
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(24),
        border: Border.all(color: AppTheme.royalGold.withValues(alpha: 0.15)),
        gradient: LinearGradient(
          colors: [AppTheme.royalGold.withValues(alpha: 0.12), Colors.black.withValues(alpha: 0.3)],
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
                const Text('AUTHORIZED ACCESS', style: TextStyle(color: AppTheme.royalGold, fontSize: 8, fontWeight: FontWeight.bold, letterSpacing: 2)),
                const SizedBox(height: 4),
                if (membershipType.isNotEmpty)
                  Container(
                    margin: const EdgeInsets.only(bottom: 4),
                    padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                    decoration: BoxDecoration(
                      color: AppTheme.royalGold.withValues(alpha: 0.15),
                      borderRadius: BorderRadius.circular(4),
                      border: Border.all(color: AppTheme.royalGold.withValues(alpha: 0.3)),
                    ),
                    child: Text(membershipType.toUpperCase(), style: const TextStyle(color: AppTheme.royalGold, fontSize: 8, fontWeight: FontWeight.w900, letterSpacing: 1)),
                  ),
                Text(fullName.toUpperCase(), style: const TextStyle(color: Colors.white, fontSize: 18, fontWeight: FontWeight.w900, letterSpacing: -0.3), maxLines: 2, overflow: TextOverflow.ellipsis),
                const SizedBox(height: 8),
                Row(
                  children: [
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                      decoration: BoxDecoration(color: Colors.black45, borderRadius: BorderRadius.circular(8), border: Border.all(color: Colors.white.withValues(alpha: 0.05))),
                      child: Text('ID: ${profile?['membershipNumber'] ?? 'N/A'}', style: const TextStyle(color: Colors.white70, fontSize: 9, fontWeight: FontWeight.bold, letterSpacing: 1)),
                    ),
                    if (membershipCategory.isNotEmpty && membershipCategory != 'None') ...[
                      const SizedBox(width: 6),
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                        decoration: BoxDecoration(
                          color: Colors.blueAccent.withValues(alpha: 0.15),
                          borderRadius: BorderRadius.circular(8),
                          border: Border.all(color: Colors.blueAccent.withValues(alpha: 0.3)),
                        ),
                        child: Text(membershipCategory.toUpperCase(), style: const TextStyle(color: Colors.blueAccent, fontSize: 9, fontWeight: FontWeight.bold, letterSpacing: 0.5)),
                      ),
                    ],
                  ],
                ),
              ],
            ),
          ),
          Container(
            width: 60, height: 60,
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              border: Border.all(color: AppTheme.royalGold, width: 1.5),
              image: photoUrl != null ? DecorationImage(image: NetworkImage(photoUrl), fit: BoxFit.cover) : null,
              boxShadow: [BoxShadow(color: AppTheme.royalGold.withValues(alpha: 0.1), blurRadius: 10)],
            ),
            child: photoUrl == null ? const Icon(Icons.person, color: AppTheme.royalGold, size: 28) : null,
          )
        ],
      ),
    );
  }

  Widget _buildCategoryHeader(String title, Color color) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Row(
        children: [
          Container(width: 3, height: 12, decoration: BoxDecoration(color: color, borderRadius: BorderRadius.circular(2))),
          const SizedBox(width: 8),
          Text(title, style: TextStyle(color: color.withValues(alpha: 0.8), fontSize: 10, fontWeight: FontWeight.w900, letterSpacing: 1.5)),
        ],
      ),
    );
  }

  Widget _buildResponsiveGrid(BuildContext context, bool isCompact, List<Widget> children) {
    return GridView.count(
      crossAxisCount: isCompact ? 3 : 2,
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      mainAxisSpacing: isCompact ? 10 : 14,
      crossAxisSpacing: isCompact ? 10 : 14,
      childAspectRatio: isCompact ? 1.0 : 1.25,
      children: children,
    );
  }

  Widget _buildAdminAnalyticsCluster(WidgetRef ref) {
    final analyticsAsync = ref.watch(adminAnalyticsProvider);
    return analyticsAsync.when(
      data: (analytics) => Row(
        children: [
          _buildCompactStatCard('TOTAL MEMBERS', '${analytics['totalMembers'] ?? 0}', Colors.blueAccent),
          const SizedBox(width: 8),
          _buildCompactStatCard('PENDING APPROVALS', '${analytics['pendingApprovals'] ?? 0}', Colors.orangeAccent),
        ],
      ),
      loading: () => const LinearProgressIndicator(color: Colors.redAccent, minHeight: 1),
      error: (_, __) => const SizedBox(),
    );
  }

  Widget _buildCompactStatCard(String label, String value, Color color) {
    return Expanded(
      child: Container(
        padding: const EdgeInsets.all(12),
        decoration: BoxDecoration(
          color: color.withValues(alpha: 0.05),
          borderRadius: BorderRadius.circular(16),
          border: Border.all(color: color.withValues(alpha: 0.1)),
        ),
        child: Column(
          children: [
            Text(value, style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w900, color: Colors.white)),
            Text(label, style: TextStyle(fontSize: 8, fontWeight: FontWeight.bold, color: color, letterSpacing: 0.5)),
          ],
        ),
      ),
    );
  }

  Widget _buildAdminGrid(BuildContext context, bool isCompact) {
    return _buildResponsiveGrid(context, isCompact, [
      _buildActionCard(context, Icons.dashboard_customize_outlined, 'Admin Panel', 'Dashboard', '/admin_dashboard', accentColor: Colors.redAccent, isCompact: isCompact),
      _buildActionCard(context, Icons.gavel, 'Approvals', 'Member Requests', '/admin/approvals', accentColor: Colors.redAccent, isCompact: isCompact),
      _buildActionCard(context, Icons.message_outlined, 'Messages', 'Dispatch', '/admin/messages', accentColor: Colors.redAccent, isCompact: isCompact),
      _buildActionCard(context, Icons.analytics_outlined, 'Audit Log', 'Sys Log', '/admin/audit', accentColor: Colors.redAccent, isCompact: isCompact),
      _buildActionCard(context, Icons.manage_accounts_outlined, 'Permissions', 'Access Matrix', '/admin/permissions', accentColor: Colors.deepOrangeAccent, isCompact: isCompact),
    ]);
  }

  Widget _buildActionCard(BuildContext context, IconData icon, String title, String subtitle, String route, {Color? accentColor, required bool isCompact}) {
    final color = accentColor ?? AppTheme.royalGold;
    return GestureDetector(
      onTap: () {
        HapticFeedback.lightImpact();
        context.push(route);
      },
      child: Card(
        margin: EdgeInsets.zero,
        child: Container(
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(16),
            gradient: LinearGradient(
              colors: [
                color.withValues(alpha: 0.08),
                Colors.black.withValues(alpha: 0.0),
              ],
              begin: Alignment.topLeft,
              end: Alignment.bottomRight,
            ),
          ),
          child: Padding(
            padding: EdgeInsets.all(isCompact ? 10.0 : 16.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Container(
                  padding: const EdgeInsets.all(8),
                  decoration: BoxDecoration(
                    color: color.withValues(alpha: 0.1),
                    shape: BoxShape.circle,
                  ),
                  child: Icon(icon, color: color, size: isCompact ? 18 : 22),
                ),
                const Spacer(),
                Text(title, 
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: Theme.of(context).textTheme.titleSmall?.copyWith(
                    fontSize: isCompact ? 11 : 13,
                    fontWeight: FontWeight.w800,
                    letterSpacing: 0.5,
                  )),
                if (!isCompact) ...[
                  const SizedBox(height: 2),
                  Text(subtitle.toUpperCase(), style: Theme.of(context).textTheme.labelSmall?.copyWith(fontSize: 7, color: Colors.white30, letterSpacing: 1)),
                ]
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildPremiumBadge(bool isAdmin) {
    final color = isAdmin ? Colors.redAccent : AppTheme.royalGold;
    return Center(
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        decoration: BoxDecoration(
          color: color.withValues(alpha: 0.1),
          borderRadius: BorderRadius.circular(20),
          border: Border.all(color: color.withValues(alpha: 0.3)),
        ),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.verified, color: color, size: 14),
            const SizedBox(width: 8),
            Text(isAdmin ? 'ADMINISTRATOR' : 'VERIFIED MEMBER', 
              style: TextStyle(color: color, fontSize: 9, fontWeight: FontWeight.bold, letterSpacing: 1)),
          ],
        ),
      ),
    );
  }

  Future<void> _handleLogout(BuildContext context, WidgetRef ref) async {
    HapticFeedback.heavyImpact();
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: AppTheme.midnightSurface,
        title: const Text('End Session', style: TextStyle(color: Colors.white)),
        content: const Text('Securely exit the platform?', style: TextStyle(color: AppTheme.textSecondaryDark)),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: const Text('CANCEL')),
          TextButton(onPressed: () => Navigator.pop(ctx, true), child: const Text('LOGOUT', style: TextStyle(color: Colors.redAccent))),
        ],
      ),
    );
    if (confirmed == true && context.mounted) {
      await ref.read(authServiceProvider).logout();
      if (context.mounted) {
        context.go('/login');
      }
    }
  }
}
