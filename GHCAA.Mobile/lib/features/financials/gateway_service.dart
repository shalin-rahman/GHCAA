import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

enum PaymentGateway { none, stripe, paypal, sslCommerz, bkash, nagad, rocket, bankTransfer, manual, dgePay }

class PaymentInitiationResponse {
  final bool success;
  final String? gatewayUrl;
  final String? message;
  final String? transactionId;

  PaymentInitiationResponse({required this.success, this.gatewayUrl, this.message, this.transactionId});

  factory PaymentInitiationResponse.fromJson(Map<String, dynamic> json) {
    return PaymentInitiationResponse(
      success: json['success'] ?? false,
      gatewayUrl: json['gatewayUrl'],
      message: json['message'],
      transactionId: json['transactionId'],
    );
  }
}

final gatewayServiceProvider = Provider<GatewayService>((ref) => GatewayService(ref.read(dioProvider)));

class GatewayService {
  final Dio _dio;
  GatewayService(this._dio);

  Future<PaymentInitiationResponse> initiate(double amount, PaymentGateway gateway, String reference) async {
    try {
      final response = await _dio.post('/gateways/initiate', data: {
        'amount': amount,
        'gateway': gateway.index, // Assuming Enum index matches backend Enums
        'reference': reference,
        'baseUrl': 'https://api.ghcaa.org', // This would normally be your real API base
      });
      return PaymentInitiationResponse.fromJson(response.data);
    } catch (e) {
      // e.toString() on a DioException includes the full request URI and can include response
      // body detail — that was rendered straight into a user-facing SnackBar (events_screen.dart).
      debugPrint('GatewayService.initiate failed: $e');
      return PaymentInitiationResponse(success: false, message: 'Could not start the payment. Please try again.');
    }
  }
}
