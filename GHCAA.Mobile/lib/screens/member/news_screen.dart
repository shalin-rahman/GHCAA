import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/config/app_config.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/utils/app_utils.dart';
import '../../core/services/org_config_service.dart';
import '../../core/widgets/app_search_field.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../features/content/content_service.dart';

final newsSearchQueryProvider = StateProvider.autoDispose<String>((ref) => "");

/// '' = all, 'News', 'Notice' — matches the PostType discriminator on NewsPost.
final newsPostTypeFilterProvider =
    StateProvider.autoDispose<String>((ref) => "");

final newsListProvider = FutureProvider.autoDispose<List<dynamic>>(
    (ref) async => ref.read(newsServiceProvider).getLatestNews());

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
    final postTypeFilter = ref.watch(newsPostTypeFilterProvider);
    final dateFormat = ref.watch(orgDateFormatProvider);

    return AppScaffold(
      title: 'News & Notices',
      breadcrumb: 'PORTAL > NEWS & NOTICES',
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(AppTheme.spaceL, AppTheme.spaceM,
                AppTheme.spaceL, AppTheme.spaceS),
            child: AppSearchField(
              controller: _searchController,
              hintText: 'Search news & notices...',
              onChanged: (v) => ref
                  .read(newsSearchQueryProvider.notifier)
                  .state = v.toLowerCase(),
              onClear: () {
                _searchController.clear();
                ref.read(newsSearchQueryProvider.notifier).state = "";
              },
            ),
          ),
          Padding(
            padding: const EdgeInsets.fromLTRB(
                AppTheme.spaceL, 0, AppTheme.spaceL, AppTheme.spaceS),
            child: Row(
              children: [
                for (final option in const [
                  ('', 'ALL'),
                  ('News', 'NEWS'),
                  ('Notice', 'NOTICES')
                ])
                  Padding(
                    padding: const EdgeInsets.only(right: AppTheme.spaceS),
                    child: ChoiceChip(
                      label: Text(option.$2,
                          style: const TextStyle(
                              fontSize: 10,
                              fontWeight: FontWeight.w900,
                              letterSpacing: 1)),
                      selected: postTypeFilter == option.$1,
                      onSelected: (_) => ref
                          .read(newsPostTypeFilterProvider.notifier)
                          .state = option.$1,
                    ),
                  ),
              ],
            ),
          ),
          Expanded(
            child: AsyncValueWidget<List<dynamic>>(
              value: newsAsync,
              loadingMessage: 'Loading news...',
              onRetry: () => ref.invalidate(newsListProvider),
              data: (news) {
                final filtered = news.where((article) {
                  final title =
                      article['title']?.toString().toLowerCase() ?? '';
                  if (!title.contains(searchQuery)) return false;
                  if (postTypeFilter.isEmpty) return true;
                  return _postTypeOf(article) == postTypeFilter;
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
                          padding: const EdgeInsets.only(
                              left: AppTheme.spaceXL, bottom: AppTheme.spaceS),
                          child: Align(
                            alignment: Alignment.centerLeft,
                            child: Text(
                              'Showing ${filtered.length} of ${news.length} posts',
                              style: TextStyle(
                                  fontSize: 10,
                                  color:
                                      AppTheme.royalGold.withValues(alpha: 0.7),
                                  fontWeight: FontWeight.bold),
                            ),
                          ),
                        ),
                      Expanded(
                        child: filtered.isEmpty
                            ? EmptyStateWidget(
                                searchQuery.isEmpty
                                    ? 'No news articles found.'
                                    : 'No stories match your search.',
                                icon: Icons.article_outlined)
                            : ListView.builder(
                                padding: const EdgeInsets.symmetric(
                                    horizontal: AppTheme.spaceL,
                                    vertical: AppTheme.spaceM),
                                itemCount: filtered.length,
                                itemBuilder: (context, index) {
                                  final article = filtered[index];
                                  final fullImgUrl = AppConfig.resolveImageUrl(
                                      article['imageUrl'] ??
                                          article['thumbnailUrl']);

                                  return Padding(
                                    padding: const EdgeInsets.only(
                                        bottom: AppTheme.spaceXL),
                                    child: GlassContainer(
                                      padding: EdgeInsets.zero,
                                      child: InkWell(
                                        onTap: () {
                                          HapticFeedback.lightImpact();
                                          context
                                              .push('/news/${article['id']}');
                                        },
                                        child: Column(
                                          crossAxisAlignment:
                                              CrossAxisAlignment.stretch,
                                          children: [
                                            if (fullImgUrl != null)
                                              ClipRRect(
                                                borderRadius:
                                                    const BorderRadius.vertical(
                                                        top: Radius.circular(
                                                            AppTheme.radiusL)),
                                                child: Image.network(
                                                  fullImgUrl,
                                                  height: 192,
                                                  fit: BoxFit.cover,
                                                  errorBuilder: (context, error,
                                                          stackTrace) =>
                                                      Container(
                                                    height: 192,
                                                    color: Colors.black26,
                                                    child: const Icon(
                                                        Icons
                                                            .broken_image_outlined,
                                                        color: Colors.white24),
                                                  ),
                                                ),
                                              )
                                            else
                                              Container(
                                                height: 192,
                                                decoration: BoxDecoration(
                                                  color: AppTheme.royalGold
                                                      .withValues(alpha: 0.05),
                                                  borderRadius:
                                                      const BorderRadius
                                                          .vertical(
                                                          top: Radius.circular(
                                                              AppTheme
                                                                  .radiusL)),
                                                ),
                                                child: Center(
                                                    child: Icon(
                                                        Icons.newspaper_rounded,
                                                        size: 64,
                                                        color: AppTheme
                                                            .royalGold
                                                            .withValues(
                                                                alpha: 0.1))),
                                              ),
                                            Padding(
                                              padding: const EdgeInsets.all(
                                                  AppTheme.spaceM),
                                              child: Column(
                                                crossAxisAlignment:
                                                    CrossAxisAlignment.start,
                                                children: [
                                                  Row(
                                                    mainAxisAlignment:
                                                        MainAxisAlignment
                                                            .spaceBetween,
                                                    children: [
                                                      Flexible(
                                                        child: Container(
                                                          padding: const EdgeInsets
                                                              .symmetric(
                                                              horizontal:
                                                                  AppTheme
                                                                      .spaceS,
                                                              vertical: AppTheme
                                                                  .spaceXS),
                                                          decoration: BoxDecoration(
                                                              color: AppTheme
                                                                  .royalGold
                                                                  .withValues(
                                                                      alpha:
                                                                          0.1),
                                                              borderRadius:
                                                                  BorderRadius
                                                                      .circular(
                                                                          AppTheme
                                                                              .radiusXS)),
                                                          child: Text(
                                                              _postTypeOf(article) ==
                                                                      'Notice'
                                                                  ? 'NOTICE'
                                                                  : _getCategoryLabel(
                                                                      article[
                                                                          'articleCategory']),
                                                              style: const TextStyle(
                                                                  color: AppTheme
                                                                      .royalGold,
                                                                  fontSize: 8,
                                                                  fontWeight:
                                                                      FontWeight
                                                                          .w900,
                                                                  letterSpacing:
                                                                      1),
                                                              overflow:
                                                                  TextOverflow
                                                                      .ellipsis),
                                                        ),
                                                      ),
                                                      Text(
                                                          AppUtils.formatDate(
                                                              article[
                                                                  'createdAt'],
                                                              format: dateFormat
                                                                  .identifier),
                                                          style: const TextStyle(
                                                              color: Colors
                                                                  .white38,
                                                              fontSize: 10,
                                                              fontWeight:
                                                                  FontWeight
                                                                      .bold)),
                                                    ],
                                                  ),
                                                  const SizedBox(
                                                      height: AppTheme.spaceM),
                                                  Text(
                                                      article['title'] ??
                                                          'Alumni News Highlight',
                                                      style: const TextStyle(
                                                          fontSize: 16,
                                                          fontWeight:
                                                              FontWeight.w900,
                                                          color: Colors.white,
                                                          height: 1.25)),
                                                  const SizedBox(
                                                      height: AppTheme.spaceS),
                                                  Text(
                                                      article['content']?.toString().substring(
                                                              0,
                                                              article['content']
                                                                          .toString()
                                                                          .length >
                                                                      150
                                                                  ? 150
                                                                  : article[
                                                                          'content']
                                                                      .toString()
                                                                      .length) ??
                                                          'Read more about this story in the alumni portal.',
                                                      style: const TextStyle(
                                                          fontSize: 12,
                                                          height: 1.5,
                                                          color: AppTheme
                                                              .textSecondaryDark),
                                                      maxLines: 2,
                                                      overflow: TextOverflow
                                                          .ellipsis),
                                                  if ((article[
                                                              'attachmentUrl'] ??
                                                          '')
                                                      .toString()
                                                      .isNotEmpty) ...[
                                                    const SizedBox(
                                                        height:
                                                            AppTheme.spaceS),
                                                    Row(
                                                      children: [
                                                        const Icon(
                                                            Icons
                                                                .picture_as_pdf_outlined,
                                                            size: 14,
                                                            color: AppTheme
                                                                .royalGold),
                                                        const SizedBox(
                                                            width: AppTheme
                                                                .spaceXS),
                                                        Expanded(
                                                          child: Text(
                                                            article['attachmentFileName']
                                                                    ?.toString() ??
                                                                'Attached document',
                                                            style: const TextStyle(
                                                                fontSize: 10,
                                                                color: AppTheme
                                                                    .royalGold,
                                                                fontWeight:
                                                                    FontWeight
                                                                        .bold),
                                                            overflow:
                                                                TextOverflow
                                                                    .ellipsis,
                                                          ),
                                                        ),
                                                      ],
                                                    ),
                                                  ],
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

  String _postTypeOf(dynamic article) {
    final raw = article['postType'];
    if (raw == null) return 'News';
    return (raw.toString() == '1' || raw.toString() == 'Notice')
        ? 'Notice'
        : 'News';
  }

  String _getCategoryLabel(dynamic category) {
    if (category == null) return 'OFFICIAL';
    switch (category.toString()) {
      case '0':
      case 'General':
        return 'GENERAL';
      case '1':
      case 'Announcement':
        return 'OFFICIAL';
      case '2':
      case 'Story':
        return 'FEATURED';
      default:
        return category.toString().toUpperCase();
    }
  }
}
