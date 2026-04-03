import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_tile.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../features/activity/activity_service.dart';

final auditSearchQueryProvider = StateProvider.autoDispose<String>((ref) => "");

final globalAuditProvider = FutureProvider.autoDispose<List<dynamic>>((ref) => ref.read(activityServiceProvider).getGlobalActivity());

class AdminAuditScreen extends ConsumerStatefulWidget {
  const AdminAuditScreen({super.key});

  @override
  ConsumerState<AdminAuditScreen> createState() => _AdminAuditScreenState();
}

class _AdminAuditScreenState extends ConsumerState<AdminAuditScreen> {
  final TextEditingController _searchController = TextEditingController();

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final auditAsync = ref.watch(globalAuditProvider);
    final searchQuery = ref.watch(auditSearchQueryProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      isAdmin: true,
      title: 'Audit Logs',
      breadcrumb: 'ADMIN > AUDIT',
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Search audit logs...',
                prefixIcon: const Icon(Icons.search, size: 20, color: AppTheme.royalGold),
                suffixIcon: _searchController.text.isNotEmpty 
                  ? IconButton(
                      icon: const Icon(Icons.close_rounded, size: 20, color: Colors.white54),
                      onPressed: () {
                        _searchController.clear();
                        ref.read(auditSearchQueryProvider.notifier).state = "";
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
              onChanged: (v) => ref.read(auditSearchQueryProvider.notifier).state = v.toLowerCase(),
            ),
          ),
          Expanded(
            child: AsyncValueWidget<List<dynamic>>(
              value: auditAsync,
              loadingMessage: 'Loading logs...',
              onRetry: () => ref.invalidate(globalAuditProvider),
              data: (logs) {
                final filtered = logs.where((log) {
                  final action = log['action']?.toString().toLowerCase() ?? '';
                  final details = log['details']?.toString().toLowerCase() ?? '';
                  final memberId = log['memberId']?.toString().toLowerCase() ?? '';
                  return action.contains(searchQuery) || details.contains(searchQuery) || memberId.contains(searchQuery);
                }).toList();

                return Column(
                  children: [
                    if (filtered.isNotEmpty)
                      Padding(
                        padding: const EdgeInsets.only(left: 24, bottom: 8),
                        child: Align(
                          alignment: Alignment.centerLeft,
                          child: Text(
                            'Showing ${filtered.length} of ${logs.length} audit logs',
                            style: TextStyle(fontSize: 10, color: AppTheme.royalGold.withValues(alpha: 0.7), fontWeight: FontWeight.bold),
                          ),
                        ),
                      ),
                    Expanded(
                      child: filtered.isEmpty
                        ? Center(child: Text(searchQuery.isEmpty ? 'No audit logs found.' : 'No logs match your search.', style: const TextStyle(color: Colors.white38, fontSize: 11, fontStyle: FontStyle.italic)))
                        : ListView.builder(
                            padding: const EdgeInsets.all(20.0),
                            itemCount: filtered.length,
                            itemBuilder: (context, index) {
                              final log = filtered[index];
                              return Padding(
                                padding: const EdgeInsets.only(bottom: 12.0),
                                child: GlassTile(
                                  icon: _getIconForAction(log['action']),
                                  title: log['action'] ?? 'System Event',
                                  subtitle: 'By Member #${log['memberId']} | ${log['createdAt'] ?? 'Date Unknown'}',
                                  onTap: () => _showDetails(context, log),
                                  trailing: const Icon(Icons.info_outline, size: 16, color: AppTheme.royalGold),
                                ),
                              );
                            },
                          ),
                    ),
                  ],
                );
              },
            ),
          ),
        ],
      ),
    );
  }

  IconData _getIconForAction(String? action) {
    if (action == null) return Icons.history_edu;
    final a = action.toLowerCase();
    if (a.contains('login')) return Icons.login;
    if (a.contains('approve')) return Icons.verified_user_sharp;
    if (a.contains('update')) return Icons.edit_note;
    if (a.contains('delete')) return Icons.delete_sweep;
    if (a.contains('export')) return Icons.file_download;
    return Icons.history_edu;
  }

  void _showDetails(BuildContext context, dynamic log) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: AppTheme.midnightSurface,
        title: Text(log['action']?.toUpperCase() ?? 'LOG DATA', style: const TextStyle(color: AppTheme.royalGold, fontSize: 16)),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _detailRow('Details', log['details']),
            _detailRow('Timestamp', log['createdAt']),
            _detailRow('Member ID', log['memberId']?.toString()),
            _detailRow('IP Address', log['ipAddress']),
            _detailRow('Source', log['source']),
            _detailRow('User Agent', log['userAgent']),
          ],
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('ABORT', style: TextStyle(color: Colors.white54)))
        ],
      ),
    );
  }

  Widget _detailRow(String label, String? value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4.0),
      child: Text('$label: ${value ?? 'N/A'}', style: const TextStyle(fontSize: 12, color: Colors.white70)),
    );
  }
}
