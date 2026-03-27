import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../features/notifications/notification_service.dart';

final notificationsListProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(notificationServiceProvider).getMyNotifications();
});

class NotificationScreen extends ConsumerWidget {
  const NotificationScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final notificationsAsync = ref.watch(notificationsListProvider);

    return AppScaffold(
      title: 'Alerts & Messages',
      child: AsyncValueWidget<List<dynamic>>(
        value: notificationsAsync,
        loadingMessage: 'Fetching system alerts...',
        onRetry: () => ref.invalidate(notificationsListProvider),
        data: (alerts) => alerts.isEmpty 
          ? const Center(child: Text('No new notifications.', style: TextStyle(color: AppTheme.textSecondaryDark)))
          : RefreshIndicator(
              color: AppTheme.royalGold,
              onRefresh: () async => ref.invalidate(notificationsListProvider),
              child: ListView.builder(
                padding: const EdgeInsets.all(24),
                itemCount: alerts.length,
                itemBuilder: (context, index) {
                  final alert = alerts[index];
                  final isRead = alert['isRead'] ?? false;
                  
                  return Padding(
                    padding: const EdgeInsets.only(bottom: 12.0),
                    child: Dismissible(
                      key: Key(alert['id'].toString()),
                      direction: DismissDirection.endToStart,
                      background: Container(
                        padding: const EdgeInsets.only(right: 20),
                        decoration: BoxDecoration(color: Colors.redAccent.withValues(alpha: 0.2), borderRadius: BorderRadius.circular(16)),
                        alignment: Alignment.centerRight,
                        child: const Icon(Icons.delete_outline, color: Colors.redAccent),
                      ),
                      onDismissed: (direction) {
                        // Optimistic UI could be handled here
                        ref.read(notificationServiceProvider).markAsRead(alert['id']);
                        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Notification dismissed')));
                      },
                      child: GlassContainer(
                        padding: EdgeInsets.zero,
                        opacity: isRead ? 0.05 : 0.15,
                        child: ListTile(
                          leading: Container(
                            padding: const EdgeInsets.all(8),
                            decoration: BoxDecoration(color: isRead ? Colors.transparent : AppTheme.royalGold.withValues(alpha: 0.1), shape: BoxShape.circle),
                            child: Icon(
                              isRead ? Icons.notifications_none : Icons.notifications_active,
                              color: isRead ? AppTheme.textSecondaryDark : AppTheme.royalGold,
                              size: 18,
                            ),
                          ),
                          title: Text(alert['title'] ?? 'System Update', style: TextStyle(fontSize: 14, fontWeight: isRead ? FontWeight.normal : FontWeight.w900, color: Colors.white)),
                          subtitle: Text(alert['message'] ?? '', style: const TextStyle(fontSize: 12, color: AppTheme.textSecondaryDark)),
                          trailing: Text(alert['sentAt'] ?? '', style: TextStyle(fontSize: 10, color: AppTheme.textSecondaryDark.withValues(alpha: 0.5))),
                          onTap: () {
                            if (!isRead) {
                              ref.read(notificationServiceProvider).markAsRead(alert['id']);
                              ref.invalidate(notificationsListProvider);
                            }
                          },
                        ),
                      ),
                    ),
                  );
                },
              ),
            ),
      ),
    );
  }
}
