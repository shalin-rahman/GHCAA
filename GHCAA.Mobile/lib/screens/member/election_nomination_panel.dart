import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_exception.dart';
import '../../core/constants/app_constants.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_dropdown_field.dart';
import '../../core/widgets/app_search_field.dart';
import '../../core/widgets/confirm_dialog.dart';
import '../../core/widgets/glass_container.dart';
import '../../features/auth/auth_service.dart';
import '../../features/elections/election_service.dart';
import '../../features/networking/networking_service.dart';

const nominationStatementMax = 4000;

// FR-37.1b. A member files a nomination as its proposer, and a candidate withdraws their own.
class ElectionNominationPanel extends ConsumerStatefulWidget {
  final int electionId;
  final String phase;

  const ElectionNominationPanel({super.key, required this.electionId, required this.phase});

  @override
  ConsumerState<ElectionNominationPanel> createState() => _ElectionNominationPanelState();
}

class _ElectionNominationPanelState extends ConsumerState<ElectionNominationPanel> {
  final _candidateSearch = TextEditingController();
  final _seconderSearch = TextEditingController();
  final _statement = TextEditingController();

  List<ElectionSeatView> _seats = const [];
  List<Nomination> _nominations = const [];
  int? _seatId;
  Map<String, dynamic>? _candidate;
  Map<String, dynamic>? _seconder;
  List<dynamic> _candidateHits = const [];
  List<dynamic> _seconderHits = const [];
  bool _submitting = false;
  int? _withdrawingId;

  bool get _nominationOpen => widget.phase == ElectionPhases.nomination;

  int? get _memberId => ref.read(userProfileProvider).value?['id'] as int?;

  bool get _canSubmit =>
      !_submitting &&
      _seatId != null &&
      _candidate != null &&
      _seconder != null &&
      _statement.text.trim().isNotEmpty;

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  void dispose() {
    _candidateSearch.dispose();
    _seconderSearch.dispose();
    _statement.dispose();
    super.dispose();
  }

  Future<void> _load() async {
    final service = ref.read(electionServiceProvider);
    try {
      final seats = await service.getSeats(widget.electionId);
      if (mounted) setState(() => _seats = seats);
    } catch (_) {
      if (mounted) setState(() => _seats = const []);
    }
    await _loadNominations();
  }

  Future<void> _loadNominations() async {
    try {
      final list = await ref.read(electionServiceProvider).getNominations(widget.electionId);
      if (mounted) setState(() => _nominations = list);
    } catch (_) {
      if (mounted) setState(() => _nominations = const []);
    }
  }

