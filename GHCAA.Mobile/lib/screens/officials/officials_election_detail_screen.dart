import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/logo_spinner.dart';
import '../../features/elections/election_service.dart';
import 'officials_dashboard_screen.dart';

final officialsAppointmentsProvider = FutureProvider.autoDispose
    .family<List<ElectionAppointment>, int>((ref, id) async {
  return ref.read(electionServiceProvider).getAppointments(id);
});

final officialsNominationsProvider =
    FutureProvider.autoDispose.family<List<Nomination>, int>((ref, id) async {
  return ref.read(electionServiceProvider).getNominations(id);
});

// One election for an official. Each section needs its own permission, as on
// the web officials area. Actions stay on the web console.
class OfficialsElectionDetailScreen extends ConsumerWidget {
  final int electionId;
  const OfficialsElectionDetailScreen({super.key, required this.electionId});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final electionsAsync = ref.watch(officialsElectionsProvider);

    return AppScaffold(
      title: 'Election',
      breadcrumb: 'OFFICIALS > ELECTION',
      child: electionsAsync.when(
        loading: () => const Center(child: LogoSpinner(size: 120)),
        error: (e, s) => const EmptyStateWidget(
            'The election is unavailable. Try again later.',
            icon: Icons.error_outline),
        data: (list) {
          final matches = list.where((e) => e.id == electionId);
          if (matches.isEmpty) {
            return const EmptyStateWidget('This election is not open to you.',
                icon: Icons.lock_outline);
          }
          return _Detail(election: matches.first);
        },
      ),
    );
  }
}

class _Detail extends ConsumerWidget {
  final AdminElection election;
  const _Detail({required this.election});

  bool _can(String permission) => election.myPermissions.contains(permission);

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final text = Theme.of(context).textTheme;
    final nominations = _can('DecideNominations')
        ? ref.watch(officialsNominationsProvider(election.id)).value
        : null;
    final appointments = _can('ViewDashboard')
        ? ref.watch(officialsAppointmentsProvider(election.id)).value
        : null;

    return ListView(
      padding: const EdgeInsets.all(AppTheme.spaceM),
      children: [
        GlassContainer(
          padding: const EdgeInsets.all(AppTheme.spaceM),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(election.title, style: text.titleMedium),
              Text('Phase: ${election.phase}'),
            ],
          ),
        ),
        if (nominations != null)
          Padding(
            padding: const EdgeInsets.only(top: AppTheme.spaceM),
            child: GlassContainer(
              padding: const EdgeInsets.all(AppTheme.spaceM),
              child: Text(
                  'Nominations waiting: ${nominations.where((n) => n.status == 'Submitted' || n.status == 'UnderScrutiny').length}',
                  key: const Key('detail-nominations')),
            ),
          ),
        if (appointments != null)
          Padding(
            padding: const EdgeInsets.only(top: AppTheme.spaceM),
            child: GlassContainer(
              padding: const EdgeInsets.all(AppTheme.spaceM),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('Appointments', style: text.titleSmall),
                  for (final a in appointments.where((a) => a.isLive))
                    Text('${a.personaName}: ${a.displayName}'),
                ],
              ),
            ),
          ),
      ],
    );
  }
}
