import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/api/api_client.dart';
import '../../features/auth/auth_service.dart';

final eventDetailsProvider = FutureProvider.family<Map<String, dynamic>?, int>((ref, eventId) async {
  try {
    final dio = ref.read(dioProvider);
    final response = await dio.get('/events/$eventId');
    return response.data as Map<String, dynamic>;
  } catch (e) {
    return null;
  }
});

class EventDetailsScreen extends ConsumerWidget {
  final int eventId;

  const EventDetailsScreen({super.key, required this.eventId});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final detailsAsync = ref.watch(eventDetailsProvider(eventId));
    final roleAsync = ref.watch(roleProvider);
    final isAdmin = roleAsync.value == 'Admin' || roleAsync.value == 'SuperAdmin';

    return AppScaffold(
      title: 'Timeline Dossier',
      breadcrumb: 'Timeline > Event Details',
      actions: isAdmin ? [
        IconButton(
          icon: const Icon(Icons.edit, color: AppTheme.royalGold, size: 20),
          onPressed: () => detailsAsync.whenData((event) {
            if (event != null) _showEditDialog(context, ref, event);
          }),
        ),
        IconButton(
          icon: const Icon(Icons.delete_outline, color: Colors.redAccent, size: 20),
          onPressed: () => _confirmDelete(context, ref),
        ),
      ] : null,
      child: detailsAsync.when(
        data: (event) {
          if (event == null) return const Center(child: Text('Event data corrupted.', style: TextStyle(color: Colors.red)));

          return SingleChildScrollView(
            padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 20),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                GlassContainer(
                   child: Column(
                     crossAxisAlignment: CrossAxisAlignment.start,
                     children: [
                       Text((event['title'] ?? 'Global Event').toUpperCase(), style: const TextStyle(fontWeight: FontWeight.w900, color: Colors.white, fontSize: 18, letterSpacing: 1)),
                       const SizedBox(height: 8),
                       Text('VENUE: ${event['venue'] ?? 'TBA'}', style: const TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.bold, letterSpacing: 1.5)),
                       const Divider(color: Colors.white12, height: 32),
                       Text(event['description'] ?? 'No operational details found.', style: const TextStyle(color: Colors.white, fontSize: 14, height: 1.5)),
                       const SizedBox(height: 16),
                       _buildStatRow('Date', event['eventDate']?.toString().split('T')[0] ?? 'N/A'),
                       _buildStatRow('Participant Limit', event['participantLimit']?.toString() ?? 'Open'),
                       _buildStatRow('Ticket Cost', '৳${event['ticketPrice']?.toString() ?? '0.00'}'),
                     ],
                   )
                ),
              ],
            ),
          );
        },
        loading: () => const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
        error: (e, s) => Center(child: Text('Sync Error: $e', style: const TextStyle(color: Colors.red))),
      ),
    );
  }

  Widget _buildStatRow(String label, String val) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 8.0),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
           Text(label.toUpperCase(), style: const TextStyle(color: AppTheme.textSecondaryDark, fontSize: 9, fontWeight: FontWeight.w900, letterSpacing: 1)),
           Text(val, style: const TextStyle(color: Colors.white, fontSize: 13, fontWeight: FontWeight.bold)),
        ],
      )
    );
  }

  void _showEditDialog(BuildContext context, WidgetRef ref, Map<String, dynamic> event) {
    final titleCtrl = TextEditingController(text: event['title']);
    final descCtrl = TextEditingController(text: event['description']);
    bool saving = false;

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: AppTheme.midnightSurface,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(24))),
      builder: (ctx) => StatefulBuilder(
        builder: (ctx, setStateModal) => Padding(
          padding: EdgeInsets.only(left: 24, right: 24, top: 24, bottom: MediaQuery.of(ctx).viewInsets.bottom + 32),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const Text('EDIT EVENT', style: TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.w900, letterSpacing: 2, fontSize: 12)),
              const SizedBox(height: 20),
              TextField(
                controller: titleCtrl,
                style: const TextStyle(color: Colors.white),
                decoration: const InputDecoration(labelText: 'Title', labelStyle: TextStyle(color: AppTheme.royalGold), enabledBorder: UnderlineInputBorder(borderSide: BorderSide(color: Colors.white24))),
              ),
              const SizedBox(height: 16),
              TextField(
                controller: descCtrl,
                style: const TextStyle(color: Colors.white),
                maxLines: 3,
                decoration: const InputDecoration(labelText: 'Description', labelStyle: TextStyle(color: AppTheme.royalGold), enabledBorder: UnderlineInputBorder(borderSide: BorderSide(color: Colors.white24))),
              ),
              const SizedBox(height: 28),
              ElevatedButton(
                style: ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold, shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12))),
                onPressed: saving ? null : () async {
                  setStateModal(() => saving = true);
                  try {
                    final dio = ref.read(dioProvider);
                    await dio.put('/events/$eventId', data: {'title': titleCtrl.text, 'description': descCtrl.text});
                    ref.invalidate(eventDetailsProvider(eventId));
                    if (ctx.mounted) Navigator.pop(ctx);
                    if (context.mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Event updated successfully.')));
                  } catch (e) {
                    if (context.mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Update failed: $e'), backgroundColor: Colors.redAccent));
                  } finally {
                    if (ctx.mounted) setStateModal(() => saving = false);
                  }
                },
                child: saving ? const SizedBox(height: 20, width: 20, child: CircularProgressIndicator(color: Colors.black, strokeWidth: 2)) : const Text('SAVE CHANGES', style: TextStyle(color: Colors.black, fontWeight: FontWeight.w900, fontSize: 12, letterSpacing: 1)),
              ),
            ],
          ),
        ),
      ),
    );
  }

  void _confirmDelete(BuildContext context, WidgetRef ref) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: AppTheme.midnightSurface,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
        title: const Text('Delete Event', style: TextStyle(color: Colors.white, fontWeight: FontWeight.w900)),
        content: const Text('This action is permanent and cannot be undone. Remove this event from the timeline?', style: TextStyle(color: AppTheme.textSecondaryDark, height: 1.5)),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('CANCEL', style: TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.bold))),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: Colors.redAccent, shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10))),
            onPressed: () async {
              Navigator.pop(ctx);
              try {
                final dio = ref.read(dioProvider);
                await dio.delete('/events/$eventId');
                if (context.mounted) {
                  ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Event removed from timeline.')));
                  context.pop();
                }
              } catch (e) {
                if (context.mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Delete failed: $e'), backgroundColor: Colors.redAccent));
              }
            },
            child: const Text('DELETE', style: TextStyle(color: Colors.white, fontWeight: FontWeight.w900, letterSpacing: 1)),
          ),
        ],
      ),
    );
  }
}
