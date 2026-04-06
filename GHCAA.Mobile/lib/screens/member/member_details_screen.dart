import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:flutter/services.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/config/app_config.dart';
import '../../core/api/api_client.dart';
import '../../features/auth/auth_service.dart';
import '../../features/admin/admin_service.dart';
import '../../features/networking/mentorship_service.dart';

final isAdminProvider = FutureProvider.autoDispose<bool>((ref) async {
  final role = await ref.read(authServiceProvider).getRole();
  return role.isStaffAdminRole;
});

String? _memberDobDisplay(Map<String, dynamic> profile) {
  final v = profile['dateOfBirth'] ?? profile['dob'];
  if (v == null) return null;
  final s = v.toString();
  return s.contains('T') ? s.split('T')[0] : s;
}

final memberDetailsProvider = FutureProvider.family<Map<String, dynamic>?, int>((ref, memberId) async {
  try {
    final dio = ref.read(dioProvider);
    final role = await ref.read(authServiceProvider).getRole();
    final isAdmin = role.isStaffAdminRole;
    
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

    return detailsAsync.when(
      data: (profile) {
        return AppScaffold(
          title: 'Member Details',
          breadcrumb: 'Management > Profile',
          actions: [
            if (isAdmin && profile != null)
              IconButton(
                icon: const Icon(Icons.edit_outlined, color: AppTheme.royalGold),
                tooltip: 'Edit member',
                onPressed: () {
                  HapticFeedback.lightImpact();
                  context.pushNamed('profile_edit', extra: profile);
                },
              ),
            if (profile != null)
              IconButton(
                icon: const Icon(Icons.message_outlined, color: AppTheme.royalGold),
                tooltip: 'Send Message',
                onPressed: () {
                  HapticFeedback.lightImpact();
                  context.push('/chat/${profile['id']}');
                },
              ),
          ],
          child: profile == null
              ? const Center(child: Text('Profile dossier not found.', style: TextStyle(color: Colors.red)))
              : _buildLoadedBody(context, ref, profile, isAdmin),
        );
      },
      loading: () => const AppScaffold(
        title: 'Member Details',
        breadcrumb: 'Management > Profile',
        child: Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
      ),
      error: (e, s) => AppScaffold(
        title: 'Member Details',
        child: Center(child: Text('Sync Error: $e', style: const TextStyle(color: Colors.red))),
      ),
    );
  }

  Widget _buildLoadedBody(BuildContext context, WidgetRef ref, Map<String, dynamic> profile, bool isAdmin) {
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
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Flexible(
                child: Text(
                  (profile['fullName'] ?? 'Anonymous Alumnus').toUpperCase(),
                  textAlign: TextAlign.center,
                  style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 24, color: Colors.white, letterSpacing: 1),
                ),
              ),
              if (profile['isVerified'] == true) ...[
                const SizedBox(width: 8),
                const Icon(Icons.verified, color: AppTheme.royalGold, size: 22),
              ],
            ],
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
              if (profile['isVerified'] == true)
                _buildChip('VERIFIED ALUMNI', Colors.cyanAccent, Colors.blueGrey),
            ],
          ),
          const SizedBox(height: 20),
          SizedBox(
            width: double.infinity,
            child: OutlinedButton.icon(
               onPressed: () => _showMentorshipDialog(context, ref, profile),
               icon: const Icon(Icons.handshake_outlined, size: 16),
               label: const Text('REQUEST MENTORSHIP', style: TextStyle(fontSize:10, fontWeight: FontWeight.bold, letterSpacing: 1)),
               style: OutlinedButton.styleFrom(
                 foregroundColor: AppTheme.royalGold,
                 side: BorderSide(color: AppTheme.royalGold.withValues(alpha: 0.3)),
               ),
            ),
          ),
          const SizedBox(height: 24),
          _buildSectionCard('ACADEMIC HISTORY', [
            ... (profile['academicHistory'] as List? ?? []).map((a) => _InfoRow(
              icon: Icons.school_outlined, 
              label: '${a['degree']} in ${a['subject']}', 
              value: '${a['institutionName']} (${a['passingYear']})'
            )),
          ]),
          const SizedBox(height: 16),
          _buildSectionCard('PROFESSIONAL HISTORY', [
            ... (profile['professionalHistory'] as List? ?? []).map((p) => _InfoRow(
              icon: Icons.work_outline, 
              label: '${p['designation']} at ${p['organizationName']}', 
              value: '${p['sector']} (${p['location']}) - ${p['isCurrent'] == true ? 'CURRENT' : (p['endDate'] ?? 'N/A')}'
            )),
          ]),
          if (isAdmin) ...[
            const SizedBox(height: 16),
            _buildSectionCard('PERSONAL INFORMATION', [
              _InfoRow(icon: Icons.badge_outlined, label: "Father's Name", value: profile['fatherName']),
              _InfoRow(icon: Icons.badge_outlined, label: "Mother's Name", value: profile['motherName']),
              _InfoRow(
                icon: Icons.cake_outlined,
                label: 'Date of Birth',
                value: _memberDobDisplay(profile),
              ),
              _InfoRow(icon: Icons.fingerprint, label: 'National ID', value: profile['nid']),
            ]),
          ],
          const SizedBox(height: 16),
          _buildSectionCard('Identity & Credentials', [
             _InfoRow(
               icon: Icons.badge_outlined,
               label: 'Institutional ID Card',
               value: 'DOWNLOAD PRINT-READY PDF',
               onTap: () => _downloadCredential(context, ref, profile['id'], 'id-card'),
             ),
             _InfoRow(
               icon: Icons.workspace_premium_outlined,
               label: 'Membership Certificate',
               value: 'DOWNLOAD OFFICIAL PDF',
               onTap: () => _downloadCredential(context, ref, profile['id'], 'certificate'),
             ),
          ]),
          const SizedBox(height: 16),
          _buildSectionCard('Contact Protocols', [
            _InfoRow(icon: Icons.email_outlined, label: 'Email', value: profile['email']),
            _InfoRow(icon: Icons.phone_outlined, label: 'Mobile', value: profile['mobileNo']),
          ]),
          if (isAdmin) ...[
            const SizedBox(height: 16),
            _buildSectionCard('MEMBERSHIP DETAILS', [
              _InfoRow(icon: Icons.auto_awesome, label: 'Merit Score', value: (profile['contributionPoints'] ?? profile['meritPoints'] ?? 0).toString()),
              _InfoRow(icon: Icons.file_present, label: 'Application Date', value: profile['createdAt'] != null ? profile['createdAt'].toString().split('T')[0] : null),
              _InfoRow(icon: Icons.verified_user, label: 'Approval Date', value: profile['approvalDate'] != null ? profile['approvalDate'].toString().split('T')[0] : 'Pending'),
            ]),
            const SizedBox(height: 16),
            _buildSectionCard('Admin Controls & Meta', [
              _InfoRow(icon: Icons.admin_panel_settings, label: 'Role', value: profile['role']),
              _InfoRow(icon: Icons.verified, label: 'Status', value: profile['status'] ?? (profile['isApproved'] == true ? 'Approved' : 'Pending'), highlight: (profile['status'] ?? (profile['isApproved'] == true ? 'Approved' : 'Pending')) == 'Pending'),
              _InfoRow(icon: Icons.calendar_today, label: 'Joined', value: profile['createdAt'] != null ? profile['createdAt'].toString().split('T')[0] : null),
            ]),
            const SizedBox(height: 16),
            _buildSectionCard('Dossier Verification', [
              if (profile['certificatePath'] != null)
                _InfoRow(
                  icon: Icons.badge_outlined,
                  label: 'NID / Certificate Proof',
                  value: 'TAP TO VIEW DOCUMENT',
                  onTap: () => _viewNetworkImage(context, profile['certificatePath']),
                ),
              if (profile['paymentProofPath'] != null)
                _InfoRow(
                  icon: Icons.receipt_long_outlined,
                  label: 'Payment Proof / Receipt',
                  value: 'TAP TO VIEW RECEIPT',
                  onTap: () => _viewNetworkImage(context, profile['paymentProofPath']),
                ),
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
        ? Center(child: Text(name != null ? name.isNotEmpty ? name[0] : '?' : '?', style: const TextStyle(color: AppTheme.royalGold, fontSize: 40, fontWeight: FontWeight.bold)))
        : null,
    );
  }

  void _viewNetworkImage(BuildContext context, String path) {
    String fullUrl;
    if (path.startsWith('http')) {
      fullUrl = path;
    } else {
      final base = AppConfig.apiBaseUrl.endsWith('/') ? AppConfig.apiBaseUrl.substring(0, AppConfig.apiBaseUrl.length - 1) : AppConfig.apiBaseUrl;
      final cleanP = path.startsWith('/') ? path.substring(1) : path;
      fullUrl = '$base/$cleanP';
    }

    showDialog(
      context: context,
      builder: (context) => Dialog(
        backgroundColor: Colors.transparent,
        insetPadding: const EdgeInsets.all(10),
        child: Stack(
          children: [
            InteractiveViewer(
              child: ClipRRect(
                borderRadius: BorderRadius.circular(12),
                child: Image.network(
                  fullUrl,
                  fit: BoxFit.contain,
                  loadingBuilder: (context, child, loadingProgress) {
                    if (loadingProgress == null) return child;
                    return const Center(child: CircularProgressIndicator(color: AppTheme.royalGold));
                  },
                  errorBuilder: (context, error, stackTrace) => Container(
                    padding: const EdgeInsets.all(20),
                    color: Colors.black87,
                    child: const Text('Security Dossier Image not found/accessible.', style: TextStyle(color: Colors.white70)),
                  ),
                ),
              ),
            ),
            Positioned(
              top: 10,
              right: 10,
              child: IconButton(
                icon: const Icon(Icons.close, color: Colors.white, size: 30),
                onPressed: () => Navigator.pop(context),
              ),
            ),
          ],
        ),
      ),
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
            child: InkWell(
              onTap: row.onTap,
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
                        Text(row.value!, style: TextStyle(color: row.onTap != null ? AppTheme.royalGold : (row.highlight ? Colors.redAccent : Colors.white), fontSize: 13, fontWeight: FontWeight.bold, decoration: row.onTap != null ? TextDecoration.underline : null)),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          )),
        ],
      ),
    );
  }

  Future<void> _handleAudit(BuildContext context, WidgetRef ref, int id, bool approve) async {
    String? reason;
    if (!approve) {
      reason = await _showRejectionDialog(context);
      if (reason == null) return; // Cancelled
    }

    try {
      final adminProfile = ref.read(userProfileProvider).value;
      final adminId = adminProfile?['id'] ?? 1;

      final success = await ref.read(adminServiceProvider).resolveApproval(id, approve, adminId: adminId, reason: reason);
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

  Future<String?> _showRejectionDialog(BuildContext context) async {
    final controller = TextEditingController();
    return showDialog<String>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Rejection Reason', style: TextStyle(color: Colors.redAccent)),
        backgroundColor: AppTheme.midnightSurface,
        content: TextField(
          controller: controller,
          maxLines: 3,
          decoration: const InputDecoration(
            hintText: 'Enter reason for rejection...',
            border: OutlineInputBorder(),
          ),
          style: const TextStyle(color: Colors.white),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context), child: const Text('CANCEL')),
          ElevatedButton(
            onPressed: () => Navigator.pop(context, controller.text),
            style: ElevatedButton.styleFrom(backgroundColor: Colors.redAccent),
            child: const Text('REJECT'),
          ),
        ],
      ),
    );
  }

  Future<void> _downloadCredential(BuildContext context, WidgetRef ref, int id, String type) async {
    try {
      final selfProfile = ref.read(userProfileProvider).value;
      final isAdmin = (await ref.read(isAdminProvider.future));
      final isSelf = selfProfile != null && selfProfile['id'] == id;
      
      final String endpoint = isAdmin && !isSelf
        ? '${AppConfig.apiBaseUrl}/admin/members/$id/$type/pdf'
        : '${AppConfig.apiBaseUrl}/profile/$type/pdf';

      final uri = Uri.parse(endpoint);
      if (await canLaunchUrl(uri)) {
        await launchUrl(uri, mode: LaunchMode.externalApplication);
      } else {
        if (context.mounted) {
          ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Could not initiate download protocol.')));
        }
      }
    } catch (e) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Download Error: $e'), backgroundColor: Colors.redAccent));
      }
    }
  }
  void _showMentorshipDialog(BuildContext context, WidgetRef ref, Map<String, dynamic> profile) {
    if (profile['id'] == null) return;
    
    final domainCtrl = TextEditingController();
    final msgCtrl = TextEditingController();

    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: AppTheme.midnightSurface,
        title: Text('REQUEST MENTORSHIP FROM ${(profile['fullName'] ?? 'ALUMNUS').toUpperCase()}', style: const TextStyle(color: AppTheme.royalGold, fontSize: 13, fontWeight: FontWeight.bold, letterSpacing: 1)),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text('Focus Area / Domain', style: TextStyle(color: Colors.white54, fontSize: 10)),
            TextField(
              controller: domainCtrl,
              style: const TextStyle(color: Colors.white, fontSize: 12),
              decoration: const InputDecoration(hintText: 'e.g., General Management, IELTS Prep'),
            ),
            const SizedBox(height: 12),
            const Text('Initial Message', style: TextStyle(color: Colors.white54, fontSize: 10)),
            TextField(
              controller: msgCtrl,
              style: const TextStyle(color: Colors.white, fontSize: 12, height: 1.4),
              maxLines: 3,
              decoration: const InputDecoration(hintText: 'Briefly explain why you want them as a mentor...'),
            ),
          ],
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('CANCEL', style: TextStyle(color: Colors.white54))),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold),
            onPressed: () async {
              if (domainCtrl.text.isEmpty) {
                 ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Please specify a domain/focus area.')));
                 return;
              }
              final ok = await ref.read(mentorshipServiceProvider).sendRequest(profile['id'], msgCtrl.text, domainCtrl.text);
              if (ctx.mounted) Navigator.pop(ctx);
              if (context.mounted) {
                if (ok) {
                  ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Mentorship request dispatched.')));
                } else {
                  ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Request failed. Have you already asked them?'), backgroundColor: Colors.orangeAccent));
                }
              }
            },
            child: const Text('SEND REQUEST', style: TextStyle(color: Colors.black, fontWeight: FontWeight.bold)),
          )
        ],
      ),
    );
  }
}

class _InfoRow {
  final IconData icon;
  final String label;
  final String? value;
  final bool highlight;
  final VoidCallback? onTap;
  _InfoRow({required this.icon, required this.label, this.value, this.highlight = false, this.onTap});
}
