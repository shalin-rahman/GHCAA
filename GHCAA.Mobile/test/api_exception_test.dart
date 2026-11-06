import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:ghcaa_mobile/core/api/api_exception.dart';

DioException _errorWith(dynamic data, {int status = 400}) {
  final options = RequestOptions(path: '/x');
  return DioException(
    requestOptions: options,
    response: Response(requestOptions: options, statusCode: status, data: data),
  );
}

void main() {
  group('84.43: ApiException.serverMessage reads ProblemDetails first', () {
    test('detail wins over title, message and error', () {
      expect(
        ApiException.serverMessage({
          'error': 'e',
          'message': 'm',
          'title': 't',
          'detail': 'd',
        }),
        'd',
      );
    });

    test('falls back to title, then message, then error', () {
      expect(ApiException.serverMessage({'title': 't', 'message': 'm'}), 't');
      expect(ApiException.serverMessage({'message': 'm', 'error': 'e'}), 'm');
      expect(ApiException.serverMessage({'error': 'e'}), 'e');
    });

    test('skips empty values', () {
      expect(ApiException.serverMessage({'detail': '', 'title': 't'}), 't');
    });

    test('84.56: a validation reply gives its first errors entry, not the generic title', () {
      expect(
        ApiException.serverMessage({
          'title': 'One or more validation errors occurred.',
          'errors': {
            'Title': ['The Title field is required.'],
            'CompanyName': ['The CompanyName field is required.'],
          },
        }),
        'The Title field is required.',
      );
    });

    test('84.56: detail still wins over errors; empty errors fall back to title', () {
      expect(
        ApiException.serverMessage({
          'detail': 'd',
          'errors': {'X': ['x']},
        }),
        'd',
      );
      expect(
        ApiException.serverMessage({
          'title': 't',
          'errors': {'X': <String>[], 'Y': ''},
        }),
        't',
      );
    });

    test('84.56: errors as a plain list gives its first entry, as on web', () {
      expect(
        ApiException.serverMessage({
          'title': 'One or more validation errors occurred.',
          'errors': ['Bad link'],
        }),
        'Bad link',
      );
    });

    test('returns null for a body that is not a map or has no known field', () {
      expect(ApiException.serverMessage(null), isNull);
      expect(ApiException.serverMessage('plain text'), isNull);
      expect(ApiException.serverMessage({'status': 400}), isNull);
    });
  });

  group('84.43: ApiException.fromDioException', () {
    test('uses detail over message', () {
      final ex = ApiException.fromDioException(
          _errorWith({'message': 'old', 'detail': 'new'}));
      expect(ex.message, 'new');
      expect(ex.statusCode, 400);
    });

    test('keeps the default message when the body has nothing usable', () {
      final ex = ApiException.fromDioException(_errorWith(null, status: 500));
      expect(ex.message, 'An unexpected network error occurred.');
    });
  });
}
