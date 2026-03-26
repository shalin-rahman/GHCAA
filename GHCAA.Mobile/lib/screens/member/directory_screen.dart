import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../features/networking/networking_service.dart';

final alumniListProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(networkingServiceProvider).searchAlumni();
});

class DirectoryScreen extends ConsumerWidget {
  const DirectoryScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final alumniAsync = ref.watch(alumniListProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      title: 'Alumni Directory',
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16.0),
        child: Column(
          children: [
            const SizedBox(height: 16),
            TextField(
              decoration: InputDecoration(
                hintText: 'Search by Name, Batch, or Domain...',
                prefixIcon: const Icon(Icons.search),
                filled: true,
                fillColor: isDark ? AppTheme.midnightSurface : Colors.white,
                border: OutlineInputBorder(borderRadius: BorderRadius.circular(16), borderSide: BorderSide.none),
              ),
            ),
            const SizedBox(height: 16),
            Expanded(
              child: AsyncValueWidget(
                value: alumniAsync,
                loadingMessage: 'Synchronizing global directory...',
                onRetry: () => ref.invalidate(alumniListProvider),
                data: (members) => ListView.builder(
                  itemCount: members.length,
                  itemBuilder: (context, index) {
                    final member = members[index];
                    return Padding(
                      padding: const EdgeInsets.only(bottom: 12.0),
                      child: GlassContainer(
                        padding: const EdgeInsets.all(16.0),
                        child: Row(
                          children: [
                            CircleAvatar(
                              radius: 24,
                              backgroundColor: AppTheme.royalGold.withOpacity(0.1),
                              child: Text(member['fullName']?[0] ?? '?', style: const TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.bold)),
                            ),
                            const SizedBox(width: 16),
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(member['fullName'] ?? 'Anonymous', style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                                  const SizedBox(height: 4),
                                  Text('${member['batch'] ?? 'Batch'} • ${member['currentDesignation'] ?? 'Alumnus'}', style: TextStyle(fontSize: 12, color: AppTheme.textSecondaryDark)),
                                ],
                              ),
                            ),
                            Icon(Icons.chevron_right, size: 16, color: AppTheme.royalGold.withOpacity(0.5))
                          ],
                        ),
                      ),
                    );
                  },
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
