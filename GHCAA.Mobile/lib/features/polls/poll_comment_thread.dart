import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_exception.dart';
import '../../core/theme/app_theme.dart';
import '../../core/utils/app_utils.dart';
import '../../core/widgets/confirm_dialog.dart';
import '../../core/widgets/logo_spinner.dart';
import '../auth/auth_service.dart';
import 'poll_service.dart';

// Comments under one poll (84.71). The server decides who can post and says why in closedReason.
class PollCommentThread extends ConsumerStatefulWidget {
  final int pollId;
  final ValueChanged<int>? onCountChange;

  const PollCommentThread({super.key, required this.pollId, this.onCountChange});

  @override
  ConsumerState<PollCommentThread> createState() => _PollCommentThreadState();
}

class _PollCommentThreadState extends ConsumerState<PollCommentThread> {
  final _draft = TextEditingController();
  final _replyDraft = TextEditingController();
  List<PollComment> _comments = const [];
  int _totalCount = 0;
  int _page = 0;
  String? _closedReason;
  bool _loading = true;
  bool _posting = false;
  int? _replyTo;

  @override
  void initState() {
    super.initState();
    _load(1);
  }

  @override
  void dispose() {
    _draft.dispose();
    _replyDraft.dispose();
    super.dispose();
  }

  bool get _hasMore => _comments.length < _totalCount;

  Future<void> _load(int page) async {
    setState(() => _loading = true);
    try {
      final result = await ref.read(pollServiceProvider).getComments(widget.pollId, page);
      if (!mounted) return;
      setState(() {
        _comments = page == 1 ? result.comments : [..._comments, ...result.comments];
        _totalCount = result.totalCount;
        _closedReason = result.closedReason;
        _page = page;
      });
    } on DioException catch (e) {
      _tell(ApiException.serverMessage(e.response?.data) ?? 'Comments could not be loaded.');
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _post({int? parentId}) async {
    final box = parentId == null ? _draft : _replyDraft;
    final text = box.text.trim();
    if (text.isEmpty || _posting) return;
    setState(() => _posting = true);
    try {
      final added = await ref.read(pollServiceProvider).postComment(widget.pollId, text, parentId: parentId);
      if (!mounted) return;
      box.clear();
      setState(() {
        if (parentId != null) {
          _comments = [
            for (final c in _comments) c.id == parentId ? c.copyWith(replies: [...c.replies, added]) : c,
          ];
          _replyTo = null;
        } else {
          // Comments come oldest first, so a new one belongs at the end. While later pages are
          // still unloaded it would show in the wrong place, so only the count moves.
          if (!_hasMore) _comments = [..._comments, added];
          _totalCount++;
        }
      });
      widget.onCountChange?.call(_countAll());
    } on DioException catch (e) {
      _tell(ApiException.serverMessage(e.response?.data) ?? 'The comment could not be posted.');
    } finally {
      if (mounted) setState(() => _posting = false);
    }
  }

  Future<void> _remove(PollComment comment) async {
    final ok = await showConfirmDialog(context, title: 'Remove this comment?', confirmLabel: 'Remove', destructive: true);
    if (!ok || !mounted) return;
    try {
      await ref.read(pollServiceProvider).removeComment(widget.pollId, comment.id);
      if (!mounted) return;
      PollComment mark(PollComment c) =>
          c.id == comment.id ? c.copyWith(removed: true, content: '') : c.copyWith(replies: c.replies.map(mark).toList());
      setState(() => _comments = _comments.map(mark).toList());
    } on DioException catch (e) {
      _tell(ApiException.serverMessage(e.response?.data) ?? 'The comment could not be removed.');
    }
  }

  // Loaded top-level comments are only part of the thread, so the badge uses the server total
  // plus the replies we can see. The server sends the exact count on the next poll reload.
  int _countAll() => _totalCount + _comments.fold(0, (sum, c) => sum + c.replies.length);

  void _tell(String message) {
    if (!mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));
  }

  @override
  Widget build(BuildContext context) {
    final myId = ref.watch(userProfileProvider).value?['id'];
    final isAdmin = ref.watch(isAdminProvider).value ?? false;
    bool canRemove(PollComment c) => !c.removed && (isAdmin || (myId != null && c.authorId == myId));

    if (_loading && _page == 0) {
      return const Padding(padding: EdgeInsets.all(16), child: Center(child: LogoSpinner(size: 48)));
    }

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        if (_comments.isEmpty)
          const Padding(
            padding: EdgeInsets.symmetric(vertical: 8),
            child: Text('No comments yet.', style: TextStyle(color: Colors.white54, fontSize: 12)),
          ),
        for (final c in _comments) ...[
          _CommentTile(comment: c, canRemove: canRemove(c), onRemove: () => _remove(c),
              onReply: _closedReason == null && !c.removed ? () => setState(() => _replyTo = c.id) : null),
          for (final r in c.replies)
            Padding(
              padding: const EdgeInsets.only(left: 24),
              child: _CommentTile(comment: r, canRemove: canRemove(r), onRemove: () => _remove(r)),
            ),
          if (_replyTo == c.id)
            Padding(
              padding: const EdgeInsets.only(left: 24, bottom: 8),
              child: _ComposeBox(
                controller: _replyDraft,
                hint: 'Write a reply',
                busy: _posting,
                onSend: () => _post(parentId: c.id),
                onCancel: () => setState(() => _replyTo = null),
              ),
            ),
        ],
        if (_hasMore)
          TextButton(
            onPressed: _loading ? null : () => _load(_page + 1),
            child: const Text('Show more comments', style: TextStyle(color: AppTheme.royalGold)),
          ),
        const SizedBox(height: 8),
        if (_closedReason != null)
          Text(_closedReason!, key: const Key('poll-comments-closed'), style: const TextStyle(color: Colors.white54, fontSize: 12))
        else
          _ComposeBox(controller: _draft, hint: 'Write a comment', busy: _posting, onSend: () => _post()),
      ],
    );
  }
}

