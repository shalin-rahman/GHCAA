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

  Future<List<dynamic>> getGalleryItems() async {
    try {
      final response = await _dio.get('/gallery');
      return response.data as List<dynamic>;
    } catch (e) {
      return [];
    }
  }
}
