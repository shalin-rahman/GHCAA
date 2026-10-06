import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/constants/app_constants.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/confirm_dialog.dart';
import '../../features/elections/election_service.dart';

Future<void> showElectionRulesUnlockSheet(BuildContext context) =>
    showModalBottomSheet(
      context: context,
      backgroundColor: AppTheme.midnightSurface,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(
          borderRadius: BorderRadius.vertical(top: Radius.circular(24))),
      builder: (_) => const ElectionRulesUnlockSheet(),
    );

// 37.13z. SuperAdmin only. Nothing is fetched until asked, because every call asks for
// the step-up code.
class ElectionRulesUnlockSheet extends ConsumerStatefulWidget {
  const ElectionRulesUnlockSheet({super.key});

  @override
  ConsumerState<ElectionRulesUnlockSheet> createState() =>
      _ElectionRulesUnlockSheetState();
}

class _ElectionRulesUnlockSheetState
    extends ConsumerState<ElectionRulesUnlockSheet> {
  bool _loaded = false;
  bool _busy = false;
  ElectionRulesUnlock? _unlock;
  final _reason = TextEditingController();
  final _minutes = TextEditingController(
      text: '${ElectionConstants.unlockDefaultMinutes}');

  ElectionService get _service => ref.read(electionServiceProvider);

  @override
  void dispose() {
    _reason.dispose();
    _minutes.dispose();
    super.dispose();
  }

  void _tell(String message) {
    if (mounted) {
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text(message)));
    }
  }

  Future<void> _load() async {
    setState(() => _busy = true);
    try {
      final unlock = await _service.getRulesUnlock();
      if (mounted) {
        setState(() {
          _unlock = unlock;
          _loaded = true;
        });
      }
    } catch (e) {
      _tell('Could not check the unlock: $e');
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _open() async {
    final reason = _reason.text.trim();
    final minutes = int.tryParse(_minutes.text.trim());
    final problem = rulesUnlockProblem(reason, minutes);
    if (problem != null) {
      _tell(problem);
      return;
    }
    final confirmed = await showConfirmDialog(context,
        title: 'Unlock election rules',
        message:
            'Frozen rules on every election can be changed for $minutes minutes. This is logged.',
        confirmLabel: 'Unlock',
        destructive: true);
    if (!confirmed) return;
    setState(() => _busy = true);
    try {
      final unlock = await _service.openRulesUnlock(reason, minutes!);
      if (mounted) {
        setState(() => _unlock = unlock);
        _reason.clear();
        _minutes.text = '${ElectionConstants.unlockDefaultMinutes}';
      }
    } catch (e) {
      _tell('Could not unlock: $e');
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  // A failed close usually means it already expired or someone else closed it, so reload.
  Future<void> _close() async {
    setState(() => _busy = true);
    try {
      await _service.closeRulesUnlock();
      if (mounted) setState(() => _unlock = null);
    } catch (e) {
      _service.logFailure('closeRulesUnlock', e);
      if (mounted) setState(() => _busy = false);
      await _load();
      return;
    }
    if (mounted) setState(() => _busy = false);
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: EdgeInsets.fromLTRB(AppTheme.spaceM, AppTheme.spaceM,
          AppTheme.spaceM, AppTheme.spaceM + MediaQuery.of(context).viewInsets.bottom),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('RULES UNLOCK',
              style: TextStyle(
                  color: AppTheme.royalGold,
                  fontSize: 13,
                  fontWeight: FontWeight.w900)),
          const SizedBox(height: AppTheme.spaceS),
          if (!_loaded)
            ElevatedButton(
              onPressed: _busy ? null : _load,
              child: const Text('Check rules unlock'),
            )
          else if (_unlock != null)
            _openUnlock(_unlock!)
          else
            _openForm(),
        ],
      ),
    );
  }

  Widget _openUnlock(ElectionRulesUnlock unlock) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text('Open by ${unlock.openedBy}, ${unlock.minutesLeft(DateTime.now())} min left',
            style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
        const SizedBox(height: 4),
        Text(unlock.reason,
            style: const TextStyle(color: AppTheme.textMuted, fontSize: 12)),
        Align(
          alignment: Alignment.centerRight,
          child: TextButton(
              onPressed: _busy ? null : _close, child: const Text('Close now')),
        ),
      ],
    );
  }

  Widget _openForm() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
            'Nothing is open. An unlock lets frozen rules be changed for a short time.',
            style: TextStyle(color: AppTheme.textMuted, fontSize: 12)),
        TextField(
          controller: _reason,
          maxLength: ElectionConstants.unlockReasonMax,
          maxLines: 3,
          style: const TextStyle(color: Colors.white),
          decoration: const InputDecoration(labelText: 'Reason'),
        ),
        TextField(
          controller: _minutes,
          keyboardType: TextInputType.number,
          style: const TextStyle(color: Colors.white),
          decoration: const InputDecoration(
              labelText: 'Minutes (1 to ${ElectionConstants.unlockMaxMinutes})'),
        ),
        const SizedBox(height: AppTheme.spaceS),
        Align(
          alignment: Alignment.centerRight,
          child: ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold),
            onPressed: _busy ? null : _open,
            child: const Text('UNLOCK',
                style: TextStyle(color: Colors.black, fontWeight: FontWeight.bold)),
          ),
        ),
      ],
    );
  }
}
