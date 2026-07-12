import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:image_picker/image_picker.dart';
import 'package:flutter/services.dart';
import '../../core/theme/app_theme.dart';
import '../../core/config/app_config.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../features/content/content_service.dart';
import '../../features/auth/auth_service.dart';
import '../../core/utils/app_utils.dart';
import '../../core/widgets/admin_action_circle.dart';
import '../../core/widgets/app_search_field.dart';
import '../../core/widgets/empty_state_widget.dart';

final galleryItemsProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  final role = await ref.read(authServiceProvider).getRole();
  final isAdmin = role.isStaffAdminRole;
  return ref.read(galleryServiceProvider).getGalleries(onlyActive: !isAdmin);
});

final gallerySearchQueryProvider = StateProvider.autoDispose<String>((ref) => "");

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

  void _showImagePreview(BuildContext context, String? url) {
    if (url == null) return;
    showDialog(
      context: context,
      builder: (context) => Dialog(
        backgroundColor: Colors.black.withValues(alpha: 0.9),
        insetPadding: const EdgeInsets.all(0),
        child: Stack(
          alignment: Alignment.topRight,
          children: [
            Center(
              child: InteractiveViewer(
                child: Image.network(
                  url,
                  fit: BoxFit.contain,
                  loadingBuilder: (c, child, progress) => progress == null ? child : const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
                  errorBuilder: (c, e, s) => Container(
                    padding: const EdgeInsets.all(AppTheme.spaceXXL),
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        const Icon(Icons.broken_image_rounded, size: AppTheme.spaceXXL, color: Colors.white24),
                        const SizedBox(height: AppTheme.spaceM),
                        Text('Image not available', style: Theme.of(context).textTheme.labelSmall?.copyWith(color: Colors.white24)),
                      ],
                    ),
                  ),
                ),
              ),
            ),
            Padding(
              padding: const EdgeInsets.all(AppTheme.spaceL),
              child: IconButton(
                icon: const Icon(Icons.close_fullscreen_rounded, color: AppTheme.royalGold, size: 28),
                onPressed: () => Navigator.pop(context),
              ),
            ),
          ],
        ),
      ),
    );
  }
 
  Future<void> _createGallery() async {
    final titleCtrl = TextEditingController();
    final descCtrl = TextEditingController();
    final locCtrl = TextEditingController();
    DateTime selectedDate = DateTime.now();
 
    final result = await showDialog<bool>(
      context: context,
      builder: (context) => StatefulBuilder(
        builder: (context, setState) => AlertDialog(
          backgroundColor: AppTheme.deepCharcoal,
          surfaceTintColor: Colors.transparent,
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20), side: const BorderSide(color: AppTheme.glassBorder)),
          title: Center(child: Text('CREATE NEW GALLERY', style: Theme.of(context).textTheme.labelLarge?.copyWith(letterSpacing: 2))),
          content: SingleChildScrollView(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const SizedBox(height: AppTheme.spaceM),
                TextField(controller: titleCtrl, decoration: const InputDecoration(labelText: 'Gallery Title', prefixIcon: Icon(Icons.collections_bookmark_rounded))),
                const SizedBox(height: AppTheme.spaceM),
                TextField(controller: descCtrl, decoration: const InputDecoration(labelText: 'Description', prefixIcon: Icon(Icons.notes_rounded)), maxLines: 2),
                const SizedBox(height: AppTheme.spaceM),
                TextField(controller: locCtrl, decoration: const InputDecoration(labelText: 'Location', prefixIcon: Icon(Icons.pin_drop_rounded))),
                const SizedBox(height: AppTheme.spaceXL),
                Text('EVENT DATE', style: Theme.of(context).textTheme.labelSmall?.copyWith(color: AppTheme.royalGold, letterSpacing: 1.5, fontSize: 8)),
                const SizedBox(height: AppTheme.spaceM),
                GestureDetector(
                  onTap: () async {
                    final picked = await showDatePicker(
                      context: context, 
                      initialDate: selectedDate, 
                      firstDate: DateTime(1960), 
                      lastDate: DateTime.now(),
                      builder: (context, child) => Theme(
                        data: Theme.of(context).copyWith(colorScheme: const ColorScheme.dark(primary: AppTheme.royalGold, surface: AppTheme.deepCharcoal)),
                        child: child!,
                      ),
                    );
                    if (picked != null) setState(() => selectedDate = picked);
                  },
                  child: Container(
                    padding: const EdgeInsets.all(AppTheme.spaceM),
                    decoration: BoxDecoration(color: AppTheme.obsidianBlack, borderRadius: BorderRadius.circular(AppTheme.radiusM), border: Border.all(color: AppTheme.glassBorder)),
                    child: Row(
                      children: [
                        const Icon(Icons.event_available_rounded, size: 18, color: AppTheme.royalGold),
                        const SizedBox(width: AppTheme.spaceM),
                        Text(AppUtils.formatDate(selectedDate), style: Theme.of(context).textTheme.bodyMedium?.copyWith(fontWeight: FontWeight.bold)),
                        const Spacer(),
                        const Text('EDIT', style: TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.w900)),
                      ],
                    ),
                  ),
                ),
              ],
            ),
          ),
          actions: [
            TextButton(onPressed: () => Navigator.pop(context), child: const Text('CANCEL', style: TextStyle(color: Colors.white38))),
            ElevatedButton(
              onPressed: () => Navigator.pop(context, true),
              child: const Text('CREATE GALLERY'),
            ),
          ],
        ),
      ),
    );
 
    if (result == true && titleCtrl.text.isNotEmpty) {
      final success = await ref.read(galleryServiceProvider).createGallery({
        'title': titleCtrl.text,
        'description': descCtrl.text,
        'location': locCtrl.text,
        'eventDate': AppUtils.formatDate(selectedDate),
        'isActive': true,
      });
      if (success) {
        ref.invalidate(galleryItemsProvider);
        if (mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Gallery created successfully.')));
      }
    }
  }
 
  Future<void> _uploadPhotos(int galleryId) async {
    final picker = ImagePicker();
    final images = await picker.pickMultiImage();
    if (images.isEmpty) return;
 
    if (!mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Uploading ${images.length} photos...')));
 
    final uploadedPaths = <String>[];
    for (final img in images) {
      final path = await ref.read(galleryServiceProvider).uploadPhoto(img.path);
      if (path != null) uploadedPaths.add(path);
    }
 
    if (uploadedPaths.isNotEmpty) {
      final success = await ref.read(galleryServiceProvider).addPhotosToGallery(galleryId, uploadedPaths);
      if (success) {
        ref.invalidate(galleryItemsProvider);
        if (mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Photos added successfully.')));
      }
    }
  }
 
  Future<void> _confirmDeleteGallery(int id) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        backgroundColor: AppTheme.deepCharcoal,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20), side: const BorderSide(color: Colors.redAccent, width: 0.5)),
        title: const Text('DELETE GALLERY', style: TextStyle(color: Colors.redAccent, fontWeight: FontWeight.w900, fontSize: 16)),
        content: const Text('Are you sure you want to delete this gallery and all its photos? This action cannot be undone.', style: TextStyle(color: Colors.white70, height: 1.5)),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context), child: const Text('CANCEL')),
          ElevatedButton(
            onPressed: () => Navigator.pop(context, true), 
            style: ElevatedButton.styleFrom(backgroundColor: Colors.redAccent),
            child: const Text('DELETE'),
          ),
        ],
      ),
    );
 
    if (confirm == true) {
      final success = await ref.read(galleryServiceProvider).deleteGallery(id);
      if (success) {
        ref.invalidate(galleryItemsProvider);
      }
    }
  }
 
  @override
  Widget build(BuildContext context) {
    final galleryAsync = ref.watch(galleryItemsProvider);
    final searchQuery = ref.watch(gallerySearchQueryProvider);
    final roleAsync = ref.watch(roleProvider);
    final isAdmin = roleAsync.value?.isStaffAdminRole ?? false;
 
    return AppScaffold(
      title: 'Gallery',
      breadcrumb: 'PORTAL > GALLERY',
      floatingActionButton: isAdmin 
        ? FloatingActionButton.extended(
            onPressed: () {
              HapticFeedback.lightImpact();
              _createGallery();
            },
            backgroundColor: AppTheme.royalGold,
            icon: const Icon(Icons.add_photo_alternate_rounded, color: Colors.black, size: 20),
            label: Text('ADD GALLERY', style: Theme.of(context).textTheme.labelLarge?.copyWith(color: Colors.black, fontSize: 11)),
          )
        : null,
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: AppTheme.spaceL, vertical: AppTheme.spaceM),
            child: AppSearchField(
              controller: _searchController,
              hintText: 'Search gallery...',
              onChanged: (v) => ref.read(gallerySearchQueryProvider.notifier).state = v.toLowerCase(),
              onClear: () {
                _searchController.clear();
                ref.read(gallerySearchQueryProvider.notifier).state = "";
              },
            ),
          ),
          Expanded(
            child: galleryAsync.when(
              data: (galleries) {
                final filtered = galleries.where((g) => g['title'].toString().toLowerCase().contains(searchQuery)).toList();
                
                if (filtered.isEmpty) {
                  return const EmptyStateWidget('No galleries found.', icon: Icons.photo_library_outlined);
                }
 
                return ListView.builder(
                  padding: const EdgeInsets.fromLTRB(AppTheme.spaceL, 0, AppTheme.spaceL, 104),
                  itemCount: filtered.length,
                  itemBuilder: (context, index) {
                    final gallery = filtered[index];
                    final photos = gallery['photos'] as List? ?? [];
                    final fullThumb = AppConfig.resolveImageUrl(photos.isNotEmpty ? photos[0]['photoPath'] : null);
 
                    return Padding(
                      padding: const EdgeInsets.only(bottom: AppTheme.spaceXL),
                      child: Card(
                        margin: EdgeInsets.zero,
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.stretch,
                          children: [
                            if (fullThumb != null)
                              Stack(
                                children: [
                                  GestureDetector(
                                    onTap: () => _showImagePreview(context, fullThumb),
                                    child: AspectRatio(
                                      aspectRatio: 16 / 9,
                                      child: ClipRRect(
                                        borderRadius: const BorderRadius.vertical(top: Radius.circular(AppTheme.radiusL)),
                                        child: Image.network(fullThumb, fit: BoxFit.cover, 
                                          errorBuilder: (context, error, stackTrace) => Container(color: Colors.black12, child: const Icon(Icons.image_not_supported_outlined, color: Colors.white10))),
                                      ),
                                    ),
                                  ),
                                  if (isAdmin)
                                    Positioned(
                                      top: AppTheme.spaceS + 4, right: AppTheme.spaceS + 4,
                                      child: Row(
                                        children: [
                                          AdminActionCircle(icon: Icons.upload_file_rounded, color: AppTheme.royalGold, tooltip: 'Upload Photos', onTap: () => _uploadPhotos(gallery['id'])),
                                          const SizedBox(width: AppTheme.spaceS),
                                          AdminActionCircle(icon: Icons.delete_sweep_rounded, color: Colors.redAccent, tooltip: 'Delete Gallery', onTap: () => _confirmDeleteGallery(gallery['id'])),
                                        ],
                                      ),
                                    ),
                                ],
                              ),
                            Padding(
                              padding: const EdgeInsets.all(AppTheme.spaceL),
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Row(
                                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                    children: [
                                      Expanded(child: Text(gallery['title'].toString().toUpperCase(), style: Theme.of(context).textTheme.titleSmall?.copyWith(fontSize: 15, fontWeight: FontWeight.w900))),
                                      const SizedBox(width: AppTheme.spaceS),
                                      Text(AppUtils.formatDate(gallery['eventDate']), style: Theme.of(context).textTheme.labelLarge),
                                    ],
                                  ),
                                  const SizedBox(height: AppTheme.spaceS),
                                  Text(gallery['description'] ?? 'No description available.', style: Theme.of(context).textTheme.bodyMedium?.copyWith(height: 1.5)),
                                  const SizedBox(height: AppTheme.spaceL),
                                  if (photos.length > 1) ...[
                                    Text('Photos', style: Theme.of(context).textTheme.labelSmall?.copyWith(color: AppTheme.royalGold.withValues(alpha: 0.6), letterSpacing: 1.2)),
                                    const SizedBox(height: AppTheme.spaceM),
                                    SizedBox(
                                      height: 72,
                                      child: ListView.builder(
                                        scrollDirection: Axis.horizontal,
                                        itemCount: photos.length - 1,
                                        itemBuilder: (c, i) {
                                          final fUrl = AppConfig.resolveImageUrl(photos[i+1]['photoPath']);
                                          if (fUrl == null) return const SizedBox();
                                          return Padding(
                                            padding: const EdgeInsets.only(right: AppTheme.spaceS),
                                            child: GestureDetector(
                                              onTap: () => _showImagePreview(context, fUrl),
                                              child: Container(
                                                decoration: BoxDecoration(borderRadius: BorderRadius.circular(AppTheme.radiusS), border: Border.all(color: AppTheme.glassBorder)),
                                                child: ClipRRect(
                                                  borderRadius: BorderRadius.circular(AppTheme.radiusS),
                                                  child: Image.network(fUrl, width: 72, height: 72, fit: BoxFit.cover,
                                                    errorBuilder: (context, error, stackTrace) => const SizedBox(width: 72, child: Icon(Icons.broken_image_outlined, color: Colors.white10))),
                                                ),
                                              ),
                                            ),
                                          );
                                        },
                                      ),
                                    ),
                                  ],
                                ],
                              ),
                            ),
                          ],
                        ),
                      ),
                    );
                  },
                );
              },
              loading: () => const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
              error: (e, s) => Center(child: Text('Error: $e', style: const TextStyle(color: Colors.redAccent))),
            ),
          ),
        ],
      ),
    );
  }
}
