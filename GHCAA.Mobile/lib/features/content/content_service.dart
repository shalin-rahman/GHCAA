import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

final newsServiceProvider = Provider<NewsService>((ref) => NewsService(ref.read(dioProvider)));
final galleryServiceProvider = Provider<GalleryService>((ref) => GalleryService(ref.read(dioProvider)));

class NewsService {
  final Dio _dio;
  NewsService(this._dio);

  Future<List<dynamic>> getLatestNews() async {
    try {
      final response = await _dio.get('/news');
      return response.data as List<dynamic>;
    } catch (e) {
      debugPrint('NewsService.getLatestNews failed: $e');
      rethrow;
    }
  }

  Future<List<dynamic>> getNewsByCategory(String category) async {
    try {
      final response = await _dio.get('/news', queryParameters: {'articleCategory': category});
      return response.data as List<dynamic>;
    } catch (e) {
      debugPrint('NewsService.getNewsByCategory failed: $e');
      rethrow;
    }
  }

  Future<List<dynamic>> getMySubmissions() async {
    try {
      final response = await _dio.get('/news/my-submissions');
      return response.data as List<dynamic>;
    } catch (e) {
      debugPrint('NewsService.getMySubmissions failed: $e');
      rethrow;
    }
  }

  Future<void> deleteMySubmission(int id) async {
    await _dio.delete('/news/$id');
  }

  Future<List<dynamic>> getPendingSubmissions() async {
    try {
      final response = await _dio.get('/news/admin/pending');
      return response.data as List<dynamic>;
    } catch (e) {
      debugPrint('NewsService.getPendingSubmissions failed: $e');
      rethrow;
    }
  }

  Future<bool> resolveArticle(int id, bool approve) async {
    try {
      final endpoint = approve ? 'approve' : 'reject';
      final response = await _dio.post('/news/admin/$id/$endpoint');
      return response.statusCode == 200;
    } catch (_) { return false; }
  }

  Future<List<dynamic>> getGalleryItems() async {
    try {
      final response = await _dio.get('/gallery');
      return response.data as List<dynamic>;
    } catch (e) {
      debugPrint('NewsService.getGalleryItems failed: $e');
      rethrow;
    }
  }

  Future<bool> uploadGalleryItem(Map<String, dynamic> data) async {
    try {
      final String? photoPath = data['photoPath'];
      data.remove('photoPath');

      final formData = FormData.fromMap(data);
      if (photoPath != null && photoPath.isNotEmpty) {
          formData.files.add(MapEntry('photo', await MultipartFile.fromFile(photoPath, filename: 'gallery_item.jpg')));
      }

      final response = await _dio.post('/gallery', data: formData);
      return response.statusCode == 200 || response.statusCode == 201;
    } catch (_) { return false; }
  }
}

class GalleryService {
  final Dio _dio;
  GalleryService(this._dio);

  Future<List<dynamic>> getGalleries({bool onlyActive = true}) async {
    try {
      final endpoint = onlyActive ? '/gallery' : '/gallery/all';
      final response = await _dio.get(endpoint);
      return response.data as List<dynamic>;
    } catch (e) {
      debugPrint('GalleryService.getGalleries failed: $e');
      rethrow;
    }
  }

  Future<bool> createGallery(Map<String, dynamic> gallery) async {
    try {
      // POST /api/gallery/admin takes a JSON object
      final response = await _dio.post('/gallery/admin', data: gallery);
      return response.statusCode == 200 || response.statusCode == 201;
    } catch (_) { return false; }
  }

  Future<bool> updateGallery(int id, Map<String, dynamic> gallery) async {
    try {
      final response = await _dio.put('/gallery/admin/$id', data: gallery);
      return response.statusCode == 200;
    } catch (_) { return false; }
  }

  Future<bool> deleteGallery(int id) async {
    try {
      final response = await _dio.delete('/gallery/admin/$id');
      return response.statusCode == 200;
    } catch (_) { return false; }
  }

  Future<String?> uploadPhoto(String filePath) async {
    try {
      final formData = FormData.fromMap({
        'file': await MultipartFile.fromFile(filePath, filename: 'gallery_upload.jpg'),
      });
      final response = await _dio.post('/gallery/upload-photo', data: formData);
      if (response.statusCode == 200) {
        return response.data['path'];
      }
    } catch (_) {}
    return null;
  }

  Future<bool> addPhotosToGallery(int galleryId, List<String> paths) async {
    try {
      final response = await _dio.post('/gallery/admin/$galleryId/photos', data: paths);
      return response.statusCode == 200;
    } catch (_) { return false; }
  }

  Future<bool> removePhoto(int photoId) async {
    try {
      final response = await _dio.delete('/gallery/admin/photos/$photoId');
      return response.statusCode == 200;
    } catch (_) { return false; }
  }
}
