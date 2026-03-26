import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/constants/app_constants.dart';
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
      child: SingleChildScrollView(
        padding: const EdgeInsets.all(AppConstants.paddingLarge),
        child: Column(
          children: [
            duesAsync.when(
              data: (dues) => GlassContainer(
                padding: const EdgeInsets.all(AppConstants.paddingExtraLarge),
                child: Column(
                  children: [
                    Text('Total Outstanding Dues', style: Theme.of(context).textTheme.bodySmall),
                    const SizedBox(height: 8),
                    Text('${dues.toStringAsFixed(2)} BDT', style: const TextStyle(fontSize: 32, fontWeight: FontWeight.bold, color: AppTheme.royalGold)),
                    const SizedBox(height: 24),
                    SizedBox(width: double.infinity, child: ElevatedButton(onPressed: () {}, child: const Text('Pay Now'))),
                  ],
                ),
              ),
              loading: () => const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
              error: (e, s) => Center(child: Text('Error: $e')),
            ),
            const SizedBox(height: AppConstants.paddingExtraLarge),
            const Align(
              alignment: Alignment.centerLeft,
              child: Text('Contribution History', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
            ),
            const SizedBox(height: AppConstants.paddingMedium),
            ledgerAsync.when(
              data: (items) => Column(
                children: items.map((item) => Padding(
                  padding: const EdgeInsets.only(bottom: 12),
                  child: GlassContainer(
                    child: ListTile(
                      title: Text(item['description'] ?? 'Donation'),
                      subtitle: Text(item['date'] ?? ''),
                      trailing: Text('${item['amount']} BDT', style: const TextStyle(fontWeight: FontWeight.bold, color: AppTheme.royalGold)),
                    ),
                  ),
                )).toList(),
              ),
              loading: () => const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
              error: (e, s) => Center(child: Text('Error: $e')),
            ),
          ],
        ),
      ),
    );
  }
}
