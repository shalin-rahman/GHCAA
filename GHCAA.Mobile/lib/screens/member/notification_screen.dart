import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../features/notifications/notification_service.dart';

final notificationsListProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(notificationServiceProvider).getMyNotifications();
});

final notificationSearchQueryProvider = StateProvider.autoDispose<String>((ref) => "");

class NotificationScreen extends ConsumerStatefulWidget {
  const NotificationScreen({super.key});

  @override
  ConsumerState<NotificationScreen> createState() => _NotificationScreenState();
}

class _NotificationScreenState extends ConsumerState<NotificationScreen> {
  final TextEditingController _searchController = TextEditingController();

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final notificationsAsync = ref.watch(notificationsListProvider);
    final searchQuery = ref.watch(notificationSearchQueryProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      title: 'Alerts Hub',
      breadcrumb: 'PORTAL > NOTIFICATIONS',
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Search notifications...',
                prefixIcon: const Icon(Icons.search, size: 20, color: AppTheme.royalGold),
                suffixIcon: _searchController.text.isNotEmpty 
                  ? IconButton(
                      icon: const Icon(Icons.close_rounded, size: 20, color: Colors.white54),
                      onPressed: () {
                        _searchController.clear();
                        ref.read(notificationSearchQueryProvider.notifier).state = "";
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
              onChanged: (v) => ref.read(notificationSearchQueryProvider.notifier).state = v.toLowerCase(),
            ),
          ),
          Expanded(
            child: AsyncValueWidget<List<dynamic>>(
              value: notificationsAsync,
              loadingMessage: 'Fetching system alerts...',
              onRetry: () => ref.invalidate(notificationsListProvider),
              data: (alerts) {
                final filtered = alerts.where((a) {
                  final title = a['title']?.toString().toLowerCase() ?? '';
                  final message = a['message']?.toString().toLowerCase() ?? '';
                  return title.contains(searchQuery) || message.contains(searchQuery);
                }).toList();

                return RefreshIndicator(
                  color: AppTheme.royalGold,
                  onRefresh: () async {
                    HapticFeedback.mediumImpact();
                    ref.invalidate(notificationsListProvider);
                  },
                  child: Column(
                    children: [
                      if (filtered.isNotEmpty)
                        Padding(
                          padding: const EdgeInsets.only(left: 24, bottom: 8),
                          child: Align(
                            alignment: Alignment.centerLeft,
                            child: Text(
                              'Showing ${filtered.length} of ${alerts.length} alerts',
                              style: TextStyle(fontSize: 10, color: AppTheme.royalGold.withValues(alpha: 0.7), fontWeight: FontWeight.bold),
                            ),
                          ),
                        ),
                      Expanded(
                        child: filtered.isEmpty
                          ? EmptyStateWidget(searchQuery.isEmpty ? 'No active notifications found.' : 'No alerts match your search.', icon: Icons.notifications_none_outlined)
                          : ListView.builder(
                              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                              itemCount: filtered.length,
                              itemBuilder: (context, index) {
                                final alert = filtered[index];
                                final isRead = alert['isRead'] ?? false;

                                return Padding(
                                  padding: const EdgeInsets.only(bottom: 12.0),
                                  child: Dismissible(
                                    key: Key(alert['id'].toString()),
                                    direction: DismissDirection.endToStart,
                                    background: Container(
                                      padding: const EdgeInsets.only(right: 24),
                                      decoration: BoxDecoration(color: Colors.redAccent.withValues(alpha: 0.1), borderRadius: BorderRadius.circular(16)),
                                      alignment: Alignment.centerRight,
                                      child: const Icon(Icons.delete_outline, color: Colors.redAccent),
                                    ),
                                    onDismissed: (direction) {
                                      HapticFeedback.heavyImpact();
                                      ref.read(notificationServiceProvider).markAsRead(alert['id']);
                                      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Notification archived.')));
                                    },
                                    child: GlassContainer(
                                      padding: const EdgeInsets.all(16),
                                      child: InkWell(
                                        onTap: () {
                                          HapticFeedback.lightImpact();
                                          if (!isRead) {
                                            ref.read(notificationServiceProvider).markAsRead(alert['id']);
                                            ref.invalidate(notificationsListProvider);
                                          }
                                        },
                                        child: Row(
                                          children: [
                                            Container(
                                              padding: const EdgeInsets.all(10),
                                              decoration: BoxDecoration(
                                                color: isRead ? Colors.transparent : AppTheme.royalGold.withValues(alpha: 0.1),
                                                shape: BoxShape.circle,
                                                border: isRead ? Border.all(color: AppTheme.textSecondaryDark.withValues(alpha: 0.2)) : null
                                              ),
                                              child: Icon(
                                                isRead ? Icons.notifications_none_outlined : Icons.notifications_active_outlined,
                                                color: isRead ? AppTheme.textSecondaryDark : AppTheme.royalGold,
                                                size: 18,
                                              ),
                                            ),
                                            const SizedBox(width: 16),
                                            Expanded(
                                              child: Column(
                                                crossAxisAlignment: CrossAxisAlignment.start,
                                                children: [
                                                  Text(
                                                    alert['title'] ?? 'System Update',
                                                    style: TextStyle(fontSize: 14, fontWeight: isRead ? FontWeight.w500 : FontWeight.w900, color: isRead ? AppTheme.textSecondaryDark : Colors.white)
                                                  ),
                                                  const SizedBox(height: 4),
                                                  Text(
                                                    alert['message'] ?? '',
                                                    style: TextStyle(fontSize: 12, color: isRead ? AppTheme.textSecondaryDark.withValues(alpha: 0.7) : AppTheme.textSecondaryDark, height: 1.4),
                                                    maxLines: 2,
                                                    overflow: TextOverflow.ellipsis,
                                                  ),
                                                ],
                                              ),
                                            ),
                                            const SizedBox(width: 8),
                                            Text(
                                              (alert['sentAt'] ?? '').toString().split('T')[0],
                                              style: const TextStyle(fontSize: 9, color: AppTheme.textSecondaryDark, fontWeight: FontWeight.bold)
                                            ),
                                          ],
                                        ),
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
}
