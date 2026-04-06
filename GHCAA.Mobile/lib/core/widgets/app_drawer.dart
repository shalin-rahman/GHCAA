import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../theme/app_theme.dart';
import '../../features/auth/auth_service.dart';
import '../config/app_config.dart';
import 'async_value_widget.dart';
import '../../features/theme/dynamic_theme_service.dart';

class AppDrawer extends ConsumerWidget {
  const AppDrawer({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final profileAsync = ref.watch(userProfileProvider);
    final roleAsync = ref.watch(roleProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return Drawer(
      backgroundColor: isDark ? AppTheme.midnightBase : Colors.white,
      child: Container(
        decoration: isDark ? const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
            colors: [AppTheme.midnightSurface, AppTheme.midnightBase],
          ),
        ) : null,
        child: Column(
          children: [
            _buildHeader(context, profileAsync, roleAsync),
            Expanded(
              child: ListView(
                padding: const EdgeInsets.symmetric(vertical: 8),
                children: [
                  roleAsync.when(
                    data: (role) {
                      final isAdmin = role.isStaffAdminRole;
                      return Column(
                        children: [
                          if (isAdmin)
                            _buildSection(context, 'ADMINISTRATION', [
                              _MenuItem(Icons.admin_panel_settings_outlined, 'Dashboard', '/admin_dashboard'),
                              _MenuItem(Icons.gavel_outlined, 'Approvals', '/admin/approvals'),
                              _MenuItem(Icons.account_tree_outlined, 'Governance', '/admin/governance'),
                              _MenuItem(Icons.account_balance_wallet_outlined, 'Ledger', '/admin/ledger'),
                              _MenuItem(Icons.collections_outlined, 'CMS / Content', '/admin/cms'),
                              _MenuItem(Icons.article_outlined, 'Article Review', '/admin/articles'),
                              _MenuItem(Icons.attach_money_outlined, 'Fee Config', '/admin/fees'),
                              _MenuItem(Icons.palette_outlined, 'Themes', '/admin/themes'),
                              _MenuItem(Icons.mail_outlined, 'Messages', '/admin/messages'),
                              _MenuItem(Icons.security_outlined, 'Gatekeeper', '/admin/gatekeeper'),
                              _MenuItem(Icons.history_edu_outlined, 'Audit Logs', '/admin/audit'),
                            ]),
                          _buildSection(context, 'MY ACCOUNT', [
                            _MenuItem(Icons.badge_outlined, 'My Profile', '/profile'),
                            _MenuItem(Icons.receipt_long_outlined, 'Payments', '/financials'),
                          ]),
                          _buildSection(context, 'COMMUNITY', [
                            _MenuItem(Icons.people_outline, 'Alumni Directory', '/directory'),
                            _MenuItem(Icons.event_note_outlined, 'Events', '/events'),
                            _MenuItem(Icons.work_outline, 'Job Hub', '/jobs'),
                            _MenuItem(Icons.account_tree_outlined, 'Committee', '/committee'),
                            _MenuItem(Icons.family_restroom_outlined, 'Family Links', '/family'),
                          ]),
                          _buildSection(context, 'MEDIA & TOOLS', [
                            _MenuItem(Icons.newspaper_outlined, 'News', '/news'),
                            _MenuItem(Icons.photo_library_outlined, 'Event Gallery', '/gallery'),
                            _MenuItem(Icons.article_outlined, 'Articles', '/articles'),
                            _MenuItem(Icons.menu_book_outlined, 'Magazine', '/magazine'),
                            _MenuItem(Icons.chat_bubble_outline, 'AI Assistant', '/assistant'),
                            _MenuItem(Icons.notifications_outlined, 'Notifications', '/notifications'),
                            _MenuItem(Icons.contact_support_outlined, 'Support', '/support'),
                            _MenuItem(Icons.info_outline, 'About', '/about'),
                          ]),
                          _buildSection(context, 'LAYOUT SETTINGS', [
                            _buildToggleItem(
                              ref, 
                              Icons.view_compact_outlined, 
                              'Top Navbar', 
                              ref.watch(globalAppBarVisibilityProvider),
                              (val) => ref.read(globalAppBarVisibilityProvider.notifier).state = val,
                            ),
                          ]),
                        ],
                      );
                    },
                    loading: () => const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
                    error: (_, __) => const Center(child: Text('Role sync error')),
                  ),
                ],
              ),
            ),
            const Divider(color: Colors.white10),
            _buildLogoutButton(ref, context),
            const SizedBox(height: 20),
          ],
        ),
      ),
    );
  }

