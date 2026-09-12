import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/services/app_localizations.dart';
import '../../core/services/org_config_service.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/org_logo.dart';

class AboutScreen extends ConsumerWidget {
  const AboutScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final branding = ref.watch(orgBrandingProvider);
    final localePack = ref.watch(localePackProvider);
    return AppScaffold(
      title: 'Our Heritage',
      breadcrumb: 'Member Portal > Institution Heritage',
      child: SingleChildScrollView(
        padding: const EdgeInsets.symmetric(horizontal: AppTheme.spaceL, vertical: AppTheme.spaceXXL),
        child: Column(
          children: [
            Hero(
              tag: 'app_logo',
              child: Container(
                width: 128,
                height: 128,
                padding: const EdgeInsets.all(AppTheme.spaceS),
                decoration: BoxDecoration(
                  borderRadius: BorderRadius.circular(AppTheme.radiusXL + 16), // 24 + 16 = 40
                  border: Border.all(
                      color: AppTheme.royalGold.withValues(alpha: 0.3), width: 2),
                  color: Colors.transparent,
                ),
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(AppTheme.radiusXL + 8), // 24 + 8 = 32
                  child: const OrgLogo(fit: BoxFit.contain),
                ),
              ),
            ).animate().scale(duration: 600.ms, curve: Curves.easeOutBack),
            const SizedBox(height: AppTheme.spaceXL),
            Text(branding.appName.toUpperCase(),
                textAlign: TextAlign.center,
                style: const TextStyle(
                    fontSize: 22,
                    fontWeight: FontWeight.w900,
                    letterSpacing: 3,
                    height: 1.2,
                    color: Colors.white)),
            const SizedBox(height: AppTheme.spaceS),
            Text(
                branding.establishedOn.isEmpty
                    ? localePack.orgName
                    : 'ESTABLISHED ${branding.establishedOn}',
                style: const TextStyle(
                    fontSize: 10,
                    color: AppTheme.royalGold,
                    fontWeight: FontWeight.w900,
                    letterSpacing: 1.5)),
            const SizedBox(height: AppTheme.spaceXXL),
            _buildInfoCard(
              'ASSOCIATION MISSION',
              localePack.tagline,
              Icons.auto_awesome_outlined,
            ).animate().fadeIn(delay: 200.ms).slideY(begin: 0.1),
            const SizedBox(height: AppTheme.spaceL),
            _buildInfoCard(
              'PLATFORM INTEGRITY',
              AppLocalizations.of(context).translate('about_platform_integrity_desc'),
              Icons.security_outlined,
            ).animate().fadeIn(delay: 400.ms).slideY(begin: 0.1),
            const SizedBox(height: AppTheme.spaceXXL),
            const Divider(color: Colors.white10),
            const SizedBox(height: AppTheme.spaceXL),
            const Text('ENGINEERING & DESIGN REGISTRY',
                style: TextStyle(
                    fontSize: 9,
                    fontWeight: FontWeight.w900,
                    letterSpacing: 2,
                    color: AppTheme.textSecondaryDark)),
            const SizedBox(height: AppTheme.spaceM),
            const Text('MB HABIBUR RAHMAN SHALIN',
                style: TextStyle(
                    fontSize: 15,
                    fontWeight: FontWeight.w900,
                    color: Colors.white,
                    letterSpacing: 0.5)),
            const SizedBox(height: AppTheme.spaceXS),
            Text(AppLocalizations.of(context).translate('about_architect_title'),
                style:
                    const TextStyle(fontSize: 10, color: AppTheme.textSecondaryDark, fontWeight: FontWeight.w500)),
            const SizedBox(height: AppTheme.spaceHUGE),
            Text('© ${DateTime.now().year} ${AppLocalizations.of(context).translate('about_copyright_org')}'.toUpperCase(),
                style: const TextStyle(fontSize: 9, color: Colors.white12, fontWeight: FontWeight.w900, letterSpacing: 1)),
            const SizedBox(height: AppTheme.spaceXL + AppTheme.spaceS), // 40
          ],
        ),
      ),
    );
  }

  Widget _buildInfoCard(String title, String content, IconData icon) {
    return GlassContainer(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(icon, size: 18, color: AppTheme.royalGold),
              const SizedBox(width: AppTheme.spaceS),
              Text(title,
                  style: const TextStyle(
                      fontSize: 11,
                      fontWeight: FontWeight.bold,
                      letterSpacing: 1,
                      color: AppTheme.royalGold)),
            ],
          ),
          const SizedBox(height: AppTheme.spaceM),
          Text(content,
              style: const TextStyle(
                  fontSize: 13, height: 1.6, color: Colors.white70)),
        ],
      ),
    );
  }
}
