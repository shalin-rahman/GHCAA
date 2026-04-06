import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:image_picker/image_picker.dart';
import 'package:dio/dio.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/api/api_client.dart';
import '../../features/auth/auth_service.dart';
import '../../features/events/events_service.dart';
import '../../core/config/app_config.dart';

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
    final isAdmin = roleAsync.value?.isStaffAdminRole ?? false;

    return AppScaffold(
      title: 'Event Details',
      breadcrumb: 'Timeline > Event Details',
      actions: isAdmin ? [
        IconButton(
          icon: const Icon(Icons.image_outlined, color: Colors.white54, size: 20),
          tooltip: 'Upload Event Logo',
          onPressed: () => _uploadEventLogo(context, ref),
        ),
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
      floatingActionButton: detailsAsync.maybeWhen(
        data: (event) {
          if (event == null) return null;
          final isOpen = _isRegistrationOpen(event);
          if (!isOpen && !isAdmin) return null; // Admins might still see something else, but generally hide if close
          return FloatingActionButton.extended(
            onPressed: () => _showRegisterDialog(context, ref, event),
            backgroundColor: AppTheme.royalGold,
            icon: const Icon(Icons.how_to_reg, color: Colors.black, size: 20),
            label: const Text('REGISTER NOW', style: TextStyle(color: Colors.black, fontWeight: FontWeight.w900, fontSize: 10, letterSpacing: 1.5)),
          );
        },
        orElse: () => null,
      ),
      child: detailsAsync.when(
        data: (event) {
          if (event == null) return const Center(child: Text('Event data corrupted.', style: TextStyle(color: Colors.red)));

          return SingleChildScrollView(
            padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 20),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                // Event Logo / Banner
                if (event['imageUrl'] != null && event['imageUrl'].toString().isNotEmpty)
                  ClipRRect(
                    borderRadius: BorderRadius.circular(16),
                    child: Image.network(
                      event['imageUrl'].toString().startsWith('http')
                          ? event['imageUrl']
                          : '${AppConfig.apiBaseUrl.replaceFirst('/api', '')}/${event['imageUrl'].toString().replaceFirst(RegExp(r'^/'), '')}',
                      height: 180,
                      width: double.infinity,
                      fit: BoxFit.cover,
                      errorBuilder: (_, __, ___) => const SizedBox(),
                    ),
                  ),
                if (event['imageUrl'] != null && event['imageUrl'].toString().isNotEmpty)
                  const SizedBox(height: 16),

                GlassContainer(
                   child: Column(
                     crossAxisAlignment: CrossAxisAlignment.start,
                     children: [
                       Text((event['title'] ?? 'Global Event').toUpperCase(), style: const TextStyle(fontWeight: FontWeight.w900, color: Colors.white, fontSize: 18, letterSpacing: 1)),
                       const SizedBox(height: 8),
                       Text('VENUE: ${event['location'] ?? event['venue'] ?? 'TBA'}', style: const TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.bold, letterSpacing: 1.5)),
                       const Divider(color: Colors.white12, height: 32),
                       Text(event['description'] ?? 'No operational details found.', style: const TextStyle(color: Colors.white, fontSize: 14, height: 1.5)),
                       const SizedBox(height: 16),
                       _buildStatRow('Start Date', event['startDate']?.toString().split('T')[0] ?? event['eventDate']?.toString().split('T')[0] ?? 'N/A'),
                       _buildStatRow('Participants', '${event['participantCount'] ?? 0} registered'),
                       _buildStatRow('Entry Fee', event['requiresPayment'] == false ? 'FREE' : '৳${event['registrationFee']?.toString() ?? '0.00'}'),
                       _buildStatRow('Non-Members', event['allowNonMembers'] == true ? 'Allowed' : 'Members Only'),
                       const SizedBox(height: 24),
                       const Text('PARTICIPATION PRESENCE', style: TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.w900, letterSpacing: 1.5)),
                       const SizedBox(height: 12),
                       _buildAttendeeList(event['registrations'] as List<dynamic>? ?? []),
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

  Widget _buildAttendeeList(List<dynamic> list) {
    if (list.isEmpty) return const Text('Establishing initial roster...', style: TextStyle(color: Colors.white38, fontSize: 11));
    
    return SizedBox(
      height: 50,
      child: ListView.builder(
        scrollDirection: Axis.horizontal,
        itemCount: list.length,
        itemBuilder: (ctx, idx) {
          final reg = list[idx];
          final m = reg['member'];
          if (m == null) return const SizedBox();
          
          String? photo;
          if (m['photoPath'] != null) {
             final base = AppConfig.apiBaseUrl.endsWith('/') ? AppConfig.apiBaseUrl.substring(0, AppConfig.apiBaseUrl.length - 1) : AppConfig.apiBaseUrl;
             photo = m['photoPath'].toString().startsWith('http') ? m['photoPath'] : '$base/${m['photoPath'].toString().startsWith('/') ? m['photoPath'].toString().substring(1) : m['photoPath']}';
          }

          return Padding(
            padding: const EdgeInsets.only(right: 12),
            child: Tooltip(
              message: m['fullName'] ?? 'Alumnus',
              child: Container(
                decoration: BoxDecoration(shape: BoxShape.circle, border: Border.all(color: AppTheme.royalGold.withValues(alpha: 0.3), width: 1)),
                child: CircleAvatar(
                  radius: 20,
                  backgroundColor: Colors.black26,
                  backgroundImage: photo != null ? NetworkImage(photo) : null,
                  child: photo == null ? Text(m['fullName']?[0] ?? '?', style: const TextStyle(fontSize: 10, color: AppTheme.royalGold)) : null,
                ),
              ),
            ),
          );
        },
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

  bool _isRegistrationOpen(Map<String, dynamic> ev) {
    if (ev['isActive'] == false) return false;
    final now = DateTime.now();
    if (ev['registrationStartDate'] != null) {
      final sd = DateTime.tryParse(ev['registrationStartDate'].toString());
      if (sd != null && sd.isAfter(now)) return false;
    }
    if (ev['registrationEndDate'] != null) {
      final ed = DateTime.tryParse(ev['registrationEndDate'].toString());
      if (ed != null && ed.isBefore(now)) return false;
    }
    return true;
  }

  void _showRegisterDialog(BuildContext context, WidgetRef ref, Map<String, dynamic> event) {
    final requiresPayment = event['requiresPayment'] == true;
    final amountCtrl = TextEditingController();
    bool isSaving = false;

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
              Text('JOIN ${event['title']?.toUpperCase()}', style: const TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.w900, letterSpacing: 2, fontSize: 12)),
              const SizedBox(height: 20),
              if (requiresPayment) ...[
                const Text('Custom Contribution (Minimum: ৳10 for free events if opted, or fixed fee)', style: TextStyle(color: AppTheme.textSecondaryDark, fontSize: 10)),
                const SizedBox(height: 8),
                TextField(
                  controller: amountCtrl,
                  keyboardType: TextInputType.number,
                  style: const TextStyle(color: Colors.white),
                  decoration: const InputDecoration(labelText: 'Amount (৳)', labelStyle: TextStyle(color: AppTheme.royalGold), enabledBorder: UnderlineInputBorder(borderSide: BorderSide(color: Colors.white24))),
                ),
                const SizedBox(height: 16),
              ] else ...[
                 const Text('This is a free event. No payment is required.', style: TextStyle(color: AppTheme.textSecondaryDark, fontSize: 12)),
                 const SizedBox(height: 16),
              ],
              const SizedBox(height: 28),
              ElevatedButton(
                style: ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold, shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12))),
                onPressed: isSaving ? null : () async {
                  
                  // Mobile Validation Replica
                  if (requiresPayment) {
                    final val = double.tryParse(amountCtrl.text) ?? 0;
                    final fee = event['registrationFee'] ?? 0;
                    if (fee == 0 && val < 10 && val > 0) {
                      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Contribution must be at least 10 BDT if provided.'), backgroundColor: Colors.orangeAccent));
                      return;
                    }
                  }

                  setStateModal(() => isSaving = true);
                  try {
                    await ref.read(eventsServiceProvider).registerForEvent(
                      eventId,
                      amount: requiresPayment ? double.tryParse(amountCtrl.text) : null,
                      paymentRef: 'APP-REG-${DateTime.now().millisecondsSinceEpoch.toString().substring(5)}'
                    );
                    if (ctx.mounted) Navigator.pop(ctx);
                    if (context.mounted) {
                      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Registration successful.')));
                      context.pop(); // Go back from details
                    }
                  } catch (e) {
                    if (context.mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Registration block: $e'), backgroundColor: Colors.redAccent));
                  } finally {
                    if (ctx.mounted) setStateModal(() => isSaving = false);
                  }
                },
                child: isSaving ? const SizedBox(height: 20, width: 20, child: CircularProgressIndicator(color: Colors.black, strokeWidth: 2)) : const Text('CONFIRM PARTICIPATION', style: TextStyle(color: Colors.black, fontWeight: FontWeight.w900, fontSize: 12, letterSpacing: 1)),
              ),
            ],
          ),
        ),
      ),
    );
  }

  void _uploadEventLogo(BuildContext context, WidgetRef ref) async {
    final picker = ImagePicker();
    final file = await picker.pickImage(source: ImageSource.gallery, imageQuality: 85);
    if (file == null) return;

    try {
      final dio = ref.read(dioProvider);
      final formData = FormData.fromMap({
        'logo': await MultipartFile.fromFile(file.path, filename: 'event_logo.jpg'),
      });
      final response = await dio.post('/events/admin/$eventId/logo', data: formData);
      if (response.statusCode == 200) {
        ref.invalidate(eventDetailsProvider(eventId));
        if (context.mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Event logo updated.')));
      }
    } catch (e) {
      if (context.mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Upload failed: $e'), backgroundColor: Colors.redAccent));
    }
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
