import 'dart:convert';
import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:ghcaa_mobile/core/config/org_config.dart';
import 'package:ghcaa_mobile/core/services/org_config_service.dart';

void main() {
  final networkConfig = {
    'branding': {
      'appName': 'Network Association',
      'shortName': 'NA',
      'fullName': 'Network Association Alumni',
      'institutionName': 'Network College',
      'institutionAcronym': 'NCA',
      'primaryColor': '#102030',
      'accentColor': '#D4AF37',
    },
    'enabledGatewayMethods': ['Manual'],
    'features': {'enableFundraising': false},
    'localization': {'dateFormat': 'MM/dd/yyyy'},
    'documents': [
      {
        'label': 'Network Bylaws',
        'url': '/assets/bylaws.pdf',
        'version': '1.0',
        'group': 'Governance',
      }
    ],
  };

  setUp(() {
    SharedPreferences.setMockInitialValues({});
  });

  Dio serviceDio(Response<dynamic> Function(RequestOptions) handler) {
    final dio = Dio();
    dio.httpClientAdapter = _CallbackAdapter(handler);
    return dio;
  }

  test('loads network configuration and caches the selected date format',
      () async {
    final service = OrgConfigService(serviceDio((request) => Response(
          requestOptions: request,
          statusCode: 200,
          data: networkConfig,
        )));

    final config = await service.load();

    expect(config.branding.appName, 'Network Association');
    expect(config.enabledGatewayMethods, ['Manual']);
    expect(config.features.enableFundraising, isFalse);
    expect(config.dateFormat.identifier, DateFormatConfig.mmDdYyyy);
    expect(config.documents, hasLength(1));
    expect(config.documents.single.label, 'Network Bylaws');
    expect(config.documents.single.group, 'Governance');
    final prefs = await SharedPreferences.getInstance();
    expect(prefs.getString('org_config_cache'), isNotEmpty);
  });

  test('defaults documents to an empty list when the field is absent',
      () async {
    final service = OrgConfigService(serviceDio((request) => Response(
          requestOptions: request,
          statusCode: 200,
          data: {
            'branding': {'appName': 'No Docs Association'},
          },
        )));

    final config = await service.load();

    expect(config.documents, isEmpty);
  });

  test('uses cached configuration when the network is unavailable', () async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('org_config_cache',
        '{"branding":{"appName":"Cached Association"},"localization":{"dateFormat":"MM/dd/yyyy"}}');
    final service = OrgConfigService(serviceDio((request) {
      throw DioException(
        requestOptions: request,
        error: 'offline',
        type: DioExceptionType.connectionError,
      );
    }));

    final config = await service.load();

    expect(config.branding.appName, 'Cached Association');
    expect(config.dateFormat.identifier, DateFormatConfig.mmDdYyyy);
  });

  test('uses offline defaults when network and cache are unavailable',
      () async {
    final service = OrgConfigService(serviceDio((request) => throw DioException(
          requestOptions: request,
          type: DioExceptionType.connectionError,
        )));

    final config = await service.load();

    expect(config.branding.appName, OrgConfig.offlineDefaults.branding.appName);
    expect(config.dateFormat.identifier, DateFormatConfig.ddMmYyyy);
  });
}

class _CallbackAdapter implements HttpClientAdapter {
  _CallbackAdapter(this.handler);

  final Response<dynamic> Function(RequestOptions) handler;

  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<Uint8List>? requestStream,
    Future<void>? cancelFuture,
  ) async {
    final response = handler(options);
    return ResponseBody.fromString(
      jsonEncode(response.data),
      response.statusCode ?? 200,
      headers: {
        Headers.contentTypeHeader: ['application/json'],
      },
    );
  }

  @override
  void close({bool force = false}) {}
}
