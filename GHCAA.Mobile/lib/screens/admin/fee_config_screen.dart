import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/constants/app_constants.dart';

class FeeConfigScreen extends ConsumerWidget {
  const FeeConfigScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return AppScaffold(
      isAdmin: true,
      title: 'Fee Policy',
      breadcrumb: 'ADMIN > FEES',
      child: ListView(
        padding: const EdgeInsets.all(AppConstants.paddingLarge),
        children: [
          _buildFeeCategory(context, 'MEMBERSHIP FEES', [
            _buildFeeItem('Admission Fee', '৳ 1000', 'One-time'),
            _buildFeeItem('Monthly Dues', '৳ 100', 'Recurring'),
            _buildFeeItem('Life Member', '৳ 10000', 'One-time'),
          ]),
          const SizedBox(height: 24),
          _buildFeeCategory(context, 'ADDITIONAL CHARGES', [
            _buildFeeItem('Card Printing', '৳ 150', 'On-demand'),
            _buildFeeItem('Courier Charge', '৳ 100', 'Standard'),
          ]),
        ],
      ),
    );
  }

  Widget _buildFeeCategory(BuildContext context, String title, List<Widget> items) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(title, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
        const SizedBox(height: 12),
        ...items,
      ],
    );
  }

  Widget _buildFeeItem(String label, String amount, String freq) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12.0),
      child: GlassContainer(
        child: ListTile(
          title: Text(label, style: const TextStyle(fontWeight: FontWeight.w600)),
          subtitle: Text(freq, style: const TextStyle(fontSize: 12)),
          trailing: Text(amount, style: const TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.bold, fontSize: 18)),
          onTap: () {},
        ),
      ),
    );
  }
}
