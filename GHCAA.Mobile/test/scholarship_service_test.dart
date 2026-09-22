import 'dart:convert';
import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/features/scholarships/scholarship_service.dart';

class _ScholarshipAdapter implements HttpClientAdapter {
  String? method;
  String? path;
  Map<String, dynamic>? query;
  Object? body;
  Object? submittedBody;

  @override
  void close({bool force = false}) {}

  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<Uint8List>? requestStream,
    Future<void>? cancelFuture,
  ) async {
    method = options.method;
    path = options.path;
    query = options.queryParameters;
    body = options.data;
    if (options.path == '/scholarships/public/funds') {
      return _json([
        {
          'id': 1,
          'name': 'Access fund',
          'description': 'Need based support',
          'targetAmount': 1000,
          'isActive': true,
        }
      ]);
    }
    if (options.path == '/scholarships/calls/3/applications') {
      submittedBody = options.data;
      return _json({
        'id': 8,
        'scholarshipCallId': 3,
        'applicantName': 'Asha',
        'applicantEmail': 'asha@example.com',
        'applicantPhone': '0123',
        'institutionName': 'School',
        'class': '10',
        'guardianName': 'Parent',
        'householdIncome': 500,
        'needStatement': 'Need',
        'meritStatement': 'Merit',
        'status': 'Submitted',
        'submittedAt': '2026-09-22T00:00:00Z',
        'referenceCode': 'SCH-001',
      });
    }
    if (options.path == '/scholarships/status/SCH-001') {
      return _json({
        'referenceCode': 'SCH-001',
        'status': 'UnderReview',
        'academicYear': '2026',
        'submittedAt': '2026-09-22T00:00:00Z',
      });
    }
    throw StateError('Unhandled ${options.method} ${options.path}');
  }

  ResponseBody _json(Object value) => ResponseBody.fromString(
        jsonEncode(value),
        200,
        headers: {
          Headers.contentTypeHeader: ['application/json'],
        },
      );
}

void main() {
  test('loads typed public funds', () async {
    final adapter = _ScholarshipAdapter();
    final service = ScholarshipService(Dio()..httpClientAdapter = adapter);

    final funds = await service.getPublicFunds();

    expect(funds.single.name, 'Access fund');
    expect(funds.single.targetAmount, 1000);
    expect(adapter.path, '/scholarships/public/funds');
  });

  test('submits an application and looks up its status', () async {
    final adapter = _ScholarshipAdapter();
    final service = ScholarshipService(Dio()..httpClientAdapter = adapter);

    final application = await service.submitApplication(
      3,
      const CreateScholarshipApplication(
        applicantName: 'Asha',
        applicantEmail: 'asha@example.com',
        applicantPhone: '0123',
        institutionName: 'School',
        className: '10',
        guardianName: 'Parent',
        householdIncome: 500,
        needStatement: 'Need',
        meritStatement: 'Merit',
      ),
    );
    final status = await service.getStatus('SCH-001', 'asha@example.com');

    expect(application.referenceCode, 'SCH-001');
    expect((adapter.submittedBody as Map)['class'], '10');
    expect(status.status, 'UnderReview');
    expect(adapter.query?['email'], 'asha@example.com');
  });
}
