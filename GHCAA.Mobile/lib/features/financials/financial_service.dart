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
      final response = await _dio.get('/financials/my-dues');
      return (response.data['amount'] ?? 0.0).toDouble();
    } catch (e) {
      return 0.0;
    }
  }

  Future<List<dynamic>> getSavedMethods() async {
    try {
      final response = await _dio.get('/financials/saved-methods');
      return response.data as List<dynamic>;
    } catch (_) { return []; }
  }

  Future<bool> deleteSavedMethod(int id) async {
    try {
      final response = await _dio.delete('/financials/saved-methods/$id');
      return response.statusCode == 200;
    } catch (_) { return false; }
  }

  Future<String?> getReceiptUrl(int paymentId) async {
    // Usually we would return binary, but for mobile we might trigger a browser download 
    // or use a specialized file downloader.
    return '${_dio.options.baseUrl}/financials/receipt/$paymentId';
  }
}
