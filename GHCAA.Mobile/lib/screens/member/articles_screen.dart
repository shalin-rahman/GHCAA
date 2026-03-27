import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/async_value_widget.dart';
import 'package:flutter/services.dart';
import '../../core/config/app_config.dart';
import '../../features/content/content_service.dart';

final myArticlesProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(newsServiceProvider).getMySubmissions();
});

class MemberArticlesScreen extends ConsumerWidget {
  const MemberArticlesScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final articlesAsync = ref.watch(myArticlesProvider);

    return AppScaffold(
      title: 'Haragangian Press',
      breadcrumb: 'Member Portal > Content Hub',
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () {
          HapticFeedback.lightImpact();
          context.pushNamed('submit_article').then((_) => ref.invalidate(myArticlesProvider));
        },
        backgroundColor: AppTheme.royalGold,
        icon: const Icon(Icons.history_edu_rounded, color: Colors.black, size: 20),
        label: const Text('SUBMIT STORY', style: TextStyle(color: Colors.black, fontWeight: FontWeight.w900, fontSize: 10, letterSpacing: 1.5)),
      ),
      child: AsyncValueWidget<List<dynamic>>(
        value: articlesAsync,
        loadingMessage: 'Synchronizing submission history...',
        onRetry: () => ref.invalidate(myArticlesProvider),
        data: (articles) => RefreshIndicator(
          color: AppTheme.royalGold,
          onRefresh: () async {
            HapticFeedback.mediumImpact();
            ref.invalidate(myArticlesProvider);
          },
          child: Column(
            children: [
              if (articles.isNotEmpty)
                Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                  child: Row(
                    children: [
                      const Icon(Icons.auto_awesome_outlined, size: 14, color: AppTheme.royalGold),
                      const SizedBox(width: 8),
                      Text('You have published ${articles.length} contributions', style: const TextStyle(color: AppTheme.textSecondaryDark, fontSize: 11, fontWeight: FontWeight.bold)),
                    ],
                  ),
                ),
              Expanded(
                child: articles.isEmpty 
                  ? const Center(child: Padding(
                      padding: EdgeInsets.all(60.0),
                      child: Text('No community stories submitted yet.\nBe the first to share a legacy contribution.', textAlign: TextAlign.center, style: TextStyle(color: AppTheme.textSecondaryDark, fontSize: 13, height: 1.5)),
                    ))
                  : ListView.builder(
                      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                      itemCount: articles.length,
                      itemBuilder: (context, index) {
                        final art = articles[index];
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
                                                  Container(
                                                    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                                                    decoration: BoxDecoration(color: statusColor.withValues(alpha: 0.1), borderRadius: BorderRadius.circular(4)),
                                                    child: Text(art['status']?.toString().toUpperCase() ?? 'PENDING', style: TextStyle(color: statusColor, fontSize: 8, fontWeight: FontWeight.w900, letterSpacing: 1.5)),
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
                                        const Icon(Icons.arrow_forward_ios_rounded, size: 14, color: AppTheme.royalGold),
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
        ),
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
    // Mapping identical to web constants
    switch (category.toString()) {
      case 'Regular': return 'MEMBER STORY';
      case 'Technical': return 'INNOVATION HUB';
      case 'EventHighlight': return 'REUNION MEMORIES';
      default: return category.toString().toUpperCase();
    }
  }
}
