import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';

final assistantServiceProvider = Provider<AssistantService>((ref) {
  return AssistantService(ref.read(dioProvider));
});

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
      print('AI Assistant Error: $e');
    }
    return 'I apologize, but I am having trouble connecting to the Haraganga network right now.';
  }
}
