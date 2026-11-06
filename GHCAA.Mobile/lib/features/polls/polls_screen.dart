import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/constants/registration_constants.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/logo_spinner.dart';
import '../lookups/dropdown_service.dart';
import 'poll_comment_thread.dart';
import 'poll_service.dart';

class PollsScreen extends ConsumerWidget {
  const PollsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final pollsAsync = ref.watch(activePollsProvider);

    return Scaffold(
      backgroundColor: AppTheme.midnightBase,
      appBar: AppBar(
        title: const Text('Member Polls', style: TextStyle(fontWeight: FontWeight.w800)),
        backgroundColor: AppTheme.midnightSurface,
        elevation: 0,
      ),
      body: Column(
        children: [
          const _CategoryFilter(),
          Expanded(child: _buildPolls(ref, pollsAsync)),
        ],
      ),
    );
  }

  Widget _buildPolls(WidgetRef ref, AsyncValue<List<Poll>> pollsAsync) {
    return pollsAsync.when(
        loading: () => const Center(child: LogoSpinner(size: 120)),
        error: (err, stack) => Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Icon(Icons.error_outline, color: Colors.redAccent, size: 48),
              const SizedBox(height: 16),
              const Text('Failed to load polls', style: TextStyle(color: Colors.white70)),
              TextButton(
                onPressed: () => ref.refresh(activePollsProvider),
                child: const Text('Retry', style: TextStyle(color: AppTheme.royalGold)),
              ),
            ],
          ),
        ),
        data: (polls) {
          if (polls.isEmpty) {
            return const Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(Icons.poll_outlined, color: Colors.white24, size: 64),
                  SizedBox(height: 16),
                  Text('No Active Polls', 
                      style: TextStyle(color: Colors.white70, fontSize: 18, fontWeight: FontWeight.bold)),
                  SizedBox(height: 8),
                  Text('Check back later for new votes.', style: TextStyle(color: Colors.white54)),
                ],
              ),
            );
          }

          return RefreshIndicator(
            onRefresh: () async => ref.refresh(activePollsProvider),
            color: AppTheme.royalGold,
            child: ListView.builder(
              padding: const EdgeInsets.all(16),
              itemCount: polls.length,
              itemBuilder: (context, index) {
                return _PollCard(poll: polls[index]);
              },
            ),
          );
        },
    );
  }
}

final _discussionCategoriesProvider = FutureProvider.autoDispose<List<Map<String, String>>>(
    (ref) => ref.read(dropdownDataProvider).getOptions(LookupGroups.discussionCategory));

class _CategoryFilter extends ConsumerWidget {
  const _CategoryFilter();

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final categories = ref.watch(_discussionCategoriesProvider).value ?? const [];
    // One category or none means there is nothing to filter by.
    if (categories.length < 2) return const SizedBox.shrink();
    final selected = ref.watch(pollCategoryFilterProvider);

    Widget chip(String? value, String label) => Padding(
          padding: const EdgeInsets.only(right: 8),
          child: ChoiceChip(
            label: Text(label),
            selected: selected == value,
            onSelected: (_) => ref.read(pollCategoryFilterProvider.notifier).state = value,
          ),
        );

    return SizedBox(
      height: 48,
      child: ListView(
        scrollDirection: Axis.horizontal,
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
        children: [
          chip(null, 'All'),
          for (final c in categories) chip(c['value'], c['label'] ?? c['value']!),
        ],
      ),
    );
  }
}

class _PollCard extends ConsumerStatefulWidget {
  final Poll poll;
  const _PollCard({required this.poll});

  @override
  ConsumerState<_PollCard> createState() => _PollCardState();
}

class _PollCardState extends ConsumerState<_PollCard> {
  bool _isSubmitting = false;
  bool _showComments = false;
  late int _commentCount = widget.poll.commentCount;

  // A poll with comments turned off still shows the old ones.
  bool get _hasThread => widget.poll.allowComments || _commentCount > 0;

  void _toggleOption(int optionId) {
    if (widget.poll.hasVoted) return;

    setState(() {
      if (!widget.poll.allowMultipleChoice) {
        widget.poll.selectedOptionIds = [optionId];
      } else {
        if (widget.poll.selectedOptionIds.contains(optionId)) {
          widget.poll.selectedOptionIds.remove(optionId);
        } else {
          widget.poll.selectedOptionIds.add(optionId);
        }
      }
    });
  }

  Future<void> _submitVote() async {
    if (widget.poll.selectedOptionIds.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please select an option first.')),
      );
      return;
    }

    setState(() => _isSubmitting = true);
    final success = await ref.read(pollServiceProvider).vote(widget.poll.id, widget.poll.selectedOptionIds);
    setState(() => _isSubmitting = false);
    
    if (!mounted) return;