  void _tell(String message) {
    if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));
  }

  String _seatTitle(int seatId) =>
      _seats.where((s) => s.seatId == seatId).firstOrNull?.positionTitle ?? 'Seat $seatId';

  Future<void> _search(bool candidate, String query) async {
    final controller = candidate ? _candidateSearch : _seconderSearch;
    void setHits(List<dynamic> hits) =>
        setState(() => candidate ? _candidateHits = hits : _seconderHits = hits);
    if (query.trim().length < 2) {
      setHits(const []);
      return;
    }
    final page = await ref.read(networkingServiceProvider).searchAlumni(query: query.trim(), pageSize: 10);
    // A slower reply for an older query must not replace the newer list.
    if (mounted && controller.text.trim() == query.trim()) {
      setHits((page['items'] as List?) ?? const []);
    }
  }

  Future<void> _submit() async {
    final memberId = _memberId;
    if (!_canSubmit || memberId == null) return;
    setState(() => _submitting = true);
    try {
      // The server sets the proposer to the signed-in member whatever is sent.
      await ref.read(electionServiceProvider).nominate(widget.electionId, {
        'electionSeatId': _seatId,
        'candidateMemberId': _candidate!['id'],
        'proposerMemberId': memberId,
        'seconderMemberId': _seconder!['id'],
        'statement': _statement.text.trim(),
      });
      if (!mounted) return;
      setState(() {
        _seatId = null;
        _candidate = null;
        _seconder = null;
        _candidateSearch.clear();
        _seconderSearch.clear();
        _statement.clear();
      });
      _tell('Nomination filed. The election commission will check it at scrutiny.');
      await _loadNominations();
    } on DioException catch (e) {
      _tell(ApiException.serverMessage(e.response?.data) ?? e.message ?? 'The nomination could not be filed.');
    } finally {
      if (mounted) setState(() => _submitting = false);
    }
  }

  Future<void> _withdraw(Nomination nomination) async {
    if (_withdrawingId != null) return;
    final confirmed = await showConfirmDialog(context,
        title: 'Withdraw nomination',
        message: 'Withdraw your nomination for ${_seatTitle(nomination.electionSeatId)}? You cannot undo this.',
        confirmLabel: 'Withdraw',
        destructive: true);
    if (!confirmed || !mounted) return;
    setState(() => _withdrawingId = nomination.id);
    try {
      final ok = await ref.read(electionServiceProvider).withdrawNomination(nomination.id);
      if (!ok) {
        _tell('The nomination could not be withdrawn.');
        return;
      }
      _tell('Your nomination was withdrawn.');
      await _loadNominations();
    } on DioException catch (e) {
      _tell(ApiException.serverMessage(e.response?.data) ?? e.message ?? 'The nomination could not be withdrawn.');
    } finally {
      if (mounted) setState(() => _withdrawingId = null);
    }
  }

  @override
  Widget build(BuildContext context) {
    final memberId = ref.watch(userProfileProvider).value?['id'] as int?;
    // The public list carries no names, so each row is named by its seat.
    final mine = _nominations.where((n) => n.candidateMemberId == memberId).toList();
    return Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
      if (_nominationOpen) ...[_form(), const SizedBox(height: AppTheme.spaceL)],
      GlassContainer(
        padding: const EdgeInsets.all(AppTheme.spaceL),
        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          const Text('Your nominations as a candidate',
              style: TextStyle(color: AppTheme.textMain, fontWeight: FontWeight.w800)),
          const SizedBox(height: AppTheme.spaceS),
          if (mine.isEmpty)
            const Text('Nobody has nominated you in this election.',
                style: TextStyle(color: AppTheme.textMuted))
          else
            for (final n in mine)
              ListTile(
                key: ValueKey('nomination-${n.id}'),
                contentPadding: EdgeInsets.zero,
                title: Text(_seatTitle(n.electionSeatId), style: const TextStyle(color: AppTheme.textMain)),
                subtitle: Text(nominationStatusLabel(n.status), style: const TextStyle(color: AppTheme.textMuted)),
                trailing: widget.phase == ElectionPhases.withdrawal && n.status == 'Accepted'
                    ? TextButton(
                        onPressed: _withdrawingId == null ? () => _withdraw(n) : null,
                        style: TextButton.styleFrom(foregroundColor: Colors.redAccent),
                        child: const Text('WITHDRAW'))
                    : null,
              ),
          if (mine.isNotEmpty && widget.phase == ElectionPhases.withdrawal)
            const Text('Withdrawing takes your name off the ballot for that seat. It cannot be undone.',
                style: TextStyle(color: AppTheme.textMuted, fontSize: 12)),
        ]),
      ),
    ]);
  }

  Widget _form() {
    return GlassContainer(
      padding: const EdgeInsets.all(AppTheme.spaceL),
      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        const Text('Nominate a candidate',
            style: TextStyle(color: AppTheme.textMain, fontWeight: FontWeight.w800)),
        const SizedBox(height: AppTheme.spaceS),
        const Text(
            'You file the nomination as its proposer. The candidate and the seconder must both be on the voter roll, and the candidate cannot be you.',
            style: TextStyle(color: AppTheme.textMuted, fontSize: 12)),
        const SizedBox(height: AppTheme.spaceM),
        AppDropdownField<int>(
          key: ValueKey('seat-$_seatId'),
          value: _seatId,
          labelText: 'Seat',
          items: [
            for (final seat in _seats)
              DropdownMenuItem(
                  value: seat.seatId,
                  child: Text(seat.places > 1 ? '${seat.positionTitle} (${seat.places} places)' : seat.positionTitle)),
          ],
          onChanged: (v) => setState(() => _seatId = v),
        ),
        const SizedBox(height: AppTheme.spaceM),
        _picker('Candidate', true),
        const SizedBox(height: AppTheme.spaceM),
        _picker('Seconder', false),
        const SizedBox(height: AppTheme.spaceM),
        TextField(
          key: const ValueKey('nomination-statement'),
          controller: _statement,
          maxLines: 5,
          maxLength: nominationStatementMax,
          style: const TextStyle(color: AppTheme.textMain),
          decoration: const InputDecoration(labelText: 'Candidate statement'),
          onChanged: (_) => setState(() {}),
        ),
        ElevatedButton(
          onPressed: _canSubmit ? _submit : null,
          child: Text(_submitting ? 'FILING...' : 'FILE NOMINATION'),
        ),
      ]),
    );
  }

  Widget _picker(String label, bool candidate) {
    final picked = candidate ? _candidate : _seconder;
    final hits = candidate ? _candidateHits : _seconderHits;
    if (picked != null) {
      return ListTile(
        contentPadding: EdgeInsets.zero,
        title: Text('$label: ${picked['fullName'] ?? ''}', style: const TextStyle(color: AppTheme.textMain)),
        subtitle: Text('${picked['membershipNumber'] ?? ''}', style: const TextStyle(color: AppTheme.textMuted)),
        trailing: IconButton(
          tooltip: 'Change $label',
          icon: const Icon(Icons.close, color: Colors.white54),
          onPressed: () => setState(() => candidate ? _candidate = null : _seconder = null),
        ),
      );
    }
    return Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
      AppSearchField(
        controller: candidate ? _candidateSearch : _seconderSearch,
        hintText: '$label: search by name or member ID',
        onChanged: (q) => _search(candidate, q),
        onClear: () => setState(() => candidate ? _candidateHits = const [] : _seconderHits = const []),
      ),
      for (final m in hits)
        ListTile(
          dense: true,
          title: Text('${m['fullName'] ?? ''}', style: const TextStyle(color: Colors.white)),
          subtitle: Text('${m['membershipNumber'] ?? ''}', style: const TextStyle(color: AppTheme.textMuted)),
          onTap: () => setState(() {
            if (candidate) {
              _candidate = Map<String, dynamic>.from(m as Map);
              _candidateHits = const [];
            } else {
              _seconder = Map<String, dynamic>.from(m as Map);
              _seconderHits = const [];
            }
          }),
        ),
    ]);
  }
}
