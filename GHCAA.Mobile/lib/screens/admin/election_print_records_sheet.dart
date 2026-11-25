import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/constants/app_constants.dart';
import '../../core/services/org_config_service.dart';
import '../../core/theme/app_theme.dart';
import '../../core/utils/app_utils.dart';
import '../../core/widgets/logo_spinner.dart';
import '../../features/elections/election_service.dart';

Future<void> showElectionPrintRecordsSheet(
        BuildContext context, AdminElection election) =>
    showModalBottomSheet(
      context: context,
      backgroundColor: AppTheme.midnightSurface,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(
          borderRadius: BorderRadius.vertical(top: Radius.circular(24))),
      builder: (_) => ElectionPrintRecordsSheet(election: election),
    );

// 37.16i. The filled forms printed for one election, read only. Revoking stays on the web.
class ElectionPrintRecordsSheet extends ConsumerStatefulWidget {
  final AdminElection election;
  const ElectionPrintRecordsSheet({super.key, required this.election});

  @override
  ConsumerState<ElectionPrintRecordsSheet> createState() =>
      _ElectionPrintRecordsSheetState();
}

class _ElectionPrintRecordsSheetState
    extends ConsumerState<ElectionPrintRecordsSheet> {
  List<ElectionFormPrintRow>? _rows;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    final service = ref.read(electionServiceProvider);
    try {
      final rows = await service.getPrintRecords(widget.election.id);
      if (mounted) setState(() => _rows = rows);
    } catch (e) {
      service.logFailure('getPrintRecords', e);
      if (mounted) {
        setState(() {
          _rows = const [];
          _error = ElectionConstants.printRecordsLoadFailed;
        });
      }
    }
  }

  String _show(DateTime value) => AppUtils.formatDate(value,
      format: ref.watch(orgDateFormatProvider).identifier, includeTime: true);

  Widget _muted(String text) => Text(text,
      style: const TextStyle(color: AppTheme.textMuted, fontSize: 11));

  Widget _tile(ElectionFormPrintRow r) => Card(
        key: Key('print-${r.id}'),
        color: Colors.white10,
        margin: const EdgeInsets.only(bottom: AppTheme.spaceS),
        child: Padding(
          padding: const EdgeInsets.all(AppTheme.spaceS),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(r.formCode,
                  style: const TextStyle(
                      color: Colors.white, fontWeight: FontWeight.bold)),
              if (r.printedAt != null) _muted('Printed ${_show(r.printedAt!)}'),
              _muted('Printed by ${r.printedByRole}'),
              _muted(
                  'Phase ${electionPhaseLabels[r.phaseAtPrint] ?? r.phaseAtPrint}'),
              if (r.revokedAt != null)
                Text(
                    'Revoked ${_show(r.revokedAt!)}: ${r.revokeReason ?? ''}',
                    style: const TextStyle(
                        color: AppTheme.dangerColor, fontSize: 11)),
            ],
          ),
        ),
      );

  @override
  Widget build(BuildContext context) {
    final rows = _rows;
    return DraggableScrollableSheet(
      expand: false,
      initialChildSize: 0.8,
      maxChildSize: 0.95,
      builder: (context, scroll) => ListView(
        controller: scroll,
        padding: const EdgeInsets.all(AppTheme.spaceM),
        children: [
          Text(
              '${ElectionConstants.printRecordsTitle.toUpperCase()} · ${widget.election.title}',
              style: const TextStyle(
                  color: AppTheme.royalGold,
                  fontSize: 13,
                  fontWeight: FontWeight.w900)),
          const SizedBox(height: AppTheme.spaceS),
          _muted(ElectionConstants.printRecordsWebOnly),
          const SizedBox(height: AppTheme.spaceS),
          if (rows == null)
            const Center(child: LogoSpinner(size: 60))
          else if (_error != null)
            _muted(_error!)
          else if (rows.isEmpty)
            _muted(ElectionConstants.printRecordsEmpty)
          else
            for (final r in rows) _tile(r),
        ],
      ),
    );
  }
}
