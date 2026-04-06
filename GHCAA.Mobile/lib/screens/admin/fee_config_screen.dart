import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../features/admin/admin_service.dart';

final feeConfigsProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(adminServiceProvider).getFeeConfigs();
});

class FeeConfigScreen extends ConsumerWidget {
  const FeeConfigScreen({super.key});

  Future<void> _upsertFee(BuildContext context, WidgetRef ref, [dynamic existing]) async {
    final amountCtrl = TextEditingController(text: existing != null ? existing['amount'].toString() : '');
    final category = existing != null ? existing['category'] : 0; // MembershipFee
    final type = existing != null ? existing['membershipType'] : 2; // General

    await showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: AppTheme.midnightSurface,
        title: Text(existing == null ? 'NEW FEE POLICY' : 'ADJUST FEE RULE', style: const TextStyle(color: AppTheme.royalGold, fontSize: 13, fontWeight: FontWeight.bold)),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
             TextField(
               controller: amountCtrl, 
               keyboardType: TextInputType.number,
               style: const TextStyle(color: Colors.white),
               decoration: const InputDecoration(labelText: 'Amount (BDT)', labelStyle: TextStyle(color: Colors.white38)),
             ),
             const SizedBox(height: 16),
             const Text('Policy metadata is governed by system roles.', style: TextStyle(color: Colors.white54, fontSize: 10)),
          ],
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('CANCEL')),
          TextButton(
            onPressed: () async {
              final amount = double.tryParse(amountCtrl.text) ?? 0;
              final payload = {
                'id': existing != null ? existing['id'] : 0,
                'category': category,
                'membershipType': type,
                'amount': amount,
                'isActive': true,
              };
              
              bool success;
              if (existing == null) {
                success = await ref.read(adminServiceProvider).addFeeConfig(payload);
              } else {
                success = await ref.read(adminServiceProvider).updateFeeConfig(payload);
              }

              if (success) {
                ref.invalidate(feeConfigsProvider);
                if (ctx.mounted) Navigator.pop(ctx);
              }
            }, 
            child: const Text('COMMIT', style: TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.bold))
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final configsAsync = ref.watch(feeConfigsProvider);

    return AppScaffold(
      isAdmin: true,
      title: 'Fee Governance',
      breadcrumb: 'ADMIN > FINANCIAL POLICY',
      floatingActionButton: FloatingActionButton(
        backgroundColor: AppTheme.royalGold,
        onPressed: () => _upsertFee(context, ref),
        child: const Icon(Icons.add, color: Colors.black),
      ),
      child: AsyncValueWidget<List<dynamic>>(
        value: configsAsync,
        loadingMessage: 'Fetching financial policies...',
        onRetry: () => ref.invalidate(feeConfigsProvider),
        data: (configs) {
          if (configs.isEmpty) {
            return const Center(child: Text('No fee configurations defined.', style: TextStyle(color: Colors.white54)));
          }

          return ListView.builder(
            padding: const EdgeInsets.all(20),
            itemCount: configs.length,
            itemBuilder: (context, index) {
              final fee = configs[index];
              return Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: GlassContainer(
                  child: ListTile(
                    title: Text(_getMembershipTypeName(fee['membershipType']), style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
                    subtitle: Text(_getCategoryName(fee['category']), style: const TextStyle(color: Colors.white54, fontSize: 11)),
                    trailing: Text('৳ ${fee['amount']}', style: const TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.bold, fontSize: 18)),
                    onTap: () {
                      HapticFeedback.lightImpact();
                      _upsertFee(context, ref, fee);
                    },
                  ),
                ),
              );
            },
          );
        },
      ),
    );
  }

  String _getMembershipTypeName(int type) {
    const names = ['Founding', 'Executive', 'General', 'Associate', 'Honorary', 'Advisory'];
    if (type >= 0 && type < names.length) return names[type].toUpperCase();
    return 'UNKNOWN';
  }

  String _getCategoryName(int cat) {
    const categories = ['Membership Fee', 'Registration Fee', 'Contribution', 'Other'];
    if (cat >= 0 && cat < categories.length) return categories[cat];
    return 'General Fee';
  }
}
