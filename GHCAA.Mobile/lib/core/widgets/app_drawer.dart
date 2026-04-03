import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../theme/app_theme.dart';
import '../../features/auth/auth_service.dart';
import '../config/app_config.dart';
import 'async_value_widget.dart';

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
                      final isAdmin = role == 'SuperAdmin' || role == 'Admin';
                      return Column(
                        children: [
                          if (isAdmin)
                            _buildSection(context, 'ADMINISTRATION', [
                              _MenuItem(Icons.admin_panel_settings_outlined, 'Dashboard', '/dashboard'),
                              _MenuItem(Icons.gavel_outlined, 'Approvals', '/admin/approvals'),
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
                          ]),
                          _buildSection(context, 'MEDIA & TOOLS', [
                            _MenuItem(Icons.newspaper_outlined, 'News', '/news'),
                            _MenuItem(Icons.photo_library_outlined, 'Event Gallery', '/gallery'),
                            _MenuItem(Icons.chat_bubble_outline, 'Assistance', '/assistant'),
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
          final isAdmin = role == 'SuperAdmin' || role == 'Admin';
          
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

  Widget _buildSection(BuildContext context, String title, List<_MenuItem> items) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Padding(
          padding: const EdgeInsets.fromLTRB(24, 20, 24, 12),
          child: Text(title, style: const TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.w900, letterSpacing: 1.5)),
        ),
        ...items.map((item) => ListTile(
          leading: Icon(item.icon, color: AppTheme.textSecondaryDark, size: 22),
          title: Text(item.title, style: const TextStyle(color: Colors.white, fontSize: 14, fontWeight: FontWeight.w500)),
          onTap: () {
            HapticFeedback.lightImpact();
            Navigator.pop(context); // Close drawer
            context.go(item.route);
          },
          dense: true,
          contentPadding: const EdgeInsets.symmetric(horizontal: 24),
        )),
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
}

class _MenuItem {
  final IconData icon;
  final String title;
  final String route;
  _MenuItem(this.icon, this.title, this.route);
}
