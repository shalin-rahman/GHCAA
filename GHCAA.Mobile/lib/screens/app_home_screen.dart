import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:flutter_animate/flutter_animate.dart';
import '../core/config/app_config.dart';
import '../core/theme/app_theme.dart';
import '../core/widgets/glass_container.dart';
import '../core/widgets/password_field.dart';
import '../core/services/biometric_service.dart';
import '../core/storage/storage_service.dart';
import '../features/auth/auth_service.dart';

class AppHomeScreen extends ConsumerStatefulWidget {
  const AppHomeScreen({super.key});

  @override
  ConsumerState<AppHomeScreen> createState() => _AppHomeScreenState();
}

class _AppHomeScreenState extends ConsumerState<AppHomeScreen> {
  final _identifierController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _isLoading = false;
  String? _errorMessage;
  bool _canBiometric = false;

  @override
  void initState() {
    super.initState();
    _checkBiometrics();
  }

  Future<void> _checkBiometrics() async {
    final hasBio = await ref.read(biometricServiceProvider).isBiometricsAvailable();
    final creds = await ref.read(storageServiceProvider).getCredentials();
    if (hasBio && creds != null) {
      setState(() => _canBiometric = true);
    }
  }

  Future<void> _handleBiometricLogin() async {
    final success = await ref.read(biometricServiceProvider).authenticate(reason: 'Unlock Haragangian Portal');
    if (success) {
      final creds = await ref.read(storageServiceProvider).getCredentials();
      if (creds != null) {
        _identifierController.text = creds['username']!;
        _passwordController.text = creds['password']!;
        _handleLogin();
      }
    }
  }

  Future<void> _handleLogin() async {
    final identifier = _identifierController.text.trim();
    final password = _passwordController.text;

    if (identifier.isEmpty || password.isEmpty) {
      setState(() => _errorMessage = 'Enter both ID and password.');
      return;
    }

    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });
    
    try {
      final error = await ref.read(authServiceProvider).login(identifier, password);
      if (!mounted) return;

      if (error == null) {
        HapticFeedback.heavyImpact();
        final role = await ref.read(authServiceProvider).getRole();
        if (!mounted) return;
        if (role.isStaffAdminRole) {
          context.go('/admin_dashboard');
        } else {
          context.go('/dashboard');
        }
      } else {
        HapticFeedback.vibrate();
        setState(() => _errorMessage = error);
      }
    } catch (e) {
      setState(() => _errorMessage = 'Login failed.');
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

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
          child: SingleChildScrollView(
            padding: const EdgeInsets.symmetric(horizontal: 24.0, vertical: 32.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const SizedBox(height: 40),
                Center(
                  child: Hero(
                    tag: 'app_logo',
                    child: Container(
                      width: 130,
                      height: 130,
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        color: Colors.white,
                        shape: BoxShape.circle,
                        border: Border.all(color: AppTheme.royalGold, width: 2),
                        boxShadow: [
                          BoxShadow(
                            color: AppTheme.royalGold.withValues(alpha: 0.3),
                            blurRadius: 20,
                            spreadRadius: 2,
                          ),
                        ],
                      ),
                      child: ClipOval(
                        child: Image.asset(
                          'assets/logo.png', 
                          fit: BoxFit.contain,
                          errorBuilder: (context, error, stackTrace) => const Icon(Icons.school, size: 60, color: AppTheme.royalGold),
                        ),
                      ),
                    ),
                  ).animate().scale(duration: 800.ms, curve: Curves.elasticOut).fadeIn(),
                ),
                const SizedBox(height: 32),
                Text(
                  AppConfig.portalTitle,
                  textAlign: TextAlign.center,
                  style: const TextStyle(fontSize: 28, fontWeight: FontWeight.w900, letterSpacing: 2, color: Colors.white),
                ).animate().slideY(begin: 0.3, duration: 600.ms).fadeIn(),
                const SizedBox(height: 4),
                Text(
                  AppConfig.organizationTagline,
                  textAlign: TextAlign.center,
                  style: const TextStyle(fontSize: 9, fontStyle: FontStyle.italic, color: AppTheme.royalGold, letterSpacing: 0.5),
                ).animate().fadeIn(delay: 400.ms),
                const SizedBox(height: 32),
                
                GlassContainer(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      const Text('QUICK LOGIN', style: TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.bold, letterSpacing: 1.5)),
                      const SizedBox(height: 16),
                      TextField(
                        controller: _identifierController,
                        style: const TextStyle(color: Colors.white),
                        decoration: const InputDecoration(
                          labelText: 'User ID / Phone',
                          prefixIcon: Icon(Icons.person_outline, size: 20),
                        ),
                      ),
                      const SizedBox(height: 12),
                      PasswordField(
                        controller: _passwordController,
                        labelText: 'Password',
                        onSubmitted: (_) => _handleLogin(),
                      ),
                      if (_errorMessage != null)
                        Padding(
                          padding: const EdgeInsets.only(top: 12),
                          child: Text(_errorMessage!, style: const TextStyle(color: Colors.redAccent, fontSize: 11)),
                        ),
                      const SizedBox(height: 20),
                      _isLoading 
                        ? const Center(child: CircularProgressIndicator(color: AppTheme.royalGold))
                        : ElevatedButton(
                            onPressed: _handleLogin,
                            child: const Text('LOGIN'),
                          ),
                      if (_canBiometric && !_isLoading) ...[
                        const SizedBox(height: 12),
                        OutlinedButton.icon(
                          onPressed: _handleBiometricLogin,
                          icon: const Icon(Icons.fingerprint, color: AppTheme.royalGold),
                          label: const Text('Unlock with Biometrics'),
                          style: OutlinedButton.styleFrom(
                            foregroundColor: AppTheme.royalGold,
                            side: const BorderSide(color: AppTheme.royalGold),
                          ),
                        )
                      ],
                    ],
                  ),
                ).animate().slideY(begin: 0.2, duration: 800.ms, curve: Curves.easeOutCirc).fadeIn(delay: 600.ms),
                
                const SizedBox(height: 24),
                Center(
                  child: TextButton(
                    onPressed: () => context.go('/register'),
                    child: const Text('NEW TO GHCAA? REGISTER HERE', style: TextStyle(color: AppTheme.royalGold, fontSize: 11, fontWeight: FontWeight.w900, letterSpacing: 0.5)),
                  ),
                ).animate().fadeIn(delay: 1000.ms),
              ],
            ),
          ),
        ),
      ),
    );
  }

  @override
  void dispose() {
    _identifierController.dispose();
    _passwordController.dispose();
    super.dispose();
  }
}
