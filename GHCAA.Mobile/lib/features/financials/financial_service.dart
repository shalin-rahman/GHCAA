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
      // Member call: GET /api/financials/my-history
      // Admin call: GET /api/ledger
      // Standardizing on my-history for the member portal to avoid 403 SuperAdminOnly restriction.
      final response = await _dio.get('/financials/my-history');
      final List<dynamic> history = response.data as List<dynamic>;

      // Map backend fields (paidAt, financialCategory) to mobile expectations (date, description)
      return history.map((item) {
        return {
          ...item,
          'date': item['paidAt'],
          'description': item['financialCategory']?.toString() ?? item['notes'] ?? 'Alumni Contribution',
          'amount': item['amount'],
        };
      }).toList();
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
    // Build URL without the /api suffix to get clean base host URL
    final baseHost = _dio.options.baseUrl.replaceFirst('/api', '');
    return '$baseHost/api/financials/receipt/$paymentId';
  }
}
