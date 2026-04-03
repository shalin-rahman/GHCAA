import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter/services.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../features/admin/admin_service.dart';

final adminLedgerRecordsProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async => ref.read(adminServiceProvider).getLedgerRecords());
final adminLedgerSummaryProvider = FutureProvider.autoDispose<Map<String, dynamic>>((ref) async => ref.read(adminServiceProvider).getLedgerSummary(DateTime.now().year));
final adminLedgerSearchQueryProvider = StateProvider.autoDispose<String>((ref) => "");

class AdminLedgerScreen extends ConsumerStatefulWidget {
  const AdminLedgerScreen({super.key});

  @override
  ConsumerState<AdminLedgerScreen> createState() => _AdminLedgerScreenState();
}

class _AdminLedgerScreenState extends ConsumerState<AdminLedgerScreen> {
  final TextEditingController _searchController = TextEditingController();

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final recordsAsync = ref.watch(adminLedgerRecordsProvider);
    final summaryAsync = ref.watch(adminLedgerSummaryProvider);
    final searchQuery = ref.watch(adminLedgerSearchQueryProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      isAdmin: true,
      title: 'Financial Ledger',
      breadcrumb: 'ADMIN > LEDGER',
      child: Column(
        children: [
          summaryAsync.when(
            data: (sum) => Padding(
              padding: const EdgeInsets.fromLTRB(20, 20, 20, 8),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  _buildStatTile(context, 'REVENUE', '৳ ${sum['totalRevenue'] ?? 0}'),
                  _buildStatTile(context, 'EXPENSES', '৳ ${sum['totalExpenses'] ?? 0}', highlight: true),
                ],
              ),
            ),
            loading: () => const LinearProgressIndicator(color: AppTheme.royalGold),
            error: (_, __) => const SizedBox(),
          ),
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Search transactions...',
                prefixIcon: const Icon(Icons.search, size: 20, color: AppTheme.royalGold),
                suffixIcon: _searchController.text.isNotEmpty 
                  ? IconButton(
                      icon: const Icon(Icons.close_rounded, size: 20, color: Colors.white54),
                      onPressed: () {
                        _searchController.clear();
                        ref.read(adminLedgerSearchQueryProvider.notifier).state = "";
                      },
                    )
                  : null,
                filled: true,
                fillColor: isDark ? Colors.black.withValues(alpha: 0.2) : Colors.white,
                contentPadding: const EdgeInsets.symmetric(horizontal: 20, vertical: 14),
                border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(16),
                    borderSide: BorderSide(color: AppTheme.royalGold.withValues(alpha: 0.1))),
              ),
              onChanged: (v) => ref.read(adminLedgerSearchQueryProvider.notifier).state = v.toLowerCase(),
            ),
          ),
          Expanded(
            child: recordsAsync.when(
              data: (records) {
                final filtered = records.where((r) {
                  final description = (r['description'] ?? '').toString().toLowerCase();
                  final memberId = r['memberId'].toString().toLowerCase();
                  return description.contains(searchQuery) || memberId.contains(searchQuery);
                }).toList();

                return Column(
                  children: [
                    if (filtered.isNotEmpty)
                      Padding(
                        padding: const EdgeInsets.only(left: 24, bottom: 8),
                        child: Align(
                          alignment: Alignment.centerLeft,
                          child: Text(
                            'Showing ${filtered.length} of ${records.length} transactions',
                            style: TextStyle(fontSize: 10, color: AppTheme.royalGold.withValues(alpha: 0.7), fontWeight: FontWeight.bold),
                          ),
                        ),
                      ),
                    Expanded(
                      child: RefreshIndicator(
                        color: AppTheme.royalGold,
                        onRefresh: () async {
                          HapticFeedback.mediumImpact();
                          ref.invalidate(adminLedgerRecordsProvider);
                          ref.invalidate(adminLedgerSummaryProvider);
                        },
                        child: filtered.isEmpty
                          ? Center(child: Text(searchQuery.isEmpty ? 'No financial history found.' : 'No items match your search.', style: const TextStyle(color: Colors.white38, fontSize: 11, fontStyle: FontStyle.italic)))
                          : ListView.builder(
                              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 4),
                              itemCount: filtered.length,
                              itemBuilder: (context, index) {
                                final record = filtered[index];
                                final isRevenue = record['recordType'] == 'Revenue' || record['recordType'] == 0;
                                
                                return Padding(
                                  padding: const EdgeInsets.only(bottom: 12.0),
                                  child: GlassContainer(
                                    child: ListTile(
                                      leading: CircleAvatar(
                                        backgroundColor: (isRevenue ? Colors.greenAccent : Colors.redAccent).withValues(alpha: 0.1),
                                        child: Icon(isRevenue ? Icons.add_circle_outline : Icons.remove_circle_outline, color: isRevenue ? Colors.greenAccent : Colors.redAccent, size: 20),
                                      ),
                                      title: Text(record['description'] ?? 'Transaction Asset', style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13, color: Colors.white)),
                                      subtitle: Text('${record['recordDate']?.split('T')[0] ?? 'Legacy Record'} | Member #${record['memberId']}', style: const TextStyle(fontSize: 10, color: AppTheme.textSecondaryDark)),
                                      trailing: Text('৳ ${record['amount']}', style: TextStyle(fontWeight: FontWeight.w900, fontSize: 14, color: isRevenue ? Colors.greenAccent : Colors.white)),
                                    ),
                                  ),
                                );
                              },
                            ),
                      ),
                    ),
                  ],
                );
              },
              loading: () => const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
              error: (e, s) => Center(child: Text('Registry Sync Error: $e')),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildStatTile(BuildContext context, String title, String value, {bool highlight = false}) {
    return SizedBox(
      width: (MediaQuery.of(context).size.width - 56) / 2,
      child: GlassContainer(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(title.toUpperCase(), style: TextStyle(fontSize: 9, letterSpacing: 1.2, fontWeight: FontWeight.w900, color: AppTheme.royalGold.withValues(alpha: 0.8))),
            const SizedBox(height: 8),
            Text(value, style: TextStyle(fontSize: 18, fontWeight: FontWeight.w900, color: highlight ? Colors.redAccent : Colors.white)),
          ],
        ),
      ),
    );
  }
}