class _CommentTile extends StatelessWidget {
  final PollComment comment;
  final bool canRemove;
  final VoidCallback onRemove;
  final VoidCallback? onReply;

  const _CommentTile({required this.comment, required this.canRemove, required this.onRemove, this.onReply});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 6),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Expanded(
                child: Text(
                  '${comment.authorName}  ·  ${AppUtils.formatDate(comment.postedAt.toLocal(), includeTime: true)}',
                  style: const TextStyle(color: Colors.white54, fontSize: 11),
                ),
              ),
              if (onReply != null)
                IconButton(
                  icon: const Icon(Icons.reply, size: 16, color: Colors.white54),
                  tooltip: 'Reply',
                  visualDensity: VisualDensity.compact,
                  onPressed: onReply,
                ),
              if (canRemove)
                IconButton(
                  icon: const Icon(Icons.delete_outline, size: 16, color: Colors.redAccent),
                  tooltip: 'Remove',
                  visualDensity: VisualDensity.compact,
                  onPressed: onRemove,
                ),
            ],
          ),
          Text(
            comment.removed ? 'This comment was removed.' : comment.content,
            style: TextStyle(
              color: comment.removed ? Colors.white38 : Colors.white,
              fontStyle: comment.removed ? FontStyle.italic : FontStyle.normal,
              fontSize: 13,
            ),
          ),
        ],
      ),
    );
  }
}

class _ComposeBox extends StatelessWidget {
  final TextEditingController controller;
  final String hint;
  final bool busy;
  final VoidCallback onSend;
  final VoidCallback? onCancel;

  const _ComposeBox({required this.controller, required this.hint, required this.busy, required this.onSend, this.onCancel});

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Expanded(
          child: TextField(
            controller: controller,
            minLines: 1,
            maxLines: 4,
            maxLength: PollService.maxCommentLength,
            style: const TextStyle(color: Colors.white, fontSize: 13),
            decoration: InputDecoration(hintText: hint, isDense: true, counterText: ''),
          ),
        ),
        if (onCancel != null)
          IconButton(icon: const Icon(Icons.close, size: 18, color: Colors.white54), tooltip: 'Cancel', onPressed: onCancel),
        IconButton(
          icon: const Icon(Icons.send, size: 18, color: AppTheme.royalGold),
          tooltip: 'Send',
          onPressed: busy ? null : onSend,
        ),
      ],
    );
  }
}
