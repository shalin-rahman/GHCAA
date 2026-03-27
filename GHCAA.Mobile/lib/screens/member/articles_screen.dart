import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/async_value_widget.dart';

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
      title: 'Portal Submissions',
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () {},
        backgroundColor: AppTheme.royalGold,
        icon: const Icon(Icons.add, color: Colors.black),
        label: const Text('SUBMIT ARTICLE', style: TextStyle(color: Colors.black, fontWeight: FontWeight.bold, fontSize: 11)),
      ),
      child: AsyncValueWidget<List<dynamic>>(
        value: articlesAsync,
        loadingMessage: 'Loading submission history...',
        onRetry: () => ref.invalidate(myArticlesProvider),
        data: (articles) => articles.isEmpty 
          ? const Center(child: Padding(
              padding: EdgeInsets.all(40.0),
              child: Text('No community articles submitted yet.', textAlign: TextAlign.center, style: TextStyle(color: AppTheme.textSecondaryDark)),
            ))
          : ListView.builder(
              padding: const EdgeInsets.all(24),
              itemCount: articles.length,
              itemBuilder: (context, index) {
                final article = articles[index];
                return Padding(
                  padding: const EdgeInsets.only(bottom: 12.0),
                  child: GlassContainer(
                    child: ListTile(
                      title: Text(article['title'] ?? 'DRAFT ARTICLE', style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14, color: Colors.white)),
                      subtitle: Text('STATUS: ${article['status'] ?? 'PENDING REVIEW'}', style: const TextStyle(fontSize: 10, color: AppTheme.royalGold)),
                      trailing: const Icon(Icons.edit_note, color: Colors.white30),
                    ),
                  ),
                );
              },
            ),
      ),
    );
  }
}
