import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/widgets/glass_container.dart';
import '../../../core/widgets/app_scaffold.dart';
import '../../../core/widgets/async_value_widget.dart';
import '../../../core/widgets/logo_spinner.dart';
import '../../../core/utils/app_utils.dart';
import '../../../core/services/org_config_service.dart';
import '../../../features/forum/forum_service.dart';
import '../../../features/auth/auth_service.dart';
import '../../../core/widgets/confirm_dialog.dart';

class ForumTopicDetailScreen extends ConsumerStatefulWidget {
  final int topicId;
  const ForumTopicDetailScreen({super.key, required this.topicId});

  @override
  ConsumerState<ForumTopicDetailScreen> createState() =>
      _ForumTopicDetailScreenState();
}

class _ForumTopicDetailScreenState
    extends ConsumerState<ForumTopicDetailScreen> {
  final TextEditingController _replyController = TextEditingController();
  final FocusNode _replyFocusNode = FocusNode();

  ForumPost? _replyingToPost;
  bool _isSubmitting = false;

  @override
  void dispose() {
    _replyController.dispose();
    _replyFocusNode.dispose();
    super.dispose();
  }

  String _getParentAuthorName(List<ForumPost> posts, int parentPostId) {
    try {
      final parent = posts.firstWhere((p) => p.id == parentPostId);
      return parent.authorName;
    } catch (_) {
      return 'Author';
    }
  }

  void _scrollToBottom() {
    // If needed, we can scroll the thread list to the bottom.
  }

  @override
  Widget build(BuildContext context) {
    final topicAsync = ref.watch(topicDetailProvider(widget.topicId));
    final postsAsync = ref.watch(topicPostsProvider(widget.topicId));

    final selfProfile = ref.watch(userProfileProvider).value;
    final myMemberId = selfProfile?['id'];
    final roleAsync = ref.watch(roleProvider);
    final isAdmin = roleAsync.value?.isStaffAdminRole ?? false;
    final dateFormat = ref.watch(orgDateFormatProvider);

    return AppScaffold(
      title: 'Topic Discussion',
      breadcrumb: 'PORTAL > DISCUSSIONS > TOPIC',
      child: AsyncValueWidget<ForumTopic?>(
        value: topicAsync,
        loadingMessage: 'Loading topic...',
        onRetry: () => ref.invalidate(topicDetailProvider(widget.topicId)),
        data: (topic) {
          if (topic == null) {
            return const Center(
                child: Text('Topic not found.',
                    style: TextStyle(color: Colors.white54)));
          }

          final canDeleteTopic =
              myMemberId != null && topic.authorId == myMemberId || isAdmin;

          return Column(
            children: [
              Expanded(
                child: AsyncValueWidget<List<ForumPost>>(
                  value: postsAsync,
                  loadingMessage: 'Loading replies...',
                  onRetry: () =>
                      ref.invalidate(topicPostsProvider(widget.topicId)),
                  data: (posts) {
                    return RefreshIndicator(
                      color: AppTheme.royalGold,
                      onRefresh: () async {
                        HapticFeedback.mediumImpact();
                        ref.invalidate(topicDetailProvider(widget.topicId));
                        ref.invalidate(topicPostsProvider(widget.topicId));
                      },
                      child: ListView.builder(
                        padding: const EdgeInsets.fromLTRB(20, 16, 20, 80),
                        itemCount: posts.length + 1,
                        itemBuilder: (context, index) {
                          if (index == 0) {
                            // Main Topic Original Post
                            return Padding(
                              padding: const EdgeInsets.only(bottom: 20.0),
                              child: Container(
                                decoration: BoxDecoration(
                                  color: AppTheme.midnightSurface
                                      .withValues(alpha: 0.5),
                                  borderRadius:
                                      BorderRadius.circular(AppTheme.radiusL),
                                  border: Border.all(
                                      color: AppTheme.royalGold
                                          .withValues(alpha: 0.3),
                                      width: 1.5),
                                ),
                                padding: const EdgeInsets.all(18.0),
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Row(
                                      children: [
                                        CircleAvatar(
                                          radius: 18,
                                          backgroundColor: AppTheme.royalGold
                                              .withValues(alpha: 0.1),
                                          backgroundImage:
                                              topic.authorPhotoUrl != null &&
                                                      topic.authorPhotoUrl!
                                                          .isNotEmpty
                                                  ? NetworkImage(
                                                      topic.authorPhotoUrl!)
                                                  : null,
                                          child: topic.authorPhotoUrl == null ||
                                                  topic.authorPhotoUrl!.isEmpty
                                              ? const Icon(Icons.person,
                                                  color: AppTheme.royalGold,
                                                  size: 18)
                                              : null,
                                        ),
                                        const SizedBox(width: 10),
                                        Expanded(
                                          child: Column(
                                            crossAxisAlignment:
                                                CrossAxisAlignment.start,
                                            children: [
                                              Text(
                                                topic.authorName,
                                                style: const TextStyle(
                                                    color: Colors.white,
                                                    fontSize: 13,
                                                    fontWeight:
                                                        FontWeight.bold),
                                              ),
                                              const SizedBox(height: 2),
                                              Text(
                                                'Posted on ${AppUtils.formatDate(topic.createdAt, format: dateFormat.identifier)}',
                                                style: const TextStyle(
                                                    color: AppTheme
                                                        .textSecondaryDark,
                                                    fontSize: 10),
                                              ),
                                            ],
                                          ),
                                        ),
                                        if (canDeleteTopic)
                                          IconButton(
                                            icon: const Icon(
                                                Icons.delete_outline,
                                                color: Colors.redAccent,
                                                size: 20),
                                            onPressed: () async {
                                              HapticFeedback.lightImpact();
                                              final confirm =
                                                  await showConfirmDialog(
                                                context,
                                                title: 'Delete Topic?',
                                                message:
                                                    'This will delete the topic and all replies permanently.',
                                                confirmLabel: 'Delete',
                                                destructive: true,
                                              );
                                              if (confirm) {
                                                HapticFeedback.mediumImpact();
                                                final ok = await ref
                                                    .read(forumServiceProvider)
                                                    .deleteTopic(topic.id);
                                                if (ok && context.mounted) {
                                                  context.pop();
                                                }
                                              }
                                            },
                                          ),
                                      ],
                                    ),
                                    const SizedBox(height: 16),
                                    Text(
                                      topic.title,
                                      style: const TextStyle(
                                        fontFamily: 'Outfit',
                                        fontSize: 18,
                                        fontWeight: FontWeight.w900,
                                        color: AppTheme.royalGold,
                                        letterSpacing: -0.3,
                                      ),
                                    ),
                                    const SizedBox(height: 10),
                                    const Divider(color: Colors.white10),
                                    const SizedBox(height: 10),
                                    Text(
                                      topic.content,
                                      style: const TextStyle(
                                          color: Colors.white,
                                          fontSize: 13.5,
                                          height: 1.5),
                                    ),
                                  ],
                                ),
                              ),
                            );
                          }

                          // Replies / Posts list
                          final post = posts[index - 1];
                          final canDeletePost = myMemberId != null &&
                                  post.authorId == myMemberId ||
                              isAdmin;

                          return Padding(
                            padding: const EdgeInsets.only(bottom: 12.0),
                            child: GlassContainer(
                              padding: const EdgeInsets.all(14.0),
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Row(
                                    children: [
                                      CircleAvatar(
                                        radius: 12,
                                        backgroundColor: AppTheme.royalGold
                                            .withValues(alpha: 0.1),
                                        backgroundImage: post.authorPhotoUrl !=
                                                    null &&
                                                post.authorPhotoUrl!.isNotEmpty
                                            ? NetworkImage(post.authorPhotoUrl!)
                                            : null,
                                        child: post.authorPhotoUrl == null ||
                                                post.authorPhotoUrl!.isEmpty
                                            ? const Icon(Icons.person,
                                                color: AppTheme.royalGold,
                                                size: 12)
                                            : null,
                                      ),
                                      const SizedBox(width: 8),
                                      Expanded(
                                        child: Column(
                                          crossAxisAlignment:
                                              CrossAxisAlignment.start,
                                          children: [
                                            Text(
                                              post.authorName,
                                              style: const TextStyle(
                                                  color: Colors.white70,
                                                  fontSize: 11.5,
                                                  fontWeight: FontWeight.bold),
                                            ),
                                            const SizedBox(height: 1),
                                            Text(
                                              AppUtils.formatDate(
                                                  post.createdAt,
                                                  format:
                                                      dateFormat.identifier),
                                              style: const TextStyle(
                                                  color: Colors.white30,
                                                  fontSize: 8.5),
                                            ),
                                          ],
                                        ),
                                      ),
                                      // Reply icon
                                      IconButton(
                                        icon: const Icon(Icons.reply_outlined,
                                            color: AppTheme.royalGold,
                                            size: 16),
                                        onPressed: () {
                                          HapticFeedback.lightImpact();
                                          setState(() {
                                            _replyingToPost = post;
                                          });
                                          _replyFocusNode.requestFocus();
                                        },
                                      ),
                                      if (canDeletePost)
                                        IconButton(
                                          icon: const Icon(Icons.delete_outline,
                                              color: Colors.redAccent,
                                              size: 16),
                                          onPressed: () async {
                                            HapticFeedback.lightImpact();
                                            final confirm =
                                                await showConfirmDialog(
                                              context,
                                              title: 'Delete Reply?',
                                              confirmLabel: 'Delete',
                                              destructive: true,
                                            );
                                            if (confirm) {
                                              HapticFeedback.mediumImpact();
                                              final ok = await ref
                                                  .read(forumServiceProvider)
                                                  .deletePost(post.id);
                                              if (ok) {
                                                ref.invalidate(
                                                    topicPostsProvider(
                                                        widget.topicId));
                                                ref.invalidate(
                                                    topicDetailProvider(
                                                        widget.topicId));
                                              }
                                            }
                                          },
                                        ),
                                    ],
                                  ),
                                  if (post.parentPostId != null) ...[
                                    const SizedBox(height: 6),
                                    Row(
                                      children: [
                                        const Icon(
                                            Icons
                                                .subdirectory_arrow_right_rounded,
                                            size: 12,
                                            color: AppTheme.royalGold),
                                        const SizedBox(width: 4),
                                        Text(
                                          'replying to ${_getParentAuthorName(posts, post.parentPostId!)}',
                                          style: TextStyle(
                                            color: AppTheme.royalGold
                                                .withValues(alpha: 0.7),
                                            fontSize: 9.5,
                                            fontWeight: FontWeight.bold,
                                          ),
                                        ),
                                      ],
                                    ),
                                  ],
                                  const SizedBox(height: 10),
                                  Text(
                                    post.content,
                                    style: const TextStyle(
                                        color: Colors.white,
                                        fontSize: 12.5,
                                        height: 1.4),
                                  ),
                                ],
                              ),
                            ),
                          );
                        },
                      ),
                    );
                  },
                ),
              ),
              // Sticky Reply Input Bar
              if (!topic.isLocked) _buildReplyInputBar(),
            ],
          );
        },
      ),
    );
  }

  Widget _buildReplyInputBar() {
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 8.0),
      decoration: BoxDecoration(
        color: isDark ? AppTheme.midnightSurface : Colors.white,
        border: const Border(top: BorderSide(color: Colors.white10)),
      ),
      child: SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            if (_replyingToPost != null)
              Container(
                padding:
                    const EdgeInsets.symmetric(horizontal: 10.0, vertical: 6.0),
                margin: const EdgeInsets.only(bottom: 8.0),
                decoration: BoxDecoration(
                  color: Colors.white.withValues(alpha: 0.05),
                  borderRadius: BorderRadius.circular(AppTheme.radiusS),
                ),
                child: Row(
                  children: [
                    const Icon(Icons.reply,
                        size: 14, color: AppTheme.royalGold),
                    const SizedBox(width: 6),
                    Expanded(
                      child: Text(
                        'Replying to ${_replyingToPost!.authorName}',
                        style: const TextStyle(
                            color: AppTheme.textSecondaryDark,
                            fontSize: 10.5,
                            fontWeight: FontWeight.bold),
                      ),
                    ),
                    GestureDetector(
                      onTap: () {
                        setState(() {
                          _replyingToPost = null;
                        });
                      },
                      child: const Icon(Icons.close,
                          size: 14, color: Colors.white54),
                    ),
                  ],
                ),
              ),
            Row(
              children: [
                Expanded(
                  child: TextField(
                    controller: _replyController,
                    focusNode: _replyFocusNode,
                    style: const TextStyle(color: Colors.white, fontSize: 13),
                    decoration: InputDecoration(
                      hintText: 'Type your reply...',
                      hintStyle:
                          const TextStyle(color: Colors.white24, fontSize: 12),
                      filled: true,
                      fillColor: Colors.black.withValues(alpha: 0.15),
                      contentPadding: const EdgeInsets.symmetric(
                          horizontal: 16, vertical: 10),
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(20),
                        borderSide: BorderSide.none,
                      ),
                      enabledBorder: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(20),
                        borderSide: BorderSide.none,
                      ),
                      focusedBorder: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(20),
                        borderSide: const BorderSide(
                            color: AppTheme.royalGold, width: 1),
                      ),
                    ),
                  ),
                ),
                const SizedBox(width: 8),
                IconButton(
                  icon: _isSubmitting
                      ? SizedBox(
                          width: 20,
                          height: 20,
                          child: LogoSpinner.small(),
                        )
                      : const Icon(Icons.send_rounded,
                          color: AppTheme.royalGold),
                  onPressed: _isSubmitting ? null : _submitReply,
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  void _submitReply() async {
    final text = _replyController.text.trim();
    if (text.isEmpty) return;

    setState(() {
      _isSubmitting = true;
    });

    final created = await ref.read(forumServiceProvider).createPost(
          widget.topicId,
          text,
          parentPostId: _replyingToPost?.id,
        );

    if (created != null) {
      HapticFeedback.mediumImpact();
      _replyController.clear();
      setState(() {
        _replyingToPost = null;
        _isSubmitting = false;
      });
      ref.invalidate(topicPostsProvider(widget.topicId));
      ref.invalidate(topicDetailProvider(widget.topicId));
      _scrollToBottom();
    } else {
      setState(() {
        _isSubmitting = false;
      });
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
              content: Text('Failed to post reply. Please try again.')),
        );
      }
    }
  }
}
