import 'package:device_info_plus/device_info_plus.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:package_info_plus/package_info_plus.dart';

/// Collected once at login and sent with auth/audit payloads.
class DeviceInfo {
  final String platform;
  final String osVersion;
  final String model;
  final String appVersion;

  const DeviceInfo({
    required this.platform,
    required this.osVersion,
    required this.model,
    required this.appVersion,
  });

  Map<String, String> toJson() => {
    'platform': platform,
    'osVersion': osVersion,
    'model': model,
    'appVersion': appVersion,
  };

  @override
  String toString() => '$model • $platform $osVersion • v$appVersion';
}

final deviceInfoProvider = FutureProvider<DeviceInfo>((ref) async {
  final plugin = DeviceInfoPlugin();
  final pkg = await PackageInfo.fromPlatform();
  final version = '${pkg.version}+${pkg.buildNumber}';

  if (kIsWeb) {
    final web = await plugin.webBrowserInfo;
    return DeviceInfo(
      platform: 'Web',
      osVersion: web.platform ?? 'Unknown',
      model: web.browserName.name,
      appVersion: version,
    );
  }

  switch (defaultTargetPlatform) {
    case TargetPlatform.android:
      final info = await plugin.androidInfo;
      return DeviceInfo(
        platform: 'Android',
        osVersion: 'Android ${info.version.release}',
        model: '${info.manufacturer} ${info.model}',
        appVersion: version,
      );
    case TargetPlatform.iOS:
      final info = await plugin.iosInfo;
      return DeviceInfo(
        platform: 'iOS',
        osVersion: '${info.systemName} ${info.systemVersion}',
        model: info.utsname.machine,
        appVersion: version,
      );
    case TargetPlatform.windows:
      final info = await plugin.windowsInfo;
      return DeviceInfo(
        platform: 'Windows',
        osVersion: info.productName,
        model: info.computerName,
        appVersion: version,
      );
    default:
      return DeviceInfo(
        platform: defaultTargetPlatform.name,
        osVersion: 'Unknown',
        model: 'Desktop',
        appVersion: version,
      );
  }
});
