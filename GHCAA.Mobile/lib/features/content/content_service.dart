import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

final newsServiceProvider = Provider<NewsService>((ref) => NewsService(ref.read(dioProvider)));

class NewsService {
  final Dio _dio;
  NewsService(this._dio);

  Future<List<dynamic>> getLatestNews() async {
    try {
      final response = await _dio.get('/news');
      return response.data as List<dynamic>;
    } catch (e) {
      return [];
    }
  }

  Future<List<dynamic>> getNewsByCategory(String category) async {
    try {
      final response = await _dio.get('/news', queryParameters: {'articleCategory': category});
      return response.data as List<dynamic>;
    } catch (e) {
      return [];
    }
  }

  Future<List<dynamic>> getMySubmissions() async {
    try {
      final response = await _dio.get('/news/my-submissions');
      return response.data as List<dynamic>;
    } catch (e) {
      return [];
    }
  }

  Future<void> deleteMySubmission(int id) async {
    await _dio.delete('/news/$id');
  }

  Future<List<dynamic>> getPendingSubmissions() async {
    try {
      final response = await _dio.get('/news/admin/pending');
      return response.data as List<dynamic>;
    } catch (_) { return []; }
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
      return [];
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
