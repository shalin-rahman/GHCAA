import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

final eventsServiceProvider = Provider<EventsService>((ref) {
  return EventsService(ref.read(dioProvider));
});

class EventsService {
  final Dio _dio;
  EventsService(this._dio);

  Future<List<dynamic>> getUpcomingEvents() async {
    try {
      final response = await _dio.get('/events');
      return response.data as List<dynamic>;
    } catch (e) {
      return [];
    }
  }

  Future<bool> registerForEvent(int eventId, {double? amount, String? paymentRef, dynamic receipt}) async {
    try {
      final response = await _dio.post('/events/register', data: {
        'eventId': eventId,
        'contributionAmount': amount,
        'paymentReference': paymentRef,
      });
      return response.statusCode == 200 || response.statusCode == 201;
    } catch (e) {
      return false;
    }
  }
}