  Widget _buildHeader(BuildContext context, AsyncValue<Map<String, dynamic>?> profileAsync, AsyncValue<String?> roleAsync) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.fromLTRB(20, 60, 20, 30),
      decoration: BoxDecoration(
        color: Colors.black.withValues(alpha: 0.2),
        border: const Border(bottom: BorderSide(color: Colors.white10)),
      ),
      child: AsyncValueWidget<Map<String, dynamic>?>(
        value: profileAsync,
        loadingMessage: '',
        data: (profile) {
          final photoPath = profile?['photoPath'];
          final photoUrl = photoPath != null ? '${AppConfig.apiBaseUrl}/$photoPath'.replaceAll('//', '/') : null;
          final role = roleAsync.value ?? 'Member';
          final isAdmin = role.isStaffAdminRole;
          
          return Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              CircleAvatar(
                radius: 36,
                backgroundColor: AppTheme.royalGold.withValues(alpha: 0.1),
                backgroundImage: photoUrl != null ? NetworkImage(photoUrl) : null,
                child: photoUrl == null ? const Icon(Icons.person, color: AppTheme.royalGold, size: 36) : null,
              ),
              const SizedBox(height: 16),
              Text(profile?['fullName'] ?? 'Alumnus', 
                  style: const TextStyle(color: Colors.white, fontSize: 18, fontWeight: FontWeight.w900, letterSpacing: 0.5)),
              const SizedBox(height: 4),
              Row(
                children: [
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                    decoration: BoxDecoration(color: isAdmin ? Colors.redAccent : AppTheme.royalGold, borderRadius: BorderRadius.circular(4)),
                    child: Text(isAdmin ? 'ADMINISTRATOR' : 'ALUMNI MEMBER', style: const TextStyle(color: Colors.black, fontSize: 9, fontWeight: FontWeight.bold)),
                  ),
                  const SizedBox(width: 8),
                  Text('Batch: ${profile?['batch'] ?? 'N/A'}', style: const TextStyle(color: AppTheme.textSecondaryDark, fontSize: 11)),
                ],
              ),
            ],
          );
        },
      ),
    );
  }

  Widget _buildSection(BuildContext context, String title, List<dynamic> items) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Padding(
          padding: const EdgeInsets.fromLTRB(24, 20, 24, 12),
          child: Text(title, style: const TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.w900, letterSpacing: 1.5)),
        ),
        ...items.map((item) {
          if (item is Widget) return item;
          if (item is _MenuItem) {
            return ListTile(
              leading: Icon(item.icon, color: AppTheme.textSecondaryDark, size: 22),
              title: Text(item.title, style: const TextStyle(color: Colors.white, fontSize: 14, fontWeight: FontWeight.w500)),
              onTap: () {
                HapticFeedback.lightImpact();
                Navigator.pop(context); // Close drawer
                context.go(item.route);
              },
              dense: true,
              contentPadding: const EdgeInsets.symmetric(horizontal: 24),
            );
          }
          return const SizedBox();
        }),
      ],
    );
  }

  Widget _buildLogoutButton(WidgetRef ref, BuildContext context) {
    return ListTile(
      leading: const Icon(Icons.logout, color: Colors.redAccent, size: 22),
      title: const Text('Sign Out', style: TextStyle(color: Colors.redAccent, fontSize: 14, fontWeight: FontWeight.bold)),
      onTap: () async {
        final confirmed = await showDialog<bool>(
          context: context,
          builder: (ctx) => AlertDialog(
            backgroundColor: AppTheme.midnightSurface,
            title: const Text('Sign Out', style: TextStyle(color: Colors.white)),
            content: const Text('Are you sure you want to sign out?', style: TextStyle(color: AppTheme.textSecondaryDark)),
            actions: [
              TextButton(onPressed: () => Navigator.pop(ctx, false), child: const Text('CANCEL')),
              TextButton(onPressed: () => Navigator.pop(ctx, true), child: const Text('LOGOUT', style: TextStyle(color: Colors.redAccent))),
            ],
          ),
        );
        if (confirmed == true) {
          HapticFeedback.heavyImpact();
          await ref.read(authServiceProvider).logout();
          if (context.mounted) context.go('/login');
        }
      },
      contentPadding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
    );
  }

  Widget _buildToggleItem(WidgetRef ref, IconData icon, String title, bool value, Function(bool) onChanged) {
    return ListTile(
      leading: Icon(icon, color: AppTheme.royalGold.withValues(alpha: 0.7), size: 22),
      title: Text(title, style: const TextStyle(color: Colors.white, fontSize: 13, fontWeight: FontWeight.w600)),
      trailing: Switch.adaptive(
        value: value,
        onChanged: (v) {
          HapticFeedback.selectionClick();
          onChanged(v);
        },
        activeTrackColor: AppTheme.royalGold.withValues(alpha: 0.3),
        activeThumbColor: AppTheme.royalGold,
      ),
      dense: true,
      contentPadding: const EdgeInsets.symmetric(horizontal: 24),
    );
  }
}

class _MenuItem {
  final IconData icon;
  final String title;
  final String route;
  _MenuItem(this.icon, this.title, this.route);
}
