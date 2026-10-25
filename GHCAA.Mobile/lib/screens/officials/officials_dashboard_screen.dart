import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/constants/app_constants.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/logo_spinner.dart';
import '../../features/elections/election_service.dart';

final officialsElectionsProvider =
    FutureProvider.autoDispose<List<AdminElection>>((ref) async {
  return ref.read(electionServiceProvider).listAdmin();
});

final officialsBoardProvider =
    FutureProvider.autoDispose.family<ElectionBoard?, int>((ref, id) async {
  return ref.read(electionServiceProvider).getBoard(id);
});

final officialsReadinessProvider = FutureProvider.autoDispose
    .family<ElectionReadiness?, int>((ref, id) async {
  return ref.read(electionServiceProvider).getReadiness(id);
});

// Spec 023 (37.12h). One card per election. A line shows only when the
// caller's permissions on that election allow it.
class OfficialsDashboardScreen extends ConsumerWidget {
  const OfficialsDashboardScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final electionsAsync = ref.watch(officialsElectionsProvider);

    return AppScaffold(
      title: 'Election Officials',
      breadcrumb: 'OFFICIALS',
      child: electionsAsync.when(
        loading: () => const Center(child: LogoSpinner(size: 120)),
        error: (e, s) => const EmptyStateWidget(
            'Elections are unavailable. Try again later.',
            icon: Icons.error_outline),
        data: (list) => list.isEmpty
            ? const EmptyStateWidget('You have no live election appointment.',
                icon: Icons.how_to_vote_outlined)
            : RefreshIndicator(
                color: AppTheme.royalGold,
                onRefresh: () async => ref.invalidate(officialsElectionsProvider),
                child: ListView(
                  padding: const EdgeInsets.all(AppTheme.spaceM),
                  children: [for (final e in list) _ElectionCard(election: e)],
                ),
              ),
      ),
    );
  }
}

class _ElectionCard extends ConsumerWidget {
  final AdminElection election;
  const _ElectionCard({required this.election});

  bool _can(String permission) => election.myPermissions.contains(permission);

  // 37.12w. The list matters only until polling opens.
  bool get _showReadiness =>
      _can('ViewDashboard') &&
      electionPhaseOrder.contains(election.phase) &&
      electionPhaseOrder.indexOf(election.phase) <
          electionPhaseOrder.indexOf(ElectionPhases.polling);

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final text = Theme.of(context).textTheme;
    final board = ref.watch(officialsBoardProvider(election.id)).value;
    final cast = board?.ballotsCast;
    final roll = board?.rollSize;
    final readiness = _showReadiness
        ? ref.watch(officialsReadinessProvider(election.id)).value
        : null;

    return Padding(
      padding: const EdgeInsets.only(bottom: AppTheme.spaceM),
      child: GlassContainer(
        padding: const EdgeInsets.all(AppTheme.spaceM),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(election.title, style: text.titleMedium),
            Text('Phase: ${election.phase}', key: const Key('card-phase')),
            if (_can('SetBallotKey') || _can('ViewDashboard'))
              Text(
                  election.ballotKeyFingerprint == null
                      ? 'No ballot key set yet.'
                      : 'Key fingerprint ${election.ballotKeyFingerprint}',
                  key: const Key('card-key')),
            if (_can('ViewDashboard'))
              Text('Roll size: ${election.eligibleVoterCount}',
                  key: const Key('card-roll')),
            // The board sends no turnout while polling is open.
            if (_can('ViewDashboard') && cast != null && roll != null)
              Text('Ballots cast: $cast of $roll', key: const Key('card-turnout')),
            if (readiness != null) ...[
              const SizedBox(height: AppTheme.spaceS),
              Text(
                  readiness.ready
                      ? 'Every item is met. Polling can open.'
                      : 'Polling cannot open until each item below is met.',
                  key: const Key('card-readiness')),
              for (final item in readiness.items) ...[
                Text(
                    '${item.met ? 'Done' : (item.advisory ? 'Warning' : 'Not yet')}: '
                    '${electionReadinessLabel(item.key, met: item.met)}',
                    key: Key('readiness-${item.key}')),
                if (item.attestation && !item.met && _can('ChangePhase'))
                  _AttestButton(electionId: election.id, condition: item.key),
              ],
            ],
            const SizedBox(height: AppTheme.spaceS),
            Wrap(
              spacing: AppTheme.spaceS,
              children: [
                FilledButton(
                  onPressed: () =>
                      context.push('/officials/elections/${election.id}'),
                  child: const Text('Open election'),
                ),
                if (_can('Approve') || _can('ViewDashboard'))
                  OutlinedButton(
                    key: const Key('card-approvals'),
                    onPressed: () => context
                        .push('/officials/approvals?electionId=${election.id}'),
                    child: const Text('Approvals'),
                  ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

// 84.16. Records one attested readiness item. The route needs step-up, which
// the interceptor asks for. A 409 means someone else recorded it first.
class _AttestButton extends ConsumerStatefulWidget {
  final int electionId;
  final String condition;
  const _AttestButton({required this.electionId, required this.condition});

  @override
  ConsumerState<_AttestButton> createState() => _AttestButtonState();
}

class _AttestButtonState extends ConsumerState<_AttestButton> {
  bool _busy = false;

  Future<void> _attest() async {
    setState(() => _busy = true);
    final messenger = ScaffoldMessenger.of(context);
    try {
      await ref
          .read(electionServiceProvider)
          .attestReadiness(widget.electionId, widget.condition);
      messenger.showSnackBar(
          const SnackBar(content: Text('Recorded in the audit log.')));
    } on DioException catch (e) {
      if (e.response?.statusCode != 409) {
        messenger.showSnackBar(
            const SnackBar(content: Text('Could not record the item.')));
      }
    } finally {
      if (mounted) setState(() => _busy = false);
      ref.invalidate(officialsReadinessProvider(widget.electionId));
    }
  }

  @override
  Widget build(BuildContext context) => Align(
        alignment: Alignment.centerLeft,
        child: TextButton(
          key: Key('attest-${widget.condition}'),
          onPressed: _busy ? null : _attest,
          child: const Text('Record as done'),
        ),
      );
}
