import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

final activityServiceProvider = Provider<ActivityService>((ref) => ActivityService(ref.read(dioProvider)));

class ActivityService {
  final Dio _dio;
  ActivityService(this._dio);

  Future<List<dynamic>> getMyActivity() async {
    try {
      final response = await _dio.get('/Activity');
      return response.data as List<dynamic>;
    } catch (e) {
      return [];
    }
  }
}
