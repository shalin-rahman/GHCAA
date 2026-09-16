import 'dart:convert';
import 'package:flutter/foundation.dart';

/// Below this size, jsonDecode is fast enough that handing it to a
/// background isolate costs more than just running it inline (isolate
/// message-passing has real overhead). Matches the threshold dio's own
/// BackgroundTransformer uses, since this replaces that transformer's
/// callback with one we can unit test directly.
const int backgroundJsonThresholdBytes = 50 * 1024;

/// Decodes [text] as JSON. Large payloads (alumni directory, events,
/// gallery lists, etc.) are decoded on a background isolate via
/// [compute] so parsing them doesn't jank the UI thread; small ones
/// decode inline.
Future<dynamic> decodeJsonInBackground(String text) {
  if (text.codeUnits.length < backgroundJsonThresholdBytes) {
    return Future.value(jsonDecode(text));
  }
  return compute(jsonDecode, text);
}
