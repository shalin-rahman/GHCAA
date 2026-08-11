import 'package:dio/dio.dart';
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
      // If the backend returns a List of items with a 'lookupGroup' field, 
      // we might need to group them ourselves.
      // But if it returns a Map, this is fine.
      if (response.data is Map) {
         return Map<String, List<dynamic>>.from(response.data);
      }
      return {};
    } catch (e) {
      return {};
    }
  }

  Future<List<dynamic>> getByGroup(String group) async {
    try {
      final response = await _dio.get('/lookups/$group');
      return response.data as List<dynamic>;
    } catch (e) {
      return [];
    }
  }
}
