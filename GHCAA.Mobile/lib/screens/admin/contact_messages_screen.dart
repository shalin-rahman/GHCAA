import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../features/admin/admin_service.dart';
import 'package:flutter/services.dart';

final contactMessagesProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async => ref.read(adminServiceProvider).getContactMessages());

class ContactMessagesScreen extends ConsumerWidget {
  const ContactMessagesScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final messagesAsync = ref.watch(contactMessagesProvider);

    return AppScaffold(
      isAdmin: true,
      title: 'Support Registry',
      breadcrumb: 'Executive Console > Inbound Communications',
      child: AsyncValueWidget<List<dynamic>>(
        value: messagesAsync,
        loadingMessage: 'Syncing inbox history...',
        onRetry: () => ref.invalidate(contactMessagesProvider),
        data: (messages) => RefreshIndicator(
          color: AppTheme.royalGold,
          onRefresh: () async {
            HapticFeedback.mediumImpact();
            ref.invalidate(contactMessagesProvider);
          },
          child: messages.isEmpty 
            ? const Center(child: Text('Support inbox is empty.', style: TextStyle(color: AppTheme.textSecondaryDark)))
            : ListView.builder(
                padding: const EdgeInsets.all(20.0),
                itemCount: messages.length,
                itemBuilder: (context, index) {
                  final msg = messages[index];
                  final isRead = msg['isRead'] ?? false;
                  
                  return Padding(
                    padding: const EdgeInsets.only(bottom: 12.0),
                    child: GlassContainer(
                      opacity: isRead ? 0.05 : 0.15,
                      child: ListTile(
                        leading: CircleAvatar(
                          backgroundColor: isRead ? AppTheme.textSecondaryDark.withValues(alpha: 0.1) : AppTheme.royalGold.withValues(alpha: 0.1),
                          child: Icon(isRead ? Icons.mark_email_read_outlined : Icons.mark_email_unread_outlined, color: isRead ? AppTheme.textSecondaryDark : AppTheme.royalGold, size: 20),
                        ),
                        title: Text(msg['fullName'] ?? 'Individual Message', style: TextStyle(fontWeight: isRead ? FontWeight.normal : FontWeight.w900, color: Colors.white)),
                        subtitle: Text(msg['subject'] ?? 'No Subject Provided', maxLines: 1, style: const TextStyle(fontSize: 12, color: AppTheme.textSecondaryDark)),
                        trailing: Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            if (!isRead)
                              IconButton(
                                icon: const Icon(Icons.check_circle_outline, color: AppTheme.royalGold, size: 20),
                                onPressed: () async {
                                  HapticFeedback.lightImpact();
                                  await ref.read(adminServiceProvider).markMessageAsRead(msg['id']);
                                  ref.invalidate(contactMessagesProvider);
                                },
                              ),
                            const Icon(Icons.arrow_forward_ios_rounded, size: 12, color: AppTheme.textSecondaryDark),
                          ],
                        ),
                        onTap: () {
                          // Preview message content
                        },
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
