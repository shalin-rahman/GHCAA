import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../core/widgets/logo_spinner.dart';
import '../../core/widgets/confirm_dialog.dart';
import '../../core/utils/app_utils.dart';
import '../../features/elections/election_service.dart';

final adminElectionsProvider =
    FutureProvider.autoDispose<List<AdminElection>>((ref) async {
  return ref.read(electionServiceProvider).listAdmin();
});

class ElectionManagementScreen extends ConsumerStatefulWidget {
  const ElectionManagementScreen({super.key});

  @override
  ConsumerState<ElectionManagementScreen> createState() =>
      _ElectionManagementScreenState();
}

class _ElectionManagementScreenState
    extends ConsumerState<ElectionManagementScreen> {
  // Only one election can have an action in flight at a time, mirroring the
  // single in-flight-id pattern used on the governance registry screen.
  int? _busyElectionId;
  String? _busyAction;

  bool _isBusy(int electionId, String action) =>
      _busyElectionId == electionId && _busyAction == action;

  Future<void> _runAction(
      int electionId, String action, Future<void> Function() run) async {
    if (_busyElectionId != null) return;
    setState(() {
      _busyElectionId = electionId;
      _busyAction = action;
    });
    try {
      await run();
      ref.invalidate(adminElectionsProvider);
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context)
            .showSnackBar(SnackBar(content: Text('Action failed: $e')));
      }
    } finally {
      if (mounted) {
        setState(() {
          _busyElectionId = null;
          _busyAction = null;
        });
      }
    }
  }

  Future<void> _advancePhase(AdminElection election) async {
    final next = nextElectionPhase(election.phase);
    if (next == null) return;
    final confirmed = await showConfirmDialog(
      context,
      title: 'Advance phase',
      message:
          'Move "${election.title}" from ${electionPhaseLabel(election.phase)} to ${electionPhaseLabel(next)}?',
    );
    if (!confirmed) return;
    await _runAction(election.id, 'phase',
        () => ref.read(electionServiceProvider).setPhase(election.id, next));
  }

  Future<void> _freezeVoterRoll(AdminElection election) async {
    final confirmed = await showConfirmDialog(
      context,
      title: 'Freeze voter roll',
      message:
          'Freeze the voter roll for "${election.title}"? This snapshots eligibility and cannot be undone.',
      destructive: true,
    );
    if (!confirmed) return;
    await _runAction(election.id, 'freeze', () async {
      final count =
          await ref.read(electionServiceProvider).freezeVoterRoll(election.id);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text('$count voters frozen onto the roll.')));
      }
    });
  }

  Future<void> _count(AdminElection election) async {
    final confirmed = await showConfirmDialog(
      context,
      title: 'Count votes',
      message: 'Count votes for "${election.title}"?',
    );
    if (!confirmed) return;
    await _runAction(election.id, 'count',
        () => ref.read(electionServiceProvider).count(election.id));
  }

  Future<void> _declare(AdminElection election) async {
    final confirmed = await showConfirmDialog(
      context,
      title: 'Declare results',
      message:
          'Declare results for "${election.title}"? Winners will be recorded to the committee roll.',
      destructive: true,
    );
    if (!confirmed) return;
    await _runAction(election.id, 'declare',
        () => ref.read(electionServiceProvider).declare(election.id));
  }

  void _createElection() {
    final titleCtrl = TextEditingController();
    final ecPeriodCtrl = TextEditingController();
    final nomOpenCtrl = TextEditingController();
    final nomCloseCtrl = TextEditingController();
    final pollOpenCtrl = TextEditingController();
    final pollCloseCtrl = TextEditingController();
    bool saving = false;

    showDialog(
      context: context,
      builder: (ctx) => StatefulBuilder(
        builder: (ctx, setDialogState) => AlertDialog(
          backgroundColor: AppTheme.midnightSurface,
          title: const Text('NEW ELECTION',
              style: TextStyle(
                  color: AppTheme.royalGold,
                  fontSize: 14,
                  fontWeight: FontWeight.bold)),
          content: SingleChildScrollView(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                TextField(
                    controller: titleCtrl,
                    style: const TextStyle(color: Colors.white),
                    decoration:
                        const InputDecoration(labelText: 'Election title')),
                TextField(
                    controller: ecPeriodCtrl,
                    keyboardType: TextInputType.number,
                    style: const TextStyle(color: Colors.white),
                    decoration:
                        const InputDecoration(labelText: 'EC period id')),
                TextField(
                    controller: nomOpenCtrl,
                    style: const TextStyle(color: Colors.white),
                    decoration: const InputDecoration(
                        labelText: 'Nomination opens (YYYY-MM-DD)')),
                TextField(
                    controller: nomCloseCtrl,
                    style: const TextStyle(color: Colors.white),
                    decoration: const InputDecoration(
                        labelText: 'Nomination closes (YYYY-MM-DD)')),
                TextField(
                    controller: pollOpenCtrl,
                    style: const TextStyle(color: Colors.white),
                    decoration: const InputDecoration(
                        labelText: 'Polling opens (YYYY-MM-DD)')),
                TextField(
                    controller: pollCloseCtrl,
                    style: const TextStyle(color: Colors.white),
                    decoration: const InputDecoration(
                        labelText: 'Polling closes (YYYY-MM-DD)')),
              ],
            ),
          ),
          actions: [
            TextButton(
              onPressed: saving ? null : () => Navigator.pop(ctx),
              child:
                  const Text('CANCEL', style: TextStyle(color: Colors.white54)),
            ),
            ElevatedButton(
              style: ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold),
              onPressed: saving
                  ? null
                  : () async {
                      final ecPeriodId = int.tryParse(ecPeriodCtrl.text);
                      final nomOpen = AppUtils.parseDate(nomOpenCtrl.text);
                      final nomClose = AppUtils.parseDate(nomCloseCtrl.text);
                      final pollOpen = AppUtils.parseDate(pollOpenCtrl.text);
                      final pollClose = AppUtils.parseDate(pollCloseCtrl.text);
                      if (titleCtrl.text.trim().isEmpty ||
                          ecPeriodId == null ||
                          nomOpen == null ||
                          nomClose == null ||
                          pollOpen == null ||
                          pollClose == null) {
                        ScaffoldMessenger.of(ctx).showSnackBar(const SnackBar(
                            content: Text('Fill in every field with a valid date.')));
                        return;
                      }
                      setDialogState(() => saving = true);
                      try {
                        await ref.read(electionServiceProvider).create(
                              title: titleCtrl.text.trim(),
                              ecPeriodId: ecPeriodId,
                              nominationOpensOn: nomOpen,
                              nominationClosesOn: nomClose,
                              pollingOpensOn: pollOpen,
                              pollingClosesOn: pollClose,
                            );
                        ref.invalidate(adminElectionsProvider);
                        if (ctx.mounted) Navigator.pop(ctx);
                      } catch (e) {
                        if (ctx.mounted) {
                          ScaffoldMessenger.of(ctx).showSnackBar(
                              SnackBar(content: Text('Could not create election: $e')));
                        }
                      } finally {
                        if (ctx.mounted) setDialogState(() => saving = false);
                      }
                    },
              child: saving
                  ? const SizedBox(
                      height: 16, width: 16, child: LogoSpinner(size: 16))
                  : const Text('CREATE',
                      style: TextStyle(
                          color: Colors.black, fontWeight: FontWeight.bold)),
            ),
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final electionsAsync = ref.watch(adminElectionsProvider);

    return AppScaffold(
      isAdmin: true,
      title: 'Elections',
      breadcrumb: 'ADMIN > ELECTIONS',
      floatingActionButton: FloatingActionButton(
        backgroundColor: AppTheme.royalGold,
        onPressed: _createElection,
        child: const Icon(Icons.add, color: Colors.black),
      ),
      child: electionsAsync.when(
        loading: () => const Center(child: LogoSpinner(size: 120)),
        error: (e, s) => Center(child: Text('Sync error: $e')),
        data: (elections) => elections.isEmpty
            ? const EmptyStateWidget('No elections have been created yet.',
                icon: Icons.how_to_vote_outlined)
            : RefreshIndicator(
                color: AppTheme.royalGold,
                onRefresh: () async => ref.invalidate(adminElectionsProvider),
                child: ListView.builder(
                  padding: const EdgeInsets.all(AppTheme.spaceM),
                  itemCount: elections.length,
                  itemBuilder: (context, index) =>
                      _ElectionCard(election: elections[index], parent: this),
                ),
              ),
      ),
    );
  }
}

