import 'package:flutter_dotenv/flutter_dotenv.dart';

class AppConfig {
  static String get apiBaseUrl {
    return dotenv.env['BASE_API_URL'] ?? 'http://127.0.0.1:5000/api';
  }

  static String get environment {
    return dotenv.env['ENVIRONMENT'] ?? 'development';
  }

  static String get appName {
    return dotenv.env['APP_NAME'] ?? 'GHCAA Mobile';
  }
}
