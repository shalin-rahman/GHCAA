import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:flutter/services.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/config/app_config.dart';
import '../../core/api/api_client.dart';
import '../../features/auth/auth_service.dart';
import '../../features/admin/admin_service.dart';

final isAdminProvider = FutureProvider.autoDispose<bool>((ref) async {
  final role = await ref.read(authServiceProvider).getRole();
  return role == 'SuperAdmin' || role == 'Admin';
});

final memberDetailsProvider = FutureProvider.family<Map<String, dynamic>?, int>((ref, memberId) async {
  try {
    final dio = ref.read(dioProvider);
    final role = await ref.read(authServiceProvider).getRole();
    final isAdmin = role == 'SuperAdmin' || role == 'Admin';
    
    // Admins fetch from the privileged endpoint to see masked/hidden fields
    final endpoint = isAdmin ? '/admin/members/$memberId' : '/networking/member/$memberId';
    final response = await dio.get(endpoint);
    return response.data as Map<String, dynamic>;
  } catch (e) {
    return null;
  }
});

class MemberDetailsScreen extends ConsumerWidget {
  final int memberId;

  const MemberDetailsScreen({super.key, required this.memberId});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final detailsAsync = ref.watch(memberDetailsProvider(memberId));
    final adminAsync = ref.watch(isAdminProvider);
    final isAdmin = adminAsync.value ?? false;

