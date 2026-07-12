import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../features/networking/family_service.dart';
import '../../core/config/app_config.dart';
import '../../core/widgets/empty_state_widget.dart';

final familyListProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(familyServiceProvider).getMyFamily();
});

final sentRequestsProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(familyServiceProvider).getSentRequests();
});

final receivedRequestsProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(familyServiceProvider).getReceivedRequests();
});

class FamilyLinkScreen extends ConsumerStatefulWidget {
  const FamilyLinkScreen({super.key});

  @override
  ConsumerState<FamilyLinkScreen> createState() => _FamilyLinkScreenState();
}

class _FamilyLinkScreenState extends ConsumerState<FamilyLinkScreen> with SingleTickerProviderStateMixin {
  late TabController _tabController;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 3, vsync: this);
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final familyAsync = ref.watch(familyListProvider);
    final sentAsync = ref.watch(sentRequestsProvider);
    final receivedAsync = ref.watch(receivedRequestsProvider);

    return AppScaffold(
      title: 'Family Networking',
      breadcrumb: 'PORTAL > FAMILY',
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _showSendRequestDialog(context),
        backgroundColor: AppTheme.royalGold,
        icon: const Icon(Icons.person_add_alt_1_outlined, color: Colors.black),
        label: const Text('LINK FAMILY', style: TextStyle(color: Colors.black, fontWeight: FontWeight.w900, fontSize: 11)),
      ),
      child: Column(
        children: [
          TabBar(
            controller: _tabController,
            indicatorColor: AppTheme.royalGold,
            labelColor: AppTheme.royalGold,
            unselectedLabelColor: Colors.white54,
            labelStyle: const TextStyle(fontSize: 10, fontWeight: FontWeight.bold, letterSpacing: 1),
            tabs: const [
              Tab(text: 'MY FAMILY', icon: Icon(Icons.people_outline, size: 20)),
              Tab(text: 'RECEIVED', icon: Icon(Icons.move_to_inbox_outlined, size: 20)),
              Tab(text: 'SENT', icon: Icon(Icons.outbox_outlined, size: 20)),
            ],
          ),
          Expanded(
            child: TabBarView(
              controller: _tabController,
              children: [
                _buildFamilyList(familyAsync),
                _buildReceivedList(receivedAsync),
                _buildSentList(sentAsync),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildFamilyList(AsyncValue<List<dynamic>> async) {
    return async.when(
      data: (members) => members.isEmpty
          ? const EmptyStateWidget('No family members linked yet.', icon: Icons.people_outline)
          : ListView.builder(
              padding: const EdgeInsets.all(20),
              itemCount: members.length,
              itemBuilder: (context, index) {
                final m = members[index];
                return _buildMemberCard(m, isMember: true);
              },
            ),
      loading: () => const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
      error: (e, s) => Center(child: Text('Error: $e')),
    );
  }

  Widget _buildReceivedList(AsyncValue<List<dynamic>> async) {
    return async.when(
      data: (requests) => requests.isEmpty
          ? const EmptyStateWidget('No pending received requests.', icon: Icons.move_to_inbox_outlined)
          : ListView.builder(
              padding: const EdgeInsets.all(20),
              itemCount: requests.length,
              itemBuilder: (context, index) {
                final r = requests[index];
                return _buildRequestCard(r, isReceived: true);
              },
            ),
      loading: () => const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
      error: (e, s) => Center(child: Text('Error: $e')),
    );
  }

  Widget _buildSentList(AsyncValue<List<dynamic>> async) {
    return async.when(
      data: (requests) => requests.isEmpty
          ? const EmptyStateWidget('No pending sent requests.', icon: Icons.outbox_outlined)
          : ListView.builder(
              padding: const EdgeInsets.all(20),
              itemCount: requests.length,
              itemBuilder: (context, index) {
                final r = requests[index];
                return _buildRequestCard(r, isReceived: false);
              },
            ),
      loading: () => const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
      error: (e, s) => Center(child: Text('Error: $e')),
    );
  }

  Widget _buildMemberCard(Map<String, dynamic> m, {required bool isMember}) {
    final photoPath = m['photoPath'] ?? m['targetMemberPhotoPath'] ?? m['requesterPhotoPath'];
    String? fullUrl;
    if (photoPath != null && photoPath.isNotEmpty) {
      if (photoPath.startsWith('http')) {
        fullUrl = photoPath;
      } else {
        final base = AppConfig.apiBaseUrl.replaceFirst('/api', '');
        fullUrl = '$base/$photoPath';
      }
    }

    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: GlassContainer(
        padding: const EdgeInsets.all(12),
        child: Row(
          children: [
            CircleAvatar(
              radius: 25,
              backgroundColor: AppTheme.royalGold.withValues(alpha: 0.1),
              backgroundImage: fullUrl != null ? NetworkImage(fullUrl) : null,
              child: fullUrl == null ? const Icon(Icons.person, color: AppTheme.royalGold) : null,
            ),
            const SizedBox(width: 16),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(m['fullName'] ?? m['targetMemberName'] ?? m['requesterName'] ?? 'Unknown Member', style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 13)),
                  Text(m['relationship']?.toString().toUpperCase() ?? 'FAMILY', style: const TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.bold, letterSpacing: 1)),
                ],
              ),
            ),
            if (isMember)
              IconButton(
                icon: const Icon(Icons.link_off, color: Colors.redAccent, size: 20),
                onPressed: () => _confirmRemoveLink(m['id']),
              ),
          ],
        ),
      ),
    );
  }

  Widget _buildRequestCard(Map<String, dynamic> r, {required bool isReceived}) {
    final name = isReceived ? r['requesterName'] : r['targetMemberName'];
    
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: GlassContainer(
        padding: const EdgeInsets.all(12),
        child: Column(
          children: [
            Row(
              children: [
                CircleAvatar(
                  radius: 20,
                  backgroundColor: AppTheme.royalGold.withValues(alpha: 0.1),
                  child: const Icon(Icons.person_pin_outlined, color: AppTheme.royalGold),
                ),
                const SizedBox(width: 16),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(name ?? 'Member', style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 13)),
                      Text('Relationship: ${r['relationship']}'.toUpperCase(), style: const TextStyle(color: AppTheme.royalGold, fontSize: 9, fontWeight: FontWeight.bold)),
                    ],
                  ),
                ),
                if (isReceived) ...[
                  IconButton(
                    icon: const Icon(Icons.check_circle_outline, color: Colors.greenAccent),
                    onPressed: () => _handleResponse(r['id'], true),
                  ),
                  IconButton(
                    icon: const Icon(Icons.cancel_outlined, color: Colors.redAccent),
                    onPressed: () => _handleResponse(r['id'], false),
                  ),
                ] else
                  IconButton(
                    icon: const Icon(Icons.delete_sweep_outlined, color: Colors.white38),
                    onPressed: () => _confirmCancelRequest(r['id']),
                  ),
              ],
            ),
            if (r['note'] != null && r['note'].toString().isNotEmpty) ...[
              const Divider(color: Colors.white10),
              Align(
                alignment: Alignment.centerLeft,
                child: Text('Note: ${r['note']}', style: const TextStyle(color: Colors.white60, fontSize: 11, fontStyle: FontStyle.italic)),
              ),
            ],
          ],
        ),
      ),
    );
  }

  Future<void> _showSendRequestDialog(BuildContext context) async {
    final membershipCtrl = TextEditingController();
    final noteCtrl = TextEditingController();
    final searchCtrl = TextEditingController();
    int selectedRel = 0; // Spouse
    List<dynamic> searchResults = [];
    bool isSearching = false;

    final result = await showDialog<bool>(
      context: context,
      builder: (context) => StatefulBuilder(
        builder: (context, setState) => AlertDialog(
          backgroundColor: AppTheme.midnightSurface,
          title: const Text('LINK FAMILY MEMBER', style: TextStyle(color: AppTheme.royalGold, fontSize: 14, fontWeight: FontWeight.bold)),
          content: SingleChildScrollView(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Row(
                  children: [
                    Expanded(
                      child: TextField(
                        controller: searchCtrl,
                        style: const TextStyle(color: Colors.white, fontSize: 12),
                        decoration: const InputDecoration(labelText: 'Search Member by Name')
                      )
                    ),
                    IconButton(
                      icon: isSearching 
                        ? const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(color: AppTheme.royalGold, strokeWidth: 2)) 
                        : const Icon(Icons.search, color: AppTheme.royalGold),
                      onPressed: () async {
                        if (searchCtrl.text.length < 3) return;
                        setState(() => isSearching = true);
                        searchResults = await ref.read(familyServiceProvider).searchFamilyMembers(searchCtrl.text);
                        setState(() => isSearching = false);
                      },
                    )
                  ],
                ),
                if (searchResults.isNotEmpty)
                  Container(
                    height: 100,
                    margin: const EdgeInsets.only(top: 8, bottom: 8),
                    decoration: BoxDecoration(color: Colors.black12, borderRadius: BorderRadius.circular(8)),
                    child: ListView.builder(
                      shrinkWrap: true,
                      itemCount: searchResults.length,
                      itemBuilder: (c, i) {
                        final m = searchResults[i];
                        return ListTile(
                          title: Text(m['fullName'] ?? '', style: const TextStyle(color: Colors.white, fontSize: 12)),
                          subtitle: Text(m['membershipNumber'] ?? '', style: const TextStyle(color: Colors.white54, fontSize: 10)),
                          onTap: () {
                            membershipCtrl.text = m['membershipNumber'] ?? '';
                            setState(() => searchResults = []);
                          },
                        );
                      }
                    )
                  ),
                const SizedBox(height: 12),
                TextField(controller: membershipCtrl, style: const TextStyle(color: Colors.white, fontSize: 12), decoration: const InputDecoration(labelText: 'Target Membership No (e.g. REG-001)')),
                const SizedBox(height: 16),
                DropdownButtonFormField<int>(
                  initialValue: selectedRel,
                  dropdownColor: AppTheme.midnightSurface,
                  decoration: const InputDecoration(labelText: 'Relationship'),
                  items: const [
                    DropdownMenuItem(value: 0, child: Text('Spouse', style: TextStyle(color: Colors.white, fontSize: 12))),
                    DropdownMenuItem(value: 1, child: Text('Parent', style: TextStyle(color: Colors.white, fontSize: 12))),
                    DropdownMenuItem(value: 2, child: Text('Child', style: TextStyle(color: Colors.white, fontSize: 12))),
                    DropdownMenuItem(value: 3, child: Text('Sibling', style: TextStyle(color: Colors.white, fontSize: 12))),
                    DropdownMenuItem(value: 4, child: Text('Other', style: TextStyle(color: Colors.white, fontSize: 12))),
                  ],
                  onChanged: (v) => setState(() => selectedRel = v ?? 0),
                ),
                TextField(controller: noteCtrl, style: const TextStyle(color: Colors.white, fontSize: 12), decoration: const InputDecoration(labelText: 'Personal Note (Optional)')),
              ],
            ),
          ),
          actions: [
            TextButton(onPressed: () => Navigator.pop(context), child: const Text('CANCEL')),
            ElevatedButton(
              onPressed: () => Navigator.pop(context, true),
              style: ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold),
              child: const Text('SEND REQUEST', style: TextStyle(color: Colors.black, fontWeight: FontWeight.bold)),
            ),
          ],
        ),
      ),
    );

    if (result == true && membershipCtrl.text.isNotEmpty) {
      final success = await ref.read(familyServiceProvider).sendRequest(membershipCtrl.text, selectedRel, note: noteCtrl.text);
      if (success) {
        ref.invalidate(sentRequestsProvider);
        if (context.mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Verification request dispatched.')));
      } else {
        if (context.mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Failed to send request. Is the member ID correct?'), backgroundColor: Colors.redAccent));
      }
    }
  }

  Future<void> _handleResponse(int id, bool approve) async {
    final success = await ref.read(familyServiceProvider).respondToRequest(id, approve);
    if (success) {
      ref.invalidate(receivedRequestsProvider);
      ref.invalidate(familyListProvider);
    }
  }

  Future<void> _confirmCancelRequest(int id) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        backgroundColor: AppTheme.midnightSurface,
        title: const Text('Cancel Request?'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context), child: const Text('NO')),
          TextButton(onPressed: () => Navigator.pop(context, true), child: const Text('YES', style: TextStyle(color: Colors.redAccent))),
        ],
      ),
    );
    if (confirm == true) {
      final success = await ref.read(familyServiceProvider).cancelRequest(id);
      if (success) ref.invalidate(sentRequestsProvider);
    }
  }

  Future<void> _confirmRemoveLink(int id) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        backgroundColor: AppTheme.midnightSurface,
        title: const Text('Remove Family Link?'),
        content: const Text('This will decouple your digital identity from this member.'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context), child: const Text('CANCEL')),
          TextButton(onPressed: () => Navigator.pop(context, true), child: const Text('REMOVE', style: TextStyle(color: Colors.redAccent))),
        ],
      ),
    );
    if (confirm == true) {
      final success = await ref.read(familyServiceProvider).removeLink(id);
      if (success) ref.invalidate(familyListProvider);
    }
  }
}
