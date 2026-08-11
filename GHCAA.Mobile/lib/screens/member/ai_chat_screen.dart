import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/logo_spinner.dart';
import '../../features/assistant/assistant_service.dart';

class Message {
  final String text;
  final bool isMe;
  Message(this.text, this.isMe);
}

final chatMessagesProvider = StateProvider<List<Message>>((ref) => [
  Message('Hi! I am the Haragangian AI. How can I assist you with alumni connections today?', false),
]);

class AIChatScreen extends ConsumerStatefulWidget {
  const AIChatScreen({super.key});

  @override
  ConsumerState<AIChatScreen> createState() => _AIChatScreenState();
}

class _AIChatScreenState extends ConsumerState<AIChatScreen> {
  final _controller = TextEditingController();
  bool _isTyping = false;

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _sendMessage() async {
    final text = _controller.text.trim();
    if (text.isEmpty) return;

    _controller.clear();
    ref.read(chatMessagesProvider.notifier).update((state) => [...state, Message(text, true)]);
    
    setState(() => _isTyping = true);
    
    final response = await ref.read(assistantServiceProvider).ask(text);
    
    if (mounted) {
      ref.read(chatMessagesProvider.notifier).update((state) => [...state, Message(response ?? 'Synchronization interrupted. Please try again.', false)]);
      setState(() => _isTyping = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final messages = ref.watch(chatMessagesProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      title: 'GHC AI Assistant',
      breadcrumb: 'Executive Hub > Alumni Intelligence',
      child: Column(
        children: [
          Expanded(
            child: ListView.builder(
              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 24),
              itemCount: messages.length,
              itemBuilder: (context, index) {
                return _buildMessage(context, messages[index].text, messages[index].isMe);
              },
            ),
          ),
          if (_isTyping)
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 24.0, vertical: 12),
              child: Row(
                children: [
                  SizedBox(
                    width: 16,
                    height: 16,
                    child: LogoSpinner.small(size: 16),
                  ),
                  const SizedBox(width: 8),
                  Text('SYNTHESIZING...', style: TextStyle(fontSize: 9, color: AppTheme.royalGold.withValues(alpha: 0.7), fontWeight: FontWeight.w900, letterSpacing: 1.5)),
                ],
              ),
            ),
          _buildInputArea(context, isDark),
        ],
      ),
    );
  }

  Widget _buildMessage(BuildContext context, String text, bool isMe) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 16.0),
      child: Align(
        alignment: isMe ? Alignment.centerRight : Alignment.centerLeft,
        child: GlassContainer(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: isMe ? CrossAxisAlignment.end : CrossAxisAlignment.start,
            children: [
              Text(
                isMe ? 'YOU' : 'HARAGANGIAN AI', 
                style: TextStyle(fontSize: 8, fontWeight: FontWeight.w900, color: isMe ? Colors.white38 : AppTheme.royalGold, letterSpacing: 1)
              ),
              const SizedBox(height: 6),
              ConstrainedBox(
                constraints: BoxConstraints(maxWidth: MediaQuery.of(context).size.width * 0.75),
                child: Text(
                  text, 
                  style: TextStyle(color: isMe ? Colors.white : Colors.white.withValues(alpha: 0.9), fontSize: 13, height: 1.5)
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildInputArea(BuildContext context, bool isDark) {
    return Container(
      padding: const EdgeInsets.fromLTRB(20, 12, 20, 32),
      decoration: BoxDecoration(
        color: Colors.black.withValues(alpha: 0.2),
        border: Border(top: BorderSide(color: Colors.white.withValues(alpha: 0.05))),
      ),
      child: Row(
        children: [
          Expanded(
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 16),
              decoration: BoxDecoration(
                color: Colors.white.withValues(alpha: 0.05),
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: Colors.white.withValues(alpha: 0.1)),
              ),
              child: TextField(
                controller: _controller, 
                onSubmitted: (_) => _sendMessage(), 
                style: const TextStyle(color: Colors.white, fontSize: 14),
                decoration: const InputDecoration(
                  hintText: 'Ask the association intelligence...', 
                  hintStyle: TextStyle(color: AppTheme.textSecondaryDark, fontSize: 13),
                  border: InputBorder.none
                )
              ),
            ),
          ),
          const SizedBox(width: 8),
          IconButton(
            onPressed: () {
              HapticFeedback.lightImpact();
              _sendMessage();
            },
            icon: const Icon(Icons.send_rounded, color: AppTheme.royalGold),
          ),
        ],
      ),
    );
  }
}
