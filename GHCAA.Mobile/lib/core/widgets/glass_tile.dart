import 'package:flutter/material.dart';
import '../theme/app_theme.dart';
import 'glass_container.dart';

class GlassTile extends StatelessWidget {
  final IconData icon;
  final String title;
  final String? subtitle;
  final VoidCallback onTap;
  final Widget? trailing;

  const GlassTile({
    super.key,
    required this.icon,
    required this.title,
    required this.onTap,
    this.subtitle,
    this.trailing,
  });

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: GlassContainer(
        padding: EdgeInsets.zero,
        child: ListTile(
          contentPadding: const EdgeInsets.symmetric(horizontal: 20, vertical: 8),
          leading: Container(
            padding: const EdgeInsets.all(10),
            decoration: BoxDecoration(
              color: AppTheme.royalGold.withOpacity(0.1),
              borderRadius: BorderRadius.circular(12),
            ),
            child: Icon(icon, color: AppTheme.royalGold, size: 20),
          ),
          title: Text(title, style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 15)),
          subtitle: subtitle != null ? Text(subtitle!, style: TextStyle(fontSize: 12, color: AppTheme.textSecondaryDark.withOpacity(0.8))) : null,
          trailing: trailing ?? const Icon(Icons.chevron_right, size: 16, color: AppTheme.royalGold),
          onTap: onTap,
        ),
      ),
    );
  }
}

class StatTile extends StatelessWidget {
  final String title;
  final String value;
  final bool highlight;

  const StatTile({
    super.key,
    required this.title,
    required this.value,
    this.highlight = false,
  });

  @override
  Widget build(BuildContext context) {
    final width = (MediaQuery.of(context).size.width - 64) / 2;
    return SizedBox(
      width: width,
      child: GlassContainer(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(title.toUpperCase(), style: TextStyle(fontSize: 10, letterSpacing: 1.2, fontWeight: FontWeight.w800, color: AppTheme.textSecondaryDark.withOpacity(0.6))),
            const SizedBox(height: 8),
            ShaderMask(
              shaderCallback: (bounds) => LinearGradient(
                colors: highlight ? [AppTheme.royalGold, AppTheme.royalGoldVibrant] : [Colors.white, Colors.white70],
              ).createShader(bounds),
              child: Text(
                value, 
                style: const TextStyle(fontSize: 28, fontWeight: FontWeight.w900, color: Colors.white)
              ),
            ),
          ],
        ),
      ),
    );
  }
}
