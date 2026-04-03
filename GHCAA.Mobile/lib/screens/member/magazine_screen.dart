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

final magazineListProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async => ref.read(newsServiceProvider).getNewsByCategory('Magazine'));
final magazineSearchQueryProvider = StateProvider.autoDispose<String>((ref) => "");

class MagazineScreen extends ConsumerStatefulWidget {
  const MagazineScreen({super.key});

  @override
  ConsumerState<MagazineScreen> createState() => _MagazineScreenState();
}

class _MagazineScreenState extends ConsumerState<MagazineScreen> {
  final TextEditingController _searchController = TextEditingController();

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final magazineAsync = ref.watch(magazineListProvider);
    final searchQuery = ref.watch(magazineSearchQueryProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      title: 'Haragangian Journal',
      breadcrumb: 'Member Portal > Annual Publications',
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Search journals...',
                prefixIcon: const Icon(Icons.search, size: 20, color: AppTheme.royalGold),
                suffixIcon: _searchController.text.isNotEmpty 
                  ? IconButton(
                      icon: const Icon(Icons.close_rounded, size: 20, color: Colors.white54),
                      onPressed: () {
                        _searchController.clear();
                        ref.read(magazineSearchQueryProvider.notifier).state = "";
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
              onChanged: (v) => ref.read(magazineSearchQueryProvider.notifier).state = v.toLowerCase(),
            ),
          ),
          Expanded(
            child: AsyncValueWidget<List<dynamic>>(
              value: magazineAsync,
              loadingMessage: 'Curating publications...',
              onRetry: () => ref.invalidate(magazineListProvider),
              data: (magazines) {
                final filtered = magazines.where((mag) {
                  final title = mag['title']?.toString().toLowerCase() ?? '';
                  return title.contains(searchQuery);
                }).toList();

                return Column(
                  children: [
                    if (filtered.isNotEmpty)
                      Padding(
                        padding: const EdgeInsets.only(left: 24, bottom: 8),
                        child: Align(
                          alignment: Alignment.centerLeft,
                          child: Text(
                            'Showing ${filtered.length} of ${magazines.length} journals',
                            style: TextStyle(fontSize: 10, color: AppTheme.royalGold.withValues(alpha: 0.7), fontWeight: FontWeight.bold),
                          ),
                        ),
                      ),
                    Expanded(
                      child: RefreshIndicator(
                        color: AppTheme.royalGold,
                        onRefresh: () async {
                          HapticFeedback.mediumImpact();
                          ref.invalidate(magazineListProvider);
                        },
                        child: filtered.isEmpty 
                          ? Center(child: Padding(
                              padding: const EdgeInsets.all(40.0),
                              child: Text(
                                searchQuery.isEmpty ? 'No publications discovered.' : 'No journals match your search.', 
                                style: const TextStyle(color: AppTheme.textSecondaryDark)),
                            ))
                          : GridView.builder(
                              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                              gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                                crossAxisCount: 2,
                                childAspectRatio: 0.65,
                                crossAxisSpacing: 16,
                                mainAxisSpacing: 20,
                              ),
                              itemCount: filtered.length,
                              itemBuilder: (context, index) {
                                final mag = filtered[index];
                                final thumbnailUrl = mag['imageUrl'];
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

                                return InkWell(
                                  onTap: () {
                                    HapticFeedback.lightImpact();
                                    context.push('/news/${mag['id']}');
                                  },
                                  child: GlassContainer(
                                    padding: EdgeInsets.zero,
                                    child: Column(
                                      crossAxisAlignment: CrossAxisAlignment.stretch,
                                      children: [
                                        Expanded(
                                          child: Container(
                                            decoration: BoxDecoration(
                                              color: AppTheme.royalGold.withValues(alpha: 0.05),
                                              borderRadius: const BorderRadius.vertical(top: Radius.circular(16)),
                                              image: fullImgUrl != null ? DecorationImage(
                                                image: NetworkImage(fullImgUrl),
                                                fit: BoxFit.cover,
                                                opacity: 0.8,
                                              ) : null,
                                            ),
                                            child: Container(
                                              padding: const EdgeInsets.all(12),
                                              alignment: Alignment.topRight,
                                              child: Container(
                                                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                                                decoration: BoxDecoration(color: AppTheme.royalGold, borderRadius: BorderRadius.circular(4)),
                                                child: const Text('READ', style: TextStyle(color: Colors.black, fontSize: 8, fontWeight: FontWeight.w900, letterSpacing: 1)),
                                              ),
                                            ),
                                          ),
                                        ),
                                        Padding(
                                          padding: const EdgeInsets.all(12.0),
                                          child: Column(
                                            crossAxisAlignment: CrossAxisAlignment.start,
                                            children: [
                                              Text(
                                                mag['title'] ?? 'ALUMNI JOURNAL', 
                                                style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 13, color: Colors.white, letterSpacing: 1, height: 1.1),
                                                maxLines: 2,
                                                overflow: TextOverflow.ellipsis,
                                              ),
                                              const SizedBox(height: 4),
                                              const Text(
                                                'OFFICIAL PUBLICATION', 
                                                style: TextStyle(fontSize: 8, color: AppTheme.royalGold, fontWeight: FontWeight.w900, letterSpacing: 1)
                                              ),
                                            ],
                                          ),
                                        ),
                                      ],
                                    ),
                                  ),
                                );
                              },
                            ),
                      ),
                    ),
                  ],
                );
              },
            ),
          ),
        ],
      ),
    );
  }
}
