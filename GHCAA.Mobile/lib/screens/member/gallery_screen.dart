import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../features/content/content_service.dart';
import '../../core/theme/app_theme.dart';
import '../../core/config/app_config.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import 'package:flutter/services.dart';

final galleryItemsProvider =
    FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(newsServiceProvider).getGalleryItems();
});

class GalleryScreen extends ConsumerStatefulWidget {
  const GalleryScreen({super.key});

  @override
  ConsumerState<GalleryScreen> createState() => _GalleryScreenState();
}

class _GalleryScreenState extends ConsumerState<GalleryScreen> {

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
              child: InteractiveViewer(
                  child: Image.network(url,
                      errorBuilder: (c, e, s) => const Center(
                          child: Icon(Icons.broken_image,
                              size: 60, color: Colors.redAccent)))),
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

  @override
  Widget build(BuildContext context) {
    final ref = this.ref;
    final galleryAsync = ref.watch(galleryItemsProvider);

    return AppScaffold(
      title: 'Memories & Moments',
      breadcrumb: 'Member Portal > Media Gallery',
      child: AsyncValueWidget<List<dynamic>>(
        value: galleryAsync,
        loadingMessage: 'Curating association memories...',
        onRetry: () => ref.invalidate(galleryItemsProvider),
        data: (galleries) => ListView.builder(
          padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
          itemCount: galleries.length,
          itemBuilder: (context, index) {
            final g = galleries[index];
            final photos = g['photos'] as List? ?? [];
            final coverPath = photos.isNotEmpty ? photos[0]['photoPath'] : null;
            final fullCoverUrl = coverPath != null 
                ? (coverPath.toString().startsWith('http') 
                    ? coverPath 
                    : '${AppConfig.apiBaseUrl}/$coverPath'.replaceAll('//', '/'))
                : null;

            return Padding(
              padding: const EdgeInsets.only(bottom: 24.0),
              child: GlassContainer(
                padding: EdgeInsets.zero,
                child: InkWell(
                  onTap: () {
                    HapticFeedback.lightImpact();
                    _showImagePreview(context, fullCoverUrl);
                  },
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      Container(
                        height: 200,
                        decoration: BoxDecoration(
                          color: AppTheme.royalGold.withValues(alpha: 0.05),
                          borderRadius: const BorderRadius.vertical(top: Radius.circular(16)),
                          image: fullCoverUrl != null ? DecorationImage(image: NetworkImage(fullCoverUrl), fit: BoxFit.cover) : null,
                        ),
                        child: Stack(
                          children: [
                            if (fullCoverUrl == null)
                              Center(child: Icon(Icons.camera_roll_outlined, size: 64, color: AppTheme.royalGold.withValues(alpha: 0.1))),
                            Positioned(
                              top: 16,
                              right: 16,
                              child: Container(
                                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
                                decoration: BoxDecoration(color: Colors.black87, borderRadius: BorderRadius.circular(6), border: Border.all(color: AppTheme.royalGold.withValues(alpha: 0.3))),
                                child: Text('${photos.length} PHOTOS', style: const TextStyle(color: AppTheme.royalGold, fontSize: 8, fontWeight: FontWeight.w900, letterSpacing: 1.5)),
                              ),
                            ),
                            Positioned(
                              bottom: 0,
                              left: 0,
                              right: 0,
                              child: Container(
                                padding: const EdgeInsets.all(12),
                                decoration: BoxDecoration(gradient: LinearGradient(begin: Alignment.bottomCenter, end: Alignment.topCenter, colors: [Colors.black.withValues(alpha: 0.8), Colors.transparent])),
                                child: Row(
                                  children: [
                                    const Icon(Icons.calendar_month_outlined, size: 14, color: AppTheme.royalGold),
                                    const SizedBox(width: 8),
                                    Text(g['eventDate']?.toString().split('T')[0] ?? 'RECENT', style: const TextStyle(color: Colors.white, fontSize: 10, fontWeight: FontWeight.w900, letterSpacing: 0.5)),
                                  ],
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                      Padding(
                        padding: const EdgeInsets.all(20.0),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(g['title'] ?? 'ALUMNI GATHERING', style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w900, color: Colors.white, height: 1.3, letterSpacing: -0.2)),
                            const SizedBox(height: 6),
                            Row(
                              children: [
                                const Icon(Icons.pin_drop_outlined, size: 12, color: AppTheme.royalGold),
                                const SizedBox(width: 6),
                                Text(g['location'] ?? 'ASSOCIATION HUB', style: const TextStyle(fontSize: 11, color: AppTheme.textSecondaryDark, fontWeight: FontWeight.bold, letterSpacing: 0.5)),
                              ],
                            ),
                            const SizedBox(height: 14),
                            Text(g['description'] ?? 'Event memories from the Haragangian association archives.', style: const TextStyle(fontSize: 13, height: 1.6, color: AppTheme.textSecondaryDark), maxLines: 2, overflow: TextOverflow.ellipsis),
                          ],
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            );
          },
        ),
      ),
    );
  }
}
