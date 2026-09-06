import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/logo_spinner.dart';
import '../../features/auth/auth_service.dart';

class ForgotPasswordScreen extends ConsumerStatefulWidget {
  const ForgotPasswordScreen({super.key});

  @override
  ConsumerState<ForgotPasswordScreen> createState() => _ForgotPasswordScreenState();
}

class _ForgotPasswordScreenState extends ConsumerState<ForgotPasswordScreen> {
  final _identifierController = TextEditingController();
  bool _isLoading = false;
  bool _submitted = false;

  Future<void> _handleSubmit() async {
    final identifier = _identifierController.text.trim();
    if (identifier.isEmpty) return;

    setState(() => _isLoading = true);
    // The backend deliberately gives the same response whether or not the identifier
    // matched an account, so there is nothing here to branch on either.
    await ref.read(authServiceProvider).forgotPassword(identifier);
    if (!mounted) return;
    setState(() {
      _isLoading = false;
      _submitted = true;
    });
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
        child: SafeArea(
          child: Center(
            child: SingleChildScrollView(
              padding: const EdgeInsets.symmetric(horizontal: AppTheme.spaceXL),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Align(
                    alignment: Alignment.centerLeft,
                    child: IconButton(
                      icon: const Icon(Icons.arrow_back, color: Colors.white70),
                      onPressed: () => context.pop(),
                    ),
                  ),
                  const SizedBox(height: AppTheme.spaceL),
                  const Text('RESET PASSWORD',
                      textAlign: TextAlign.center,
                      style: TextStyle(
                          fontFamily: 'Outfit',
                          fontSize: 24,
                          fontWeight: FontWeight.w900,
                          letterSpacing: 2,
                          color: Colors.white)),
                  const SizedBox(height: AppTheme.spaceXXL),
                  GlassContainer(
                    child: _submitted
                        ? Column(
                            crossAxisAlignment: CrossAxisAlignment.stretch,
                            children: [
                              const Icon(Icons.mark_email_read_outlined,
                                  color: AppTheme.royalGold, size: 40),
                              const SizedBox(height: AppTheme.spaceM),
                              const Text(
                                'If that account exists, a password reset link has been sent. Check your inbox.',
                                textAlign: TextAlign.center,
                                style: TextStyle(color: Colors.white70),
                              ),
                              const SizedBox(height: AppTheme.spaceL),
                              ElevatedButton(
                                onPressed: () => context.go('/login'),
                                child: const Text('BACK TO LOGIN'),
                              ),
                            ],
                          )
                        : Column(
                            crossAxisAlignment: CrossAxisAlignment.stretch,
                            children: [
                              const Text(
                                'Enter the email or username on your account and we\'ll send you a link to reset your password.',
                                style: TextStyle(color: Colors.white70, fontSize: 13),
                              ),
                              const SizedBox(height: AppTheme.spaceL),
                              TextField(
                                controller: _identifierController,
                                decoration: const InputDecoration(
                                    labelText: 'Email or Username',
                                    prefixIcon: Icon(Icons.badge_outlined)),
                                onSubmitted: (_) => _handleSubmit(),
                              ),
                              const SizedBox(height: AppTheme.spaceL),
                              _isLoading
                                  ? Center(child: LogoSpinner.small())
                                  : ElevatedButton(
                                      onPressed: _handleSubmit,
                                      child: const Text('SEND RESET LINK'),
                                    ),
                            ],
                          ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
