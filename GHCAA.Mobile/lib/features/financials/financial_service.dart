import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

final financialServiceProvider = Provider<FinancialService>((ref) {
  return FinancialService(ref.read(dioProvider));
});

class FinancialService {
  final Dio _dio;
  FinancialService(this._dio);

  Future<List<dynamic>> getLedger() async {
    try {
      final response = await _dio.get('/financial/ledger');
      return response.data as List<dynamic>;
    } catch (e) {
      return [];
    }
  }

  Future<double> getOutstandingDues() async {
    try {
      final response = await _dio.get('/financial/dues');
      return (response.data['amount'] ?? 0.0).toDouble();
    } catch (e) {
      return 0.0;
    }
  }
}
