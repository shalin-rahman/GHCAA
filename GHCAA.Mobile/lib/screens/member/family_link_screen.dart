import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../features/family/family_service.dart';

class FamilyLinkScreen extends ConsumerWidget {
  const FamilyLinkScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final familyAsync = ref.watch(familyListProvider);

    return AppScaffold(
      title: 'Legacy Family',
      breadcrumb: 'Identity Registry > Legacy Connections',
      floatingActionButton: FloatingActionButton(
        onPressed: () {
          HapticFeedback.lightImpact();
        },
        backgroundColor: AppTheme.royalGold,
        child: const Icon(Icons.add_link_rounded, color: Colors.black),
      ),
      child: familyAsync.when(
        data: (members) => members.isEmpty 
          ? const Center(child: Text('No legacy family connections registered.', style: TextStyle(color: AppTheme.textSecondaryDark)))
          : RefreshIndicator(
              color: AppTheme.royalGold,
              onRefresh: () async {
                HapticFeedback.mediumImpact();
                ref.invalidate(familyListProvider);
              },
              child: ListView.builder(
                padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 24),
                itemCount: members.length,
                itemBuilder: (context, index) {
                  final member = members[index];
                  return Padding(
                    padding: const EdgeInsets.only(bottom: 12.0),
                    child: GlassContainer(
                      padding: const EdgeInsets.all(12),
                      child: Row(
                        children: [
                          Container(
                            padding: const EdgeInsets.all(10),
                            decoration: BoxDecoration(color: AppTheme.royalGold.withValues(alpha: 0.1), shape: BoxShape.circle),
                            child: const Icon(Icons.person_pin_outlined, color: AppTheme.royalGold, size: 20),
                          ),
                          const SizedBox(width: 16),
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(member['name']?.toUpperCase() ?? 'FAMILY MEMBER', style: const TextStyle(fontWeight: FontWeight.w900, color: Colors.white, fontSize: 13, letterSpacing: 0.5)),
                                const SizedBox(height: 4),
                                Text(member['relation']?.toUpperCase() ?? 'RELATIVE', style: const TextStyle(fontSize: 9, color: AppTheme.royalGold, fontWeight: FontWeight.w900, letterSpacing: 1)),
                              ],
                            ),
                          ),
                          const Icon(Icons.verified_user_outlined, size: 14, color: Colors.green),
                        ],
                      ),
                    ),
                  );
                },
              ),
            ),
        loading: () => const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
        error: (e, s) => Center(child: Text('Error: $e', style: const TextStyle(color: Colors.redAccent))),
      ),
    );
  }
}
