import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../features/financials/financial_service.dart';

final ledgerProvider = FutureProvider<List<dynamic>>((ref) async => ref.read(financialServiceProvider).getLedger());
final duesProvider = FutureProvider<double>((ref) async => ref.read(financialServiceProvider).getOutstandingDues());

class FinancialPortalScreen extends ConsumerWidget {
  const FinancialPortalScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final ledgerAsync = ref.watch(ledgerProvider);
    final duesAsync = ref.watch(duesProvider);

    return AppScaffold(
      title: 'Economic Portal',
      breadcrumb: 'Member Portal > Payment Portal',
      child: RefreshIndicator(
        color: AppTheme.royalGold,
        onRefresh: () async {
          HapticFeedback.mediumImpact();
          ref.invalidate(ledgerProvider);
          ref.invalidate(duesProvider);
        },
        child: SingleChildScrollView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.symmetric(horizontal: 20.0, vertical: 24.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              duesAsync.when(
                data: (dues) => GlassContainer(
                  padding: const EdgeInsets.all(24.0),
                  child: Column(
                    children: [
                      const Text('TOTAL OUTSTANDING DUES', style: TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.w900, letterSpacing: 1.5)),
                      const SizedBox(height: 16),
                      Text('${dues.toStringAsFixed(2)} BDT', style: const TextStyle(fontSize: 36, fontWeight: FontWeight.w900, color: Colors.white, letterSpacing: 1)),
                      const SizedBox(height: 24),
                      SizedBox(
                        width: double.infinity, 
                        child: ElevatedButton(
                          onPressed: () {
                            HapticFeedback.lightImpact();
                            // Payment logic
                          }, 
                          style: ElevatedButton.styleFrom(padding: const EdgeInsets.symmetric(vertical: 16)),
                          child: const Text('PAY OUTSTANDING', style: TextStyle(fontWeight: FontWeight.w900, fontSize: 12, letterSpacing: 1))
                        )
                      ),
                    ],
                  ),
                ),
                loading: () => const Center(child: Padding(padding: EdgeInsets.all(40), child: CircularProgressIndicator(color: AppTheme.royalGold))),
                error: (e, s) => Center(child: Text('Error: $e', style: const TextStyle(color: Colors.redAccent))),
              ),
              const SizedBox(height: 40),
              const Padding(
                padding: EdgeInsets.only(left: 4, bottom: 16),
                child: Text('CONTRIBUTION HISTORY', style: TextStyle(fontWeight: FontWeight.w900, fontSize: 11, color: AppTheme.royalGold, letterSpacing: 1.5)),
              ),
              ledgerAsync.when(
                data: (items) => items.isEmpty 
                  ? const Center(child: Padding(padding: EdgeInsets.all(40), child: Text('No transaction history found.', style: TextStyle(color: AppTheme.textSecondaryDark))))
                  : Column(
                      children: items.map((item) => Padding(
                        padding: const EdgeInsets.only(bottom: 12),
                        child: GlassContainer(
                          padding: const EdgeInsets.all(16),
                          child: Row(
                            children: [
                              Container(
                                padding: const EdgeInsets.all(10),
                                decoration: BoxDecoration(color: AppTheme.royalGold.withValues(alpha: 0.1), borderRadius: BorderRadius.circular(12)),
                                child: const Icon(Icons.receipt_outlined, color: AppTheme.royalGold, size: 20),
                              ),
                              const SizedBox(width: 16),
                              Expanded(
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Text(item['description'] ?? 'Alumni Contribution', style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 14, color: Colors.white)),
                                    const SizedBox(height: 4),
                                    Text(item['date'] ?? 'Just Now', style: const TextStyle(fontSize: 11, color: AppTheme.textSecondaryDark)),
                                  ],
                                ),
                              ),
                              Text('${item['amount']} BDT', style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 14, color: AppTheme.royalGold)),
                            ],
                          ),
                        ),
                      )).toList(),
                    ),
                loading: () => const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
                error: (e, s) => Center(child: Text('Error: $e')),
              ),
              const SizedBox(height: 40),
            ],
          ),
        ),
      ),
    );
  }
}
