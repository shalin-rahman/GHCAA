import '../../core/widgets/custom_network_image.dart';
import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:printing/printing.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/theme/app_theme.dart';
import '../../core/config/app_config.dart';
import '../../core/utils/app_utils.dart';
import '../../features/elections/ballot_receipt.dart';
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
            ? const _NoOpenElection()
            : _ElectionBody(election: current),
      ),
    );
  }
}

// The paper for the election passed down from ElectionScreen, keyed by election
// id so switching elections doesn't reuse a stale cache entry.
final _ballotProvider =
    FutureProvider.autoDispose.family<Ballot, int>((ref, electionId) async {
  return ref.read(electionServiceProvider).getBallot(electionId);
});

// The officials list is a side note, so a failed call hides it instead of breaking the screen.
final _boardProvider =
    FutureProvider.autoDispose.family<ElectionBoard?, int>((ref, electionId) async {
  try {
    return await ref.read(electionServiceProvider).getBoard(electionId);
  } catch (_) {
    return null;
  }
});

// Phases after polling, where the count and the result can be looked at.
const _resultPhases = {'Counting', 'Declared', 'Archived'};

class _ElectionBody extends ConsumerStatefulWidget {
  final ElectionSummary election;
  const _ElectionBody({required this.election});
  @override
  ConsumerState<_ElectionBody> createState() => _ElectionBodyState();
}

class _ElectionBodyState extends ConsumerState<_ElectionBody> {
  // Seat id to the nominations picked. An empty list is an abstention. A seat with no entry is
  // undecided, and the review button stays off until every seat has one.
  final Map<int, List<int>> _picks = {};
  bool _reviewing = false;
  bool _submitting = false;
  bool _alreadyVoted = false;
  String? _trackingCode;
  // Kept in memory only so the receipt can be built from the confirmation view.
  List<({String position, String choice})>? _receiptLines;

  bool _closed(Ballot ballot) =>
      ballot.phase != 'Polling' ||
      _trackingCode != null ||
      _alreadyVoted ||
      ballot.hasVoted ||
      ballot.isEligible == false;

  void _pick(Ballot ballot, BallotSeat seat, BallotCandidate candidate) {
    if (_closed(ballot)) return;
    setState(() {
      final next = togglePick(
          _picks[seat.seatId] ?? const [], candidate.nominationId, seat.places);
      // Clearing the last pick leaves the seat undecided, not abstained.
      if (next.isEmpty) {
        _picks.remove(seat.seatId);
      } else {
        _picks[seat.seatId] = next;
      }
    });
  }

  void _abstain(Ballot ballot, BallotSeat seat) {
    if (_closed(ballot)) return;
    setState(() => _picks[seat.seatId] = []);
  }

