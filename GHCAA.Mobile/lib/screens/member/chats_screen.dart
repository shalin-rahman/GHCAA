import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../features/messaging/chat_service.dart';
import '../../features/auth/auth_service.dart';
import '../../core/real_time/notification_hub_service.dart';
import 'package:intl/intl.dart';

class ChatsScreen extends ConsumerStatefulWidget {
  const ChatsScreen({super.key});

  @override
  ConsumerState<ChatsScreen> createState() => _ChatsScreenState();
}

class _ChatsScreenState extends ConsumerState<ChatsScreen> with SingleTickerProviderStateMixin {
  late TabController _tabController;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 3, vsync: this);
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AppScaffold(
      title: 'Institutional Messaging',
      breadcrumb: 'Member Portal > Social Hub',
      bottom: TabBar(
        controller: _tabController,
        dividerColor: Colors.transparent,
        indicatorColor: AppTheme.royalGold,
        labelColor: AppTheme.royalGold,
        unselectedLabelColor: Colors.white54,
        tabs: const [
          Tab(text: 'DIRECT'),
          Tab(text: 'BATCH HUB'),
          Tab(text: 'NOTICES'),
        ],
      ),
      child: TabBarView(
        controller: _tabController,
        children: [
          _buildDirectChats(),
          _buildBatchHub(),
          _buildOfficialNotices(),
        ],
      ),
    );
  }

  Widget _buildDirectChats() {
    final conversationsAsync = ref.watch(conversationsProvider);
    return AsyncValueWidget<List<dynamic>>(
      value: conversationsAsync,
      loadingMessage: 'Syncing encrypted channels...',
      onRetry: () => ref.invalidate(conversationsProvider),
      data: (conversations) {
        if (conversations.isEmpty) {
          return _buildEmptyState('No active conversations yet.', Icons.forum_outlined, () => context.push('/directory'));
        }

        return ListView.builder(
          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 20),
          itemCount: conversations.length,
          itemBuilder: (context, index) {
            final conversation = conversations[index];
            return Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: GlassContainer(
                padding: EdgeInsets.zero,
                child: ListTile(
                  contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                  onTap: () {
                    HapticFeedback.lightImpact();
                    context.push('/chat/${conversation['otherUserId']}');
                  },
                  leading: _buildAvatar(conversation['otherUserPhoto'], conversation['otherUserName']),
                  title: Text(conversation['otherUserName'] ?? 'Alumnus', style: const TextStyle(fontWeight: FontWeight.bold, color: Colors.white)),
                  subtitle: Text(conversation['lastMessage'] ?? '', maxLines: 1, overflow: TextOverflow.ellipsis, style: TextStyle(color: Colors.white.withValues(alpha: 0.6), fontSize: 12)),
                  trailing: _buildTimeAndUnread(conversation['lastMessageTime'], conversation['unreadCount']),
                ),
              ),
            );
          },
        );
      },
    );
  }

  Widget _buildBatchHub() {
    final userAsync = ref.watch(userProfileProvider);
    return userAsync.when(
      data: (user) {
        final batch = user?['passingYear']?.toString() ?? 'N/A';
        final dept = user?['department'] ?? 'General';
        
        return Column(
          children: [
            Padding(
              padding: const EdgeInsets.all(16),
              child: GlassContainer(
                child: Row(
                  children: [
                    const Icon(Icons.group_work_outlined, color: AppTheme.royalGold),
                    const SizedBox(width: 16),
                    Expanded(
                      child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                        Text('BATCH OF $batch ($dept)', style: const TextStyle(fontWeight: FontWeight.bold, color: Colors.white)),
                        const Text('Live collaborative hub for your cohort.', style: TextStyle(fontSize: 10, color: Colors.white54)),
                      ]),
                    ),
                    IconButton(icon: const Icon(Icons.refresh, color: AppTheme.royalGold, size: 20), onPressed: () {
                        ref.read(notificationHubServiceProvider).joinBatch('BATCH-$batch');
                    }),
                  ],
                ),
              ),
            ),
            Expanded(
              child: StreamBuilder<Map<String, dynamic>>(
                stream: ref.read(notificationHubServiceProvider).batchMessages,
                builder: (context, snapshot) {
                   if (snapshot.hasData) {
                     final msg = snapshot.data!;
                     return ListView(
                       padding: const EdgeInsets.all(16),
                       children: [
                         _buildLiveMessage(msg['senderName'], msg['content'], msg['timestamp']),
                       ],
                     );
                   }
                   return _buildPlaceholderFeed('Batch chatter will appear here in real-time.', Icons.connect_without_contact);
                },
              ),
            ),
          ],
        );
      },
      loading: () => const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
      error: (e, s) => Center(child: Text('Registry Sync Error: $e')),
    );
  }

  Widget _buildOfficialNotices() {
    return Column(
      children: [
        const Padding(
          padding: EdgeInsets.all(16),
          child: GlassContainer(
            child: Row(children: [
              Icon(Icons.campaign_outlined, color: Colors.redAccent),
              SizedBox(width: 16),
              Expanded(child: Text('OFFICIAL ALUMNI NOTICES', style: TextStyle(fontWeight: FontWeight.bold, letterSpacing: 1))),
            ]),
          ),
        ),
        Expanded(
          child: StreamBuilder<Map<String, dynamic>>(
            stream: ref.read(notificationHubServiceProvider).notices,
            builder: (context, snapshot) {
              if (snapshot.hasData) {
                 final notice = snapshot.data!;
                 return ListView(
                   padding: const EdgeInsets.all(16),
                   children: [
                     _buildLiveMessage('ASSOCIATION PRESS', notice['title'], notice['timestamp'], isNotice: true),
                   ],
                 );
               }
              return _buildPlaceholderFeed('Stay tuned for institutional announcements.', Icons.info_outline);
            },
          ),
        ),
      ],
    );
  }

  Widget _buildLiveMessage(String? sender, String? content, dynamic time, {bool isNotice = false}) {
    return Container(
      margin: const EdgeInsets.only(bottom: 12, left: 16, right: 16),
      child: GlassContainer(
        padding: const EdgeInsets.all(12),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
              Text(sender ?? 'System', style: TextStyle(fontSize: 10, fontWeight: FontWeight.bold, color: isNotice ? Colors.redAccent : AppTheme.royalGold)),
              Text(_formatTime(time), style: const TextStyle(fontSize: 9, color: Colors.white24)),
            ]),
            const SizedBox(height: 6),
            Text(content ?? '', style: const TextStyle(fontSize: 13, color: Colors.white70)),
          ],
        ),
      ),
    );
  }

  Widget _buildPlaceholderFeed(String message, IconData icon) {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(icon, size: 48, color: Colors.white10),
          const SizedBox(height: 16),
          Text(message, style: const TextStyle(color: Colors.white24, fontSize: 13)),
        ],
      ),
    );
  }

  Widget _buildEmptyState(String message, IconData icon, VoidCallback onAction) {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(icon, size: 64, color: Colors.white24),
          const SizedBox(height: 24),
          Text(message, style: const TextStyle(color: Colors.white54, fontSize: 16)),
          const SizedBox(height: 8),
          TextButton(
            onPressed: onAction,
            child: const Text('SEARCH ALUMNI DIRECTORY', style: TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.bold, letterSpacing: 1.5)),
          ),
        ],
      ),
    );
  }

  Widget _buildAvatar(String? url, String? name) {
    return Container(
      width: 48,
      height: 48,
      decoration: BoxDecoration(
        color: Colors.black26, shape: BoxShape.circle,
        border: Border.all(color: AppTheme.royalGold.withValues(alpha: 0.3)),
        image: url != null ? DecorationImage(image: NetworkImage(url), fit: BoxFit.cover) : null,
      ),
      child: url == null 
        ? Center(child: Text(name != null && name.isNotEmpty ? name[0] : '?', style: const TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.bold)))
        : null,
    );
  }

  Widget _buildTimeAndUnread(dynamic time, dynamic unreadCount) {
    return Column(
      mainAxisAlignment: MainAxisAlignment.center,
      crossAxisAlignment: CrossAxisAlignment.end,
      children: [
        Text(_formatTime(time), style: TextStyle(fontSize: 10, color: AppTheme.royalGold.withValues(alpha: 0.8), fontWeight: FontWeight.bold)),
        if (unreadCount != null && unreadCount > 0)
          Container(
            margin: const EdgeInsets.only(top: 4), padding: const EdgeInsets.all(6),
            decoration: const BoxDecoration(color: AppTheme.royalGold, shape: BoxShape.circle),
            child: Text(unreadCount.toString(), style: const TextStyle(color: Colors.black, fontSize: 8, fontWeight: FontWeight.bold)),
          ),
      ],
    );
  }

  String _formatTime(dynamic time) {
    if (time == null) return '';
    try {
      final dt = DateTime.parse(time.toString());
      final now = DateTime.now();
      if (dt.year == now.year && dt.month == now.month && dt.day == now.day) {
        return DateFormat.Hm().format(dt);
      }
      return DateFormat('dd-MM-yyyy').format(dt);
    } catch (e) { return ''; }
  }
}
