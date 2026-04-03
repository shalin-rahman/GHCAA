import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/config/app_config.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../features/content/content_service.dart';

final newsSearchQueryProvider = StateProvider.autoDispose<String>((ref) => "");

final newsListProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async => ref.read(newsServiceProvider).getLatestNews());

class NewsScreen extends ConsumerStatefulWidget {
  const NewsScreen({super.key});

  @override
  ConsumerState<NewsScreen> createState() => _NewsScreenState();
}

class _NewsScreenState extends ConsumerState<NewsScreen> {
  final TextEditingController _searchController = TextEditingController();

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final newsAsync = ref.watch(newsListProvider);
    final searchQuery = ref.watch(newsSearchQueryProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      title: 'Haraganga News Hub',
      breadcrumb: 'PORTAL > NEWS',
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Search news hub...',
                prefixIcon: const Icon(Icons.search, size: 20, color: AppTheme.royalGold),
                suffixIcon: _searchController.text.isNotEmpty 
                  ? IconButton(
                      icon: const Icon(Icons.close_rounded, size: 20, color: Colors.white54),
                      onPressed: () {
                        _searchController.clear();
                        ref.read(newsSearchQueryProvider.notifier).state = "";
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
              onChanged: (v) => ref.read(newsSearchQueryProvider.notifier).state = v.toLowerCase(),
            ),
          ),
          Expanded(
            child: AsyncValueWidget<List<dynamic>>(
              value: newsAsync,
              loadingMessage: 'Loading news...',
              onRetry: () => ref.invalidate(newsListProvider),
              data: (news) {
                final filtered = news.where((article) {
                  final title = article['title']?.toString().toLowerCase() ?? '';
                  return title.contains(searchQuery);
                }).toList();

                return RefreshIndicator(
                  color: AppTheme.royalGold,
                  onRefresh: () async {
                    HapticFeedback.mediumImpact();
                    ref.invalidate(newsListProvider);
                  },
                  child: Column(
                    children: [
                      if (filtered.isNotEmpty)
                        Padding(
                          padding: const EdgeInsets.only(left: 24, bottom: 8),
                          child: Align(
                            alignment: Alignment.centerLeft,
                            child: Text(
                              'Showing ${filtered.length} of ${news.length} news stories',
                              style: TextStyle(fontSize: 10, color: AppTheme.royalGold.withValues(alpha: 0.7), fontWeight: FontWeight.bold),
                            ),
                          ),
                        ),
                      Expanded(
                        child: filtered.isEmpty
                          ? Center(child: Text(searchQuery.isEmpty ? 'No news articles found.' : 'No stories match your search.', style: const TextStyle(color: AppTheme.textSecondaryDark)))
                          : ListView.builder(
                              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                              itemCount: filtered.length,
                              itemBuilder: (context, index) {
                                final article = filtered[index];
                                final thumbnailUrl = article['imageUrl'] ?? article['thumbnailUrl'];
                                String? fullImgUrl;
                                if (thumbnailUrl != null && thumbnailUrl.toString().isNotEmpty) {
                                  if (thumbnailUrl.toString().startsWith('http')) {
                                    fullImgUrl = thumbnailUrl.toString();
                                  } else {
                                    final base = AppConfig.apiBaseUrl.endsWith('/') ? AppConfig.apiBaseUrl.substring(0, AppConfig.apiBaseUrl.length - 1) : AppConfig.apiBaseUrl;
                                    final cleanP = thumbnailUrl.toString().startsWith('/') ? thumbnailUrl.toString().substring(1) : thumbnailUrl.toString();
                                    fullImgUrl = '$base/$cleanP';
                                  }
                                }

                                return Padding(
                                  padding: const EdgeInsets.only(bottom: 24.0),
                                  child: GlassContainer(
                                    padding: EdgeInsets.zero,
                                    child: InkWell(
                                      onTap: () {
                                        HapticFeedback.lightImpact();
                                        context.push('/news/${article['id']}');
                                      },
                                      child: Column(
                                        crossAxisAlignment: CrossAxisAlignment.stretch,
                                        children: [
                                          if (fullImgUrl != null)
                                            ClipRRect(
                                              borderRadius: const BorderRadius.vertical(top: Radius.circular(16)),
                                              child: Image.network(
                                                fullImgUrl, 
                                                height: 190, 
                                                fit: BoxFit.cover,
                                                errorBuilder: (context, error, stackTrace) => Container(
                                                  height: 190,
                                                  color: Colors.black26,
                                                  child: const Icon(Icons.broken_image_outlined, color: Colors.white24),
                                                ),
                                              ),
                                            )
                                          else
                                            Container(
                                              height: 190,
                                              decoration: BoxDecoration(
                                                color: AppTheme.royalGold.withValues(alpha: 0.05),
                                                borderRadius: const BorderRadius.vertical(top: Radius.circular(16)),
                                              ),
                                              child: Center(child: Icon(Icons.newspaper_rounded, size: 64, color: AppTheme.royalGold.withValues(alpha: 0.1))),
                                            ),
                                          Padding(
                                            padding: const EdgeInsets.all(16.0),
                                            child: Column(
                                              crossAxisAlignment: CrossAxisAlignment.start,
                                              children: [
                                                Row(
                                                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                                  children: [
                                                    Container(
                                                      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                                                      decoration: BoxDecoration(color: AppTheme.royalGold.withValues(alpha: 0.1), borderRadius: BorderRadius.circular(4)),
                                                      child: Text(_getCategoryLabel(article['articleCategory']), style: const TextStyle(color: AppTheme.royalGold, fontSize: 8, fontWeight: FontWeight.w900, letterSpacing: 1)),
                                                    ),
                                                    Text(article['createdAt']?.toString().split('T')[0] ?? 'RECENT', style: const TextStyle(color: Colors.white38, fontSize: 10, fontWeight: FontWeight.bold)),
                                                  ],
                                                ),
                                                const SizedBox(height: 12),
                                                Text(article['title'] ?? 'Alumni News Highlight', style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w900, color: Colors.white, height: 1.25)),
                                                const SizedBox(height: 8),
                                                Text(article['content']?.toString().substring(0, article['content'].toString().length > 150 ? 150 : article['content'].toString().length) ?? 'Read more about this story in the alumni portal.', style: const TextStyle(fontSize: 12, height: 1.5, color: AppTheme.textSecondaryDark), maxLines: 2, overflow: TextOverflow.ellipsis),
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

  String _getCategoryLabel(dynamic category) {
    if (category == null) return 'OFFICIAL';
    switch (category.toString()) {
      case '0':
      case 'General': return 'GENERAL';
      case '1':
      case 'Announcement': return 'OFFICIAL';
      case '2':
      case 'Story': return 'FEATURED';
      default: return category.toString().toUpperCase();
    }
  }
}