  Future<void> _submit(Ballot ballot) async {
    // buildBallotChoices turns a missing seat into an abstention, so an undecided seat must not get this far.
    if (_submitting || _closed(ballot) || !allSeatsDecided(ballot.seats, _picks)) return;
    setState(() => _submitting = true);
    final choices = buildBallotChoices(ballot.seats, _picks);
    try {
      final code = await ref
          .read(electionServiceProvider)
          .castBallot(widget.election.id, choices);
      if (mounted) {
        setState(() {
          _trackingCode = code;
          _receiptLines = ballotReceiptLines(ballot.seats, choices);
          _reviewing = false;
        });
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
              : (e.message ?? 'The ballot could not be recorded.'))));
    } finally {
      if (mounted) setState(() => _submitting = false);
    }
  }

  Future<void> _saveReceipt(Ballot ballot) async {
    final lines = _receiptLines;
    final code = _trackingCode;
    if (lines == null || code == null) return;
    final bytes = await buildBallotReceiptPdf(
      electionTitle: ballot.title,
      dateText: AppUtils.formatDate(DateTime.now()),
      trackingCode: code,
      lines: lines,
    );
    await Printing.sharePdf(bytes: bytes, filename: 'ballot-receipt.pdf');
  }

  @override
  Widget build(BuildContext context) {
    final ballotAsync = ref.watch(_ballotProvider(widget.election.id));

    return AsyncValueWidget<Ballot>(
      value: ballotAsync,
      onRetry: () => ref.invalidate(_ballotProvider(widget.election.id)),
      data: (ballot) {
        final closed = _closed(ballot);
        return ListView(
          padding: const EdgeInsets.all(AppTheme.spaceL),
          children: [
            _header(ballot),
            const SizedBox(height: AppTheme.spaceL),
            _OfficialsCard(electionId: widget.election.id),
            if (_trackingCode != null)
              _confirmation(ballot)
            else if (_alreadyVoted || ballot.hasVoted)
              const _Notice(body: 'You have already voted in this election.')
            else if (ballot.isEligible == false)
              _Notice(
                  title: 'You cannot vote in this election',
                  body: '${ballot.ineligibilityReason ?? ballotNotOnRoll}\n'
                      'If you think this is wrong, contact the election commission before polling closes.')
            else if (ballot.phase != 'Polling')
              const _Notice(
                  body:
                      'Voting opens once this election reaches the polling phase.'),
            if (_resultPhases.contains(ballot.phase)) ...[
              OutlinedButton.icon(
                onPressed: () =>
                    context.push('/election/results'),
                icon: const Icon(Icons.bar_chart),
                label: const Text('Results and tracking codes'),
              ),
              const SizedBox(height: AppTheme.spaceL),
            ],
            if (!closed && ballot.seats.isEmpty)
              const EmptyStateWidget('No candidates have been published yet.',
                  icon: Icons.people_outline)
            else if (!closed && _reviewing)
              ..._review(ballot)
            else if (!closed)
              ..._paper(ballot),
          ],
        );
      },
    );
  }

  Widget _header(Ballot ballot) {
    final election = widget.election;
    return GlassContainer(
        padding: const EdgeInsets.all(AppTheme.spaceL),
        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Text(ballot.title,
              style: const TextStyle(
                  color: AppTheme.textMain,
                  fontSize: 20,
                  fontWeight: FontWeight.w800)),
          const SizedBox(height: 8),
          Text('Phase: ${electionPhaseLabel(ballot.phase)}',
              style: const TextStyle(color: AppTheme.textMuted)),
          if (election.pollingOpensOn != null)
            Text('Voting: ${AppUtils.formatDate(election.pollingOpensOn)}',
                style: const TextStyle(color: AppTheme.textMuted)),
          if (ballot.ballotKeyFingerprint != null)
            SelectableText(
                "Returning officer's key: ${ballot.ballotKeyFingerprint}. "
                'It should match the one the commission announced.',
                style: const TextStyle(color: AppTheme.textMuted)),
        ]));
  }

  Widget _confirmation(Ballot ballot) {
    return GlassContainer(
        padding: const EdgeInsets.all(AppTheme.spaceL),
        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          const Text('Your ballot was recorded',
              style: TextStyle(
                  color: AppTheme.textMain,
                  fontSize: 16,
                  fontWeight: FontWeight.w700)),
          const SizedBox(height: 8),
          SelectableText('Tracking code: $_trackingCode',
              style: const TextStyle(color: AppTheme.royalGold)),
          const SizedBox(height: 8),
          const Text(
              'Keep this code. It shows your ballot was counted and does not show who cast it.',
              style: TextStyle(color: AppTheme.textMuted)),
          const SizedBox(height: AppTheme.spaceM),
          FilledButton.icon(
            onPressed: () => _saveReceipt(ballot),
            icon: const Icon(Icons.picture_as_pdf_outlined),
            label: const Text('Save receipt'),
          ),
          const SizedBox(height: AppTheme.spaceS),
          const Text(
              'You can save the receipt only here. It is gone once you leave this screen.',
              style: TextStyle(color: AppTheme.textMuted)),
        ]));
  }

  List<Widget> _paper(Ballot ballot) {
    final decided = allSeatsDecided(ballot.seats, _picks);
    return [
      for (final seat in ballot.seats) ...[
        Padding(
          padding: const EdgeInsets.symmetric(vertical: AppTheme.spaceS),
          child: Semantics(
            header: true,
            child: Text(seat.positionTitle,
                style: const TextStyle(
                    color: AppTheme.royalGold,
                    fontSize: 13,
                    fontWeight: FontWeight.w700)),
          ),
        ),
        if (seat.places > 1)
          Padding(
            padding: const EdgeInsets.only(bottom: AppTheme.spaceS),
            child: Text(
                'Pick up to ${seat.places} (${(_picks[seat.seatId] ?? const []).length} picked)',
                style: const TextStyle(color: AppTheme.textMuted)),
          ),
        for (final candidate in seat.candidates)
          _CandidateCard(
            candidate: candidate,
            chosen: (_picks[seat.seatId] ?? const [])
                .contains(candidate.nominationId),
            single: seat.places == 1,
            onChoose: () => _pick(ballot, seat, candidate),
          ),
        _ChoiceCard(
          title: ballotAbstainOption,
          subtitle: ballotAbstainHelp,
          leading: const CircleAvatar(child: Icon(Icons.block_outlined)),
          chosen: _picks[seat.seatId]?.isEmpty == true,
          single: true,
          onChoose: () => _abstain(ballot, seat),
        ),
      ],
      const SizedBox(height: AppTheme.spaceL),
      FilledButton.icon(
        onPressed: decided ? () => setState(() => _reviewing = true) : null,
        icon: const Icon(Icons.fact_check_outlined),
        label: const Text('Review ballot'),
      ),
      if (!decided) ...[
        const SizedBox(height: AppTheme.spaceS),
        const Text(ballotUndecidedHint,
            style: TextStyle(color: AppTheme.textMuted)),
      ],
    ];
  }

  List<Widget> _review(Ballot ballot) {
    final lines = ballotReceiptLines(
        ballot.seats, buildBallotChoices(ballot.seats, _picks));
    return [
      const Text('Review your ballot',
          style: TextStyle(
              color: AppTheme.textMain,
              fontSize: 16,
              fontWeight: FontWeight.w700)),
      const SizedBox(height: AppTheme.spaceS),
      for (final line in lines)
        Card(
          child: ListTile(
            title: Text(line.position),
            subtitle: Text(line.choice),
          ),
        ),
      const SizedBox(height: AppTheme.spaceL),
      FilledButton.icon(
        onPressed: _submitting ? null : () => _submit(ballot),
        icon: _submitting
            ? const SizedBox(
                width: 18,
                height: 18,
                child: CircularProgressIndicator(strokeWidth: 2))
            : const Icon(Icons.how_to_vote),
        label: Text(_submitting ? 'Recording...' : 'Confirm and cast ballot'),
      ),
      TextButton(
        onPressed:
            _submitting ? null : () => setState(() => _reviewing = false),
        child: const Text('Back to the paper'),
      ),
    ];
  }
}

