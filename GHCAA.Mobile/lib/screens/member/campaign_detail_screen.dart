import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/logo_spinner.dart';
import '../../core/utils/app_utils.dart';
import '../../core/config/org_config.dart';
import '../../core/services/org_config_service.dart';
import '../../features/campaigns/campaign_service.dart';

final campaignBySlugProvider =
    FutureProvider.autoDispose.family<Campaign, String>((ref, slug) async {
  return ref.read(campaignServiceProvider).getCampaign(slug);
});

final campaignHonourRollProvider =
    FutureProvider.autoDispose.family<CampaignHonourRoll, String>((ref, slug) async {
  return ref.read(campaignServiceProvider).getHonourRoll(slug);
});

class CampaignDetailScreen extends ConsumerStatefulWidget {
  final String slug;
  const CampaignDetailScreen({super.key, required this.slug});

  @override
  ConsumerState<CampaignDetailScreen> createState() => _CampaignDetailScreenState();
}

class _CampaignDetailScreenState extends ConsumerState<CampaignDetailScreen> {
  final _amountController = TextEditingController();
  final _messageController = TextEditingController();
  bool _isAnonymous = false;
  bool _submitting = false;

  @override
  void dispose() {
    _amountController.dispose();
    _messageController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final campaignAsync = ref.watch(campaignBySlugProvider(widget.slug));
    final honourRollAsync = ref.watch(campaignHonourRollProvider(widget.slug));
    final currency = ref.watch(orgCurrencyProvider);

    return AppScaffold(
      title: 'Campaign',
      breadcrumb: 'GIVING > CAMPAIGNS',
      child: campaignAsync.when(
        data: (campaign) => ListView(
          padding: const EdgeInsets.all(16),
          children: [
            Text(campaign.title, style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 18)),
            const SizedBox(height: 12),
            Text(campaign.story, style: const TextStyle(color: Colors.white70, fontSize: 13, height: 1.5)),
            const SizedBox(height: 16),
            ClipRRect(
              borderRadius: BorderRadius.circular(4),
              child: LinearProgressIndicator(
                value: campaign.progressPercent / 100,
                backgroundColor: Colors.white10,
                color: AppTheme.royalGold,
                minHeight: 8,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              '${AppUtils.formatCurrency(campaign.amountReceived, currency)} raised of ${AppUtils.formatCurrency(campaign.targetAmount, currency)}',
              style: const TextStyle(color: AppTheme.royalGold, fontSize: 12, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 24),
            GlassContainer(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text('MAKE A PLEDGE', style: TextStyle(color: Colors.white, fontSize: 12, fontWeight: FontWeight.bold, letterSpacing: 1)),
                  const SizedBox(height: 12),
                  TextField(
                    controller: _amountController,
                    keyboardType: const TextInputType.numberWithOptions(decimal: true),
                    style: const TextStyle(color: Colors.white),
                    decoration: const InputDecoration(labelText: 'Amount', labelStyle: TextStyle(color: Colors.white54)),
                  ),
                  const SizedBox(height: 12),
                  TextField(
                    controller: _messageController,
                    style: const TextStyle(color: Colors.white),
                    decoration: const InputDecoration(labelText: 'Message (optional)', labelStyle: TextStyle(color: Colors.white54)),
                  ),
                  CheckboxListTile(
                    value: _isAnonymous,
                    onChanged: (v) => setState(() => _isAnonymous = v ?? false),
                    title: const Text('Give anonymously', style: TextStyle(color: Colors.white70, fontSize: 12)),
                    controlAffinity: ListTileControlAffinity.leading,
                    contentPadding: EdgeInsets.zero,
                  ),
                  SizedBox(
                    width: double.infinity,
                    child: ElevatedButton(
                      style: ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold),
                      onPressed: _submitting ? null : () => _submitPledge(campaign.slug),
                      child: Text(_submitting ? 'SUBMITTING...' : 'PLEDGE NOW',
                          style: const TextStyle(color: Colors.black, fontWeight: FontWeight.bold)),
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 24),
            const Text('HONOUR ROLL', style: TextStyle(color: Colors.white, fontSize: 12, fontWeight: FontWeight.bold, letterSpacing: 1)),
            const SizedBox(height: 12),
            honourRollAsync.when(
              data: (roll) => _buildHonourRoll(roll, currency),
              loading: () => const Center(child: LogoSpinner(size: 80)),
              error: (e, _) => Text('Error: $e', style: const TextStyle(color: Colors.redAccent)),
            ),
          ],
        ),
        loading: () => const Center(child: LogoSpinner(size: 120)),
        error: (e, _) => Center(child: Text('Error: $e', style: const TextStyle(color: Colors.redAccent))),
      ),
    );
  }

  Widget _buildHonourRoll(CampaignHonourRoll roll, OrgCurrency currency) {
    if (roll.tiers.isEmpty && roll.untiered.isEmpty) {
      return const Text('No donors yet. Be the first to give.', style: TextStyle(color: Colors.white38, fontSize: 12));
    }
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        for (final tier in roll.tiers) ...[
          Text(tier.tierName, style: const TextStyle(color: AppTheme.royalGold, fontSize: 11, fontWeight: FontWeight.bold)),
          const SizedBox(height: 6),
          for (final donor in tier.donors) _buildDonorRow(donor, currency),
          const SizedBox(height: 12),
        ],
        for (final donor in roll.untiered) _buildDonorRow(donor, currency),
      ],
    );
  }

  Widget _buildDonorRow(HonourRollEntry donor, OrgCurrency currency) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 6),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Expanded(child: Text(donor.displayName, style: const TextStyle(color: Colors.white70, fontSize: 12))),
          Text(AppUtils.formatCurrency(donor.amountReceived, currency), style: const TextStyle(color: Colors.white54, fontSize: 12)),
        ],
      ),
    );
  }

  Future<void> _submitPledge(String slug) async {
    final amount = double.tryParse(_amountController.text.trim());
    if (amount == null || amount <= 0) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Enter a valid amount.'), backgroundColor: Colors.redAccent),
      );
      return;
    }
    setState(() => _submitting = true);
    try {
      await ref.read(campaignServiceProvider).createPledge(
            slug,
            CreatePledge(
              amount: amount,
              isAnonymous: _isAnonymous,
              message: _messageController.text.trim().isEmpty ? null : _messageController.text.trim(),
            ),
          );
      if (!mounted) return;
      _amountController.clear();
      _messageController.clear();
      ref.invalidate(campaignBySlugProvider(slug));
      ref.invalidate(campaignHonourRollProvider(slug));
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Thank you for your pledge.')),
      );
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Failed to submit pledge.'), backgroundColor: Colors.redAccent),
      );
    } finally {
      if (mounted) setState(() => _submitting = false);
    }
  }
}
