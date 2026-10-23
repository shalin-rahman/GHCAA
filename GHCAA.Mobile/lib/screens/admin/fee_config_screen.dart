import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_date_picker.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../features/admin/admin_service.dart';
import '../../core/utils/app_utils.dart';
import '../../core/services/org_config_service.dart';

final feeConfigsProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(adminServiceProvider).getFeeConfigs();
});

// TODO 8.15. The API keys a fee on category, type and effective date, so a missing date stored
// 0001-01-01 and a second add was refused. An edit keeps the row's own date unless changed.
Map<String, dynamic> buildFeePayload(dynamic existing, double amount, DateTime effectiveDate) => {
      'id': existing != null ? existing['id'] : 0,
      'category': existing != null ? existing['category'] : 0, // MembershipFee
      'membershipType': existing != null ? existing['membershipType'] : 2, // General
      'amount': amount,
      'effectiveDate': AppUtils.toWire(effectiveDate),
      'isActive': true,
    };

class FeeConfigScreen extends ConsumerWidget {
  const FeeConfigScreen({super.key});

  Future<void> _upsertFee(BuildContext context, WidgetRef ref, [dynamic existing]) async {
    final currency = ref.read(orgCurrencyProvider);
    final amountCtrl = TextEditingController(text: existing != null ? existing['amount'].toString() : '');
    final dateFormat = ref.read(orgDateFormatProvider).identifier;
    var effectiveDate = (existing != null ? AppUtils.parseDate(existing['effectiveDate']?.toString()) : null) ??
        DateTime.now();

    await showDialog(
      context: context,
      builder: (ctx) => StatefulBuilder(builder: (ctx, setState) => AlertDialog(
        backgroundColor: AppTheme.midnightSurface,
        title: Text(existing == null ? 'NEW FEE POLICY' : 'ADJUST FEE RULE', style: const TextStyle(color: AppTheme.royalGold, fontSize: 13, fontWeight: FontWeight.bold)),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
             TextField(
               controller: amountCtrl, 
               keyboardType: TextInputType.number,
               style: const TextStyle(color: Colors.white),
               decoration: InputDecoration(labelText: 'Amount (${currency.code})', labelStyle: const TextStyle(color: Colors.white38)),
             ),
             ListTile(
               key: const Key('fee-effective-date'),
               contentPadding: EdgeInsets.zero,
               title: const Text('Effective from', style: TextStyle(color: Colors.white38, fontSize: 12)),
               subtitle: Text(displayAppDate(effectiveDate, format: dateFormat), style: const TextStyle(color: Colors.white)),
               trailing: const Icon(Icons.event, color: AppTheme.royalGold, size: 18),
               onTap: () async {
                 final picked = await showAppDatePicker(context: ctx, initialDate: effectiveDate);
                 if (picked != null) setState(() => effectiveDate = picked);
               },
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
              final payload = buildFeePayload(existing, amount, effectiveDate);
              
              bool success;
              try {
                if (existing == null) {
                  success = await ref.read(adminServiceProvider).addFeeConfig(payload);
                } else {
                  success = await ref.read(adminServiceProvider).updateFeeConfig(payload);
                }
              } catch (e) {
                // The API client puts the server's detail in message.
                if (ctx.mounted) {
                  ScaffoldMessenger.of(ctx).showSnackBar(SnackBar(content: Text(
                      'Could not save: ${e is DioException ? (e.message ?? e.toString()) : e}')));
                }
                return;
              }

              if (success) {
                ref.invalidate(feeConfigsProvider);
                if (ctx.mounted) Navigator.pop(ctx);
              }
            }, 
            child: const Text('COMMIT', style: TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.bold))
          ),
        ],
      )),
    );
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final configsAsync = ref.watch(feeConfigsProvider);
    final currency = ref.watch(orgCurrencyProvider);

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
            return const EmptyStateWidget('No fee configurations defined.', icon: Icons.payments_outlined);
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
                    trailing: Text(AppUtils.formatCurrency(fee['amount'], currency), style: const TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.bold, fontSize: 18)),
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
    const names = ['Founding', 'Executive', 'General', 'Associate', 'Honorary', 'Advisory', 'Guest'];
    if (type >= 0 && type < names.length) return names[type].toUpperCase();
    return 'UNKNOWN';
  }

  String _getCategoryName(int cat) {
    const categories = ['Membership Fee', 'Registration Fee', 'Contribution', 'Other'];
    if (cat >= 0 && cat < categories.length) return categories[cat];
    return 'General Fee';
  }
}
