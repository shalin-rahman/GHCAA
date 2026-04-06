import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../features/networking/networking_service.dart';
import '../../features/lookups/dropdown_service.dart';
import '../../core/config/app_config.dart';
import '../../core/widgets/custom_network_image.dart';
import '../../core/widgets/skeleton_loader.dart';

final directorySearchQueryProvider = StateProvider<String>((ref) => '');

class DirectoryScreen extends ConsumerStatefulWidget {
  const DirectoryScreen({super.key});

  @override
  ConsumerState<DirectoryScreen> createState() => _DirectoryScreenState();
}

class _DirectoryScreenState extends ConsumerState<DirectoryScreen> {
  final ScrollController _scrollController = ScrollController();
  List<dynamic> _alumni = [];
  bool _isLoading = true;
  String? _selectedBatch;
  String? _selectedDept;
  String? _selectedType;
  String? _selectedCategory;
  int _pageNumber = 1;
  final int _pageSize = 20;
  bool _isLoadingMore = false;
  bool _hasMore = true;
  int _totalItems = 0;
  


  final TextEditingController _searchController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _fetchAlumni();
    _scrollController.addListener(() {
      if (_scrollController.position.pixels >= _scrollController.position.maxScrollExtent * 0.9) {
        _fetchMoreAlumni();
      }
    });
  }

  @override
  void dispose() {
    _scrollController.dispose();
    _searchController.dispose();
    super.dispose();
  }

  Future<void> _fetchAlumni({bool refresh = false}) async {
    if (refresh) {
      _pageNumber = 1;
      _hasMore = true;
      _alumni = [];
      setState(() => _isLoading = true);
    } else {
      setState(() => _isLoading = true);
    }
    
    final query = ref.read(directorySearchQueryProvider);
    final service = ref.read(networkingServiceProvider);
    
    final result = await service.searchAlumni(
      query: query, 
      batch: _selectedBatch,
      department: _selectedDept,
      membershipType: _selectedType,
      category: _selectedCategory,
      pageNumber: _pageNumber, 
      pageSize: _pageSize
    );
    
    if (mounted) {
      setState(() {
        _alumni = (result['items'] as List<dynamic>?) ?? [];
        _totalItems = (result['totalItems'] as int?) ?? _alumni.length;
        _hasMore = _alumni.length < _totalItems;
        _isLoading = false;
      });
    }
  }

  Future<void> _fetchMoreAlumni() async {
    if (_isLoadingMore || !_hasMore) return;
    setState(() => _isLoadingMore = true);
    
    _pageNumber++;
    final query = ref.read(directorySearchQueryProvider);
    final service = ref.read(networkingServiceProvider);
    
    final result = await service.searchAlumni(
      query: query, 
      batch: _selectedBatch,
      department: _selectedDept,
      membershipType: _selectedType,
      category: _selectedCategory,
      pageNumber: _pageNumber, 
      pageSize: _pageSize
    );
    
    if (mounted) {
      setState(() {
        final newItems = (result['items'] as List<dynamic>?) ?? [];
        if (newItems.isNotEmpty) {
          _alumni.addAll(newItems);
          _hasMore = _alumni.length < _totalItems;
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
    return AppScaffold(
      title: 'Member Directory',
      breadcrumb: 'PORTAL > MEMBER DIRECTORY',
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16.0),
        child: Column(
          children: [
            TextField(
              key: const ValueKey('directory_search'),
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Search alumni registry...',
                prefixIcon: const Icon(Icons.search_rounded, size: 20),
                suffixIcon: _searchController.text.isNotEmpty 
                  ? IconButton(
                      icon: const Icon(Icons.cancel_rounded, size: 18, color: Colors.white24),
                      onPressed: () {
                        _searchController.clear();
                        _onSearchChanged('');
                      },
                    )
                  : null,
              ),
              onChanged: _onSearchChanged,
            ),
            if (!_isLoading && _totalItems > 0)
              Padding(
                padding: const EdgeInsets.only(top: 8.0, left: 4.0),
                child: Row(
                  children: [
                    Text(
                      'Showing ${_alumni.length} of $_totalItems alumni records',
                      style: TextStyle(
                        fontSize: 10,
                        color: AppTheme.royalGold.withValues(alpha: 0.7),
                        fontWeight: FontWeight.w600,
                        letterSpacing: 0.5
                      ),
                    ),
                  ],
                ),
              ),
            const SizedBox(height: 12),
            // Filter Row 1: Batch & Dept
            Row(
              children: [
                Expanded(
                  child: FutureBuilder<List<Map<String, String>>>(
                    future: ref.read(dropdownDataProvider).getOptions('PassingYear'),
                    builder: (context, snapshot) => _buildFilterDropdown('BATCH', snapshot.data?.map((e) => e['label']!).toList() ?? []),
                  ),
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: FutureBuilder<List<Map<String, String>>>(
                    future: ref.read(dropdownDataProvider).getOptions('Subject'),
                    builder: (context, snapshot) => _buildFilterDropdown('SUBJECT', snapshot.data?.map((e) => e['label']!).toList() ?? []),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 8),
            // Filter Row 2: Type & Category
            Row(
              children: [
                Expanded(
                  child: _buildFilterDropdown('TYPE', ['General', 'Founding', 'Executive', 'Associate', 'Honorary', 'Advisory']),
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: _buildFilterDropdown('CATEGORY', ['LifelongPatron', 'Sponsor', 'Advisor', 'Mentor', 'Volunteer', 'Student']),
                ),
              ],
            ),
            const SizedBox(height: 20),
            Expanded(
              child: _isLoading 
                ? ListView.builder(
                    itemCount: 8,
                    itemBuilder: (context, index) => SkeletonLoader.memberCard(),
                  )
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
                              
                              String? photoUrl;
                              if (m['photoPath'] != null && m['photoPath'].toString().isNotEmpty) {
                                final p = m['photoPath'];
                                if (p.startsWith('http')) {
                                  photoUrl = p;
                                } else {
                                  final base = AppConfig.apiBaseUrl.endsWith('/') ? AppConfig.apiBaseUrl.substring(0, AppConfig.apiBaseUrl.length - 1) : AppConfig.apiBaseUrl;
                                  final cleanP = p.startsWith('/') ? p.substring(1) : p;
                                  photoUrl = '$base/$cleanP';
                                }
                              }

                              return Padding(
                                padding: const EdgeInsets.only(bottom: 8.0),
                                child: GlassContainer(
                                  padding: const EdgeInsets.symmetric(horizontal: 10.0, vertical: 6.0),
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
                                            const SizedBox(width: 12),
                                            Expanded(
                                              child: Column(
                                                crossAxisAlignment: CrossAxisAlignment.start,
                                                children: [
                                                  Row(
                                                    children: [
                                                      Flexible(
                                                        child: Text(m['fullName'] ?? 'Anonymous Alumnus',
                                                            style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 13, color: Colors.white, letterSpacing: -0.2),
                                                            overflow: TextOverflow.ellipsis),
                                                      ),
                                                      if (m['isVerified'] == true) ...[
                                                        const SizedBox(width: 4),
                                                        const Icon(Icons.verified, color: AppTheme.royalGold, size: 14),
                                                      ],
                                                    ],
                                                  ),
                                                  Row(
                                                    children: [
                                                      Text(m['membershipNumber'] ?? 'REG-PENDING', style: const TextStyle(color: AppTheme.royalGold, fontSize: 9, fontWeight: FontWeight.bold, letterSpacing: 1)),
                                                      if (m['memberCategory'] != null) ...[
                                                        const SizedBox(width: 8),
                                                        Container(
                                                          padding: const EdgeInsets.symmetric(horizontal: 4, vertical: 0.5),
                                                          decoration: BoxDecoration(color: AppTheme.royalGold.withValues(alpha: 0.1), borderRadius: BorderRadius.circular(3)),
                                                          child: Text(m['memberCategory']!.toUpperCase(), style: const TextStyle(color: AppTheme.royalGold, fontSize: 7, fontWeight: FontWeight.w900)),
                                                        ),
                                                      ],
                                                    ],
                                                  ),
                                                ],
                                              ),
                                            ),
                                            if (m['membershipType'] != null)
                                              Container(
                                                padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                                                decoration: BoxDecoration(color: Colors.lightGreen.withValues(alpha: 0.1), borderRadius: BorderRadius.circular(4)),
                                                child: Text(m['membershipType']!, style: const TextStyle(color: Colors.lightGreenAccent, fontSize: 8, fontWeight: FontWeight.bold)),
                                              ),
                                            const SizedBox(width: 4),
                                            const Icon(Icons.chevron_right_rounded, size: 14, color: AppTheme.royalGold)
                                          ],
                                        ),
                                        const SizedBox(height: 10),
                                        const Divider(color: Colors.white10, height: 1),
                                        const SizedBox(height: 8),
                                        Row(
                                          children: [
                                            Expanded(
                                              flex: 5,
                                              child: Row(
                                                mainAxisSize: MainAxisSize.min,
                                                children: [
                                                  const Icon(Icons.school_outlined, size: 12, color: AppTheme.royalGold),
                                                  const SizedBox(width: 6),
                                                  Flexible(
                                                    child: Text(
                                                      _getCompactBatch(m),
                                                      style: const TextStyle(fontSize: 10, color: AppTheme.textSecondaryDark, fontWeight: FontWeight.w500),
                                                      maxLines: 1,
                                                      overflow: TextOverflow.ellipsis,
                                                    ),
                                                  ),
                                                ],
                                              ),
                                            ),
                                            const SizedBox(width: 12),
                                            Expanded(
                                              flex: 4,
                                              child: Row(
                                                mainAxisAlignment: MainAxisAlignment.end,
                                                mainAxisSize: MainAxisSize.min,
                                                children: [
                                                  Flexible(
                                                    child: Text(
                                                      m['designation'] ?? 'Alumnus',
                                                      style: const TextStyle(fontSize: 10, color: AppTheme.textSecondaryDark, fontWeight: FontWeight.w500),
                                                      textAlign: TextAlign.right,
                                                      maxLines: 1,
                                                      overflow: TextOverflow.ellipsis,
                                                    ),
                                                  ),
                                                  const SizedBox(width: 6),
                                                  const Icon(Icons.business_center_outlined, size: 12, color: AppTheme.royalGold),
                                                ],
                                              ),
                                            ),
                                          ],
                                        ),
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

  String _getCompactBatch(Map<String, dynamic> m) {
    final degree = m['degree'] ?? 'HSC';
    final year = m['passingYear']?.toString() ?? '';
    return 'Batch of $degree $year';
  }

  Widget _buildFilterDropdown(String label, List<String> options) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8),
      decoration: BoxDecoration(
        color: Colors.black.withValues(alpha: 0.2),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: AppTheme.royalGold.withValues(alpha: 0.1)),
      ),
      child: DropdownButtonHideUnderline(
        child: DropdownButton<String>(
          value: label == 'BATCH' ? _selectedBatch : (label == 'SUBJECT' ? _selectedDept : (label == 'TYPE' ? _selectedType : _selectedCategory)),
          hint: Text(label, style: const TextStyle(color: AppTheme.textSecondaryDark, fontSize: 8, fontWeight: FontWeight.bold)),
          dropdownColor: AppTheme.midnightSurface,
          icon: const Icon(Icons.arrow_drop_down, color: AppTheme.royalGold, size: 16),
          isExpanded: true,
          style: const TextStyle(color: Colors.white, fontSize: 10, fontWeight: FontWeight.bold),
          items: [
            DropdownMenuItem(value: null, child: Text('ALL $label', style: const TextStyle(fontSize: 10))),
            ...options.map((o) => DropdownMenuItem(value: o, child: Text(o, style: const TextStyle(fontSize: 10)))),
          ],
          onChanged: (v) {
            setState(() {
              if (label == 'BATCH') {
                _selectedBatch = v;
              } else if (label == 'SUBJECT') {
                _selectedDept = v;
              } else if (label == 'TYPE') {
                _selectedType = v;
              } else {
                _selectedCategory = v;
              }
            });
            _fetchAlumni(refresh: true);
          },
        ),
      ),
    );
  }

  Widget _buildMemberThumbnail(String? url, String? name) {
    return SizedBox(
      width: 44,
      height: 52,
      child: CustomNetworkImage(
        imageUrl: url ?? '',
        borderRadius: 10,
        fit: BoxFit.cover,
      ),
    );
  }
}