    return AppScaffold(
      title: 'Member Details',
      breadcrumb: 'Management > Profile',
      actions: [
        if (isAdmin)
          IconButton(
            icon: const Icon(Icons.edit_outlined, color: AppTheme.royalGold),
            onPressed: () {
              HapticFeedback.lightImpact();
              context.pushNamed('profile_edit', extra: detailsAsync.value);
            },
          ),
      ],
      child: detailsAsync.when(
        data: (profile) {
          if (profile == null) {
            return const Center(child: Text('Profile dossier not found.', style: TextStyle(color: Colors.red)));
          }

          String? photoUrl;
          if (profile['photoPath'] != null && profile['photoPath'].toString().isNotEmpty) {
            final p = profile['photoPath'];
            if (p.startsWith('http')) {
              photoUrl = p;
            } else {
              final base = AppConfig.apiBaseUrl.endsWith('/') ? AppConfig.apiBaseUrl.substring(0, AppConfig.apiBaseUrl.length - 1) : AppConfig.apiBaseUrl;
              final cleanP = p.startsWith('/') ? p.substring(1) : p;
              photoUrl = '$base/$cleanP';
            }
          }

          return SingleChildScrollView(
            padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 20),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.center,
              children: [
                _buildAvatar(photoUrl, profile['fullName']),
                const SizedBox(height: 24),
                Text(
                  (profile['fullName'] ?? 'Anonymous Alumnus').toUpperCase(),
                  textAlign: TextAlign.center,
                  style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 24, color: Colors.white, letterSpacing: 1),
                ),
                const SizedBox(height: 8),
                Text(
                  'MEMBER-ID: ${profile['membershipNumber'] ?? 'REG-PENDING'}',
                  style: const TextStyle(color: AppTheme.royalGold, fontSize: 11, fontWeight: FontWeight.bold, letterSpacing: 2),
                ),
                const SizedBox(height: 12),
                Wrap(
                  spacing: 8,
                  runSpacing: 8,
                  alignment: WrapAlignment.center,
                  children: [
                    if (profile['membershipType'] != null)
                      _buildChip(profile['membershipType'], Colors.lightGreenAccent, Colors.lightGreen),
                    if (profile['categoryBadge'] != null)
                      _buildChip(profile['categoryBadge'], Colors.blueAccent, Colors.blue),
                    if (profile['bloodGroup'] != null)
                      _buildChip('Blood: ${profile['bloodGroup']}', Colors.redAccent, Colors.red),
                  ],
                ),
                const SizedBox(height: 32),
                _buildSectionCard('ACADEMIC HISTORY', [
                  _InfoRow(icon: Icons.school_outlined, label: 'Graduation Year', value: profile['passingYear']?.toString()),
                  _InfoRow(icon: Icons.menu_book_outlined, label: 'Qualification', value: profile['degree']),
                  _InfoRow(icon: Icons.class_outlined, label: 'Department', value: profile['subject']),
                ]),
                const SizedBox(height: 16),
                _buildSectionCard('PROFESSIONAL HISTORY', [
                  _InfoRow(icon: Icons.work_outline, label: 'Designation', value: profile['designation']),
                  _InfoRow(icon: Icons.business_outlined, label: 'Organization', value: profile['organizationName']),
                  _InfoRow(icon: Icons.category_outlined, label: 'Professional Sector', value: profile['professionalSector']),
                  _InfoRow(icon: Icons.location_city_outlined, label: 'Location', value: profile['location']),
                ]),
                if (isAdmin) ...[
                  const SizedBox(height: 16),
                  _buildSectionCard('PERSONAL INFORMATION', [
                    _InfoRow(icon: Icons.badge_outlined, label: "Father's Name", value: profile['fatherName']),
                    _InfoRow(icon: Icons.badge_outlined, label: "Mother's Name", value: profile['motherName']),
                    _InfoRow(icon: Icons.cake_outlined, label: 'Date of Birth', value: profile['dob'] != null ? profile['dob'].toString().split('T')[0] : null),
                    _InfoRow(icon: Icons.fingerprint, label: 'National ID', value: profile['nid']),
                  ]),
                ],
                const SizedBox(height: 16),
                _buildSectionCard('Contact Protocols', [
                  _InfoRow(icon: Icons.email_outlined, label: 'Email', value: profile['email']),
                  _InfoRow(icon: Icons.phone_outlined, label: 'Mobile', value: profile['mobileNo']),
                ]),
                if (isAdmin) ...[
                  const SizedBox(height: 16),
                  _buildSectionCard('MEMBERSHIP DETAILS', [
                    _InfoRow(icon: Icons.auto_awesome, label: 'Merit Score', value: profile['meritPoints']?.toString() ?? 'Zero'),
                    _InfoRow(icon: Icons.file_present, label: 'Application Date', value: profile['createdAt'] != null ? profile['createdAt'].toString().split('T')[0] : null),
                    _InfoRow(icon: Icons.verified_user, label: 'Approval Date', value: profile['approvalDate'] != null ? profile['approvalDate'].toString().split('T')[0] : 'Pending'),
                  ]),
                  const SizedBox(height: 16),
                  _buildSectionCard('Admin Controls & Meta', [
                    _InfoRow(icon: Icons.admin_panel_settings, label: 'Role', value: profile['role']),
                    _InfoRow(icon: Icons.verified, label: 'Status', value: profile['status'] ?? (profile['isApproved'] == true ? 'Approved' : 'Pending'), highlight: (profile['status'] ?? (profile['isApproved'] == true ? 'Approved' : 'Pending')) == 'Pending'),
                    _InfoRow(icon: Icons.calendar_today, label: 'Joined', value: profile['createdAt'] != null ? profile['createdAt'].toString().split('T')[0] : null),
                  ]),
                ],
                if (isAdmin && (profile['status'] == 'Pending' || profile['isApproved'] == false)) ...[
                  const SizedBox(height: 32),
                  Row(
                    children: [
                      Expanded(
                        child: ElevatedButton(
                          onPressed: () => _handleAudit(context, ref, profile['id'], false),
                          style: ElevatedButton.styleFrom(backgroundColor: Colors.redAccent.withValues(alpha: 0.1), foregroundColor: Colors.redAccent, side: const BorderSide(color: Colors.redAccent, width: 0.5)),
                          child: const Text('REJECT', style: TextStyle(fontWeight: FontWeight.w900, letterSpacing: 1)),
                        ),
                      ),
                      const SizedBox(width: 16),
                      Expanded(
                        child: ElevatedButton(
                          onPressed: () => _handleAudit(context, ref, profile['id'], true),
                          style: ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold, foregroundColor: Colors.black),
                          child: const Text('APPROVE', style: TextStyle(fontWeight: FontWeight.w900, letterSpacing: 1)),
                        ),
                      ),
                    ],
                  ),
                ],
              ],
            ),
          );
        },
        loading: () => const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
        error: (e, s) => Center(child: Text('Sync Error: $e', style: const TextStyle(color: Colors.red))),
      ),
    );
  }

  Widget _buildChip(String label, Color textColor, Color bgColor) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(color: bgColor.withValues(alpha: 0.1), borderRadius: BorderRadius.circular(16), border: Border.all(color: bgColor.withValues(alpha: 0.3))),
      child: Text(label, style: TextStyle(color: textColor, fontSize: 10, fontWeight: FontWeight.bold, letterSpacing: 1)),
    );
  }

  Widget _buildAvatar(String? url, String? name) {
    return Container(
      width: 120,
      height: 120,
      decoration: BoxDecoration(
        color: Colors.black,
        shape: BoxShape.circle,
        border: Border.all(color: AppTheme.royalGold.withValues(alpha: 0.5), width: 3),
        image: url != null ? DecorationImage(image: NetworkImage(url), fit: BoxFit.cover) : null,
        boxShadow: [
          BoxShadow(color: AppTheme.royalGold.withValues(alpha: 0.2), blurRadius: 20, spreadRadius: 5),
        ],
      ),
      child: url == null 
        ? Center(child: Text(name != null ? name[0] : '?', style: const TextStyle(color: AppTheme.royalGold, fontSize: 40, fontWeight: FontWeight.bold)))
        : null,
    );
  }

  Widget _buildSectionCard(String title, List<_InfoRow> rows) {
    return GlassContainer(
      padding: const EdgeInsets.all(20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(title.toUpperCase(), style: const TextStyle(color: AppTheme.royalGold, fontSize: 12, fontWeight: FontWeight.w900, letterSpacing: 1.5)),
          const Divider(color: Colors.white12, height: 24),
          ...rows.where((row) => row.value != null && row.value!.isNotEmpty).map((row) => Padding(
            padding: const EdgeInsets.only(bottom: 12),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Icon(row.icon, size: 16, color: row.highlight ? Colors.redAccent : AppTheme.textSecondaryDark),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(row.label.toUpperCase(), style: const TextStyle(color: AppTheme.textSecondaryDark, fontSize: 9, fontWeight: FontWeight.bold, letterSpacing: 1)),
                      const SizedBox(height: 2),
                      Text(row.value!, style: TextStyle(color: row.highlight ? Colors.redAccent : Colors.white, fontSize: 13, fontWeight: FontWeight.bold)),
                    ],
                  ),
                ),
              ],
            ),
          )),
        ],
      ),
    );
  }

  Future<void> _handleAudit(BuildContext context, WidgetRef ref, int id, bool approve) async {
    try {
      final success = await ref.read(adminServiceProvider).resolveApproval(id, approve);
      if (success) {
        ref.invalidate(memberDetailsProvider(id));
        if (context.mounted) {
          ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(approve ? 'Member Approved' : 'Application Rejected')));
          context.pop();
        }
      }
    } catch (e) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Audit Error: $e'), backgroundColor: Colors.redAccent));
      }
    }
  }
}

class _InfoRow {
  final IconData icon;
  final String label;
  final String? value;
  final bool highlight;
  _InfoRow({required this.icon, required this.label, this.value, this.highlight = false});
}
