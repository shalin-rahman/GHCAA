import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../features/auth/auth_service.dart';
import '../../core/services/device_info_service.dart';


class ProfileScreen extends ConsumerWidget {
  const ProfileScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final profileAsync = ref.watch(userProfileProvider);

    return AppScaffold(
      title: 'Member Profile',
      child: AsyncValueWidget(
        value: profileAsync,
        loadingMessage: 'Synchronizing profile data...',
        data: (profile) {
          final data = profile as Map<String, dynamic>?;
          if (data == null) return const Center(child: Text('Profile not found.', style: TextStyle(color: Colors.white)));
          
          return SingleChildScrollView(
            padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 24),
            child: Column(
              children: [
                CircleAvatar(
                  radius: 54,
                  backgroundColor: AppTheme.royalGold.withOpacity(0.1),
                  child: Text(data['fullName']?[0] ?? '?', style: const TextStyle(fontSize: 40, color: AppTheme.royalGold, fontWeight: FontWeight.bold)),
                ),
                const SizedBox(height: 16),
                Text(data['fullName'] ?? 'Full Name', style: const TextStyle(fontSize: 22, fontWeight: FontWeight.w900, color: Colors.white)),
                Text('Membership ID: ${data['membershipId'] ?? 'Pending'}', style: TextStyle(color: AppTheme.textSecondaryDark, fontSize: 12)),
                const SizedBox(height: 32),
                
                _buildSection(context, 'Privacy Preferences', [
                  _buildToggle('Show Mobile No', data['mobileNoIsVisible'] ?? true),
                  _buildToggle('Show Professional Info', data['professionalInfoIsVisible'] ?? true),
                ]),
                const SizedBox(height: 24),
                
                _buildSection(context, 'Security & Activity', [
                  _buildActionTile(context, Icons.history_edu, 'Activity Audit Logs', '/activity'),
                  _buildActionTile(context, Icons.fingerprint, 'Biometric Lock (FaceID)', null),
                  _buildActionTile(context, Icons.support_agent, 'Help & Technical Support', '/support'),
                ]),
                const SizedBox(height: 24),

                _buildSection(context, 'About GHCAA Portal', [
                  Consumer(builder: (context, ref, _) {
                    final deviceAsync = ref.watch(deviceInfoProvider);
                    final deviceStr = deviceAsync.when(
                      data: (d) => d.toString(),
                      loading: () => 'Detecting device...',
                      error: (_, __) => 'Device info unavailable',
                    );
                    return _buildActionTile(context, Icons.phone_android_outlined, deviceStr, null, color: AppTheme.textSecondaryDark);
                  }),
                  _buildActionTile(context, Icons.info_outline, 'Version Info (1.0.0 Dev)', null),
                  _buildActionTile(context, Icons.gavel, 'Terms & Privacy Policy', null),
                  _buildActionTile(context, Icons.developer_mode, 'Lead Dev: Shalin Rahman', null),
                  _buildActionTile(context, Icons.business_outlined, 'Partnership: GHCAA', null, color: AppTheme.royalGold.withOpacity(0.5)),
                ]),

                const SizedBox(height: 48),

                SizedBox(
                  width: double.infinity,
                  child: OutlinedButton.icon(
                    onPressed: () => context.go('/'),
                    icon: const Icon(Icons.logout, size: 18),
                    label: const Text('Sign Out of Portal'),
                    style: OutlinedButton.styleFrom(foregroundColor: Colors.redAccent, side: const BorderSide(color: Colors.redAccent)),
                  ),
                ),
                const SizedBox(height: 32),
              ],
            ),
          );
        },
      ),
    );
  }

  Widget _buildSection(BuildContext context, String title, List<Widget> children) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Padding(
          padding: const EdgeInsets.only(left: 8.0, bottom: 8.0),
          child: Text(title.toUpperCase(), style: const TextStyle(fontSize: 10, fontWeight: FontWeight.bold, letterSpacing: 1.2, color: AppTheme.royalGold)),
        ),
        GlassContainer(padding: EdgeInsets.zero, child: Column(children: children)),
      ],
    );
  }

  Widget _buildToggle(String label, bool value) {
    return SwitchListTile(
      title: Text(label, style: const TextStyle(fontSize: 14, color: Colors.white)),
      value: value,
      onChanged: (v) {},
      activeColor: AppTheme.royalGold,
    );
  }

  Widget _buildActionTile(BuildContext context, IconData icon, String title, String? route, {Color? color}) {
    return ListTile(
      leading: Icon(icon, size: 20, color: color ?? AppTheme.textSecondaryDark),
      title: Text(title, style: const TextStyle(fontSize: 14, color: Colors.white)),
      trailing: const Icon(Icons.chevron_right, size: 16, color: AppTheme.royalGold),
      onTap: route != null ? () => context.go(route) : () {},
    );
  }
}
