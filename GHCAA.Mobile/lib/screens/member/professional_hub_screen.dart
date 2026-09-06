import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/api/api_client.dart';
import '../../core/config/app_config.dart';
import '../../core/widgets/empty_state_widget.dart';
import '../../core/widgets/logo_spinner.dart';

final profHubSearchQueryProvider = StateProvider<String>((ref) => '');

class ProfessionalHubScreen extends ConsumerStatefulWidget {
  const ProfessionalHubScreen({super.key});

  @override
  ConsumerState<ProfessionalHubScreen> createState() => _ProfessionalHubScreenState();
}

class _ProfessionalHubScreenState extends ConsumerState<ProfessionalHubScreen> {
  final TextEditingController _searchController = TextEditingController();
  final ScrollController _scrollController = ScrollController();
  
  List<dynamic> _professionals = [];
  bool _isLoading = false;
  bool _isLoadingMore = false;
  bool _hasMore = true;
  int _pageNumber = 1;
  final int _pageSize = 15;
  int _totalItems = 0;

  String? _selectedSector;
  Timer? _searchDebounce;

  @override
  void initState() {
    super.initState();
    _scrollController.addListener(_onScroll);
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _fetchAlumni(refresh: true);
    });
  }

  @override
  void dispose() {
    _searchController.dispose();
    _scrollController.dispose();
    _searchDebounce?.cancel();
    super.dispose();
  }

  void _onScroll() {
    if (_scrollController.position.pixels >= _scrollController.position.maxScrollExtent - 200 && !_isLoadingMore && _hasMore) {
      _fetchAlumni();
    }
  }

  Future<void> _fetchAlumni({bool refresh = false}) async {
    if (refresh) {
      if (!mounted) return;
      setState(() {
        _pageNumber = 1;
        _isLoading = true;
        _hasMore = true;
        _professionals.clear();
      });
    } else {
      if (_isLoadingMore || !_hasMore) return;
      setState(() => _isLoadingMore = true);
      _pageNumber++;
    }

    try {
      final dio = ref.read(dioProvider);
      final query = ref.read(profHubSearchQueryProvider);
      
      final Map<String, dynamic> params = {
        'page': _pageNumber,
        'pageSize': _pageSize,
        if (query.isNotEmpty) 'query': query,
        if (_selectedSector != null && _selectedSector!.isNotEmpty) 'professionalSector': _selectedSector,
      };

      final response = await dio.get('/networking/directory', queryParameters: params);
      
      if (mounted) {
        setState(() {
          final items = (response.data['items'] as List<dynamic>?) ?? [];
          _totalItems = response.data['totalItems'] ?? 0;
          
          if (refresh) {
            _professionals = items;
          } else {
            _professionals.addAll(items);
          }
          
          _hasMore = _professionals.length < _totalItems;
          _isLoading = false;
          _isLoadingMore = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _isLoading = false;
          _isLoadingMore = false;
        });
      }
    }
  }

  void _onSearchChanged(String value) {
    _searchDebounce?.cancel();
    _searchDebounce = Timer(const Duration(milliseconds: 300), () {
      ref.read(profHubSearchQueryProvider.notifier).state = value.toLowerCase();
      _fetchAlumni(refresh: true);
    });
  }

  @override
  Widget build(BuildContext context) {
    return AppScaffold(
      title: 'Professional Hub',
      breadcrumb: 'CAREER > PROFESSIONALS',
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16.0),
        child: Column(
          children: [
            TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Search by industry or designation...',
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
                      'Found $_totalItems professionals',
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
            Expanded(
              child: _isLoading 
                ? const Center(child: LogoSpinner(size: 120))
                : RefreshIndicator(
                    color: AppTheme.royalGold,
                    onRefresh: () async {
                      HapticFeedback.mediumImpact();
                      await _fetchAlumni(refresh: true);
                    },
                    child: _professionals.isEmpty
                      ? ListView(
                          children: const [
                            SizedBox(height: 100),
                            EmptyStateWidget('No professionals found.', icon: Icons.people_outline),
                          ],
                        )
                      : ListView.builder(
                          controller: _scrollController,
                          itemCount: _professionals.length + (_hasMore ? 1 : 0),
                          itemBuilder: (context, index) {
                            if (index == _professionals.length) {
                              return Padding(
                                padding: const EdgeInsets.all(16.0),
                                child: Center(child: LogoSpinner.small()),
                              );
                            }
                            
                            final member = _professionals[index];
                            return _buildMemberCard(member);
                          },
                        ),
                  ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildMemberCard(Map<String, dynamic> member) {
    String? photoUrl;
    if (member['photoUrl'] != null && member['photoUrl'].toString().isNotEmpty) {
      photoUrl = member['photoUrl'];
      if (!photoUrl!.startsWith('http')) {
        final b = AppConfig.apiBaseUrl.replaceFirst('/api', '');
        photoUrl = '$b/${photoUrl.replaceFirst(RegExp(r'^/'), '')}';
      }
    }

    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: GestureDetector(
        onTap: () {
          HapticFeedback.lightImpact();
          context.push('/directory/${member['id']}');
        },
        child: GlassContainer(
          padding: const EdgeInsets.all(16),
          child: Row(
            children: [
              CircleAvatar(
                radius: 28,
                backgroundColor: Colors.white10,
                backgroundImage: photoUrl != null ? NetworkImage(photoUrl) : null,
                child: photoUrl == null
                    ? Text(member['fullName']?[0] ?? '?', style: const TextStyle(fontSize: 20, color: AppTheme.royalGold))
                    : null,
              ),
              const SizedBox(width: 16),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Flexible(
                          child: Text(
                            member['fullName'] ?? 'Unknown Member',
                            style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15, color: Colors.white),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                        ),
                        if (member['isVerified'] == true) ...[
                          const SizedBox(width: 4),
                          const Icon(Icons.verified, color: AppTheme.royalGold, size: 14),
                        ],
                      ],
                    ),
                    const SizedBox(height: 4),
                    Text(
                      '${member['currentDesignation'] ?? 'Professional'} @ ${member['currentOrganization'] ?? 'Alumni Network'}',
                      style: const TextStyle(color: AppTheme.royalGold, fontSize: 11, fontWeight: FontWeight.bold, letterSpacing: 0.5),
                      maxLines: 2,
                    ),
                    const SizedBox(height: 4),
                    Text(
                      'Sector: ${member['professionalSector'] ?? 'Unspecified'}',
                      style: const TextStyle(color: Colors.white54, fontSize: 10),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
