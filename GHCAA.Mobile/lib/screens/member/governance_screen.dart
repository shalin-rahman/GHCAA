import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:cached_network_image/cached_network_image.dart';
import '../../core/api/governance_api.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/logo_spinner.dart';
import '../../core/theme/app_theme.dart';
import '../../core/config/app_config.dart';
import '../../core/utils/app_utils.dart';

final currentECProvider = FutureProvider<Map<String, dynamic>>((ref) async {
  return await ref.read(governanceApiProvider).getCurrentEC();
});

final activeConstitutionProvider = FutureProvider<Map<String, dynamic>?>((ref) async {
  try {
    return await ref.read(governanceApiProvider).getCurrentConstitution();
  } catch (e) {
    return null;
  }
});

class GovernanceScreen extends ConsumerWidget {
  const GovernanceScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final ec = ref.watch(currentECProvider);
    final constitution = ref.watch(activeConstitutionProvider);

    return AppScaffold(
      title: 'Institutional Governance',
      breadcrumb: 'Association Hub > Governance Registry',
      child: DefaultTabController(
        length: 2,
        child: Column(
          children: [
            const TabBar(
              indicatorColor: AppTheme.royalGold,
              labelColor: AppTheme.royalGold,
              unselectedLabelColor: AppTheme.textMuted,
              labelStyle: TextStyle(fontWeight: FontWeight.w900, letterSpacing: 1.2, fontFamily: 'Outfit'),
              tabs: [
                Tab(text: 'CURRENT EC', icon: Icon(Icons.people_alt_rounded)),
                Tab(text: 'CONSTITUTION', icon: Icon(Icons.gavel_rounded)),
              ],
            ),
            Expanded(
              child: TabBarView(
                children: [
                   _buildECView(context, ec),
                   _buildConstitutionView(context, constitution, ref),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildECView(BuildContext context, AsyncValue<Map<String, dynamic>> ec) {
    return ec.when(
      data: (data) {
        final period = data['period'];
        final members = data['members'] as List<dynamic>;

        return ListView(
          padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 24),
          children: [
            GlassContainer(
              padding: const EdgeInsets.all(20),
              child: Row(
                children: [
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          (period['title'] ?? 'Interim Executive Committee').toString().toUpperCase(), 
                          style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 16, color: Colors.white, fontFamily: 'Outfit', letterSpacing: -0.5)
                        ),
                        const SizedBox(height: 4),
                        Text(
                          'Tenure: ${AppUtils.parseDate(period['startDate'])?.year ?? ""} - ${period['endDate'] != null ? AppUtils.parseDate(period['endDate'])?.year : (DateTime.now().year + 2)}',
                          style: const TextStyle(color: AppTheme.textMuted, fontSize: 12, fontFamily: 'Outfit'),
                        ),
                      ],
                    ),
                  ),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                    decoration: BoxDecoration(
                      color: Colors.green.withValues(alpha: 0.2),
                      borderRadius: BorderRadius.circular(8),
                      border: Border.all(color: Colors.green.withValues(alpha: 0.5)),
                    ),
                    child: const Text('ACTIVE', style: TextStyle(color: Colors.green, fontSize: 10, fontWeight: FontWeight.w900, fontFamily: 'Outfit')),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 32),
            const Padding(
              padding: EdgeInsets.only(left: 4, bottom: 12),
              child: Text(
                'EXECUTIVE COMMITTEE MEMBERS', 
                style: TextStyle(fontSize: 12, fontWeight: FontWeight.w900, color: AppTheme.textMuted, letterSpacing: 1.5, fontFamily: 'Outfit')
              ),
            ),
            const Divider(height: 1),
            const SizedBox(height: 12),
            ...members.map((m) {
              final member = m['member'];
              final position = m['positionLabel'] ?? m['positionName'] ?? m['position'].toString();
              final photoUrl = AppConfig.resolveImageUrl(member['photoPath']);
              
              return Padding(
                padding: const EdgeInsets.symmetric(vertical: 8),
                child: Row(
                  children: [
                    Container(
                      decoration: BoxDecoration(
                        shape: BoxShape.circle,
                        border: Border.all(color: AppTheme.royalGold.withValues(alpha: 0.3), width: 1.5),
                        boxShadow: [
                          BoxShadow(
                            color: AppTheme.royalGold.withValues(alpha: 0.1),
                            blurRadius: 8,
                            spreadRadius: 2,
                          )
                        ],
                      ),
                      child: ClipOval(
                        child: photoUrl != null
                          ? CachedNetworkImage(
                              imageUrl: photoUrl,
                              width: 56,
                              height: 56,
                              fit: BoxFit.cover,
                              placeholder: (context, url) => Container(
                                color: AppTheme.deepCharcoal,
                                child: Center(child: LogoSpinner.small()),
                              ),
                              errorWidget: (context, url, error) => Container(
                                width: 56,
                                height: 56,
                                color: AppTheme.deepCharcoal,
                                child: Center(
                                  child: Text(
                                    member['fullName']?[0]?.toUpperCase() ?? '?',
                                    style: const TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.w900, fontSize: 20, fontFamily: 'Outfit'),
                                  ),
                                ),
                              ),
                            )
                          : Container(
                              width: 56,
                              height: 56,
                              color: AppTheme.deepCharcoal,
                              child: Center(
                                child: Text(
                                  member['fullName']?[0]?.toUpperCase() ?? '?',
                                  style: const TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.w900, fontSize: 20, fontFamily: 'Outfit'),
                                ),
                              ),
                            ),
                      ),
                    ),
                    const SizedBox(width: 16),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            (member['fullName'] ?? 'System Member').toString().toUpperCase(),
                            style: const TextStyle(fontWeight: FontWeight.w900, color: Colors.white, fontSize: 14, fontFamily: 'Outfit', letterSpacing: 0.2)
                          ),
                          const SizedBox(height: 2),
                          Text(
                            position,
                            style: const TextStyle(fontSize: 11, color: AppTheme.royalGold, fontWeight: FontWeight.bold, fontFamily: 'Outfit')
                          ),
                        ],
                      ),
                    ),
                    if (member['membershipNumber'] != null)
                      Text(
                        member['membershipNumber'],
                        style: const TextStyle(fontSize: 10, color: AppTheme.textMuted, fontFamily: 'Outfit', fontWeight: FontWeight.bold)
                      ),
                  ],
                ),
              );
            }),
          ],
        );
      },
      loading: () => const Center(child: LogoSpinner(size: 120)),
      error: (e, s) => Center(child: Text('Synchronizing Registry Error: $e', style: const TextStyle(color: Colors.redAccent))),
    );
  }

  Widget _buildConstitutionView(BuildContext context, AsyncValue<Map<String, dynamic>?> consti, WidgetRef ref) {
    return consti.when(
      data: (data) {
        if (data == null) {
          return Center(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(Icons.gavel_rounded, size: 64, color: AppTheme.royalGold.withValues(alpha: 0.2)),
                const SizedBox(height: 16),
                const Text('CONSTITUTION NOT PUBLISHED', style: TextStyle(fontWeight: FontWeight.w900, color: AppTheme.textMuted, letterSpacing: 2)),
                const SizedBox(height: 8),
                const Padding(
                  padding: EdgeInsets.symmetric(horizontal: 40),
                  child: Text('The official association by-laws are currently being digitized.', textAlign: TextAlign.center, style: TextStyle(color: AppTheme.textMuted, fontSize: 12)),
                ),
              ],
            ),
          );
        }

        final version = data['version'] ?? '1.0';
        final changes = data['changeSummary'] ?? 'Initial adoption of association by-laws.';
        final effective = AppUtils.parseDate(data['effectiveDate']) ?? DateTime.now();

        return ListView(
          padding: const EdgeInsets.all(24.0),
          children: [
             GlassContainer(
              padding: const EdgeInsets.all(24.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Text('By-Laws Version $version', style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w900, color: AppTheme.royalGold, fontFamily: 'Outfit')),
                      const Icon(Icons.verified_user_rounded, color: AppTheme.royalGold),
                    ],
                  ),
                  const SizedBox(height: 8),
                  Text('Effective from ${effective.day}/${effective.month}/${effective.year}', style: const TextStyle(color: AppTheme.textMuted, fontSize: 12, fontFamily: 'Outfit')),
                ],
              ),
            ),
            const SizedBox(height: 32),
            const Text('REVISION NOTES', style: TextStyle(fontWeight: FontWeight.w900, fontSize: 12, color: AppTheme.textMuted, letterSpacing: 1.5, fontFamily: 'Outfit')),
            const SizedBox(height: 12),
            Text(changes, style: const TextStyle(fontSize: 14, color: Colors.white70, height: 1.6, fontFamily: 'Outfit')),
            const SizedBox(height: 32),
            ElevatedButton.icon(
              onPressed: () => _viewFullConstitution(context, data['content'] ?? ''),
              icon: const Icon(Icons.description_rounded),
              label: const Text('READ FULL CONSTITUTION'),
              style: ElevatedButton.styleFrom(
                minimumSize: const Size(double.infinity, 60),
                backgroundColor: AppTheme.deepCharcoal,
                side: const BorderSide(color: AppTheme.royalGold, width: 1),
              ),
            ),
            const SizedBox(height: 40),
            const Divider(),
            const Padding(
              padding: EdgeInsets.symmetric(vertical: 20),
              child: Text('DEMOCRATIC PARTICIPATION', style: TextStyle(fontSize: 14, fontWeight: FontWeight.w900, letterSpacing: 1, color: AppTheme.royalGold, fontFamily: 'Outfit')),
            ),
            const Text('An amendment is being proposed for this version. Cast your vote as a verified member.', style: TextStyle(color: AppTheme.textMuted, fontSize: 13, height: 1.5)),
            const SizedBox(height: 24),
            Row(
              children: [
                Expanded(
                  child: OutlinedButton.icon(
                    onPressed: () => _vote(context, ref, data['id'], true),
                    icon: const Icon(Icons.thumb_up_alt_rounded, color: Colors.green),
                    label: const Text('SUPPORT', style: TextStyle(fontWeight: FontWeight.w900, letterSpacing: 1)),
                    style: OutlinedButton.styleFrom(padding: const EdgeInsets.symmetric(vertical: 16), side: BorderSide(color: Colors.green.withValues(alpha: 0.3))),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: OutlinedButton.icon(
                    onPressed: () => _vote(context, ref, data['id'], false),
                    icon: const Icon(Icons.thumb_down_alt_rounded, color: Colors.red),
                    label: const Text('OPPOSE', style: TextStyle(fontWeight: FontWeight.w900, letterSpacing: 1)),
                    style: OutlinedButton.styleFrom(padding: const EdgeInsets.symmetric(vertical: 16), side: BorderSide(color: Colors.red.withValues(alpha: 0.3))),
                  ),
                ),
              ],
            ),
          ],
        );
      },
      loading: () => const Center(child: LogoSpinner(size: 120)),
      error: (e, s) => Center(child: Text('Governance Data Error: $e', style: const TextStyle(color: Colors.redAccent))),
    );
  }

  void _viewFullConstitution(BuildContext context, String content) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      builder: (context) => DraggableScrollableSheet(
        initialChildSize: 0.9,
        builder: (context, scrollController) => Container(
          padding: const EdgeInsets.all(20),
          decoration: const BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
          ),
          child: ListView(
            controller: scrollController,
            children: [
              const Text('Digital Constitution', style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold)),
              const SizedBox(height: 20),
              Text(content, style: const TextStyle(fontSize: 16, height: 1.5)),
            ],
          ),
        ),
      ),
    );
  }

  void _vote(BuildContext context, WidgetRef ref, int id, bool isFor) async {
    final success = await ref.read(governanceApiProvider).voteOnAmendment(id, isFor);
    if (context.mounted) {
       ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(success ? 'Your vote has been securely recorded.' : 'Voting failed. You may have already voted.'),
          backgroundColor: success ? Colors.green : Colors.red,
        ),
      );
    }
  }
}
