import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/constants/app_constants.dart';
import '../../features/support/support_service.dart';

final familyListProvider = FutureProvider<List<dynamic>>((ref) => ref.read(familyServiceProvider).getFamilyLinks());

class FamilyLinkScreen extends ConsumerWidget {
  const FamilyLinkScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final familyAsync = ref.watch(familyListProvider);

    return AppScaffold(
      title: 'Family Members',
      floatingActionButton: FloatingActionButton(
        onPressed: () {},
        backgroundColor: AppTheme.royalGold,
        child: const Icon(Icons.add, color: Colors.white),
      ),
      child: familyAsync.when(
        data: (members) => members.isEmpty 
          ? const Center(child: Text('No family members linked yet.'))
          : ListView.builder(
              padding: const EdgeInsets.all(AppConstants.paddingLarge),
              itemCount: members.length,
              itemBuilder: (context, index) {
                final member = members[index];
                return Padding(
                  padding: const EdgeInsets.only(bottom: 12.0),
                  child: GlassContainer(
                    child: ListTile(
                      leading: const CircleAvatar(backgroundColor: AppTheme.royalGold, child: Icon(Icons.person, color: Colors.white)),
                      title: Text(member['name'] ?? 'Family Member', style: const TextStyle(fontWeight: FontWeight.bold)),
                      subtitle: Text(member['relation'] ?? 'Relative'),
                      trailing: const Icon(Icons.verified_user, size: 16, color: Colors.green),
                    ),
                  ),
                );
              },
            ),
        loading: () => const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
        error: (e, s) => Center(child: Text('Error: $e')),
      ),
    );
  }
}
