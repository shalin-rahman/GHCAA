import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_tile.dart';
import '../../core/widgets/async_value_widget.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../core/widgets/reject_reason_dialog.dart';
import '../../features/content/content_service.dart';
import 'package:flutter/services.dart';

final pendingGalleryApprovalsProvider = FutureProvider.autoDispose<Map<String, List<dynamic>>>(
  (ref) async => ref.read(galleryServiceProvider).getPendingGalleryApprovals(),
);
final galleryApprovalSearchQueryProvider = StateProvider.autoDispose<String>((ref) => "");

/// Sibling screen mirroring ArticleApprovalScreen's queue pattern (rather than
/// generalizing ApprovalQueueScreen into tabs) — reviews both pending member
/// albums and pending individual photos submitted into admin-owned galleries,
/// since the backend exposes them as two related but distinct lists.
class GalleryApprovalScreen extends ConsumerStatefulWidget {
  const GalleryApprovalScreen({super.key});

  @override
  ConsumerState<GalleryApprovalScreen> createState() => _GalleryApprovalScreenState();
}

class _GalleryApprovalScreenState extends ConsumerState<GalleryApprovalScreen> {
  final TextEditingController _searchController = TextEditingController();

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final pendingAsync = ref.watch(pendingGalleryApprovalsProvider);
    final searchQuery = ref.watch(galleryApprovalSearchQueryProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      isAdmin: true,
      title: 'Albums & Photos',
      breadcrumb: 'ADMIN > ALBUM REVIEW',
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Search pending albums & photos...',
                prefixIcon: const Icon(Icons.search, size: 20, color: AppTheme.royalGold),
                suffixIcon: _searchController.text.isNotEmpty
                  ? IconButton(
                      icon: const Icon(Icons.close_rounded, size: 20, color: Colors.white54),
                      onPressed: () {
                        _searchController.clear();
                        ref.read(galleryApprovalSearchQueryProvider.notifier).state = "";
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
              onChanged: (v) => ref.read(galleryApprovalSearchQueryProvider.notifier).state = v.toLowerCase(),
            ),
          ),
          Expanded(
            child: AsyncValueWidget<Map<String, List<dynamic>>>(
              value: pendingAsync,
              loadingMessage: 'Loading pending albums...',
              onRetry: () => ref.invalidate(pendingGalleryApprovalsProvider),
              data: (pending) {
                final galleries = pending['galleries'] ?? [];
                final photos = pending['photos'] ?? [];

                final filteredGalleries = galleries.where((g) {
                  final title = (g['title'] ?? '').toString().toLowerCase();
                  final memberId = (g['ownerMemberId'] ?? '').toString();
                  return title.contains(searchQuery) || memberId.contains(searchQuery);
                }).toList();

                final filteredPhotos = photos.where((p) {
                  final caption = (p['caption'] ?? '').toString().toLowerCase();
                  final memberId = (p['uploadedByMemberId'] ?? '').toString();
                  return caption.contains(searchQuery) || memberId.contains(searchQuery);
                }).toList();

                final total = filteredGalleries.length + filteredPhotos.length;

                return RefreshIndicator(
                  color: AppTheme.royalGold,
                  onRefresh: () async {
                    HapticFeedback.mediumImpact();
                    ref.invalidate(pendingGalleryApprovalsProvider);
                  },
                  child: total == 0
                    ? ListView(
                        children: [
                          EmptyStateWidget(
                            searchQuery.isEmpty ? 'No pending albums or photos found.' : 'No items match your search.',
                            icon: Icons.photo_library_outlined,
                          ),
                        ],
                      )
                    : ListView(
                        padding: const EdgeInsets.all(20.0),
                        children: [
                          if (filteredGalleries.isNotEmpty) ...[
                            Text('PENDING ALBUMS', style: Theme.of(context).textTheme.labelSmall?.copyWith(color: AppTheme.royalGold.withValues(alpha: 0.7), letterSpacing: 1.2)),
                            const SizedBox(height: AppTheme.spaceM),
                            ...filteredGalleries.map((gallery) => Padding(
                              padding: const EdgeInsets.only(bottom: 12.0),
                              child: GlassTile(
                                icon: Icons.collections_bookmark_rounded,
                                title: gallery['title'] ?? 'Untitled Album',
                                subtitle: 'By Member #${gallery['ownerMemberId'] ?? '—'}',
                                trailing: Row(
                                  mainAxisSize: MainAxisSize.min,
                                  children: [
                                    IconButton(
                                      icon: const Icon(Icons.check_circle_outline, color: Colors.greenAccent, size: 24),
                                      onPressed: () => _handleGalleryResolve(context, ref, gallery['id'], true),
                                    ),
                                    IconButton(
                                      icon: const Icon(Icons.cancel_outlined, color: Colors.redAccent, size: 24),
                                      onPressed: () => _handleGalleryResolve(context, ref, gallery['id'], false),
                                    ),
                                  ],
                                ),
                                onTap: () {},
                              ),
                            )),
                            const SizedBox(height: AppTheme.spaceL),
                          ],
                          if (filteredPhotos.isNotEmpty) ...[
                            Text('PENDING PHOTOS', style: Theme.of(context).textTheme.labelSmall?.copyWith(color: AppTheme.royalGold.withValues(alpha: 0.7), letterSpacing: 1.2)),
                            const SizedBox(height: AppTheme.spaceM),
                            ...filteredPhotos.map((photo) => Padding(
                              padding: const EdgeInsets.only(bottom: 12.0),
                              child: GlassTile(
                                icon: Icons.image_rounded,
                                title: photo['caption']?.toString().isNotEmpty == true ? photo['caption'] : 'Untitled Photo',
                                subtitle: 'By Member #${photo['uploadedByMemberId'] ?? '—'}',
                                trailing: Row(
                                  mainAxisSize: MainAxisSize.min,
                                  children: [
                                    IconButton(
                                      icon: const Icon(Icons.check_circle_outline, color: Colors.greenAccent, size: 24),
                                      onPressed: () => _handlePhotoResolve(context, ref, photo['id'], true),
                                    ),
                                    IconButton(
                                      icon: const Icon(Icons.cancel_outlined, color: Colors.redAccent, size: 24),
                                      onPressed: () => _handlePhotoResolve(context, ref, photo['id'], false),
                                    ),
                                  ],
                                ),
                                onTap: () {},
                              ),
                            )),
                          ],
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

  Future<void> _handleGalleryResolve(BuildContext context, WidgetRef ref, int id, bool approve) async {
    String? reason;
    bool notify = true;
    if (!approve) {
      final result = await showRejectReasonWithNotifyDialog(context, title: 'REJECT ALBUM');
      if (result == null) return;
      reason = result.reason;
      notify = result.notify;
    }
    HapticFeedback.heavyImpact();
    final success = await ref.read(galleryServiceProvider).resolveGalleryApproval(id, approve, reason: reason, notifyMember: notify);
    if (success && context.mounted) {
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(approve ? 'Album approved.' : 'Album rejected.')));
      ref.invalidate(pendingGalleryApprovalsProvider);
    }
  }

  Future<void> _handlePhotoResolve(BuildContext context, WidgetRef ref, int id, bool approve) async {
    String? reason;
    bool notify = true;
    if (!approve) {
      final result = await showRejectReasonWithNotifyDialog(context, title: 'REJECT PHOTO');
      if (result == null) return;
      reason = result.reason;
      notify = result.notify;
    }
    HapticFeedback.heavyImpact();
    final success = await ref.read(galleryServiceProvider).resolvePhotoApproval(id, approve, reason: reason, notifyMember: notify);
    if (success && context.mounted) {
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(approve ? 'Photo approved.' : 'Photo rejected.')));
      ref.invalidate(pendingGalleryApprovalsProvider);
    }
  }
}