class _ElectionCard extends StatelessWidget {
  final AdminElection election;
  final _ElectionManagementScreenState parent;

  const _ElectionCard({required this.election, required this.parent});

  @override
  Widget build(BuildContext context) {
    final nextPhase = nextElectionPhase(election.phase);

    return Padding(
      padding: const EdgeInsets.only(bottom: AppTheme.spaceM),
      child: GlassContainer(
        padding: const EdgeInsets.all(AppTheme.spaceM),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Expanded(
                  child: Text(election.title,
                      style: const TextStyle(
                          fontWeight: FontWeight.w900,
                          color: Colors.white,
                          fontSize: 13)),
                ),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                  decoration: BoxDecoration(
                    color: Colors.white10,
                    borderRadius: BorderRadius.circular(4),
                    border: Border.all(color: AppTheme.royalGold, width: 0.5),
                  ),
                  child: Text(electionPhaseLabel(election.phase).toUpperCase(),
                      style: const TextStyle(
                          color: AppTheme.royalGold,
                          fontSize: 9,
                          fontWeight: FontWeight.w900)),
                ),
              ],
            ),
            const SizedBox(height: 8),
            Text('${election.eligibleVoterCount} eligible voters',
                style: const TextStyle(color: AppTheme.textMuted, fontSize: 11)),
            const Divider(color: Colors.white10, height: 24),
            Wrap(
              spacing: 8,
              runSpacing: 4,
              children: [
                if (nextPhase != null)
                  _actionButton(
                    label: 'Advance to ${electionPhaseLabel(nextPhase)}',
                    icon: Icons.arrow_forward,
                    busy: parent._isBusy(election.id, 'phase'),
                    onPressed: () => parent._advancePhase(election),
                  ),
                _actionButton(
                  label: 'Freeze roll',
                  icon: Icons.lock_outline,
                  busy: parent._isBusy(election.id, 'freeze'),
                  onPressed: () => parent._freezeVoterRoll(election),
                ),
                _actionButton(
                  label: 'Count',
                  icon: Icons.calculate_outlined,
                  busy: parent._isBusy(election.id, 'count'),
                  onPressed: () => parent._count(election),
                ),
                _actionButton(
                  label: 'Declare',
                  icon: Icons.campaign_outlined,
                  busy: parent._isBusy(election.id, 'declare'),
                  onPressed: () => parent._declare(election),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _actionButton({
    required String label,
    required IconData icon,
    required bool busy,
    required VoidCallback onPressed,
  }) {
    return TextButton.icon(
      onPressed: busy ? null : onPressed,
      icon: busy
          ? const SizedBox(
              width: 14,
              height: 14,
              child: CircularProgressIndicator(strokeWidth: 2))
          : Icon(icon, size: 16, color: AppTheme.royalGold),
      label: Text(label,
          style: const TextStyle(
              color: AppTheme.royalGold,
              fontSize: 11,
              fontWeight: FontWeight.bold)),
    );
  }
}
