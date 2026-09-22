import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/api/api_client.dart';
import 'archive_models.dart';

export 'archive_models.dart';

final archiveServiceProvider = Provider<ArchiveService>(
  (ref) => ArchiveService(ref.read(dioProvider)),
);

class ArchiveService {
  final Dio _dio;

  ArchiveService(this._dio);

  Future<List<ArchiveCollection>> getPublicCollections({String? search}) async {
    final response = await _dio.get(
      '/archive/public',
      queryParameters: search == null || search.trim().isEmpty
          ? null
          : {'search': search.trim()},
    );
    return (response.data as List)
        .map((item) => ArchiveCollection.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<ArchiveItem> getPublicItem(int id) async {
    final response = await _dio.get('/archive/items/$id');
    return ArchiveItem.fromJson(response.data as Map<String, dynamic>);
  }
}
