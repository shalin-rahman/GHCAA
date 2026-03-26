import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:flutter_animate/flutter_animate.dart';
import '../core/theme/app_theme.dart';
import '../core/widgets/glass_container.dart';

class AppHomeScreen extends StatelessWidget {
  const AppHomeScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return Scaffold(
      body: Container(
        decoration: BoxDecoration(
          gradient: RadialGradient(
            center: const Alignment(0, -0.6),
            radius: 1.5,
            colors: isDark
                ? [AppTheme.midnightSurface, AppTheme.midnightBase]
                : [AppTheme.daylightSurface, AppTheme.daylightBase],
            stops: const [0.0, 1.0],
          ),
        ),
        child: SafeArea(
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 24.0, vertical: 32.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const Spacer(flex: 2),
                Center(
                  child: Hero(
                    tag: 'app_logo',
                    child: Container(
                      width: 140,
                      height: 140,
                      padding: const EdgeInsets.all(4),
                      decoration: BoxDecoration(
                        color: Colors.white.withOpacity(0.05),
                        borderRadius: BorderRadius.circular(32),
                        border: Border.all(color: AppTheme.royalGold.withOpacity(0.2)),
                      ),
                      child: ClipRRect(
                        borderRadius: BorderRadius.circular(28),
                        child: Image.asset(
                          'assets/logo.jpg', 
                          fit: BoxFit.cover,
                          errorBuilder: (context, error, stackTrace) => const Icon(Icons.school, size: 60, color: AppTheme.royalGold),
                        ),
                      ),
                    ),
                  ).animate().scale(duration: 800.ms, curve: Curves.elasticOut).fadeIn(),
                ),
                const SizedBox(height: 48),
                const Text(
                  'GHCAA PORTAL',
                  textAlign: TextAlign.center,
                  style: TextStyle(fontSize: 32, fontWeight: FontWeight.w900, letterSpacing: 3, color: Colors.white),
                ).animate().slideY(begin: 0.3, duration: 600.ms).fadeIn(),
                const SizedBox(height: 8),
                const Text(
                  'Sharing Heritage, Aligning Lives, Integrating Networks',
                  textAlign: TextAlign.center,
                  style: TextStyle(fontSize: 10, fontStyle: FontStyle.italic, color: AppTheme.royalGold, letterSpacing: 0.5),
                ).animate().fadeIn(delay: 400.ms),
                const SizedBox(height: 24),
                const Text(
                  'Connecting Government Haraganga College Alumni through secure membership and integrated financial governance.',
                  textAlign: TextAlign.center,
                  style: TextStyle(fontSize: 13, color: AppTheme.textSecondaryDark, height: 1.5),
                ).animate().fadeIn(delay: 600.ms),
                const Spacer(flex: 3),
                
                GlassContainer(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      ElevatedButton(
                        onPressed: () => context.go('/register'),
                        child: const Text('Register as Member'),
                      ),
                      const SizedBox(height: 16),
                      OutlinedButton(
                        onPressed: () => context.go('/login'),
                        child: const Text('Member / Admin Login'),
                      ),
                    ],
                  ),
                ).animate().slideY(begin: 0.2, duration: 800.ms, curve: Curves.easeOutCirc).fadeIn(delay: 800.ms),
                const Spacer(flex: 1),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
