import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_tile.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../features/content/content_service.dart';
import 'package:flutter/services.dart';

final pendingArticlesProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async => ref.read(newsServiceProvider).getPendingSubmissions());
final articleApprovalSearchQueryProvider = StateProvider.autoDispose<String>((ref) => "");

class ArticleApprovalScreen extends ConsumerStatefulWidget {
  const ArticleApprovalScreen({super.key});

  @override
  ConsumerState<ArticleApprovalScreen> createState() => _ArticleApprovalScreenState();
}

class _ArticleApprovalScreenState extends ConsumerState<ArticleApprovalScreen> {
  final TextEditingController _searchController = TextEditingController();

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final articlesAsync = ref.watch(pendingArticlesProvider);
    final searchQuery = ref.watch(articleApprovalSearchQueryProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      isAdmin: true,
      title: 'Submission Review',
      breadcrumb: 'ADMIN > SUBMISSIONS',
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Search pending submissions...',
                prefixIcon: const Icon(Icons.search, size: 20, color: AppTheme.royalGold),
                suffixIcon: _searchController.text.isNotEmpty 
                  ? IconButton(
                      icon: const Icon(Icons.close_rounded, size: 20, color: Colors.white54),
                      onPressed: () {
                        _searchController.clear();
                        ref.read(articleApprovalSearchQueryProvider.notifier).state = "";
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
              onChanged: (v) => ref.read(articleApprovalSearchQueryProvider.notifier).state = v.toLowerCase(),
            ),
          ),
          Expanded(
            child: AsyncValueWidget<List<dynamic>>(
              value: articlesAsync,
              loadingMessage: 'Loading submissions...',
              onRetry: () => ref.invalidate(pendingArticlesProvider),
              data: (articles) {
                final filtered = articles.where((art) {
                  final title = (art['title'] ?? '').toString().toLowerCase();
                  final memberId = art['memberId'].toString();
                  return title.contains(searchQuery) || memberId.contains(searchQuery);
                }).toList();

                return RefreshIndicator(
                  color: AppTheme.royalGold,
                  onRefresh: () async {
                    HapticFeedback.mediumImpact();
                    ref.invalidate(pendingArticlesProvider);
                  },
                  child: Column(
                    children: [
                      if (filtered.isNotEmpty)
                        Padding(
                          padding: const EdgeInsets.only(left: 24, bottom: 8),
                          child: Align(
                            alignment: Alignment.centerLeft,
                            child: Text(
                              'Showing ${filtered.length} of ${articles.length} pending reviews',
                              style: TextStyle(fontSize: 10, color: AppTheme.royalGold.withValues(alpha: 0.7), fontWeight: FontWeight.bold),
                            ),
                          ),
                        ),
                      Expanded(
                        child: filtered.isEmpty 
                          ? Center(child: Text(searchQuery.isEmpty ? 'No pending submissions found.' : 'No items match your search.', style: const TextStyle(color: Colors.white54, fontSize: 11, fontStyle: FontStyle.italic)))
                          : ListView.builder(
                              padding: const EdgeInsets.all(20.0),
                              itemCount: filtered.length,
                              itemBuilder: (context, index) {
                                final art = filtered[index];
                                return Padding(
                                  padding: const EdgeInsets.only(bottom: 12.0),
                                  child: GlassTile(
                                    icon: Icons.history_edu_rounded,
                                    title: art['title'] ?? 'Archive Contribution',
                                    subtitle: 'By Member #${art['memberId']} | ${art['articleCategory'] ?? 'General'}',
                                    trailing: Row(
                                      mainAxisSize: MainAxisSize.min,
                                      children: [
                                        IconButton(
                                          icon: const Icon(Icons.check_circle_outline, color: Colors.greenAccent, size: 24),
                                          onPressed: () => _handleResolve(context, ref, art['id'], true),
                                        ),
                                        IconButton(
                                          icon: const Icon(Icons.cancel_outlined, color: Colors.redAccent, size: 24),
                                          onPressed: () => _handleResolve(context, ref, art['id'], false),
                                        ),
                                      ],
                                    ),
                                    onTap: () {
                                      // Optional: Preview content before approval
                                    },
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

  Future<void> _handleResolve(BuildContext context, WidgetRef ref, int id, bool approve) async {
    HapticFeedback.heavyImpact();
    final success = await ref.read(newsServiceProvider).resolveArticle(id, approve);
    if (success && context.mounted) {
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(approve ? 'Article Published to Chronicle' : 'Article Decline Finalized')));
      ref.invalidate(pendingArticlesProvider);
    }
  }
}
