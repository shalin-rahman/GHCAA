import 'package:flutter/foundation.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';

import 'org_config.dart';

class AppConfig {
  static String get apiBaseUrl {
    // Web and desktop hosts reach the API on localhost; Android emulator uses 10.0.2.2.
    if (kIsWeb ||
        defaultTargetPlatform == TargetPlatform.windows ||
        defaultTargetPlatform == TargetPlatform.linux ||
        defaultTargetPlatform == TargetPlatform.macOS) {
      return 'http://localhost:5087/api';
    }
    return dotenv.env['BASE_API_URL'] ?? 'http://10.0.2.2:5087/api';
  }

  static String get environment {
    return dotenv.env['ENVIRONMENT'] ?? 'development';
  }

  static String get appName {
    return dotenv.env['APP_NAME'] ?? OrgConfig.offlineDefaults.branding.shortName;
  }

  static String get organizationName {
    return dotenv.env['ORG_NAME'] ?? OrgConfig.offlineDefaults.branding.fullName;
  }

  static String get organizationAcronym {
    return dotenv.env['ORG_ACRONYM'] ?? OrgConfig.offlineDefaults.branding.institutionAcronym;
  }

  static String get organizationTagline {
    return dotenv.env['ORG_TAGLINE'] ?? (OrgConfig.offlineDefaults.locales['en']?.tagline ?? '');
  }

  static String get portalTitle {
    return dotenv.env['PORTAL_TITLE'] ?? OrgConfig.offlineDefaults.branding.fullName;
  }

  static String get portalDescription {
    return dotenv.env['PORTAL_DESCRIPTION'] ?? '';
  }

  static String get appVersion {
    return dotenv.env['APP_VERSION'] ?? 'v1.0.0 (Gold Edition)';
  }

  static String get memberNoun {
    return dotenv.env['MEMBER_NOUN'] ?? OrgConfig.offlineDefaults.branding.memberNickname;
  }
  static String? resolveImageUrl(String? path) {
    if (path == null || path.trim().isEmpty) return null;
    if (path.startsWith('http')) return path;
    
    final baseUrl = apiBaseUrl.replaceFirst('/api', '');
    final cleanPath = path.startsWith('/') ? path.substring(1) : path;
    return '$baseUrl/$cleanPath';
  }
}
