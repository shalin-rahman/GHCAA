import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:image_picker/image_picker.dart';
import 'package:flutter/services.dart';
import '../../core/theme/app_theme.dart';
import '../../core/config/app_config.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../features/content/content_service.dart';
import '../../features/auth/auth_service.dart';

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
        backgroundColor: Colors.transparent,
        insetPadding: const EdgeInsets.all(10),
        child: Stack(
          alignment: Alignment.topRight,
          children: [
            InteractiveViewer(
              child: ClipRRect(
                borderRadius: BorderRadius.circular(16),
                child: Image.network(
                  url,
                  fit: BoxFit.contain,
                  loadingBuilder: (c, child, progress) => progress == null ? child : const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
                  errorBuilder: (c, e, s) => Container(
                    color: Colors.black54,
                    padding: const EdgeInsets.all(40),
                    child: const Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(Icons.broken_image, size: 48, color: Colors.white24),
                        SizedBox(height: 12),
                        Text('Historical asset not found.', style: TextStyle(color: Colors.white24, fontSize: 12)),
                      ],
                    ),
                  ),
                ),
              ),
            ),
            IconButton(
              icon: const Icon(Icons.close, color: Colors.white, size: 30),
              onPressed: () => Navigator.pop(context),
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
          backgroundColor: AppTheme.midnightSurface,
          title: const Text('CREATE ALUMNI GALLERY', style: TextStyle(color: AppTheme.royalGold, fontSize: 14, fontWeight: FontWeight.w900)),
          content: SingleChildScrollView(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                TextField(controller: titleCtrl, style: const TextStyle(color: Colors.white), decoration: const InputDecoration(labelText: 'Gallery Title')),
                TextField(controller: descCtrl, style: const TextStyle(color: Colors.white), decoration: const InputDecoration(labelText: 'Description')),
                TextField(controller: locCtrl, style: const TextStyle(color: Colors.white), decoration: const InputDecoration(labelText: 'Location (Optional)')),
                const SizedBox(height: 16),
                ListTile(
                  title: const Text('Event Date', style: TextStyle(color: Colors.white70, fontSize: 12)),
                  subtitle: Text("${selectedDate.toLocal()}".split(' ')[0], style: const TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.bold)),
                  trailing: const Icon(Icons.calendar_today, color: AppTheme.royalGold),
                  onTap: () async {
                    final picked = await showDatePicker(
                      context: context, 
                      initialDate: selectedDate, 
                      firstDate: DateTime(1900), 
                      lastDate: DateTime.now()
                    );
                    if (picked != null) setState(() => selectedDate = picked);
                  },
                ),
              ],
            ),
          ),
          actions: [
            TextButton(onPressed: () => Navigator.pop(context), child: const Text('CANCEL')),
            ElevatedButton(
              onPressed: () => Navigator.pop(context, true),
              style: ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold),
              child: const Text('CREATE', style: TextStyle(color: Colors.black, fontWeight: FontWeight.bold)),
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
        'eventDate': selectedDate.toIso8601String(),
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
        if (mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Photos synchronized.')));
      }
    }
  }

  Future<void> _confirmDeleteGallery(int id) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        backgroundColor: AppTheme.midnightSurface,
        title: const Text('Delete Gallery?', style: TextStyle(color: Colors.redAccent)),
        content: const Text('This will permanently remove this collection and all associated photos.', style: TextStyle(color: Colors.white70)),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context), child: const Text('CANCEL')),
          TextButton(onPressed: () => Navigator.pop(context, true), child: const Text('DELETE', style: TextStyle(color: Colors.redAccent, fontWeight: FontWeight.bold))),
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
      title: 'Legacy Archive & Moments',
      breadcrumb: 'PORTAL > LEGACY ARCHIVE',
      floatingActionButton: isAdmin 
        ? FloatingActionButton.extended(
            onPressed: () {
              HapticFeedback.lightImpact();
              _createGallery();
            },
            backgroundColor: AppTheme.royalGold,
            icon: const Icon(Icons.add_a_photo_outlined, color: Colors.black),
            label: const Text('CREATE GALLERY', style: TextStyle(color: Colors.black, fontWeight: FontWeight.w900, fontSize: 11)),
          )
        : null,
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Search archived memories...',
                prefixIcon: const Icon(Icons.search, color: AppTheme.royalGold),
                suffixIcon: _searchController.text.isNotEmpty 
                  ? IconButton(icon: const Icon(Icons.close, size: 18), onPressed: () { _searchController.clear(); ref.read(gallerySearchQueryProvider.notifier).state = ""; })
                  : null,
              ),
              onChanged: (v) => ref.read(gallerySearchQueryProvider.notifier).state = v.toLowerCase(),
            ),
          ),
          Expanded(
            child: galleryAsync.when(
              data: (galleries) {
                final filtered = galleries.where((g) => g['title'].toString().toLowerCase().contains(searchQuery)).toList();
                
                if (filtered.isEmpty) {
                  return const Center(child: Text('The vaults are empty of matching memories.', style: TextStyle(color: Colors.white24)));
                }

                return ListView.builder(
                  padding: const EdgeInsets.fromLTRB(20, 0, 20, 100),
                  itemCount: filtered.length,
                  itemBuilder: (context, index) {
                    final gallery = filtered[index];
                    final photos = gallery['photos'] as List? ?? [];
                    final thumbUrl = photos.isNotEmpty ? photos[0]['photoPath'] : null;
                    final base = AppConfig.apiBaseUrl.replaceFirst('/api', '');
                    final fullThumb = (thumbUrl != null && thumbUrl.startsWith('http')) ? thumbUrl : (thumbUrl != null ? '$base/$thumbUrl' : null);

                    return Padding(
                      padding: const EdgeInsets.only(bottom: 20),
                      child: GlassContainer(
                        padding: EdgeInsets.zero,
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
                                        borderRadius: const BorderRadius.vertical(top: Radius.circular(16)),
                                        child: Image.network(fullThumb, fit: BoxFit.cover),
                                      ),
                                    ),
                                  ),
                                  if (isAdmin)
                                    Positioned(
                                      top: 8, right: 8,
                                      child: Row(
                                        children: [
                                          CircleAvatar(
                                            backgroundColor: Colors.black54,
                                            child: IconButton(
                                              icon: const Icon(Icons.add_photo_alternate, color: AppTheme.royalGold, size: 18),
                                              onPressed: () => _uploadPhotos(gallery['id']),
                                            ),
                                          ),
                                          const SizedBox(width: 8),
                                          CircleAvatar(
                                            backgroundColor: Colors.black54,
                                            child: IconButton(
                                              icon: const Icon(Icons.delete_outline, color: Colors.redAccent, size: 18),
                                              onPressed: () => _confirmDeleteGallery(gallery['id']),
                                            ),
                                          ),
                                        ],
                                      ),
                                    ),
                                ],
                              ),
                            Padding(
                              padding: const EdgeInsets.all(16),
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Row(
                                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                    children: [
                                      Expanded(child: Text(gallery['title'].toUpperCase(), style: const TextStyle(color: Colors.white, fontWeight: FontWeight.w900, fontSize: 14, letterSpacing: 1))),
                                      Text(gallery['eventDate']?.toString().split('T')[0] ?? '', style: const TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.bold)),
                                    ],
                                  ),
                                  const SizedBox(height: 4),
                                  Text(gallery['description'] ?? 'No archived context available.', style: const TextStyle(color: Colors.white60, fontSize: 11)),
                                  const SizedBox(height: 12),
                                  if (photos.length > 1)
                                    SizedBox(
                                      height: 60,
                                      child: ListView.builder(
                                        scrollDirection: Axis.horizontal,
                                        itemCount: photos.length - 1,
                                        itemBuilder: (c, i) {
                                          final pUrl = photos[i+1]['photoPath'];
                                          final fUrl = (pUrl != null && pUrl.startsWith('http')) ? pUrl : '$base/$pUrl';
                                          return Padding(
                                            padding: const EdgeInsets.only(right: 8),
                                            child: GestureDetector(
                                              onTap: () => _showImagePreview(context, fUrl),
                                              child: ClipRRect(
                                                borderRadius: BorderRadius.circular(8),
                                                child: Image.network(fUrl, width: 60, height: 60, fit: BoxFit.cover),
                                              ),
                                            ),
                                          );
                                        },
                                      ),
                                    ),
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
              error: (e, s) => Center(child: Text('Archive Sync Error: $e', style: const TextStyle(color: Colors.redAccent))),
            ),
          ),
        ],
      ),
    );
  }
}
