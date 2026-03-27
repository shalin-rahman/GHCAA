import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../features/auth/auth_service.dart';
import '../../core/config/app_config.dart';

class ProfileScreen extends ConsumerWidget {
  const ProfileScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final profileAsync = ref.watch(userProfileProvider);

    return AppScaffold(
      title: 'Member Profile',
      breadcrumb: 'Member Portal > Identity Registry',
      child: AsyncValueWidget<Map<String, dynamic>?>(
        value: profileAsync,
        loadingMessage: 'Synchronizing with registry...',
        data: (profile) {
          if (profile == null) return const Center(child: Text('Profile not synchronized.', style: TextStyle(color: AppTheme.textSecondaryDark)));
          
          return RefreshIndicator(
            color: AppTheme.royalGold,
            onRefresh: () async {
              HapticFeedback.mediumImpact();
              ref.invalidate(userProfileProvider);
            },
            child: SingleChildScrollView(
              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 24),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  _buildDigitalIDCard(context, profile),
                  const SizedBox(height: 32),
                  
                  _buildSectionHeader('Haragangian Academic History'),
                  _buildAcademicTimeline(profile['academicHistory'] ?? []),
                  const SizedBox(height: 32),

                  _buildSectionHeader('Professional Career Legacy'),
                  _buildProfessionalTimeline(profile['professionalHistory'] ?? []),
                  const SizedBox(height: 32),

                  _buildSectionHeader('Privacy & Directory Visibility'),
                  GlassContainer(
                    padding: EdgeInsets.zero,
                    child: Column(
                      children: [
                        _buildToggle('EXPOSE MOBILE IDENTITY', profile['isMobilePublic'] ?? true),
                        const Divider(color: Colors.white10, height: 1),
                        _buildToggle('EXPOSE PROFESSIONAL DATA', profile['isprofessionalInfoPublic'] ?? true),
                      ],
                    ),
                  ),
                  const SizedBox(height: 32),

                  _buildSectionHeader('Security & Audit Log'),
                  GlassContainer(
                    padding: EdgeInsets.zero,
                    child: Column(
                      children: [
                        _buildActionTile(context, Icons.history_edu_outlined, 'Security Activity Logs', '/activity'),
                        const Divider(color: Colors.white10, height: 1),
                        _buildActionTile(context, Icons.devices_outlined, 'Registered Device Metadata', null),
                      ],
                    ),
                  ),
                  
                  const SizedBox(height: 56),
                  _buildSignOutButton(context),
                  const SizedBox(height: 48),
                ],
              ),
            ),
          );
        },
      ),
    );
  }

  Widget _buildDigitalIDCard(BuildContext context, Map<String, dynamic> data) {
    final status = data['status'];
    final isVerified = status == 'Active' || status == 1;

    return GlassContainer(
      padding: const EdgeInsets.all(24),
      child: Column(
        children: [
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              _buildMemberPhoto(data['photoPath'], data['fullName']),
              const SizedBox(width: 20),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      data['membershipNumber'] ?? 'REGISTRY PENDING',
                      style: const TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.w900, letterSpacing: 2),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      data['fullName'] ?? 'Full Name',
                      style: const TextStyle(fontSize: 20, fontWeight: FontWeight.w900, color: Colors.white, height: 1.1),
                    ),
                    const SizedBox(height: 8),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                      decoration: BoxDecoration(
                        color: isVerified ? Colors.green.withValues(alpha: 0.1) : Colors.orange.withValues(alpha: 0.1),
                        borderRadius: BorderRadius.circular(4),
                        border: Border.all(color: isVerified ? Colors.green.withValues(alpha: 0.3) : Colors.orange.withValues(alpha: 0.3)),
                      ),
                      child: Text(
                        isVerified ? 'VERIFIED ACTIVE ✅' : 'AUDIT PENDING ⏳',
                        style: TextStyle(color: isVerified ? Colors.green : Colors.orange, fontSize: 10, fontWeight: FontWeight.bold),
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 24),
          const Divider(color: Colors.white10, height: 1),
          const SizedBox(height: 20),
          _buildProfileHealth(data['profileCompletionPercentage'] ?? 0),
        ],
      ),
    );
  }

  Widget _buildMemberPhoto(String? path, String name) {
    final fullUrl = path != null ? '${AppConfig.apiBaseUrl}/$path'.replaceAll('//', '/') : null;
    
    return Container(
      width: 80,
      height: 100,
      decoration: BoxDecoration(
        color: Colors.black,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: AppTheme.royalGold.withValues(alpha: 0.2)),
        image: fullUrl != null ? DecorationImage(image: NetworkImage(fullUrl), fit: BoxFit.cover) : null,
      ),
      child: fullUrl == null 
        ? Center(child: Text(name[0], style: const TextStyle(color: AppTheme.royalGold, fontSize: 32, fontWeight: FontWeight.bold)))
        : null,
    );
  }

  Widget _buildProfileHealth(int percentage) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            const Text('Registry Data Health', style: TextStyle(color: AppTheme.textSecondaryDark, fontSize: 10, fontWeight: FontWeight.bold)),
            Text('$percentage%', style: const TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.bold)),
          ],
        ),
        const SizedBox(height: 8),
        ClipRRect(
          borderRadius: BorderRadius.circular(2),
          child: LinearProgressIndicator(
            value: percentage / 100,
            backgroundColor: Colors.white.withValues(alpha: 0.05),
            color: AppTheme.royalGold,
            minHeight: 4,
          ),
        ),
      ],
    );
  }

  Widget _buildSectionHeader(String title) {
    return Padding(
      padding: const EdgeInsets.only(left: 4, bottom: 12),
      child: Text(
        title.toUpperCase(),
        style: const TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.w900, letterSpacing: 1.5),
      ),
    );
  }

  Widget _buildAcademicTimeline(List<dynamic> history) {
    if (history.isEmpty) return _buildEmptyPlaceholder('No academic registration found');
    
    return GlassContainer(
      padding: const EdgeInsets.all(16),
      child: Column(
        children: history.map((item) => _buildTimelineItem(
          title: '${item['degree']} in ${item['subject']}',
          subtitle: item['institutionName'],
          trailing: '${item['passingYear']}',
          isLast: history.indexOf(item) == history.length - 1,
        )).toList(),
      ),
    );
  }

  Widget _buildProfessionalTimeline(List<dynamic> history) {
    if (history.isEmpty) return _buildEmptyPlaceholder('Professional history pending update');
    
    return GlassContainer(
      padding: const EdgeInsets.all(16),
      child: Column(
        children: history.map((item) => _buildTimelineItem(
          title: item['designation'],
          subtitle: item['organizationName'],
          trailing: item['isCurrent'] ? 'Present' : '${item['startDate']?.toString().split('-')[0]}',
          isLast: history.indexOf(item) == history.length - 1,
        )).toList(),
      ),
    );
  }

  Widget _buildTimelineItem({required String title, required String subtitle, required String trailing, required bool isLast}) {
    return Container(
      margin: EdgeInsets.only(bottom: isLast ? 0 : 16),
      child: Row(
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(title, style: const TextStyle(color: Colors.white, fontSize: 14, fontWeight: FontWeight.w700)),
                Text(subtitle, style: const TextStyle(color: AppTheme.textSecondaryDark, fontSize: 11)),
              ],
            ),
          ),
          Text(trailing, style: const TextStyle(color: AppTheme.royalGold, fontSize: 12, fontWeight: FontWeight.w600)),
        ],
      ),
    );
  }

  Widget _buildEmptyPlaceholder(String message) {
    return GlassContainer(
      padding: const EdgeInsets.symmetric(vertical: 20),
      child: Center(child: Text(message, style: const TextStyle(color: AppTheme.textSecondaryDark, fontSize: 12, fontStyle: FontStyle.italic))),
    );
  }

  Widget _buildToggle(String label, bool value) {
    return SwitchListTile(
      title: Text(label, style: const TextStyle(fontSize: 14, color: Colors.white)),
      value: value,
      onChanged: (v) {},
      activeThumbColor: AppTheme.royalGold,
      contentPadding: const EdgeInsets.symmetric(horizontal: 16),
    );
  }

  Widget _buildActionTile(BuildContext context, IconData icon, String title, String? route) {
    return ListTile(
      leading: Icon(icon, size: 20, color: AppTheme.textSecondaryDark),
      title: Text(title, style: const TextStyle(fontSize: 14, color: Colors.white)),
      trailing: const Icon(Icons.chevron_right, size: 16, color: AppTheme.royalGold),
      onTap: route != null ? () => context.go(route) : () {},
      contentPadding: const EdgeInsets.symmetric(horizontal: 16),
    );
  }

  Widget _buildSignOutButton(BuildContext context) {
    return SizedBox(
      width: double.infinity,
      child: OutlinedButton(
        onPressed: () => context.go('/login'),
        style: OutlinedButton.styleFrom(
          side: const BorderSide(color: Colors.white10),
          padding: const EdgeInsets.symmetric(vertical: 16),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
        ),
        child: const Text('SECURITY LOGOUT', style: TextStyle(color: Colors.white60, fontSize: 12, fontWeight: FontWeight.bold, letterSpacing: 1)),
      ),
    );
  }
}
