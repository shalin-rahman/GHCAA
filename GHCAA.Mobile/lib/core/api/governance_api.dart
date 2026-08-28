import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'api_client.dart';

final governanceApiProvider = Provider<GovernanceApi>((ref) {
  final dio = ref.read(dioProvider);
  return GovernanceApi(dio);
});

class GovernanceApi {
  final Dio _dio;

  GovernanceApi(this._dio);

  Future<Map<String, dynamic>> getCurrentEC() async {
    final response = await _dio.get('/governance/ec/current');
    return response.data;
  }
 
  Future<List<dynamic>> getECHistory() async {
    final response = await _dio.get('/governance/ec/history');
    return response.data;
  }
 
  Future<Map<String, dynamic>> getCurrentConstitution() async {
    final response = await _dio.get('/governance/constitution');
    return response.data;
  }
 
  Future<List<dynamic>> getConstitutionHistory() async {
    final response = await _dio.get('/governance/constitution/history');
    return response.data;
  }
 
  Future<bool> voteOnAmendment(int constitutionId, bool isFor, {String? comments}) async {
    try {
      await _dio.post(
        '/governance/constitution/$constitutionId/vote',
        data: isFor,
        queryParameters: comments != null ? {'comments': comments} : null,
      );
      return true;
    } catch (e) {
      debugPrint('GovernanceApi.voteOnAmendment failed: $e');
      return false;
    }
  }

}
