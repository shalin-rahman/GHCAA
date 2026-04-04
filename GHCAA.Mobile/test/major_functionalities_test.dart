import 'package:flutter_test/flutter_test.dart';
import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:ghcaa_mobile/features/events/events_service.dart';
import 'package:ghcaa_mobile/features/financials/gateway_service.dart';
import 'package:ghcaa_mobile/core/storage/storage_service.dart';

// --- Fakes ---

class FakeStorage extends StorageService {
  final Map<String, dynamic> _store = {};

  @override
  Future<void> saveToken(String token) async => _store['jwt_token'] = token;
  
  @override
  Future<String?> getToken() async => _store['jwt_token'] as String?;
  
  @override
  Future<void> removeToken() async => _store.remove('jwt_token');

  @override
  Future<void> saveRole(String role) async => _store['user_role'] = role;
  
  @override
  Future<String?> getRole() async => _store['user_role'] as String?;

  @override
  Future<void> clearAll() async => _store.clear();
}

class FakeRef extends Fake implements Ref {
  @override
  T read<T>(ProviderListenable<T> provider) {
    throw UnimplementedError();
  }
}

class FakeDio extends Fake implements Dio {
  final Future<Response> Function(String path, dynamic data)? onPost;
  final Future<Response> Function(String path)? onGet;

  FakeDio({this.onPost, this.onGet});

  @override
  Future<Response<T>> post<T>(
    String path, {
    Object? data,
    Map<String, dynamic>? queryParameters,
    Options? options,
    CancelToken? cancelToken,
    void Function(int, int)? onSendProgress,
    void Function(int, int)? onReceiveProgress,
  }) async {
    if (onPost != null) {
      final res = await onPost!(path, data);
      return res as Response<T>;
    }
    throw UnimplementedError();
  }

  @override
  Future<Response<T>> get<T>(
    String path, {
    Object? data,
    Map<String, dynamic>? queryParameters,
    Options? options,
    CancelToken? cancelToken,
    void Function(int, int)? onReceiveProgress,
  }) async {
    if (onGet != null) {
      final res = await onGet!(path);
      return res as Response<T>;
    }
    throw UnimplementedError();
  }
  
  @override
  BaseOptions get options => BaseOptions(baseUrl: 'https://fake-url.test');
}


void main() {
  group('Major Functionalities Tests (Mobile)', () {
    
    group('Authentication Flow', () {
      test('Successful Login returns null and saves tokens', () async {
        final storage = FakeStorage();
        final dio = FakeDio(
          onPost: (path, data) async {
            if (path == '/auth/login') {
              return Response(
                requestOptions: RequestOptions(path: path),
                statusCode: 200,
                data: {'token': 'dummy_valid_jwt', 'role': 'Admin'},
              );
            }
            throw Exception('Unexpected path');
          },
        );

        final authService = AuthService(dio, storage, FakeRef());

        final result = await authService.login('user@test.com', 'password');

        // null implies success
        expect(result, isNull);
        // Verify storage
        expect(await storage.getToken(), 'dummy_valid_jwt');
        expect(await storage.getRole(), 'Admin');
      });

      test('Failed Login returns generic error message dynamically', () async {
        final storage = FakeStorage();
        final dio = FakeDio(
          onPost: (path, data) async {
            throw DioException(
              requestOptions: RequestOptions(path: path),
              response: Response(
                requestOptions: RequestOptions(path: path),
                statusCode: 401,
              ),
            );
          },
        );

        final authService = AuthService(dio, storage, FakeRef());
        final result = await authService.login('wrong', 'wrong');

        expect(result, "Invalid username or password.");
        expect(await storage.getToken(), isNull);
      });
    });

    group('Networking & Events Flow', () {
       test('Fetching upcoming events returns mapped list', () async {
        final dio = FakeDio(
          onGet: (path) async {
            if (path == '/events') {
              return Response(
                requestOptions: RequestOptions(path: path),
                statusCode: 200,
                data: [
                  {'id': 1, 'title': 'Reunion 2026'},
                  {'id': 2, 'title': 'AGM Meeting'},
                ],
              );
            }
            throw Exception('Unexpected get request');
          },
        );

        final eventsService = EventsService(dio);
        final events = await eventsService.getUpcomingEvents();

        expect(events.length, 2);
        expect(events[0]['title'], 'Reunion 2026');
      });

      test('Register for Event correctly submits payload', () async {
         final dio = FakeDio(
          onPost: (path, data) async {
            if (path == '/events/register') {
              // Ensure payload is correct
              final map = data as Map<String, dynamic>;
              expect(map['eventId'], 10);
              expect(map['contributionAmount'], 500.0);
              
              return Response(
                requestOptions: RequestOptions(path: path),
                statusCode: 201, 
              );
            }
            return Response(requestOptions: RequestOptions(path: path), statusCode: 500);
          },
        );

        final eventsService = EventsService(dio);
        final result = await eventsService.registerForEvent(10, amount: 500.0, paymentRef: 'TRX_123');

        expect(result, isTrue);
      });
    });

    group('Financials & Gateways Flow', () {
      test('Initiate payment successfully returns constructed initiation URL', () async {
        final dio = FakeDio(
          onPost: (path, data) async {
            if (path == '/gateways/initiate') {
              return Response(
                requestOptions: RequestOptions(path: path),
                statusCode: 200,
                data: {
                  'success': true,
                  'gatewayUrl': 'https://sandbox.sslcommerz.com/checkout',
                  'transactionId': 'GHCAA-ABCDEF'
                },
              );
            }
             throw Exception('Unexpected post request');
          },
        );

        final gatewayService = GatewayService(dio);
        final response = await gatewayService.initiate(100.0, PaymentGateway.sslCommerz, 'EVT-REG-4');

        expect(response.success, isTrue);
        expect(response.gatewayUrl, 'https://sandbox.sslcommerz.com/checkout');
        expect(response.transactionId, 'GHCAA-ABCDEF');
      });

      test('Initiate payment failure returns safe response wrapper', () async {
        final dio = FakeDio(
          onPost: (path, data) async {
            throw Exception('Connection Reset');
          },
        );

        final gatewayService = GatewayService(dio);
        final response = await gatewayService.initiate(100.0, PaymentGateway.bkash, 'EVT-REG-4');

        expect(response.success, isFalse);
        expect(response.gatewayUrl, isNull);
        expect(response.message, contains('Connection Reset'));
      });
    });
  });
}
