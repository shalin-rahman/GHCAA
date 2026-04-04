import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../features/networking/networking_service.dart';
import '../../core/config/app_config.dart';

final ecPeriodsProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(networkingServiceProvider).getECPeriods();
});

final selectedECPeriodProvider = StateProvider.autoDispose<int?>((ref) => null);

final committeeListProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  final periods = await ref.watch(ecPeriodsProvider.future);
  var periodId = ref.watch(selectedECPeriodProvider);
  // Auto-select the first (current) period if none chosen yet
  if (periodId == null && periods.isNotEmpty) {
    periodId = periods.first['id'] as int?;
    Future.microtask(() => ref.read(selectedECPeriodProvider.notifier).state = periodId);
  }
  return ref.read(networkingServiceProvider).getExecutiveCommittee(periodId: periodId);
});

final committeeSearchQueryProvider = StateProvider.autoDispose<String>((ref) => "");

class CommitteeScreen extends ConsumerStatefulWidget {
  const CommitteeScreen({super.key});

  @override
  ConsumerState<CommitteeScreen> createState() => _CommitteeScreenState();
}

class _CommitteeScreenState extends ConsumerState<CommitteeScreen> {
  final TextEditingController _searchController = TextEditingController();

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final committeeAsync = ref.watch(committeeListProvider);
    final periodsAsync = ref.watch(ecPeriodsProvider);
    final selectedPeriod = ref.watch(selectedECPeriodProvider);
    final searchQuery = ref.watch(committeeSearchQueryProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      title: 'Executive Committee',
      breadcrumb: 'Association Hub > Governance Registry',
      child: Column(
        children: [
          // Period Selector bar
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 8),
            color: Colors.black.withValues(alpha: 0.2),
            child: periodsAsync.when(
              data: (periods) => Row(
                children: [
                  const Icon(Icons.history_edu_outlined, color: AppTheme.royalGold, size: 20),
                  const SizedBox(width: 12),
                  const Text('GOVERNANCE TERM:', style: TextStyle(color: AppTheme.textSecondaryDark, fontSize: 10, fontWeight: FontWeight.bold, letterSpacing: 1)),
                  const SizedBox(width: 12),
                  Expanded(
                    child: DropdownButtonHideUnderline(
                      child: DropdownButton<int>(
                        isExpanded: true,
                        dropdownColor: AppTheme.midnightSurface,
                        value: selectedPeriod ?? (periods.isNotEmpty ? periods.first['id'] : null),
                        icon: const Icon(Icons.arrow_drop_down, color: AppTheme.royalGold),
                        style: const TextStyle(color: Colors.white, fontSize: 12, fontWeight: FontWeight.w900),
                        items: periods.map<DropdownMenuItem<int>>((p) {
                          return DropdownMenuItem<int>(
                            value: p['id'] as int,
                            child: Text(p['title'] ?? 'Registry Period', overflow: TextOverflow.ellipsis),
                          );
                        }).toList(),
                        onChanged: (val) {
                          HapticFeedback.lightImpact();
                          ref.read(selectedECPeriodProvider.notifier).state = val;
                        },
                      ),
                    ),
                  ),
                ],
              ),
              loading: () => const LinearProgressIndicator(color: AppTheme.royalGold, minHeight: 2),
              error: (_, __) => const SizedBox(),
            ),
          ),
          // Search bar
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 12, 20, 4),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Search by name or position...',
                prefixIcon: const Icon(Icons.search, size: 20, color: AppTheme.royalGold),
                suffixIcon: _searchController.text.isNotEmpty
                  ? IconButton(
                      icon: const Icon(Icons.close_rounded, size: 20, color: Colors.white54),
                      onPressed: () {
                        _searchController.clear();
                        ref.read(committeeSearchQueryProvider.notifier).state = "";
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
              onChanged: (v) => ref.read(committeeSearchQueryProvider.notifier).state = v.toLowerCase(),
            ),
          ),
          Expanded(
            child: AsyncValueWidget<List<dynamic>>(
              value: committeeAsync,
              loadingMessage: 'Synchronizing Board of Trustees...',
              onRetry: () => ref.invalidate(committeeListProvider),
              data: (members) {
                final filtered = members.where((m) {
                  final name = (m['fullName'] ?? '').toString().toLowerCase();
                  final position = (m['positionName'] ?? '').toString().toLowerCase();
                  return name.contains(searchQuery) || position.contains(searchQuery);
                }).toList();

                return RefreshIndicator(
                  color: AppTheme.royalGold,
                  onRefresh: () async {
                    HapticFeedback.mediumImpact();
                    ref.invalidate(committeeListProvider);
                    ref.invalidate(ecPeriodsProvider);
                  },
                  child: Column(
                    children: [
                      if (filtered.isNotEmpty)
                        Padding(
                          padding: const EdgeInsets.only(left: 24, bottom: 8, top: 4),
                          child: Align(
                            alignment: Alignment.centerLeft,
                            child: Text(
                              'Showing ${filtered.length} of ${members.length} members',
                              style: TextStyle(fontSize: 10, color: AppTheme.royalGold.withValues(alpha: 0.7), fontWeight: FontWeight.bold),
                            ),
                          ),
                        ),
                      Expanded(
                        child: filtered.isEmpty
                          ? Center(child: Text(searchQuery.isEmpty ? 'No active committee records found.' : 'No members match your search.', style: const TextStyle(color: AppTheme.textSecondaryDark)))
                          : ListView.builder(
                              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 4),
                              itemCount: filtered.length,
                              itemBuilder: (context, index) {
                                final m = filtered[index];
                            String? photoUrl;
                            if (m['photoPath'] != null && m['photoPath'].toString().isNotEmpty) {
                              if (m['photoPath'].toString().startsWith('http')) {
                                photoUrl = m['photoPath'].toString();
                              } else {
                                final base = AppConfig.apiBaseUrl.endsWith('/') ? AppConfig.apiBaseUrl.substring(0, AppConfig.apiBaseUrl.length - 1) : AppConfig.apiBaseUrl;
                                final cleanP = m['photoPath'].toString().startsWith('/') ? m['photoPath'].toString().substring(1) : m['photoPath'].toString();
                                photoUrl = '$base/$cleanP';
                              }
                            }

                            return Padding(
                              padding: const EdgeInsets.only(bottom: 12.0),
                              child: GlassContainer(
                                padding: const EdgeInsets.all(16),
                                child: Row(
                                  children: [
                                    _buildMemberAvatar(photoUrl, m['fullName']),
                                    const SizedBox(width: 16),
                                    Expanded(
                                      child: Column(
                                        crossAxisAlignment: CrossAxisAlignment.start,
                                        children: [
                                          Text(
                                            (m['fullName'] ?? 'System Member').toUpperCase(),
                                            style: const TextStyle(fontWeight: FontWeight.w900, color: Colors.white, fontSize: 13, letterSpacing: 0.5)
                                          ),
                                          const SizedBox(height: 4),
                                          Text(
                                            '${m['positionName'] ?? 'Member'} | Batch ${m['passingYear'] ?? 'N/A'}',
                                            style: TextStyle(fontSize: 10, color: AppTheme.royalGold.withValues(alpha: 0.8), fontWeight: FontWeight.w900, letterSpacing: 1)
                                          ),
                                        ],
                                      ),
                                    ),
                                    IconButton(
                                      icon: const Icon(Icons.contact_emergency_outlined, size: 18, color: AppTheme.textSecondaryDark),
                                      onPressed: () {
                                        HapticFeedback.lightImpact();
                                        context.push('/directory/${m['memberId']}');
                                      }
                                    ),
                                  ],
                                ),
                              ),
                            );
                          },
                        ),
                      ),
                    ],
                  ),
                );
              },
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildMemberAvatar(String? url, String? name) {
    return Container(
      decoration: BoxDecoration(
        shape: BoxShape.circle,
        border: Border.all(color: AppTheme.royalGold.withValues(alpha: 0.2), width: 1.5),
      ),
      child: CircleAvatar(
        radius: 26,
        backgroundColor: Colors.black26,
        backgroundImage: url != null ? NetworkImage(url) : null,
        child: url == null ? Text(name?[0] ?? '?', style: const TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.bold)) : null,
      ),
    );
  }
}
