import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/api/api_client.dart';
import 'campaign_models.dart';

export 'campaign_models.dart';

final campaignServiceProvider = Provider<CampaignService>(
  (ref) => CampaignService(ref.read(dioProvider)),
);

class CampaignService {
  final Dio _dio;

  CampaignService(this._dio);

  Future<List<Campaign>> getPublicCampaigns() async {
    final response = await _dio.get('/campaigns/public');
    return (response.data as List)
        .map((item) => Campaign.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<Campaign> getCampaign(String slug) async {
    final response = await _dio.get('/campaigns/${Uri.encodeComponent(slug)}');
    return Campaign.fromJson(response.data as Map<String, dynamic>);
  }

  Future<CampaignHonourRoll> getHonourRoll(String slug) async {
    final response =
        await _dio.get('/campaigns/${Uri.encodeComponent(slug)}/honour-roll');
    return CampaignHonourRoll.fromJson(response.data as Map<String, dynamic>);
  }

  Future<CampaignPledge> createPledge(String slug, CreatePledge pledge) async {
    final response = await _dio.post(
      '/campaigns/${Uri.encodeComponent(slug)}/pledges',
      data: pledge.toJson(),
    );
    return CampaignPledge.fromJson(response.data as Map<String, dynamic>);
  }

  Future<List<CampaignPledge>> getMyPledges() async {
    final response = await _dio.get('/campaigns/my-pledges');
    return (response.data as List)
        .map((item) => CampaignPledge.fromJson(item as Map<String, dynamic>))
        .toList();
  }
}
