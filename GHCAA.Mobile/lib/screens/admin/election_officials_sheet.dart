import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/constants/app_constants.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_dropdown_field.dart';
import '../../core/widgets/app_search_field.dart';
import '../../core/widgets/confirm_dialog.dart';
import '../../core/widgets/logo_spinner.dart';
import '../../features/admin/admin_service.dart';
import '../../features/auth/auth_service.dart';
import '../../features/elections/election_service.dart';

Future<void> showElectionOfficialsSheet(
        BuildContext context, AdminElection election) =>
    showModalBottomSheet(
      context: context,
      backgroundColor: AppTheme.midnightSurface,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(
          borderRadius: BorderRadius.vertical(top: Radius.circular(24))),
      builder: (_) => ElectionOfficialsSheet(election: election),
    );

// 37.13y. Officials of one election: list, appoint, revoke, and for a SuperAdmin the
// emergency revoke that a second person approves on the web admin.
class ElectionOfficialsSheet extends ConsumerStatefulWidget {
  final AdminElection election;
  const ElectionOfficialsSheet({super.key, required this.election});

  @override
  ConsumerState<ElectionOfficialsSheet> createState() =>
      _ElectionOfficialsSheetState();
}

class _ElectionOfficialsSheetState
    extends ConsumerState<ElectionOfficialsSheet> {
  List<ElectionAppointment>? _appointments;
  List<ElectionPersonaOption> _personas = const [];
  List<dynamic> _memberResults = const [];
  bool _busy = false;
  int? _emergencyId;

  int? _personaId;
  int? _memberId;
  String? _memberLabel;
  bool _isReturningOfficer = false;
  // Member search is admin only on the API, but election officials can open this sheet too.
  bool _searchDenied = false;
  final _search = TextEditingController();
  final _name = TextEditingController();
  final _email = TextEditingController();
  final _phone = TextEditingController();
  final _emergencyReason = TextEditingController();

  ElectionService get _service => ref.read(electionServiceProvider);

  @override
  void initState() {
    super.initState();
    _load();
    _loadPersonas();
  }

  Future<void> _loadPersonas() async {
    try {
      final value = await _service.getPersonas();
      if (mounted) setState(() => _personas = value);
    } catch (e) {
      _service.logFailure('getPersonas', e);
    }
  }

  @override
  void dispose() {
    for (final c in [_search, _name, _email, _phone, _emergencyReason]) {
      c.dispose();
    }
    super.dispose();
  }

  Future<void> _load() async {
    try {
      final list = await _service.getAppointments(widget.election.id);
      if (mounted) setState(() => _appointments = list);
    } catch (e) {
      _service.logFailure('getAppointments', e);
      if (mounted) setState(() => _appointments = const []);
    }
  }

  void _tell(String message) {
    if (mounted) {
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text(message)));
    }
  }

  // Runs one call with the busy flag held. The API's own message comes back in the error.
  Future<void> _run(Future<void> Function() call, String failure) async {
    if (_busy) return;
    setState(() => _busy = true);
    try {
      await call();
    } catch (e) {
      _tell('$failure: $e');
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _searchMembers(String query) async {
    if (query.trim().length < 2) {
      setState(() => _memberResults = const []);
      return;
    }
    try {
      final items = await ref.read(adminServiceProvider).searchMembers(query.trim());
      // A slower reply for an older query must not replace the newer list.
      if (mounted && _search.text.trim() == query.trim()) {
        setState(() => _memberResults = items);
      }
    } catch (e) {
      if (e is DioException && e.response?.statusCode == 403) {
        if (mounted) setState(() => _searchDenied = true);
        return;
      }
      _service.logFailure('searchMembers', e);
    }
  }

  Future<void> _appoint() async {
    final request = buildAppointRequest(
      personaId: _personaId,
      memberId: _memberId,
      displayName: _name.text,
      email: _email.text,
      phone: _phone.text,
      isReturningOfficer: _isReturningOfficer,
    );
    if (request.problem != null) {
      _tell(request.problem!);
      return;
    }
    await _run(() async {
      await _service.appoint(widget.election.id, request.body!);
      setState(() {
        _personaId = null;
        _memberId = null;
        _memberLabel = null;
        _isReturningOfficer = false;
        _memberResults = const [];
      });
      for (final c in [_search, _name, _email, _phone]) {
        c.clear();
      }
      _tell('Appointed. They accept from their own appointments page.');
      await _load();
    }, 'Could not appoint');
  }

  Future<void> _revoke(ElectionAppointment a) async {
    final confirmed = await showConfirmDialog(context,
        title: 'Revoke appointment',
        message: 'Remove ${a.displayName} as ${a.personaName}?',
        confirmLabel: 'Revoke',
        destructive: true);
    if (!confirmed) return;
    await _run(() async {
      await _service.revokeAppointment(a.id, null);
      await _load();
    }, 'Could not revoke');
  }

  Future<void> _sendEmergency(ElectionAppointment a) async {
    final reason = _emergencyReason.text.trim();
    if (reason.isEmpty) {
      _tell('Give a reason for the emergency revoke.');
      return;
    }
    await _run(() async {
      await _service.emergencyRevoke(a.id, reason);
      setState(() => _emergencyId = null);
      _emergencyReason.clear();
      _tell('Saved. A second person must approve this on the web admin.');
    }, 'Could not ask for the emergency revoke');
  }

  @override
  Widget build(BuildContext context) {
    final isSuperAdmin = ref.watch(isSuperAdminProvider).value ?? false;
    final list = _appointments;
    final open = list?.where((a) => a.revokedAt == null).toList() ?? const [];
    final revokedCount = (list?.length ?? 0) - open.length;

    return Padding(
      padding: EdgeInsets.only(
          bottom: MediaQuery.of(context).viewInsets.bottom),
      child: DraggableScrollableSheet(
        expand: false,
        initialChildSize: 0.8,
        maxChildSize: 0.95,
        builder: (context, scroll) => ListView(
          controller: scroll,
          padding: const EdgeInsets.all(AppTheme.spaceM),
          children: [
            Text('OFFICIALS · ${widget.election.title}',
                style: const TextStyle(
                    color: AppTheme.royalGold,
                    fontSize: 13,
                    fontWeight: FontWeight.w900)),
            const SizedBox(height: AppTheme.spaceS),
            if (list == null)
              const Center(child: LogoSpinner(size: 60))
            else if (open.isEmpty)
              const Text('Nobody is appointed yet.',
                  style: TextStyle(color: AppTheme.textMuted, fontSize: 12))
            else
              for (final a in open) _officialTile(a, isSuperAdmin),
            if (revokedCount > 0)
              Padding(
                padding: const EdgeInsets.only(top: AppTheme.spaceS),
                child: Text('$revokedCount revoked, shown on the web admin.',
                    style: const TextStyle(
                        color: AppTheme.textMuted, fontSize: 11)),
              ),
            const Divider(color: Colors.white10, height: 32),
            _appointForm(),
          ],
        ),
      ),
    );
  }

  Widget _officialTile(ElectionAppointment a, bool isSuperAdmin) {
    final status = a.isPending ? 'Waiting to accept' : (a.isLive ? 'Live' : 'Not live');
    return Card(
      color: Colors.white10,
      margin: const EdgeInsets.only(bottom: AppTheme.spaceS),
      child: Padding(
        padding: const EdgeInsets.all(AppTheme.spaceS),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(a.displayName,
                style: const TextStyle(
                    color: Colors.white, fontWeight: FontWeight.bold)),
            Text(
                '${a.personaName}${a.isReturningOfficer ? ' · Returning Officer' : ''} · $status',
                style: const TextStyle(color: AppTheme.textMuted, fontSize: 11)),
            Wrap(spacing: 8, children: [
              TextButton(
                  onPressed: _busy ? null : () => _revoke(a),
                  child: const Text('Revoke')),
              if (isSuperAdmin)
                TextButton(
                    onPressed: _busy
                        ? null
                        : () => setState(() =>
                            _emergencyId = _emergencyId == a.id ? null : a.id),
                    child: const Text('Emergency revoke',
                        style: TextStyle(color: Colors.redAccent))),
            ]),
            if (_emergencyId == a.id) ...[
              const Text(
                  'Use this once the rules have frozen and a plain revoke is refused.',
                  style: TextStyle(color: AppTheme.textMuted, fontSize: 11)),
              TextField(
                controller: _emergencyReason,
                maxLength: ElectionConstants.appointmentReasonMax,
                style: const TextStyle(color: Colors.white),
                decoration: const InputDecoration(labelText: 'Reason'),
              ),
              Align(
                alignment: Alignment.centerRight,
                child: TextButton(
                    onPressed: _busy ? null : () => _sendEmergency(a),
                    child: const Text('Send for approval')),
              ),
            ],
          ],
        ),
      ),
    );
  }

  Widget _appointForm() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text('APPOINT',
            style: TextStyle(
                color: AppTheme.royalGold,
                fontSize: 12,
                fontWeight: FontWeight.w900)),
        const SizedBox(height: AppTheme.spaceS),
        AppDropdownField<int>(
          value: _personaId,
          labelText: 'Role',
          items: [
            for (final p in _personas)
              DropdownMenuItem(value: p.id, child: Text(p.name)),
          ],
          onChanged: (value) => setState(() => _personaId = value),
        ),
        const SizedBox(height: AppTheme.spaceS),
        if (_memberId != null)
          ListTile(
            contentPadding: EdgeInsets.zero,
            title: Text(_memberLabel ?? '',
                style: const TextStyle(color: Colors.white)),
            trailing: IconButton(
              icon: const Icon(Icons.close, color: Colors.white54),
              onPressed: () => setState(() {
                _memberId = null;
                _memberLabel = null;
              }),
            ),
          )
        else ...[
          if (_searchDenied)
            const Text('Only an admin can search members. Give a name and email below.',
                style: TextStyle(color: AppTheme.textMuted, fontSize: 12))
          else
            AppSearchField(
              controller: _search,
              hintText: 'Search members',
              onChanged: _searchMembers,
              onClear: () => setState(() => _memberResults = const []),
            ),
          for (final m in _memberResults)
            ListTile(
              dense: true,
              title: Text('${m['fullName'] ?? ''}',
                  style: const TextStyle(color: Colors.white)),
              subtitle: Text('${m['membershipNumber'] ?? ''}',
                  style: const TextStyle(color: AppTheme.textMuted)),
              onTap: () => setState(() {
                _memberId = m['id'] as int?;
                _memberLabel = '${m['fullName'] ?? ''}';
                _memberResults = const [];
              }),
            ),
          const Padding(
            padding: EdgeInsets.only(top: AppTheme.spaceS),
            child: Text('Or someone outside the membership:',
                style: TextStyle(color: AppTheme.textMuted, fontSize: 11)),
          ),
          TextField(
              controller: _name,
              style: const TextStyle(color: Colors.white),
              decoration: const InputDecoration(labelText: 'Name')),
          TextField(
              controller: _email,
              keyboardType: TextInputType.emailAddress,
              style: const TextStyle(color: Colors.white),
              decoration: const InputDecoration(labelText: 'Email')),
          TextField(
              controller: _phone,
              keyboardType: TextInputType.phone,
              style: const TextStyle(color: Colors.white),
              decoration: const InputDecoration(labelText: 'Phone (optional)')),
        ],
        CheckboxListTile(
          contentPadding: EdgeInsets.zero,
          value: _isReturningOfficer,
          onChanged: (value) =>
              setState(() => _isReturningOfficer = value ?? false),
          title: const Text('Returning Officer',
              style: TextStyle(color: Colors.white, fontSize: 13)),
        ),
        Align(
          alignment: Alignment.centerRight,
          child: ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold),
            onPressed: _busy ? null : _appoint,
            child: const Text('APPOINT',
                style: TextStyle(
                    color: Colors.black, fontWeight: FontWeight.bold)),
          ),
        ),
      ],
    );
  }
}
