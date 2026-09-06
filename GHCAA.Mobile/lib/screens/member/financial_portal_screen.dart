import 'dart:io';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:screen_protector/screen_protector.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../core/utils/app_utils.dart';
import '../../core/widgets/app_search_field.dart';
import '../../features/financials/financial_service.dart';
import '../../features/financials/gateway_service.dart';
import '../../features/files/file_service.dart';
import 'package:go_router/go_router.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../core/widgets/logo_spinner.dart';

final ledgerProvider = FutureProvider<List<dynamic>>((ref) async => ref.read(financialServiceProvider).getLedger());
final duesProvider = FutureProvider<double>((ref) async => ref.read(financialServiceProvider).getOutstandingDues());
final savedMethodsProvider = FutureProvider<List<dynamic>>((ref) async => ref.read(financialServiceProvider).getSavedMethods());
// 29G.1: admin-configured, enabled payment methods (manual channels work without live gateway keys).
final activePaymentConfigsProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async => ref.read(financialServiceProvider).getActivePaymentConfigs());
final ledgerSearchQueryProvider = StateProvider.autoDispose<String>((ref) => "");

class FinancialPortalScreen extends ConsumerStatefulWidget {
  const FinancialPortalScreen({super.key});

  @override
  ConsumerState<FinancialPortalScreen> createState() => _FinancialPortalScreenState();
}

class _FinancialPortalScreenState extends ConsumerState<FinancialPortalScreen> {
  final TextEditingController _searchController = TextEditingController();
  int? _deletingMethodId;

  @override
  void initState() {
    super.initState();
    ScreenProtector.preventScreenshotOn();
  }

