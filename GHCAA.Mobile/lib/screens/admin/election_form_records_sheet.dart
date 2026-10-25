import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/constants/app_constants.dart';
import '../../core/services/org_config_service.dart';
import '../../core/theme/app_theme.dart';
import '../../core/utils/app_utils.dart';
import '../../core/widgets/app_date_picker.dart';
import '../../core/widgets/app_dropdown_field.dart';
import '../../core/widgets/logo_spinner.dart';
import '../../core/widgets/reject_reason_dialog.dart';
import '../../features/elections/election_service.dart';
import 'election_management_screen.dart' show adminElectionsProvider;

Future<void> showElectionFormRecordsSheet(
        BuildContext context, AdminElection election) =>
    showModalBottomSheet(
      context: context,
      backgroundColor: AppTheme.midnightSurface,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(
          borderRadius: BorderRadius.vertical(top: Radius.circular(24))),
      builder: (_) => ElectionFormRecordsSheet(election: election),
    );

// Figures go in once, against the declared result, for an allowed recount.
bool canEnterRecountFigures(AdminElection election, ElectionRecount recount) =>
    election.phase == ElectionPhases.declared &&
    recount.allowed == true &&
    recount.figures.isEmpty &&
    election.myPermissions.contains('Count');

// 37.14m. What the printed forms read that setup does not ask for: notice details, consent
// times, scrutiny ticks and recount requests (ER-11, ER-28). The server checks phase and
// permission. The sheet only hides what the caller cannot do.
class ElectionFormRecordsSheet extends ConsumerStatefulWidget {
  final AdminElection election;
  const ElectionFormRecordsSheet({super.key, required this.election});

  @override
  ConsumerState<ElectionFormRecordsSheet> createState() =>
      _ElectionFormRecordsSheetState();
}

