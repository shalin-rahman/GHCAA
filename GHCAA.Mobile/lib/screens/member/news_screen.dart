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

final newsListProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async => ref.read(newsServiceProvider).getLatestNews());

class NewsScreen extends ConsumerWidget {
  const NewsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final newsAsync = ref.watch(newsListProvider);

    return AppScaffold(
      title: 'Alumni Press',
      breadcrumb: 'Member Portal > Haraganga News',
      child: AsyncValueWidget<List<dynamic>>(
        value: newsAsync,
        loadingMessage: 'Streaming latest headlines...',
        onRetry: () => ref.invalidate(newsListProvider),
        data: (news) => RefreshIndicator(
          color: AppTheme.royalGold,
          onRefresh: () async {
            HapticFeedback.mediumImpact();
            ref.invalidate(newsListProvider);
          },
          child: ListView.builder(
            padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
            itemCount: news.length,
            itemBuilder: (context, index) {
              final article = news[index];
              final thumbnailUrl = article['imageUrl'] ?? article['thumbnailUrl'];
              final fullImgUrl = thumbnailUrl != null 
                  ? (thumbnailUrl.toString().startsWith('http') 
                      ? thumbnailUrl 
                      : '${AppConfig.apiBaseUrl}/$thumbnailUrl'.replaceAll('//', '/'))
                  : null;

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
                                child: const Icon(Icons.broken_image_outlined, color: AppTheme.textSecondaryDark, size: 32),
                              ),
                            ),
                          ),
                        Padding(
                          padding: const EdgeInsets.all(20.0),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Row(
                                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                children: [
                                  Container(
                                    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                                    decoration: BoxDecoration(color: AppTheme.royalGold.withValues(alpha: 0.1), borderRadius: BorderRadius.circular(4)),
                                    child: Text(_getCategoryLabel(article['articleCategory']), style: const TextStyle(color: AppTheme.royalGold, fontSize: 8, fontWeight: FontWeight.w900, letterSpacing: 1.5)),
                                  ),
                                  Text(article['createdAt']?.toString().split('T')[0] ?? article['publishedAt']?.toString().split('T')[0] ?? 'RECENT', style: TextStyle(fontSize: 10, color: AppTheme.textSecondaryDark.withValues(alpha: 0.6), fontWeight: FontWeight.bold)),
                                ],
                              ),
                              const SizedBox(height: 14),
                              Text(article['title'] ?? 'Headline Missing', style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w900, color: Colors.white, height: 1.3, letterSpacing: -0.2)),
                              const SizedBox(height: 10),
                              Text(article['content']?.toString().substring(0, article['content'].toString().length > 150 ? 150 : article['content'].toString().length) ?? article['summary'] ?? 'Article content summary not and available.', style: const TextStyle(fontSize: 13, height: 1.6, color: AppTheme.textSecondaryDark)),
                              const SizedBox(height: 24),
                              Row(
                                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                children: [
                                  Row(
                                    children: [
                                      Container(width: 20, height: 20, decoration: const BoxDecoration(color: AppTheme.royalGold, shape: BoxShape.circle), child: Center(child: Text(article['authorName']?[0] ?? 'A', style: const TextStyle(color: Colors.black, fontSize: 10, fontWeight: FontWeight.bold)))),
                                      const SizedBox(width: 8),
                                      Text('By ${article['authorName'] ?? 'Official Admin'}', style: const TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: Colors.white70)),
                                    ],
                                  ),
                                  const Text('READ ARTICLE ↗', style: TextStyle(fontSize: 9, fontWeight: FontWeight.w900, color: AppTheme.royalGold, letterSpacing: 1.5)),
                                ],
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
      ),
    );
  }

  String _getCategoryLabel(dynamic category) {
    if (category == null) return 'OFFICIAL';
    // Mapping identical to web constants
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