  @override
  void dispose() {
    ScreenProtector.preventScreenshotOff();
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

  Future<void> _initiatePayment(double amount, PaymentGateway gateway) async {
    HapticFeedback.lightImpact();
    // Show loading indicator
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (context) => const Center(child: LogoSpinner(size: 120)),
    );

    try {
      final response = await ref.read(gatewayServiceProvider).initiate(amount, gateway, 'OUTSTANDING_DUES');
      if (mounted) Navigator.of(context).pop(); // Dismiss loading

      if (response.success && response.gatewayUrl != null) {
        if (mounted) context.pushNamed('payment_web', extra: response.gatewayUrl);
      } else {
        if (mounted) {
           ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text(response.message ?? 'Failed to initiate payment')),
          );
        }
      }
    } catch (e) {
      if (mounted) {
        Navigator.of(context).pop();
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Error: $e')));
      }
    }
  }

  // 29G.1/29G.3: surface admin-configured methods from /payment-config/active instead of
  // hardcoded gateways. Manual channels (bKash/Nagad/Rocket/Bank/Cash) submit for verification
  // without any live gateway keys; online channels (if an admin enables one) route to the gateway.
  void _showPaymentSheet(double amount) {
    showModalBottomSheet(
      context: context,
      backgroundColor: Colors.transparent,
      isScrollControlled: true,
      builder: (sheetContext) => Consumer(
        builder: (context, ref, _) {
          final configsAsync = ref.watch(activePaymentConfigsProvider);
          return GlassContainer(
            padding: const EdgeInsets.all(24),
            child: configsAsync.when(
              data: (methods) {
                if (methods.isEmpty) {
                  return const EmptyStateWidget(
                    'No payment methods are currently available. Please contact the association office.',
                    icon: Icons.payments_outlined,
                  );
                }
                return Column(
                  mainAxisSize: MainAxisSize.min,
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text('SELECT PAYMENT METHOD', style: TextStyle(fontWeight: FontWeight.w900, fontSize: 12, color: AppTheme.royalGold, letterSpacing: 1.5)),
                    const SizedBox(height: 24),
                    ...methods.map((m) => Padding(
                          padding: const EdgeInsets.only(bottom: 12),
                          child: _methodTile(m, amount),
                        )),
                    const SizedBox(height: 16),
                  ],
                );
              },
              loading: () => const Padding(padding: EdgeInsets.all(40), child: Center(child: LogoSpinner(size: 100))),
              error: (e, s) => Text('Error loading payment methods: $e', style: const TextStyle(color: Colors.redAccent)),
            ),
          );
        },
      ),
    );
  }

  Widget _methodTile(dynamic m, double amount) {
    final isOnline = m['isOnline'] == true;
    final iconStr = (m['icon'] as String?)?.trim();
    return InkWell(
      onTap: () {
        Navigator.pop(context);
        if (isOnline) {
          _initiatePayment(amount, _gatewayFromString(m['gateway']?.toString()));
        } else {
          _showManualPaymentForm(m, amount);
        }
      },
      child: GlassContainer(
        padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
        child: Row(
          children: [
            (iconStr != null && iconStr.isNotEmpty)
                ? Text(iconStr, style: const TextStyle(fontSize: 22))
                : const Icon(Icons.account_balance_wallet_outlined, color: AppTheme.royalGold),
            const SizedBox(width: 16),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(m['displayName'] ?? 'Payment', style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 14)),
                  if ((m['description'] as String?)?.isNotEmpty == true)
                    Padding(
                      padding: const EdgeInsets.only(top: 2),
                      child: Text(m['description'], style: const TextStyle(color: Colors.white38, fontSize: 10)),
                    ),
                ],
              ),
            ),
            Icon(isOnline ? Icons.open_in_new : Icons.chevron_right, color: Colors.white24, size: 18),
          ],
        ),
      ),
    );
  }

  // Manual submission: show where to pay (admin-configured wallet/bank details), collect the
  // member's transaction reference + optional receipt, and POST to record-payment for verification.
  void _showManualPaymentForm(dynamic config, double amount) {
    final txnController = TextEditingController();
    File? receipt;
    bool submitting = false;
    final requiresReference = config['requiresReference'] == true;
    final requiresReceipt = config['requiresReceipt'] == true;
    final displayName = config['displayName']?.toString() ?? 'Payment';

    showModalBottomSheet(
      context: context,
      backgroundColor: Colors.transparent,
      isScrollControlled: true,
      builder: (sheetContext) => Padding(
        padding: EdgeInsets.only(bottom: MediaQuery.of(sheetContext).viewInsets.bottom),
        child: StatefulBuilder(
          builder: (context, setSheetState) {
            Future<void> pickReceipt() async {
              final file = await ref.read(fileServiceProvider).pickImage();
              if (file != null) setSheetState(() => receipt = file);
            }

            Future<void> submit() async {
              final txn = txnController.text.trim();
              if (requiresReference && txn.isEmpty) {
                ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Please enter the transaction / reference ID.')));
                return;
              }
              if (requiresReceipt && receipt == null) {
                ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Please attach your payment receipt.')));
                return;
              }
              setSheetState(() => submitting = true);
              final ok = await ref.read(financialServiceProvider).recordPayment(
                    transactionId: txn.isEmpty ? 'MANUAL-${config['method']}' : txn,
                    amount: amount,
                    paymentMethod: config['method']?.toString() ?? 'ManualReceipt',
                    financialCategory: 'MembershipFee',
                    notes: 'Paid via $displayName (pending verification)',
                    receipt: receipt,
                  );
              if (!context.mounted) return;
              setSheetState(() => submitting = false);
              Navigator.pop(context);
              ScaffoldMessenger.of(context).showSnackBar(SnackBar(
                content: Text(ok ? 'Payment submitted for verification.' : 'Failed to submit payment. Please try again.'),
              ));
              if (ok) {
                ref.invalidate(ledgerProvider);
                ref.invalidate(duesProvider);
              }
            }

            return GlassContainer(
              padding: const EdgeInsets.all(24),
              child: SingleChildScrollView(
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(displayName.toUpperCase(), style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 13, color: AppTheme.royalGold, letterSpacing: 1.2)),
                    const SizedBox(height: 4),
                    Text('Payable: ${AppUtils.formatCurrency(amount)}', style: const TextStyle(color: Colors.white70, fontSize: 12, fontWeight: FontWeight.bold)),
                    const SizedBox(height: 20),
                    // Where to pay — display-only account details from the admin config.
                    _detailRow('Wallet Number', config['walletNumber']),
                    _detailRow('Account Number', config['accountNumber']),
                    _detailRow('Account Holder', config['accountHolderName']),
                    _detailRow('Bank', config['bankName']),
                    _detailRow('Branch', config['branchName']),
                    _detailRow('Routing', config['routingNumber']),
                    if ((config['instructions'] as String?)?.isNotEmpty == true)
                      Padding(
                        padding: const EdgeInsets.only(top: 8, bottom: 4),
                        child: Text(config['instructions'], style: const TextStyle(color: Colors.white54, fontSize: 11, height: 1.4)),
                      ),
                    const SizedBox(height: 20),
                    if (requiresReference) ...[
                      TextField(
                        controller: txnController,
                        style: const TextStyle(color: Colors.white, fontSize: 13),
                        decoration: const InputDecoration(
                          labelText: 'Transaction ID / Reference *',
                          labelStyle: TextStyle(color: Colors.white54, fontSize: 12),
                        ),
                      ),
                      const SizedBox(height: 16),
                    ],
                    // Receipt upload
                    InkWell(
                      onTap: submitting ? null : pickReceipt,
                      child: GlassContainer(
                        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                        child: Row(
                          children: [
                            Icon(receipt != null ? Icons.check_circle_outline : Icons.upload_file_outlined, color: AppTheme.royalGold, size: 18),
                            const SizedBox(width: 12),
                            Expanded(
                              child: Text(
                                receipt != null ? receipt!.path.split('/').last : (requiresReceipt ? 'Attach receipt *' : 'Attach receipt (optional)'),
                                style: const TextStyle(color: Colors.white70, fontSize: 12),
                                overflow: TextOverflow.ellipsis,
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
                    const SizedBox(height: 24),
                    SizedBox(
                      width: double.infinity,
                      child: ElevatedButton(
                        onPressed: submitting ? null : submit,
                        style: ElevatedButton.styleFrom(padding: const EdgeInsets.symmetric(vertical: 16)),
                        child: submitting
                            ? const SizedBox(height: 18, width: 18, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.black))
                            : const Text('SUBMIT PAYMENT', style: TextStyle(fontWeight: FontWeight.w900, fontSize: 12, letterSpacing: 1)),
                      ),
                    ),
                    const SizedBox(height: 8),
                  ],
                ),
              ),
            );
          },
        ),
      ),
    );
  }

  Widget _detailRow(String label, dynamic value) {
    final str = value?.toString();
    if (str == null || str.isEmpty) return const SizedBox.shrink();
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 120,
            child: Text(label.toUpperCase(), style: const TextStyle(color: Colors.white38, fontSize: 9, fontWeight: FontWeight.w900, letterSpacing: 0.5)),
          ),
          Expanded(
            child: Text(str, style: const TextStyle(color: Colors.white, fontSize: 13, fontWeight: FontWeight.bold)),
          ),
        ],
      ),
    );
  }

  // Map the config's PaymentGateway enum name (e.g. "SSLCommerz", "BkashGateway", "DGePay")
  // to the mobile PaymentGateway enum, tolerating case and the "Gateway" suffix.
  PaymentGateway _gatewayFromString(String? gw) {
    if (gw == null) return PaymentGateway.none;
    final key = gw.toLowerCase().replaceAll('gateway', '');
    return PaymentGateway.values.firstWhere(
      (g) => g.name.toLowerCase() == key,
      orElse: () => PaymentGateway.none,
    );
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
                            _showPaymentSheet(dues);
                          } : null, 
                          style: ElevatedButton.styleFrom(padding: const EdgeInsets.symmetric(vertical: 16)),
                          child: const Text('PAY OUTSTANDING', style: TextStyle(fontWeight: FontWeight.w900, fontSize: 12, letterSpacing: 1))
                        )
                      ),
                    ],
                  ),
                ),
                loading: () => const Center(child: Padding(padding: EdgeInsets.all(40), child: LogoSpinner(size: 120))),
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
                  ? const GlassContainer(child: EmptyStateWidget('No saved payment methods', icon: Icons.credit_card_outlined))
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
                                onPressed: _deletingMethodId == m['id'] ? null : () async {
                                    setState(() => _deletingMethodId = m['id']);
                                    try {
                                      final success = await ref.read(financialServiceProvider).deleteSavedMethod(m['id']);
                                      if (success) ref.invalidate(savedMethodsProvider);
                                    } finally {
                                      if (mounted) setState(() => _deletingMethodId = null);
                                    }
                                },
                              )
                            ],
                          ),
                        ),
                      )).toList(),
                    ),
                loading: () => const Center(child: LogoSpinner(size: 120)),
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
                    return EmptyStateWidget(
                      searchQuery.isEmpty ? 'No transaction history found.' : 'No transactions match your search.',
                      icon: Icons.receipt_long_outlined,
                    );
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
                loading: () => const Center(child: Padding(padding: EdgeInsets.all(40), child: LogoSpinner(size: 120))),
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
