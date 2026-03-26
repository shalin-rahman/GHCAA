import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:share_plus/share_plus.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../core/widgets/skeleton_loader.dart';
import '../../features/content/content_service.dart';

final galleryItemsProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(newsServiceProvider).getGalleryItems();
});

class GalleryScreen extends ConsumerWidget {
  const GalleryScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final galleryAsync = ref.watch(galleryItemsProvider);

    return AppScaffold(
      title: 'Media Gallery',
      child: AsyncValueWidget(
        value: galleryAsync,
        loadingMessage: 'Curating association memories...',
        loadingWidget: SkeletonLoader.cardStub(),
        onRetry: () => ref.invalidate(galleryItemsProvider),
        data: (items) => GridView.builder(
          padding: const EdgeInsets.all(24),
          gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
            crossAxisCount: 2, 
            crossAxisSpacing: 16, 
            mainAxisSpacing: 16, 
            childAspectRatio: 0.8
          ),
          itemCount: items.length,
          itemBuilder: (context, index) {
            final item = items[index];
            return GestureDetector(
              onTap: () => _showImagePreview(context, item['imageUrl']),
              onLongPress: () {
                if (item['imageUrl'] != null) {
                  Share.share('Memories from GHCAA: ${item['imageUrl']}');
                }
              },
              child: GlassContainer(
                padding: EdgeInsets.zero,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    Expanded(
                      child: ClipRRect(
                        borderRadius: const BorderRadius.vertical(top: Radius.circular(16)),
                        child: Image.network(item['imageUrl'] ?? '', fit: BoxFit.cover, errorBuilder: (c,e,s) => const Icon(Icons.image_not_supported)),
                      ),
                    ),
                    Padding(
                      padding: const EdgeInsets.all(8.0),
                      child: Text(
                        item['title'] ?? 'Moment', 
                        style: const TextStyle(fontSize: 10, fontWeight: FontWeight.bold, color: Colors.white70),
                        textAlign: TextAlign.center,
                      ),
                    ),
                  ],
                ),
              ),
            );
          },
        ),
      ),
    );
  }

  void _showImagePreview(BuildContext context, String? url) {
    if (url == null) return;
    showDialog(
      context: context,
      builder: (context) => Dialog(
        backgroundColor: Colors.transparent,
        child: Stack(
          alignment: Alignment.topRight,
          children: [
            ClipRRect(
              borderRadius: BorderRadius.circular(24),
              child: InteractiveViewer(child: Image.network(url, errorBuilder: (c,e,s) => const Center(child: Icon(Icons.broken_image, size: 60, color: Colors.redAccent)))),
            ),
            IconButton(
              icon: const Icon(Icons.close, color: Colors.white),
              onPressed: () => Navigator.pop(context),
            ),
          ],
        ),
      ),
    );
  }
}
