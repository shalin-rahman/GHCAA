import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/constants/app_constants.dart';

class AdminLedgerScreen extends ConsumerWidget {
  const AdminLedgerScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return AppScaffold(
      isAdmin: true,
      title: 'Global Ledger',
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.all(AppConstants.paddingLarge),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                _buildStatTile(context, 'Total Revenue', '৳ 1,240,000'),
                _buildStatTile(context, 'Pending Dues', '৳ 42,000', highlight: true),
              ],
            ),
          ),
          Expanded(
            child: ListView.builder(
              padding: const EdgeInsets.symmetric(horizontal: AppConstants.paddingLarge),
              itemCount: 15,
              itemBuilder: (context, index) {
                return Padding(
                  padding: const EdgeInsets.only(bottom: 12.0),
                  child: GlassContainer(
                    child: ListTile(
                      leading: const Icon(Icons.receipt_long, color: AppTheme.royalGold),
                      title: Text('Member # ${1024 + index}', style: const TextStyle(fontWeight: FontWeight.bold)),
                      subtitle: const Text('Membership Fee Paid'),
                      trailing: const Text('৳ 500', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                    ),
                  ),
                );
              },
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildStatTile(BuildContext context, String title, String value, {bool highlight = false}) {
    return SizedBox(
      width: (MediaQuery.of(context).size.width - 64) / 2,
      child: GlassContainer(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(title.toUpperCase(), style: TextStyle(fontSize: 10, letterSpacing: 1.2, fontWeight: FontWeight.bold, color: AppTheme.textSecondaryDark.withOpacity(0.6))),
            const SizedBox(height: 12),
            Text(value, style: const TextStyle(fontSize: 24, fontWeight: FontWeight.w900, color: Colors.white)),
          ],
        ),
      ),
    );
  }
}
