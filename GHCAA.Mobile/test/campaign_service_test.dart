import 'dart:convert';
import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/features/campaigns/campaign_service.dart';

class _CampaignAdapter implements HttpClientAdapter {
  String? method;
  String? path;
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
    if (options.path == '/campaigns/public') {
      return _json([
        {
          'id': 1,
          'title': 'Library Fund',
          'slug': 'library-fund',
          'story': 'Rebuilding the alumni library',
          'targetAmount': 5000,
          'amountReceived': 1250,
          'isActive': true,
        }
      ]);
    }
    if (options.path == '/campaigns/library-fund') {
      return _json({
        'id': 1,
        'title': 'Library Fund',
        'slug': 'library-fund',
        'story': 'Rebuilding the alumni library',
        'targetAmount': 5000,
        'amountReceived': 1250,
        'isActive': true,
      });
    }
    if (options.path == '/campaigns/library-fund/honour-roll') {
      return _json({
        'targetAmount': 5000,
        'totalReceived': 1250,
        'progressPercent': 25,
        'donorCount': 2,
        'tiers': [
          {
            'tierName': 'Gold',
            'minimumAmount': 500,
            'donors': [
              {'displayName': 'Asha', 'amountReceived': 1000},
            ],
          }
        ],
        'untiered': [
          {'displayName': 'Anonymous', 'amountReceived': 250},
        ],
      });
    }
    if (options.path == '/campaigns/library-fund/pledges') {
      submittedBody = options.data;
      return _json({
        'id': 9,
        'campaignId': 1,
        'donorName': 'Asha',
        'amount': 250,
        'amountReceived': 0,
        'status': 'Pending',
        'isAnonymous': false,
        'pledgedAt': '2026-09-23T00:00:00Z',
      });
    }
    if (options.path == '/campaigns/my-pledges') {
      return _json([
        {
          'id': 9,
          'campaignId': 1,
          'donorName': 'Asha',
          'amount': 250,
          'amountReceived': 0,
          'status': 'Pending',
          'isAnonymous': false,
          'pledgedAt': '2026-09-23T00:00:00Z',
        }
      ]);
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
  test('loads typed public campaigns', () async {
    final adapter = _CampaignAdapter();
    final service = CampaignService(Dio()..httpClientAdapter = adapter);

    final campaigns = await service.getPublicCampaigns();

    expect(campaigns.single.title, 'Library Fund');
    expect(campaigns.single.progressPercent, 25);
    expect(adapter.path, '/campaigns/public');
  });

  test('loads a campaign by slug', () async {
    final adapter = _CampaignAdapter();
    final service = CampaignService(Dio()..httpClientAdapter = adapter);

    final campaign = await service.getCampaign('library-fund');

    expect(campaign.slug, 'library-fund');
    expect(campaign.amountReceived, 1250);
  });

  test('loads the honour roll with tiers and untiered donors', () async {
    final adapter = _CampaignAdapter();
    final service = CampaignService(Dio()..httpClientAdapter = adapter);

    final roll = await service.getHonourRoll('library-fund');

    expect(roll.tiers.single.tierName, 'Gold');
    expect(roll.tiers.single.donors.single.displayName, 'Asha');
    expect(roll.untiered.single.displayName, 'Anonymous');
  });

  test('submits a pledge and lists my pledges', () async {
    final adapter = _CampaignAdapter();
    final service = CampaignService(Dio()..httpClientAdapter = adapter);

    final pledge = await service.createPledge(
      'library-fund',
      const CreatePledge(amount: 250, message: 'Good luck'),
    );
    final myPledges = await service.getMyPledges();

    expect(pledge.status, 'Pending');
    expect((adapter.submittedBody as Map)['amount'], 250);
    expect(myPledges.single.amount, 250);
  });
}
