import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../features/messaging/chat_service.dart';
import '../../core/services/app_localizations.dart';
import 'package:intl/intl.dart';

class ChatRoomScreen extends ConsumerStatefulWidget {
  final int otherUserId;

  const ChatRoomScreen({super.key, required this.otherUserId});

  @override
  ConsumerState<ChatRoomScreen> createState() => _ChatRoomScreenState();
}

class _ChatRoomScreenState extends ConsumerState<ChatRoomScreen> {
  final TextEditingController _messageController = TextEditingController();
  final ScrollController _scrollController = ScrollController();
  final List<Map<String, dynamic>> _messages = [];
  bool _isInit = false;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(chatServiceProvider).initHub();
    });
  }

  void _scrollToBottom() {
    if (_scrollController.hasClients) {
      _scrollController.animateTo(
        _scrollController.position.maxScrollExtent,
        duration: const Duration(milliseconds: 300),
        curve: Curves.easeOut,
      );
    }
  }

  Future<void> _sendMessage() async {
    final text = _messageController.text.trim();
    if (text.isEmpty) return;

    _messageController.clear();
    HapticFeedback.lightImpact();

    try {
      await ref.read(chatServiceProvider).sendDirectMessage(widget.otherUserId, text);
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Failed to send: $e'), backgroundColor: Colors.redAccent));
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final historyAsync = ref.watch(chatHistoryProvider(widget.otherUserId));
    final stream = ref.watch(chatServiceProvider).messageStream;

    return AppScaffold(
      title: AppLocalizations.of(context).translate('chat_room_title'),
      breadcrumb: 'Portal > Secure Chat',
      child: Column(
        children: [
          Expanded(
            child: AsyncValueWidget<List<dynamic>>(
              value: historyAsync,
              data: (history) {
                if (!_isInit) {
                  _messages.clear();
                  _messages.addAll(history.map((e) => Map<String, dynamic>.from(e)));
                  _isInit = true;
                  WidgetsBinding.instance.addPostFrameCallback((_) => _scrollToBottom());
                }

                return StreamBuilder<Map<String, dynamic>>(
                  stream: stream,
                  builder: (context, snapshot) {
                    if (snapshot.hasData) {
                      final msg = snapshot.data!;
                      if (msg['senderUserId'] == widget.otherUserId || msg['receiverUserId'] == widget.otherUserId) {
                        if (!_messages.any((m) => m['id'] == msg['id'])) {
                          _messages.add(msg);
                          WidgetsBinding.instance.addPostFrameCallback((_) => _scrollToBottom());
                        }
                      }
                    }

                    return ListView.builder(
                      controller: _scrollController,
                      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 20),
                      itemCount: _messages.length,
                      itemBuilder: (context, index) {
                        final m = _messages[index];
                        final isMe = m['senderUserId'] != widget.otherUserId;
                        return _buildMessageBubble(m, isMe);
                      },
                    );
                  },
                );
              },
            ),
          ),
          _buildInputArea(),
        ],
      ),
    );
  }

  Widget _buildMessageBubble(Map<String, dynamic> m, bool isMe) {
    return Align(
      alignment: isMe ? Alignment.centerRight : Alignment.centerLeft,
      child: Container(
        margin: const EdgeInsets.only(bottom: 12),
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
        constraints: BoxConstraints(maxWidth: MediaQuery.of(context).size.width * 0.75),
        decoration: BoxDecoration(
          color: isMe ? AppTheme.royalGold.withValues(alpha: 0.15) : Colors.white.withValues(alpha: 0.05),
          borderRadius: BorderRadius.only(
            topLeft: const Radius.circular(16),
            topRight: const Radius.circular(16),
            bottomLeft: Radius.circular(isMe ? 16 : 4),
            bottomRight: Radius.circular(isMe ? 4 : 16),
          ),
          border: Border.all(color: isMe ? AppTheme.royalGold.withValues(alpha: 0.2) : Colors.white12),
        ),
        child: Column(
          crossAxisAlignment: isMe ? CrossAxisAlignment.end : CrossAxisAlignment.start,
          children: [
            Text(m['content'] ?? '', style: const TextStyle(color: Colors.white, fontSize: 14)),
            const SizedBox(height: 4),
            Text(
              _formatTime(m['sentAt'] ?? m['createdAt']),
              style: TextStyle(fontSize: 9, color: isMe ? AppTheme.royalGold.withValues(alpha: 0.7) : Colors.white38, fontWeight: FontWeight.bold),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildInputArea() {
    return Container(
      padding: EdgeInsets.fromLTRB(16, 8, 16, MediaQuery.of(context).padding.bottom + 16),
      decoration: BoxDecoration(
        color: Colors.black.withValues(alpha: 0.3),
        border: const Border(top: BorderSide(color: Colors.white10)),
      ),
      child: Row(
        children: [
          Expanded(
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 16),
              decoration: BoxDecoration(
                color: Colors.white.withValues(alpha: 0.05),
                borderRadius: BorderRadius.circular(24),
                border: Border.all(color: Colors.white10),
              ),
              child: TextField(
                controller: _messageController,
                style: const TextStyle(color: Colors.white, fontSize: 14),
                decoration: const InputDecoration(
                  hintText: 'Type your message...',
                  hintStyle: TextStyle(color: Colors.white24, fontSize: 13),
                  border: InputBorder.none,
                ),
                onSubmitted: (_) => _sendMessage(),
              ),
            ),
          ),
          const SizedBox(width: 12),
          IconButton(
            onPressed: _sendMessage,
            icon: const Icon(Icons.send_rounded, color: AppTheme.royalGold),
            style: IconButton.styleFrom(backgroundColor: AppTheme.royalGold.withValues(alpha: 0.1)),
          ),
        ],
      ),
    );
  }

  String _formatTime(dynamic time) {
    if (time == null) return '';
    try {
      final dt = DateTime.parse(time.toString());
      return DateFormat.Hm().format(dt);
    } catch (e) {
      return '';
    }
  }

  @override
  void dispose() {
    _messageController.dispose();
    _scrollController.dispose();
    super.dispose();
  }
}
