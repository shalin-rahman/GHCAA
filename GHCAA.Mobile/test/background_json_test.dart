import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/utils/background_json.dart';

void main() {
  group('decodeJsonInBackground', () {
    test('small payload decodes inline and matches jsonDecode', () async {
      const text = '{"id": 1, "name": "GHCAA"}';
      expect(text.codeUnits.length, lessThan(backgroundJsonThresholdBytes));

      final result = await decodeJsonInBackground(text);

      expect(result, jsonDecode(text));
    });

    test('large list payload is routed to a background isolate and still matches jsonDecode', () async {
      final members = List.generate(
        2000,
        (i) => {
          'id': i,
          'fullName': 'Alumni Member Number $i',
          'batchYear': 2000 + (i % 25),
          'email': 'member$i@example.com',
        },
      );
      final text = jsonEncode(members);
      expect(text.codeUnits.length, greaterThan(backgroundJsonThresholdBytes));

      final result = await decodeJsonInBackground(text);

      expect(result, jsonDecode(text));
    });

    test('decodes an empty object the same as jsonDecode', () async {
      const text = '{}';

      final result = await decodeJsonInBackground(text);

      expect(result, jsonDecode(text));
    });
  });
}
