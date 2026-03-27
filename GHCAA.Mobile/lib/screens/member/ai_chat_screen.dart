import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/glass_container.dart';
import '../../features/assistant/assistant_service.dart';

class Message {
  final String text;
  final bool isMe;
  Message(this.text, this.isMe);
}

final chatMessagesProvider = StateProvider<List<Message>>((ref) => [
  Message('Hi! I am Haraganga AI. How can I assist you with alumni connections today?', false),
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
      ref.read(chatMessagesProvider.notifier).update((state) => [...state, Message(response ?? 'No reply', false)]);
      setState(() => _isTyping = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final messages = ref.watch(chatMessagesProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return Scaffold(
      appBar: AppBar(title: const Text('Haraganga AI'), backgroundColor: Colors.transparent, elevation: 0),
      body: Container(
        decoration: BoxDecoration(
          gradient: RadialGradient(
            center: const Alignment(0, -1.0),
            radius: 1.5,
            colors: isDark 
              ? [AppTheme.midnightSurface, AppTheme.midnightBase]
              : [AppTheme.daylightSurface, AppTheme.daylightBase],
          ),
        ),
        child: Column(
          children: [
            Expanded(
              child: ListView.builder(
                padding: const EdgeInsets.all(24),
                itemCount: messages.length,
                itemBuilder: (context, index) {
                  return _buildMessage(context, messages[index].text, messages[index].isMe);
                },
              ),
            ),
            if (_isTyping)
              const Padding(
                padding: EdgeInsets.symmetric(horizontal: 24.0, vertical: 8),
                child: Align(alignment: Alignment.centerLeft, child: Text('AI is typing...', style: TextStyle(fontSize: 10, color: AppTheme.royalGold))),
              ),
            _buildInputArea(context, isDark),
          ],
        ),
      ),
    );
  }

  Widget _buildMessage(BuildContext context, String text, bool isMe) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12.0),
      child: Align(
        alignment: isMe ? Alignment.centerRight : Alignment.centerLeft,
        child: GlassContainer(
          padding: const EdgeInsets.all(16),
          opacity: isMe ? 0.3 : 0.1,
          child: ConstrainedBox(
            constraints: BoxConstraints(maxWidth: MediaQuery.of(context).size.width * 0.7),
            child: Text(text),
          ),
        ),
      ),
    );
  }

  Widget _buildInputArea(BuildContext context, bool isDark) {
    return Container(
      padding: const EdgeInsets.all(24),
      decoration: BoxDecoration(
        color: isDark ? AppTheme.midnightSurface : AppTheme.daylightSurface,
        border: Border(top: BorderSide(color: Colors.white.withValues(alpha: 0.1))),
      ),
      child: Row(
        children: [
          Expanded(child: TextField(controller: _controller, onSubmitted: (_) => _sendMessage(), decoration: const InputDecoration(hintText: 'Type your question...', border: InputBorder.none))),
          IconButton(icon: const Icon(Icons.send, color: AppTheme.royalGold), onPressed: _sendMessage),
        ],
      ),
    );
  }
}
