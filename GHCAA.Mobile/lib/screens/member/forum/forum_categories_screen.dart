import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/widgets/glass_container.dart';
import '../../../core/widgets/app_scaffold.dart';
import '../../../core/widgets/async_value_widget.dart';
import '../../../core/widgets/empty_state_widget.dart';
import '../../../features/forum/forum_service.dart';

final forumSearchQueryProvider = StateProvider.autoDispose<String>((ref) => "");

class ForumCategoriesScreen extends ConsumerStatefulWidget {
  const ForumCategoriesScreen({super.key});

  @override
  ConsumerState<ForumCategoriesScreen> createState() => _ForumCategoriesScreenState();
}

class _ForumCategoriesScreenState extends ConsumerState<ForumCategoriesScreen> {
  final TextEditingController _searchController = TextEditingController();

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final categoriesAsync = ref.watch(forumCategoriesProvider);
    final searchQuery = ref.watch(forumSearchQueryProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      title: 'Discussions',
      breadcrumb: 'PORTAL > DISCUSSIONS',
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Search categories...',
                prefixIcon: const Icon(Icons.search, size: 20, color: AppTheme.royalGold),
                suffixIcon: _searchController.text.isNotEmpty 
                  ? IconButton(
                      icon: const Icon(Icons.cancel_rounded, size: 18, color: Colors.white24),
                      onPressed: () {
                        _searchController.clear();
                        ref.read(forumSearchQueryProvider.notifier).state = "";
                      },
                    )
                  : null,
                filled: true,
                fillColor: isDark ? Colors.black.withValues(alpha: 0.2) : Colors.white,
                contentPadding: const EdgeInsets.symmetric(horizontal: 20, vertical: 14),
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(16),
                  borderSide: BorderSide(color: AppTheme.royalGold.withValues(alpha: 0.1)),
                ),
              ),
              onChanged: (v) => ref.read(forumSearchQueryProvider.notifier).state = v.toLowerCase(),
            ),
          ),
          Expanded(
            child: AsyncValueWidget<List<ForumCategory>>(
              value: categoriesAsync,
              loadingMessage: 'Loading categories...',
              onRetry: () => ref.invalidate(forumCategoriesProvider),
              data: (categories) {
                final filtered = categories.where((cat) {
                  return cat.name.toLowerCase().contains(searchQuery) ||
                      (cat.description?.toLowerCase().contains(searchQuery) ?? false);
                }).toList();

                return RefreshIndicator(
                  color: AppTheme.royalGold,
                  onRefresh: () async {
                    HapticFeedback.mediumImpact();
                    ref.invalidate(forumCategoriesProvider);
                  },
                  child: filtered.isEmpty
                      ? EmptyStateWidget(
                          searchQuery.isEmpty ? 'No discussion categories found.' : 'No categories match your search.',
                          icon: Icons.forum_outlined,
                        )
                      : ListView.builder(
                          padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                          itemCount: filtered.length,
                          itemBuilder: (context, index) {
                            final cat = filtered[index];

                            return Padding(
                              padding: const EdgeInsets.only(bottom: 16.0),
                              child: GlassContainer(
                                padding: EdgeInsets.zero,
                                child: InkWell(
                                  onTap: () {
                                    HapticFeedback.lightImpact();
                                    context.push('/forum/topics/${cat.id}');
                                  },
                                  borderRadius: BorderRadius.circular(AppTheme.radiusL),
                                  child: Padding(
                                    padding: const EdgeInsets.all(18.0),
                                    child: Column(
                                      crossAxisAlignment: CrossAxisAlignment.start,
                                      children: [
                                        Row(
                                          children: [
                                            Expanded(
                                              child: Text(
                                                cat.name,
                                                style: const TextStyle(
                                                  fontFamily: 'Outfit',
                                                  fontWeight: FontWeight.w900,
                                                  fontSize: 16,
                                                  color: AppTheme.royalGold,
                                                  letterSpacing: -0.2,
                                                ),
                                              ),
                                            ),
                                            const Icon(
                                              Icons.arrow_forward_ios_rounded,
                                              size: 14,
                                              color: AppTheme.royalGold,
                                            ),
                                          ],
                                        ),
                                        if (cat.description != null && cat.description!.isNotEmpty) ...[
                                          const SizedBox(height: 6),
                                          Text(
                                            cat.description!,
                                            style: const TextStyle(
                                              color: AppTheme.textSecondaryDark,
                                              fontSize: 12,
                                              height: 1.4,
                                            ),
                                          ),
                                        ],
                                        const SizedBox(height: 14),
                                        const Divider(color: Colors.white10, height: 1),
                                        const SizedBox(height: 12),
                                        Row(
                                          children: [
                                            _buildBadge(Icons.topic_outlined, '${cat.topicCount} Topics'),
                                            const SizedBox(width: 20),
                                            _buildBadge(Icons.comment_outlined, '${cat.postCount} Posts'),
                                          ],
                                        ),
                                      ],
                                    ),
                                  ),
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
    );
  }

  Widget _buildBadge(IconData icon, String label) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(icon, size: 14, color: AppTheme.royalGold.withValues(alpha: 0.7)),
        const SizedBox(width: 6),
        Text(
          label,
          style: const TextStyle(
            color: AppTheme.textSecondaryDark,
            fontSize: 11,
            fontWeight: FontWeight.bold,
            letterSpacing: 0.2,
          ),
        ),
      ],
    );
  }
}
