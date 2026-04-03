import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/constants/app_constants.dart';
import '../../core/api/api_client.dart';
import '../../features/lookups/dropdown_service.dart';
import '../../core/config/app_config.dart';

class AdminCommunicationHub extends ConsumerStatefulWidget {
  const AdminCommunicationHub({super.key});

  @override
  ConsumerState<AdminCommunicationHub> createState() => _AdminCommState();
}

class _AdminCommState extends ConsumerState<AdminCommunicationHub> {
  final _titleController = TextEditingController();
  final _bodyController = TextEditingController();
  String _targetMethod = 'batch';
  String? _targetValue;
  String _selectedChannel = 'push';
  bool _isLoading = false;

  Future<void> _broadcast() async {
    if (_titleController.text.isEmpty || _bodyController.text.isEmpty || _targetValue == null) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('All fields are required')));
      return;
    }

    setState(() => _isLoading = true);
    try {
      final dio = ref.read(dioProvider);
      final payload = {
        'targetMethod': _targetMethod,
        'targetValue': _targetValue,
        'subject': _titleController.text,
        'body': _bodyController.text,
        'channel': _selectedChannel,
      };

      await dio.post('/admin/comm/send-custom', data: payload);
      
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Message sent successfully.')));
        _titleController.clear();
        _bodyController.clear();
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Error: $e'), backgroundColor: Colors.redAccent));
      }
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return AppScaffold(
      isAdmin: true,
      title: 'Communications',
      breadcrumb: 'ADMIN > COMMUNICATIONS',
      child: SingleChildScrollView(
        padding: const EdgeInsets.all(AppConstants.paddingLarge),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text('BROADCAST MESSAGE', style: TextStyle(fontWeight: FontWeight.w900, fontSize: 10, color: AppTheme.royalGold, letterSpacing: 1.5)),
            const SizedBox(height: 16),
            GlassContainer(
              child: Column(
                children: [
                  DropdownButtonFormField<String>(
                    initialValue: _targetMethod,
                    dropdownColor: AppTheme.midnightSurface,
                    decoration: const InputDecoration(labelText: 'Recipient Batch/Type'),
                    items: const [
                       DropdownMenuItem(value: 'batch', child: Text('By Year')),
                       DropdownMenuItem(value: 'type', child: Text('By Role')),
                    ],
                    onChanged: (v) {
                      setState(() {
                        _targetMethod = v!;
                        _targetValue = null;
                      });
                    },
                  ),
                  const SizedBox(height: 16),
                  _buildTargetDropdown(),
                ],
              ),
            ),
            const SizedBox(height: 24),
            GlassContainer(
              child: Column(
                children: [
                   TextField(
                     controller: _titleController,
                     style: const TextStyle(color: Colors.white),
                     decoration: const InputDecoration(labelText: 'Broadcast Title'),
                   ),
                   const SizedBox(height: 16),
                   TextField(
                     controller: _bodyController,
                     maxLines: 5,
                     style: const TextStyle(color: Colors.white),
                     decoration: const InputDecoration(labelText: 'Broadcast Content'),
                   ),
                   const SizedBox(height: 24),
                   const Text('COMMUNICATION CHANNEL', style: TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.bold, letterSpacing: 1.2)),
                   const SizedBox(height: 12),
                   FutureBuilder<List<Map<String, String>>>(
                     future: ref.read(dropdownDataProvider).getOptions('BroadcastChannel'),
                     builder: (context, snapshot) {
                       final options = snapshot.data ?? [];
                       if (options.isEmpty) {
                         return SegmentedButton<String>(
                           segments: const [
                             ButtonSegment(value: 'push', label: Text('PUSH', style: TextStyle(fontSize: 10))),
                             ButtonSegment(value: 'email', label: Text('EMAIL', style: TextStyle(fontSize: 10))),
                           ],
                           selected: {_selectedChannel},
                           onSelectionChanged: (Set<String> newSelection) {
                             if (newSelection.isNotEmpty) setState(() => _selectedChannel = newSelection.first);
                           },
                         );
                       }
                       return SizedBox(
                         width: double.infinity,
                         child: SegmentedButton<String>(
                           segments: options.map((o) => ButtonSegment(
                             value: o['value']!,
                             label: Text(o['label']!, style: const TextStyle(fontSize: 10, fontWeight: FontWeight.bold)),
                           )).toList(),
                           selected: {_selectedChannel},
                           onSelectionChanged: (Set<String> newSelection) {
                             if (newSelection.isNotEmpty) setState(() => _selectedChannel = newSelection.first);
                           },
                           style: SegmentedButton.styleFrom(
                             backgroundColor: Colors.white.withValues(alpha: 0.1),
                             selectedBackgroundColor: AppTheme.deepCharcoal,
                             selectedForegroundColor: AppTheme.royalGold,
                             foregroundColor: Colors.white,
                           ),
                         ),
                       );
                     },
                   ),
                ],
              ),
            ),
            const SizedBox(height: 32),
            SizedBox(
              width: double.infinity, 
              height: 56,
              child: ElevatedButton.icon(
                onPressed: _isLoading ? null : _broadcast, 
                icon: _isLoading ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.black)) : const Icon(Icons.campaign, color: Colors.black),
                label: const Text('SEND', style: TextStyle(fontWeight: FontWeight.w900, color: Colors.black, letterSpacing: 1)),
                style: ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold),
              )
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildTargetDropdown() {
    final group = _targetMethod == 'batch' ? 'PassingYear' : 'MembershipType';
    return FutureBuilder<List<Map<String, String>>>(
      future: ref.read(dropdownDataProvider).getOptions(group),
      builder: (context, snapshot) {
        final options = snapshot.data ?? [];
        return DropdownButtonFormField<String>(
          initialValue: _targetValue,
          dropdownColor: AppTheme.midnightSurface,
          decoration: InputDecoration(labelText: _targetMethod == 'batch' ? 'Select Batch' : 'Select Type'),
          items: options.map((o) => DropdownMenuItem(value: o['value'], child: Text(o['label']!))).toList(),
          onChanged: (v) => setState(() => _targetValue = v),
        );
      },
    );
  }
}

final adminGalleriesProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  final dio = ref.read(dioProvider);
  final response = await dio.get('/gallery/all');
  return response.data as List<dynamic>;
});

final adminGallerySearchQueryProvider = StateProvider.autoDispose<String>((ref) => "");

class AdminCMS extends ConsumerWidget {
  const AdminCMS({super.key});

  Future<void> _toggleGallery(WidgetRef ref, int id) async {
    try {
      final dio = ref.read(dioProvider);
      await dio.patch('/gallery/admin/$id/toggle-active');
      ref.invalidate(adminGalleriesProvider);
    } catch (_) {}
  }

  Future<void> _deleteGallery(WidgetRef ref, int id) async {
    try {
      final dio = ref.read(dioProvider);
      await dio.delete('/gallery/admin/$id');
      ref.invalidate(adminGalleriesProvider);
    } catch (_) {}
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final galleriesAsync = ref.watch(adminGalleriesProvider);
    final searchQuery = ref.watch(adminGallerySearchQueryProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      isAdmin: true,
      title: 'Content Management',
      breadcrumb: 'ADMIN > CMS HUB',
      floatingActionButton: FloatingActionButton.extended(
        backgroundColor: AppTheme.royalGold,
        onPressed: () => _showGalleryDialog(context, ref),
        icon: const Icon(Icons.add_photo_alternate_outlined, color: Colors.black),
        label: const Text('CREATE GALLERY', style: TextStyle(color: Colors.black, fontWeight: FontWeight.w900, fontSize: 10, letterSpacing: 1.5)),
      ),
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
            child: TextField(
              decoration: InputDecoration(
                hintText: 'Search gallery items...',
                prefixIcon: const Icon(Icons.search, size: 20, color: AppTheme.royalGold),
                suffixIcon: searchQuery.isNotEmpty
                  ? IconButton(
                      icon: const Icon(Icons.close_rounded, size: 20, color: Colors.white54),
                      onPressed: () => ref.read(adminGallerySearchQueryProvider.notifier).state = "",
                    )
                  : null,
                filled: true,
                fillColor: isDark ? Colors.black.withValues(alpha: 0.2) : Colors.white,
                contentPadding: const EdgeInsets.symmetric(horizontal: 20, vertical: 14),
                border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(16),
                    borderSide: BorderSide(color: AppTheme.royalGold.withValues(alpha: 0.1))),
              ),
              onChanged: (v) => ref.read(adminGallerySearchQueryProvider.notifier).state = v.toLowerCase(),
            ),
          ),
          Expanded(
            child: galleriesAsync.when(
              data: (galleries) {
                final filtered = galleries.where((g) {
                  final title = (g['title'] ?? '').toString().toLowerCase();
                  return title.contains(searchQuery);
                }).toList();

                return Column(
                  children: [
                    if (filtered.isNotEmpty)
                      Padding(
                        padding: const EdgeInsets.only(left: 24, bottom: 8),
                        child: Align(
                          alignment: Alignment.centerLeft,
                          child: Text(
                            'Showing ${filtered.length} of ${galleries.length} media records',
                            style: TextStyle(fontSize: 10, color: AppTheme.royalGold.withValues(alpha: 0.7), fontWeight: FontWeight.bold),
                          ),
                        ),
                      ),
                    Expanded(
                      child: ListView.builder(
                        padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                        itemCount: filtered.length,
                        itemBuilder: (context, index) {
                          final g = filtered[index];
                          final photo = (g['photos'] as List?)?.firstOrNull;
                          String? photoUrl;
                          if (photo != null && photo['filePath'] != null) {
                            final path = photo['filePath'].toString();
                            if (path.startsWith('http')) {
                              photoUrl = path;
                            } else {
                              final base = AppConfig.apiBaseUrl.endsWith('/') ? AppConfig.apiBaseUrl.substring(0, AppConfig.apiBaseUrl.length - 1) : AppConfig.apiBaseUrl;
                              final cleanP = path.startsWith('/') ? path.substring(1) : path;
                              photoUrl = '$base/$cleanP';
                            }
                          }

                          return Padding(
                            padding: const EdgeInsets.only(bottom: 12.0),
                            child: GlassContainer(
                              padding: const EdgeInsets.all(16),
                              child: Row(
                                children: [
                                  Container(
                                    width: 80, 
                                    height: 60, 
                                    decoration: BoxDecoration(
                                      color: AppTheme.royalGold.withValues(alpha: 0.1),
                                      borderRadius: BorderRadius.circular(8),
                                      image: photoUrl != null 
                                          ? DecorationImage(image: NetworkImage(photoUrl), fit: BoxFit.cover) 
                                          : null,
                                    ),
                                    child: photoUrl == null ? const Icon(Icons.image, color: AppTheme.royalGold) : null,
                                  ),
                                  const SizedBox(width: AppConstants.paddingMedium),
                                  Expanded(
                                    child: Column(
                                      crossAxisAlignment: CrossAxisAlignment.start, 
                                      children: [
                                        Text(g['title'] ?? 'Media Item', style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13, color: Colors.white)), 
                                        Text(g['eventDate'] != null ? g['eventDate'].split('T')[0] : 'Record', style: const TextStyle(fontSize: 10, color: Colors.white70))
                                      ]
                                    )
                                  ),
                                  IconButton(
                                    icon: Icon(g['isActive'] == true ? Icons.visibility : Icons.visibility_off, size: 16, color: g['isActive'] == true ? AppTheme.royalGold : Colors.white30), 
                                    onPressed: () => _toggleGallery(ref, g['id'])
                                  ),
                                  IconButton(
                                    icon: const Icon(Icons.delete_outline, size: 16, color: Colors.redAccent), 
                                    onPressed: () => _deleteGallery(ref, g['id'])
                                  ),
                                ],
                              ),
                            ),
                          );
                        },
                      ),
                    ),
                  ],
                );
              },
              loading: () => const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
              error: (e, s) => Center(child: Text('Error: $e')),
            ),
          ),
        ],
      ),
    );
  }

  void _showGalleryDialog(BuildContext context, WidgetRef ref) {
    final titleCtrl = TextEditingController();
    final descCtrl = TextEditingController();
    final dateCtrl = TextEditingController();

    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: AppTheme.midnightSurface,
        title: const Text('New Gallery', style: TextStyle(color: AppTheme.royalGold, fontSize: 14, fontWeight: FontWeight.bold)),
        content: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextField(controller: titleCtrl, style: const TextStyle(color: Colors.white), decoration: const InputDecoration(labelText: 'Gallery Title')),
              TextField(controller: descCtrl, style: const TextStyle(color: Colors.white), decoration: const InputDecoration(labelText: 'Description')),
              TextField(controller: dateCtrl, style: const TextStyle(color: Colors.white), decoration: const InputDecoration(labelText: 'Event Date (YYYY-MM-DD)')),
            ],
          ),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('CANCEL', style: TextStyle(color: Colors.white54))),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold),
            onPressed: () async {
              try {
                final dio = ref.read(dioProvider);
                final payload = {
                  'title': titleCtrl.text,
                  'description': descCtrl.text,
                  'eventDate': dateCtrl.text.isNotEmpty ? dateCtrl.text : DateTime.now().toIso8601String(),
                  'isActive': true,
                  'isFeatured': false,
                };
                
                await dio.post('/gallery/admin', data: payload);
                ref.invalidate(adminGalleriesProvider);
                if (ctx.mounted) Navigator.pop(ctx);
              } catch (e) {
                if (ctx.mounted) {
                  ScaffoldMessenger.of(ctx).showSnackBar(SnackBar(content: Text('Error: $e')));
                }
              }
            },
            child: const Text('CREATE', style: TextStyle(color: Colors.black, fontWeight: FontWeight.bold)),
          ),
        ],
      ),
    );
  }
}
