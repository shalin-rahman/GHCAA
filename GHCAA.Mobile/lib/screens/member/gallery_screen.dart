import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../features/content/content_service.dart';
import '../../core/theme/app_theme.dart';
import '../../core/config/app_config.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import 'package:flutter/services.dart';

import 'package:image_picker/image_picker.dart';

final gallerySearchQueryProvider = StateProvider.autoDispose<String>((ref) => "");

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
  final TextEditingController _searchController = TextEditingController();

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  Future<void> _uploadMemory() async {
    final picker = ImagePicker();
    final image = await picker.pickImage(source: ImageSource.gallery);
    if (image == null) return;

    final titleCtrl = TextEditingController();
    final descCtrl = TextEditingController();

    if (!mounted) return;
    
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: AppTheme.midnightSurface,
        title: const Text('SHARE LEGACY MEMORY', style: TextStyle(color: AppTheme.royalGold, fontSize: 13, fontWeight: FontWeight.w900, letterSpacing: 1)),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            TextField(controller: titleCtrl, style: const TextStyle(color: Colors.white, fontSize: 13), decoration: const InputDecoration(labelText: 'Moment Title', labelStyle: TextStyle(color: Colors.white38, fontSize: 11))),
            const SizedBox(height: 12),
            TextField(controller: descCtrl, style: const TextStyle(color: Colors.white, fontSize: 13), decoration: const InputDecoration(labelText: 'Short Description', labelStyle: TextStyle(color: Colors.white38, fontSize: 11)), maxLines: 2),
          ],
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: const Text('CANCEL')),
          TextButton(onPressed: () => Navigator.pop(ctx, true), child: const Text('UPLOAD', style: TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.w900))),
        ],
      ),
    );

    if (confirmed == true) {
      final payload = {
        'title': titleCtrl.text,
        'description': descCtrl.text,
        'photoPath': image.path,
        'isApproved': false, // Admin needs to moderate
      };
      final success = await ref.read(newsServiceProvider).uploadGalleryItem(payload);
      if (success) {
        if (mounted) {
           ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Memory submitted for legacy moderation.')));
        }
        ref.invalidate(galleryItemsProvider);
      }
    }
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
    final galleryAsync = ref.watch(galleryItemsProvider);
    final searchQuery = ref.watch(gallerySearchQueryProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      title: 'Memories & Moments',
      breadcrumb: 'PORTAL > MEDIA GALLERY',
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () {
          HapticFeedback.lightImpact();
          _uploadMemory();
        },
        backgroundColor: AppTheme.royalGold,
        icon: const Icon(Icons.add_photo_alternate_rounded, color: Colors.black, size: 20),
        label: const Text('SHARE MOMENT', style: TextStyle(color: Colors.black, fontWeight: FontWeight.w900, fontSize: 10, letterSpacing: 1.2)),
      ),
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Search gallery memories...',
                prefixIcon: const Icon(Icons.search, size: 20, color: AppTheme.royalGold),
                suffixIcon: _searchController.text.isNotEmpty 
                  ? IconButton(
                      icon: const Icon(Icons.close_rounded, size: 20, color: Colors.white54),
                      onPressed: () {
                        _searchController.clear();
                        ref.read(gallerySearchQueryProvider.notifier).state = "";
                      },
                    )
                  : null,
                filled: true,
                fillColor: isDark ? Colors.black.withValues(alpha: 0.2) : Colors.white,
                contentPadding: const EdgeInsets.symmetric(horizontal: 20, vertical: 14),
                border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(16),
                    borderSide: BorderSide(color: AppTheme.royalGold.withValues(alpha: 0.1))),
              ),
              onChanged: (v) => ref.read(gallerySearchQueryProvider.notifier).state = v.toLowerCase(),
            ),
          ),
          Expanded(
            child: AsyncValueWidget<List<dynamic>>(
              value: galleryAsync,
              loadingMessage: 'Synchronizing alumni memories...',
              onRetry: () => ref.invalidate(galleryItemsProvider),
              data: (items) {
                final filtered = items.where((item) {
                  final title = item['title']?.toString().toLowerCase() ?? '';
                  final caption = item['caption']?.toString().toLowerCase() ?? '';
                  return title.contains(searchQuery) || caption.contains(searchQuery);
                }).toList();

                return RefreshIndicator(
                  color: AppTheme.royalGold,
                  onRefresh: () async {
                    HapticFeedback.mediumImpact();
                    ref.invalidate(galleryItemsProvider);
                  },
                  child: Column(
                    children: [
                      if (filtered.isNotEmpty)
                        Padding(
                          padding: const EdgeInsets.only(left: 24, top: 4, bottom: 8),
                          child: Align(
                            alignment: Alignment.centerLeft,
                            child: Text(
                              'Displaying ${filtered.length} of ${items.length} community captures',
                              style: TextStyle(fontSize: 10, color: AppTheme.royalGold.withValues(alpha: 0.7), fontWeight: FontWeight.bold),
                            ),
                          ),
                        ),
                      Expanded(
                        child: filtered.isEmpty
                          ? Center(child: Text(searchQuery.isEmpty ? 'No gallery assets found.' : 'No assets match your search.', style: const TextStyle(color: AppTheme.textSecondaryDark)))
                          : GridView.builder(
                              padding: const EdgeInsets.fromLTRB(20, 4, 20, 100),
                              gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                                  crossAxisCount: 2,
                                  crossAxisSpacing: 16,
                                  mainAxisSpacing: 16,
                                  childAspectRatio: 0.85),
                              itemCount: filtered.length,
                              itemBuilder: (context, index) {
                                final item = filtered[index];
                                final rawUrl = item['imageUrl'];
                                final base = AppConfig.apiBaseUrl.endsWith('/') ? AppConfig.apiBaseUrl.substring(0, AppConfig.apiBaseUrl.length - 1) : AppConfig.apiBaseUrl;
                                final fullUrl = (rawUrl != null && rawUrl.toString().startsWith('http')) ? rawUrl : '$base/$rawUrl';

                                return GlassContainer(
                                  padding: EdgeInsets.zero,
                                  child: InkWell(
                                    onTap: () {
                                      HapticFeedback.lightImpact();
                                      _showImagePreview(context, fullUrl);
                                    },
                                    child: Column(
                                      crossAxisAlignment: CrossAxisAlignment.stretch,
                                      children: [
                                        Expanded(
                                          child: ClipRRect(
                                            borderRadius: const BorderRadius.vertical(top: Radius.circular(AppConstants.radiusMedium)),
                                            child: Image.network(
                                              fullUrl,
                                              fit: BoxFit.cover,
                                              errorBuilder: (c, e, s) => Container(color: Colors.black26, child: const Center(child: Icon(Icons.broken_image, color: Colors.white24, size: 32))),
                                            ),
                                          ),
                                        ),
                                        Padding(
                                          padding: const EdgeInsets.all(AppConstants.paddingSmall),
                                          child: Text(
                                            item['title'] ?? 'ALUMNI MOMENT',
                                            style: const TextStyle(
                                                fontSize: 10.5,
                                                fontWeight: FontWeight.w900,
                                                color: Colors.white,
                                                letterSpacing: -0.1),
                                            maxLines: 1,
                                            overflow: TextOverflow.ellipsis,
                                          ),
                                        ),
                                      ],
                                    ),
                                  ),
                                );
                              },
                            ),
                      ),
                    ],
                  ),
                );
              },
            ),
          ),
        ],
      ),
    );
  }
}

class AppConstants {
  static const double paddingLarge = 20.0;
  static const double paddingSmall = 12.0;
  static const double radiusMedium = 16.0;
}
