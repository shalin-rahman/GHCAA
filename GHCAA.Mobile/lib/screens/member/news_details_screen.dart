import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/api/api_client.dart';
import '../../core/widgets/logo_spinner.dart';
import '../../features/auth/auth_service.dart';
import '../../core/config/app_config.dart';

final newsDetailsProvider = FutureProvider.family<Map<String, dynamic>?, int>((ref, newsId) async {
  try {
    final dio = ref.read(dioProvider);
    final response = await dio.get('/news/$newsId');
    return response.data as Map<String, dynamic>;
  } catch (e) {
    return null;
  }
});

class NewsDetailsScreen extends ConsumerWidget {
  final int newsId;

  const NewsDetailsScreen({super.key, required this.newsId});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final detailsAsync = ref.watch(newsDetailsProvider(newsId));
    final roleAsync = ref.watch(roleProvider);
    final isAdmin = roleAsync.value?.isStaffAdminRole ?? false;

    return AppScaffold(
      title: 'Article Details',
      breadcrumb: 'Alumni Press > Story',
      actions: isAdmin ? [
        IconButton(
          icon: const Icon(Icons.check_circle_outline, color: Colors.greenAccent, size: 20),
          tooltip: 'Approve',
          onPressed: () async {
            try {
              final dio = ref.read(dioProvider);
              await dio.put('/news/$newsId/approve');
              ref.invalidate(newsDetailsProvider(newsId));
              if (context.mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Article approved and published.')));
            } catch (e) {
              if (context.mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Approval failed: $e'), backgroundColor: Colors.redAccent));
            }
          },
        ),
        IconButton(
          icon: const Icon(Icons.edit, color: AppTheme.royalGold, size: 20),
          tooltip: 'Edit',
          onPressed: () => detailsAsync.whenData((article) {
            if (article != null) _showEditDialog(context, ref, article);
          }),
        ),
        IconButton(
          icon: const Icon(Icons.delete_outline, color: Colors.redAccent, size: 20),
          tooltip: 'Delete',
          onPressed: () => _confirmDelete(context, ref),
        ),
      ] : null,
      child: detailsAsync.when(
        data: (article) {
          if (article == null) return const Center(child: Text('Story corrupted or missing.', style: TextStyle(color: Colors.red)));

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

          return SingleChildScrollView(
            padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 20),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                if (fullImgUrl != null)
                   Container(
                     height: 220,
                     padding: const EdgeInsets.only(bottom: 12.0),
                     decoration: BoxDecoration(
                       borderRadius: BorderRadius.circular(16),
                       image: DecorationImage(image: NetworkImage(fullImgUrl), fit: BoxFit.cover),
                       boxShadow: [
                          BoxShadow(color: AppTheme.royalGold.withValues(alpha: 0.1), blurRadius: 10, spreadRadius: 2),
                       ],
                     ),
                   ),
                GlassContainer(
                   child: Column(
                     crossAxisAlignment: CrossAxisAlignment.start,
                     children: [
                       Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                             Text('CATEGORY: ${_getCategoryLabel(article['articleCategory'])}', style: const TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.bold, letterSpacing: 1.5)),
                             Text(article['createdAt']?.toString().split('T')[0] ?? 'RECENT', style: TextStyle(fontSize: 10, color: AppTheme.textSecondaryDark.withValues(alpha: 0.6), fontWeight: FontWeight.bold)),
                          ],
                       ),
                       const SizedBox(height: 12),
                       Text((article['title'] ?? 'Global Headline').toUpperCase(), style: const TextStyle(fontWeight: FontWeight.w900, color: Colors.white, fontSize: 20, letterSpacing: 0.5, height: 1.2)),
                       const SizedBox(height: 8),
                       Row(
                         children: [
                           Container(width: 20, height: 20, decoration: const BoxDecoration(color: AppTheme.royalGold, shape: BoxShape.circle), child: Center(child: Text(article['authorName']?[0] ?? 'A', style: const TextStyle(color: Colors.black, fontSize: 10, fontWeight: FontWeight.bold)))),
                           const SizedBox(width: 8),
                           Text('Written by ${article['authorName'] ?? 'Official Admin'}', style: const TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: Colors.white70)),
                         ],
                       ),
                       const Divider(color: Colors.white12, height: 32),
                       if (article['summary'] != null && article['summary'].toString().isNotEmpty)
                          Padding(
                            padding: const EdgeInsets.only(bottom: 24),
                            child: Text(article['summary'], style: const TextStyle(color: AppTheme.royalGold, fontSize: 14, fontStyle: FontStyle.italic, height: 1.6)),
                          ),
                       Text(article['content'] ?? 'No article content found.', style: const TextStyle(color: Colors.white, fontSize: 14, height: 1.8)),
                       
                     ],
                   )
                ),
                const SizedBox(height: 48),
              ],
            ),
          );
        },
        loading: () => const Center(child: LogoSpinner(size: 120)),
        error: (e, s) => Center(child: Text('Sync Error: $e', style: const TextStyle(color: Colors.red))),
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

  void _showEditDialog(BuildContext context, WidgetRef ref, Map<String, dynamic> article) {
    final titleCtrl = TextEditingController(text: article['title']);
    final contentCtrl = TextEditingController(text: article['content']);
    bool saving = false;

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: AppTheme.midnightSurface,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(24))),
      builder: (ctx) => StatefulBuilder(
        builder: (ctx, setStateModal) => Padding(
          padding: EdgeInsets.only(left: 24, right: 24, top: 24, bottom: MediaQuery.of(ctx).viewInsets.bottom + 32),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const Text('EDIT ARTICLE', style: TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.w900, letterSpacing: 2, fontSize: 12)),
              const SizedBox(height: 20),
              TextField(
                controller: titleCtrl,
                style: const TextStyle(color: Colors.white),
                decoration: const InputDecoration(labelText: 'Headline', labelStyle: TextStyle(color: AppTheme.royalGold), enabledBorder: UnderlineInputBorder(borderSide: BorderSide(color: Colors.white24))),
              ),
              const SizedBox(height: 16),
              TextField(
                controller: contentCtrl,
                style: const TextStyle(color: Colors.white),
                maxLines: 4,
                decoration: const InputDecoration(labelText: 'Content', labelStyle: TextStyle(color: AppTheme.royalGold), enabledBorder: UnderlineInputBorder(borderSide: BorderSide(color: Colors.white24))),
              ),
              const SizedBox(height: 28),
              ElevatedButton(
                style: ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold, shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12))),
                onPressed: saving ? null : () async {
                  setStateModal(() => saving = true);
                  try {
                    final dio = ref.read(dioProvider);
                    await dio.put('/news/$newsId', data: {'title': titleCtrl.text, 'content': contentCtrl.text});
                    ref.invalidate(newsDetailsProvider(newsId));
                    if (ctx.mounted) Navigator.pop(ctx);
                    if (context.mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Article updated.')));
                  } catch (e) {
                    if (context.mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Update failed: $e'), backgroundColor: Colors.redAccent));
                  } finally {
                    if (ctx.mounted) setStateModal(() => saving = false);
                  }
                },
                child: saving
                    ? SizedBox(height: 20, width: 20, child: LogoSpinner.small())
                    : const Text('SAVE CHANGES', style: TextStyle(color: Colors.black, fontWeight: FontWeight.w900, fontSize: 12, letterSpacing: 1)),
              ),
            ],
          ),
        ),
      ),
    );
  }

  void _confirmDelete(BuildContext context, WidgetRef ref) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: AppTheme.midnightSurface,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
        title: const Text('Delete Article', style: TextStyle(color: Colors.white, fontWeight: FontWeight.w900)),
        content: const Text('This article will be permanently removed from the Alumni Press. Proceed?', style: TextStyle(color: AppTheme.textSecondaryDark, height: 1.5)),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('CANCEL', style: TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.bold))),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: Colors.redAccent, shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10))),
            onPressed: () async {
              Navigator.pop(ctx);
              try {
                final dio = ref.read(dioProvider);
                await dio.delete('/news/$newsId');
                if (context.mounted) {
                  ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Article removed.')));
                  context.pop();
                }
              } catch (e) {
                if (context.mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Delete failed: $e'), backgroundColor: Colors.redAccent));
              }
            },
            child: const Text('DELETE', style: TextStyle(color: Colors.white, fontWeight: FontWeight.w900, letterSpacing: 1)),
          ),
        ],
      ),
    );
  }
}
