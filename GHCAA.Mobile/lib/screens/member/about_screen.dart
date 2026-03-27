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
      title: 'About the Portal',
      child: SingleChildScrollView(
        padding: const EdgeInsets.all(24.0),
        child: Column(
          children: [
            const SizedBox(height: 40),
            Hero(
              tag: 'app_logo',
              child: Container(
                width: 120,
                height: 120,
                decoration: BoxDecoration(
                  borderRadius: BorderRadius.circular(32),
                  border: Border.all(
                      color: AppTheme.royalGold.withValues(alpha: 0.2)),
                ),
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(31),
                  child: Image.asset('assets/logo.png', fit: BoxFit.cover),
                ),
              ),
            ).animate().scale(duration: 600.ms, curve: Curves.easeOutBack),
            const SizedBox(height: 32),
            Text(AppConfig.appName,
                style: const TextStyle(
                    fontSize: 28,
                    fontWeight: FontWeight.w900,
                    letterSpacing: 2,
                    color: Colors.white)),
            Text(AppConfig.appVersion,
                style: const TextStyle(
                    fontSize: 12,
                    color: AppTheme.royalGold,
                    fontWeight: FontWeight.bold)),
            const SizedBox(height: 48),
            _buildInfoCard(
              'OUR MISSION',
              AppConfig.portalDescription,
              Icons.auto_awesome_outlined,
            ).animate().fadeIn(delay: 200.ms).slideY(begin: 0.1),
            const SizedBox(height: 24),
            _buildInfoCard(
              'PLATFORM ROBUSTNESS',
              'Built with enterprise-grade security, real-time auditing, and transparent background processing to serve the Haragangian community worldwide.',
              Icons.security_outlined,
            ).animate().fadeIn(delay: 400.ms).slideY(begin: 0.1),
            const SizedBox(height: 48),
            const Divider(color: Colors.white10),
            const SizedBox(height: 24),
            const Text('ENGINEERING & DESIGN',
                style: TextStyle(
                    fontSize: 10,
                    fontWeight: FontWeight.bold,
                    letterSpacing: 1.5,
                    color: AppTheme.textSecondaryDark)),
            const SizedBox(height: 12),
            const Text('Md Habibur Rahman Shalin',
                style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.bold,
                    color: Colors.white)),
            const Text('Advanced AI Coding & Platform Architecture',
                style:
                    TextStyle(fontSize: 10, color: AppTheme.textSecondaryDark)),
            const SizedBox(height: 64),
            Text('© ${DateTime.now().year} ${AppConfig.organizationAcronym}',
                style: const TextStyle(fontSize: 10, color: Colors.white24)),
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
