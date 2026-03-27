import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../features/networking/networking_service.dart';

final alumniListProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(networkingServiceProvider).searchAlumni();
});

final directorySearchQueryProvider = StateProvider<String>((ref) => '');

class DirectoryScreen extends ConsumerWidget {
  const DirectoryScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final alumniAsync = ref.watch(alumniListProvider);
    final searchQuery = ref.watch(directorySearchQueryProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      title: 'Global Directory',
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16.0),
        child: Column(
          children: [
            const SizedBox(height: 16),
            TextField(
              decoration: InputDecoration(
                hintText: 'Search by Name, Batch, or Domain...',
                prefixIcon: const Icon(Icons.search, color: AppTheme.royalGold),
                filled: true,
                fillColor: isDark ? AppTheme.midnightSurface : Colors.white,
                border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(16),
                    borderSide: BorderSide.none),
              ),
              onChanged: (value) {
                ref.read(directorySearchQueryProvider.notifier).state = value.toLowerCase();
              },
            ),
            const SizedBox(height: 16),
            Expanded(
              child: AsyncValueWidget<List<dynamic>>(
                value: alumniAsync,
                loadingMessage: 'Synchronizing directory...',
                onRetry: () => ref.invalidate(alumniListProvider),
                data: (members) {
                  final filtered = members.where((m) {
                    final name = (m['fullName'] ?? '').toString().toLowerCase();
                    final batch = (m['batch'] ?? '').toString().toLowerCase();
                    final domain = (m['currentDesignation'] ?? '').toString().toLowerCase();
                    return name.contains(searchQuery) || batch.contains(searchQuery) || domain.contains(searchQuery);
                  }).toList();

                  return RefreshIndicator(
                    color: AppTheme.royalGold,
                    onRefresh: () async {
                      HapticFeedback.mediumImpact();
                      ref.invalidate(alumniListProvider);
                    },
                    child: filtered.isEmpty
                        ? const Center(child: Text('No results found for your search.'))
                        : ListView.builder(
                            physics: const AlwaysScrollableScrollPhysics(),
                            itemCount: filtered.length,
                            itemBuilder: (context, index) {
                              final member = filtered[index];
                              return Padding(
                                padding: const EdgeInsets.only(bottom: 12.0),
                                child: GlassContainer(
                                  padding: const EdgeInsets.all(16.0),
                                  child: Row(
                                    children: [
                                      CircleAvatar(
                                        radius: 24,
                                        backgroundColor: AppTheme.royalGold.withValues(alpha: 0.1),
                                        child: Text(member['fullName']?[0] ?? '?',
                                            style: const TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.bold)),
                                      ),
                                      const SizedBox(width: 16),
                                      Expanded(
                                        child: Column(
                                          crossAxisAlignment: CrossAxisAlignment.start,
                                          children: [
                                            Text(member['fullName'] ?? 'Anonymous',
                                                style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16, color: Colors.white)),
                                            const SizedBox(height: 4),
                                            Text('${member['batch'] ?? 'Batch'} • ${member['currentDesignation'] ?? 'Alumnus'}',
                                                style: const TextStyle(fontSize: 12, color: AppTheme.textSecondaryDark)),
                                          ],
                                        ),
                                      ),
                                      const Icon(Icons.chevron_right, size: 16, color: AppTheme.royalGold)
                                    ],
                                  ),
                                ),
                              );
                            },
                          ),
                  );
                },
              ),
            ),
          ],
        ),
      ),
    );
  }
}
