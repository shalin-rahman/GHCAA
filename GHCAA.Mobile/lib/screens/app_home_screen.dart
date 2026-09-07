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
import '../core/services/app_localizations.dart';
import '../core/widgets/logo_spinner.dart';
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
    final storage = ref.read(storageServiceProvider);
    final biometricEnabled = await storage.isBiometricEnabled();
    final refreshToken = await storage.getRefreshToken();
    if (hasBio && biometricEnabled && refreshToken != null) {
      setState(() => _canBiometric = true);
    }
  }

  // 82.40: re-authenticates through the stored refresh token instead of an autofilled
  // password — see AuthService.loginWithStoredToken.
  Future<void> _handleBiometricLogin() async {
    final reason = AppLocalizations.of(context).translate('biometric_unlock_prompt');
    final success = await ref.read(biometricServiceProvider).authenticate(reason: reason);
    if (!success) return;

    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    try {
      final error = await ref.read(authServiceProvider).loginWithStoredToken();
      if (!mounted) return;

      if (error == null) {
        HapticFeedback.heavyImpact();
        final role = await ref.read(roleProvider.future);
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
    } finally {
      if (mounted) setState(() => _isLoading = false);
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
        ref.invalidate(roleProvider);
        ref.invalidate(userProfileProvider);
        
        final role = await ref.read(roleProvider.future);
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
            padding: const EdgeInsets.symmetric(horizontal: AppTheme.spaceL, vertical: AppTheme.spaceXL),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const SizedBox(height: AppTheme.spaceXXL),
                Center(
                  child: Hero(
                    tag: 'app_logo',
                    // No disc, ring or glow: assets/logo.png is a transparent crest, so it
                    // sits directly on the backdrop the same way it does on web.
                    child: SizedBox(
                      width: 130,
                      height: 130,
                      child: Padding(
                        padding: const EdgeInsets.all(12),
                        child: Image.asset(
                          'assets/logo.png', 
                          fit: BoxFit.contain,
                          errorBuilder: (context, error, stackTrace) => const Icon(Icons.school, size: 60, color: AppTheme.royalGold),
                        ),
                      ),
                    ),
                  ).animate().scale(duration: 800.ms, curve: Curves.elasticOut).fadeIn(),
                ),
                const SizedBox(height: AppTheme.spaceXL),
                Text(
                  AppConfig.portalTitle,
                  textAlign: TextAlign.center,
                  style: const TextStyle(fontSize: 28, fontWeight: FontWeight.w900, letterSpacing: 2, color: Colors.white),
                ).animate().slideY(begin: 0.3, duration: 600.ms).fadeIn(),
                const SizedBox(height: AppTheme.spaceXS),
                Text(
                  AppConfig.organizationTagline,
                  textAlign: TextAlign.center,
                  style: const TextStyle(fontSize: 9, fontStyle: FontStyle.italic, color: AppTheme.royalGold, letterSpacing: 0.5),
                ).animate().fadeIn(delay: 400.ms),
                 const SizedBox(height: AppTheme.spaceXL),
                
                 GlassContainer(
                   child: Column(
                     crossAxisAlignment: CrossAxisAlignment.stretch,
                     children: [
                       Text(AppLocalizations.of(context).translate('login').toUpperCase(), style: const TextStyle(color: AppTheme.royalGold, fontSize: 10, fontWeight: FontWeight.bold, letterSpacing: 1.5)),
                       const SizedBox(height: AppTheme.spaceM),
                       TextField(
                         controller: _identifierController,
                         style: const TextStyle(color: Colors.white),
                         decoration: InputDecoration(
                           labelText: AppLocalizations.of(context).translate('email_or_nid'),
                           prefixIcon: const Icon(Icons.person_outline, size: 20),
                         ),
                       ),
                       const SizedBox(height: AppTheme.spaceM),
                       PasswordField(
                         controller: _passwordController,
                         labelText: AppLocalizations.of(context).translate('password'),
                         onSubmitted: (_) => _handleLogin(),
                       ),
                       if (_errorMessage != null)
                         Padding(
                           padding: const EdgeInsets.only(top: AppTheme.spaceM),
                           child: Text(_errorMessage!, style: const TextStyle(color: Colors.redAccent, fontSize: 11)),
                         ),
                       const SizedBox(height: AppTheme.spaceL),
                       _isLoading
                         ? Center(child: LogoSpinner.small())
                         : ElevatedButton(
                             onPressed: _handleLogin,
                             child: Text(AppLocalizations.of(context).translate('login').toUpperCase()),
                           ),
                       if (_canBiometric && !_isLoading) ...[
                         const SizedBox(height: AppTheme.spaceM),
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
                 
                 const SizedBox(height: AppTheme.spaceL),
                 Center(
                   child: Column(
                     children: [
                       TextButton(
                         onPressed: () => context.go('/register'),
                         child: Text('${AppLocalizations.of(context).translate('register').toUpperCase()} HERE', style: const TextStyle(color: AppTheme.royalGold, fontSize: 11, fontWeight: FontWeight.w900, letterSpacing: 0.5)),
                       ),
                       const SizedBox(height: AppTheme.spaceM),
                       Consumer(
                         builder: (context, ref, child) {
                           final currentLocale = ref.watch(languageProvider);
                           return Row(
                             mainAxisAlignment: MainAxisAlignment.center,
                             children: [
                               TextButton(
                                 onPressed: () => ref.read(languageProvider.notifier).setLanguage('en'),
                                 child: Text('English', style: TextStyle(color: currentLocale.languageCode == 'en' ? AppTheme.royalGold : Colors.white54, fontSize: 12, fontWeight: FontWeight.bold)),
                               ),
                               const Text('|', style: TextStyle(color: Colors.white24)),
                               TextButton(
                                 onPressed: () => ref.read(languageProvider.notifier).setLanguage('bn'),
                                 child: Text('বাংলা', style: TextStyle(color: currentLocale.languageCode == 'bn' ? AppTheme.royalGold : Colors.white54, fontSize: 12, fontWeight: FontWeight.bold)),
                               ),
                             ],
                           );
                         },
                       ),
                     ],
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
