import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_exception.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../core/widgets/logo_spinner.dart';
import '../../core/utils/app_utils.dart';
import '../../features/elections/election_service.dart';

final myAppointmentsProvider =
    FutureProvider.autoDispose<List<ElectionAppointment>>((ref) async {
  return ref.read(electionServiceProvider).getMyAppointments();
});

class MyAppointmentsScreen extends ConsumerStatefulWidget {
  const MyAppointmentsScreen({super.key});

  @override
  ConsumerState<MyAppointmentsScreen> createState() =>
      _MyAppointmentsScreenState();
}

class _MyAppointmentsScreenState extends ConsumerState<MyAppointmentsScreen> {
  final Map<int, bool> _agreed = {};
  final Map<int, TextEditingController> _reasons = {};
  int? _busyId;

  @override
  void dispose() {
    for (final c in _reasons.values) {
      c.dispose();
    }
    super.dispose();
  }

  TextEditingController _reasonFor(int id) =>
      _reasons.putIfAbsent(id, TextEditingController.new);

  Future<void> _run(int id, Future<void> Function() call, String done) async {
    setState(() => _busyId = id);
    try {
      await call();
      ref.invalidate(myAppointmentsProvider);
      if (mounted) {
        ScaffoldMessenger.of(context)
            .showSnackBar(SnackBar(content: Text(done)));
      }
    } catch (e) {
      final detail =
          e is DioException ? ApiException.serverMessage(e.response?.data) : null;
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text(detail ?? 'That did not go through.')));
      }
    } finally {
      if (mounted) setState(() => _busyId = null);
    }
  }

  void _accept(ElectionAppointment a) => _run(
      a.id,
      () => ref.read(electionServiceProvider).acceptAppointment(a.id),
      'You are now ${a.personaName} for ${a.electionTitle}.');

  void _decline(ElectionAppointment a) {
    final reason = _reasonFor(a.id).text.trim();
    _run(
        a.id,
        () => ref
            .read(electionServiceProvider)
            .declineAppointment(a.id, reason.isEmpty ? null : reason),
        'Appointment declined.');
  }

  @override
  Widget build(BuildContext context) {
    final appointmentsAsync = ref.watch(myAppointmentsProvider);

    return AppScaffold(
      title: 'Election Appointments',
      breadcrumb: 'MY ACCOUNT > APPOINTMENTS',
      child: appointmentsAsync.when(
        loading: () => const Center(child: LogoSpinner(size: 120)),
        error: (e, s) => const EmptyStateWidget(
            'Appointments are unavailable. Try again later.',
            icon: Icons.error_outline),
        data: (list) => list.isEmpty
            ? const EmptyStateWidget('You have no open election appointment.',
                icon: Icons.how_to_vote_outlined)
            : RefreshIndicator(
                color: AppTheme.royalGold,
                onRefresh: () async => ref.invalidate(myAppointmentsProvider),
                child: ListView(
                  padding: const EdgeInsets.all(AppTheme.spaceM),
                  children: [for (final a in list) _card(context, a)],
                ),
              ),
      ),
    );
  }

  Widget _card(BuildContext context, ElectionAppointment a) {
    final text = Theme.of(context).textTheme;
    final busy = _busyId == a.id;

    return Padding(
      padding: const EdgeInsets.only(bottom: AppTheme.spaceM),
      child: GlassContainer(
        padding: const EdgeInsets.all(AppTheme.spaceM),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(a.personaName, style: text.titleMedium),
            Text(
                '${a.electionTitle}. Appointed ${AppUtils.formatDate(a.appointedAt)}.',
                style: text.bodySmall),
            const SizedBox(height: AppTheme.spaceS),
            if (a.isLive)
              Text(
                  'You accepted this role on ${AppUtils.formatDate(a.acceptedAt)}.')
            else if (!a.isPending)
              Text('This appointment has expired.', style: text.bodySmall)
            else ...[
              Text('Declaration', style: text.titleSmall),
              Text(a.declarationText),
              CheckboxListTile(
                key: Key('agree-${a.id}'),
                contentPadding: EdgeInsets.zero,
                controlAffinity: ListTileControlAffinity.leading,
                value: _agreed[a.id] ?? false,
                onChanged: (v) => setState(() => _agreed[a.id] = v ?? false),
                title: const Text(
                    'I have read this declaration and agree to it.'),
              ),
              TextField(
                controller: _reasonFor(a.id),
                maxLength: 400,
                decoration:
                    const InputDecoration(labelText: 'Reason, if you decline'),
              ),
              Wrap(
                spacing: AppTheme.spaceS,
                children: [
                  FilledButton(
                    onPressed: busy || !(_agreed[a.id] ?? false)
                        ? null
                        : () => _accept(a),
                    child: const Text('Accept'),
                  ),
                  OutlinedButton(
                    onPressed: busy ? null : () => _decline(a),
                    child: const Text('Decline'),
                  ),
                ],
              ),
            ],
          ],
        ),
      ),
    );
  }
}
