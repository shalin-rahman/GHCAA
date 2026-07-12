import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../features/events/events_service.dart';
import '../../features/auth/auth_service.dart';
import '../../features/financials/gateway_service.dart';
import '../financials/payment_web_page.dart';
import '../../core/config/app_config.dart';
import '../../core/widgets/custom_network_image.dart';
import '../../core/utils/app_utils.dart';
import '../../core/widgets/app_search_field.dart';
import '../../core/widgets/empty_state_widget.dart';

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
    Future<void> processPayment(double amount, PaymentGateway gateway, String eventTitle) async {
      final gatewayService = ref.read(gatewayServiceProvider);
      final res = await gatewayService.initiate(
          amount, gateway, 'EVT-REG-$eventTitle');
      if (res.success && res.gatewayUrl != null && context.mounted) {
        Navigator.of(context).push(MaterialPageRoute(
            builder: (_) => PaymentWebPage(url: res.gatewayUrl!)));
      } else if (!res.success && context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(
          content: Text(res.message ?? 'Payment initiation failed.'),
          backgroundColor: Colors.redAccent,
        ));
      }
    }

    Future<void> handleEventPayment(double amount, String eventTitle) async {
      showModalBottomSheet(
        context: context,
        backgroundColor: AppTheme.deepCharcoal,
        shape: const RoundedRectangleBorder(
          borderRadius: BorderRadius.vertical(top: Radius.circular(AppTheme.radiusXL)),
        ),
        builder: (context) => Container(
          padding: const EdgeInsets.all(AppTheme.spaceL),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Text('SELECT PAYMENT METHOD', 
                textAlign: TextAlign.center,
                style: Theme.of(context).textTheme.labelLarge?.copyWith(letterSpacing: 2, color: AppTheme.royalGold)),
              const SizedBox(height: AppTheme.spaceL),
              _paymentOption(context, 'SSLCommerz', 'Secure Pay with Cards/MFS', Icons.account_balance_wallet_rounded, () async {
                Navigator.pop(context);
                await processPayment(amount, PaymentGateway.sslCommerz, eventTitle);
              }),
              const SizedBox(height: AppTheme.spaceM),
              _paymentOption(context, 'DGePay', 'Debit/Credit Cards & Wallets', Icons.credit_card_rounded, () async {
                Navigator.pop(context);
                await processPayment(amount, PaymentGateway.dgePay, eventTitle);
              }),
              const SizedBox(height: AppTheme.spaceXL),
            ],
          ),
        ),
      );
    }

    final eventsAsync = ref.watch(eventsListProvider);
    final searchQuery = ref.watch(eventSearchQueryProvider);
    final roleAsync = ref.watch(FutureProvider((ref) => ref.read(authServiceProvider).getRole()));
    final isAdmin = roleAsync.value?.isStaffAdminRole ?? false;


    return AppScaffold(
      isAdmin: isAdmin,
      title: 'Events',
      breadcrumb: 'PORTAL > EVENTS',
      floatingActionButton: isAdmin ? FloatingActionButton.extended(
        onPressed: () => _showCreateEvent(context, ref),
        backgroundColor: AppTheme.royalGold,
        elevation: 8,
        icon: const Icon(Icons.add, color: Colors.black, size: 20),
        label: Text('ADD EVENT', style: Theme.of(context).textTheme.labelLarge?.copyWith(color: Colors.black, fontSize: 11)),
      ) : null,
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(AppTheme.spaceL, AppTheme.spaceM, AppTheme.spaceL, AppTheme.spaceS),
            child: AppSearchField(
              controller: _searchController,
              hintText: 'Search events...',
              onChanged: (v) => ref.read(eventSearchQueryProvider.notifier).state = v.toLowerCase(),
              onClear: () {
                _searchController.clear();
                ref.read(eventSearchQueryProvider.notifier).state = "";
              },
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
                          padding: const EdgeInsets.only(left: AppTheme.spaceL, bottom: AppTheme.spaceS, top: AppTheme.spaceXS),
                          child: Align(
                            alignment: Alignment.centerLeft,
                            child: Text(
                              'Showing ${filtered.length} of ${events.length} listed events',
                              style: Theme.of(context).textTheme.labelSmall?.copyWith(color: AppTheme.royalGold.withValues(alpha: 0.6), letterSpacing: 1),
                            ),
                          ),
                        ),
                      Expanded(
                        child: filtered.isEmpty
                          ? EmptyStateWidget(searchQuery.isEmpty ? 'No active events found.' : 'No events match your search.', icon: Icons.event_outlined)
                          : ListView.builder(
                              padding: const EdgeInsets.symmetric(horizontal: AppTheme.spaceL, vertical: AppTheme.spaceM),
                              itemCount: filtered.length,
                              itemBuilder: (context, index) {
                                final ev = filtered[index];
                                final isOpen = _isRegistrationOpen(ev);
                                final fullImgUrl = AppConfig.resolveImageUrl(ev['coverImageUrl'] ?? ev['imageUrl']);
 
                                return Padding(
                                  padding: const EdgeInsets.only(bottom: AppTheme.spaceL),
                                  child: GestureDetector(
                                    onTap: () {
                                      HapticFeedback.lightImpact();
                                      context.push('/events/${ev['id']}');
                                    },
                                    child: Card(
                                      margin: EdgeInsets.zero,
                                      child: Column(
                                        crossAxisAlignment: CrossAxisAlignment.stretch,
                                        children: [
                                          Stack(
                                            children: [
                                              SizedBox(
                                                height: 192,
                                                width: double.infinity,
                                                child: ClipRRect(
                                                  borderRadius: const BorderRadius.vertical(top: Radius.circular(AppTheme.radiusL)),
                                                  child: CustomNetworkImage(
                                                    imageUrl: fullImgUrl ?? '',
                                                    borderRadius: 0,
                                                    fit: BoxFit.cover,
                                                  ),
                                                ),
                                              ),
                                              if (fullImgUrl == null)
                                                SizedBox(
                                                  height: 192,
                                                  child: Center(child: Icon(Icons.celebration_outlined, size: 64, color: AppTheme.royalGold.withValues(alpha: 0.1))),
                                                ),
                                              Positioned(
                                                top: 16,
                                                right: 16,
                                                child: Container(
                                                  padding: const EdgeInsets.symmetric(horizontal: AppTheme.spaceS, vertical: AppTheme.spaceXS),
                                                  decoration: BoxDecoration(
                                                    color: isOpen ? Colors.green.withValues(alpha: 0.9) : Colors.redAccent.withValues(alpha: 0.9), 
                                                    borderRadius: BorderRadius.circular(AppTheme.radiusXS),
                                                    boxShadow: [BoxShadow(color: Colors.black.withValues(alpha: 0.3), blurRadius: AppTheme.spaceS)]
                                                  ),
                                                  child: Text(isOpen ? 'REGISTRATION OPEN' : 'CLOSED', style: const TextStyle(color: Colors.white, fontSize: 8, fontWeight: FontWeight.w900, letterSpacing: 1)),
                                                ),
                                              ),
                                              Positioned(
                                                bottom: 0,
                                                left: 0,
                                                right: 0,
                                                child: Container(
                                                  padding: const EdgeInsets.all(AppTheme.spaceM),
                                                  decoration: BoxDecoration(
                                                    gradient: LinearGradient(
                                                      begin: Alignment.bottomCenter, 
                                                      end: Alignment.topCenter, 
                                                      colors: [Colors.black.withValues(alpha: 0.9), Colors.transparent]
                                                    )
                                                  ),
                                                  child: Row(
                                                    children: [
                                                      const Icon(Icons.location_on_rounded, size: 16, color: AppTheme.royalGold),
                                                      const SizedBox(width: AppTheme.spaceS),
                                                      Text(ev['location']?.toString().toUpperCase() ?? 'LOCATION TBD', style: const TextStyle(color: Colors.white, fontSize: 10, fontWeight: FontWeight.w900, letterSpacing: 0.5)),
                                                    ],
                                                  ),
                                                ),
                                              ),
                                            ],
                                          ),
                                          Padding(
                                            padding: const EdgeInsets.all(AppTheme.spaceL),
                                            child: Column(
                                              crossAxisAlignment: CrossAxisAlignment.start,
                                              children: [
                                                  Row(
                                                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                                    children: [
                                                      Expanded(
                                                        child: Text(AppUtils.formatDate(ev['startDate']), style: Theme.of(context).textTheme.labelLarge, overflow: TextOverflow.ellipsis),
                                                      ),
                                                      const SizedBox(width: 8),
                                                      if (ev['registrationFee'] != null && ev['registrationFee'] > 0)
                                                        Flexible(
                                                          child: Text(AppUtils.formatCurrency(ev['registrationFee']), style: Theme.of(context).textTheme.titleLarge?.copyWith(color: AppTheme.royalGold, fontWeight: FontWeight.w900), overflow: TextOverflow.ellipsis),
                                                        )
                                                      else
                                                        const Flexible(
                                                          child: Text('FREE ENTRY', style: TextStyle(color: Colors.greenAccent, fontSize: 11, fontWeight: FontWeight.w900, letterSpacing: 1.5), overflow: TextOverflow.ellipsis),
                                                        ),
                                                    ],
                                                  ),
                                                  const SizedBox(height: AppTheme.spaceM),
                                                Text(ev['title'] ?? 'Alumni Reunion Event', style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w900)),
                                                const SizedBox(height: AppTheme.spaceS),
                                                Text(ev['description'] ?? 'No description provided.', style: Theme.of(context).textTheme.bodyMedium, maxLines: 2, overflow: TextOverflow.ellipsis),
                                                const SizedBox(height: AppTheme.spaceL),
                                                SizedBox(
                                                  width: double.infinity,
                                                  child: ElevatedButton.icon(
                                                    onPressed: isOpen ? () {
                                                      HapticFeedback.lightImpact();
                                                      handleEventPayment((ev['registrationFee'] ?? 0).toDouble(), ev['title'] ?? 'Event');
                                                    } : null,
                                                    icon: Icon(isOpen ? Icons.how_to_reg_rounded : Icons.lock_clock_outlined, size: 18),
                                                    label: Text(isOpen ? 'CONFIRM REGISTRATION' : 'REGISTRATION CLOSED'),
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
    if (ev['registrationStartDate'] != null && AppUtils.parseDate(ev['registrationStartDate'].toString())?.isAfter(now) == true) return false;
    if (ev['registrationEndDate'] != null && AppUtils.parseDate(ev['registrationEndDate'].toString())?.isBefore(now) == true) return false;
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
          backgroundColor: AppTheme.deepCharcoal,
          surfaceTintColor: Colors.transparent,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(AppTheme.radiusXL),
            side: const BorderSide(color: AppTheme.glassBorder),
          ),
          title: Center(
            child: Text('CREATE NEW EVENT', 
              style: Theme.of(context).textTheme.labelLarge?.copyWith(letterSpacing: 2)),
          ),
          content: SizedBox(
            width: MediaQuery.of(context).size.width * 0.85,
            child: SingleChildScrollView(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                   const SizedBox(height: AppTheme.spaceM),
                  TextField(controller: titleCtrl, decoration: const InputDecoration(labelText: 'Event Title', prefixIcon: Icon(Icons.title_rounded))),
                  const SizedBox(height: AppTheme.spaceM),
                  TextField(controller: descCtrl, decoration: const InputDecoration(labelText: 'Description', prefixIcon: Icon(Icons.description_rounded)), maxLines: 3),
                  const SizedBox(height: AppTheme.spaceM),
                  TextField(controller: locCtrl, decoration: const InputDecoration(labelText: 'Primary Location', prefixIcon: Icon(Icons.place_rounded))),
 
                  const SizedBox(height: AppTheme.spaceL),
                  _sectionHeader(context, 'EVENT DATES'),
                  const SizedBox(height: AppTheme.spaceM),
                  _datePickerRow(context, 'Start Date', startDate, (picked) => setDialogState(() => startDate = picked)),
                  const SizedBox(height: AppTheme.spaceS),
                  _datePickerRow(context, 'End Date', endDate, (picked) => setDialogState(() => endDate = picked)),
 
                  const SizedBox(height: AppTheme.spaceL),
                  _sectionHeader(context, 'REGISTRATION DETAILS'),
                  const SizedBox(height: AppTheme.spaceM),
                  _datePickerRow(context, 'Opens', regStart, (picked) => setDialogState(() => regStart = picked), nullable: true),
                  const SizedBox(height: AppTheme.spaceS),
                  _datePickerRow(context, 'Closes', regEnd, (picked) => setDialogState(() => regEnd = picked), nullable: true),
 
                  const SizedBox(height: AppTheme.spaceL),
                  _sectionHeader(context, 'FEE AND CAPACITY'),
                  const SizedBox(height: AppTheme.spaceM),
 
                  // Free/Paid toggle
                  Container(
                    padding: const EdgeInsets.all(AppTheme.spaceM),
                    decoration: BoxDecoration(
                      color: Colors.white.withValues(alpha: 0.03),
                      borderRadius: BorderRadius.circular(AppTheme.radiusM),
                      border: Border.all(color: AppTheme.glassBorder),
                    ),
                    child: Row(
                      children: [
                        Icon(isFree ? Icons.celebration_rounded : Icons.payments_rounded,
                            color: isFree ? Colors.greenAccent : AppTheme.royalGold, size: 20),
                        const SizedBox(width: AppTheme.spaceM),
                        Expanded(
                          child: Text(isFree ? 'FREE EVENT' : 'PAID EVENT',
                              style: Theme.of(context).textTheme.labelLarge?.copyWith(
                                color: isFree ? Colors.greenAccent : AppTheme.royalGold,
                                fontSize: 10,
                              )),
                        ),
                        Transform.scale(
                          scale: 0.7,
                          child: Switch(
                            value: !isFree,
                            onChanged: (v) => setDialogState(() { isFree = !v; if (!v) feeCtrl.clear(); }),
                            activeThumbColor: AppTheme.royalGold,
                          ),
                        ),
                      ],
                    ),
                  ),
 
                  // Fee field
                  if (!isFree) Padding(
                    padding: const EdgeInsets.only(top: AppTheme.spaceM),
                    child: TextField(
                      controller: feeCtrl,
                      keyboardType: const TextInputType.numberWithOptions(decimal: true),
                      decoration: const InputDecoration(labelText: 'Registration Fee (BDT)', prefixIcon: Icon(Icons.currency_exchange_rounded)),
                    ),
                  ),
 
                  const SizedBox(height: 12),
                  Row(
                    children: [
                      Expanded(
                        child: Text('ALLOW NON-MEMBERS', 
                          style: Theme.of(context).textTheme.bodySmall?.copyWith(fontWeight: FontWeight.w700, letterSpacing: 0.5)),
                      ),
                      Transform.scale(
                        scale: 0.7,
                        child: Switch(
                          value: allowNonMembers,
                          onChanged: (v) => setDialogState(() => allowNonMembers = v),
                          activeThumbColor: AppTheme.royalGold,
                        ),
                      ),
                    ],
                  ),
 
                  const SizedBox(height: 4),
                  TextField(
                    controller: capacityCtrl,
                    keyboardType: TextInputType.number,
                    decoration: const InputDecoration(labelText: 'Participant Capacity', prefixIcon: Icon(Icons.groups_rounded), hintText: 'Unlimited if empty'),
                  ),
                ],
              ),
            ),
          ),
          actions: [
            TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('CANCEL', style: TextStyle(color: Colors.white38))),
            ElevatedButton(
              onPressed: () async {
                if (titleCtrl.text.isEmpty || descCtrl.text.isEmpty || locCtrl.text.isEmpty) {
                  ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Please fill all required fields.')));
                  return;
                }
                try {
                  final fee = isFree ? null : (double.tryParse(feeCtrl.text));
                  final payload = {
                    'title': titleCtrl.text.trim(),
                    'description': descCtrl.text.trim(),
                    'location': locCtrl.text.trim(),
                    'startDate': AppUtils.formatDate(startDate),
                    'endDate': AppUtils.formatDate(endDate),
                    'requiresPayment': !isFree,
                    'registrationFee': fee,
                    'allowNonMembers': allowNonMembers,
                    'isActive': true,
                    if (regStart != null) 'registrationStartDate': AppUtils.formatDate(regStart!),
                    if (regEnd != null) 'registrationEndDate': AppUtils.formatDate(regEnd!),
                  };
                  final success = await ref.read(eventsServiceProvider).createEvent(payload);
                  if (success) {
                    ref.invalidate(eventsListProvider);
                    if (ctx.mounted) Navigator.pop(ctx);
                    if (context.mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Event created successfully.')));
                  }
                } catch (e) {
                  if (context.mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Error: $e'), backgroundColor: Colors.redAccent));
                }
              },
              child: const Text('CREATE EVENT'),
            ),
          ],
        ),
      ),
    );
  }
 
  Widget _sectionHeader(BuildContext context, String title) {
    return Text(title, style: Theme.of(context).textTheme.labelSmall?.copyWith(color: AppTheme.royalGold, letterSpacing: 1.5, fontSize: 8));
  }

  Widget _paymentOption(BuildContext context, String title, String subtitle, IconData icon, VoidCallback onTap) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(AppTheme.radiusL),
      child: Container(
        padding: const EdgeInsets.all(AppTheme.spaceM),
        decoration: BoxDecoration(
          color: Colors.white.withValues(alpha: 0.03),
          borderRadius: BorderRadius.circular(AppTheme.radiusL),
          border: Border.all(color: AppTheme.glassBorder),
        ),
        child: Row(
          children: [
            Container(
              padding: const EdgeInsets.all(AppTheme.spaceS),
              decoration: BoxDecoration(
                color: AppTheme.royalGold.withValues(alpha: 0.1),
                shape: BoxShape.circle,
              ),
              child: Icon(icon, color: AppTheme.royalGold, size: 24),
            ),
            const SizedBox(width: AppTheme.spaceM),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(title, style: Theme.of(context).textTheme.titleSmall?.copyWith(fontWeight: FontWeight.w900, color: Colors.white)),
                  Text(subtitle, style: Theme.of(context).textTheme.labelSmall?.copyWith(color: Colors.white38, fontSize: 10)),
                ],
              ),
            ),
            const Icon(Icons.chevron_right_rounded, color: Colors.white24),
          ],
        ),
      ),
    );
  }

  Widget _datePickerRow(BuildContext context, String label, DateTime? value, void Function(DateTime) onPicked, {bool nullable = false}) {
    return GestureDetector(
      onTap: () async {
        final picked = await showDatePicker(
          context: context,
          initialDate: value ?? DateTime.now(),
          firstDate: DateTime.now().subtract(const Duration(days: 1)),
          lastDate: DateTime.now().add(const Duration(days: 365 * 3)),
          builder: (context, child) => Theme(
            data: Theme.of(context).copyWith(
              colorScheme: const ColorScheme.dark(primary: AppTheme.royalGold, surface: AppTheme.deepCharcoal),
            ),
            child: child!,
          ),
        );
        if (picked != null) onPicked(picked);
      },
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: AppTheme.spaceM, vertical: AppTheme.spaceM),
        decoration: BoxDecoration(
          color: AppTheme.obsidianBlack,
          borderRadius: BorderRadius.circular(AppTheme.radiusM),
          border: Border.all(color: AppTheme.glassBorder),
        ),
        child: Row(
          children: [
            const Icon(Icons.event_note_rounded, size: 16, color: AppTheme.royalGold),
            const SizedBox(width: AppTheme.spaceM),
            Expanded(
              child: Text(
                value != null ? AppUtils.formatDate(value) : 'Select $label',
                style: Theme.of(context).textTheme.bodySmall?.copyWith(color: value != null ? Colors.white : Colors.white38),
              ),
            ),
            Text(label.toUpperCase(), style: Theme.of(context).textTheme.labelSmall?.copyWith(fontSize: 7, color: AppTheme.royalGold.withValues(alpha: 0.5))),
          ],
        ),
      ),
    );
  }
}
