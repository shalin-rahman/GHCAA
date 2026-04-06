import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/config/app_config.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/password_field.dart';
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
            padding: const EdgeInsets.all(32.0),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                // Branded Logo Integration
                Container(
                  padding: const EdgeInsets.all(4),
                  decoration: BoxDecoration(
                    color: Colors.white.withValues(alpha: 0.05),
                    borderRadius: BorderRadius.circular(24),
                    border: Border.all(
                        color: AppTheme.royalGold.withValues(alpha: 0.2)),
                  ),
                  child: ClipRRect(
                    borderRadius: BorderRadius.circular(20),
                  child: Image.asset(
                    'assets/logo.png',
                    height: 120,
                    width: 120,
                    fit: BoxFit.contain,
                    errorBuilder: (context, error, stackTrace) => const Icon(
                        Icons.school,
                        size: 72,
                        color: AppTheme.royalGold),
                  ),
                ),
              ),
                const SizedBox(height: 24),
                Text(AppConfig.organizationAcronym,
                    style: const TextStyle(
                        fontSize: 28,
                        fontWeight: FontWeight.w900,
                        letterSpacing: 2,
                        color: Colors.white)),
                const SizedBox(height: 4),
                Text(
                  AppConfig.organizationTagline,
                  style: const TextStyle(
                      fontSize: 10,
                      fontStyle: FontStyle.italic,
                      color: AppTheme.royalGold,
                      letterSpacing: 0.5),
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 12),
                Text(AppConfig.organizationName,
                    style: const TextStyle(
                        fontSize: 12, color: AppTheme.textSecondaryDark)),
                const SizedBox(height: 32),
                
                if (_errorMessage != null)
                  Container(
                    width: double.infinity,
                    margin: const EdgeInsets.only(bottom: 16),
                    padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 16),
                    decoration: BoxDecoration(
                      color: Colors.red.withValues(alpha: 0.2),
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: Colors.red.withValues(alpha: 0.5)),
                    ),
                    child: Row(
                      children: [
                        const Icon(Icons.error_outline, color: Colors.redAccent, size: 20),
                        const SizedBox(width: 12),
                        Expanded(
                          child: Text(
                            _errorMessage!,
                            style: const TextStyle(color: Colors.redAccent, fontSize: 13),
                          ),
                        ),
                      ],
                    ),
                  ),

                GlassContainer(
                  padding: const EdgeInsets.all(24.0),
                  child: Column(
                    children: [
                      TextField(
                        controller: _identifierController,
                        decoration: const InputDecoration(
                            labelText: 'User Name',
                            prefixIcon: Icon(Icons.person_outline)),
                      ),
                      const SizedBox(height: 16),
                      PasswordField(
                        controller: _passwordController,
                        labelText: 'Password',
                        textInputAction: TextInputAction.done,
                        onSubmitted: (_) => _handleLogin(),
                      ),
                      const SizedBox(height: 16),
                      Row(
                        children: [
                          Switch(
                            value: _enableBiometric,
                            onChanged: (val) => setState(() => _enableBiometric = val),
                            activeThumbColor: AppTheme.royalGold,
                          ),
                          const SizedBox(width: 8),
                          const Expanded(
                            child: Text(
                              'Enable Biometric Quick Login',
                              style: TextStyle(color: Colors.white70, fontSize: 13),
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 24),
                      _isLoading
                          ? const CircularProgressIndicator(
                              color: AppTheme.royalGold)
                          : SizedBox(
                              width: double.infinity,
                              child: ElevatedButton(
                                  onPressed: _handleLogin,
                                  child: const Text('Login')),
                            ),
                    ],
                  ),
                ),
                const SizedBox(height: 24),
                TextButton(
                  onPressed: () => context.push('/register'),
                  child: const Text('Register',
                      style: TextStyle(color: AppTheme.royalGold)),
                ),
                const SizedBox(height: 32),
                Text(AppConfig.appVersion,
                    style: const TextStyle(fontSize: 10, color: Colors.grey)),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