    if (success) {
      HapticFeedback.heavyImpact();
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Vote submitted successfully!')),
      );
      ref.invalidate(activePollsProvider);
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Failed to submit vote.')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 16),
      color: AppTheme.midnightSurface,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16), side: const BorderSide(color: Colors.white12)),
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Row(
              children: [
                Expanded(
                  child: Text(
                    widget.poll.title,
                    style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Colors.white),
                  ),
                ),
                if (widget.poll.hasVoted)
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                    decoration: BoxDecoration(color: Colors.green.withValues(alpha: 0.2), borderRadius: BorderRadius.circular(12)),
                    child: const Text('VOTED', style: TextStyle(color: Colors.greenAccent, fontSize: 10, fontWeight: FontWeight.bold)),
                  ),
              ],
            ),
            if (widget.poll.description != null && widget.poll.description!.isNotEmpty) ...[
              const SizedBox(height: 8),
              Text(widget.poll.description!, style: const TextStyle(color: Colors.white70, fontSize: 13)),
            ],
            const SizedBox(height: 20),

            if (!widget.poll.hasVoted) ...[
              ...widget.poll.options.map((opt) {
                final isSelected = widget.poll.selectedOptionIds.contains(opt.id);
                return Padding(
                  padding: const EdgeInsets.only(bottom: 8.0),
                  child: InkWell(
                    onTap: () {
                      HapticFeedback.selectionClick();
                      _toggleOption(opt.id);
                    },
                    borderRadius: BorderRadius.circular(12),
                    child: Container(
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        border: Border.all(color: isSelected ? AppTheme.royalGold : Colors.white12),
                        borderRadius: BorderRadius.circular(12),
                        color: isSelected ? AppTheme.royalGold.withValues(alpha: 0.1) : Colors.transparent,
                      ),
                      child: Row(
                        children: [
                          Icon(
                            isSelected ? Icons.check_circle : Icons.circle_outlined,
                            color: isSelected ? AppTheme.royalGold : Colors.white38,
                          ),
                          const SizedBox(width: 12),
                          Expanded(child: Text(opt.text, style: const TextStyle(color: Colors.white))),
                        ],
                      ),
                    ),
                  ),
                );
              }),
              const SizedBox(height: 12),
              ElevatedButton(
                onPressed: _isSubmitting || widget.poll.selectedOptionIds.isEmpty ? null : _submitVote,
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppTheme.royalGold,
                  foregroundColor: Colors.black,
                  padding: const EdgeInsets.symmetric(vertical: 14),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                ),
                child: _isSubmitting 
                  ? SizedBox(width: 20, height: 20, child: LogoSpinner.small())
                  : const Text('SUBMIT VOTE', style: TextStyle(fontWeight: FontWeight.bold)),
              ),
              if (widget.poll.allowMultipleChoice)
                const Padding(
                  padding: EdgeInsets.only(top: 8.0),
                  child: Text('Multiple selections allowed', textAlign: TextAlign.center, style: TextStyle(color: Colors.white54, fontSize: 11)),
                ),
            ] else ...[
              ...widget.poll.options.map((opt) {
                final isSelected = widget.poll.selectedOptionIds.contains(opt.id);
                return Padding(
                  padding: const EdgeInsets.only(bottom: 12.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Expanded(
                            child: Row(
                              children: [
                                if (isSelected) const Icon(Icons.check, color: AppTheme.royalGold, size: 14),
                                if (isSelected) const SizedBox(width: 4),
                                Expanded(
                                  child: Text(opt.text, style: TextStyle(color: isSelected ? AppTheme.royalGold : Colors.white, fontWeight: isSelected ? FontWeight.bold : FontWeight.normal)),
                                ),
                              ],
                            ),
                          ),
                          Text('${opt.percentage}%', style: const TextStyle(color: Colors.white70, fontWeight: FontWeight.bold)),
                        ],
                      ),
                      const SizedBox(height: 6),
                      ClipRRect(
                        borderRadius: BorderRadius.circular(4),
                        child: LinearProgressIndicator(
                          value: opt.percentage / 100,
                          backgroundColor: Colors.white10,
                          color: isSelected ? AppTheme.royalGold : Colors.white38,
                          minHeight: 6,
                        ),
                      ),
                    ],
                  ),
                );
              }),
              const SizedBox(height: 8),
              Text('Total Participants: ${widget.poll.totalVotes}', style: const TextStyle(color: Colors.white54, fontSize: 12), textAlign: TextAlign.right),
            ],
            const SizedBox(height: 8),
            Row(
              children: [
                Expanded(
                  child: Text(widget.poll.category, style: const TextStyle(color: Colors.white38, fontSize: 11)),
                ),
                if (_hasThread)
                  TextButton.icon(
                    key: Key('poll-comments-toggle-${widget.poll.id}'),
                    onPressed: () => setState(() => _showComments = !_showComments),
                    icon: Icon(_showComments ? Icons.expand_less : Icons.chat_bubble_outline, size: 16, color: AppTheme.royalGold),
                    label: Text('Comments ($_commentCount)', style: const TextStyle(color: AppTheme.royalGold, fontSize: 12)),
                  ),
              ],
            ),
            if (_showComments)
              PollCommentThread(
                pollId: widget.poll.id,
                onCountChange: (n) => setState(() => _commentCount = n),
              ),
          ],
        ),
      ),
    );
  }
}
