import 'package:flutter/foundation.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';

class AppConfig {
  static String get apiBaseUrl {
    // Universal Bridge Logic: 
    // Web browsers use localhost. Android emulators use 10.0.2.2.
    if (kIsWeb) {
      return 'http://localhost:5087/api';
    }
    return dotenv.env['BASE_API_URL'] ?? 'http://10.0.2.2:5087/api';
  }

  static String get environment {
    return dotenv.env['ENVIRONMENT'] ?? 'development';
  }

  static String get appName {
    return dotenv.env['APP_NAME'] ?? 'Haragangian';
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
    return dotenv.env['PORTAL_TITLE'] ?? 'Haragangian Portal';
  }

  static String get portalDescription {
    return dotenv.env['PORTAL_DESCRIPTION'] ?? 'Connecting Haraganga College members through secure membership and integrated financial governance.';
  }

  static String get appVersion {
    return dotenv.env['APP_VERSION'] ?? 'v1.0.0 (Gold Edition)';
  }

  static String get memberNoun {
    return dotenv.env['MEMBER_NOUN'] ?? 'Haragangian';
  }
  static String? resolveImageUrl(String? path) {
    if (path == null || path.trim().isEmpty) return null;
    if (path.startsWith('http')) return path;
    
    final baseUrl = apiBaseUrl.replaceFirst('/api', '');
    final cleanPath = path.startsWith('/') ? path.substring(1) : path;
    return '$baseUrl/$cleanPath';
  }
}
