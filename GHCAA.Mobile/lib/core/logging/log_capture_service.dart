import 'dart:io';

import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:path_provider/path_provider.dart';

final logCaptureServiceProvider = Provider<LogCaptureService>((ref) => LogCaptureService());

/// 12.3: a rotating, on-device log of API errors and notable app events, so a
/// user reporting a problem (12.4) can attach recent context instead of just
/// the one error visible on screen. Kept mobile-only and file-based — the
/// backend's /contact DTO has a hard 2000-char message limit and no
/// attachment support, so this never touches the API contract.
class LogCaptureService {
  static const int _maxBytes = 256 * 1024;
  static const String _fileName = 'ghcaa_app_log.txt';

  File? _cachedFile;

  Future<File> _logFile() async {
    if (_cachedFile != null) return _cachedFile!;
    final dir = await getApplicationDocumentsDirectory();
    _cachedFile = File('${dir.path}/$_fileName');
    return _cachedFile!;
  }

  Future<void> _append(String category, String message) async {
    try {
      final file = await _logFile();
      final line = '${DateTime.now().toIso8601String()} [$category] $message\n';
      await file.writeAsString(line, mode: FileMode.append, flush: true);
      await _rotateIfNeeded(file);
    } catch (e) {
      debugPrint('LogCaptureService._append failed: $e');
    }
  }

  /// Drops the oldest half of the log once it crosses [_maxBytes], rather than
  /// tracking entry counts or timestamps — cheap and keeps the file bounded
  /// without a rotation scheme the size doesn't justify.
  Future<void> _rotateIfNeeded(File file) async {
    final length = await file.length();
    if (length <= _maxBytes) return;
    final content = await file.readAsString();
    final halfway = content.length ~/ 2;
    final nextLineBreak = content.indexOf('\n', halfway);
    final truncated = content.substring(nextLineBreak == -1 ? halfway : nextLineBreak + 1);
    await file.writeAsString(truncated, flush: true);
  }

  Future<void> logApiError({
    required String method,
    required String path,
    int? statusCode,
    required String message,
  }) => _append('API', '$method $path (${statusCode ?? '-'}) => $message');

  Future<void> logEvent(String message) => _append('EVENT', message);

  /// Returns the last [maxChars] characters of the log, for inlining into the
  /// /contact message body alongside the existing error summary/stack.
  Future<String> tail({int maxChars = 1200}) async {
    try {
      final file = await _logFile();
      if (!await file.exists()) return '(no log entries)';
      final content = await file.readAsString();
      if (content.length <= maxChars) return content;
      return content.substring(content.length - maxChars);
    } catch (e) {
      debugPrint('LogCaptureService.tail failed: $e');
      return '(log unavailable: $e)';
    }
  }

  /// The raw log file, for the "Share Logs" action (share_plus) — the full,
  /// untruncated content the /contact message can't carry.
  Future<File?> logFileForSharing() async {
    try {
      final file = await _logFile();
      return await file.exists() ? file : null;
    } catch (e) {
      debugPrint('LogCaptureService.logFileForSharing failed: $e');
      return null;
    }
  }
}
