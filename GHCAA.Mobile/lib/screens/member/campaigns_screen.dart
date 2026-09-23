import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../core/widgets/logo_spinner.dart';
import '../../core/utils/app_utils.dart';
import '../../core/services/org_config_service.dart';
import '../../features/campaigns/campaign_service.dart';

final publicCampaignsProvider =
    FutureProvider.autoDispose<List<Campaign>>((ref) async {
  return ref.read(campaignServiceProvider).getPublicCampaigns();
});

class CampaignsScreen extends ConsumerWidget {
  const CampaignsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final campaignsAsync = ref.watch(publicCampaignsProvider);
    final currency = ref.watch(orgCurrencyProvider);

    return AppScaffold(
      title: 'Fundraising Campaigns',
      breadcrumb: 'GIVING > CAMPAIGNS',
      actions: [
        IconButton(
          icon: const Icon(Icons.receipt_long_outlined, color: Colors.white70),
          tooltip: 'My Pledges',
          onPressed: () => context.push('/campaigns/my-pledges'),
        ),
      ],
      child: campaignsAsync.when(
        data: (campaigns) {
          if (campaigns.isEmpty) {
            return const EmptyStateWidget('No active campaigns right now.', icon: Icons.volunteer_activism_outlined);
          }
          return ListView.builder(
            padding: const EdgeInsets.all(16),
            itemCount: campaigns.length,
            itemBuilder: (ctx, i) {
              final campaign = campaigns[i];
              return Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: InkWell(
                  onTap: () => context.push('/campaigns/${campaign.slug}'),
                  child: GlassContainer(
                    padding: const EdgeInsets.all(14),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(campaign.title, style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 15)),
                        const SizedBox(height: 8),
                        Text(
                          campaign.story,
                          maxLines: 2,
                          overflow: TextOverflow.ellipsis,
                          style: const TextStyle(color: Colors.white54, fontSize: 12),
                        ),
                        const SizedBox(height: 12),
                        ClipRRect(
                          borderRadius: BorderRadius.circular(4),
                          child: LinearProgressIndicator(
                            value: campaign.progressPercent / 100,
                            backgroundColor: Colors.white10,
                            color: AppTheme.royalGold,
                            minHeight: 6,
                          ),
                        ),
                        const SizedBox(height: 8),
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(
                              '${AppUtils.formatCurrency(campaign.amountReceived, currency)} raised',
                              style: const TextStyle(color: AppTheme.royalGold, fontSize: 11, fontWeight: FontWeight.bold),
                            ),
                            Text(
                              'of ${AppUtils.formatCurrency(campaign.targetAmount, currency)}',
                              style: const TextStyle(color: Colors.white38, fontSize: 11),
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                ),
              );
            },
          );
        },
        loading: () => const Center(child: LogoSpinner(size: 120)),
        error: (e, _) => Center(child: Text('Error: $e', style: const TextStyle(color: Colors.redAccent))),
      ),
    );
  }
}
