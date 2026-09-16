import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../features/networking/networking_service.dart';
import '../../features/lookups/dropdown_service.dart';
import '../../core/config/app_config.dart';
import '../../core/constants/registration_constants.dart';
import '../../core/widgets/custom_network_image.dart';
import '../../core/widgets/skeleton_loader.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../core/widgets/logo_spinner.dart';
import '../../core/widgets/app_search_field.dart';
import '../../core/widgets/app_dropdown_field.dart';

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
  final int _pageSize = 20;
  bool _isLoadingMore = false;
  bool _hasMore = true;
  int _totalItems = 0;
  String? _nextCursor;

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
      _nextCursor = null;
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
      cursor: null,
      pageSize: _pageSize
    );

    if (mounted) {
      setState(() {
        _alumni = (result['items'] as List<dynamic>?) ?? [];
        _totalItems = (result['totalItems'] as int?) ?? _alumni.length;
        _nextCursor = result['nextCursor'] as String?;
        _hasMore = _nextCursor != null;
        _isLoading = false;
      });
    }
  }

  Future<void> _fetchMoreAlumni() async {
    if (_isLoadingMore || !_hasMore || _nextCursor == null) return;
    setState(() => _isLoadingMore = true);

    final query = ref.read(directorySearchQueryProvider);
    final service = ref.read(networkingServiceProvider);

    final result = await service.searchAlumni(
      query: query,
      batch: _selectedBatch,
      department: _selectedDept,
      membershipType: _selectedType,
      category: _selectedCategory,
      cursor: _nextCursor,
      pageSize: _pageSize
    );

    if (mounted) {
      setState(() {
        final newItems = (result['items'] as List<dynamic>?) ?? [];
        _nextCursor = result['nextCursor'] as String?;
        if (newItems.isNotEmpty) {
          _alumni.addAll(newItems);
          _hasMore = _nextCursor != null;
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
        padding: const EdgeInsets.symmetric(horizontal: AppTheme.spaceL),
        child: Column(
          children: [
            const SizedBox(height: AppTheme.spaceM),
            AppSearchField(
              key: const ValueKey('directory_search'),
              controller: _searchController,
              hintText: 'Search members...',
              debounce: const Duration(milliseconds: 300),
              onChanged: _onSearchChanged,
              onClear: () => _onSearchChanged(''),
            ),
            if (!_isLoading && _totalItems > 0)
              Padding(
                padding: const EdgeInsets.symmetric(vertical: AppTheme.spaceM, horizontal: AppTheme.spaceXS),
                child: Row(
                  children: [
                    Text(
                      'Showing ${_alumni.length} of $_totalItems members',
                      style: Theme.of(context).textTheme.labelSmall?.copyWith(fontSize: 8, letterSpacing: 1.5, color: Colors.white38),
                    ),
                    const Spacer(),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                      decoration: BoxDecoration(color: AppTheme.obsidianBlack, borderRadius: BorderRadius.circular(4), border: Border.all(color: AppTheme.glassBorder)),
                      child: Text('SYNCED', style: Theme.of(context).textTheme.labelSmall?.copyWith(color: Colors.greenAccent, fontSize: 8)),
                    ),
                  ],
                ),
              ),
            const SizedBox(height: AppTheme.spaceM),
            Column(
              children: [
                Row(
                  children: [
                    Expanded(
                      child: FutureBuilder<List<Map<String, String>>>(
                        future: ref.read(dropdownDataProvider).getOptions(LookupGroups.passingYear),
                        builder: (context, snapshot) => _buildFilterDropdown('YEAR', snapshot.data?.map((e) => e['label']!).toList() ?? []),
                      ),
                    ),
                    const SizedBox(width: AppTheme.spaceS),
                    Expanded(
                      child: FutureBuilder<List<Map<String, String>>>(
                        future: ref.read(dropdownDataProvider).getOptions('Subject'),
                        builder: (context, snapshot) => _buildFilterDropdown('DEPT', snapshot.data?.map((e) => e['label']!).toList() ?? []),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: AppTheme.spaceS),
                Row(
                  children: [
                    Expanded(
                      // 28.21: keep in sync with MembershipType in GHCAA.Domain/Enums.cs —
                      // omitting a value here silently hides those members from the directory.
                      child: _buildFilterDropdown('TYPE', ['General', 'Founding', 'Executive', 'Associate', 'Honorary', 'Advisory', 'Guest']),
                    ),
                    const SizedBox(width: AppTheme.spaceS),
                    Expanded(
                      child: _buildFilterDropdown('CATEGORY', ['LifelongPatron', 'Sponsor', 'Advisor', 'Mentor', 'Volunteer', 'Student']),
                    ),
                  ],
                ),
              ],
            ),
            const SizedBox(height: AppTheme.spaceL),
            Expanded(
              child: _isLoading 
                ? ListView.builder(
                    itemCount: 6,
                    itemBuilder: (context, index) => Padding(
                      padding: const EdgeInsets.only(bottom: AppTheme.spaceM),
                      child: SkeletonLoader.memberCard(context),
                    ),
                  )
                : RefreshIndicator(
                    color: AppTheme.royalGold,
                    backgroundColor: AppTheme.deepCharcoal,
                    onRefresh: () async {
                      HapticFeedback.mediumImpact();
                      await _fetchAlumni(refresh: true);
                    },
                    child: _alumni.isEmpty
                        ? const EmptyStateWidget('No members found.', icon: Icons.people_outline)
                        : ListView.builder(
                            controller: _scrollController,
                            padding: const EdgeInsets.only(bottom: 100),
                            physics: const AlwaysScrollableScrollPhysics(),
                            itemCount: _alumni.length + (_hasMore || _isLoadingMore ? 1 : 0),
                            itemBuilder: (context, index) {
                              if (index == _alumni.length) {
                                return Padding(
                                  padding: const EdgeInsets.symmetric(vertical: 32.0),
                                  child: Center(child: LogoSpinner.small()),
                                );
                              }
 
                              final m = _alumni[index];
                              String? photoUrl;
                              if (m['photoPath'] != null && m['photoPath'].toString().isNotEmpty) {
                                final p = m['photoPath'];
                                final base = AppConfig.apiBaseUrl.replaceFirst('/api', '');
                                photoUrl = p.startsWith('http') ? p : '$base/$p';
                              }
 
                              return Padding(
                                padding: const EdgeInsets.only(bottom: AppTheme.spaceM),
                                child: Card(
                                  child: InkWell(
                                    borderRadius: BorderRadius.circular(AppTheme.radiusL),
                                    onTap: () {
                                      HapticFeedback.lightImpact();
                                      context.push('/directory/${m['id']}');
                                    },
                                    child: Padding(
                                      padding: const EdgeInsets.all(AppTheme.spaceM),
                                      child: Column(
                                        children: [
                                          Row(
                                            children: [
                                              _buildMemberThumbnail(context, photoUrl, m['fullName']),
                                              const SizedBox(width: AppTheme.spaceM),
                                              Expanded(
                                                child: Column(
                                                  crossAxisAlignment: CrossAxisAlignment.start,
                                                  children: [
                                                    Row(
                                                      children: [
                                                        Flexible(
                                                          child: Text(m['fullName']?.toString() ?? 'Member',
                                                              style: Theme.of(context).textTheme.titleMedium?.copyWith(fontSize: 13, fontWeight: FontWeight.w900, letterSpacing: 0.5),
                                                              overflow: TextOverflow.ellipsis),
                                                        ),
                                                        if (m['isVerified'] == true) ...[
                                                          const SizedBox(width: 6),
                                                          const Icon(Icons.verified_user_rounded, color: AppTheme.royalGold, size: 14),
                                                        ],
                                                      ],
                                                    ),
                                                    const SizedBox(height: 4),
                                                    Row(
                                                      children: [
                                                        Text(m['membershipNumber'] ?? 'PENDING', style: Theme.of(context).textTheme.labelSmall?.copyWith(color: AppTheme.royalGold, fontSize: 8)),
                                                        const SizedBox(width: 8),
                                                        if (m['memberCategory'] != null)
                                                          Container(
                                                            padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                                                            decoration: BoxDecoration(color: AppTheme.royalGold.withValues(alpha: 0.05), borderRadius: BorderRadius.circular(4)),
                                                            child: Text(m['memberCategory']!.toString().toUpperCase(), style: const TextStyle(color: AppTheme.royalGold, fontSize: 7, fontWeight: FontWeight.w900)),
                                                          ),
                                                      ],
                                                    ),
                                                  ],
                                                ),
                                              ),
                                              const Icon(Icons.arrow_forward_ios_rounded, size: 12, color: Colors.white12)
                                            ],
                                          ),
                                          const Padding(
                                            padding: EdgeInsets.symmetric(vertical: AppTheme.spaceM),
                                            child: Divider(color: Colors.white10, height: 1),
                                          ),
                                          Row(
                                            children: [
                                              Flexible(child: _metaBadge(Icons.school_rounded, _getCompactBatch(m))),
                                              const SizedBox(width: 4),
                                              Flexible(child: _metaBadge(Icons.cases_rounded, m['designation'] ?? 'Member')),
                                            ],
                                          ),
                                        ],
                                      ),
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
 
  Widget _metaBadge(IconData icon, String text) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(icon, size: 12, color: AppTheme.royalGold.withValues(alpha: 0.5)),
        const SizedBox(width: AppTheme.spaceXS),
        Flexible(
          child: Text(
            text, 
            style: Theme.of(context).textTheme.labelSmall?.copyWith(fontSize: 8, color: Colors.white54),
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
          ),
        ),
      ],
    );
  }
 
  String _getCompactBatch(Map<String, dynamic> m) {
    final degree = m['degree'] ?? 'HSC';
    final year = m['passingYear']?.toString() ?? '';
    return '$degree $year';
  }
 
  Widget _buildFilterDropdown(String label, List<String> options) {
    final selected = label == 'YEAR'
        ? _selectedBatch
        : (label == 'DEPT'
            ? _selectedDept
            : (label == 'TYPE' ? _selectedType : _selectedCategory));
    return AppDropdownField<String?>(
      value: selected,
      hintText: label,
      items: [
        DropdownMenuItem<String?>(value: null, child: Text('ALL $label')),
        ...options.map((o) => DropdownMenuItem<String?>(value: o, child: Text(o))),
      ],
      onChanged: (v) {
        setState(() {
          if (label == 'YEAR') {
            _selectedBatch = v;
          } else if (label == 'DEPT') {
            _selectedDept = v;
          } else if (label == 'TYPE') {
            _selectedType = v;
          } else {
            _selectedCategory = v;
          }
        });
        _fetchAlumni(refresh: true);
      },
    );
  }
 
  Widget _buildMemberThumbnail(BuildContext context, String? url, String? name) {
    return Container(
      width: 48,
      height: 48,
      decoration: BoxDecoration(
        color: AppTheme.obsidianBlack,
        borderRadius: BorderRadius.circular(AppTheme.radiusM),
        border: Border.all(color: AppTheme.glassBorder),
      ),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(AppTheme.radiusM),
        child: url != null && url.isNotEmpty
          ? CustomNetworkImage(imageUrl: url, fit: BoxFit.cover)
          : const Icon(Icons.person_pin_rounded, color: AppTheme.royalGold, size: 24),
      ),
    );
  }
}
