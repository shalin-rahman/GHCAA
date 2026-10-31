import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';
import '../../core/constants/app_constants.dart';
import '../../core/constants/registration_constants.dart';
import '../../core/utils/ttl_cache.dart';

final lookupServiceProvider = Provider<LookupService>((ref) => LookupService(ref.read(dioProvider)));

final lookupsByGroupProvider = FutureProvider.family<List<dynamic>, String>((ref, group) async {
  return ref.read(lookupServiceProvider).getByGroup(group);
});

// Used when the PassingYear lookup is empty. Newest first, starting at this year + 6
// in the association's time zone, the same rule the API validates against.
List<String> passingYearFallback([DateTime? nowUtc]) {
  final currentYear = (nowUtc ?? DateTime.now().toUtc()).add(AcademicConstants.associationUtcOffset).year;
  final latestYear = currentYear + AcademicConstants.passingYearFutureYears;
  return List.generate(
    latestYear - AcademicConstants.earliestPassingYear + 1,
    (index) => '${latestYear - index}',
  );
}

final batchListProvider = FutureProvider<List<String>>((ref) async {
  final items = await ref.read(lookupServiceProvider).getByGroup('PassingYear');
  if (items.isEmpty) return passingYearFallback();
  return items.map((e) => e['value'].toString()).toList()..sort((a,b) => b.compareTo(a));
});

final sectorListProvider = FutureProvider<List<String>>((ref) async {
  final items = await ref.read(lookupServiceProvider).getByGroup('ProfessionalSector');
  return items.map((e) => e['value'].toString()).toList();
});

class LookupService {
  final Dio _dio;
  final _groups = TtlCache<String, List<dynamic>>(CacheConstants.listCacheTtl);
  final _all = TtlCache<String, Map<String, List<dynamic>>>(CacheConstants.listCacheTtl);
  static const _allKey = 'all';

  LookupService(this._dio);

  void clearCache() {
    _groups.clear();
    _all.clear();
  }

  Future<Map<String, List<dynamic>>> getAllLookups() async {
    final cachedAll = _all.get(_allKey);
    if (cachedAll != null) return cachedAll;
    try {
      final response = await _dio.get('/lookups');
      // Only the Map shape is handled; a List response (e.g. flat items with a
      // 'lookupGroup' field) falls through to the empty map below, ungrouped.
      if (response.data is Map) {
         final all = Map<String, List<dynamic>>.unmodifiable({
           for (final e in (response.data as Map).entries)
             e.key as String: List<dynamic>.unmodifiable(e.value as List),
         });
         _all.set(_allKey, all);
         return all;
      }
      return {};
    } catch (e) {
      debugPrint('LookupService.getAllLookups failed: $e');
      return {};
    }
  }

  Future<List<dynamic>> getByGroup(String group) async {
    final cached = _groups.get(group);
    if (cached != null) return cached;
    try {
      final response = await _dio.get('/lookups/$group');
      final items = List<dynamic>.unmodifiable(response.data as List<dynamic>);
      _groups.set(group, items);
      return items;
    } catch (e) {
      debugPrint('LookupService.getByGroup($group) failed: $e');
      return [];
    }
  }
}
