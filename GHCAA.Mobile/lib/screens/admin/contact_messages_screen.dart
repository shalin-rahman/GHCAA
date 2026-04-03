import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../features/admin/admin_service.dart';
import 'package:flutter/services.dart';

final contactMessagesProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async => ref.read(adminServiceProvider).getContactMessages());
final contactMessagesSearchQueryProvider = StateProvider.autoDispose<String>((ref) => "");

class ContactMessagesScreen extends ConsumerStatefulWidget {
  const ContactMessagesScreen({super.key});

  @override
  ConsumerState<ContactMessagesScreen> createState() => _ContactMessagesScreenState();
}

class _ContactMessagesScreenState extends ConsumerState<ContactMessagesScreen> {
  final TextEditingController _searchController = TextEditingController();

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final messagesAsync = ref.watch(contactMessagesProvider);
    final searchQuery = ref.watch(contactMessagesSearchQueryProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      isAdmin: true,
      title: 'Portal Enquiries',
      breadcrumb: 'ADMIN > ENQUIRIES',
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Search enquiries...',
                prefixIcon: const Icon(Icons.search, size: 20, color: AppTheme.royalGold),
                suffixIcon: _searchController.text.isNotEmpty 
                  ? IconButton(
                      icon: const Icon(Icons.close_rounded, size: 20, color: Colors.white54),
                      onPressed: () {
                        _searchController.clear();
                        ref.read(contactMessagesSearchQueryProvider.notifier).state = "";
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
              onChanged: (v) => ref.read(contactMessagesSearchQueryProvider.notifier).state = v.toLowerCase(),
            ),
          ),
          Expanded(
            child: AsyncValueWidget<List<dynamic>>(
              value: messagesAsync,
              loadingMessage: 'Loading enquiries...',
              onRetry: () => ref.invalidate(contactMessagesProvider),
              data: (messages) {
                final filtered = messages.where((msg) {
                  final name = (msg['fullName'] ?? '').toString().toLowerCase();
                  final subject = (msg['subject'] ?? '').toString().toLowerCase();
                  return name.contains(searchQuery) || subject.contains(searchQuery);
                }).toList();

                return RefreshIndicator(
                  color: AppTheme.royalGold,
                  onRefresh: () async {
                    HapticFeedback.mediumImpact();
                    ref.invalidate(contactMessagesProvider);
                  },
                  child: Column(
                    children: [
                      if (filtered.isNotEmpty)
                        Padding(
                          padding: const EdgeInsets.only(left: 24, bottom: 8),
                          child: Align(
                            alignment: Alignment.centerLeft,
                            child: Text(
                              'Showing ${filtered.length} of ${messages.length} system enquiries',
                              style: TextStyle(fontSize: 10, color: AppTheme.royalGold.withValues(alpha: 0.7), fontWeight: FontWeight.bold),
                            ),
                          ),
                        ),
                      Expanded(
                        child: filtered.isEmpty 
                          ? Center(child: Text(searchQuery.isEmpty ? 'No enquiries found.' : 'No messages match your search.', style: const TextStyle(color: Colors.white54, fontSize: 11, fontStyle: FontStyle.italic)))
                          : ListView.builder(
                              padding: const EdgeInsets.all(20.0),
                              itemCount: filtered.length,
                              itemBuilder: (context, index) {
                                final msg = filtered[index];
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
