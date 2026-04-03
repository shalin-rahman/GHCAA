import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter/services.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/api/api_client.dart';
import '../../features/admin/admin_service.dart';

final ecPeriodsAdminProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(adminServiceProvider).getECPeriods();
});

final governanceSearchQueryProvider = StateProvider.autoDispose<String>((ref) => "");

class AdminGovernanceScreen extends ConsumerStatefulWidget {
  const AdminGovernanceScreen({super.key});

  @override
  ConsumerState<AdminGovernanceScreen> createState() => _AdminGovernanceState();
}

class _AdminGovernanceState extends ConsumerState<AdminGovernanceScreen> {
  final TextEditingController _searchController = TextEditingController();

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final periodsAsync = ref.watch(ecPeriodsAdminProvider);
    final searchQuery = ref.watch(governanceSearchQueryProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      isAdmin: true,
      title: 'Executive Committee',
      breadcrumb: 'ADMIN > GOVERNANCE',
      floatingActionButton: FloatingActionButton(
        backgroundColor: AppTheme.royalGold,
        onPressed: _createPeriod,
        child: const Icon(Icons.add, color: Colors.black),
      ),
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Search governance terms...',
                prefixIcon: const Icon(Icons.search, size: 20, color: AppTheme.royalGold),
                suffixIcon: _searchController.text.isNotEmpty 
                  ? IconButton(
                      icon: const Icon(Icons.close_rounded, size: 20, color: Colors.white54),
                      onPressed: () {
                        _searchController.clear();
                        ref.read(governanceSearchQueryProvider.notifier).state = "";
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
              onChanged: (v) => ref.read(governanceSearchQueryProvider.notifier).state = v.toLowerCase(),
            ),
          ),
          Expanded(
            child: periodsAsync.when(
              data: (periods) {
                final filtered = periods.where((p) {
                  final title = (p['title'] ?? '').toString().toLowerCase();
                  return title.contains(searchQuery);
                }).toList();

                return Column(
                  children: [
                    if (filtered.isNotEmpty)
                      Padding(
                        padding: const EdgeInsets.only(left: 24, bottom: 8),
                        child: Align(
                          alignment: Alignment.centerLeft,
                          child: Text(
                            'Showing ${filtered.length} of ${periods.length} committee terms',
                            style: TextStyle(fontSize: 10, color: AppTheme.royalGold.withValues(alpha: 0.7), fontWeight: FontWeight.bold),
                          ),
                        ),
                      ),
                    Expanded(
                      child: RefreshIndicator(
                        color: AppTheme.royalGold,
                        onRefresh: () async {
                          HapticFeedback.mediumImpact();
                          ref.invalidate(ecPeriodsAdminProvider);
                        },
                        child: filtered.isEmpty 
                          ? Center(child: Text(searchQuery.isEmpty ? 'No committee periods found.' : 'No terms match your search.', style: const TextStyle(color: Colors.white54, fontSize: 11, fontStyle: FontStyle.italic)))
                          : ListView.builder(
                              padding: const EdgeInsets.all(20),
                              itemCount: filtered.length,
                              itemBuilder: (context, index) {
                                final p = filtered[index];
                                return Padding(
                                  padding: const EdgeInsets.only(bottom: 16),
                                  child: GlassContainer(
                                    padding: const EdgeInsets.all(20),
                                    child: Column(
                                      crossAxisAlignment: CrossAxisAlignment.start,
                                      children: [
                                        Row(
                                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                          children: [
                                            Expanded(
                                              child: Text(p['title']?.toUpperCase() ?? 'EC PERIOD', 
                                                style: const TextStyle(fontWeight: FontWeight.w900, color: Colors.white, fontSize: 13, letterSpacing: 0.5)),
                                            ),
                                            _buildStatusBadge(p['isActive'] == true),
                                          ],
                                        ),
                                        const SizedBox(height: 12),
                                        Text('${p['startDate'].split('T')[0]} — ${p['endDate'] != null ? p['endDate'].split('T')[0] : 'PRESENT'}', 
                                          style: const TextStyle(color: AppTheme.textSecondaryDark, fontSize: 11, fontWeight: FontWeight.bold)),
                                        const Divider(color: Colors.white10, height: 32),
                                        Row(
                                          children: [
                                            TextButton.icon(
                                              onPressed: () => _showMembers(p['id'], p['title']),
                                              icon: const Icon(Icons.people_alt_outlined, size: 16, color: AppTheme.royalGold),
                                              label: const Text('GOVERNANCE NODES', style: TextStyle(color: AppTheme.royalGold, fontSize: 11, fontWeight: FontWeight.bold)),
                                            ),
                                            const Spacer(),
                                            if (p['isActive'] != true)
                                              IconButton(
                                                icon: const Icon(Icons.bolt, color: AppTheme.royalGold, size: 18),
                                                onPressed: () => _activatePeriod(p['id']),
                                              ),
                                            IconButton(
                                              icon: const Icon(Icons.edit_outlined, color: Colors.white70, size: 18),
                                              onPressed: () => _editPeriod(p),
                                            ),
                                          ],
                                        ),
                                      ],
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
              error: (e, s) => Center(child: Text('Sync Error: $e')),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildStatusBadge(bool isActive) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: isActive ? Colors.green.withValues(alpha: 0.1) : Colors.white10,
        borderRadius: BorderRadius.circular(4),
        border: Border.all(color: isActive ? Colors.greenAccent : Colors.white24, width: 0.5),
      ),
      child: Text(isActive ? 'ACTIVE TERM' : 'ARCHIVED', 
        style: TextStyle(color: isActive ? Colors.greenAccent : Colors.white38, fontSize: 8, fontWeight: FontWeight.w900, letterSpacing: 1)),
    );
  }

  void _showMembers(int periodId, String title) {
    showModalBottomSheet(
      context: context,
      backgroundColor: AppTheme.midnightSurface,
      isScrollControlled: true,
      builder: (ctx) => Padding(
        padding: EdgeInsets.only(bottom: MediaQuery.of(ctx).viewInsets.bottom),
        child: Container(
          padding: const EdgeInsets.all(24),
          height: MediaQuery.of(ctx).size.height * 0.7,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text('Members: $title', style: const TextStyle(color: AppTheme.royalGold, fontSize: 16, fontWeight: FontWeight.bold)),
              const Divider(color: Colors.white24, height: 32),
              // We simulate the assignment form
              TextField(
                decoration: InputDecoration(
                  labelText: 'Membership Number',
                  labelStyle: const TextStyle(color: Colors.white70),
                  enabledBorder: const OutlineInputBorder(borderSide: BorderSide(color: Colors.white24)),
                  focusedBorder: const OutlineInputBorder(borderSide: BorderSide(color: AppTheme.royalGold)),
                  suffixIcon: IconButton(
                    icon: const Icon(Icons.add_circle, color: AppTheme.royalGold),
                    onPressed: () async {
                      // Implement assignment logic
                      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Member added (Simulation)')));
                    },
                  ),
                ),
                style: const TextStyle(color: Colors.white),
              ),
              const SizedBox(height: 16),
              const Expanded(
                child: Center(child: Text('Current members will appear here.', style: TextStyle(color: Colors.white54))),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Future<void> _activatePeriod(int id) async {
    try {
      final dio = ref.read(dioProvider);
      await dio.post('/admin/governance/periods/$id/activate');
      ref.invalidate(ecPeriodsAdminProvider);
    } catch (_) {}
  }

  void _editPeriod(dynamic period) {
    _showPeriodDialog(period: period);
  }

  void _createPeriod() {
    _showPeriodDialog();
  }

  void _showPeriodDialog({dynamic period}) {
    final titleCtrl = TextEditingController(text: period != null ? period['title'] : '');
    final startCtrl = TextEditingController(text: period != null ? period['startDate']?.split('T')[0] : '');
    final endCtrl = TextEditingController(text: period != null ? period['endDate']?.split('T')[0] : '');

    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: AppTheme.midnightSurface,
        title: Text(period == null ? 'ESTABLISH GOVERNANCE TERM' : 'MODIFY TERM ASSET', style: const TextStyle(color: AppTheme.royalGold, fontSize: 14, fontWeight: FontWeight.bold)),
        content: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextField(controller: titleCtrl, style: const TextStyle(color: Colors.white), decoration: const InputDecoration(labelText: 'Term Title (e.g., EC 2026-2028)')),
              TextField(controller: startCtrl, style: const TextStyle(color: Colors.white), decoration: const InputDecoration(labelText: 'Start Date (YYYY-MM-DD)')),
              TextField(controller: endCtrl, style: const TextStyle(color: Colors.white), decoration: const InputDecoration(labelText: 'End Date (YYYY-MM-DD) - Optional')),
            ],
          ),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('ABORT', style: TextStyle(color: Colors.white54))),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold),
            onPressed: () async {
              try {
                final dio = ref.read(dioProvider);
                final payload = {
                  'title': titleCtrl.text,
                  'startDate': startCtrl.text,
                  if (endCtrl.text.isNotEmpty) 'endDate': endCtrl.text,
                };
                
                if (period == null) {
                  await dio.post('/admin/governance/periods', data: payload);
                } else {
                  await dio.put('/admin/governance/periods/${period['id']}', data: payload);
                }
                
                ref.invalidate(ecPeriodsAdminProvider);
                if (ctx.mounted) Navigator.pop(ctx);
              } catch (e) {
                if (ctx.mounted) {
                  ScaffoldMessenger.of(ctx).showSnackBar(SnackBar(content: Text('Error saving term: $e')));
                }
              }
            },
            child: const Text('COMMIT CHANGES', style: TextStyle(color: Colors.black, fontWeight: FontWeight.bold)),
          ),
        ],
      ),
    );
  }
}
