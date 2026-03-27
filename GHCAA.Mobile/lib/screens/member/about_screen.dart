import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import '../../core/config/app_config.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';

class AboutScreen extends StatelessWidget {
  const AboutScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return AppScaffold(
      title: 'Our Heritage',
      breadcrumb: 'Member Portal > Institution Heritage',
      child: SingleChildScrollView(
        padding: const EdgeInsets.symmetric(horizontal: 24.0, vertical: 40.0),
        child: Column(
          children: [
            Hero(
              tag: 'app_logo',
              child: Container(
                width: 130,
                height: 130,
                padding: const EdgeInsets.all(8),
                decoration: BoxDecoration(
                  borderRadius: BorderRadius.circular(40),
                  border: Border.all(
                      color: AppTheme.royalGold.withValues(alpha: 0.3), width: 2),
                  color: Colors.black.withValues(alpha: 0.2),
                ),
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(32),
                  child: Image.asset('assets/logo.png', fit: BoxFit.contain,
                      errorBuilder: (c, e, s) => const Center(child: Icon(Icons.school_outlined, color: AppTheme.royalGold, size: 60))),
                ),
              ),
            ).animate().scale(duration: 600.ms, curve: Curves.easeOutBack),
            const SizedBox(height: 32),
            Text(AppConfig.appName.toUpperCase(),
                textAlign: TextAlign.center,
                style: const TextStyle(
                    fontSize: 22,
                    fontWeight: FontWeight.w900,
                    letterSpacing: 3,
                    height: 1.2,
                    color: Colors.white)),
            const SizedBox(height: 8),
            Text('ESTABLISHED 1971 | ${AppConfig.appVersion}',
                style: const TextStyle(
                    fontSize: 10,
                    color: AppTheme.royalGold,
                    fontWeight: FontWeight.w900,
                    letterSpacing: 1.5)),
            const SizedBox(height: 56),
            _buildInfoCard(
              'ASSOCIATION MISSION',
              AppConfig.portalDescription,
              Icons.auto_awesome_outlined,
            ).animate().fadeIn(delay: 200.ms).slideY(begin: 0.1),
            const SizedBox(height: 24),
            _buildInfoCard(
              'PLATFORM INTEGRITY',
              'Built with enterprise-grade obsidian-cloud security, real-time auditing, and transparent background processing to serve the Haragangian community worldwide.',
              Icons.security_outlined,
            ).animate().fadeIn(delay: 400.ms).slideY(begin: 0.1),
            const SizedBox(height: 56),
            const Divider(color: Colors.white10),
            const SizedBox(height: 32),
            const Text('ENGINEERING & DESIGN REGISTRY',
                style: TextStyle(
                    fontSize: 9,
                    fontWeight: FontWeight.w900,
                    letterSpacing: 2,
                    color: AppTheme.textSecondaryDark)),
            const SizedBox(height: 16),
            const Text('MB HABIBUR RAHMAN SHALIN',
                style: TextStyle(
                    fontSize: 15,
                    fontWeight: FontWeight.w900,
                    color: Colors.white,
                    letterSpacing: 0.5)),
            const SizedBox(height: 4),
            const Text('Advanced Platform Architect & Haragangian Alumnus',
                style:
                    TextStyle(fontSize: 10, color: AppTheme.textSecondaryDark, fontWeight: FontWeight.w500)),
            const SizedBox(height: 64),
            Text('© ${DateTime.now().year} Haragangian Alumni'.toUpperCase(),
                style: const TextStyle(fontSize: 9, color: Colors.white12, fontWeight: FontWeight.w900, letterSpacing: 1)),
            const SizedBox(height: 40),
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
              const SizedBox(width: 8),
              Text(title,
                  style: const TextStyle(
                      fontSize: 11,
                      fontWeight: FontWeight.bold,
                      letterSpacing: 1,
                      color: AppTheme.royalGold)),
            ],
          ),
          const SizedBox(height: 16),
          Text(content,
              style: const TextStyle(
                  fontSize: 13, height: 1.6, color: Colors.white70)),
        ],
      ),
    );
  }
}
