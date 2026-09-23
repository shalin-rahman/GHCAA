import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../core/widgets/logo_spinner.dart';
import '../../core/utils/app_utils.dart';
import '../../core/config/org_config.dart';
import '../../core/services/org_config_service.dart';
import '../../features/campaigns/campaign_service.dart';

final myPledgesProvider = FutureProvider.autoDispose<List<CampaignPledge>>((ref) async {
  return ref.read(campaignServiceProvider).getMyPledges();
});

class MyPledgesScreen extends ConsumerWidget {
  const MyPledgesScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final pledgesAsync = ref.watch(myPledgesProvider);
    final currency = ref.watch(orgCurrencyProvider);

    return AppScaffold(
      title: 'My Pledges',
      breadcrumb: 'GIVING > MY PLEDGES',
      child: pledgesAsync.when(
        data: (pledges) {
          if (pledges.isEmpty) {
            return const EmptyStateWidget('You have not made any pledges yet.', icon: Icons.receipt_long_outlined);
          }
          return ListView.builder(
            padding: const EdgeInsets.all(16),
            itemCount: pledges.length,
            itemBuilder: (ctx, i) => _buildPledgeCard(pledges[i], currency),
          );
        },
        loading: () => const Center(child: LogoSpinner(size: 120)),
        error: (e, _) => Center(child: Text('Error: $e', style: const TextStyle(color: Colors.redAccent))),
      ),
    );
  }

  Widget _buildPledgeCard(CampaignPledge pledge, OrgCurrency currency) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: GlassContainer(
        padding: const EdgeInsets.all(14),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  AppUtils.formatCurrency(pledge.amount, currency),
                  style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 16),
                ),
                _buildStatusBadge(pledge.status),
              ],
            ),
            if (pledge.message != null && pledge.message!.isNotEmpty) ...[
              const SizedBox(height: 8),
              Text(pledge.message!, style: const TextStyle(color: Colors.white54, fontSize: 12)),
            ],
            if (pledge.pledgedAt != null) ...[
              const SizedBox(height: 8),
              Text(
                AppUtils.formatDate(pledge.pledgedAt!),
                style: const TextStyle(color: Colors.white38, fontSize: 11),
              ),
            ],
          ],
        ),
      ),
    );
  }

  Widget _buildStatusBadge(String status) {
    Color color;
    switch (status.toLowerCase()) {
      case 'confirmed':
      case 'received':
        color = Colors.greenAccent;
        break;
      case 'pending':
        color = AppTheme.royalGold;
        break;
      case 'cancelled':
      case 'declined':
        color = Colors.redAccent;
        break;
      default:
        color = Colors.white38;
    }
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.15),
        borderRadius: BorderRadius.circular(4),
        border: Border.all(color: color.withValues(alpha: 0.4)),
      ),
      child: Text(status, style: TextStyle(color: color, fontSize: 10, fontWeight: FontWeight.bold)),
    );
  }
}
