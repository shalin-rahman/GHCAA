import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/constants/app_constants.dart';

final myArticlesProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  // Mock logic or call /api/news/my-submissions
  return []; 
});

class MemberArticlesScreen extends ConsumerWidget {
  const MemberArticlesScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final articlesAsync = ref.watch(myArticlesProvider);

    return AppScaffold(
      title: 'My Submissions',
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () {},
        backgroundColor: AppTheme.royalGold,
        icon: const Icon(Icons.add, color: Colors.white),
        label: const Text('New Article', style: TextStyle(color: Colors.white)),
      ),
      child: articlesAsync.when(
        data: (articles) => articles.isEmpty 
          ? const Center(child: Text('No articles submitted yet.'))
          : ListView.builder(
              padding: const EdgeInsets.all(AppConstants.paddingLarge),
              itemCount: articles.length,
              itemBuilder: (context, index) {
                final article = articles[index];
                return Padding(
                  padding: const EdgeInsets.only(bottom: 12.0),
                  child: GlassContainer(
                    child: ListTile(
                      title: Text(article['title'] ?? 'Draft', style: const TextStyle(fontWeight: FontWeight.bold)),
                      subtitle: Text('Status: ${article['status'] ?? 'Pending'}'),
                      trailing: const Icon(Icons.edit_note),
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
