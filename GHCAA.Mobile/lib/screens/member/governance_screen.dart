import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/governance_api.dart';
import '../../core/widgets/app_scaffold.dart';

final currentECProvider = FutureProvider<Map<String, dynamic>>((ref) async {
  return await ref.read(governanceApiProvider).getCurrentEC();
});

final activeConstitutionProvider = FutureProvider<Map<String, dynamic>>((ref) async {
  return await ref.read(governanceApiProvider).getCurrentConstitution();
});

class GovernanceScreen extends ConsumerWidget {
  const GovernanceScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final ec = ref.watch(currentECProvider);
    final constitution = ref.watch(activeConstitutionProvider);

    return AppScaffold(
      title: 'Institutional Governance',
      child: DefaultTabController(
        length: 2,
        child: Column(
          children: [
            const TabBar(
              tabs: [
                Tab(text: 'Current EC', icon: Icon(Icons.group)),
                Tab(text: 'Constitution', icon: Icon(Icons.gavel)),
              ],
            ),
            Expanded(
              child: TabBarView(
                children: [
                   _buildECView(context, ec),
                   _buildConstitutionView(context, constitution, ref),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildECView(BuildContext context, AsyncValue<Map<String, dynamic>> ec) {
    return ec.when(
      data: (data) {
        final period = data['period'];
        final members = data['members'] as List<dynamic>;

        return ListView(
          padding: const EdgeInsets.all(16.0),
          children: [
            Card(
              child: ListTile(
                title: Text(period['title'], style: const TextStyle(fontWeight: FontWeight.bold)),
                subtitle: Text('Tenure: ${DateTime.parse(period['startDate']).year} - ${period['endDate'] != null ? DateTime.parse(period['endDate']).year : 'Present'}'),
                trailing: const Chip(label: Text('ACTIVE'), backgroundColor: Colors.green),
              ),
            ),
            const SizedBox(height: 20),
            const Text('Executive Committee Members', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
            const Divider(),
            ...members.map((m) {
              final member = m['member'];
              final position = m['positionLabel'] ?? m['position'].toString();
              
              return ListTile(
                leading: CircleAvatar(
                  backgroundImage: member['photoPath'] != null 
                    ? NetworkImage(member['photoPath']) 
                    : null,
                  child: member['photoPath'] == null ? const Icon(Icons.person) : null,
                ),
                title: Text(member['fullName']),
                subtitle: Text(position),
                trailing: Text(member['membershipNumber'] ?? ''),
              );
            }),
          ],
        );
      },
      loading: () => const Center(child: CircularProgressIndicator()),
      error: (e, s) => Center(child: Text('Error: $e')),
    );
  }

  Widget _buildConstitutionView(BuildContext context, AsyncValue<Map<String, dynamic>> consti, WidgetRef ref) {
    return consti.when(
      data: (data) {
        final version = data['version'];
        final changes = data['changeSummary'];
        final effective = DateTime.parse(data['effectiveDate']);

        return ListView(
          padding: const EdgeInsets.all(16.0),
          children: [
             Card(
              color: Colors.blueGrey[900],
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text('By-Laws Version $version', style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: Colors.amber)),
                        const Icon(Icons.verified, color: Colors.amber),
                      ],
                    ),
                    const SizedBox(height: 8),
                    Text('Effective from ${effective.day}/${effective.month}/${effective.year}', style: const TextStyle(color: Colors.white70)),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 20),
            const Text('Revision Notes', style: TextStyle(fontWeight: FontWeight.bold)),
            Padding(
              padding: const EdgeInsets.symmetric(vertical: 8.0),
              child: Text(changes),
            ),
            const SizedBox(height: 20),
            ElevatedButton.icon(
              onPressed: () => _viewFullConstitution(context, data['content']),
              icon: const Icon(Icons.description),
              label: const Text('Read Full Constitution'),
              style: ElevatedButton.styleFrom(minimumSize: const Size(double.infinity, 50)),
            ),
            const SizedBox(height: 20),
            const Divider(),
            const Padding(
              padding: EdgeInsets.symmetric(vertical: 10),
              child: Text('Democratic Participation', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
            ),
            const Text('An amendment is being proposed for this version. Cast your vote as a verified member.'),
            const SizedBox(height: 10),
            Row(
              children: [
                Expanded(
                  child: OutlinedButton.icon(
                    onPressed: () => _vote(context, ref, data['id'], true),
                    icon: const Icon(Icons.thumb_up, color: Colors.green),
                    label: const Text('SUPPORT'),
                  ),
                ),
                const SizedBox(width: 10),
                Expanded(
                  child: OutlinedButton.icon(
                    onPressed: () => _vote(context, ref, data['id'], false),
                    icon: const Icon(Icons.thumb_down, color: Colors.red),
                    label: const Text('OPPOSE'),
                  ),
                ),
              ],
            ),
          ],
        );
      },
      loading: () => const Center(child: CircularProgressIndicator()),
      error: (e, s) => Center(child: Text('Not Available: $e')),
    );
  }

  void _viewFullConstitution(BuildContext context, String content) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      builder: (context) => DraggableScrollableSheet(
        initialChildSize: 0.9,
        builder: (context, scrollController) => Container(
          padding: const EdgeInsets.all(20),
          decoration: const BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
          ),
          child: ListView(
            controller: scrollController,
            children: [
              const Text('Digital Constitution', style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold)),
              const SizedBox(height: 20),
              Text(content, style: const TextStyle(fontSize: 16, height: 1.5)),
            ],
          ),
        ),
      ),
    );
  }

  void _vote(BuildContext context, WidgetRef ref, int id, bool isFor) async {
    final success = await ref.read(governanceApiProvider).voteOnAmendment(id, isFor);
    if (context.mounted) {
       ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(success ? 'Your vote has been securely recorded.' : 'Voting failed. You may have already voted.'),
          backgroundColor: success ? Colors.green : Colors.red,
        ),
      );
    }
  }
}
