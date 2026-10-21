import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/constants/app_constants.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/logo_spinner.dart';
import '../../features/admin/admin_service.dart';

// 37.14m: what the outgoing committee has handed over, ticked against the term.
void showHandoverChecklistSheet(BuildContext context, {required int periodId, required String title}) {
  showModalBottomSheet(
    context: context,
    backgroundColor: AppTheme.midnightSurface,
    isScrollControlled: true,
    shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(24))),
    builder: (ctx) => HandoverChecklistSheet(periodId: periodId, title: title),
  );
}

class HandoverChecklistSheet extends ConsumerStatefulWidget {
  final int periodId;
  final String title;
  const HandoverChecklistSheet({super.key, required this.periodId, required this.title});

  @override
  ConsumerState<HandoverChecklistSheet> createState() => _HandoverChecklistSheetState();
}

class _HandoverChecklistSheetState extends ConsumerState<HandoverChecklistSheet> {
  List<dynamic>? _checks;
  String? _busyItem;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    try {
      final list = await ref.read(adminServiceProvider).getHandoverChecks(widget.periodId);
      if (mounted) setState(() => _checks = list);
    } catch (_) {
      if (mounted) {
        setState(() => _checks = _checks ?? []);
        _tell('Could not load the handover checklist.');
      }
    }
  }

  bool _isChecked(String item) =>
      _checks?.any((c) => c['item'] == item && c['isChecked'] == true) ?? false;

  Future<void> _setCheck(String item, bool isChecked) async {
    if (_busyItem != null) return;
    setState(() => _busyItem = item);
    try {
      final list = await ref.read(adminServiceProvider).setHandoverCheck(widget.periodId, item, isChecked);
      if (mounted) setState(() => _checks = list);
    } catch (_) {
      _tell('Could not save the tick. Please try again.');
    } finally {
      if (mounted) setState(() => _busyItem = null);
    }
  }

  void _tell(String message) {
    if (!mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));
  }

  @override
  Widget build(BuildContext context) {
    return SafeArea(
      child: Padding(
        padding: const EdgeInsets.fromLTRB(20, 20, 20, 12),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text('HANDOVER CHECKLIST',
                style: TextStyle(color: AppTheme.royalGold, fontSize: 14, fontWeight: FontWeight.bold, letterSpacing: 1)),
            const SizedBox(height: 4),
            Text(widget.title, style: const TextStyle(color: AppTheme.textSecondaryDark, fontSize: 12)),
            const SizedBox(height: 12),
            if (_checks == null)
              const Padding(padding: EdgeInsets.all(24), child: Center(child: LogoSpinner()))
            else
              Flexible(
                child: ListView(
                  shrinkWrap: true,
                  children: [
                    for (final option in GovernanceConstants.handoverCheckItems)
                      CheckboxListTile(
                        key: Key('handover-${option['value']}'),
                        contentPadding: EdgeInsets.zero,
                        dense: true,
                        title: Text(option['label']!, style: const TextStyle(color: Colors.white, fontSize: 13)),
                        value: _isChecked(option['value']!),
                        activeColor: AppTheme.royalGold,
                        controlAffinity: ListTileControlAffinity.leading,
                        onChanged: _busyItem != null ? null : (v) => _setCheck(option['value']!, v ?? false),
                      ),
                  ],
                ),
              ),
          ],
        ),
      ),
    );
  }
}
