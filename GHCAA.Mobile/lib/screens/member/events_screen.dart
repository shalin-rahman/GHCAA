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
import '../../core/widgets/custom_network_image.dart';

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
    final isAdmin = roleAsync.value?.isStaffAdminRole ?? false;
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      isAdmin: isAdmin,
      title: 'Events',
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
                                          Stack(
                                            children: [
                                              SizedBox(
                                                height: 190,
                                                width: double.infinity,
                                                child: CustomNetworkImage(
                                                  imageUrl: fullImgUrl ?? '',
                                                  borderRadius: 16,
                                                  fit: BoxFit.cover,
                                                ),
                                              ),
                                              if (fullImgUrl == null)
                                                SizedBox(
                                                  height: 190,
                                                  child: Center(child: Icon(Icons.celebration_outlined, size: 64, color: AppTheme.royalGold.withValues(alpha: 0.1))),
                                                ),
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
    final feeCtrl = TextEditingController();
    final capacityCtrl = TextEditingController();
    bool isFree = false;
    bool allowNonMembers = false;
    DateTime startDate = DateTime.now().add(const Duration(days: 30));
    DateTime endDate = DateTime.now().add(const Duration(days: 30, hours: 4));
    DateTime? regStart;
    DateTime? regEnd;

    showDialog(
      context: context,
      builder: (ctx) => StatefulBuilder(
        builder: (ctx, setDialogState) => AlertDialog(
          backgroundColor: AppTheme.midnightSurface,
          title: const Text('CREATE EVENT', style: TextStyle(color: AppTheme.royalGold, fontSize: 13, fontWeight: FontWeight.bold, letterSpacing: 1)),
          content: SizedBox(
            width: double.maxFinite,
            child: SingleChildScrollView(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [

                  // Core Details
                  TextField(controller: titleCtrl, style: const TextStyle(color: Colors.white), decoration: const InputDecoration(labelText: 'Event Title *', prefixIcon: Icon(Icons.event_outlined))),
                  const SizedBox(height: 10),
                  TextField(controller: descCtrl, style: const TextStyle(color: Colors.white, height: 1.5), decoration: const InputDecoration(labelText: 'Description *', prefixIcon: Icon(Icons.description_outlined)), maxLines: 3),
                  const SizedBox(height: 10),
                  TextField(controller: locCtrl, style: const TextStyle(color: Colors.white), decoration: const InputDecoration(labelText: 'Location *', prefixIcon: Icon(Icons.location_on_outlined))),

                  const SizedBox(height: 16),
                  const Divider(color: Colors.white12),
                  const Text('EVENT DATES', style: TextStyle(color: Colors.white38, fontSize: 9, fontWeight: FontWeight.bold, letterSpacing: 1)),
                  const SizedBox(height: 8),

                  // Event Start Date
                  _datePickerRow(ctx, 'Start Date', startDate, (picked) => setDialogState(() => startDate = picked)),
                  const SizedBox(height: 8),
                  _datePickerRow(ctx, 'End Date', endDate, (picked) => setDialogState(() => endDate = picked)),

                  const SizedBox(height: 16),
                  const Divider(color: Colors.white12),
                  const Text('REGISTRATION WINDOW', style: TextStyle(color: Colors.white38, fontSize: 9, fontWeight: FontWeight.bold, letterSpacing: 1)),
                  const SizedBox(height: 8),
                  _datePickerRow(ctx, 'Reg. Opens', regStart, (picked) => setDialogState(() => regStart = picked), nullable: true),
                  const SizedBox(height: 8),
                  _datePickerRow(ctx, 'Reg. Closes', regEnd, (picked) => setDialogState(() => regEnd = picked), nullable: true),

                  const SizedBox(height: 16),
                  const Divider(color: Colors.white12),
                  const Text('PRICING & ACCESS', style: TextStyle(color: Colors.white38, fontSize: 9, fontWeight: FontWeight.bold, letterSpacing: 1)),
                  const SizedBox(height: 8),

                  // Free/Paid toggle
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                    decoration: BoxDecoration(
                      color: isFree ? Colors.greenAccent.withValues(alpha: 0.06) : Colors.white.withValues(alpha: 0.04),
                      borderRadius: BorderRadius.circular(10),
                      border: Border.all(color: isFree ? Colors.greenAccent.withValues(alpha: 0.3) : Colors.white12),
                    ),
                    child: Row(
                      children: [
                        Icon(isFree ? Icons.volunteer_activism_outlined : Icons.credit_card_outlined,
                            color: isFree ? Colors.greenAccent : AppTheme.royalGold, size: 18),
                        const SizedBox(width: 10),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(isFree ? 'FREE EVENT' : 'PAID EVENT',
                                  style: TextStyle(color: isFree ? Colors.greenAccent : AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.bold, letterSpacing: 1)),
                              Text(isFree ? 'No fee required for participants' : 'Set a registration fee below',
                                  style: const TextStyle(color: Colors.white38, fontSize: 9)),
                            ],
                          ),
                        ),
                        Switch(
                          value: isFree,
                          onChanged: (v) => setDialogState(() { isFree = v; if (v) feeCtrl.clear(); }),
                          activeThumbColor: Colors.greenAccent,
                          inactiveThumbColor: AppTheme.royalGold,
                          inactiveTrackColor: AppTheme.royalGold.withValues(alpha: 0.3),
                        ),
                      ],
                    ),
                  ),

                  // Fee field (only when paid)
                  AnimatedSize(
                    duration: const Duration(milliseconds: 200),
                    child: isFree ? const SizedBox() : Padding(
                      padding: const EdgeInsets.only(top: 10),
                      child: TextField(
                        controller: feeCtrl,
                        keyboardType: const TextInputType.numberWithOptions(decimal: true),
                        style: const TextStyle(color: Colors.white),
                        decoration: const InputDecoration(
                          labelText: 'Registration Fee (BDT)',
                          prefixIcon: Icon(Icons.currency_exchange),
                          hintText: 'e.g. 500',
                        ),
                      ),
                    ),
                  ),

                  const SizedBox(height: 10),
                  // Non-member toggle
                  SwitchListTile(
                    value: allowNonMembers,
                    onChanged: (v) => setDialogState(() => allowNonMembers = v),
                    activeThumbColor: AppTheme.royalGold,
                    title: const Text('Allow Non-Members', style: TextStyle(color: Colors.white, fontSize: 12)),
                    subtitle: const Text('Open registration to guests & public', style: TextStyle(color: Colors.white38, fontSize: 10)),
                    contentPadding: EdgeInsets.zero,
                    dense: true,
                  ),

                  const SizedBox(height: 4),
                  TextField(
                    controller: capacityCtrl,
                    keyboardType: TextInputType.number,
                    style: const TextStyle(color: Colors.white),
                    decoration: const InputDecoration(labelText: 'Max Capacity (leave blank for unlimited)', prefixIcon: Icon(Icons.people_outline)),
                  ),
                ],
              ),
            ),
          ),
          actions: [
            TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('CANCEL', style: TextStyle(color: Colors.white54))),
            ElevatedButton(
              style: ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold),
              onPressed: () async {
                if (titleCtrl.text.isEmpty || descCtrl.text.isEmpty || locCtrl.text.isEmpty) {
                  ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Title, description and location are required.')));
                  return;
                }
                try {
                  final fee = isFree ? null : (double.tryParse(feeCtrl.text));
                  final payload = {
                    'title': titleCtrl.text.trim(),
                    'description': descCtrl.text.trim(),
                    'location': locCtrl.text.trim(),
                    'startDate': startDate.toUtc().toIso8601String(),
                    'endDate': endDate.toUtc().toIso8601String(),
                    'requiresPayment': !isFree,
                    'registrationFee': fee,
                    'allowNonMembers': allowNonMembers,
                    'isActive': true,
                    if (regStart != null) 'registrationStartDate': regStart!.toUtc().toIso8601String(),
                    if (regEnd != null) 'registrationEndDate': regEnd!.toUtc().toIso8601String(),
                  };
                  final success = await ref.read(eventsServiceProvider).createEvent(payload);
                  if (success) {
                    ref.invalidate(eventsListProvider);
                    if (ctx.mounted) Navigator.pop(ctx);
                    if (context.mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Event created successfully.')));
                  } else {
                    if (context.mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Failed to create event.'), backgroundColor: Colors.redAccent));
                  }
                } catch (e) {
                  if (context.mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Error: $e'), backgroundColor: Colors.redAccent));
                }
              },
              child: const Text('CREATE', style: TextStyle(color: Colors.black, fontWeight: FontWeight.bold)),
            ),
          ],
        ),
      ),
    );
  }

  Widget _datePickerRow(BuildContext ctx, String label, DateTime? value, void Function(DateTime) onPicked, {bool nullable = false}) {
    return GestureDetector(
      onTap: () async {
        final picked = await showDatePicker(
          context: ctx,
          initialDate: value ?? DateTime.now(),
          firstDate: DateTime.now().subtract(const Duration(days: 1)),
          lastDate: DateTime.now().add(const Duration(days: 365 * 3)),
          builder: (context, child) => Theme(
            data: ThemeData.dark().copyWith(colorScheme: const ColorScheme.dark(primary: AppTheme.royalGold, surface: Color(0xFF1A1A2E))),
            child: child!,
          ),
        );
        if (picked != null) onPicked(picked);
      },
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
        decoration: BoxDecoration(
          color: Colors.white.withValues(alpha: 0.04),
          borderRadius: BorderRadius.circular(10),
          border: Border.all(color: Colors.white12),
        ),
        child: Row(
          children: [
            const Icon(Icons.calendar_today_outlined, size: 16, color: AppTheme.royalGold),
            const SizedBox(width: 10),
            Expanded(
              child: Text(
                value != null ? '${value.day}/${value.month}/${value.year}' : 'Set $label',
                style: TextStyle(color: value != null ? Colors.white : Colors.white38, fontSize: 12),
              ),
            ),
            Text(label, style: const TextStyle(color: Colors.white38, fontSize: 9, letterSpacing: 0.5)),
          ],
        ),
      ),
    );
  }
}
