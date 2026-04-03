import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../features/events/events_service.dart';
import '../../features/auth/auth_service.dart';
import '../../features/financials/gateway_service.dart';
import '../financials/payment_web_page.dart';
import '../../core/config/app_config.dart';

final eventSearchQueryProvider = StateProvider.autoDispose<String>((ref) => "");

final eventsListProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(eventsServiceProvider).getUpcomingEvents();
});

class EventsScreen extends ConsumerStatefulWidget {
  const EventsScreen({super.key});

  @override
  ConsumerState<EventsScreen> createState() => _EventsScreenState();
}

class _EventsScreenState extends ConsumerState<EventsScreen> {
  final TextEditingController _searchController = TextEditingController();

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    Future<void> handleEventPayment(double amount, String eventTitle) async {
      final gateway = ref.read(gatewayServiceProvider);
      final res = await gateway.initiate(
          amount, PaymentGateway.sslCommerz, 'EVT-REG-$eventTitle');
      if (res.success && res.gatewayUrl != null && context.mounted) {
        Navigator.of(context).push(MaterialPageRoute(
            builder: (_) => PaymentWebPage(url: res.gatewayUrl!)));
      }
    }

    final eventsAsync = ref.watch(eventsListProvider);
    final searchQuery = ref.watch(eventSearchQueryProvider);
    final roleAsync = ref.watch(FutureProvider((ref) => ref.read(authServiceProvider).getRole()));
    final isAdmin = roleAsync.value == 'Admin' || roleAsync.value == 'SuperAdmin';
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      isAdmin: isAdmin,
      title: 'Events & Gatherings',
      breadcrumb: 'PORTAL > EVENTS',
      floatingActionButton: isAdmin ? FloatingActionButton.extended(
        onPressed: () => _showCreateEvent(context, ref),
        backgroundColor: AppTheme.royalGold,
        icon: const Icon(Icons.add, color: Colors.black),
        label: const Text('MANAGE EVENTS', style: TextStyle(color: Colors.black, fontWeight: FontWeight.w900, fontSize: 10, letterSpacing: 1.5)),
      ) : null,
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Search events...',
                prefixIcon: const Icon(Icons.search, size: 20, color: AppTheme.royalGold),
                suffixIcon: _searchController.text.isNotEmpty 
                  ? IconButton(
                      icon: const Icon(Icons.close_rounded, size: 20, color: Colors.white54),
                      onPressed: () {
                        _searchController.clear();
                        ref.read(eventSearchQueryProvider.notifier).state = "";
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
              onChanged: (v) => ref.read(eventSearchQueryProvider.notifier).state = v.toLowerCase(),
            ),
          ),
          Expanded(
            child: AsyncValueWidget<List<dynamic>>(
              value: eventsAsync,
              loadingMessage: 'Loading events...',
              onRetry: () => ref.invalidate(eventsListProvider),
              data: (events) {
                final filtered = events.where((e) {
                  final title = e['title']?.toString().toLowerCase() ?? '';
                  return title.contains(searchQuery);
                }).toList();

                return RefreshIndicator(
                  color: AppTheme.royalGold,
                  onRefresh: () async {
                    HapticFeedback.mediumImpact();
                    ref.invalidate(eventsListProvider);
                  },
                  child: Column(
                    children: [
                      if (filtered.isNotEmpty)
                        Padding(
                          padding: const EdgeInsets.only(left: 24, bottom: 8),
                          child: Align(
                            alignment: Alignment.centerLeft,
                            child: Text(
                              'Showing ${filtered.length} of ${events.length} listed events',
                              style: TextStyle(fontSize: 10, color: AppTheme.royalGold.withValues(alpha: 0.7), fontWeight: FontWeight.bold),
                            ),
                          ),
                        ),
                      Expanded(
                        child: filtered.isEmpty
                          ? Center(child: Text(searchQuery.isEmpty ? 'No active events found.' : 'No events match your search.', style: const TextStyle(color: AppTheme.textSecondaryDark)))
                          : ListView.builder(
                              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                              itemCount: filtered.length,
                              itemBuilder: (context, index) {
                                final ev = filtered[index];
                                final isOpen = _isRegistrationOpen(ev);
                                final coverUrl = ev['coverImageUrl'] ?? ev['imageUrl'];
                                String? fullImgUrl;
                                if (coverUrl != null && coverUrl.toString().isNotEmpty) {
                                  if (coverUrl.toString().startsWith('http')) {
                                    fullImgUrl = coverUrl.toString();
                                  } else {
                                    final base = AppConfig.apiBaseUrl.endsWith('/') ? AppConfig.apiBaseUrl.substring(0, AppConfig.apiBaseUrl.length - 1) : AppConfig.apiBaseUrl;
                                    final cleanP = coverUrl.toString().startsWith('/') ? coverUrl.toString().substring(1) : coverUrl.toString();
                                    fullImgUrl = '$base/$cleanP';
                                  }
                                }

                                return Padding(
                                  padding: const EdgeInsets.only(bottom: 24.0),
                                  child: GestureDetector(
                                    onTap: () {
                                      HapticFeedback.lightImpact();
                                      context.push('/events/${ev['id']}');
                                    },
                                    child: GlassContainer(
                                      padding: EdgeInsets.zero,
                                      child: Column(
                                        crossAxisAlignment: CrossAxisAlignment.stretch,
                                        children: [
                                          Container(
                                            height: 190,
                                            decoration: BoxDecoration(
                                              color: AppTheme.royalGold.withValues(alpha: 0.05),
                                              borderRadius: const BorderRadius.vertical(top: Radius.circular(16)),
                                              image: fullImgUrl != null ? DecorationImage(image: NetworkImage(fullImgUrl), fit: BoxFit.cover) : null,
                                            ),
                                            child: Stack(
                                              children: [
                                                if (fullImgUrl == null)
                                                  Center(child: Icon(Icons.celebration_outlined, size: 64, color: AppTheme.royalGold.withValues(alpha: 0.1))),
                                                Positioned(
                                                  top: 16,
                                                  right: 16,
                                                  child: Container(
                                                    padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
                                                    decoration: BoxDecoration(color: isOpen ? Colors.green.withValues(alpha: 0.9) : Colors.redAccent.withValues(alpha: 0.9), borderRadius: BorderRadius.circular(6)),
                                                    child: Text(isOpen ? 'REGISTRATION OPEN' : 'CLOSED', style: const TextStyle(color: Colors.white, fontSize: 8, fontWeight: FontWeight.w900, letterSpacing: 1)),
                                                  ),
                                                ),
                                                Positioned(
                                                  bottom: 0,
                                                  left: 0,
                                                  right: 0,
                                                  child: Container(
                                                    padding: const EdgeInsets.all(12),
                                                    decoration: BoxDecoration(gradient: LinearGradient(begin: Alignment.bottomCenter, end: Alignment.topCenter, colors: [Colors.black.withValues(alpha: 0.8), Colors.transparent])),
                                                    child: Row(
                                                      children: [
                                                        const Icon(Icons.pin_drop_outlined, size: 14, color: AppTheme.royalGold),
                                                        const SizedBox(width: 8),
                                                        Text(ev['location']?.toString().toUpperCase() ?? 'LOCATION TBD', style: const TextStyle(color: Colors.white, fontSize: 10, fontWeight: FontWeight.w900, letterSpacing: 0.5)),
                                                      ],
                                                    ),
                                                  ),
                                                ),
                                              ],
                                            ),
                                          ),
                                          Padding(
                                            padding: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 12.0),
                                            child: Column(
                                              crossAxisAlignment: CrossAxisAlignment.start,
                                              children: [
                                                Row(
                                                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                                  children: [
                                                    Text(ev['startDate']?.toString().split('T')[0] ?? 'DATE TBD', style: const TextStyle(color: AppTheme.royalGold, fontSize: 11, fontWeight: FontWeight.w900, letterSpacing: 1)),
                                                    if (ev['registrationFee'] != null && ev['registrationFee'] > 0)
                                                      Text('${ev['registrationFee']} BDT', style: const TextStyle(color: Colors.white, fontSize: 13, fontWeight: FontWeight.w900))
                                                    else
                                                      const Text('FREE ENTRY', style: TextStyle(color: Colors.greenAccent, fontSize: 10, fontWeight: FontWeight.w900, letterSpacing: 1)),
                                                  ],
                                                ),
                                                const SizedBox(height: 14),
                                                Text(ev['title'] ?? 'Alumni Reunion Event', style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w900, color: Colors.white, height: 1.2, letterSpacing: -0.2)),
                                                const SizedBox(height: 6),
                                                Text(ev['description'] ?? 'No description provided.', style: const TextStyle(fontSize: 12, height: 1.4, color: AppTheme.textSecondaryDark), maxLines: 2, overflow: TextOverflow.ellipsis),
                                                const SizedBox(height: 16),
                                                SizedBox(
                                                  width: double.infinity,
                                                  child: ElevatedButton.icon(
                                                    onPressed: isOpen ? () {
                                                      HapticFeedback.lightImpact();
                                                      handleEventPayment((ev['registrationFee'] ?? 0).toDouble(), ev['title'] ?? 'Event');
                                                    } : null,
                                                    icon: Icon(isOpen ? Icons.how_to_reg_rounded : Icons.lock_clock_outlined, size: 16),
                                                    label: Text(isOpen ? 'REGISTER' : 'CLOSED', style: const TextStyle(fontSize: 11, fontWeight: FontWeight.w900, letterSpacing: 1)),
                                                    style: ElevatedButton.styleFrom(
                                                      backgroundColor: isOpen ? AppTheme.royalGold : Colors.white10,
                                                      foregroundColor: isOpen ? Colors.black : Colors.white38,
                                                      padding: const EdgeInsets.symmetric(vertical: 16),
                                                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                                                    ),
                                                  ),
                                                ),
                                              ],
                                            ),
                                          ),
                                        ],
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


  bool _isRegistrationOpen(Map<String, dynamic> ev) {
    if (ev['isActive'] == false) return false;
    final now = DateTime.now();
    if (ev['registrationStartDate'] != null && DateTime.parse(ev['registrationStartDate']).isAfter(now)) return false;
    if (ev['registrationEndDate'] != null && DateTime.parse(ev['registrationEndDate']).isBefore(now)) return false;
    return true;
  }

  void _showCreateEvent(BuildContext context, WidgetRef ref) {
    final titleCtrl = TextEditingController();
    final descCtrl = TextEditingController();
    final locCtrl = TextEditingController();

    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: AppTheme.midnightSurface,
        title: const Text('CREATE EVENT', style: TextStyle(color: AppTheme.royalGold, fontSize: 14, fontWeight: FontWeight.bold)),
        content: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextField(controller: titleCtrl, style: const TextStyle(color: Colors.white), decoration: const InputDecoration(labelText: 'Title')),
              TextField(controller: descCtrl, style: const TextStyle(color: Colors.white, height: 1.5), decoration: const InputDecoration(labelText: 'Description'), maxLines: 3),
              TextField(controller: locCtrl, style: const TextStyle(color: Colors.white), decoration: const InputDecoration(labelText: 'Location')),
            ],
          ),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('CANCEL', style: TextStyle(color: Colors.white54))),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold),
            onPressed: () async {
              try {
                final payload = {
                  'title': titleCtrl.text,
                  'description': descCtrl.text,
                  'location': locCtrl.text,
                  'startDate': DateTime.now().add(const Duration(days: 30)).toIso8601String(),
                  'isActive': true,
                };
                final success = await ref.read(eventsServiceProvider).createEvent(payload);
                if (success) {
                   ref.invalidate(eventsListProvider);
                   if (ctx.mounted) Navigator.pop(ctx);
                }
              } catch (_) {}
            },
            child: const Text('SAVE', style: TextStyle(color: Colors.black, fontWeight: FontWeight.bold)),
          ),
        ],
      ),
    );
  }
}
