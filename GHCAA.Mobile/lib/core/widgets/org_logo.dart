import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../config/app_config.dart';
import '../services/org_config_service.dart';

class OrgLogo extends ConsumerWidget {
  const OrgLogo({
    super.key,
    this.width,
    this.height,
    this.fit = BoxFit.contain,
  });

  final double? width;
  final double? height;
  final BoxFit fit;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final logoPath = ref.watch(orgBrandingProvider).logoUrl;
    if (logoPath.isEmpty) {
      return Image.asset(
        'assets/logo.png',
        width: width,
        height: height,
        fit: fit,
      );
    }
    final image = logoPath.startsWith('http')
        ? logoPath
        : '${AppConfig.apiBaseUrl.replaceFirst(RegExp(r'/$'), '')}/${logoPath.replaceFirst(RegExp(r'^/'), '')}';

    return Image.network(
      image,
      width: width,
      height: height,
      fit: fit,
      errorBuilder: (context, error, stackTrace) => Image.asset(
        'assets/logo.png',
        width: width,
        height: height,
        fit: fit,
      ),
    );
  }
}
