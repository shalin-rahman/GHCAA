import 'dart:async';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:signalr_netcore/signalr_client.dart';
import '../../core/config/app_config.dart';
import '../../core/storage/storage_service.dart';

final notificationHubServiceProvider = Provider<NotificationHubService>((ref) {
  return NotificationHubService(ref);
});

class NotificationHubService {
  final Ref _ref;
  HubConnection? _hubConnection;
  
  final _notificationController = StreamController<Map<String, dynamic>>.broadcast();
  final _adminAlertController = StreamController<Map<String, dynamic>>.broadcast();
  final _batchMessageController = StreamController<Map<String, dynamic>>.broadcast();
  final _noticeController = StreamController<Map<String, dynamic>>.broadcast();

  NotificationHubService(this._ref);

  Stream<Map<String, dynamic>> get notifications => _notificationController.stream;
  Stream<Map<String, dynamic>> get adminAlerts => _adminAlertController.stream;
  Stream<Map<String, dynamic>> get batchMessages => _batchMessageController.stream;
  Stream<Map<String, dynamic>> get notices => _noticeController.stream;

  Future<void> initHub() async {
    if (_hubConnection != null && _hubConnection!.state == HubConnectionState.Connected) return;

    final token = await _ref.read(storageServiceProvider).getToken();
    if (token == null) return;

    final hubUrl = '${AppConfig.apiBaseUrl}/hubs/notifications';
    
    _hubConnection = HubConnectionBuilder()
        .withUrl(hubUrl, options: HttpConnectionOptions(
          accessTokenFactory: () async => token,
          skipNegotiation: false,
          transport: HttpTransportType.WebSockets,
        ))
        .withAutomaticReconnect()
        .build();

    _hubConnection!.on('ReceiveNotification', (args) {
      if (args != null && args.isNotEmpty) _notificationController.add(args[0] as Map<String, dynamic>);
    });

    _hubConnection!.on('ReceiveAdminAlert', (args) {
      if (args != null && args.isNotEmpty) _adminAlertController.add(args[0] as Map<String, dynamic>);
    });

    _hubConnection!.on('ReceiveBatchMessage', (args) {
      if (args != null && args.isNotEmpty) _batchMessageController.add(args[0] as Map<String, dynamic>);
    });

    _hubConnection!.on('ReceiveNotice', (args) {
      if (args != null && args.isNotEmpty) _noticeController.add(args[0] as Map<String, dynamic>);
    });

    try {
      await _hubConnection!.start();
      debugPrint('SignalR: Connected to NotificationHub');
    } catch (e) {
      debugPrint('SignalR Notification Error: $e');
    }
  }

  Future<void> joinBatch(String batchName) async {
    if (_hubConnection?.state != HubConnectionState.Connected) await initHub();
    await _hubConnection!.invoke('JoinBatch', args: [batchName]);
  }

  Future<void> joinDepartment(String deptName) async {
    if (_hubConnection?.state != HubConnectionState.Connected) await initHub();
    await _hubConnection!.invoke('JoinDepartment', args: [deptName]);
  }

  void dispose() {
    _hubConnection?.stop();
    _notificationController.close();
    _adminAlertController.close();
    _batchMessageController.close();
    _noticeController.close();
  }
}
