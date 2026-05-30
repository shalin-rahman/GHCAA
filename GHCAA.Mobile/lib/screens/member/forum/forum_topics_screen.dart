import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/widgets/glass_container.dart';
import '../../../core/widgets/app_scaffold.dart';
import '../../../core/widgets/async_value_widget.dart';
import '../../../core/utils/app_utils.dart';
import '../../../features/forum/forum_service.dart';

final topicSearchQueryProvider = StateProvider.autoDispose<String>((ref) => "");

class ForumTopicsScreen extends ConsumerStatefulWidget {
  final int categoryId;
  const ForumTopicsScreen({super.key, required this.categoryId});

  @override
  ConsumerState<ForumTopicsScreen> createState() => _ForumTopicsScreenState();
}

class _ForumTopicsScreenState extends ConsumerState<ForumTopicsScreen> {
  final TextEditingController _searchController = TextEditingController();

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final topicsAsync = ref.watch(forumTopicsProvider(widget.categoryId));
    final searchQuery = ref.watch(topicSearchQueryProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      title: 'Topics',
      breadcrumb: 'PORTAL > DISCUSSIONS > TOPICS',
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () {
          HapticFeedback.mediumImpact();
          _showCreateTopicDialog(context, ref);
        },
        backgroundColor: AppTheme.royalGold,
        icon: const Icon(Icons.add_comment_outlined, color: Colors.black, size: 20),
        label: const Text(
          'NEW TOPIC',
          style: TextStyle(
            color: Colors.black,
            fontSize: 11,
            fontWeight: FontWeight.w900,
            letterSpacing: 1.0,
          ),
        ),
      ),
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
            child: Row(
              children: [
                Expanded(
                  child: TextField(
                    controller: _searchController,
                    decoration: InputDecoration(
                      hintText: 'Search topics...',
                      prefixIcon: const Icon(Icons.search, size: 20, color: AppTheme.royalGold),
                      suffixIcon: _searchController.text.isNotEmpty 
                        ? IconButton(
                            icon: const Icon(Icons.cancel_rounded, size: 18, color: Colors.white24),
                            onPressed: () {
                              _searchController.clear();
                              ref.read(topicSearchQueryProvider.notifier).state = "";
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
                    onChanged: (v) => ref.read(topicSearchQueryProvider.notifier).state = v.toLowerCase(),
                  ),
                ),
              ],
            ),
          ),
          Expanded(
            child: AsyncValueWidget<List<ForumTopic>>(
              value: topicsAsync,
              loadingMessage: 'Loading topics...',
              onRetry: () => ref.invalidate(forumTopicsProvider(widget.categoryId)),
              data: (topics) {
                final filtered = topics.where((t) {
                  return t.title.toLowerCase().contains(searchQuery) ||
                      t.content.toLowerCase().contains(searchQuery) ||
                      t.authorName.toLowerCase().contains(searchQuery);
                }).toList();

                return RefreshIndicator(
                  color: AppTheme.royalGold,
                  onRefresh: () async {
                    HapticFeedback.mediumImpact();
                    ref.invalidate(forumTopicsProvider(widget.categoryId));
                  },
                  child: filtered.isEmpty
                      ? Center(
                          child: Text(
                            searchQuery.isEmpty ? 'No topics created in this category yet.' : 'No topics match your search.',
                            style: const TextStyle(color: AppTheme.textSecondaryDark),
                          ),
                        )
                      : ListView.builder(
                          padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                          itemCount: filtered.length,
                          itemBuilder: (context, index) {
                            final topic = filtered[index];

                            return Padding(
                              padding: const EdgeInsets.only(bottom: 16.0),
                              child: GlassContainer(
                                padding: EdgeInsets.zero,
                                child: InkWell(
                                  onTap: () {
                                    HapticFeedback.lightImpact();
                                    context.push('/forum/topic/${topic.id}');
                                  },
                                  borderRadius: BorderRadius.circular(AppTheme.radiusL),
                                  child: Padding(
                                    padding: const EdgeInsets.all(16.0),
                                    child: Column(
                                      crossAxisAlignment: CrossAxisAlignment.start,
                                      children: [
                                        Row(
                                          children: [
                                            if (topic.isPinned) ...[
                                              const Icon(Icons.push_pin, size: 14, color: AppTheme.royalGold),
                                              const SizedBox(width: 6),
                                            ],
                                            Expanded(
                                              child: Text(
                                                topic.title,
                                                style: const TextStyle(
                                                  fontFamily: 'Outfit',
                                                  fontWeight: FontWeight.w800,
                                                  fontSize: 14,
                                                  color: Colors.white,
                                                  letterSpacing: -0.2,
                                                ),
                                              ),
                                            ),
                                            if (topic.isLocked) ...[
                                              const SizedBox(width: 8),
                                              const Icon(Icons.lock_outline, size: 14, color: Colors.white38),
                                            ],
                                          ],
                                        ),
                                        const SizedBox(height: 6),
                                        Text(
                                          topic.content.length > 100
                                              ? '${topic.content.substring(0, 97)}...'
                                              : topic.content,
                                          style: const TextStyle(
                                            color: AppTheme.textSecondaryDark,
                                            fontSize: 12,
                                            height: 1.4,
                                          ),
                                        ),
                                        const SizedBox(height: 14),
                                        const Divider(color: Colors.white10, height: 1),
                                        const SizedBox(height: 12),
                                        Row(
                                          children: [
                                            CircleAvatar(
                                              radius: 10,
                                              backgroundColor: AppTheme.royalGold.withValues(alpha: 0.1),
                                              backgroundImage: topic.authorPhotoUrl != null && topic.authorPhotoUrl!.isNotEmpty
                                                  ? NetworkImage(topic.authorPhotoUrl!)
                                                  : null,
                                              child: topic.authorPhotoUrl == null || topic.authorPhotoUrl!.isEmpty
                                                  ? const Icon(Icons.person, color: AppTheme.royalGold, size: 10)
                                                  : null,
                                            ),
                                            const SizedBox(width: 6),
                                            Text(
                                              topic.authorName,
                                              style: const TextStyle(
                                                color: Colors.white70,
                                                fontSize: 11,
                                                fontWeight: FontWeight.w600,
                                              ),
                                            ),
                                            const Spacer(),
                                            _buildStat(Icons.visibility_outlined, '${topic.viewCount}'),
                                            const SizedBox(width: 14),
                                            _buildStat(Icons.comment_outlined, '${topic.replyCount}'),
                                            const SizedBox(width: 14),
                                            Text(
                                              AppUtils.formatDate(topic.createdAt),
                                              style: const TextStyle(
                                                color: Colors.white38,
                                                fontSize: 9,
                                                fontWeight: FontWeight.bold,
                                              ),
                                            ),
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

  Widget _buildStat(IconData icon, String value) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(icon, size: 12, color: AppTheme.royalGold.withValues(alpha: 0.7)),
        const SizedBox(width: 4),
        Text(
          value,
          style: const TextStyle(
            color: AppTheme.textSecondaryDark,
            fontSize: 10,
            fontWeight: FontWeight.bold,
          ),
        ),
      ],
    );
  }

  void _showCreateTopicDialog(BuildContext context, WidgetRef ref) {
    final titleCtrl = TextEditingController();
    final contentCtrl = TextEditingController();
    bool isSubmitting = false;

    showDialog(
      context: context,
      builder: (ctx) => StatefulBuilder(
        builder: (ctx, setState) => AlertDialog(
          backgroundColor: AppTheme.midnightSurface,
          title: const Text(
            'CREATE NEW TOPIC',
            style: TextStyle(
              color: AppTheme.royalGold,
              fontSize: 14,
              fontWeight: FontWeight.w900,
              letterSpacing: 1.5,
            ),
          ),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextField(
                controller: titleCtrl,
                style: const TextStyle(color: Colors.white, fontSize: 13),
                decoration: const InputDecoration(
                  labelText: 'Topic Title',
                  labelStyle: TextStyle(color: Colors.white38, fontSize: 11),
                  enabledBorder: UnderlineInputBorder(borderSide: BorderSide(color: Colors.white10)),
                ),
              ),
              const SizedBox(height: 12),
              TextField(
                controller: contentCtrl,
                maxLines: 4,
                style: const TextStyle(color: Colors.white, fontSize: 13),
                decoration: const InputDecoration(
                  labelText: 'Topic Description / Question',
                  labelStyle: TextStyle(color: Colors.white38, fontSize: 11),
                  enabledBorder: UnderlineInputBorder(borderSide: BorderSide(color: Colors.white10)),
                ),
              ),
            ],
          ),
          actions: [
            TextButton(
              onPressed: isSubmitting ? null : () => Navigator.pop(ctx),
              child: const Text('CANCEL', style: TextStyle(color: Colors.white54)),
            ),
            ElevatedButton(
              onPressed: isSubmitting
                  ? null
                  : () async {
                      final title = titleCtrl.text.trim();
                      final content = contentCtrl.text.trim();
                      if (title.isEmpty || content.isEmpty) return;

                      setState(() {
                        isSubmitting = true;
                      });

                      final created = await ref
                          .read(forumServiceProvider)
                          .createTopic(widget.categoryId, title, content);

                      if (created != null) {
                        HapticFeedback.heavyImpact();
                        ref.invalidate(forumTopicsProvider(widget.categoryId));
                        if (ctx.mounted) {
                          Navigator.pop(ctx);
                          // Go directly to newly created topic detail
                          context.push('/forum/topic/${created.id}');
                        }
                      } else {
                        setState(() {
                          isSubmitting = false;
                        });
                        if (ctx.mounted) {
                          ScaffoldMessenger.of(context).showSnackBar(
                            const SnackBar(content: Text('Failed to create topic. Please try again.')),
                          );
                        }
                      }
                    },
              child: isSubmitting
                  ? const SizedBox(
                      width: 16,
                      height: 16,
                      child: CircularProgressIndicator(color: Colors.black, strokeWidth: 2),
                    )
                  : const Text('POST TOPIC', style: TextStyle(color: Colors.black, fontWeight: FontWeight.bold)),
            ),
          ],
        ),
      ),
    );
  }
}
