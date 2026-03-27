import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../features/networking/networking_service.dart';
import '../../core/config/app_config.dart';

final directorySearchQueryProvider = StateProvider<String>((ref) => '');

class DirectoryScreen extends ConsumerStatefulWidget {
  const DirectoryScreen({super.key});

  @override
  ConsumerState<DirectoryScreen> createState() => _DirectoryScreenState();
}

class _DirectoryScreenState extends ConsumerState<DirectoryScreen> {
  final ScrollController _scrollController = ScrollController();
  final List<dynamic> _alumni = [];
  bool _isLoading = true;
  bool _isLoadingMore = false;
  bool _hasMore = true;
  int _pageNumber = 1;
  final int _pageSize = 20;

  @override
  void initState() {
    super.initState();
    _fetchAlumni();
    _scrollController.addListener(_onScroll);
  }

  @override
  void dispose() {
    _scrollController.dispose();
    super.dispose();
  }

  void _onScroll() {
    if (_scrollController.position.pixels >= _scrollController.position.maxScrollExtent - 200) {
      if (!_isLoadingMore && _hasMore) {
        _fetchMoreAlumni();
      }
    }
  }

  Future<void> _fetchAlumni({bool refresh = false}) async {
    if (refresh) {
      if (!mounted) return;
      setState(() {
        _isLoading = true;
        _pageNumber = 1;
        _alumni.clear();
        _hasMore = true;
      });
    }

    final query = ref.read(directorySearchQueryProvider);
    final service = ref.read(networkingServiceProvider);
    
    final newItems = await service.searchAlumni(query: query, pageNumber: _pageNumber, pageSize: _pageSize);
    
    if (mounted) {
      setState(() {
        if (newItems.isNotEmpty) {
          _alumni.addAll(newItems);
          _hasMore = newItems.length == _pageSize;
        } else {
          _hasMore = false;
        }
        _isLoading = false;
      });
    }
  }

  Future<void> _fetchMoreAlumni() async {
    setState(() => _isLoadingMore = true);
    
    _pageNumber++;
    final query = ref.read(directorySearchQueryProvider);
    final service = ref.read(networkingServiceProvider);
    
    final newItems = await service.searchAlumni(query: query, pageNumber: _pageNumber, pageSize: _pageSize);
    
    if (mounted) {
      setState(() {
        if (newItems.isNotEmpty) {
          _alumni.addAll(newItems);
          _hasMore = newItems.length == _pageSize;
        } else {
          _hasMore = false;
        }
        _isLoadingMore = false;
      });
    }
  }

