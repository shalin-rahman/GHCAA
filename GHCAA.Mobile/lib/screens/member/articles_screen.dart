import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../core/widgets/confirm_dialog.dart';
import 'package:flutter/services.dart';
import '../../core/config/app_config.dart';
import '../../features/content/content_service.dart';

final myArticlesProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(newsServiceProvider).getMySubmissions();
});

final articlesSearchQueryProvider = StateProvider.autoDispose<String>((ref) => "");

class MemberArticlesScreen extends ConsumerStatefulWidget {
  const MemberArticlesScreen({super.key});

  @override
  ConsumerState<MemberArticlesScreen> createState() => _MemberArticlesScreenState();
}

class _MemberArticlesScreenState extends ConsumerState<MemberArticlesScreen> {
  final TextEditingController _searchController = TextEditingController();

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  Future<void> _deleteArticle(BuildContext context, int id, String status) async {
    if (status.toLowerCase() == 'approved') {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Published articles cannot be deleted directly. Contact admin.'), backgroundColor: Colors.orangeAccent)
      );
      return;
    }

    final confirm = await showConfirmDialog(
      context,
      title: 'Confirm Delete',
      message: 'Are you sure you want to delete this submission?',
      confirmLabel: 'Delete',
      destructive: true,
    );

    if (!confirm) return;

