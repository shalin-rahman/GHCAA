import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../features/networking/mentorship_service.dart';
import '../../core/config/app_config.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../core/widgets/logo_spinner.dart';

final sentMentorshipsProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(mentorshipServiceProvider).getSentRequests();
});

final receivedMentorshipsProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(mentorshipServiceProvider).getReceivedRequests();
});

class MentorshipHubScreen extends ConsumerStatefulWidget {
  const MentorshipHubScreen({super.key});

  @override
  ConsumerState<MentorshipHubScreen> createState() => _MentorshipHubScreenState();
}

class _MentorshipHubScreenState extends ConsumerState<MentorshipHubScreen> with SingleTickerProviderStateMixin {
  late TabController _tabs;
  int? _processingRequestId;

  @override
  void initState() {
    super.initState();
    _tabs = TabController(length: 2, vsync: this);
  }

  @override
  void dispose() {
    _tabs.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final sentAsync = ref.watch(sentMentorshipsProvider);
    final receivedAsync = ref.watch(receivedMentorshipsProvider);

    return AppScaffold(
      title: 'Mentorship Hub',
      breadcrumb: 'CAREER > MENTORSHIP',
      child: Column(
        children: [
          TabBar(
            controller: _tabs,
            indicatorColor: AppTheme.royalGold,
            labelColor: AppTheme.royalGold,
            unselectedLabelColor: Colors.white38,
            labelStyle: const TextStyle(fontSize: 11, fontWeight: FontWeight.bold, letterSpacing: 1),
            tabs: const [
              Tab(text: 'MY REQUESTS'),
              Tab(text: 'I AM MENTORING'),
            ],
          ),
          Expanded(
            child: TabBarView(
              controller: _tabs,
              children: [
                _buildList(sentAsync, isSent: true),
                _buildList(receivedAsync, isSent: false),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildList(AsyncValue<List<dynamic>> asyncData, {required bool isSent}) {
    return asyncData.when(
      data: (list) {
        if (list.isEmpty) {
          return EmptyStateWidget(isSent ? 'You have not requested mentorship.' : 'No active mentees.', icon: Icons.people_outline);
        }
        return ListView.builder(
          padding: const EdgeInsets.all(16),
          itemCount: list.length,
          itemBuilder: (ctx, i) {
            final req = list[i];
            final person = isSent ? req['mentor'] : req['requester'];
            if (person == null) return const SizedBox();
            
            final status = req['status'] ?? 0;
            String statusText = 'PENDING';
            Color statusColor = Colors.orangeAccent;
            if (status == 1) { statusText = 'ACTIVE'; statusColor = Colors.greenAccent; }
            else if (status == 2) { statusText = 'DECLINED'; statusColor = Colors.redAccent; }
            else if (status == 3) { statusText = 'COMPLETED'; statusColor = Colors.blueAccent; }

            String? photoPath;
            if (person['photoPath'] != null) {
              final bp = AppConfig.apiBaseUrl.replaceFirst('/api', '');
              photoPath = person['photoPath'].toString().startsWith('http') ? person['photoPath'] : '$bp/${person['photoPath'].toString().replaceFirst(RegExp(r'^/'), '')}';
            }

            return Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: GlassContainer(
                padding: const EdgeInsets.all(14),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        CircleAvatar(
                          radius: 20,
                          backgroundColor: Colors.white10,
                          backgroundImage: photoPath != null ? NetworkImage(photoPath) : null,
                          child: photoPath == null ? Text(person['fullName']?[0] ?? '?', style: const TextStyle(color: AppTheme.royalGold)) : null,
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(person['fullName'] ?? 'Alumnus', style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 13)),
                              Text('No. ${person['membershipNumber'] ?? 'N/A'}', style: const TextStyle(color: Colors.white54, fontSize: 10)),
                            ],
                          ),
                        ),
                        Container(
                          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                          decoration: BoxDecoration(
                            color: statusColor.withValues(alpha: 0.1),
                            borderRadius: BorderRadius.circular(6),
                            border: Border.all(color: statusColor.withValues(alpha: 0.3)),
                          ),
                          child: Text(statusText, style: TextStyle(color: statusColor, fontSize: 8, fontWeight: FontWeight.bold, letterSpacing: 0.5)),
                        ),
                      ],
                    ),
                    const SizedBox(height: 12),
                    if (req['domain'] != null && req['domain'].toString().isNotEmpty) ...[
                      Text('Domain: ${req['domain']}', style: const TextStyle(color: AppTheme.royalGold, fontSize: 11, fontWeight: FontWeight.bold)),
                      const SizedBox(height: 4),
                    ],
                    if (req['message'] != null && req['message'].toString().isNotEmpty)
                      Text('"${req['message']}"', style: const TextStyle(color: Colors.white70, fontSize: 12, fontStyle: FontStyle.italic)),
                    
                    if (req['responseNote'] != null) ...[
                       const SizedBox(height: 8),
                       Container(
                         padding: const EdgeInsets.all(8),
                         decoration: BoxDecoration(color: Colors.white.withValues(alpha: 0.05), borderRadius: BorderRadius.circular(8)),
                         child: Row(
                           crossAxisAlignment: CrossAxisAlignment.start,
                           children: [
                             const Icon(Icons.reply_outlined, color: Colors.white38, size: 14),
                             const SizedBox(width: 6),
                             Expanded(child: Text(req['responseNote'], style: const TextStyle(color: Colors.white, fontSize: 10))),
                           ],
                         ),
                       )
                    ],

                    if (!isSent && status == 0) ...[
                      const SizedBox(height: 16),
                      Row(
                        children: [
                          Expanded(
                            child: ElevatedButton(
                              style: ElevatedButton.styleFrom(backgroundColor: Colors.white10),
                              onPressed: _processingRequestId == req['id'] ? null : () => _respond(req['id'], false),
                              child: const Text('DECLINE', style: TextStyle(color: Colors.white54, fontSize: 11, fontWeight: FontWeight.bold)),
                            ),
                          ),
                          const SizedBox(width: 8),
                          Expanded(
                            child: ElevatedButton(
                              style: ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold),
                              onPressed: _processingRequestId == req['id'] ? null : () => _respond(req['id'], true),
                              child: const Text('ACCEPT', style: TextStyle(color: Colors.black, fontSize: 11, fontWeight: FontWeight.bold)),
                            ),
                          ),
                        ],
                      )
                    ],

                    if (status == 1) ...[ // Active
                      const SizedBox(height: 16),
                      SizedBox(
                        width: double.infinity,
                        child: OutlinedButton(
                          style: OutlinedButton.styleFrom(side: const BorderSide(color: Colors.blueAccent)),
                          onPressed: _processingRequestId == req['id'] ? null : () => _markComplete(req['id']),
                          child: const Text('MARK COMPLETED', style: TextStyle(color: Colors.blueAccent, fontSize: 10, fontWeight: FontWeight.bold, letterSpacing: 1)),
                        ),
                      )
                    ]
                  ],
                ),
              ),
            );
          },
        );
      },
      loading: () => const Center(child: LogoSpinner(size: 120)),
      error: (e, _) => Center(child: Text('Error: $e', style: const TextStyle(color: Colors.redAccent))),
    );
  }

  void _respond(int requestId, bool accept) {
    if (_processingRequestId != null) return;
    HapticFeedback.lightImpact();
    // Simplified: No note input in mobile yet, just quick accept/decline
    _completeRespondAction(requestId, accept, null);
  }

  Future<void> _completeRespondAction(int requestId, bool accept, String? note) async {
    setState(() => _processingRequestId = requestId);
    try {
      final success = await ref.read(mentorshipServiceProvider).respondToRequest(requestId, accept, note);
      if (!mounted) return;
      if (success) {
        ref.invalidate(receivedMentorshipsProvider);
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(accept ? 'Mentorship accepted.' : 'Mentorship declined.')));
      } else {
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Failed to update status.'), backgroundColor: Colors.redAccent));
      }
    } finally {
      if (mounted) setState(() => _processingRequestId = null);
    }
  }

  Future<void> _markComplete(int requestId) async {
    if (_processingRequestId != null) return;
    setState(() => _processingRequestId = requestId);
    try {
      final ok = await ref.read(mentorshipServiceProvider).markComplete(requestId);
      if (mounted) {
        if (ok) {
          ref.invalidate(sentMentorshipsProvider);
          ref.invalidate(receivedMentorshipsProvider);
          ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Mentorship marked as completed.')));
        }
      }
    } finally {
      if (mounted) setState(() => _processingRequestId = null);
    }
  }
}
