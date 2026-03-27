import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../features/events/events_service.dart';
import '../../features/financials/gateway_service.dart';
import '../financials/payment_web_page.dart';

final eventsListProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(eventsServiceProvider).getUpcomingEvents();
});

class EventsScreen extends ConsumerWidget {
  const EventsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    Future<void> handleEventPayment(double amount, String eventTitle) async {
       final gateway = ref.read(gatewayServiceProvider);
       final res = await gateway.initiate(amount, PaymentGateway.sslCommerz, 'EVT-REG-$eventTitle');
       if (res.success && res.gatewayUrl != null && context.mounted) {
         Navigator.of(context).push(MaterialPageRoute(builder: (_) => PaymentWebPage(url: res.gatewayUrl!)));
       }
    }
    
    final eventsAsync = ref.watch(eventsListProvider);

    return AppScaffold(
      title: 'Alumni Events',
      child: AsyncValueWidget(
        value: eventsAsync,
        loadingMessage: 'Synchronizing upcoming celebrations...',
        onRetry: () => ref.invalidate(eventsListProvider),
        data: (events) => events.isEmpty 
          ? const Center(child: Text('No upcoming events found.', style: TextStyle(color: AppTheme.textSecondaryDark)))
          : RefreshIndicator(
              color: AppTheme.royalGold,
              onRefresh: () async => ref.invalidate(eventsListProvider),
              child: ListView.builder(
                padding: const EdgeInsets.all(24),
                itemCount: events.length,
                itemBuilder: (context, index) {
                  final event = events[index];
                  return Padding(
                    padding: const EdgeInsets.only(bottom: 24.0),
                    child: GlassContainer(
                      padding: EdgeInsets.zero,
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: [
                          Container(
                            height: 180,
                            decoration: BoxDecoration(
                              color: AppTheme.royalGold.withValues(alpha: 0.05),
                              image: event['coverImageUrl'] != null 
                                ? DecorationImage(image: NetworkImage(event['coverImageUrl']), fit: BoxFit.cover)
                                : null,
                            ),
                            child: event['coverImageUrl'] == null 
                              ? Center(child: Icon(Icons.celebration_outlined, size: 64, color: AppTheme.royalGold.withValues(alpha: 0.2)))
                              : Stack(
                                  children: [
                                    Positioned(
                                      top: 16, right: 16,
                                      child: Container(
                                        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                                        decoration: BoxDecoration(color: Colors.black54, borderRadius: BorderRadius.circular(20)),
                                        child: Text(event['eventDate'] ?? 'Date TBD', style: const TextStyle(color: Colors.white, fontSize: 10, fontWeight: FontWeight.bold)),
                                      ),
                                    ),
                                  ],
                                ),
                          ),
                          Padding(
                            padding: const EdgeInsets.all(24.0),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(
                                  event['title']?.toUpperCase() ?? 'ALUMNI REUNION',
                                  style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w900, letterSpacing: 1.2),
                                ),
                                const SizedBox(height: 8),
                                Text(
                                  event['description'] ?? 'No description available for this event.',
                                  style: TextStyle(height: 1.5, fontSize: 13, color: AppTheme.textSecondaryDark),
                                  maxLines: 3,
                                  overflow: TextOverflow.ellipsis,
                                ),
                                const SizedBox(height: 24),
                                Row(
                                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                  children: [
                                    Column(
                                      crossAxisAlignment: CrossAxisAlignment.start,
                                      children: [
                                        const Text('REGISTRATION FEE', style: TextStyle(fontSize: 10, color: AppTheme.royalGold, fontWeight: FontWeight.bold)),
                                        Text('${event['registrationFee'] ?? 0} BDT', style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 16)),
                                      ],
                                    ),
                                    ElevatedButton.icon(
                                      onPressed: () => handleEventPayment((event['registrationFee'] ?? 0).toDouble(), event['title'] ?? 'Event'),
                                      icon: const Icon(Icons.how_to_reg_outlined, size: 18),
                                      label: const Text('JOIN EVENT'),
                                      style: ElevatedButton.styleFrom(padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12)),
                                    ),
                                  ],
                                ),
                              ],
                            ),
                          ),
                        ],
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
