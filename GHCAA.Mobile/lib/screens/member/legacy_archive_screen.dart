import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../core/widgets/glass_container.dart';
import '../../features/archive/archive_service.dart';

final archiveCollectionsProvider =
    FutureProvider.autoDispose<List<ArchiveCollection>>(
  (ref) => ref.read(archiveServiceProvider).getPublicCollections(),
);

class LegacyArchiveScreen extends ConsumerWidget {
  const LegacyArchiveScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final archive = ref.watch(archiveCollectionsProvider);
    return AppScaffold(
      title: 'Oral History Archive',
      breadcrumb: 'PORTAL > ORAL HISTORY',
      child: AsyncValueWidget<List<ArchiveCollection>>(
        value: archive,
        loadingMessage: 'Loading the oral-history archive...',
        onRetry: () => ref.invalidate(archiveCollectionsProvider),
        data: (collections) => collections.isEmpty
            ? const EmptyStateWidget(
                'No archive collections are available yet.',
                icon: Icons.history_edu_outlined,
              )
            : ListView.separated(
                padding: const EdgeInsets.all(AppTheme.spaceL),
                itemCount: collections.length,
                separatorBuilder: (_, __) =>
                    const SizedBox(height: AppTheme.spaceM),
                itemBuilder: (context, index) {
                  final collection = collections[index];
                  return GlassContainer(
                    child: ListTile(
                      leading: Icon(Icons.history_edu,
                          color: Theme.of(context).colorScheme.secondary),
                      title: Text(collection.title,
                          style: const TextStyle(fontWeight: FontWeight.w800)),
                      subtitle: Text(
                        collection.description ??
                            (collection.decade == null
                                ? 'Community history'
                                : '${collection.decade}s'),
                      ),
                    ),
                  );
                },
              ),
      ),
    );
  }
}
