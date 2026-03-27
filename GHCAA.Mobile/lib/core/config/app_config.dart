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

  static String get organizationName {
    return dotenv.env['ORG_NAME'] ?? 'Government Haraganga College Alumni Association';
  }

  static String get organizationAcronym {
    return dotenv.env['ORG_ACRONYM'] ?? 'GHCAA';
  }

  static String get organizationTagline {
    return dotenv.env['ORG_TAGLINE'] ?? 'Sharing Heritage, Aligning Lives, Integrating Networks';
  }

  static String get portalTitle {
    return dotenv.env['PORTAL_TITLE'] ?? '${organizationAcronym} PORTAL';
  }

  static String get portalDescription {
    return dotenv.env['PORTAL_DESCRIPTION'] ?? 'Connecting $organizationName members through secure membership and integrated financial governance.';
  }

  static String get appVersion {
    return dotenv.env['APP_VERSION'] ?? 'v1.0.0 (Gold Edition)';
  }
}