class _OfficialsCard extends ConsumerWidget {
  final int electionId;
  const _OfficialsCard({required this.electionId});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final board = ref.watch(_boardProvider(electionId)).valueOrNull;
    if (board == null || board.groups.isEmpty) return const SizedBox.shrink();
    return Padding(
      padding: const EdgeInsets.only(bottom: AppTheme.spaceL),
      child: GlassContainer(
        padding: const EdgeInsets.all(AppTheme.spaceL),
        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          const Text('Who runs this election',
              style: TextStyle(
                  color: AppTheme.textMain,
                  fontSize: 16,
                  fontWeight: FontWeight.w700)),
          for (final group in board.groups) ...[
            const SizedBox(height: AppTheme.spaceS),
            Text(group.groupName,
                style: const TextStyle(
                    color: AppTheme.royalGold,
                    fontSize: 13,
                    fontWeight: FontWeight.w700)),
            for (final member in group.members)
              Text('${member.displayName}, ${member.personaName}',
                  style: const TextStyle(color: AppTheme.textMuted)),
          ],
        ]),
      ),
    );
  }
}

// With no open election the last declared result is still worth reaching.
class _NoOpenElection extends StatelessWidget {
  const _NoOpenElection();

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.all(AppTheme.spaceL),
      children: [
        const EmptyStateWidget('No election is open.',
            icon: Icons.how_to_vote_outlined),
        const SizedBox(height: AppTheme.spaceL),
        OutlinedButton.icon(
          onPressed: () => context.push('/election/results'),
          icon: const Icon(Icons.bar_chart),
          label: const Text('Latest results'),
        ),
      ],
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
            if (title != null) ...[
              Semantics(
                header: true,
                child: Text(title!,
                    style: const TextStyle(
                        color: AppTheme.textMain,
                        fontSize: 16,
                        fontWeight: FontWeight.w700)),
              ),
              const SizedBox(height: 8),
            ],
            SelectableText(body,
                style: const TextStyle(color: AppTheme.textMuted)),
          ])),
    );
  }
}

class _CandidateCard extends StatelessWidget {
  final BallotCandidate candidate;
  final bool chosen;
  final bool single;
  final VoidCallback onChoose;

  const _CandidateCard({
    required this.candidate,
    required this.chosen,
    required this.single,
    required this.onChoose,
  });

  @override
  Widget build(BuildContext context) {
    final photoUrl =
        candidate.photoUrl != null && candidate.photoUrl!.isNotEmpty
            ? AppConfig.resolveImageUrl(candidate.photoUrl)
            : null;

    return _ChoiceCard(
      title: candidate.name,
      subtitle: candidate.statement.isEmpty ? null : candidate.statement,
      leading: photoUrl == null
          ? const CircleAvatar(child: Icon(Icons.person_outline))
          : CircleAvatar(backgroundImage: cachedImageProvider(context, photoUrl, size: 40)),
      chosen: chosen,
      single: single,
      onChoose: onChoose,
    );
  }
}

// One option on the paper. The checked state and the radio grouping are set so a screen
// reader says "checked" or "not checked", the way it does for the inputs on web.
class _ChoiceCard extends StatelessWidget {
  final String title;
  final String? subtitle;
  final Widget leading;
  final bool chosen;
  final bool single;
  final VoidCallback onChoose;

  const _ChoiceCard({
    required this.title,
    this.subtitle,
    required this.leading,
    required this.chosen,
    required this.single,
    required this.onChoose,
  });

  @override
  Widget build(BuildContext context) {
    return MergeSemantics(
      child: Semantics(
        checked: chosen,
        inMutuallyExclusiveGroup: single,
        child: Card(
          child: ListTile(
            title: Text(title),
            subtitle: subtitle == null ? null : Text(subtitle!),
            leading: leading,
            selected: chosen,
            onTap: onChoose,
            trailing: Icon(
                single
                    ? (chosen ? Icons.check_circle : Icons.radio_button_unchecked)
                    : (chosen ? Icons.check_box : Icons.check_box_outline_blank),
                color: chosen ? AppTheme.royalGold : AppTheme.textMuted),
          ),
        ),
      ),
    );
  }
}