  void _onSearchChanged(String value) {
    ref.read(directorySearchQueryProvider.notifier).state = value.toLowerCase();
    _fetchAlumni(refresh: true);
  }

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      title: 'Alumni Registry',
      breadcrumb: 'Member Portal > Infinite Registry',
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16.0),
        child: Column(
          children: [
            TextField(
              decoration: InputDecoration(
                hintText: 'Search by Name, Batch, or Industry...',
                prefixIcon: const Icon(Icons.search, size: 20, color: AppTheme.royalGold),
                filled: true,
                fillColor: isDark ? Colors.black.withValues(alpha: 0.2) : Colors.white,
                contentPadding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
                border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(16),
                    borderSide: BorderSide(color: AppTheme.royalGold.withValues(alpha: 0.1))),
              ),
              onChanged: _onSearchChanged,
            ),
            const SizedBox(height: 20),
            Expanded(
              child: _isLoading 
                ? const Center(child: CircularProgressIndicator(color: AppTheme.royalGold))
                : RefreshIndicator(
                    color: AppTheme.royalGold,
                    onRefresh: () async {
                      HapticFeedback.mediumImpact();
                      await _fetchAlumni(refresh: true);
                    },
                    child: _alumni.isEmpty
                        ? const Center(child: Text('No records found in the registry.', style: TextStyle(color: AppTheme.textSecondaryDark)))
                        : ListView.builder(
                            controller: _scrollController,
                            padding: const EdgeInsets.only(bottom: 40),
                            physics: const AlwaysScrollableScrollPhysics(),
                            itemCount: _alumni.length + (_hasMore || _isLoadingMore ? 1 : 0),
                            itemBuilder: (context, index) {
                              if (index == _alumni.length) {
                                return const Padding(
                                  padding: EdgeInsets.symmetric(vertical: 24.0),
                                  child: Center(child: CircularProgressIndicator(color: AppTheme.royalGold, strokeWidth: 2)),
                                );
                              }

                              final m = _alumni[index];
                              final photoUrl = m['photoPath'] != null 
                                  ? '${AppConfig.apiBaseUrl}/${m['photoPath']}'.replaceAll('//', '/') 
                                  : null;

                              return Padding(
                                padding: const EdgeInsets.only(bottom: 12.0),
                                child: GlassContainer(
                                  padding: const EdgeInsets.all(16.0),
                                  child: InkWell(
                                    onTap: () {
                                      HapticFeedback.lightImpact();
                                      context.push('/directory/${m['id']}');
                                    },
                                    child: Column(
                                      children: [
                                        Row(
                                          children: [
                                            _buildMemberThumbnail(photoUrl, m['fullName']),
                                            const SizedBox(width: 16),
                                            Expanded(
                                              child: Column(
                                                crossAxisAlignment: CrossAxisAlignment.start,
                                                children: [
                                                  Text(m['fullName'] ?? 'Anonymous Alumnus',
                                                      style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 16, color: Colors.white)),
                                                  const SizedBox(height: 4),
                                                  Row(
                                                    children: [
                                                      Text(m['membershipNumber'] ?? 'REG-PENDING', style: const TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.bold, letterSpacing: 1)),
                                                      if (m['rank'] != null) ...[
                                                        const SizedBox(width: 8),
                                                        Container(
                                                          padding: const EdgeInsets.symmetric(horizontal: 4, vertical: 2),
                                                          decoration: BoxDecoration(color: Colors.amber.withValues(alpha: 0.1), borderRadius: BorderRadius.circular(4)),
                                                          child: Text('#${m['rank']}', style: const TextStyle(color: Colors.amber, fontSize: 9, fontWeight: FontWeight.bold)),
                                                        ),
                                                      ],
                                                      if (m['categoryBadge'] != null) ...[
                                                        const SizedBox(width: 8),
                                                        Text(m['categoryBadge'], style: const TextStyle(color: Colors.blueAccent, fontSize: 9, fontWeight: FontWeight.bold)),
                                                      ],
                                                    ],
                                                  ),
                                                ],
                                              ),
                                            ),
                                            const Icon(Icons.arrow_forward_ios_rounded, size: 14, color: AppTheme.royalGold)
                                          ],
                                        ),
                                        const SizedBox(height: 16),
                                        const Divider(color: Colors.white10, height: 1),
                                        const SizedBox(height: 12),
                                        _buildInfoRow(Icons.school_outlined, 'Batch: ${m['passingYear'] ?? 'N/A'} (${m['degree'] ?? 'None'} ${_getMajorDisplay(m['degree'], m['subject'])})'),
                                        const SizedBox(height: 8),
                                        _buildInfoRow(Icons.work_outline, '${m['designation'] ?? 'Alumnus'} at ${m['professionalSector'] ?? 'General Industry'}'),
                                        if (m['bloodGroup'] != null) ...[
                                          const SizedBox(height: 8),
                                          _buildInfoRow(Icons.water_drop_outlined, 'Blood Category: ${m['bloodGroup']}', color: Colors.redAccent.withValues(alpha: 0.7)),
                                        ],
                                      ],
                                    ),
                                  ),
                                ),
                              );
                            },
                          ),
                  ),
            ),
          ],
        ),
      ),
    );
  }

  String _getMajorDisplay(dynamic degree, dynamic subject) {
    if (degree == null) return '';
    final major = subject ?? 'None';
    return (major != 'None') ? 'in $major' : '';
  }

  Widget _buildInfoRow(IconData icon, String text, {Color? color}) {
    return Row(
      children: [
        Icon(icon, size: 14, color: color ?? AppTheme.royalGold),
        const SizedBox(width: 10),
        Expanded(
          child: Text(text, style: TextStyle(fontSize: 11, color: color ?? AppTheme.textSecondaryDark, fontWeight: FontWeight.w500)),
        ),
      ],
    );
  }

  Widget _buildMemberThumbnail(String? url, String? name) {
    return Container(
      width: 48,
      height: 60,
      decoration: BoxDecoration(
        color: Colors.black,
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: AppTheme.royalGold.withValues(alpha: 0.1)),
        image: url != null ? DecorationImage(image: NetworkImage(url), fit: BoxFit.cover) : null,
      ),
      child: url == null 
        ? Center(child: Text(name != null && name.isNotEmpty ? name[0] : '?', style: const TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.bold)))
        : null,
    );
  }
}
