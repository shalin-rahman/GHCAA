import 'package:flutter/material.dart';
import '../theme/app_theme.dart';

/// A reusable password field with show/hide toggle.
/// Use this on every screen that has a password input.
class PasswordField extends StatefulWidget {
  final TextEditingController controller;
  final String labelText;
  final String? hintText;
  final TextInputAction textInputAction;
  final void Function(String)? onSubmitted;

  const PasswordField({
    super.key,
    required this.controller,
    this.labelText = 'Password',
    this.hintText,
    this.textInputAction = TextInputAction.done,
    this.onSubmitted,
  });

  @override
  State<PasswordField> createState() => _PasswordFieldState();
}

class _PasswordFieldState extends State<PasswordField> {
  bool _obscure = true;

  @override
  Widget build(BuildContext context) {
    return TextField(
      controller: widget.controller,
      obscureText: _obscure,
      textInputAction: widget.textInputAction,
      onSubmitted: widget.onSubmitted,
      style: const TextStyle(color: Colors.white),
      decoration: InputDecoration(
        labelText: widget.labelText,
        hintText: widget.hintText,
        prefixIcon: const Icon(Icons.lock_outline, color: AppTheme.royalGold),
        suffixIcon: IconButton(
          icon: Icon(
            _obscure ? Icons.visibility_off_outlined : Icons.visibility_outlined,
            color: AppTheme.textSecondaryDark,
            size: 20,
          ),
          tooltip: _obscure ? 'Show password' : 'Hide password',
          onPressed: () => setState(() => _obscure = !_obscure),
        ),
      ),
    );
  }
}
