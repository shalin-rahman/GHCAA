import 'dart:async';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:signalr_netcore/signalr_client.dart';
import '../../core/api/api_client.dart';
import '../../core/config/app_config.dart';
import '../../core/storage/storage_service.dart';

final chatServiceProvider = Provider<ChatService>((ref) {
  return ChatService(ref);
});

class ChatService {
  final Ref _ref;
  HubConnection? _hubConnection;
  final _messageController = StreamController<Map<String, dynamic>>.broadcast();

  ChatService(this._ref);

  Stream<Map<String, dynamic>> get messageStream => _messageController.stream;

  Future<void> initHub() async {
    if (_hubConnection != null && _hubConnection!.state == HubConnectionState.Connected) return;

    final token = await _ref.read(storageServiceProvider).getToken();
    if (token == null) return;

    final hubUrl = '${AppConfig.apiBaseUrl}/hubs/chat';
    
    _hubConnection = HubConnectionBuilder()
        .withUrl(hubUrl, options: HttpConnectionOptions(
          accessTokenFactory: () async => token,
          skipNegotiation: false,
          transport: HttpTransportType.WebSockets,
        ))
        .withAutomaticReconnect()
        .build();

    _hubConnection!.on('ReceiveMessage', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        _messageController.add(arguments[0] as Map<String, dynamic>);
      }
    });

    try {
      await _hubConnection!.start();
      debugPrint('SignalR: Connected to ChatHub');
    } catch (e) {
      debugPrint('SignalR Error: $e');
    }
  }

  Future<void> sendDirectMessage(int receiverUserId, String message) async {
    if (_hubConnection == null || _hubConnection!.state != HubConnectionState.Connected) {
      await initHub();
    }
    
    try {
      await _hubConnection!.invoke('SendDirectMessage', args: [receiverUserId, message]);
    } catch (e) {
      debugPrint('SignalR Invoke Error: $e');
      rethrow;
    }
  }

  Future<List<dynamic>> getConversations() async {
    final dio = _ref.read(dioProvider);
    final response = await dio.get('/chat/conversations');
    return response.data as List<dynamic>;
  }

  Future<List<dynamic>> getChatHistory(int otherUserId) async {
    final dio = _ref.read(dioProvider);
    final response = await dio.get('/chat/history/$otherUserId');
    return response.data as List<dynamic>;
  }

  void dispose() {
    _hubConnection?.stop();
    _messageController.close();
  }
}

final conversationsProvider = FutureProvider.autoDispose<List<dynamic>>((ref) async {
  return ref.read(chatServiceProvider).getConversations();
});

final chatHistoryProvider = FutureProvider.family.autoDispose<List<dynamic>, int>((ref, otherUserId) async {
  return ref.read(chatServiceProvider).getChatHistory(otherUserId);
});
