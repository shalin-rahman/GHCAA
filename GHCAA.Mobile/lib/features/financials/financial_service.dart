import 'dart:io';
import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';
import '../../core/utils/upload_file_naming.dart';

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
      debugPrint('FinancialService.getLedger failed: $e');
      rethrow;
    }
  }

  Future<double> getOutstandingDues() async {
    try {
      // 82.32: was `response.data['amount']`, but GET /api/financials/my-dues returns a JSON
      // array of MembershipDueDto (Id, Year, Amount, IsPaid, ...), not a single object with an
      // 'amount' key. Indexing a list with a string key threw, and the catch below silently
      // turned every real balance into 0.0 with no error shown to the member.
      final response = await _dio.get('/financials/my-dues');
      final List<dynamic> dues = response.data as List<dynamic>;
      final unpaid = dues.where((d) => d['isPaid'] != true);
      return unpaid.fold<double>(0.0, (sum, d) => sum + ((d['amount'] ?? 0.0) as num).toDouble());
    } catch (e) {
      debugPrint('FinancialService.getOutstandingDues failed: $e');
      return 0.0;
    }
  }

  Future<List<dynamic>> getSavedMethods() async {
    try {
      final response = await _dio.get('/financials/saved-methods');
      return response.data as List<dynamic>;
    } catch (e) {
      debugPrint('FinancialService.getSavedMethods failed: $e');
      rethrow;
    }
  }

  Future<bool> deleteSavedMethod(int id) async {
    try {
      final response = await _dio.delete('/financials/saved-methods/$id');
      return response.statusCode == 200;
    } catch (e) {
      debugPrint('FinancialService.deleteSavedMethod failed: $e');
      return false;
    }
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
    } catch (e) {
      debugPrint('FinancialService.getActivePaymentConfigs failed: $e');
      rethrow;
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
          await MultipartFile.fromFile(receipt.path, filename: UploadFileNaming.forType('paymentproof', receipt.path)),
        ));
      }
      final response = await _dio.post('/financials/record-payment', data: formData);
      return response.statusCode == 200;
    } catch (e) {
      debugPrint('FinancialService.recordPayment failed: $e');
      return false;
    }
  }
}
