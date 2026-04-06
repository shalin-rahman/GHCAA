import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../core/utils/app_utils.dart';
import '../../core/widgets/app_search_field.dart';
import '../../features/financials/financial_service.dart';

final ledgerProvider = FutureProvider<List<dynamic>>((ref) async => ref.read(financialServiceProvider).getLedger());
final duesProvider = FutureProvider<double>((ref) async => ref.read(financialServiceProvider).getOutstandingDues());
final savedMethodsProvider = FutureProvider<List<dynamic>>((ref) async => ref.read(financialServiceProvider).getSavedMethods());
final ledgerSearchQueryProvider = StateProvider.autoDispose<String>((ref) => "");

class FinancialPortalScreen extends ConsumerStatefulWidget {
  const FinancialPortalScreen({super.key});

  @override
  ConsumerState<FinancialPortalScreen> createState() => _FinancialPortalScreenState();
}

class _FinancialPortalScreenState extends ConsumerState<FinancialPortalScreen> {
  final TextEditingController _searchController = TextEditingController();

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  Future<void> _downloadReceipt(int id) async {
    final url = await ref.read(financialServiceProvider).getReceiptUrl(id);
    if (url != null) {
      final uri = Uri.parse(url);
      if (await canLaunchUrl(uri)) {
        await launchUrl(uri);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final ledgerAsync = ref.watch(ledgerProvider);
    final duesAsync = ref.watch(duesProvider);
    final methodsAsync = ref.watch(savedMethodsProvider);
    final searchQuery = ref.watch(ledgerSearchQueryProvider).toLowerCase();

    return AppScaffold(
      title: 'Fees & Dues',
      breadcrumb: 'PORTAL > FINANCIALS',
      child: RefreshIndicator(
        color: AppTheme.royalGold,
        onRefresh: () async {
          HapticFeedback.mediumImpact();
          ref.invalidate(ledgerProvider);
          ref.invalidate(duesProvider);
          ref.invalidate(savedMethodsProvider);
        },
        child: SingleChildScrollView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.symmetric(horizontal: 20.0, vertical: 24.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Outstanding Dues
              duesAsync.when(
                data: (dues) => GlassContainer(
                  padding: const EdgeInsets.all(24.0),
                  child: Column(
                    children: [
                      Text(AppUtils.formatCurrency(dues), style: const TextStyle(fontSize: 36, fontWeight: FontWeight.w900, color: Colors.white, letterSpacing: 1)),
                      const SizedBox(height: 24),
                      SizedBox(
                        width: double.infinity, 
                        child: ElevatedButton(
                          onPressed: dues > 0 ? () {
                            HapticFeedback.mediumImpact();
                            // Checkout flow
                          } : null, 
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
              const SizedBox(height: 32),

              // Saved Payment Methods (Flexibility)
              const Padding(
                padding: EdgeInsets.only(left: 4, bottom: 12),
                child: Text('SAVED PAYMENT METHODS', style: TextStyle(fontWeight: FontWeight.w900, fontSize: 10, color: AppTheme.royalGold, letterSpacing: 1.5)),
              ),
              methodsAsync.when(
                data: (methods) => methods.isEmpty 
                  ? const GlassContainer(child: Center(child: Text('No saved payment methods', style: TextStyle(color: Colors.white38, fontSize: 11))))
                  : Column(
                      children: methods.map((m) => Padding(
                        padding: const EdgeInsets.only(bottom: 8),
                        child: GlassContainer(
                          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                          child: Row(
                            children: [
                              Icon(m['type'] == 'Card' ? Icons.credit_card_outlined : Icons.account_balance_wallet_outlined, size: 18, color: AppTheme.royalGold),
                              const SizedBox(width: 12),
                              Text(m['provider'] ?? 'SECURE METHOD', style: const TextStyle(color: Colors.white, fontSize: 13, fontWeight: FontWeight.bold)),
                              const Spacer(),
                              Text('**** ${m['lastFour'] ?? 'XXXX'}', style: const TextStyle(color: Colors.white54, fontSize: 12)),
                              const SizedBox(width: 16),
                              IconButton(
                                icon: const Icon(Icons.delete_outline, color: Colors.redAccent, size: 16),
                                onPressed: () async {
                                    final success = await ref.read(financialServiceProvider).deleteSavedMethod(m['id']);
                                    if (success) ref.invalidate(savedMethodsProvider);
                                },
                              )
                            ],
                          ),
                        ),
                      )).toList(),
                    ),
                loading: () => const Center(child: CircularProgressIndicator()),
                error: (e, s) => const SizedBox(),
              ),
              const SizedBox(height: 40),
              
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  const Padding(
                    padding: EdgeInsets.only(left: 4),
                    child: Text('TRANSACTION HISTORY', style: TextStyle(fontWeight: FontWeight.w900, fontSize: 11, color: AppTheme.royalGold, letterSpacing: 1.5)),
                  ),
                  if (ledgerAsync.hasValue && ledgerAsync.value!.isNotEmpty)
                    Text(
                      'Showing ${ledgerAsync.value!.length} Transactions',
                      style: const TextStyle(fontSize: 9, color: AppTheme.royalGold, fontWeight: FontWeight.bold),
                    ),
                ],
              ),
              const SizedBox(height: 16),

              AppSearchField(
                controller: _searchController,
                hintText: 'Search transactions...',
                onChanged: (v) => ref.read(ledgerSearchQueryProvider.notifier).state = v,
                onClear: () {
                  _searchController.clear();
                  ref.read(ledgerSearchQueryProvider.notifier).state = "";
                },
              ),
              const SizedBox(height: 20),

              ledgerAsync.when(
                data: (items) {
                  final filtered = items.where((item) {
                     final desc = (item['description'] ?? '').toString().toLowerCase();
                     return desc.contains(searchQuery);
                  }).toList();

                  if (filtered.isEmpty) {
                    return Center(child: Padding(
                      padding: const EdgeInsets.all(40), 
                      child: Text(
                        searchQuery.isEmpty ? 'No transaction history found.' : 'No transactions match your search.', 
                        textAlign: TextAlign.center,
                        style: const TextStyle(color: AppTheme.textSecondaryDark, fontSize: 12))
                    ));
                  }

                  return Column(
                    children: filtered.map((item) => Padding(
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
                                  Text(item['description'] ?? 'Alumni Contribution', style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 13, color: Colors.white, letterSpacing: -0.2)),
                                  const SizedBox(height: 2),
                                  Text(AppUtils.formatDate(item['date']), style: const TextStyle(fontSize: 10, color: AppTheme.textSecondaryDark, fontWeight: FontWeight.bold)),
                                ],
                              ),
                            ),
                            Column(
                                crossAxisAlignment: CrossAxisAlignment.end,
                                children: [
                                    Text(AppUtils.formatCurrency(item['amount']), style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 13, color: AppTheme.royalGold)),
                                    const SizedBox(height: 4),
                                    GestureDetector(
                                        onTap: () {
                                            HapticFeedback.lightImpact();
                                            _downloadReceipt(item['id']);
                                        },
                                        child: const Text('GET RECEIPT', style: TextStyle(fontSize: 8.5, color: Colors.white38, decoration: TextDecoration.underline, fontWeight: FontWeight.w900, letterSpacing: 0.5)),
                                    )
                                ]
                            ),
                          ],
                        ),
                      ),
                    )).toList(),
                  );
                },
                loading: () => const Center(child: Padding(padding: EdgeInsets.all(40), child: CircularProgressIndicator(color: AppTheme.royalGold))),
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
