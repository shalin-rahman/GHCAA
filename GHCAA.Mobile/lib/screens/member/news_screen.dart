import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:share_plus/share_plus.dart';
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
      title: 'Haraganga News',
      child: AsyncValueWidget(
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
            padding: const EdgeInsets.all(24),
            itemCount: news.length,
            itemBuilder: (context, index) {
              final article = news[index];
              return Padding(
                padding: const EdgeInsets.only(bottom: 24.0),
                child: GestureDetector(
                  onLongPress: () {
                    HapticFeedback.heavyImpact();
                    Share.share('Read this on GHCAA: ${article['title']}\n${article['summary'] ?? ''}');
                  },
                  child: GlassContainer(
                    padding: EdgeInsets.zero,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        if (article['thumbnailUrl'] != null)
                          ClipRRect(
                            borderRadius: const BorderRadius.vertical(top: Radius.circular(16)),
                            child: Image.network(article['thumbnailUrl'] ?? '', height: 180, fit: BoxFit.cover),
                          ),
                        Padding(
                          padding: const EdgeInsets.all(20.0),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(article['title'] ?? 'Headline Missing', style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w900, color: Colors.white)),
                              const SizedBox(height: 12),
                              Text(article['summary'] ?? 'Summary not available.', style: TextStyle(fontSize: 13, height: 1.5, color: AppTheme.textSecondaryDark)),
                              const SizedBox(height: 16),
                              Row(
                                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                children: [
                                  Text(article['publishedAt'] ?? 'Today', style: TextStyle(fontSize: 10, color: AppTheme.royalGold.withValues(alpha: 0.6))),
                                  const Icon(Icons.arrow_forward, size: 14, color: AppTheme.royalGold),
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
}
