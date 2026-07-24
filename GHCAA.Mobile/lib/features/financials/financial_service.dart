import 'dart:io';
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

  /// Admin-configured, enabled payment methods (bKash/Nagad/Rocket/Bank/Manual, etc).
  /// These are the manual-payment channels the member pays into; no live gateway keys required.
  /// GET /api/payment-config/active (AllowAnonymous). Enum fields arrive as string names.
  Future<List<dynamic>> getActivePaymentConfigs() async {
    try {
      final response = await _dio.get('/payment-config/active');
      return response.data as List<dynamic>;
    } catch (_) {
      return [];
    }
  }

  /// Submit a manual payment for admin verification.
  /// POST /api/financials/record-payment ([FromForm] multipart). MemberId is derived
  /// server-side from the JWT. [paymentMethod] must be a PaymentMethod enum name
  /// (e.g. "BKash"); [financialCategory] a FinancialCategory enum name (e.g. "MembershipFee").
  Future<bool> recordPayment({
    required String transactionId,
    required double amount,
    required String paymentMethod,
    required String financialCategory,
    String? notes,
    File? receipt,
  }) async {
    try {
      final formData = FormData.fromMap({
        'transactionId': transactionId,
        'amount': amount,
        'paidAt': DateTime.now().toUtc().toIso8601String(),
        'financialCategory': financialCategory,
        'paymentMethod': paymentMethod,
        if (notes != null && notes.isNotEmpty) 'notes': notes,
      });
      if (receipt != null) {
        formData.files.add(MapEntry(
          'receipt',
          await MultipartFile.fromFile(receipt.path, filename: receipt.path.split('/').last),
        ));
      }
      final response = await _dio.post('/financials/record-payment', data: formData);
      return response.statusCode == 200;
    } catch (_) {
      return false;
    }
  }
}
