import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../features/events/events_service.dart';
import '../../features/financials/gateway_service.dart';
import '../financials/payment_web_page.dart';
import '../../core/config/app_config.dart';

final eventsListProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(eventsServiceProvider).getUpcomingEvents();
});

class EventsScreen extends ConsumerWidget {
  const EventsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
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

    return AppScaffold(
      title: 'Alumni Gatherings',
      breadcrumb: 'Member Portal > Events & Reunions',
      child: AsyncValueWidget<List<dynamic>>(
        value: eventsAsync,
        loadingMessage: 'Synchronizing upcoming celebrations...',
        onRetry: () => ref.invalidate(eventsListProvider),
        data: (events) => RefreshIndicator(
          color: AppTheme.royalGold,
          onRefresh: () async {
            HapticFeedback.mediumImpact();
            ref.invalidate(eventsListProvider);
          },
          child: events.isEmpty
              ? const Center(child: Text('No upcoming celebrations found in the registry.', style: TextStyle(color: AppTheme.textSecondaryDark)))
              : ListView.builder(
                  padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                  itemCount: events.length,
                  itemBuilder: (context, index) {
                    final ev = events[index];
                    final isOpen = _isRegistrationOpen(ev);
                    final coverUrl = ev['coverImageUrl'] ?? ev['imageUrl'];
                    final fullImgUrl = coverUrl != null 
                        ? (coverUrl.toString().startsWith('http') 
                            ? coverUrl 
                            : '${AppConfig.apiBaseUrl}/$coverUrl'.replaceAll('//', '/'))
                        : null;

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
                                  color: AppTheme.royalGold.withOpacity(0.05),
                                  borderRadius: const BorderRadius.vertical(top: Radius.circular(16)),
                                  image: fullImgUrl != null ? DecorationImage(image: NetworkImage(fullImgUrl), fit: BoxFit.cover) : null,
                                ),
                                child: Stack(
                                  children: [
                                    if (fullImgUrl == null)
                                      Center(child: Icon(Icons.celebration_outlined, size: 64, color: AppTheme.royalGold.withOpacity(0.1))),
                                    Positioned(
                                      top: 16,
                                      right: 16,
                                      child: Container(
                                        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
                                        decoration: BoxDecoration(color: isOpen ? Colors.green.withOpacity(0.9) : Colors.redAccent.withOpacity(0.9), borderRadius: BorderRadius.circular(6)),
                                        child: Text(isOpen ? 'REGISTRATION OPEN' : 'REGISTRY CLOSED', style: const TextStyle(color: Colors.white, fontSize: 8, fontWeight: FontWeight.w900, letterSpacing: 1)),
                                      ),
                                    ),
                                    Positioned(
                                      bottom: 0,
                                      left: 0,
                                      right: 0,
                                      child: Container(
                                        padding: const EdgeInsets.all(12),
                                        decoration: BoxDecoration(gradient: LinearGradient(begin: Alignment.bottomCenter, end: Alignment.topCenter, colors: [Colors.black.withOpacity(0.8), Colors.transparent])),
                                        child: Row(
                                          children: [
                                            const Icon(Icons.pin_drop_outlined, size: 14, color: AppTheme.royalGold),
                                            const SizedBox(width: 8),
                                            Text(ev['location']?.toString().toUpperCase() ?? 'VENUE TBD', style: const TextStyle(color: Colors.white, fontSize: 10, fontWeight: FontWeight.w900, letterSpacing: 0.5)),
                                          ],
                                        ),
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                              Padding(
                                padding: const EdgeInsets.all(20.0),
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
                                    Text(ev['title'] ?? 'Alumni Reunion Event', style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w900, color: Colors.white, height: 1.3, letterSpacing: -0.2)),
                                    const SizedBox(height: 10),
                                    Text(ev['description'] ?? 'No description provided in the registry.', style: const TextStyle(fontSize: 13, height: 1.6, color: AppTheme.textSecondaryDark), maxLines: 3, overflow: TextOverflow.ellipsis),
                                    const SizedBox(height: 24),
                                    SizedBox(
                                      width: double.infinity,
                                      child: ElevatedButton.icon(
                                        onPressed: isOpen ? () {
                                          HapticFeedback.lightImpact();
                                          handleEventPayment((ev['registrationFee'] ?? 0).toDouble(), ev['title'] ?? 'Event');
                                        } : null,
                                        icon: Icon(isOpen ? Icons.how_to_reg_rounded : Icons.lock_clock_outlined, size: 16),
                                        label: Text(isOpen ? 'PARTICIPATE NOW' : 'REGISTRY CLOSED', style: const TextStyle(fontSize: 11, fontWeight: FontWeight.w900, letterSpacing: 1)),
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
}