class _ElectionFormRecordsSheetState
    extends ConsumerState<ElectionFormRecordsSheet> {
  late AdminElection _election = widget.election;

  DateTime? _finalListOn;
  DateTime? _campaignOpensOn;
  DateTime? _campaignClosesOn;
  DateTime? _silenceStartsAt;
  DateTime? _recountWindowEndsOn;
  final _venue = TextEditingController();
  final _noticeReference = TextEditingController();
  bool _savingDetails = false;

  List<ElectionRecount>? _recounts;
  int? _seatId;
  final _applicant = TextEditingController();
  final _applicantMember = TextEditingController();
  final _grounds = TextEditingController();
  final _explanation = TextEditingController();
  DateTime? _receivedAt;
  bool _savingRecount = false;
  int? _busyRecountId;
  final Map<int, TextEditingController> _refuseReasons = {};

  int? _figuresOpenId;
  List<SeatResultCandidate> _figureRows = const [];
  final Map<int, TextEditingController> _figureVotes = {};

  List<Nomination>? _nominations;
  final Map<int, DateTime?> _consent = {};
  int? _consentBusyId;
  int? _decideBusyId;
  int? _scrutinyOpenId;
  List<ScrutinyCheck>? _checks;
  String? _busyItem;

  ElectionService get _service => ref.read(electionServiceProvider);
  bool _can(String permission) => _election.myPermissions.contains(permission);

  @override
  void initState() {
    super.initState();
    final d = _election.formDetails;
    _finalListOn = d?.finalListOn;
    _campaignOpensOn = d?.campaignOpensOn;
    _campaignClosesOn = d?.campaignClosesOn;
    _silenceStartsAt = d?.silenceStartsAt;
    _recountWindowEndsOn = d?.recountWindowEndsOn;
    _venue.text = d?.venue ?? '';
    _noticeReference.text = d?.noticeReference ?? '';
    if (_can('ViewDashboard')) _loadRecounts();
    if (_can('ManageSetup') || _can('DecideNominations')) _loadNominations();
  }

  @override
  void dispose() {
    for (final c in [
      _venue,
      _noticeReference,
      _applicant,
      _applicantMember,
      _grounds,
      _explanation,
      ..._refuseReasons.values,
      ..._figureVotes.values,
    ]) {
      c.dispose();
    }
    super.dispose();
  }

  void _tell(String message) {
    if (mounted) {
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text(message)));
    }
  }

  String? _trimmed(TextEditingController c) =>
      c.text.trim().isEmpty ? null : c.text.trim();

  Future<void> _saveDetails() async {
    if (_savingDetails) return;
    setState(() => _savingDetails = true);
    try {
      final updated = await _service.updateFormDetails(
          _election.id,
          ElectionFormDetails(
            finalListOn: _finalListOn,
            campaignOpensOn: _campaignOpensOn,
            campaignClosesOn: _campaignClosesOn,
            silenceStartsAt: _silenceStartsAt,
            venue: _trimmed(_venue),
            noticeReference: _trimmed(_noticeReference),
            recountWindowEndsOn: _recountWindowEndsOn,
          ));
      if (!mounted) return;
      setState(() => _election = updated);
      ref.invalidate(adminElectionsProvider);
      _tell(ElectionConstants.formDetailsSaved);
    } catch (e) {
      _tell('Could not save the form details: $e');
    } finally {
      if (mounted) setState(() => _savingDetails = false);
    }
  }

  Future<void> _loadRecounts() async {
    try {
      final list = await _service.getRecounts(_election.id);
      if (mounted) setState(() => _recounts = list);
    } catch (e) {
      _service.logFailure('getRecounts', e);
      if (mounted) setState(() => _recounts = _recounts ?? const []);
    }
  }

  bool get _canRecord =>
      _seatId != null &&
      _applicant.text.trim().isNotEmpty &&
      _grounds.text.trim().isNotEmpty &&
      _receivedAt != null;

  Future<void> _recordRecount() async {
    if (!_canRecord || _savingRecount) return;
    setState(() => _savingRecount = true);
    try {
      final recount = await _service.recordRecount(
        _election.id,
        electionSeatId: _seatId!,
        applicantName: _applicant.text.trim(),
        applicantMemberId: int.tryParse(_applicantMember.text.trim()),
        grounds: _grounds.text.trim(),
        explanation: _trimmed(_explanation),
        receivedAt: _receivedAt!,
      );
      if (!mounted) return;
      setState(() {
        _recounts = [...?_recounts, recount];
        _seatId = null;
        _receivedAt = null;
      });
      for (final c in [_applicant, _applicantMember, _grounds, _explanation]) {
        c.clear();
      }
      _tell(ElectionConstants.recountRecorded);
    } catch (e) {
      _tell('Could not record the recount request: $e');
    } finally {
      if (mounted) setState(() => _savingRecount = false);
    }
  }

  Future<void> _decide(ElectionRecount recount, bool allowed) async {
    if (_busyRecountId != null) return;
    final reason = _refuseReasons[recount.id]?.text.trim() ?? '';
    if (!allowed && reason.isEmpty) {
      _tell(ElectionConstants.recountRefuseReasonRequired);
      return;
    }
    setState(() => _busyRecountId = recount.id);
    try {
      await _service.decideRecount(
          _election.id, recount.id, allowed, reason.isEmpty ? null : reason);
      _tell(ElectionConstants.recountDecided);
      await _loadRecounts();
    } catch (e) {
      _tell('Could not record the decision: $e');
    } finally {
      if (mounted) setState(() => _busyRecountId = null);
    }
  }

  // Rows come from the declared result for the recount seat, so the nominations always match it.
  Future<void> _openFigures(ElectionRecount recount) async {
    setState(() {
      _figuresOpenId = recount.id;
      _figureRows = const [];
    });
    try {
      final results = await _service.getResults(_election.id);
      final rows = results?.seats
              .where((s) => s.seatId == recount.electionSeatId)
              .expand((s) => s.candidates)
              .toList() ??
          const <SeatResultCandidate>[];
      if (!mounted) return;
      if (rows.isEmpty) {
        setState(() => _figuresOpenId = null);
        _tell(ElectionConstants.recountNoResult);
        return;
      }
      for (final c in _figureVotes.values) {
        c.dispose();
      }
      _figureVotes
        ..clear()
        ..addAll({
          for (final r in rows)
            r.nominationId: TextEditingController(text: '${r.voteCount}')
        });
      setState(() => _figureRows = rows);
    } catch (e) {
      _service.logFailure('getResults', e);
      if (mounted) setState(() => _figuresOpenId = null);
    }
  }

  List<RecountFigure>? _figures() {
    if (_figureRows.isEmpty) return null;
    final figures = <RecountFigure>[];
    for (final row in _figureRows) {
      final votes =
          int.tryParse(_figureVotes[row.nominationId]?.text.trim() ?? '');
      if (votes == null || votes < 0) return null;
      figures
          .add(RecountFigure(nominationId: row.nominationId, voteCount: votes));
    }
    return figures;
  }

  Future<void> _saveFigures(ElectionRecount recount) async {
    final figures = _figures();
    if (figures == null || _busyRecountId != null) return;
    setState(() => _busyRecountId = recount.id);
    try {
      await _service.recordRecountFigures(_election.id, recount.id, figures);
      if (mounted) setState(() => _figuresOpenId = null);
      _tell(ElectionConstants.recountFiguresSaved);
      await _loadRecounts();
    } catch (e) {
      _tell('Could not save the recount figures: $e');
    } finally {
      if (mounted) setState(() => _busyRecountId = null);
    }
  }

  Future<void> _loadNominations() async {
    try {
      final list = await _service.getAdminNominations(_election.id);
      if (mounted) setState(() => _nominations = list);
    } catch (e) {
      _service.logFailure('getAdminNominations', e);
      if (mounted) setState(() => _nominations = const []);
    }
  }

  Future<void> _setConsent(Nomination n) async {
    if (_consentBusyId != null) return;
    final picked = await showAppDatePicker(
        context: context,
        includeTime: true,
        initialDate: _consent[n.id] ?? n.consentAt);
    if (picked == null || !mounted) return;
    setState(() => _consentBusyId = n.id);
    try {
      await _service.recordConsent(_election.id, n.id, picked);
      if (mounted) setState(() => _consent[n.id] = picked);
      _tell(ElectionConstants.consentSaved);
    } catch (e) {
      _tell('Could not save the consent time: $e');
    } finally {
      if (mounted) setState(() => _consentBusyId = null);
    }
  }

  // 84.16. The route needs step-up, which the interceptor asks for. A rejection must say why.
  Future<void> _scrutinise(Nomination n, bool accepted) async {
    if (_decideBusyId != null) return;
    String? reason;
    if (!accepted) {
      reason = await showRejectReasonDialog(context, title: 'REJECT NOMINATION', requireReason: true);
      if (reason == null || !mounted) return;
    }
    setState(() => _decideBusyId = n.id);
    try {
      await _service.scrutinise(n.id, accepted, reason: reason);
      await _loadNominations();
    } catch (e) {
      _tell('Failed to record the scrutiny decision.');
    } finally {
      if (mounted) setState(() => _decideBusyId = null);
    }
  }

  Future<void> _toggleScrutiny(Nomination n) async {
    if (_scrutinyOpenId == n.id) {
      setState(() => _scrutinyOpenId = null);
      return;
    }
    setState(() {
      _scrutinyOpenId = n.id;
      _checks = null;
    });
    try {
      final checks = await _service.getScrutinyChecks(_election.id, n.id);
      if (mounted && _scrutinyOpenId == n.id) setState(() => _checks = checks);
    } catch (e) {
      _service.logFailure('getScrutinyChecks', e);
      if (mounted) setState(() => _checks = const []);
    }
  }

  // Ticks can only change while the election is in scrutiny.
  bool get _canTick =>
      _election.phase == ElectionPhases.scrutiny && _busyItem == null;

  Future<void> _setCheck(int nominationId, String item, bool isChecked) async {
    if (!_canTick) return;
    setState(() => _busyItem = item);
    try {
      final checks = await _service.setScrutinyCheck(
          _election.id, nominationId, item, isChecked);
      if (mounted) setState(() => _checks = checks);
    } catch (e) {
      _tell('Could not save the tick: $e');
    } finally {
      if (mounted) setState(() => _busyItem = null);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding:
          EdgeInsets.only(bottom: MediaQuery.of(context).viewInsets.bottom),
      child: DraggableScrollableSheet(
        expand: false,
        initialChildSize: 0.8,
        maxChildSize: 0.95,
        builder: (context, scroll) => ListView(
          controller: scroll,
          padding: const EdgeInsets.all(AppTheme.spaceM),
          children: [
            Text(
                '${ElectionConstants.formRecordsTitle.toUpperCase()} · ${_election.title}',
                style: const TextStyle(
                    color: AppTheme.royalGold,
                    fontSize: 13,
                    fontWeight: FontWeight.w900)),
            if (_can('ManageSetup')) ...[
              const SizedBox(height: AppTheme.spaceS),
              _detailsForm(),
            ],
            if (_can('ViewDashboard')) ...[
              const Divider(color: Colors.white10, height: 32),
              _recountSection(),
            ],
            if (_can('ManageSetup') || _can('DecideNominations')) ...[
              const Divider(color: Colors.white10, height: 32),
              _nominationSection(),
            ],
          ],
        ),
      ),
    );
  }

  Widget _heading(String text) => Padding(
        padding: const EdgeInsets.only(bottom: AppTheme.spaceS),
        child: Text(text.toUpperCase(),
            style: const TextStyle(
                color: AppTheme.royalGold,
                fontSize: 12,
                fontWeight: FontWeight.w900)),
      );

  Widget _muted(String text) => Text(text,
      style: const TextStyle(color: AppTheme.textMuted, fontSize: 11));

  String _show(DateTime value, {bool includeTime = false}) =>
      AppUtils.formatDate(value,
          format: ref.watch(orgDateFormatProvider).identifier,
          includeTime: includeTime);

  // A date or time with a clear button, since an empty field clears the value on the server.
  Widget _dateField(String key, String label, DateTime? value, bool includeTime,
      ValueChanged<DateTime?> onChanged) {
    return ListTile(
      key: Key('form-$key'),
      contentPadding: EdgeInsets.zero,
      dense: true,
      title: Text(label,
          style: const TextStyle(color: AppTheme.textMuted, fontSize: 11)),
      subtitle: Text(
          value == null ? 'Not set' : _show(value, includeTime: includeTime),
          style: const TextStyle(color: Colors.white, fontSize: 13)),
      trailing: value == null
          ? const Icon(Icons.event, color: AppTheme.royalGold, size: 18)
          : IconButton(
              key: Key('form-$key-clear'),
              icon: const Icon(Icons.close, color: Colors.white54, size: 18),
              onPressed: () => setState(() => onChanged(null)),
            ),
      onTap: () async {
        final picked = await showAppDatePicker(
            context: context, includeTime: includeTime, initialDate: value);
        if (picked != null && mounted) setState(() => onChanged(picked));
      },
    );
  }

  Widget _detailsForm() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        _muted(ElectionConstants.formDetailsHint),
        _dateField('finalListOn', ElectionConstants.finalListOnLabel,
            _finalListOn, false, (v) => _finalListOn = v),
        _dateField('campaignOpensOn', ElectionConstants.campaignOpensOnLabel,
            _campaignOpensOn, false, (v) => _campaignOpensOn = v),
        _dateField('campaignClosesOn', ElectionConstants.campaignClosesOnLabel,
            _campaignClosesOn, false, (v) => _campaignClosesOn = v),
        _dateField('silenceStartsAt', ElectionConstants.silenceStartsAtLabel,
            _silenceStartsAt, true, (v) => _silenceStartsAt = v),
        _dateField(
            'recountWindowEndsOn',
            ElectionConstants.recountWindowEndsOnLabel,
            _recountWindowEndsOn,
            true,
            (v) => _recountWindowEndsOn = v),
        TextField(
            key: const Key('form-venue'),
            controller: _venue,
            style: const TextStyle(color: Colors.white),
            decoration:
                const InputDecoration(labelText: ElectionConstants.venueLabel)),
        TextField(
            key: const Key('form-noticeReference'),
            controller: _noticeReference,
            style: const TextStyle(color: Colors.white),
            decoration: const InputDecoration(
                labelText: ElectionConstants.noticeReferenceLabel)),
        const SizedBox(height: AppTheme.spaceS),
        Align(
          alignment: Alignment.centerRight,
          child: ElevatedButton(
            key: const Key('form-save'),
            style:
                ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold),
            onPressed: _savingDetails ? null : _saveDetails,
            child: const Text('SAVE',
                style: TextStyle(
                    color: Colors.black, fontWeight: FontWeight.bold)),
          ),
        ),
      ],
    );
  }

  Widget _recountSection() {
    final list = _recounts;
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        _heading(ElectionConstants.recountsTitle),
        if (list == null)
          const Center(child: LogoSpinner(size: 60))
        else if (list.isEmpty)
          _muted(ElectionConstants.recountsEmpty)
        else
          for (final r in list) _recountTile(r),
        if (_can('DecideAppeals')) _recountForm(),
      ],
    );
  }

  Widget _recountTile(ElectionRecount r) {
    final status = r.allowed == null
        ? ElectionConstants.recountPending
        : r.allowed!
            ? ElectionConstants.recountAllowed
            : '${ElectionConstants.recountRefused}: ${r.decisionReason ?? ''}';
    final busy = _busyRecountId != null;
    return Card(
      key: Key('recount-${r.id}'),
      color: Colors.white10,
      margin: const EdgeInsets.only(bottom: AppTheme.spaceS),
      child: Padding(
        padding: const EdgeInsets.all(AppTheme.spaceS),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('${r.positionTitle} · ${r.applicantName}',
                style: const TextStyle(
                    color: Colors.white, fontWeight: FontWeight.bold)),
            _muted(r.grounds),
            if (r.receivedAt != null)
              _muted('Received ${_show(r.receivedAt!, includeTime: true)}'),
            _muted(status),
            for (final f in r.figures)
              _muted('Nomination ${f.nominationId}: ${f.voteCount}'),
            if (r.allowed == null && _can('DecideAppeals')) ...[
              TextField(
                key: Key('recount-${r.id}-reason'),
                controller:
                    _refuseReasons.putIfAbsent(r.id, TextEditingController.new),
                style: const TextStyle(color: Colors.white),
                decoration: const InputDecoration(
                    labelText: ElectionConstants.recountRefuseReasonLabel),
              ),
              Wrap(spacing: 8, children: [
                TextButton(
                    key: Key('recount-${r.id}-allow'),
                    onPressed: busy ? null : () => _decide(r, true),
                    child: const Text('Allow')),
                TextButton(
                    key: Key('recount-${r.id}-refuse'),
                    onPressed: busy ? null : () => _decide(r, false),
                    child: const Text('Refuse',
                        style: TextStyle(color: Colors.redAccent))),
              ]),
            ],
            if (canEnterRecountFigures(_election, r)) ...[
              if (_figuresOpenId != r.id)
                TextButton(
                    key: Key('recount-${r.id}-figures'),
                    onPressed: busy ? null : () => _openFigures(r),
                    child: const Text(ElectionConstants.recountFiguresTitle))
              else
                _figuresForm(r),
            ],
          ],
        ),
      ),
    );
  }

  Widget _figuresForm(ElectionRecount r) {
    if (_figureRows.isEmpty) return const Center(child: LogoSpinner(size: 40));
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        for (final row in _figureRows)
          TextField(
            key: Key('figure-${row.nominationId}'),
            controller: _figureVotes[row.nominationId],
            keyboardType: TextInputType.number,
            style: const TextStyle(color: Colors.white),
            decoration: InputDecoration(labelText: row.name),
            onChanged: (_) => setState(() {}),
          ),
        Align(
          alignment: Alignment.centerRight,
          child: TextButton(
              key: Key('recount-${r.id}-save-figures'),
              onPressed: _figures() == null || _busyRecountId != null
                  ? null
                  : () => _saveFigures(r),
              child: const Text('Save figures')),
        ),
      ],
    );
  }

  Widget _recountForm() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const SizedBox(height: AppTheme.spaceS),
        AppDropdownField<int>(
          key: const Key('recount-seat'),
          value: _seatId,
          labelText: ElectionConstants.recountSeatLabel,
          items: [
            for (final p in _election.positions)
              DropdownMenuItem(value: p.id, child: Text(p.title)),
          ],
          onChanged: (value) => setState(() => _seatId = value),
        ),
        TextField(
            key: const Key('recount-applicant'),
            controller: _applicant,
            style: const TextStyle(color: Colors.white),
            onChanged: (_) => setState(() {}),
            decoration: const InputDecoration(
                labelText: ElectionConstants.recountApplicantLabel)),
        TextField(
            key: const Key('recount-applicant-member'),
            controller: _applicantMember,
            keyboardType: TextInputType.number,
            style: const TextStyle(color: Colors.white),
            decoration: const InputDecoration(
                labelText: ElectionConstants.recountApplicantMemberLabel)),
        TextField(
            key: const Key('recount-grounds'),
            controller: _grounds,
            style: const TextStyle(color: Colors.white),
            onChanged: (_) => setState(() {}),
            decoration: const InputDecoration(
                labelText: ElectionConstants.recountGroundsLabel)),
        TextField(
            key: const Key('recount-explanation'),
            controller: _explanation,
            maxLines: 3,
            minLines: 1,
            style: const TextStyle(color: Colors.white),
            decoration: const InputDecoration(
                labelText: ElectionConstants.recountExplanationLabel)),
        _dateField('recount-received', ElectionConstants.recountReceivedAtLabel,
            _receivedAt, true, (v) => _receivedAt = v),
        Align(
          alignment: Alignment.centerRight,
          child: ElevatedButton(
            key: const Key('recount-record'),
            style:
                ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold),
            onPressed: !_canRecord || _savingRecount ? null : _recordRecount,
            child: const Text('RECORD',
                style: TextStyle(
                    color: Colors.black, fontWeight: FontWeight.bold)),
          ),
        ),
      ],
    );
  }

  Widget _nominationSection() {
    final list = _nominations;
    final titles = {for (final p in _election.positions) p.id: p.title};
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        _heading('Nominations'),
        if (list == null)
          const Center(child: LogoSpinner(size: 60))
        else if (list.isEmpty)
          _muted('No nominations yet.')
        else
          for (final n in list)
            _nominationTile(n,
                n.positionTitle ?? titles[n.electionSeatId] ?? 'Seat ${n.electionSeatId}'),
      ],
    );
  }

  Widget _nominationTile(Nomination n, String positionTitle) {
    final consent = _consent[n.id] ?? n.consentAt;
    final undecided = n.status == 'Submitted' || n.status == 'UnderScrutiny';
    // The server only takes a decision while the election is in scrutiny.
    final canDecide = _can('DecideNominations') && undecided && _election.phase == ElectionPhases.scrutiny;
    // A withdrawn or rejected candidate is off the ballot, so their consent time no longer matters.
    final canEditConsent = _can('ManageSetup') && n.status != 'Withdrawn' && n.status != 'Rejected';
    return Card(
      key: Key('nomination-${n.id}'),
      color: Colors.white10,
      margin: const EdgeInsets.only(bottom: AppTheme.spaceS),
      child: Padding(
        padding: const EdgeInsets.all(AppTheme.spaceS),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('$positionTitle · ${n.candidateName ?? 'member ${n.candidateMemberId}'}',
                style: const TextStyle(
                    color: Colors.white, fontWeight: FontWeight.bold)),
            _muted('Status: ${n.status}'),
            _muted(consent == null
                ? ElectionConstants.consentMissing
                : '${ElectionConstants.consentLabel} ${_show(consent, includeTime: true)}'),
            Wrap(spacing: 8, children: [
              if (canEditConsent)
                TextButton(
                    key: Key('nomination-${n.id}-consent'),
                    onPressed:
                        _consentBusyId != null ? null : () => _setConsent(n),
                    child: const Text(ElectionConstants.consentLabel)),
              if (_can('DecideNominations'))
                TextButton(
                    key: Key('nomination-${n.id}-scrutiny'),
                    onPressed: () => _toggleScrutiny(n),
                    child: const Text(ElectionConstants.scrutinyChecksTitle)),
              if (canDecide) ...[
                TextButton(
                    key: Key('nomination-${n.id}-accept'),
                    onPressed:
                        _decideBusyId != null ? null : () => _scrutinise(n, true),
                    child: const Text('Accept')),
                TextButton(
                    key: Key('nomination-${n.id}-reject'),
                    onPressed:
                        _decideBusyId != null ? null : () => _scrutinise(n, false),
                    child: const Text('Reject')),
              ],
            ]),
            if (_scrutinyOpenId == n.id) _scrutinyList(n),
          ],
        ),
      ),
    );
  }

  Widget _scrutinyList(Nomination n) {
    final checks = _checks;
    if (checks == null) return const Center(child: LogoSpinner(size: 40));
    return Column(
      children: [
        if (_election.phase != ElectionPhases.scrutiny)
          _muted('Ticks can be changed only during scrutiny.'),
        for (final option in ElectionConstants.scrutinyCheckItems)
          CheckboxListTile(
            key: Key('scrutiny-${option['value']}'),
            contentPadding: EdgeInsets.zero,
            dense: true,
            title: Text(option['label']!,
                style: const TextStyle(color: Colors.white, fontSize: 13)),
            value: checks.any((c) => c.item == option['value'] && c.isChecked),
            activeColor: AppTheme.royalGold,
            controlAffinity: ListTileControlAffinity.leading,
            onChanged: _canTick
                ? (v) => _setCheck(n.id, option['value']!, v ?? false)
                : null,
          ),
      ],
    );
  }
}
