import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';
import '../../core/constants/app_constants.dart';
import '../../core/models/paged_list.dart';
import '../../core/utils/ttl_cache.dart';

final eventsServiceProvider = Provider<EventsService>((ref) {
  return EventsService(ref.read(dioProvider));
});

class EventsService {
  final Dio _dio;
  final _upcoming = TtlCache<String, PagedList>(CacheConstants.listCacheTtl);
  static const _upcomingKey = 'upcoming';

  EventsService(this._dio);

  /// For callers that change events through dio directly, and for session changes.
  void clearCache() => _upcoming.clear();

  /// Only the first page is cached; it is the one a return to the screen shows.
  Future<PagedList> getUpcomingEventsPage(int page) async {
    if (page == 1) {
      final cached = _upcoming.get(_upcomingKey);
      if (cached != null) return cached;
    }
    try {
      final response = await _dio.get('/events',
          queryParameters: {'page': page, 'pageSize': PagingConstants.listPageSize});
      final events = PagedList.fromJson(response.data as Map<String, dynamic>);
      if (page == 1) _upcoming.set(_upcomingKey, events);
      return events;
    } catch (e) {
      debugPrint('EventsService.getUpcomingEventsPage failed: $e');
      rethrow;
    }
  }

  Future<bool> registerForEvent(int eventId, {double? amount, String? paymentRef, dynamic receipt}) async {
    try {
      final response = await _dio.post('/events/register', data: {
        'eventId': eventId,
        'contributionAmount': amount,
        'paymentReference': paymentRef,
      });
      _upcoming.clear();
      return response.statusCode == 200 || response.statusCode == 201;
    } catch (e) {
      debugPrint('EventsService.registerForEvent failed: $e');
      return false;
    }
  }

  Future<bool> createEvent(Map<String, dynamic> data) async {
    try {
      final response = await _dio.post('/events/admin', data: data);
      _upcoming.clear();
      return response.statusCode == 201 || response.statusCode == 200;
    } catch (e) {
      debugPrint('EventsService.createEvent failed: $e');
      return false;
    }
  }

  Future<bool> deleteEvent(int id) async {
    try {
      final response = await _dio.delete('/events/admin/$id');
      _upcoming.clear();
      return response.statusCode == 200;
    } catch (e) {
      debugPrint('EventsService.deleteEvent failed: $e');
      return false;
    }
  }
}