    try {
      await ref.read(newsServiceProvider).deleteMySubmission(id);
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Article deleted successfully.'))
        );
      }
      ref.invalidate(myArticlesProvider);
    } catch (e) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Failed to delete article.'), backgroundColor: Colors.redAccent)
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final articlesAsync = ref.watch(myArticlesProvider);
    final searchQuery = ref.watch(articlesSearchQueryProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      title: 'Articles & Submissions',
      breadcrumb: 'PORTAL > CONTENT HUB',
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () {
          HapticFeedback.lightImpact();
          context.pushNamed('submit_article').then((_) => ref.invalidate(myArticlesProvider));
        },
        backgroundColor: AppTheme.royalGold,
        icon: const Icon(Icons.history_edu_rounded, color: Colors.black, size: 20),
        label: const Text('SUBMIT STORY', style: TextStyle(color: Colors.black, fontWeight: FontWeight.w900, fontSize: 10, letterSpacing: 1.5)),
      ),
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Search your articles...',
                prefixIcon: const Icon(Icons.search, size: 20, color: AppTheme.royalGold),
                suffixIcon: _searchController.text.isNotEmpty
                  ? IconButton(
                      icon: const Icon(Icons.close_rounded, size: 20, color: Colors.white54),
                      onPressed: () {
                        _searchController.clear();
                        ref.read(articlesSearchQueryProvider.notifier).state = "";
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
              onChanged: (v) => ref.read(articlesSearchQueryProvider.notifier).state = v.toLowerCase(),
            ),
          ),
          Expanded(
            child: AsyncValueWidget<List<dynamic>>(
              value: articlesAsync,
              loadingMessage: 'Synchronizing submission history...',
              onRetry: () => ref.invalidate(myArticlesProvider),
              data: (articles) {
                final filtered = searchQuery.isEmpty
                  ? articles
                  : articles.where((a) {
                      final title = (a['title'] ?? '').toString().toLowerCase();
                      final cat = (a['articleCategory'] ?? '').toString().toLowerCase();
                      return title.contains(searchQuery) || cat.contains(searchQuery);
                    }).toList();

                return RefreshIndicator(
                  color: AppTheme.royalGold,
                  onRefresh: () async {
                    HapticFeedback.mediumImpact();
                    ref.invalidate(myArticlesProvider);
                  },
                  child: Column(
                    children: [
                      if (filtered.isNotEmpty)
                        Padding(
                          padding: const EdgeInsets.only(left: 24, top: 4, bottom: 4),
                          child: Align(
                            alignment: Alignment.centerLeft,
                            child: Text(
                              'Showing ${filtered.length} of ${articles.length} articles',
                              style: TextStyle(fontSize: 10, color: AppTheme.royalGold.withValues(alpha: 0.7), fontWeight: FontWeight.bold),
                            ),
                          ),
                        ),
                      Expanded(
                        child: filtered.isEmpty
                          ? EmptyStateWidget(
                              searchQuery.isEmpty
                                ? 'No community stories submitted yet.\nBe the first to share a legacy contribution.'
                                : 'No articles match your search.',
                              icon: Icons.article_outlined,
                            )
                          : ListView.builder(
                              padding: const EdgeInsets.fromLTRB(20, 4, 20, 100),
                              itemCount: filtered.length,
                              itemBuilder: (context, index) {
                                final art = filtered[index];
                                final statusColor = _getStatusColor(art['status']);
                                final fullImgUrl = art['imageUrl'] != null
                                    ? (art['imageUrl'].toString().startsWith('http')
                                        ? art['imageUrl']
                                        : '${AppConfig.apiBaseUrl}/${art['imageUrl']}'.replaceAll('//', '/'))
                                    : null;

                                return Padding(
                                  padding: const EdgeInsets.only(bottom: 16.0),
                                  child: GlassContainer(
                                    padding: EdgeInsets.zero,
                                    child: InkWell(
                                      onTap: () {
                                        HapticFeedback.lightImpact();
                                        final id = art['id'];
                                        if (id != null) context.pushNamed('news_details', pathParameters: {'id': id.toString()});
                                      },
                                      child: Column(
                                        crossAxisAlignment: CrossAxisAlignment.stretch,
                                        children: [
                                          if (fullImgUrl != null)
                                            ClipRRect(
                                              borderRadius: const BorderRadius.vertical(top: Radius.circular(16)),
                                              child: Image.network(fullImgUrl, height: 140, fit: BoxFit.cover, errorBuilder: (c, e, s) => Container(height: 140, color: Colors.black26)),
                                            ),
                                          Padding(
                                            padding: const EdgeInsets.all(16.0),
                                            child: Row(
                                              children: [
                                                Expanded(
                                                  child: Column(
                                                    crossAxisAlignment: CrossAxisAlignment.start,
                                                    children: [
                                                      Row(
                                                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                                        children: [
                                                          Flexible(
                                                            child: Container(
                                                              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                                                              decoration: BoxDecoration(color: statusColor.withValues(alpha: 0.1), borderRadius: BorderRadius.circular(4)),
                                                              child: Text(art['status']?.toString().toUpperCase() ?? 'PENDING', style: TextStyle(color: statusColor, fontSize: 8, fontWeight: FontWeight.w900, letterSpacing: 1.5), overflow: TextOverflow.ellipsis),
                                                            ),
                                                          ),
                                                          Text(art['createdAt']?.toString().split('T')[0] ?? art['submittedAt']?.toString().split('T')[0] ?? 'RECENT', style: const TextStyle(fontSize: 10, color: AppTheme.textSecondaryDark, fontWeight: FontWeight.bold)),
                                                        ],
                                                      ),
                                                      const SizedBox(height: 12),
                                                      Text(art['title'] ?? 'ALUMNI ARTICLE STORY', style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 16, color: Colors.white, height: 1.2, letterSpacing: -0.2)),
                                                      const SizedBox(height: 10),
                                                      Row(
                                                        children: [
                                                          const Icon(Icons.bookmark_border_rounded, size: 10, color: AppTheme.royalGold),
                                                          const SizedBox(width: 4),
                                                          Text(_getCategoryLabel(art['articleCategory']), style: const TextStyle(fontSize: 10, color: AppTheme.textSecondaryDark, fontWeight: FontWeight.bold, letterSpacing: 1)),
                                                        ],
                                                      ),
                                                    ],
                                                  ),
                                                ),
                                                const SizedBox(width: 12),
                                                IconButton(
                                                  icon: const Icon(Icons.delete_outline, size: 20, color: Colors.white70),
                                                  onPressed: () {
                                                    HapticFeedback.lightImpact();
                                                    if (art['id'] != null) {
                                                      _deleteArticle(context, art['id'], art['status']?.toString() ?? '');
                                                    }
                                                  },
                                                ),
                                              ],
                                            ),
                                          ),
                                        ],
                                      ),
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

  Color _getStatusColor(dynamic status) {
    if (status == null) return Colors.amber;
    switch (status.toString().toLowerCase()) {
      case 'approved': return Colors.greenAccent;
      case 'rejected': return Colors.redAccent;
      case 'draft': return Colors.white38;
      default: return Colors.amber;
    }
  }

  String _getCategoryLabel(dynamic category) {
    if (category == null) return 'REGULAR';
    switch (category.toString()) {
      case 'Regular': return 'MEMBER STORY';
      case 'Technical': return 'INNOVATION HUB';
      case 'EventHighlight': return 'REUNION MEMORIES';
      default: return category.toString().toUpperCase();
    }
  }
}
