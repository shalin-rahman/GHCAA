import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

final lookupServiceProvider = Provider<LookupService>((ref) => LookupService(ref.read(dioProvider)));

final lookupsByGroupProvider = FutureProvider.family<List<dynamic>, String>((ref, group) async {
  return ref.read(lookupServiceProvider).getByGroup(group);
});

final batchListProvider = FutureProvider<List<String>>((ref) async {
  final items = await ref.read(lookupServiceProvider).getByGroup('PassingYear');
  return items.map((e) => e['value'].toString()).toList()..sort((a,b) => b.compareTo(a));
});

final sectorListProvider = FutureProvider<List<String>>((ref) async {
  final items = await ref.read(lookupServiceProvider).getByGroup('ProfessionalSector');
  return items.map((e) => e['value'].toString()).toList();
});

class LookupService {
  final Dio _dio;
  LookupService(this._dio);

  Future<Map<String, List<dynamic>>> getAllLookups() async {
    try {
      final response = await _dio.get('/lookups');
      // Only the Map shape is handled; a List response (e.g. flat items with a
      // 'lookupGroup' field) falls through to the empty map below, ungrouped.
      if (response.data is Map) {
         return Map<String, List<dynamic>>.from(response.data);
      }
      return {};
    } catch (e) {
      debugPrint('LookupService.getAllLookups failed: $e');
      return {};
    }
  }

  Future<List<dynamic>> getByGroup(String group) async {
    try {
      final response = await _dio.get('/lookups/$group');
      return response.data as List<dynamic>;
    } catch (e) {
      debugPrint('LookupService.getByGroup($group) failed: $e');
      return [];
    }
  }
}
