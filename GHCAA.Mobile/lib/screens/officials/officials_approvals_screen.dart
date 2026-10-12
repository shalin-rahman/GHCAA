import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_exception.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/logo_spinner.dart';
import '../../core/utils/app_utils.dart';
import '../../features/elections/election_service.dart';
import 'officials_dashboard_screen.dart';

final officialsApprovalsProvider = FutureProvider.autoDispose
    .family<List<ElectionApproval>, int>((ref, electionId) async {
  return ref.read(electionServiceProvider).getApprovals(electionId);
});

// Two-person steps waiting for a second official. The server flags the
// caller's own requests, and those rows get no Approve or Reject button.
class OfficialsApprovalsScreen extends ConsumerStatefulWidget {
  final int electionId;
  const OfficialsApprovalsScreen({super.key, required this.electionId});

  @override
  ConsumerState<OfficialsApprovalsScreen> createState() =>
      _OfficialsApprovalsScreenState();
}

class _OfficialsApprovalsScreenState
    extends ConsumerState<OfficialsApprovalsScreen> {
  int? _busyId;

  Future<void> _decide(
      ElectionApproval a, Future<void> Function() call, String done) async {
    setState(() => _busyId = a.id);
    try {
      await call();
      ref.invalidate(officialsApprovalsProvider(widget.electionId));
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(done)));
      }
    } catch (e) {
      final detail =
          e is DioException ? ApiException.serverMessage(e.response?.data) : null;
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text(detail ?? 'That did not go through.')));
      }
    } finally {
      if (mounted) setState(() => _busyId = null);
    }
  }

  @override
  Widget build(BuildContext context) {
    final approvalsAsync =
        ref.watch(officialsApprovalsProvider(widget.electionId));
    final canApprove = ref.watch(officialsElectionsProvider).maybeWhen(
          data: (list) => list.any((e) =>
              e.id == widget.electionId && e.myPermissions.contains('Approve')),
          orElse: () => false,
        );

    return AppScaffold(
      title: 'Approvals',
      breadcrumb: 'OFFICIALS > APPROVALS',
      child: approvalsAsync.when(
        loading: () => const Center(child: LogoSpinner(size: 120)),
        error: (e, s) => const EmptyStateWidget(
            'Approvals are unavailable. Try again later.',
            icon: Icons.error_outline),
        data: (list) => list.isEmpty
            ? const EmptyStateWidget('Nothing is waiting for approval.',
                icon: Icons.fact_check_outlined)
            : RefreshIndicator(
                color: AppTheme.royalGold,
                onRefresh: () async =>
                    ref.invalidate(officialsApprovalsProvider(widget.electionId)),
                child: ListView(
                  padding: const EdgeInsets.all(AppTheme.spaceM),
                  children: [for (final a in list) _card(context, a, canApprove)],
                ),
              ),
      ),
    );
  }

  Widget _card(BuildContext context, ElectionApproval a, bool canApprove) {
    final text = Theme.of(context).textTheme;
    final busy = _busyId == a.id;

    return Padding(
      padding: const EdgeInsets.only(bottom: AppTheme.spaceM),
      child: GlassContainer(
        padding: const EdgeInsets.all(AppTheme.spaceM),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(electionApprovalActionLabel(a.action), style: text.titleMedium),
            Text(
                'Asked for by ${a.requestedBy}. Expires ${AppUtils.formatDate(a.expiresAt)}.',
                style: text.bodySmall),
            if (a.requestedByMe)
              Text('You asked for this, so a second official must decide.',
                  style: text.bodySmall)
            else if (a.approvedAt == null && canApprove) ...[
              const SizedBox(height: AppTheme.spaceS),
              Wrap(
                spacing: AppTheme.spaceS,
                children: [
                  FilledButton(
                    onPressed: busy
                        ? null
                        : () => _decide(
                            a,
                            () => ref
                                .read(electionServiceProvider)
                                .approve(a.id),
                            'Approved.'),
                    child: const Text('Approve'),
                  ),
                  OutlinedButton(
                    onPressed: busy
                        ? null
                        : () => _decide(
                            a,
                            () => ref
                                .read(electionServiceProvider)
                                .reject(a.id, null),
                            'Rejected.'),
                    child: const Text('Reject'),
                  ),
                ],
              ),
            ],
          ],
        ),
      ),
    );
  }
}
