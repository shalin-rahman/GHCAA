import 'dart:async';
import 'package:flutter/material.dart';

/// 29E.4: onChanged used to fire on every keystroke, so search callsites re-fetched
/// (network round-trip) per character typed. This now debounces the callback — the
/// consumer only gets the settled query after [debounce] of no typing. The clear button
/// bypasses the debounce so clearing is instant.
class AppSearchField extends StatefulWidget {
  final TextEditingController controller;
  final String hintText;
  final ValueChanged<String> onChanged;
  final VoidCallback onClear;
  final Duration debounce;

  const AppSearchField({
    super.key,
    required this.controller,
    required this.onChanged,
    required this.onClear,
    this.hintText = 'Search...',
    this.debounce = const Duration(milliseconds: 350),
  });

  @override
  State<AppSearchField> createState() => _AppSearchFieldState();
}

class _AppSearchFieldState extends State<AppSearchField> {
  Timer? _debounceTimer;

  @override
  void dispose() {
    _debounceTimer?.cancel();
    super.dispose();
  }

  void _onChanged(String value) {
    // Rebuild so the clear (suffix) icon shows/hides as text changes.
    setState(() {});
    _debounceTimer?.cancel();
    _debounceTimer = Timer(widget.debounce, () => widget.onChanged(value));
  }

  void _onClear() {
    _debounceTimer?.cancel();
    widget.onClear();
    setState(() {});
  }

  @override
  Widget build(BuildContext context) {
    return TextField(
      controller: widget.controller,
      decoration: InputDecoration(
        hintText: widget.hintText,
        prefixIcon: Icon(Icons.search_rounded, color: Theme.of(context).colorScheme.secondary),
        suffixIcon: widget.controller.text.isNotEmpty
            ? IconButton(
                icon: const Icon(Icons.close_rounded, size: 18),
                onPressed: _onClear,
              )
            : null,
      ),
      onChanged: _onChanged,
    );
  }
}
