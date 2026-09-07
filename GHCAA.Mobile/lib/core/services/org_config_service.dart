import 'dart:convert';
import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../config/org_config.dart';
import '../api/api_client.dart';
import 'app_localizations.dart';

// ---------------------------------------------------------------------------
// SharedPreferences cache key
// ---------------------------------------------------------------------------
const String _cacheKey = 'org_config_cache';

// ---------------------------------------------------------------------------
// OrgConfigService — Fetch → Cache → Fallback chain
// ---------------------------------------------------------------------------
/// Fetches [OrgConfig] from the backend, caches the JSON in SharedPreferences,
/// and falls back to [OrgConfig.offlineDefaults] when both network and cache
/// are unavailable. This guarantees the app never crashes from missing config.
class OrgConfigService {
  final Dio _dio;

  OrgConfigService(this._dio);

  /// Primary entry point. Called once on app startup.
  ///
  /// Fallback chain: Network → Cache → Build defaults.
  Future<OrgConfig> load() async {
    // 1. Try network fetch
    try {
      final response = await _dio.get('/config');
      if (response.statusCode == 200 && response.data != null) {
        final Map<String, dynamic> json = response.data is String
            ? jsonDecode(response.data)
            : response.data;
        // Persist to cache for offline resilience
        await _saveToCache(json);
        return OrgConfig.fromJson(json);
      }
    } catch (e) {
      debugPrint('[OrgConfigService] Network fetch failed: $e');
    }

    // 2. Try cached value
    try {
      final cached = await _loadFromCache();
      if (cached != null) {
        debugPrint('[OrgConfigService] Loaded config from cache.');
        return cached;
      }
    } catch (e) {
      debugPrint('[OrgConfigService] Cache read failed: $e');
    }

    // 3. Build defaults — app always works
    debugPrint('[OrgConfigService] Using offline default config.');
    return OrgConfig.offlineDefaults;
  }

  /// Force-refresh: ignores cache, hits network, then updates cache.
  /// Returns null on failure (caller should keep existing state).
  Future<OrgConfig?> refresh() async {
    try {
      final response = await _dio.get('/config');
      if (response.statusCode == 200 && response.data != null) {
        final Map<String, dynamic> json = response.data is String
            ? jsonDecode(response.data)
            : response.data;
        await _saveToCache(json);
        return OrgConfig.fromJson(json);
      }
    } catch (e) {
      debugPrint('[OrgConfigService] Refresh failed: $e');
    }
    return null;
  }

  // ---- Cache helpers ----

  Future<void> _saveToCache(Map<String, dynamic> json) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      await prefs.setString(_cacheKey, jsonEncode(json));
    } catch (e) {
      debugPrint('[OrgConfigService] Cache write failed: $e');
    }
  }

  Future<OrgConfig?> _loadFromCache() async {
    final prefs = await SharedPreferences.getInstance();
    final raw = prefs.getString(_cacheKey);
    if (raw != null && raw.isNotEmpty) {
      return OrgConfig.fromJson(jsonDecode(raw));
    }
    return null;
  }
}

// ---------------------------------------------------------------------------
// Riverpod Providers
// ---------------------------------------------------------------------------

/// The service instance — depends on the Dio provider from api_client.dart.
final orgConfigServiceProvider = Provider<OrgConfigService>((ref) {
  final dio = ref.read(dioProvider);
  return OrgConfigService(dio);
});

/// Async provider that loads OrgConfig once and caches the result for the
/// lifetime of the ProviderScope. Screens watch this to get config values.
final orgConfigProvider = FutureProvider<OrgConfig>((ref) async {
  final service = ref.read(orgConfigServiceProvider);
  return service.load();
});

/// Convenience provider: returns the locale-appropriate [LocalePack] based
/// on the currently selected language. Falls back to English if the active
/// locale doesn't have a matching pack.
final localePackProvider = Provider<LocalePack>((ref) {
  final configAsync = ref.watch(orgConfigProvider);
  final locale = ref.watch(languageProvider);
  final langCode = locale.languageCode;

  return configAsync.when(
    data: (config) => config.locales[langCode] ?? config.locales['en'] ?? LocalePack.fromJson({}),
    loading: () => OrgConfig.offlineDefaults.locales[langCode] ?? OrgConfig.offlineDefaults.locales['en']!,
    error: (_, __) => OrgConfig.offlineDefaults.locales[langCode] ?? OrgConfig.offlineDefaults.locales['en']!,
  );
});

/// Convenience provider: returns [FeatureToggles] for easy feature-gating.
final featureTogglesProvider = Provider<FeatureToggles>((ref) {
  final configAsync = ref.watch(orgConfigProvider);

  return configAsync.when(
    data: (config) => config.features,
    loading: () => OrgConfig.offlineDefaults.features,
    error: (_, __) => OrgConfig.offlineDefaults.features,
  );
});

/// Convenience provider: returns [OrgBranding] for theming and labels.
final orgBrandingProvider = Provider<OrgBranding>((ref) {
  final configAsync = ref.watch(orgConfigProvider);

  return configAsync.when(
    data: (config) => config.branding,
    loading: () => OrgConfig.offlineDefaults.branding,
    error: (_, __) => OrgConfig.offlineDefaults.branding,
  );
});

/// Convenience provider: returns [OrgCurrency] for money formatting.
final orgCurrencyProvider = Provider<OrgCurrency>((ref) {
  final configAsync = ref.watch(orgConfigProvider);

  return configAsync.when(
    data: (config) => config.currency,
    loading: () => OrgConfig.offlineDefaults.currency,
    error: (_, __) => OrgConfig.offlineDefaults.currency,
  );
});
