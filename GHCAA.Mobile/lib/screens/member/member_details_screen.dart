import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/config/app_config.dart';
import '../../core/api/api_client.dart';

final memberDetailsProvider = FutureProvider.family<Map<String, dynamic>?, int>((ref, memberId) async {
  try {
    final dio = ref.read(dioProvider);
    final response = await dio.get('/networking/member/$memberId');
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

    return AppScaffold(
      title: 'Executive Profile',
      breadcrumb: 'Alumni Registry > Dossier',
      child: detailsAsync.when(
        data: (profile) {
          if (profile == null) {
            return const Center(child: Text('Profile dossier not found.', style: TextStyle(color: Colors.red)));
          }

          final photoUrl = profile['photoPath'] != null 
              ? '${AppConfig.apiBaseUrl}/${profile['photoPath']}'.replaceAll('//', '/') 
              : null;

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
                const SizedBox(height: 32),
                _buildSectionCard('Academic Chronicle', [
                  _InfoRow(icon: Icons.school_outlined, label: 'Passing Year', value: profile['passingYear']?.toString()),
                  _InfoRow(icon: Icons.menu_book_outlined, label: 'Highest Degree', value: profile['degree']),
                  _InfoRow(icon: Icons.class_outlined, label: 'Subject/Major', value: profile['subject']),
                ]),
                const SizedBox(height: 16),
                _buildSectionCard('Professional Dossier', [
                  _InfoRow(icon: Icons.work_outline, label: 'Current Designation', value: profile['designation']),
                  _InfoRow(icon: Icons.business_outlined, label: 'Industry Sector', value: profile['professionalSector']),
                  _InfoRow(icon: Icons.location_city_outlined, label: 'Current Location', value: profile['address']),
                ]),
                const SizedBox(height: 16),
                _buildSectionCard('Contact Protocols', [
                  _InfoRow(icon: Icons.email_outlined, label: 'Email', value: profile['email']),
                  _InfoRow(icon: Icons.phone_outlined, label: 'Mobile', value: profile['mobileNo']),
                  _InfoRow(icon: Icons.water_drop_outlined, label: 'Blood Group', value: profile['bloodGroup'], highlight: true),
                ]),
              ],
            ),
          );
        },
        loading: () => const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
        error: (e, s) => Center(child: Text('Sync Error: $e', style: const TextStyle(color: Colors.red))),
      ),
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
}

class _InfoRow {
  final IconData icon;
  final String label;
  final String? value;
  final bool highlight;
  _InfoRow({required this.icon, required this.label, this.value, this.highlight = false});
}
