import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:flutter/services.dart';
import '../../core/config/app_config.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/password_field.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/logo_spinner.dart';
import '../../features/auth/auth_service.dart';

class LoginScreen extends ConsumerStatefulWidget {
  const LoginScreen({super.key});

  @override
  ConsumerState<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends ConsumerState<LoginScreen> {
  String? _errorMessage;
  final _identifierController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _isLoading = false;
  bool _enableBiometric = false;

  Future<void> _handleLogin() async {
    final identifier = _identifierController.text.trim();
    final password = _passwordController.text;

    if (identifier.isEmpty || password.isEmpty) {
      setState(() => _errorMessage = 'Please enter both identifier and password.');
      return;
    }

    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });
    
    try {
      final error = await ref.read(authServiceProvider).login(
            identifier,
            password,
            enableBiometric: _enableBiometric,
          );
          
      if (!mounted) return;

      if (error == null) {
        // Invalidate cached providers so they re-read the newly saved role/profile
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
        setState(() => _errorMessage = error);
        if (!mounted) return;
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Login failed. Please check your credentials.')),
        );
      }
    } catch (e) {
      if (!mounted) return;
      setState(() => _errorMessage = 'An unexpected error occurred: $e');
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('An unexpected error occurred during login.')),
      );
    } finally {
      if (context.mounted) {
        setState(() => _isLoading = false);
      }
    }
  }

  // 29E.1: The social login buttons were dead stubs — they only showed a "Connecting…"
  // snackbar and never authenticated (the Google/Facebook SDK packages aren't in the
  // pubspec, and the org has no gateway keys configured). Rather than ship a button that
  // silently does nothing, the social section is hidden until real SDK wiring lands. The
  // auth_service googleLogin/facebookLogin plumbing is retained for that future work.

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Container(
        decoration: const BoxDecoration(
          gradient: RadialGradient(
            center: Alignment(0, -0.8),
            radius: 1.5,
            colors: [AppTheme.midnightSurface, AppTheme.midnightBase],
          ),
        ),
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.symmetric(horizontal: AppTheme.spaceXL),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const SizedBox(height: AppTheme.spaceHUGE),
                // Branded Logo Integration
                Center(
                  // Transparent crest, no black disc or ring behind it (matches web).
                  child: Padding(
                    padding: const EdgeInsets.all(AppTheme.spaceS),
                    child: ClipRRect(
                      borderRadius: BorderRadius.circular(AppTheme.radiusXL),
                      child: Image.asset(
                        'assets/logo.png',
                        height: 100,
                        width: 100,
                        fit: BoxFit.contain,
                        errorBuilder: (context, error, stackTrace) => const Icon(
                            Icons.account_balance_rounded,
                            size: 60,
                            color: AppTheme.royalGold),
                      ),
                    ),
                  ),
                ),
                const SizedBox(height: AppTheme.spaceL),
                FittedBox(
                  fit: BoxFit.scaleDown,
                  child: Text(AppConfig.organizationAcronym,
                      textAlign: TextAlign.center,
                      style: const TextStyle(
                          fontFamily: 'Outfit',
                          fontSize: 40,
                          fontWeight: FontWeight.w900,
                          letterSpacing: 8,
                          color: Colors.white)),
                ),
                const SizedBox(height: AppTheme.spaceXS),
                FittedBox(
                  fit: BoxFit.scaleDown,
                  child: Text(
                    AppConfig.organizationTagline.toUpperCase(),
                    textAlign: TextAlign.center,
                    style: Theme.of(context).textTheme.labelSmall?.copyWith(
                        color: AppTheme.royalGold,
                        fontSize: 8,
                        fontWeight: FontWeight.w900,
                        letterSpacing: 1.5),
                  ),
                ),
                const SizedBox(height: AppTheme.spaceM),
                Text(AppConfig.organizationName,
                    textAlign: TextAlign.center,
                    style: Theme.of(context).textTheme.bodySmall?.copyWith(color: AppTheme.textSecondaryDark)),
                const SizedBox(height: AppTheme.spaceXXL),
                
                if (_errorMessage != null)
                  Container(
                    margin: const EdgeInsets.only(bottom: AppTheme.spaceL),
                    padding: const EdgeInsets.all(AppTheme.spaceM),
                    decoration: BoxDecoration(
                      color: Colors.red.withValues(alpha: 0.1),
                      borderRadius: BorderRadius.circular(AppTheme.radiusL),
                      border: Border.all(color: Colors.redAccent.withValues(alpha: 0.3)),
                    ),
                    child: Row(
                      children: [
                        const Icon(Icons.warning_amber_rounded, color: Colors.redAccent, size: 20),
                        const SizedBox(width: AppTheme.spaceM),
                        Expanded(
                          child: Text(
                            _errorMessage!,
                            style: const TextStyle(color: Colors.redAccent, fontSize: 12, fontWeight: FontWeight.bold),
                          ),
                        ),
                      ],
                    ),
                  ),
 
                GlassContainer(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      const Text('GHCAA AUTHENTICATION', 
                        textAlign: TextAlign.center,
                        style: TextStyle(
                          color: AppTheme.royalGold,
                          fontWeight: FontWeight.w900,
                          fontSize: 10,
                          letterSpacing: 2,
                          fontFamily: 'Outfit'
                        )),
                      const SizedBox(height: AppTheme.spaceXL),
                      TextField(
                        controller: _identifierController,
                        decoration: const InputDecoration(
                            labelText: 'Email or Username',
                            prefixIcon: Icon(Icons.badge_outlined)),
                      ),
                      const SizedBox(height: AppTheme.spaceM),
                      PasswordField(
                        controller: _passwordController,
                        labelText: 'Password',
                        textInputAction: TextInputAction.done,
                        onSubmitted: (_) => _handleLogin(),
                      ),
                      const SizedBox(height: AppTheme.spaceM),
                      Row(
                        children: [
                            Transform.scale(
                              scale: 0.8,
                              child: Switch(
                                value: _enableBiometric,
                                onChanged: (val) {
                                  HapticFeedback.selectionClick();
                                  setState(() => _enableBiometric = val);
                                },
                                activeThumbColor: AppTheme.royalGold,
                              ),
                            ),
                            Expanded(
                              child: Text(
                                'Enable Biometric Access',
                                style: Theme.of(context).textTheme.bodySmall?.copyWith(fontSize: 11),
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: AppTheme.spaceL),
                        _isLoading
                          ? Center(child: LogoSpinner.small())
                          : ElevatedButton(
                              onPressed: () {
                                HapticFeedback.mediumImpact();
                                _handleLogin();
                              },
                              child: const Text('ENGAGE PORTAL')),
                        // 29E.1: social login section removed — see note above _LoginScreenState.build.
                      ],
                    ),
                  ),
                const SizedBox(height: AppTheme.spaceXL),
                Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    const Flexible(
                      child: Text(
                        "New to the collective?",
                        style: TextStyle(color: Colors.white38, fontSize: 13),
                        overflow: TextOverflow.ellipsis,
                      ),
                    ),
                    const SizedBox(width: 4),
                    FittedBox(
                      child: TextButton(
                        onPressed: () {
                          HapticFeedback.lightImpact();
                          context.push('/register');
                        },
                        child: const Text('REGISTER NOW',
                            style: TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.w900, fontSize: 12)),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: AppTheme.spaceHUGE),
                Text('SYSTEM VERSION ${AppConfig.appVersion}',
                    textAlign: TextAlign.center,
                    style: const TextStyle(fontSize: 9, color: Colors.white10, fontWeight: FontWeight.bold, letterSpacing: 1)),
                const SizedBox(height: AppTheme.spaceL),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
