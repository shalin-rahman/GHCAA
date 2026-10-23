import 'package:flutter/material.dart';
import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/constants/app_constants.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../core/widgets/logo_spinner.dart';
import '../../core/widgets/confirm_dialog.dart';
import '../../core/utils/app_utils.dart';
import '../../features/admin/admin_service.dart';
import '../../features/auth/auth_service.dart';
import '../../features/elections/ballot_key.dart';
import '../../features/elections/election_service.dart';
import 'election_form_records_sheet.dart';
import 'election_officials_sheet.dart';
import 'election_rules_unlock_sheet.dart';

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
    final warning = pollingApproverWarning(election, next);
    final confirmed = await showConfirmDialog(
      context,
      title: 'Advance phase',
      message:
          'Move "${election.title}" from ${electionPhaseLabel(election.phase)} to ${electionPhaseLabel(next)}?'
          '${warning == null ? '' : '\n\n$warning'}',
      destructive: warning != null,
    );
    if (!confirmed) return;
    await _runAction(election.id, 'phase', () async {
      _tellIfWaiting(
          await ref.read(electionServiceProvider).setPhase(election.id, next));
    });
  }

  void _tellIfWaiting(bool waiting) {
    if (!waiting || !mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(const SnackBar(
        content: Text('Saved. A second official must approve this under '
            'Approvals before it happens.')));
  }

  void _tell(String message) {
    if (!mounted) return;
    ScaffoldMessenger.of(context)
        .showSnackBar(SnackBar(content: Text(message)));
  }

  Future<void> _publish(AdminElection election) async {
    final confirmed = await showConfirmDialog(
      context,
      title: 'Publish election',
      message: 'Publish "${election.title}" and open nominations?',
    );
    if (!confirmed) return;
    await _runAction(election.id, 'phase', () async {
      _tellIfWaiting(
          await ref.read(electionServiceProvider).publish(election.id));
    });
  }

  Future<void> _close(AdminElection election) async {
    final confirmed = await showConfirmDialog(
      context,
      title: 'Close polling',
      message: 'Close "${election.title}"? Voting will no longer be accepted.',
      destructive: true,
    );
    if (!confirmed) return;
    await _runAction(election.id, 'phase', () async {
      _tellIfWaiting(await ref.read(electionServiceProvider).close(election.id));
    });
  }

  // The server refuses with 409 once a candidate has votes.
  Future<void> _removeCandidate(
      AdminElection election, AdminElectionCandidate candidate) async {
    final confirmed = await showConfirmDialog(
      context,
      title: 'Remove candidate',
      message: 'Remove this candidate from the election?',
      destructive: true,
    );
    if (!confirmed) return;
    await _runAction(election.id, 'candidate-${candidate.id}', () async {
      try {
        await ref
            .read(electionServiceProvider)
            .removeCandidate(election.id, candidate.id);
      } on DioException catch (e) {
        if (e.response?.statusCode != 409) rethrow;
        _tell('This candidate has votes and cannot be removed.');
      }
    });
  }

  Future<void> _keyDestroyed(AdminElection election) async {
    final confirmed = await showConfirmDialog(
      context,
      title: 'Key destroyed',
      message: 'Record that every copy of the returning officer key for '
          '"${election.title}" has been destroyed?',
      destructive: true,
    );
    if (!confirmed) return;
    await _runAction(election.id, 'key', () async {
      try {
        await ref.read(electionServiceProvider).keyDestroyed(election.id);
        _tell('Recorded in the audit log.');
      } on DioException catch (e) {
        if (e.response?.statusCode != 409) rethrow;
        _tell('The key was already recorded as destroyed.');
      }
    });
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

  Future<void> _unfreezeVoterRoll(AdminElection election) async {
    final reason = await _askReason('Unfreeze voter roll');
    if (reason == null) return;
    await _runAction(election.id, 'freeze', () async {
      final removed = await ref
          .read(electionServiceProvider)
          .unfreezeVoterRoll(election.id, reason);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text('Voter roll unfrozen. $removed removed.')));
      }
    });
  }

  Future<String?> _askReason(String title) async {
    final ctrl = TextEditingController();
    final ok = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(title),
        content: TextField(
          controller: ctrl,
          autofocus: true,
          maxLines: 3,
          maxLength: 400,
          decoration: const InputDecoration(
              labelText: 'Reason',
              helperText: ElectionConstants.unfreezeReasonHint,
              helperMaxLines: 5),
        ),
        actions: [
          TextButton(
              onPressed: () => Navigator.pop(ctx, false),
              child: const Text('CANCEL')),
          ElevatedButton(
              onPressed: () => Navigator.pop(ctx, true),
              child: const Text('CONTINUE')),
        ],
      ),
    );
    final reason = ctrl.text.trim();
    ctrl.dispose();
    if (ok != true) return null;
    if (reason.isEmpty && mounted) {
      ScaffoldMessenger.of(context)
          .showSnackBar(const SnackBar(content: Text('A reason is required.')));
      return null;
    }
    return reason;
  }

  // 84.44. Same steps as createBallotKey on the web admin. Only the public key
  // goes to the server. The private key lives in local variables until the
  // file is saved.
  Future<void> _makeBallotKey(AdminElection election) async {
    if (_busyElectionId != null) return;
    final replacing = election.ballotKeyFingerprint != null;
    final confirmed = await showConfirmDialog(
      context,
      title: replacing
          ? 'Replace returning officer key'
          : 'Make returning officer key',
      message: 'A key file will be saved to this device. Only the returning '
          'officer should keep it. The count cannot run without it, and '
          'nobody, including the system, can recover it if it is lost.',
      confirmLabel: 'Make key',
    );
    if (!confirmed || !mounted) return;
    final tools = ref.read(ballotKeyToolsProvider);
    await _runAction(election.id, 'ballot-key', () async {
      final BallotKeyPair pair;
      try {
        pair = await _withProgress('Making the key. This can take a minute.',
            tools.generate());
      } catch (_) {
        _tell('This device could not make the key.');
        return;
      }
      // Save before the server takes the public key, so a cancelled or
      // failed save never seals the election under a key nobody has.
      final bool saved;
      try {
        saved = await tools.save(ballotKeyFileName(election.title),
            ballotKeyPem(pair.privateKey));
      } catch (_) {
        _tell('The key file could not be saved. Nothing was sent.');
        return;
      }
      if (!saved) {
        _tell('Key file not saved. Nothing was sent.');
        return;
      }
      try {
        final waiting = await ref
            .read(electionServiceProvider)
            .setBallotKey(election.id, pair.publicKey);
        if (waiting) {
          // The old key stays in force until a second person approves.
          _tell('Saved. A second person must approve the new key. Keep both '
              'key files until then.');
        } else {
          _tell('Key stored. Keep the saved file safe until the count.');
        }
      } on DioException catch (e) {
        _tell('The key was not stored: ${_reason(e)}');
      }
    });
  }

  Future<T> _withProgress<T>(String message, Future<T> work) async {
    showDialog<void>(
      context: context,
      barrierDismissible: false,
      builder: (_) => PopScope(
        canPop: false,
        child: AlertDialog(
          key: const Key('ballot-key-progress'),
          content: Row(
            children: [
              const SizedBox(
                  height: 24, width: 24, child: LogoSpinner(size: 24)),
              const SizedBox(width: 16),
              Expanded(child: Text(message)),
            ],
          ),
        ),
      ),
    );
    try {
      return await work;
    } finally {
      if (mounted) Navigator.of(context, rootNavigator: true).pop();
    }
  }

  // 84.44. Same call as countWithKeyFile on the web admin. The server opens
  // the sealed ballots, so the key read from the file goes in the request
  // body. The step-up interceptor asks for a code and replays the call.
  Future<void> _count(AdminElection election) async {
    if (_busyElectionId != null) return;
    final confirmed = await showConfirmDialog(
      context,
      title: 'Count votes',
      message: 'Count votes for "${election.title}"? Choose the returning '
          "officer's key file next. After the first count, the same file "
          'shows the stored results.',
      confirmLabel: 'Choose key file',
    );
    if (!confirmed || !mounted) return;
    final tools = ref.read(ballotKeyToolsProvider);
    await _runAction(election.id, 'count', () async {
      final String? text;
      try {
        text = await tools.pick();
      } catch (_) {
        _tell('The key file could not be read.');
        return;
      }
      if (text == null) return;
      try {
        final run = await ref
            .read(electionServiceProvider)
            .runCount(election.id, readBallotKeyFile(text));
        if (run.results != null) {
          _tell('Count done. ${run.results!.length} result rows stored.');
        } else {
          _tell('Saved. A second person must approve the count. Then count '
              'again with the same key file.');
        }
      } on DioException catch (e) {
        _tell('Count failed: ${_reason(e)}');
      }
    });
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
    await _runAction(election.id, 'declare', () async {
      _tellIfWaiting(
          await ref.read(electionServiceProvider).declare(election.id));
    });
  }

  // The server's detail text for a failed call (the API client puts it in message).
  String _reason(Object e) =>
      e is DioException ? (e.message ?? e.toString()) : e.toString();

  Future<void> _createElection() async {
    final List<dynamic> periods;
    try {
      periods = await ref.read(adminServiceProvider).getECPeriods();
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text('Could not load EC periods: ${_reason(e)}')));
      }
      return;
    }
    if (!mounted) return;
    int? ecPeriodId;
    final titleCtrl = TextEditingController();
    final nomOpenCtrl = TextEditingController();
    final nomCloseCtrl = TextEditingController();
    final pollOpenCtrl = TextEditingController();
    final pollCloseCtrl = TextEditingController();
    final scrutinyCtrl = TextEditingController();
    final withdrawCtrl = TextEditingController();
    ElectionTieRule tieRule = ElectionTieRule.drawingLots;
    bool isTest = false;
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
                DropdownButtonFormField<int>(
                    initialValue: ecPeriodId,
                    isExpanded: true,
                    decoration: const InputDecoration(labelText: 'EC period'),
                    items: [
                      for (final p in periods)
                        DropdownMenuItem<int>(
                            value: p['id'] as int,
                            child: Text((p['title'] ?? 'EC period').toString(),
                                overflow: TextOverflow.ellipsis)),
                    ],
                    onChanged: (v) => setDialogState(() => ecPeriodId = v)),
                TextField(
                    controller: nomOpenCtrl,
                    style: const TextStyle(color: Colors.white),
                    decoration: const InputDecoration(
                        labelText: 'Nomination opens (YYYY-MM-DD)',
                        helperText: ElectionConstants.nominationOpensHint,
                        helperMaxLines: 3)),
                TextField(
                    controller: nomCloseCtrl,
                    style: const TextStyle(color: Colors.white),
                    decoration: const InputDecoration(
                        labelText: 'Nomination closes (YYYY-MM-DD)',
                        helperText: ElectionConstants.nominationClosesHint,
                        helperMaxLines: 3)),
                TextField(
                    controller: scrutinyCtrl,
                    style: const TextStyle(color: Colors.white),
                    decoration: const InputDecoration(
                        labelText: 'Scrutiny date, optional (YYYY-MM-DD)',
                        helperText: ElectionConstants.scrutinyHint,
                        helperMaxLines: 3)),
                TextField(
                    controller: withdrawCtrl,
                    style: const TextStyle(color: Colors.white),
                    decoration: const InputDecoration(
                        labelText: 'Withdrawal closes, optional (YYYY-MM-DD)',
                        helperText: ElectionConstants.withdrawalClosesHint,
                        helperMaxLines: 3)),
                TextField(
                    controller: pollOpenCtrl,
                    style: const TextStyle(color: Colors.white),
                    decoration: const InputDecoration(
                        labelText: 'Polling opens (YYYY-MM-DD)',
                        helperText: ElectionConstants.pollingOpensHint,
                        helperMaxLines: 3)),
                TextField(
                    controller: pollCloseCtrl,
                    style: const TextStyle(color: Colors.white),
                    decoration: const InputDecoration(
                        labelText: 'Polling closes (YYYY-MM-DD)',
                        helperText: ElectionConstants.pollingClosesHint,
                        helperMaxLines: 3)),
                DropdownButtonFormField<ElectionTieRule>(
                    initialValue: tieRule,
                    isExpanded: true,
                    decoration: const InputDecoration(labelText: 'If votes tie'),
                    selectedItemBuilder: (_) => [
                          for (final r in ElectionTieRule.values)
                            Align(
                                alignment: Alignment.centerLeft,
                                child: Text(r.label,
                                    overflow: TextOverflow.ellipsis)),
                        ],
                    items: [
                      for (final r in ElectionTieRule.values)
                        DropdownMenuItem<ElectionTieRule>(
                            value: r,
                            child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                mainAxisSize: MainAxisSize.min,
                                children: [
                                  Text(r.label),
                                  Text(r.description,
                                      style: const TextStyle(
                                          color: AppTheme.textMuted,
                                          fontSize: 11)),
                                ])),
                    ],
                    onChanged: (v) => setDialogState(
                        () => tieRule = v ?? ElectionTieRule.drawingLots)),
                Align(
                    alignment: Alignment.centerLeft,
                    child: Padding(
                        padding: const EdgeInsets.only(top: 4),
                        child: Text(tieRule.description,
                            style: const TextStyle(
                                color: AppTheme.textMuted, fontSize: 11)))),
                SwitchListTile(
                    contentPadding: EdgeInsets.zero,
                    value: isTest,
                    activeThumbColor: AppTheme.royalGold,
                    title: const Text('Test run',
                        style: TextStyle(color: Colors.white, fontSize: 13)),
                    subtitle: const Text(ElectionConstants.testRunHint,
                        style: TextStyle(color: AppTheme.textMuted, fontSize: 11)),
                    onChanged: (v) => setDialogState(() => isTest = v)),
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
                      final nomOpen = AppUtils.parseDate(nomOpenCtrl.text);
                      final nomClose = AppUtils.parseDate(nomCloseCtrl.text);
                      final pollOpen = AppUtils.parseDate(pollOpenCtrl.text);
                      final pollClose = AppUtils.parseDate(pollCloseCtrl.text);
                      final scrutiny = AppUtils.parseDate(scrutinyCtrl.text);
                      final withdraw = AppUtils.parseDate(withdrawCtrl.text);
                      final optionalBad =
                          (scrutinyCtrl.text.trim().isNotEmpty && scrutiny == null) ||
                              (withdrawCtrl.text.trim().isNotEmpty && withdraw == null);
                      if (optionalBad) {
                        ScaffoldMessenger.of(ctx).showSnackBar(const SnackBar(
                            content: Text('Enter the scrutiny and withdrawal dates as YYYY-MM-DD, or leave them empty.')));
                        return;
                      }
                      if (titleCtrl.text.trim().isEmpty ||
                          ecPeriodId == null ||
                          nomOpen == null ||
                          nomClose == null ||
                          pollOpen == null ||
                          pollClose == null) {
                        ScaffoldMessenger.of(ctx).showSnackBar(const SnackBar(
                            content: Text('Choose an EC period and fill in every date.')));
                        return;
                      }
                      setDialogState(() => saving = true);
                      try {
                        await ref.read(electionServiceProvider).create(
                              title: titleCtrl.text.trim(),
                              ecPeriodId: ecPeriodId!,
                              nominationOpensOn: nomOpen,
                              nominationClosesOn: nomClose,
                              pollingOpensOn: pollOpen,
                              pollingClosesOn: pollClose,
                              isTest: isTest,
                              tieRule: tieRule,
                              scrutinyOn: scrutiny,
                              withdrawalClosesOn: withdraw,
                            );
                        ref.invalidate(adminElectionsProvider);
                        if (ctx.mounted) Navigator.pop(ctx);
                      } catch (e) {
                        if (ctx.mounted) {
                          ScaffoldMessenger.of(ctx).showSnackBar(SnackBar(
                              content: Text('Could not create election: ${_reason(e)}')));
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
    final isSuperAdmin = ref.watch(isSuperAdminProvider).value ?? false;

    return AppScaffold(
      isAdmin: true,
      title: 'Elections',
      breadcrumb: 'ADMIN > ELECTIONS',
      actions: [
        if (isSuperAdmin)
          IconButton(
            tooltip: 'Rules unlock',
            icon: const Icon(Icons.lock_open_outlined, color: AppTheme.royalGold),
            onPressed: () => showElectionRulesUnlockSheet(context),
          ),
      ],
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

  // 84.16. The same gates the web admin uses. The server checks them again.
  bool _can(String permission) => election.myPermissions.contains(permission);
  bool _at(String phase) => election.phase == phase;

  // The key can be made or replaced up to the day polling opens.
  bool get _canSetBallotKey =>
      electionPhaseOrder.indexOf(election.phase) <
      electionPhaseOrder.indexOf(ElectionPhases.polling);

  // Publish and close have their own routes, and counting ends with declare.
  bool get _canAdvance =>
      _can('ChangePhase') &&
      !_at(ElectionPhases.announced) &&
      !_at(ElectionPhases.polling) &&
      !_at(ElectionPhases.counting);

  bool get _canEditRoll =>
      _can('ManageVoterRoll') &&
      (_at(ElectionPhases.announced) || _at(ElectionPhases.nomination));

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
            if (election.isTest) ...[
              const SizedBox(height: 4),
              const Text('Test run. Members and the public do not see it.',
                  style: TextStyle(color: AppTheme.royalGold, fontSize: 11)),
            ],
            const SizedBox(height: 4),
            Text(
                election.ballotKeyFingerprint == null
                    ? 'No returning officer key yet. Voting cannot open without one.'
                    : 'Officer key ${election.ballotKeyFingerprint}',
                style: const TextStyle(color: AppTheme.textMuted, fontSize: 11)),
            if (election.candidates.isNotEmpty) ...[
              const SizedBox(height: 8),
              for (final c in election.candidates)
                Row(
                  key: Key('candidate-${c.id}'),
                  children: [
                    Expanded(
                      child: Text('${c.positionTitle}: ${c.name}',
                          style: const TextStyle(
                              color: Colors.white, fontSize: 11)),
                    ),
                    if (_can('ManageSetup'))
                      _actionButton(
                        key: Key('candidate-${c.id}-remove'),
                        label: 'Remove',
                        icon: Icons.person_remove_outlined,
                        busy: parent._isBusy(election.id, 'candidate-${c.id}'),
                        onPressed: () => parent._removeCandidate(election, c),
                      ),
                  ],
                ),
            ],
            const Divider(color: Colors.white10, height: 24),
            Wrap(
              spacing: 8,
              runSpacing: 4,
              children: [
                if (_at(ElectionPhases.announced) && _can('ChangePhase'))
                  _actionButton(
                    key: const Key('action-publish'),
                    label: 'Publish',
                    icon: Icons.public,
                    busy: parent._isBusy(election.id, 'phase'),
                    onPressed: () => parent._publish(election),
                  ),
                if (_at(ElectionPhases.polling) && _can('ChangePhase'))
                  _actionButton(
                    key: const Key('action-close'),
                    label: 'Close polling',
                    icon: Icons.stop_circle_outlined,
                    busy: parent._isBusy(election.id, 'phase'),
                    onPressed: () => parent._close(election),
                  ),
                if (nextPhase != null && _canAdvance)
                  _actionButton(
                    key: const Key('action-advance'),
                    label: 'Advance to ${electionPhaseLabel(nextPhase)}',
                    icon: Icons.arrow_forward,
                    busy: parent._isBusy(election.id, 'phase'),
                    onPressed: () => parent._advancePhase(election),
                  ),
                if (_canEditRoll) ...[
                  _actionButton(
                    key: const Key('action-freeze'),
                    label: 'Freeze roll',
                    icon: Icons.lock_outline,
                    busy: parent._isBusy(election.id, 'freeze'),
                    onPressed: () => parent._freezeVoterRoll(election),
                  ),
                  _actionButton(
                    key: const Key('action-unfreeze'),
                    label: 'Unfreeze roll',
                    icon: Icons.lock_open_outlined,
                    busy: parent._isBusy(election.id, 'freeze'),
                    onPressed: () => parent._unfreezeVoterRoll(election),
                  ),
                ],
                if (_canSetBallotKey && _can('SetBallotKey'))
                  _actionButton(
                    key: const Key('action-ballot-key'),
                    label: election.ballotKeyFingerprint == null
                        ? 'Make officer key'
                        : 'Replace officer key',
                    icon: Icons.vpn_key_outlined,
                    busy: parent._isBusy(election.id, 'ballot-key'),
                    onPressed: () => parent._makeBallotKey(election),
                  ),
                if (_at(ElectionPhases.counting) && _can('Count'))
                  _actionButton(
                    key: const Key('action-count'),
                    label: 'Count with officer key',
                    icon: Icons.calculate_outlined,
                    busy: parent._isBusy(election.id, 'count'),
                    onPressed: () => parent._count(election),
                  ),
                if (_at(ElectionPhases.counting) && _can('Declare'))
                  _actionButton(
                    key: const Key('action-declare'),
                    label: 'Declare',
                    icon: Icons.campaign_outlined,
                    busy: parent._isBusy(election.id, 'declare'),
                    onPressed: () => parent._declare(election),
                  ),
                if (_at(ElectionPhases.declared) &&
                    _can('SetBallotKey') &&
                    election.ballotKeyFingerprint != null)
                  _actionButton(
                    key: const Key('action-key-destroyed'),
                    label: 'Key destroyed',
                    icon: Icons.key_off_outlined,
                    busy: parent._isBusy(election.id, 'key'),
                    onPressed: () => parent._keyDestroyed(election),
                  ),
                if (_can('ViewDashboard'))
                  _actionButton(
                    key: const Key('action-approvals'),
                    label: 'Approvals',
                    icon: Icons.how_to_reg_outlined,
                    busy: false,
                    onPressed: () => context.push(
                        '/officials/approvals?electionId=${election.id}'),
                  ),
                if (_can('AppointOfficials'))
                  _actionButton(
                    key: const Key('action-officials'),
                    label: 'Officials',
                    icon: Icons.badge_outlined,
                    busy: false,
                    onPressed: () =>
                        showElectionOfficialsSheet(context, election),
                  ),
                if (_can('ManageSetup') || _can('ViewDashboard'))
                  _actionButton(
                    key: const Key('action-form-records'),
                    label: ElectionConstants.formRecordsTitle,
                    icon: Icons.description_outlined,
                    busy: false,
                    onPressed: () =>
                        showElectionFormRecordsSheet(context, election),
                  ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _actionButton({
    Key? key,
    required String label,
    required IconData icon,
    required bool busy,
    required VoidCallback onPressed,
  }) {
    return TextButton.icon(
      key: key,
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
