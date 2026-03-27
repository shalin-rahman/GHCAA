import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/constants/app_constants.dart';

class ContactMessagesScreen extends ConsumerWidget {
  const ContactMessagesScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return AppScaffold(
      isAdmin: true,
      title: 'Support Inbox',
      child: ListView.builder(
        padding: const EdgeInsets.all(AppConstants.paddingLarge),
        itemCount: 8,
        itemBuilder: (context, index) {
          final isResolved = index % 3 == 0;
          return Padding(
            padding: const EdgeInsets.only(bottom: 12.0),
            child: GlassContainer(
              opacity: isResolved ? 0.05 : 0.15,
              child: ListTile(
                leading: CircleAvatar(
                  backgroundColor: isResolved ? Colors.grey : AppTheme.royalGold,
                  child: const Icon(Icons.person, color: Colors.white, size: 20),
                ),
                title: Text('Message #${100 - index}', style: const TextStyle(fontWeight: FontWeight.bold)),
                subtitle: const Text('Problem with registration payment', maxLines: 1),
                trailing: Chip(
                  label: Text(isResolved ? 'Resolved' : 'Pending', style: const TextStyle(fontSize: 10, color: Colors.white)),
                  backgroundColor: isResolved ? Colors.green.withValues(alpha: 0.5) : Colors.orange.withValues(alpha: 0.5),
                  padding: EdgeInsets.zero,
                ),
                onTap: () {},
              ),
            ),
          );
        },
      ),
    );
  }
}
