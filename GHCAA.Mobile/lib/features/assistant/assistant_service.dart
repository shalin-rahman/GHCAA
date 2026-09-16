import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

final assistantServiceProvider = Provider<AssistantService>((ref) => AssistantService(ref.read(dioProvider)));

class AssistantService {
  final Dio _dio;
  AssistantService(this._dio);

  Future<String?> ask(String question) async {
    try {
      final response = await _dio.post('/assistant/ask', data: {'question': question});
      if (response.statusCode == 200) {
        return response.data['answer'] as String?;
      }
    } catch (e) {
      debugPrint('AssistantService.ask failed: $e');
      return 'The assistant is temporarily offline. Please try again later.';
    }
    return "I can't answer that right now. Try again.";
  }
}
