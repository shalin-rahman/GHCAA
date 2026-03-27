import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
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
      title: 'Alerts Hub',
      breadcrumb: 'Executive Hub > System Notifications',
      child: AsyncValueWidget<List<dynamic>>(
        value: notificationsAsync,
        loadingMessage: 'Fetching system alerts...',
        onRetry: () => ref.invalidate(notificationsListProvider),
        data: (alerts) => alerts.isEmpty 
          ? const Center(child: Text('No active notifications found.', style: TextStyle(color: AppTheme.textSecondaryDark)))
          : RefreshIndicator(
              color: AppTheme.royalGold,
              onRefresh: () async {
                HapticFeedback.mediumImpact();
                ref.invalidate(notificationsListProvider);
              },
              child: ListView.builder(
                padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
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
                                      style: TextStyle(fontSize: 12, color: isRead ? AppTheme.textSecondaryDark.withValues(alpha: 0.7) : AppTheme.textSecondaryDark, height: 1.4)
                                    ),
                                  ],
                                ),
                              ),
                              const SizedBox(width: 8),
                              Text(
                                (alert['sentAt'] ?? '').toString().split(' ')[0], 
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
      ),
    );
  }
}
