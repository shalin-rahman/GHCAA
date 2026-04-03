import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../features/activity/activity_service.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../core/widgets/glass_tile.dart';

final activityHistoryTrackerProvider = FutureProvider.autoDispose<List<dynamic>>((ref) => ref.read(activityServiceProvider).getMyActivity());
final activitySearchQueryProvider = StateProvider.autoDispose<String>((ref) => "");

class MemberActivityHistoryScreen extends ConsumerStatefulWidget {
  const MemberActivityHistoryScreen({super.key});

  @override
  ConsumerState<MemberActivityHistoryScreen> createState() => _MemberActivityHistoryScreenState();
}

class _MemberActivityHistoryScreenState extends ConsumerState<MemberActivityHistoryScreen> {
  final TextEditingController _searchController = TextEditingController();

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final activityAsync = ref.watch(activityHistoryTrackerProvider);
    final searchQuery = ref.watch(activitySearchQueryProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      title: 'Activity Record',
      breadcrumb: 'Identity Registry > Action History',
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Search action history...',
                prefixIcon: const Icon(Icons.search, size: 20, color: AppTheme.royalGold),
                suffixIcon: _searchController.text.isNotEmpty 
                  ? IconButton(
                      icon: const Icon(Icons.close_rounded, size: 20, color: Colors.white54),
                      onPressed: () {
                        _searchController.clear();
                        ref.read(activitySearchQueryProvider.notifier).state = "";
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
              onChanged: (v) => ref.read(activitySearchQueryProvider.notifier).state = v.toLowerCase(),
            ),
          ),
          Expanded(
            child: AsyncValueWidget<List<dynamic>>(
              value: activityAsync,
              loadingMessage: 'Synchronizing activity details...',
              onRetry: () => ref.invalidate(activityHistoryTrackerProvider),
              data: (logs) {
                final filtered = logs.where((log) {
                  final action = (log['action'] ?? '').toString().toLowerCase();
                  final details = (log['details'] ?? '').toString().toLowerCase();
                  return action.contains(searchQuery) || details.contains(searchQuery);
                }).toList();

                return Column(
                  children: [
                    if (filtered.isNotEmpty)
                      Padding(
                        padding: const EdgeInsets.only(left: 24, bottom: 8),
                        child: Align(
                          alignment: Alignment.centerLeft,
                          child: Text(
                            'Showing ${filtered.length} of ${logs.length} activity records',
                            style: TextStyle(fontSize: 10, color: AppTheme.royalGold.withValues(alpha: 0.7), fontWeight: FontWeight.bold),
                          ),
                        ),
                      ),
                    Expanded(
                      child: RefreshIndicator(
                        color: AppTheme.royalGold,
                        onRefresh: () async {
                          HapticFeedback.mediumImpact();
                          ref.invalidate(activityHistoryTrackerProvider);
                        },
                        child: filtered.isEmpty 
                          ? Center(child: Padding(
                              padding: const EdgeInsets.all(40.0),
                              child: Text(
                                searchQuery.isEmpty ? 'No activity records synchronized.' : 'No items match your search.', 
                                style: const TextStyle(color: AppTheme.textSecondaryDark)),
                            ))
                          : ListView.builder(
                              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                              itemCount: filtered.length,
                              itemBuilder: (context, index) {
                                final log = filtered[index];
                                return Padding(
                                  padding: const EdgeInsets.only(bottom: 12.0),
                                  child: GlassTile(
                                    icon: _getIconForAction(log['action']),
                                    title: (log['action'] ?? 'SYSTEM EVENT').toUpperCase(),
                                    subtitle: '${log['details']}\n${log['createdAt'] ?? ''}',
                                    onTap: () {
                                      HapticFeedback.lightImpact();
                                    },
                                  ),
                                );
                              },
                            ),
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
    if (action == null) return Icons.history;
    final a = action.toLowerCase();
    if (a.contains('login')) return Icons.login;
    if (a.contains('update')) return Icons.edit_note;
    if (a.contains('password')) return Icons.lock_reset;
    if (a.contains('apply')) return Icons.assignment_outlined;
    return Icons.history;
  }
}
