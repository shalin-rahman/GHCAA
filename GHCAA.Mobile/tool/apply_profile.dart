// Applies an institution profile pack (profiles/<name>/org-config.json) to the
// platform-level Flutter build files that flutter_launcher_icons and
// flutter_native_splash can't reach on their own: the Android app label, the
// iOS bundle display name, and the per-profile icon/splash generator config.
//
// Usage (from GHCAA.Mobile/):
//   dart run tool/apply_profile.dart --profile=ghc
//   dart run tool/apply_profile.dart --profile=ghc --check   (verify only, no writes)
//
// The GHC flavor's applicationId is pinned in android/app/build.gradle.kts
// and is never computed here — see _requireApplicationId below. Running this
// script never rewrites that value; it only asserts the file still has it.
import 'dart:convert';
import 'dart:io';

// Bundle ids are a platform-store identity, not branding, so they don't come
// from the profile pack. Each shipped flavor gets one fixed entry here.
// GHC's is the live published app; it must never change.
const Map<String, String> _pinnedApplicationIds = {
  'ghc': 'com.ghcaa.portal',
};

void main(List<String> args) {
  final profile = _argValue(args, '--profile') ?? 'ghc';
  final checkOnly = args.contains('--check');

  final configFile = File('../profiles/$profile/org-config.json');
  if (!configFile.existsSync()) {
    stderr.writeln('No profile pack at profiles/$profile/org-config.json');
    exitCode = 1;
    return;
  }

  final config = jsonDecode(configFile.readAsStringSync()) as Map<String, dynamic>;
  final branding = config['Branding'] as Map<String, dynamic>? ?? {};
  final appLabel = (branding['MemberNickname'] as String?)?.trim().isNotEmpty == true
      ? branding['MemberNickname'] as String
      : (branding['ShortName'] as String? ?? profile);

  final applicationId = _pinnedApplicationIds[profile];
  if (applicationId == null) {
    stderr.writeln(
      'No pinned applicationId for profile "$profile". Add one to '
      '_pinnedApplicationIds explicitly before building this flavor — '
      'it is never derived from the profile name.',
    );
    exitCode = 1;
    return;
  }

  var driftFound = false;
  driftFound |= _syncAndroidManifest(appLabel, checkOnly);
  driftFound |= _syncInfoPlist(appLabel, checkOnly);
  driftFound |= _verifyApplicationId(profile, applicationId);
  _writeIconSplashConfig(profile);

  if (driftFound && checkOnly) {
    stderr.writeln('Profile "$profile" is out of sync with the platform files above.');
    exitCode = 1;
    return;
  }

  stdout.writeln(
    'Profile "$profile" applied: label="$appLabel", applicationId="$applicationId"'
    '${checkOnly ? ' (check only, no writes)' : ''}',
  );
}

String? _argValue(List<String> args, String flag) {
  final prefix = '$flag=';
  for (final a in args) {
    if (a.startsWith(prefix)) return a.substring(prefix.length);
  }
  return null;
}

/// Updates the `android:label` on the <application> tag. Returns true if the
/// file's current value didn't already match [appLabel].
bool _syncAndroidManifest(String appLabel, bool checkOnly) {
  final file = File('android/app/src/main/AndroidManifest.xml');
  final content = file.readAsStringSync();
  final labelPattern = RegExp('android:label="([^"]*)"');
  final match = labelPattern.firstMatch(content);
  if (match == null) {
    stderr.writeln('AndroidManifest.xml: no android:label found.');
    return true;
  }
  if (match.group(1) == appLabel) return false;

  if (!checkOnly) {
    file.writeAsStringSync(content.replaceFirst(labelPattern, 'android:label="$appLabel"'));
  }
  return true;
}

/// Updates CFBundleDisplayName and CFBundleName in Info.plist. Returns true
/// if either value didn't already match [appLabel].
bool _syncInfoPlist(String appLabel, bool checkOnly) {
  final file = File('ios/Runner/Info.plist');
  var content = file.readAsStringSync();
  var changed = false;

  for (final key in ['CFBundleDisplayName', 'CFBundleName']) {
    final pattern = RegExp('(<key>$key</key>\\s*<string>)([^<]*)(</string>)');
    final match = pattern.firstMatch(content);
    if (match == null) continue;
    if (match.group(2) != appLabel) {
      changed = true;
      if (!checkOnly) {
        content = content.replaceFirst(pattern, '${match.group(1)}$appLabel${match.group(3)}');
      }
    }
  }

  if (changed && !checkOnly) file.writeAsStringSync(content);
  return changed;
}

/// Confirms the pinned applicationId is still what it must be. This never
/// writes — an applicationId change for a shipped flavor is a decision a
/// person makes deliberately in build.gradle.kts, not something this script
/// should ever touch.
bool _verifyApplicationId(String profile, String expected) {
  final file = File('android/app/build.gradle.kts');
  final content = file.readAsStringSync();
  final match = RegExp(r'applicationId\s*=\s*"([^"]*)"').firstMatch(content);
  final actual = match?.group(1);
  if (actual != expected) {
    stderr.writeln(
      'android/app/build.gradle.kts applicationId is "$actual", expected '
      '"$expected" for profile "$profile". This must be fixed by hand — '
      'never auto-written.',
    );
    return true;
  }
  return false;
}

/// Writes a per-profile flutter_launcher_icons / flutter_native_splash
/// config so `dart run flutter_launcher_icons -f <file>` and
/// `dart run flutter_native_splash:create --path=<file>` can target this
/// profile's art. Falls back to the app's existing assets/logo.png when the
/// profile hasn't supplied its own icon/splash images yet.
void _writeIconSplashConfig(String profile) {
  final iconPath = 'profiles/$profile/assets/icon.png';
  final splashPath = 'profiles/$profile/assets/splash.png';
  final image = File('../$iconPath').existsSync() ? '../$iconPath' : 'assets/logo.png';
  final splash = File('../$splashPath').existsSync() ? '../$splashPath' : 'assets/logo.png';

  File('flutter_launcher_icons-$profile.yaml').writeAsStringSync('''
flutter_icons:
  android: "launcher_icon"
  ios: true
  image_path: "$image"
''');

  File('flutter_native_splash-$profile.yaml').writeAsStringSync('''
flutter_native_splash:
  color: "#0B0E0F"
  image: "$splash"
  android_12:
    image: "$splash"
    color: "#0B0E0F"
''');
}
