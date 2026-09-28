import 'package:dio/dio.dart';
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
  // Seat id to the chosen nomination. A seat with no entry is cast blank.
  final Map<int, int> _choices = {};
  bool _submitting = false;
  bool _alreadyVoted = false;
  String? _trackingCode;

  bool get _closed =>
      widget.election.phase != 'Polling' ||
      _trackingCode != null ||
      _alreadyVoted;

  void _choose(Nomination nomination) {
    if (_closed) return;
    setState(() {
      if (_choices[nomination.electionSeatId] == nomination.id) {
        _choices.remove(nomination.electionSeatId);
      } else {
        _choices[nomination.electionSeatId] = nomination.id;
      }
    });
  }

  Future<void> _submit(Iterable<int> seatIds) async {
    if (_closed || _submitting) return;
    setState(() => _submitting = true);
    try {
      final code = await ref
          .read(electionServiceProvider)
          .castBallot(widget.election.id, {
        for (final seatId in seatIds)
          seatId: [if (_choices[seatId] != null) _choices[seatId]!],
      });
      if (mounted) {
        setState(() => _trackingCode = code);
        ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('Your ballot was recorded.')));
      }
    } on DioException catch (e) {
      if (!mounted) return;
      final already = e.response?.statusCode == 409;
      if (already) setState(() => _alreadyVoted = true);
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(
          content: Text(already
              ? 'You have already voted in this election.'
              : 'The ballot could not be recorded.')));
    } finally {
      if (mounted) setState(() => _submitting = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final nominationsAsync =
        ref.watch(_nominationsProvider(widget.election.id));
    final isPolling = widget.election.phase == 'Polling';

    return AsyncValueWidget<List<Nomination>>(
      value: nominationsAsync,
      onRetry: () => ref.invalidate(_nominationsProvider(widget.election.id)),
      data: (nominations) {
        if (nominations.isEmpty) {
          return const EmptyStateWidget(
              'No candidates have been published yet.',
              icon: Icons.people_outline);
        }

        final bySeat = <int, List<Nomination>>{};
        for (final nomination in nominations) {
          bySeat
              .putIfAbsent(nomination.electionSeatId, () => [])
              .add(nomination);
        }
        // Only seats with an accepted candidate go on the ballot.
        final contested = bySeat.entries
            .where((e) => e.value.any((n) => n.status == 'Accepted'))
            .map((e) => e.key)
            .toList();

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
                      Text(
                          'Phase: ${electionPhaseLabel(widget.election.phase)}',
                          style: const TextStyle(color: AppTheme.textMuted)),
                      if (widget.election.pollingOpensOn != null)
                        Text(
                            'Voting: ${AppUtils.formatDate(widget.election.pollingOpensOn)}',
                            style: const TextStyle(color: AppTheme.textMuted)),
                    ])),
            const SizedBox(height: AppTheme.spaceL),
            if (_trackingCode != null)
              _Notice(
                  title: 'Your ballot was recorded',
                  body: 'Tracking code: $_trackingCode\n'
                      'Keep this code. It shows your ballot was counted and '
                      'does not show who cast it.')
            else if (_alreadyVoted)
              const _Notice(body: 'You have already voted in this election.')
            else if (!isPolling)
              const _Notice(
                  body:
                      'Voting opens once this election reaches the polling phase.'),
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
                    canChoose: !_closed && nomination.status == 'Accepted',
                    chosen: _choices[seatId] == nomination.id,
                    onChoose: () => _choose(nomination),
                  )),
            ],
            if (!_closed && contested.isNotEmpty) ...[
              const SizedBox(height: AppTheme.spaceL),
              FilledButton.icon(
                onPressed: _submitting ? null : () => _submit(contested),
                icon: _submitting
                    ? const SizedBox(
                        width: 18,
                        height: 18,
                        child: CircularProgressIndicator(strokeWidth: 2))
                    : const Icon(Icons.how_to_vote),
                label: Text(_submitting ? 'Recording...' : 'Cast ballot'),
              ),
              const SizedBox(height: AppTheme.spaceS),
              const Text('A seat with no choice is cast blank.',
                  style: TextStyle(color: AppTheme.textMuted)),
            ],
          ],
        );
      },
    );
  }
}

class _Notice extends StatelessWidget {
  final String? title;
  final String body;
  const _Notice({this.title, required this.body});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: AppTheme.spaceL),
      child: GlassContainer(
          padding: const EdgeInsets.all(AppTheme.spaceL),
          child:
              Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            if (title != null)
              Text(title!,
                  style: const TextStyle(
                      color: Colors.white,
                      fontSize: 16,
                      fontWeight: FontWeight.w700)),
            if (title != null) const SizedBox(height: 8),
            SelectableText(body,
                style: const TextStyle(color: AppTheme.textMuted)),
          ])),
    );
  }
}

class _NominationCard extends StatelessWidget {
  final Nomination nomination;
  final bool canChoose;
  final bool chosen;
  final VoidCallback onChoose;

  const _NominationCard({
    required this.nomination,
    required this.canChoose,
    required this.chosen,
    required this.onChoose,
  });

  @override
  Widget build(BuildContext context) {
    final photoUrl =
        nomination.photoPath != null && nomination.photoPath!.isNotEmpty
            ? AppConfig.resolveImageUrl(nomination.photoPath)
            : null;

    return Card(
      child: ListTile(
        title: Text('Candidate #${nomination.candidateMemberId}'),
        subtitle: Text(nomination.statement),
        leading: photoUrl == null
            ? const CircleAvatar(child: Icon(Icons.person_outline))
            : CircleAvatar(backgroundImage: NetworkImage(photoUrl)),
        selected: chosen,
        onTap: canChoose ? onChoose : null,
        trailing: nomination.status == 'Accepted'
            ? Icon(chosen ? Icons.check_circle : Icons.radio_button_unchecked,
                color: chosen ? AppTheme.royalGold : AppTheme.textMuted)
            : Text(nominationStatusLabel(nomination.status)),
      ),
    );
  }
}
