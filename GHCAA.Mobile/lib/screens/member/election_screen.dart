import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/theme/app_theme.dart';
import '../../core/config/app_config.dart';
import '../../core/utils/app_utils.dart';
import '../../features/elections/election_service.dart';

class ElectionScreen extends ConsumerWidget {
  const ElectionScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final election = ref.watch(currentElectionProvider);
    return AppScaffold(
      title: 'Association Election',
      breadcrumb: 'ASSOCIATION > ELECTION',
      child: AsyncValueWidget<ElectionSummary?>(
        value: election,
        onRetry: () => ref.invalidate(currentElectionProvider),
        data: (current) => current == null
            ? const EmptyStateWidget('No election is open.',
                icon: Icons.how_to_vote_outlined)
            : _ElectionBody(election: current),
      ),
    );
  }
}

// Loads nominations for the election passed down from ElectionScreen, keyed
// by election id so switching elections doesn't reuse a stale cache entry.
final _nominationsProvider = FutureProvider.autoDispose
    .family<List<Nomination>, int>((ref, electionId) async {
  return ref.read(electionServiceProvider).getNominations(electionId);
});

class _ElectionBody extends ConsumerStatefulWidget {
  final ElectionSummary election;
  const _ElectionBody({required this.election});
  @override
  ConsumerState<_ElectionBody> createState() => _ElectionBodyState();
}

class _ElectionBodyState extends ConsumerState<_ElectionBody> {
  int? _votingNominationId;
  final Set<int> _votedSeatIds = {};

  Future<void> _vote(Nomination nomination) async {
    if (_votingNominationId != null) return;
    setState(() => _votingNominationId = nomination.id);
    try {
      await ref.read(electionServiceProvider).vote(widget.election.id,
          electionSeatId: nomination.electionSeatId,
          nominationId: nomination.id);
      if (mounted) {
        setState(() => _votedSeatIds.add(nomination.electionSeatId));
        ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('Your vote was recorded.')));
      }
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('The vote could not be recorded.')));
      }
    } finally {
      if (mounted) setState(() => _votingNominationId = null);
    }
  }

  @override
  Widget build(BuildContext context) {
    final nominationsAsync =
        ref.watch(_nominationsProvider(widget.election.id));
    final isPolling = widget.election.phase == 'Polling';

    return AsyncValueWidget<List<Nomination>>(
      value: nominationsAsync,
      onRetry: () =>
          ref.invalidate(_nominationsProvider(widget.election.id)),
      data: (nominations) {
        if (nominations.isEmpty) {
          return const EmptyStateWidget(
              'No candidates have been published yet.',
              icon: Icons.people_outline);
        }

        final bySeat = <int, List<Nomination>>{};
        for (final nomination in nominations) {
          bySeat.putIfAbsent(nomination.electionSeatId, () => []).add(nomination);
        }

        return ListView(
          padding: const EdgeInsets.all(AppTheme.spaceL),
          children: [
            GlassContainer(
                padding: const EdgeInsets.all(AppTheme.spaceL),
                child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(widget.election.title,
                          style: const TextStyle(
                              color: Colors.white,
                              fontSize: 20,
                              fontWeight: FontWeight.w800)),
                      const SizedBox(height: 8),
                      Text('Phase: ${electionPhaseLabel(widget.election.phase)}',
                          style: const TextStyle(color: AppTheme.textMuted)),
                      if (widget.election.pollingOpensOn != null)
                        Text(
                            'Voting: ${AppUtils.formatDate(widget.election.pollingOpensOn)}',
                            style: const TextStyle(color: AppTheme.textMuted)),
                    ])),
            const SizedBox(height: AppTheme.spaceL),
            for (final seatId in bySeat.keys) ...[
              Padding(
                padding: const EdgeInsets.symmetric(vertical: AppTheme.spaceS),
                child: Text('Seat #$seatId',
                    style: const TextStyle(
                        color: AppTheme.royalGold,
                        fontSize: 13,
                        fontWeight: FontWeight.w700)),
              ),
              ...bySeat[seatId]!.map((nomination) => _NominationCard(
                    nomination: nomination,
                    canVote: isPolling &&
                        nomination.status == 'Accepted' &&
                        !_votedSeatIds.contains(nomination.electionSeatId),
                    hasVotedThisSeat:
                        _votedSeatIds.contains(nomination.electionSeatId),
                    isVoting: _votingNominationId == nomination.id,
                    onVote: () => _vote(nomination),
                  )),
            ],
          ],
        );
      },
    );
  }
}

class _NominationCard extends StatelessWidget {
  final Nomination nomination;
  final bool canVote;
  final bool hasVotedThisSeat;
  final bool isVoting;
  final VoidCallback onVote;

  const _NominationCard({
    required this.nomination,
    required this.canVote,
    required this.hasVotedThisSeat,
    required this.isVoting,
    required this.onVote,
  });

  @override
  Widget build(BuildContext context) {
    final photoUrl = nomination.photoPath != null && nomination.photoPath!.isNotEmpty
        ? AppConfig.resolveImageUrl(nomination.photoPath)
        : null;

    return Card(
      child: ListTile(
        title: Text('Candidate #${nomination.candidateMemberId}'),
        subtitle: Text(nomination.statement),
        leading: photoUrl == null
            ? const CircleAvatar(child: Icon(Icons.person_outline))
            : CircleAvatar(backgroundImage: NetworkImage(photoUrl)),
        trailing: canVote
            ? IconButton(
                icon: isVoting
                    ? const SizedBox(
                        width: 20,
                        height: 20,
                        child: CircularProgressIndicator(strokeWidth: 2))
                    : const Icon(Icons.how_to_vote),
                onPressed: isVoting ? null : onVote,
              )
            : Text(hasVotedThisSeat
                ? 'Voted'
                : nominationStatusLabel(nomination.status)),
      ),
    );
  }
}
