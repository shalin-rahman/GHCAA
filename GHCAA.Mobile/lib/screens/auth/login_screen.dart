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
  final _identifierController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _isLoading = false;

  Future<void> _handleLogin() async {
    setState(() => _isLoading = true);
    final success = await ref.read(authServiceProvider).login(
          _identifierController.text,
          _passwordController.text,
        );
    setState(() => _isLoading = false);

    if (success) {
      final role = await ref.read(authServiceProvider).getRole();
      if (role == 'SuperAdmin' || role == 'Admin') {
        context.go('/admin_dashboard');
      } else {
        context.go('/dashboard');
      }
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Login Failed. Check credentials.')));
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
                    color: Colors.white.withOpacity(0.05),
                    borderRadius: BorderRadius.circular(24),
                    border:
                        Border.all(color: AppTheme.royalGold.withOpacity(0.2)),
                  ),
                  child: ClipRRect(
                    borderRadius: BorderRadius.circular(20),
                    child: Image.asset(
                      'assets/logo.jpg',
                      height: 100,
                      width: 100,
                      fit: BoxFit.cover,
                      errorBuilder: (context, error, stackTrace) => const Icon(
                          Icons.school,
                          size: 60,
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
                const SizedBox(height: 48),
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
                      const SizedBox(height: 32),
                      _isLoading
                          ? const CircularProgressIndicator(
                              color: AppTheme.royalGold)
                          : SizedBox(
                              width: double.infinity,
                              child: ElevatedButton(
                                  onPressed: _handleLogin,
                                  child: const Text('AUTHENTICATE')),
                            ),
                    ],
                  ),
                ),
                const SizedBox(height: 24),
                TextButton(
                  onPressed: () => context.push('/register'),
                  child: const Text('Request Enrollment Membership',
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
