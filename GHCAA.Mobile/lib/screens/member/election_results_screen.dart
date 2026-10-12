import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/utils/app_utils.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../core/widgets/glass_container.dart';
import '../../features/elections/election_service.dart';

const resultsNotDeclaredText = 'Results are not declared yet.';
const trackingCodesNotPublishedText = 'Tracking codes are not published yet.';

// Both providers follow the latest declared election, so a retry on that one reloads them.
final _resultsProvider = FutureProvider.autoDispose<ElectionResults?>((ref) async {
  final election = await ref.watch(latestDeclaredElectionProvider.future);
  if (election == null) return null;
  return ref.read(electionServiceProvider).getResults(election.id);
});

final _trackingCodesProvider = FutureProvider.autoDispose<List<String>?>((ref) async {
  final election = await ref.watch(latestDeclaredElectionProvider.future);
  if (election == null) return null;
  return ref.read(electionServiceProvider).getTrackingCodes(election.id);
});

class ElectionResultsScreen extends ConsumerStatefulWidget {
  const ElectionResultsScreen({super.key});

  @override
  ConsumerState<ElectionResultsScreen> createState() =>
      _ElectionResultsScreenState();
}

class _ElectionResultsScreenState extends ConsumerState<ElectionResultsScreen> {
  String _query = '';

  @override
  Widget build(BuildContext context) {
    final results = ref.watch(_resultsProvider);
    final codes = ref.watch(_trackingCodesProvider);

    return AppScaffold(
      title: 'Election Results',
      breadcrumb: 'ASSOCIATION > ELECTION > RESULTS',
      child: ListView(
        padding: const EdgeInsets.all(AppTheme.spaceL),
        children: [
          AsyncValueWidget<ElectionResults?>(
            value: results,
            onRetry: () => ref.invalidate(latestDeclaredElectionProvider),
            data: (r) => r == null
                ? const EmptyStateWidget(resultsNotDeclaredText,
                    icon: Icons.hourglass_empty)
                : _ResultsBody(results: r),
          ),
          const SizedBox(height: AppTheme.spaceL),
          const Text('Tracking codes',
              style: TextStyle(
                  color: AppTheme.royalGold,
                  fontSize: 14,
                  fontWeight: FontWeight.w700)),
          const SizedBox(height: AppTheme.spaceS),
          AsyncValueWidget<List<String>?>(
            value: codes,
            onRetry: () => ref.invalidate(latestDeclaredElectionProvider),
            data: (list) => list == null
                ? const Text(trackingCodesNotPublishedText,
                    style: TextStyle(color: AppTheme.textMuted))
                : _codeList(list),
          ),
        ],
      ),
    );
  }

  Widget _codeList(List<String> all) {
    final shown = filterTrackingCodes(all, _query);
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        TextField(
          style: const TextStyle(color: AppTheme.textMain),
          decoration: const InputDecoration(
              labelText: 'Find your tracking code',
              prefixIcon: Icon(Icons.search)),
          onChanged: (v) => setState(() => _query = v),
        ),
        const SizedBox(height: AppTheme.spaceS),
        Text('${shown.length} of ${all.length} codes',
            style: const TextStyle(color: AppTheme.textMuted)),
        const SizedBox(height: AppTheme.spaceS),
        for (final code in shown)
          Padding(
            padding: const EdgeInsets.symmetric(vertical: 2),
            child: SelectableText(code,
                style: const TextStyle(color: AppTheme.textMain)),
          ),
      ],
    );
  }
}

class _ResultsBody extends StatelessWidget {
  final ElectionResults results;
  const _ResultsBody({required this.results});

  @override
  Widget build(BuildContext context) {
    final turnout = results.eligibleVoters == 0
        ? '0%'
        : '${(results.ballotsCast * 100 / results.eligibleVoters).toStringAsFixed(1)}%';
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        GlassContainer(
          padding: const EdgeInsets.all(AppTheme.spaceL),
          child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(results.title,
                    style: const TextStyle(
                        color: AppTheme.textMain,
                        fontSize: 20,
                        fontWeight: FontWeight.w800)),
                const SizedBox(height: 8),
                if (results.declaredOn != null)
                  Text('Declared: ${AppUtils.formatDate(results.declaredOn)}',
                      style: const TextStyle(color: AppTheme.textMuted)),
                Text(
                    'Turnout: ${results.ballotsCast} of ${results.eligibleVoters} ($turnout)',
                    style: const TextStyle(color: AppTheme.textMuted)),
                Text('Spoiled ballots: ${results.spoiledBallots}',
                    style: const TextStyle(color: AppTheme.textMuted)),
              ]),
        ),
        for (final seat in results.seats) ...[
          Padding(
            padding: const EdgeInsets.only(top: AppTheme.spaceL, bottom: 4),
            child: Text(seat.positionTitle,
                style: const TextStyle(
                    color: AppTheme.royalGold,
                    fontSize: 13,
                    fontWeight: FontWeight.w700)),
          ),
          for (final c in seat.candidates)
            Card(
              child: ListTile(
                title: Text(c.name),
                subtitle: c.isElected || c.isTie
                    ? Text([
                        if (c.isElected) 'Elected',
                        if (c.isTie) 'Tie',
                      ].join(', '))
                    : null,
                trailing: Text('${c.voteCount}',
                    style: const TextStyle(fontWeight: FontWeight.w700)),
              ),
            ),
          Padding(
            padding: const EdgeInsets.only(left: 4),
            child: Text('Abstentions: ${seat.abstentions}',
                style: const TextStyle(color: AppTheme.textMuted)),
          ),
        ],
      ],
    );
  }
}
