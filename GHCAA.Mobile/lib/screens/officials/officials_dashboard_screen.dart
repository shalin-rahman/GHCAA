import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
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

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final text = Theme.of(context).textTheme;
    final board = ref.watch(officialsBoardProvider(election.id)).value;
    final cast = board?.ballotsCast;
    final roll = board?.rollSize;

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
