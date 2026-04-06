import 'package:flutter/material.dart';
import '../theme/app_theme.dart';

class AppSearchField extends StatelessWidget {
  final TextEditingController controller;
  final String hintText;
  final ValueChanged<String> onChanged;
  final VoidCallback onClear;

  const AppSearchField({
    super.key,
    required this.controller,
    required this.onChanged,
    required this.onClear,
    this.hintText = 'Search...',
  });

  @override
  Widget build(BuildContext context) {
    return TextField(
      controller: controller,
      decoration: InputDecoration(
        hintText: hintText,
        prefixIcon: const Icon(Icons.search_rounded, color: AppTheme.royalGold),
        suffixIcon: controller.text.isNotEmpty 
          ? IconButton(
              icon: const Icon(Icons.close_rounded, size: 18),
              onPressed: onClear,
            )
          : null,
      ),
      onChanged: onChanged,
    );
  }
}
