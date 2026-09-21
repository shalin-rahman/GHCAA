import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/utils/app_utils.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../features/notifications/notification_service.dart';

final communicationsProvider = FutureProvider.autoDispose<List<CommunicationLog>>(
  (ref) => ref.read(notificationServiceProvider).getMyCommunications(),
);

class CommunicationsScreen extends ConsumerWidget {
  const CommunicationsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return AppScaffold(
      title: 'My communications',
      breadcrumb: 'PORTAL > COMMUNICATIONS',
      child: AsyncValueWidget<List<CommunicationLog>>(
        value: ref.watch(communicationsProvider),
        loadingMessage: 'Loading communication history...',
        onRetry: () => ref.invalidate(communicationsProvider),
        data: (logs) => logs.isEmpty
            ? const EmptyStateWidget('No communications have been recorded.', icon: Icons.mail_outline)
            : RefreshIndicator(
                onRefresh: () async => ref.invalidate(communicationsProvider),
                child: ListView.builder(
                  padding: const EdgeInsets.all(20),
                  itemCount: logs.length,
                  itemBuilder: (context, index) {
                    final log = logs[index];
                    return Card(
                      child: ListTile(
                        title: Text(log.subject.isEmpty ? log.channel : log.subject),
                        subtitle: Text(
                          '${log.channel} - ${log.deliveryScope} - ${log.status} - '
                          '${AppUtils.formatDate(log.sentDate, includeTime: true)}\n${log.body}',
                        ),
                        isThreeLine: true,
                      ),
                    );
                  },
                ),
              ),
      ),
    );
  }
}
