import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/logo_spinner.dart';
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
      child: election.when(
        loading: () =>
            const Center(child: LogoSpinner(size: 56, ripple: false)),
        error: (_, __) => const EmptyStateWidget(
            'Election data is unavailable.',
            icon: Icons.how_to_vote_outlined),
        data: (current) => current == null
            ? const EmptyStateWidget('No election is open.',
                icon: Icons.how_to_vote_outlined)
            : _ElectionBody(election: current),
      ),
    );
  }
}

class _ElectionBody extends ConsumerStatefulWidget {
  final ElectionSummary election;
  const _ElectionBody({required this.election});
  @override
  ConsumerState<_ElectionBody> createState() => _ElectionBodyState();
}

class _ElectionBodyState extends ConsumerState<_ElectionBody> {
  late Future<List<Nomination>> _nominations;
  bool _voting = false;

  @override
  void initState() {
    super.initState();
    _nominations =
        ref.read(electionServiceProvider).getNominations(widget.election.id);
  }

  Future<void> _vote(Nomination nomination) async {
    if (_voting) return;
    setState(() => _voting = true);
    try {
      await ref.read(electionServiceProvider).vote(widget.election.id,
          electionSeatId: nomination.electionSeatId,
          nominationId: nomination.id);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('Your vote was recorded.')));
      }
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('The vote could not be recorded.')));
      }
    } finally {
      if (mounted) setState(() => _voting = false);
    }
  }

  @override
  Widget build(BuildContext context) => FutureBuilder<List<Nomination>>(
        future: _nominations,
        builder: (context, snapshot) {
          if (!snapshot.hasData) {
            return const Center(child: LogoSpinner(size: 56, ripple: false));
          }
          final nominations = snapshot.data!;
          if (nominations.isEmpty) {
            return const EmptyStateWidget(
                'No candidates have been published yet.',
                icon: Icons.people_outline);
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
                        Text(
                            'Phase: ${electionPhaseLabel(widget.election.phase)}',
                            style: const TextStyle(color: AppTheme.textMuted)),
                        if (widget.election.pollingOpensOn != null)
                          Text(
                              'Voting: ${AppUtils.formatDate(widget.election.pollingOpensOn)}',
                              style:
                                  const TextStyle(color: AppTheme.textMuted)),
                      ])),
              const SizedBox(height: AppTheme.spaceL),
              ...nominations.map((nomination) => Card(
                    child: ListTile(
                      title: Text('Candidate #${nomination.candidateMemberId}'),
                      subtitle: Text(nomination.statement),
                      leading: nomination.photoPath == null
                          ? const CircleAvatar(
                              child: Icon(Icons.person_outline))
                          : CircleAvatar(
                              backgroundImage: NetworkImage(
                                  AppConfig.resolveImageUrl(
                                      nomination.photoPath!)!),
                            ),
                      trailing: nomination.status == 'Accepted'
                          ? IconButton(
                              icon: const Icon(Icons.how_to_vote),
                              onPressed:
                                  _voting ? null : () => _vote(nomination))
                          : Text(nominationStatusLabel(nomination.status)),
                    ),
                  )),
            ],
          );
        },
      );
}
